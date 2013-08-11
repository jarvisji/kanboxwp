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

namespace kanboxwp
{
    public partial class SetupPage : PhoneApplicationPage
    {
        public SetupPage()
        {
            InitializeComponent();
        }

        private async void piAccount_Loaded(object sender, RoutedEventArgs e)
        {
            KbAccountInfo accountInfo = await App.ViewModel.GetAccountInfo();
            tbEmail.Text = accountInfo.Email;

            pbStorageUsage.Maximum = (double)accountInfo.SpaceQuota;
            pbStorageUsage.Value = (double)accountInfo.SpaceUsed;

            string quota = FileUtil.ConvertBytesToKMGB(accountInfo.SpaceQuota);
            string used = FileUtil.ConvertBytesToKMGB(accountInfo.SpaceUsed);
            tbStorageUsage.Text = used + " / " + quota;
        }

        private void hbLogout_Click(object sender, RoutedEventArgs e)
        {
            FileUtil.deleteTokenFile();
        }
    }
}