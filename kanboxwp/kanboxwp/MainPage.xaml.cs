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
using Newtonsoft.Json;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Storage;

namespace kanboxwp
{
    public partial class MainPage : PhoneApplicationPage
    {
        private ItemViewModel curMyKanboxFileListSelection;

        public MainPage()
        {
            InitializeComponent();
            DataContext = App.ViewModel;
            this.Loaded += MainPage_Loaded;
            pivotMain.SelectionChanged += pivotMain_SelectionChanged;
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                try
                {
                    App.ViewModel.LoadMyKanboxFiles();
                }
                catch (Exception ex)
                {
                    string a = ex.Message;
                }
            }
        }

        void pivotMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                PivotItem currentSelection = (PivotItem)e.AddedItems[0];
                int idx = (currentSelection.Parent as Pivot).SelectedIndex;
                ShowHideApplicationBar(idx);
                if (idx == 1)
                {
                    App.ViewModel.LoadRecentViewedFiles();
                }
            }
        }

        // Currently, application bar only visible for MyKanbox files pivot item (index = 0).
        private void ShowHideApplicationBar(int selectedPivotIndex)
        {
            if (selectedPivotIndex == 0)
            {
                ApplicationBar.IsVisible = true;
            }
            else
            {
                ApplicationBar.IsVisible = false;
            }
        }

        // handle click on file/folder name.
        private async void lbMyKanbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                curMyKanboxFileListSelection = (ItemViewModel)e.AddedItems[0];
                KbListContentInfo contentInfo = curMyKanboxFileListSelection.ContentInfo;
                if (contentInfo.IsFolder)
                {
                    //App.ViewModel.LoadMyKanboxFiles(contentInfo.FullPath);
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
                        StorageFile sfile = await App.ViewModel.DownloadFile(contentInfo);
                        pbDownloading.Visibility = System.Windows.Visibility.Collapsed;
                        await Windows.System.Launcher.LaunchFileAsync(sfile);
                    }
                }
            }
        }

        // Upload file to server
        private void ibUpload_Click(object sender, EventArgs e)
        {

        }

        // Refresh MyKanbox files
        private void ipRefresh_Click(object sender, EventArgs e)
        {
            App.ViewModel.LoadMyKanboxFiles();
        }

        private void lbMyKanbox_Loaded(object sender, RoutedEventArgs e)
        {
            lbMyKanbox.SelectedItem = null;
        }

        private void ipSetup_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SetupPage.xaml", UriKind.Relative));
        }

        private async void lbRecent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                ItemViewModel ivmSelection = (ItemViewModel)e.AddedItems[0];
                StorageFile localfile = await FileUtil.GetFileInAppDataFolder(ivmSelection.ContentInfo.FullPath);
                if (localfile == null)
                {
                    localfile = await App.ViewModel.DownloadFile(ivmSelection.ContentInfo);
                }
                await Windows.System.Launcher.LaunchFileAsync(localfile);
            }
        }
    }
}