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