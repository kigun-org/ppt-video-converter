/*
 *  Converter - easily convert videos to a PowerPoint friendly format.
 *  Copyright (C) 2021  Mihai Tarce
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Converter
{
    /*
     * Application icon from:
     * Flat Folders ISCN (19 items) by janosch500
     * https://iconarchive.com/show/tropical-waters-folders-icons-by-janosch500.html
     * https://www.deviantart.com/janosch500/art/Green-Tropical-Waters-folders-531656952
     */
public partial class Form1 : Form
    {
        private BackgroundWorker handbrakeWorker;

        private BindingList<QueueItem> queueList = new System.ComponentModel.BindingList<QueueItem>();

        private QueueItem currentQueueItem;

        public Form1()
        {
            InitializeComponent();

            // cancelButton.Image = Image.FromFile();
            // cancelButton.ImageAlign = ContentAlignment.MiddleRight;
            // cancelButton.TextAlign = ContentAlignment.MiddleLeft;
            // cancelButton.TextImageRelation = TextImageRelation.ImageAboveText;

            queueList.AllowNew = true;
            queueList.RaiseListChangedEvents = true;
            queueList.AllowRemove = false;
            queueList.AllowEdit = false;
            // queueList.ListChanged += new ListChangedEventHandler(queueList_ListChanged);

            queueListBox.DataSource = queueList;

            LoadCommandLineArgs();
        }

        private void ProcessNextItem()
        {
            if (queueList.Count > 0 &&
                (handbrakeWorker == null || !handbrakeWorker.IsBusy))
            {
                handbrakeWorker = new BackgroundWorker();

                handbrakeWorker.WorkerReportsProgress = true;
                handbrakeWorker.WorkerSupportsCancellation = true;

                handbrakeWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
                handbrakeWorker.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker_ProgressChanged);
                handbrakeWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);

                QueueItem nextItem = null;
                foreach (var item in queueList)
                {
                    if (item.Status == QueueItemStatus.Pending)
                    {
                        nextItem = item;
                        break;
                    }
                }

                if (nextItem != null)
                {
                    nextItem.Status = QueueItemStatus.InProgress;
                    handbrakeWorker.RunWorkerAsync(nextItem);

                    cancelButton.Enabled = true;
                }
            }
        }

        private void AddFileToQueue(string path)
        {
            queueList.Add(new QueueItem(path));
        }

        private void LoadCommandLineArgs()
        {
            var cmdArgs = Environment.GetCommandLineArgs();

            if (cmdArgs.Length >= 2)
            {
                foreach (string arg in cmdArgs.Skip(1))
                {
                    AddFileToQueue(arg);
                }

                ProcessNextItem();
            }
        }

        private void queueListBox_DragDrop(object sender, DragEventArgs e)
        {
            foreach (string drop in (string[])e.Data.GetData(DataFormats.FileDrop, false))
            {
                AddFileToQueue(drop);
            }

            ProcessNextItem();
        }

        private void queueListBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            handbrakeWorker.CancelAsync();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            currentQueueItem = e.Argument as QueueItem;

            ProcessStartInfo info = new ProcessStartInfo("HandBrakeCLI\\HandBrakeCLI.exe");

            // input file
            info.ArgumentList.Add("-i");
            info.ArgumentList.Add(currentQueueItem.FullPath);

            // output file (filename.mp4 in same folder, if it exists add number, e.g. filename N.mp4)
            info.ArgumentList.Add("-o");
            info.ArgumentList.Add(MakeUniqueFileName(currentQueueItem.FullPath).ToString());

            // --format av_mp4
            // av_mp4 (default ca_aac/av_aac), av_mkv (ca_aac/mp3)
            info.ArgumentList.Add("--format");
            info.ArgumentList.Add("av_mp4");

            // --encoder x264
            // Force H264
            info.ArgumentList.Add("--encoder");
            info.ArgumentList.Add("x264");

            // --aencoder ca_aac
            // Force AAC
            // info.ArgumentList.Add("--aencoder");
            // info.ArgumentList.Add("av_aac");

            // No audio
            info.ArgumentList.Add("--audio");
            info.ArgumentList.Add("none");

            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.RedirectStandardOutput = true;
            // info.RedirectStandardError = true;

            try
            {
                using (Process handbrakeProcess = Process.Start(info))
                {
                    Regex regex = new Regex(@"^Encoding: task (\d+) of (\d+), (\d+.\d+) %");

                    StreamReader sr = handbrakeProcess.StandardOutput;
                    while (!sr.EndOfStream)
                    {
                        if (handbrakeWorker.CancellationPending)
                        {
                            handbrakeProcess.Kill();
                            e.Cancel = true;
                            break;
                        }
                        else
                        {
                            string output = sr.ReadLine();
                            Match match = regex.Match(output);

                            if (match.Success)
                            {
                                int progressValue = (int)Math.Round(double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture.NumberFormat));
                                handbrakeWorker.ReportProgress(progressValue, output);
                            }
                            else
                            {
                                // show error
                                handbrakeWorker.ReportProgress(0, output);
                            }
                        }
                    }

                    handbrakeProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            statusLabel.Text = e.UserState.ToString();
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            cancelButton.Enabled = false;

            if (e.Error != null)
            {
                currentQueueItem.Status = QueueItemStatus.Error;
                statusLabel.Text = e.Error.Message;

                ProcessNextItem();
            }
            else if (e.Cancelled)
            {
                // if cancelled do not process next item
                currentQueueItem.Status = QueueItemStatus.Error;
                statusLabel.Text = "Cancelled";
                progressBar.Value = 0;
            }
            else
            {
                currentQueueItem.Status = QueueItemStatus.Complete;
                statusLabel.Text = "Conversion complete.";
                progressBar.Value = 100;

                ProcessNextItem();
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (handbrakeWorker != null && handbrakeWorker.IsBusy)
            {
                handbrakeWorker.CancelAsync();
            }
        }

        private FileInfo MakeUniqueFileName(string path)
        {
            string dir = Path.GetDirectoryName(path);
            string fileName = Path.GetFileNameWithoutExtension(path);
            string fileExt = ".mp4";

            path = Path.Combine(dir, fileName + fileExt);

            for (int i = 1; ; ++i)
            {
                if (!File.Exists(path))
                    return new FileInfo(path);

                path = Path.Combine(dir, fileName + " " + i + fileExt);
            }
        }
    }
}
