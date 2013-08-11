using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using kanboxwp.Utils;
using kanboxwp.Entities;
using Windows.Storage;

namespace kanboxwp
{
    public partial class FolderViewPage : PhoneApplicationPage
    {
        private string currentPath;

        public FolderViewPage()
        {
            InitializeComponent();
            DataContext = App.ViewModel;
        }

        private void FolderViewPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (NavigationContext.QueryString.TryGetValue("path", out currentPath))
            {
                tbTitle.Text = currentPath.Substring(currentPath.LastIndexOf("/") + 1);
                try
                {
                    App.ViewModel.LoadFolderFiles(currentPath);
                }
                catch (Exception ex)
                {
                    string a = ex.Message;
                }
            }
            else
            {
                throw new ArgumentException("Inner error: parameter \"path\" not found.");
            }
        }

        private async void lbFolderItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                ItemViewModel curFileListSelection = (ItemViewModel)e.AddedItems[0];
                KbListContentInfo contentInfo = curFileListSelection.ContentInfo;
                if (contentInfo.IsFolder)
                {
                    NavigationService.Navigate(new Uri("/FolderViewPage.xaml?path=" + contentInfo.FullPath, UriKind.Relative));
                }
                else
                {
                    if (contentInfo.FileSize > App.CONF_MAXDOWNLOADSIZE)
                    {
                        string i18nResource = "文件大于{0}M，请使用PC客户端下载。";
                        MessageBox.Show(string.Format(i18nResource, App.CONF_MAXDOWNLOADSIZE / 1024 / 1024));
                        return;
                    }
                    else
                    {
                        pbDownloading.Visibility = System.Windows.Visibility.Visible;
                        StorageFile sfile = null;
                        try
                        {
                            sfile = await App.ViewModel.DownloadFile(contentInfo);
                        }
                        catch (Exception ex)
                        {
                            string a = ex.Message;
                        }
                        pbDownloading.Visibility = System.Windows.Visibility.Collapsed;
                        if (sfile != null)
                        {
                            await Windows.System.Launcher.LaunchFileAsync(sfile);
                        }
                    }
                }
            }
        }

        // upload file to server
        private void ibUpload_Click(object sender, EventArgs e)
        {

        }

        // refresh current list
        private void ipRefresh_Click(object sender, EventArgs e)
        {
            App.ViewModel.LoadFolderFiles(currentPath);
        }

        private void lbFolderItems_Loaded(object sender, RoutedEventArgs e)
        {
            // Clear last selection, so SelectionChanged event can handle user tap the same item to last viewed.
            lbFolderItems.SelectedItem = null;
        }

    }
}