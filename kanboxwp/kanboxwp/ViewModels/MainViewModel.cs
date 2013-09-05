using kanboxwp.Entities;
using kanboxwp.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Windows.Storage;


namespace kanboxwp
{
    public class MainViewModel : INotifyPropertyChanged
    {
        const string ICON_FILETYPE_EXCEL = "file.excel.64.png";
        const string ICON_FILETYPE_FOLDER = "file.folder.64.png";
        const string ICON_FILETYPE_IMAGE = "file.image.64.png";
        const string ICON_FILETYPE_MUSIC = "file.music.64.png";
        const string ICON_FILETYPE_PDF = "file.pdf.64.png";
        const string ICON_FILETYPE_POWERPOINT = "file.powerpoint.64.png";
        const string ICON_FILETYPE_VIDEO = "file.video.64.png";
        const string ICON_FILETYPE_WORD = "file.word.64.png";
        const string ICON_FILETYPE_ZIP = "file.zip.64.png";
        const string ICON_FILETYPE_UNKNOWN = "file.unknown.64.png";

        private Dictionary<string, string> FileTypeIcon = new Dictionary<string, string>() { { "xlsx", ICON_FILETYPE_EXCEL }, { "xls", ICON_FILETYPE_EXCEL }, { "xltx", ICON_FILETYPE_EXCEL }, { "xlt", ICON_FILETYPE_EXCEL }, { "csv", ICON_FILETYPE_EXCEL }, { "jpg", ICON_FILETYPE_IMAGE }, { "jpeg", ICON_FILETYPE_IMAGE }, { "png", ICON_FILETYPE_IMAGE }, { "gif", ICON_FILETYPE_IMAGE }, { "bmp", ICON_FILETYPE_IMAGE }, { "tif", ICON_FILETYPE_IMAGE }, { "tiff", ICON_FILETYPE_IMAGE }, { "mp3", ICON_FILETYPE_MUSIC }, { "wav", ICON_FILETYPE_MUSIC }, { "wma", ICON_FILETYPE_MUSIC }, { "flac", ICON_FILETYPE_MUSIC }, { "ogg", ICON_FILETYPE_MUSIC }, { "aac", ICON_FILETYPE_MUSIC }, { "m4a", ICON_FILETYPE_MUSIC }, { "m4r", ICON_FILETYPE_MUSIC }, { "amr", ICON_FILETYPE_MUSIC }, { "mp2", ICON_FILETYPE_MUSIC }, { "pdf", ICON_FILETYPE_PDF }, { "ppt", ICON_FILETYPE_POWERPOINT }, { "pptx", ICON_FILETYPE_POWERPOINT }, { "pptm", ICON_FILETYPE_POWERPOINT }, { "potm", ICON_FILETYPE_POWERPOINT }, { "potx", ICON_FILETYPE_POWERPOINT }, { "pot", ICON_FILETYPE_POWERPOINT }, { "pps", ICON_FILETYPE_POWERPOINT }, { "ppsx", ICON_FILETYPE_POWERPOINT }, { "ppsm", ICON_FILETYPE_POWERPOINT }, { "mp4", ICON_FILETYPE_VIDEO }, { "avi", ICON_FILETYPE_VIDEO }, { "mov", ICON_FILETYPE_VIDEO }, { "rmvb", ICON_FILETYPE_VIDEO }, { "rm", ICON_FILETYPE_VIDEO }, { "wmv", ICON_FILETYPE_VIDEO }, { "3gp", ICON_FILETYPE_VIDEO }, { "mpg", ICON_FILETYPE_VIDEO }, { "vob", ICON_FILETYPE_VIDEO }, { "flv", ICON_FILETYPE_VIDEO }, { "doc", ICON_FILETYPE_WORD }, { "docx", ICON_FILETYPE_WORD }, { "docm", ICON_FILETYPE_WORD }, { "dot", ICON_FILETYPE_WORD }, { "dotx", ICON_FILETYPE_WORD }, { "dotm", ICON_FILETYPE_WORD }, { "zip", ICON_FILETYPE_ZIP }, { "rar", ICON_FILETYPE_ZIP }, { "7z", ICON_FILETYPE_ZIP } };

