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
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Converter
{
    public enum QueueItemStatus
    {
        Pending,
        InProgress,
        Complete,
        Error
    }

    internal class QueueItem : INotifyPropertyChanged
    {
        private string fullPath;

        private QueueItemStatus status;

        public QueueItem(string fullPathForItem)
        {
            fullPath = fullPathForItem;
            status = QueueItemStatus.Pending;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public QueueItemStatus Status
        {
            get { return status; }
            set
            {
                if (value != status)
                {
                    status = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string FullPath
        {
            get { return fullPath; }
        }

        public override string ToString()
        {
            string statusLabel = "  ";

            switch (status)
            {
                case QueueItemStatus.InProgress:
                    statusLabel += "⌛";
                    break;
                case QueueItemStatus.Complete:
                    statusLabel += "✓";
                    break;
                case QueueItemStatus.Error:
                    statusLabel += "⚠";
                    break;
            }

            return Path.GetFileName(fullPath) + statusLabel;
        }
    }
}