using Microsoft.Phone.Controls;
using System;
using System.IO.IsolatedStorage;
using System.IO;
using kanboxwp.Utils;
using System.Net;
using kanboxwp.Entities;
using Newtonsoft.Json;
using System.Windows.Navigation;
using System.Windows;


namespace kanboxwp
{
    public partial class AuthPage : PhoneApplicationPage
    {


        public AuthPage()
        {
            InitializeComponent();
            wb_auth.Navigating += wb_auth_Navigating;
            wb_auth.Navigate(new Uri(KBApiUtil.GetAuthCodeRequestUrl()));
        }

        // 20130801, return url is "https://auth.kanbox.com/0/kanboxwpdummypage?state=&code=98f77bbda66caed87bdfe29281c0001f"
        private async void wb_auth_Navigating(object sender, NavigatingEventArgs e)
        {
            string url = e.Uri.ToString();
            if (KBApiUtil.IsAuthCodeResponse(url))
            {
                e.Cancel = true;
                string authcode = KBApiUtil.ParseAuthCodeFromUrl(e.Uri.Query.ToString());
                //KBApiUtil.GetToken(authcode, GetTokenCallback);
                KbToken token = await KBApiUtil.GetTokenAsync(authcode);
                FileUtil.writeTokenFile(token);
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
        }

        private void hbRefreshBrowser_Click(object sender, RoutedEventArgs e)
        {
            wb_auth.Navigate(new Uri(KBApiUtil.GetAuthCodeRequestUrl()));
        }        
    }
}