        private Dictionary<string, KbListInfo> cachedPathListInfoDict = new Dictionary<string, KbListInfo>();
        public Dictionary<string, KbListInfo> PathListInfoDict { get { return cachedPathListInfoDict; } }

        private KbToken token;
        private List<KbListContentInfo> recentViewedFiles = new List<KbListContentInfo>();
        public List<KbListContentInfo> RecentViewedFiles { get { return recentViewedFiles; } }

        /// <summary>
        /// Collection of ItemViewModel for MyKanbox files.
        /// </summary>
        public ObservableCollection<ItemViewModel> MyKanboxItems { get; private set; }
        /// <summary>
        /// For Recent files.
        /// </summary>
        public ObservableCollection<ItemViewModel> RecentItems { get; private set; }
        /// <summary>
        /// For files in current folder, serve for FolderViewPage.
        /// </summary>
        public ObservableCollection<ItemViewModel> FolderItems { get; private set; }

        public MainViewModel()
        {
            this.MyKanboxItems = new ObservableCollection<ItemViewModel>();
            this.RecentItems = new ObservableCollection<ItemViewModel>();
            this.FolderItems = new ObservableCollection<ItemViewModel>();
        }

        public bool IsDataLoaded { get; private set; }

        /// <summary>
        /// Make sure we have token information saved in file, otherwise redirect to auth page.
        /// Load file list information to MainViewModel.
        /// </summary>
        public async void LoadMyKanboxFiles()
        {
            if (await CheckToken())
            {
                KbListInfo lastListInfo;
                // if local cache has data, show first at once, then pull from server to refresh.
                if (cachedPathListInfoDict.TryGetValue("/", out lastListInfo))
                {
                    ShowData(lastListInfo, MyKanboxItems);
                }
                KbListInfo listInfo = await GetFileList("/");
                if (!listInfo.Equals(lastListInfo))
                {
                    ShowData(listInfo, MyKanboxItems);
                }
            }
        }

        /// <summary>
        /// Load file list for specific folder from server.
        /// </summary>
        /// <param name="path"></param>
        public async void LoadFolderFiles(string path)
        {
            if (await CheckToken())
            {
                ClearFolderFiles();
                KbListInfo lastListInfo;
                // if local cache has data, show first at once, then pull from server to refresh.
                if (cachedPathListInfoDict.TryGetValue(path, out lastListInfo))
                {
                    ShowData(lastListInfo, FolderItems);
                }
                KbListInfo listInfo = await GetFileList(path);
                if (!listInfo.Equals(lastListInfo))
                {
                    ShowData(listInfo, FolderItems);
                }
            }
        }

        public void LoadRecentViewedFiles()
        {
            KbListInfo mockListInfo = new KbListInfo();
            mockListInfo.Contents.AddRange(recentViewedFiles);
            ShowData(mockListInfo, RecentItems);
        }

        public void ClearFolderFiles()
        {
            FolderItems.Clear();
        }


        // Show data on page.
        private void ShowData(KbListInfo listInfo, ObservableCollection<ItemViewModel> Items)
        {
            Items.Clear();
            foreach (KbListContentInfo contentInfo in listInfo.Contents)
            {
                ItemViewModel ivm = new ItemViewModel();
                ivm.FileName = contentInfo.FullPath.Substring(contentInfo.FullPath.LastIndexOf("/") + 1);
                ivm.FileInfo = contentInfo.ModificationDate.ToString();
                if (!contentInfo.IsFolder)
                {
                    ivm.FileInfo = FileUtil.ConvertBytesToKMGB(contentInfo.FileSize) + " " + ivm.FileInfo;
                }
                ivm.FileIcon = getFileIcon(contentInfo);
                ivm.ContentInfo = contentInfo;
                Items.Add(ivm);
            }

            this.IsDataLoaded = true;
        }

