using kanboxwp.Entities;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace kanboxwp
{
    public class ItemViewModel : INotifyPropertyChanged
    {
        private string _fileName;
        /// <summary>
        /// File name.
        /// </summary>
        /// <returns></returns>
        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                if (value != _fileName)
                {
                    _fileName = value;
                    NotifyPropertyChanged("FileName");
                }
            }
        }

        private KbListContentInfo _contentInfo;
        /// <summary>
        /// Contains content information of current file.
        /// </summary>
        /// <returns></returns>
        public KbListContentInfo ContentInfo
        {
            get
            {
                return _contentInfo;
            }
            set
            {
                if (value != _contentInfo)
                {
                    _contentInfo = value;
                    NotifyPropertyChanged("ContentInfo");
                }
            }
        }

        private string _fileInfo;
        /// <summary>
        /// File information, composed by file size and modification date.
        /// </summary>
        /// <returns></returns>
        public string FileInfo
        {
            get
            {
                return _fileInfo;
            }
            set
            {
                if (value != _fileInfo)
                {
                    _fileInfo = value;
                    NotifyPropertyChanged("FileInfo");
                }
            }
        }

        private string _fileIcon;
        /// <summary>
        /// Icon path of file, all icon files are located in "Images" directory.
        /// </summary>
        /// <returns></returns>
        public string FileIcon
        {
            get
            {
                return _fileIcon;
            }
            set
            {
                if (value != _fileIcon)
                {
                    _fileIcon = value;
                    NotifyPropertyChanged("FileIcon");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}