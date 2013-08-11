using kanboxwp.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace kanboxwp.Utils
{
    class KBApiUtil
    {
        const string CLIENTID = "0c9d52c00872a6189107bc031136777f";
        const string CLIENTSECRET = "cde546504ec498e004feb16efac7f8b7";
        const string KB_AUTH_URL = "https://auth.kanbox.com/0/auth?response_type=code&client_id={0}&redirect_uri={1}";
        const string KB_REDIRECTURL_KANBOXWP_DUMMYPAGE = "kanboxwpdummypage";
        const string KB_AUTH_URL_PARAM_CODE = "code";
        const string KB_DEFAULT_TOKENTYPE = "Bearer";

        const string AuthUrl = "https://auth.kanbox.com/0/auth";
        const string TokenUrl = "https://auth.kanbox.com/0/token";
        const string ListUrl = "https://api.kanbox.com/0/list";
        const string DownloadUrl = "https://api.kanbox.com/0/download";
        const string InfoUrl = "https://api.kanbox.com/0/info";

        const string Upload = "https://api-upload.kanbox.com/0/upload";
        const string Delete = "https://api.kanbox.com/0/delete";
        const string Move = "https://api.kanbox.com/0/move";
        const string CreateFolder = "https://api.kanbox.com/0/create_folder";

        public const string KB_STATUS_OK = "ok";
        public const string KB_STATUS_NOCHANGE = "nochange";


        private static string _RedirectUri = "urn:ietf:wg:oauth:2.0:oob";
        public static string RedirectUri
        {
            get
            {
                return _RedirectUri;
            }
            set
            {
                RedirectUri = value;
            }
        }

        /// <summary>
        /// Convert Object to Dictionary.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ObjectToDictionary(object obj)
        {
            return obj.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(obj, new object[0]));
        }

        /// <summary>
        /// Compose object fields to parameters string.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetUrlData(object obj)
        {
            return GetUrlData(ObjectToDictionary(obj));
        }

        /// <summary>
        /// Compose dictionary keys and values to parameters string.
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static string GetUrlData(Dictionary<string, object> dict)
        {
            var arr = new string[dict.Keys.Count];
            var i = 0;
            foreach (var item in dict)
            {
                arr[i] = string.Format("{0}={1}", item.Key, item.Value == null ? string.Empty : HttpUtility.UrlEncode(item.Value.ToString()));
                i++;
            }
            return string.Join("&", arr);
        }

        /// <summary>
        /// Append dictionary as parameters to a url.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public static string Combine(string url, Dictionary<string, object> ps)
        {
            return url + "?" + GetUrlData(ps);
        }

        /// <summary>
        /// Append object fields as parameters to a url.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Combine(string url, object obj)
        {
            return Combine(url, ObjectToDictionary(obj));
        }


        /// <summary>
        /// Generate request url for auth code.
        /// </summary>
        /// <returns></returns>
        public static string GetAuthCodeRequestUrl()
        {
            string url = string.Format(KB_AUTH_URL, CLIENTID, KB_REDIRECTURL_KANBOXWP_DUMMYPAGE);
            return url;
        }

        /// <summary>
        /// Check a url string is response of auth code or not.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsAuthCodeResponse(string url)
        {
            bool ret = false;
            if (url.Contains(KB_REDIRECTURL_KANBOXWP_DUMMYPAGE + "?") && url.Contains(KB_AUTH_URL_PARAM_CODE))
            {
                ret = true;
            }
            return ret;
        }

        /// <summary>
        /// Parse auth code value from response url.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string ParseAuthCodeFromUrl(string url)
        {
            string code = "";
            string[] paramStrs = url.Split('&');
            foreach (string paramstr in paramStrs)
            {
                if (paramstr.Contains(KB_AUTH_URL_PARAM_CODE))
                {
                    code = paramstr.Split('=')[1];
                    break;
                }
            }
            return code;
        }

        /// <summary>
        /// Get file list in special path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<KbListInfo> GetFileListAsync(string path, KbToken token)
        {
            return await GetFileListAsync(path, token, null);
        }

        /// <summary>
        /// Get file list in special path and check the hashcode to see if content was changed.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="token"></param>
        /// <param name="hashcode"></param>
        /// <returns></returns>
        public static async Task<KbListInfo> GetFileListAsync(string path, KbToken token, string hashcode)
        {
            string url = ListUrl + path;
            if (hashcode != null)
            {
                url += "?hash=" + hashcode;
            }
            string received = await doGetAsync(url, GetAuthorizationHeader(token));
            KbListInfo listInfo = JsonConvert.DeserializeObject<KbListInfo>(received);
            return listInfo;
        }

        /// <summary>
        /// Get account info from server.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<KbAccountInfo> GetAccountInfo(KbToken token)
        {
            string received = await doGetAsync(InfoUrl, GetAuthorizationHeader(token));
            KbAccountInfo accountInfo = JsonConvert.DeserializeObject<KbAccountInfo>(received);
            return accountInfo;
        }

        /// <summary>
        /// Download file from server.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<StorageFile> DownloadFileAsync(string path, KbToken token)
        {
            string url = DownloadUrl + Uri.EscapeDataString(path);
            Stream respStream = await doGetBytesAsync(url, GetAuthorizationHeader(token));
            StorageFile sfile = await FileUtil.SaveStream(respStream, path);
            respStream.Close();
            return sfile;
        }

        private static Dictionary<string, string> GetAuthorizationHeader(KbToken token)
        {
            Dictionary<string, string> header = new Dictionary<string, string>();
            header.Add("Authorization", KB_DEFAULT_TOKENTYPE + " " + token.AccessToken);
            return header;
        }

        /// <summary>
        /// Get token by authorization code.
        /// </summary>
        /// <param name="authcode"></param>
        /// <returns></returns>
        public static async Task<KbToken> GetTokenAsync(string authcode)
        {
            string postdata = GetUrlData(new
            {
                grant_type = HttpUtility.UrlEncode("authorization_code"),
                client_id = HttpUtility.UrlEncode(CLIENTID),
                client_secret = HttpUtility.UrlEncode(CLIENTSECRET),
                code = HttpUtility.UrlEncode(authcode),
                redirect_uri = HttpUtility.UrlEncode(KB_REDIRECTURL_KANBOXWP_DUMMYPAGE)
            });
            string received = await doPostAsync(TokenUrl, postdata);
            KbToken token = JsonConvert.DeserializeObject<KbToken>(received);
            UpdateExpiresTime(token);
            return token;
        }

        /// <summary>
        /// Refresh access code in KbToken.
        /// </summary>
        /// <param name="RefreshToken"></param>
        /// <returns></returns>
        public static async Task<KbToken> RefreshTokenAsync(string refreshToken)
        {
            string data = GetUrlData(new
            {
                grant_type = HttpUtility.UrlEncode("refresh_token"),
                client_id = HttpUtility.UrlEncode(CLIENTID),
                client_secret = HttpUtility.UrlEncode(CLIENTSECRET),
                refresh_token = HttpUtility.UrlEncode(refreshToken)
            });
            string received = await doPostAsync(TokenUrl, data);
            KbToken token = JsonConvert.DeserializeObject<KbToken>(received);
            UpdateExpiresTime(token);
            return token;
        }

        private static void UpdateExpiresTime(KbToken token)
        {
            token.ExpiresTime = DateTime.Now.AddSeconds(token.ExpiresIn).ToString();
        }

        /// <summary> 
        /// Post data to remote server by async task. This is an alternative solution to use WebClient.
        /// </summary> 
        /// <param name="url"></param> 
        /// <param name="postdata"></param>       
        /// <returns></returns> 
        private static async Task<string> doPostAsync(string url, string postdata)
        {
            var request = WebRequest.Create(new Uri(url, UriKind.Absolute)) as HttpWebRequest;
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            byte[] data = Encoding.UTF8.GetBytes(postdata);
            request.ContentLength = data.Length;
            using (var requestStream = await Task<Stream>.Factory.FromAsync(request.BeginGetRequestStream, request.EndGetRequestStream, request))
            {
                await requestStream.WriteAsync(data, 0, data.Length);
            }
            WebResponse response = await Task<WebResponse>.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, request);
            StreamReader sr = new StreamReader(response.GetResponseStream());
            string received = await sr.ReadToEndAsync();
            sr.Close();
            return received;
        }

        private static async Task<string> doGetAsync(string url)
        {
            return await doGetAsync(url, null);
        }

        private static async Task<string> doGetAsync(string url, Dictionary<string, string> headers)
        {
            HttpWebRequest request = WebRequest.CreateHttp(url);
            addRequestHeaders(headers, request);
            WebResponse response = await Task<WebResponse>.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, request);
            StreamReader sr = new StreamReader(response.GetResponseStream());
            string received = await sr.ReadToEndAsync();
            sr.Close();
            return received;
        }

        private static void addRequestHeaders(Dictionary<string, string> headers, HttpWebRequest request)
        {
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    request.Headers[header.Key] = header.Value;
                }
            }
        }

        private static async Task<Stream> doGetBytesAsync(string url, Dictionary<string, string> headers)
        {
            HttpWebRequest request = WebRequest.CreateHttp(url);
            addRequestHeaders(headers, request);
            WebResponse response = await Task<WebResponse>.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, request);
            return response.GetResponseStream(); //TODO: Stream shouldn't be returned in case forget to close outside.
        }

        /// <summary>
        /// Get token by authorization code.
        /// </summary>
        /// <param name="authcode"></param>
        /// <param name="handler"></param>
        [Obsolete]
        public static void GetToken(string authcode, UploadStringCompletedEventHandler handler)
        {
            string data = GetUrlData(new
            {
                grant_type = HttpUtility.UrlEncode("authorization_code"),
                client_id = HttpUtility.UrlEncode(CLIENTID),
                client_secret = HttpUtility.UrlEncode(CLIENTSECRET),
                code = HttpUtility.UrlEncode(authcode),
                redirect_uri = HttpUtility.UrlEncode(KB_REDIRECTURL_KANBOXWP_DUMMYPAGE)
            });
            WebClient webclient = new WebClient();
            webclient.Headers["Content-Type"] = "application/x-www-form-urlencoded";
            webclient.UploadStringCompleted += handler;
            webclient.UploadStringAsync(new Uri(TokenUrl), "POST", data);
        }

        /// <summary>
        /// Refresh access token.
        /// </summary>
        /// <param name="RefreshToken"></param>
        /// <param name="handler"></param>
        [Obsolete]
        public static void RefreshToken(string refreshToken, UploadStringCompletedEventHandler handler)
        {
            string data = GetUrlData(new
            {
                grant_type = HttpUtility.UrlEncode("refresh_token"),
                client_id = HttpUtility.UrlEncode(CLIENTID),
                client_secret = HttpUtility.UrlEncode(CLIENTSECRET),
                refresh_token = HttpUtility.UrlEncode(refreshToken)
            });
            WebClient webclient = new WebClient();
            webclient.Headers["Content-Type"] = "application/x-www-form-urlencoded";
            webclient.UploadStringCompleted += handler;
            webclient.UploadStringAsync(new Uri(TokenUrl), "POST", data);
        }

        [Obsolete]
        public static void GetFileList(string path, KbToken token, DownloadStringCompletedEventHandler callback)
        {
            Uri pathUrl = new Uri(ListUrl + path);
            WebClient webclient = new WebClient();
            SetHeaderAuthorization(webclient, token);
            webclient.DownloadStringCompleted += callback;
            webclient.DownloadStringAsync(pathUrl);
        }

        [Obsolete]
        private static void SetHeaderAuthorization(WebClient webclient, KbToken token)
        {
            webclient.Headers["Authorization"] = KB_DEFAULT_TOKENTYPE + " " + token.AccessToken;
        }
    }
}