        public async Task<KbAccountInfo> GetAccountInfo()
        {
            await CheckToken();
            return await KBApiUtil.GetAccountInfo(token);
        }


        // Get icon file path for individual extend file name.
        private string getFileIcon(KbListContentInfo contentInfo)
        {
            string fileIconPath = "Images/";
            if (contentInfo.IsFolder)
            {
                fileIconPath += ICON_FILETYPE_FOLDER;
            }
            else
            {
                string fileTypeIcon = ICON_FILETYPE_UNKNOWN;
                string fileExt = contentInfo.FullPath.Substring(contentInfo.FullPath.LastIndexOf(".") + 1);
                if (fileExt != null)
                {
                    fileExt = fileExt.ToLower();
                    if (!FileTypeIcon.TryGetValue(fileExt, out fileTypeIcon))
                    {
                        fileTypeIcon = ICON_FILETYPE_UNKNOWN;
                    }
                }
                fileIconPath += fileTypeIcon;
            }
            return fileIconPath;
        }

        // Make sure get token and it hasn't expired.
        private async Task<bool> CheckToken()
        {
            if (token == null)
            {
                token = FileUtil.readTokenFile();
            }
            if (token == null)
            {
                (App.Current as App).RootFrame.Navigate(new Uri("/AuthPage.xaml", UriKind.Relative));
                return false;
            }
            await RefreshTokenIfNeed();
            return true;
        }

        private async Task<bool> RefreshTokenIfNeed()
        {
            if (IsExpired(token))
            {
                token = await KBApiUtil.RefreshTokenAsync(token.RefreshToken);
                FileUtil.writeTokenFile(token);
            }
            return true;
        }

        /// <summary>
        /// Download file from server to local, and keep contentInfo to recentViewedFiles.
        /// </summary>
        /// <param name="contentInfo"></param>
        /// <returns></returns>
        public async Task<StorageFile> DownloadFile(KbListContentInfo contentInfo)
        {
            StorageFile sfile = null;
            if (await CheckToken())
            {
                sfile = await KBApiUtil.DownloadFileAsync(contentInfo.FullPath, token);
                if (!recentViewedFiles.Exists(o => o.FullPath.Equals(contentInfo.FullPath)))
                {
                    recentViewedFiles.Insert(0, contentInfo);
                }
            }
            return sfile;
        }


        // Get file list from server with Hash code, if the content of directory hasn't changed since last access, server will return KB_STATUS_NOCHANGE instead of KB_STATUS_OK, in this case, we should load file list from local cache.
        private async Task<KbListInfo> GetFileList(string path)
        {
            KbListInfo lastListInfo;
            KbListInfo newListInfo;
            if (cachedPathListInfoDict.TryGetValue(path, out lastListInfo))
            {
                newListInfo = await KBApiUtil.GetFileListAsync(path, token, lastListInfo.Hash);
            }
            else
            {
                newListInfo = await KBApiUtil.GetFileListAsync(path, token);
            }

            if (KBApiUtil.KB_STATUS_OK.Equals(newListInfo.Status))
            {
                if (cachedPathListInfoDict.ContainsKey(path))
                {
                    cachedPathListInfoDict.Remove(path);
                }
                cachedPathListInfoDict.Add(path, newListInfo);
                return newListInfo;
            }
            else
            {
                return lastListInfo;
            }
        }

        // Check current time is exceed expiresTime in token or not.
        private bool IsExpired(KbToken token)
        {
            DateTime expiresTime = DateTime.Parse(token.ExpiresTime);
            DateTime nowTime = DateTime.Now;
            return nowTime > expiresTime;
        }

        private bool IsNoError(AsyncCompletedEventArgs e)
        {
            bool ret = true;
            if (e.Error != null)
            {
                ret = false;
                MessageBox.Show(e.Error.Message);
            }
            return ret;
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