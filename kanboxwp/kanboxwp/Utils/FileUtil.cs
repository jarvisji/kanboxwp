
using kanboxwp.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Streams;


namespace kanboxwp.Utils
{
    class FileUtil
    {
        const string FILENAME_TOKEN = "tokeninfo.txt";
        const string FILENAME_RECENTVIEWEDFILES = "recentviewedfiles.txt";
        const string FILENAME_MYKANBOXFILES = "mykanboxfiles.txt";

        /// <summary>
        /// Save token information to file.
        /// </summary>
        /// <param name="code"></param>
        public static void writeTokenFile(KbToken token)
        {
            writeIsolatedStorageFile(FILENAME_TOKEN, token);
        }

        /// <summary>
        /// Read saved token information from file.
        /// </summary>
        /// <returns></returns>
        public static KbToken readTokenFile()
        {
            KbToken token = (KbToken)readIsolatedStorageFile<KbToken>(FILENAME_TOKEN);
            return token;
        }

        public static void deleteTokenFile()
        {
            deleteIsolatedStorageFile(FILENAME_TOKEN);
        }

        public static void writeRecentViewedFile()
        {
            writeIsolatedStorageFile(FILENAME_RECENTVIEWEDFILES, App.ViewModel.RecentViewedFiles);
        }

        public static void readRecentViewdFile()
        {
            List<KbListContentInfo> recentViewedFiles = (List<KbListContentInfo>)readIsolatedStorageFile<List<KbListContentInfo>>(FILENAME_RECENTVIEWEDFILES);
            if (recentViewedFiles != null)
            {
                App.ViewModel.RecentViewedFiles.AddRange(recentViewedFiles);
            }
        }

        public static void writeMykanboxFile()
        {
            KbListInfo mykanboxFiles;
            if (App.ViewModel.PathListInfoDict.TryGetValue("/", out mykanboxFiles))
            {
                writeIsolatedStorageFile(FILENAME_MYKANBOXFILES, mykanboxFiles);
            }
        }

        public static void readMykanboxFile()
        {
            KbListInfo mykanboxFiles = (KbListInfo)readIsolatedStorageFile<KbListInfo>(FILENAME_MYKANBOXFILES);
            if (mykanboxFiles != null)
            {
                App.ViewModel.PathListInfoDict.Add("/", mykanboxFiles);
            }
        }

        // Deserialize json string to object which saved in IsolatedStorageFile.
        private static object readIsolatedStorageFile<T>(string filename)
        {
            object ret = null;
            IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
            if (isf.FileExists(filename))
            {
                using (var isfs = isf.OpenFile(filename, FileMode.Open))
                {
                    StreamReader sr = new StreamReader(isfs);
                    ret = JsonConvert.DeserializeObject<T>(sr.ReadLine());
                    sr.Close();
                }
            }
            return ret;
        }

        private static void deleteIsolatedStorageFile(string filename)
        {
            IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
            if (isf.FileExists(filename))
            {
                isf.DeleteFile(filename);
            }
        }

        // Serialize data to json string and save to IsolatedStorageFile.
        private static void writeIsolatedStorageFile(string filename, object data)
        {
            IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
            if (isf.FileExists(filename))
            {
                isf.DeleteFile(filename);
            }
            using (var isfs = isf.CreateFile(filename))
            {
                StreamWriter sw = new StreamWriter(isfs);
                sw.WriteLine(JsonConvert.SerializeObject(data));
                sw.Flush();
                sw.Close();
            }
        }


        /// <summary>
        /// Write a text file to the app's local folder. 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        public static async Task<string> WriteTextFile(string filename, string contents)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile textFile = await localFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

            using (IRandomAccessStream textStream = await textFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (DataWriter textWriter = new DataWriter(textStream))
                {
                    textWriter.WriteString(contents);
                    await textWriter.StoreAsync();
                }
            }

            return textFile.Path;
        }

        /// <summary>
        /// Read the contents of a text file from the app's local folder.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static async Task<string> ReadTextFile(string filename)
        {
            string contents;
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile textFile = await localFolder.GetFileAsync(filename);

            using (IRandomAccessStream textStream = await textFile.OpenReadAsync())
            {
                using (DataReader textReader = new DataReader(textStream))
                {
                    uint textLength = (uint)textStream.Size;
                    await textReader.LoadAsync(textLength);
                    contents = textReader.ReadString(textLength);
                }
            }
            return contents;
        }

        /// <summary>
        /// Save stream to file in app's local folder.
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<StorageFile> SaveStream(Stream inputStream, string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentException("Invalid file name (null).");
            }
            StorageFile localFile = await CreateFileInFolders(fileName);
            using (var localFileStream = await localFile.OpenStreamForWriteAsync())
            {
                await inputStream.CopyToAsync(localFileStream);
            }
            return localFile;
        }

        private static async Task<StorageFile> CreateFileInFolders(string fileName)
        {
            int lastPathSeperatorIndex = fileName.LastIndexOf("/");
            string fPath = null;
            string fName = fileName;
            if (lastPathSeperatorIndex >= 0)
            {
                fPath = fileName.Substring(0, lastPathSeperatorIndex);
                fName = fileName.Substring(lastPathSeperatorIndex + 1);
            }

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            string[] pathes = fPath.Split('/');
            for (int i = 0; i < pathes.Length; i++)
            {
                if (!string.IsNullOrEmpty(pathes[i].Trim()))
                {
                    localFolder = await localFolder.CreateFolderAsync(pathes[i], CreationCollisionOption.OpenIfExists);
                }
            }
            StorageFile localFile = await localFolder.CreateFileAsync(fName, CreationCollisionOption.ReplaceExisting);
            return localFile;
        }

        /// <summary>
        /// Get parent path.
        /// </summary>
        /// <param name="path">Input path MUST hasn't filename.</param>
        /// <returns>Return "" if input path is root "/" or hasn't "/" seperator.</returns>
        public static string getParentPath(string pathWithoutFileName)
        {
            string parentPath = "";
            string path = pathWithoutFileName;
            if (path.EndsWith("/"))
            {
                path = path.Substring(0, path.Length - 1);
            }
            if (!path.StartsWith("/"))
            {
                path = "/" + path;
            }
            string[] pathSegments = path.Split('/');
            if (pathSegments.Length > 2)
            {
                // there are 1 level of sub-directory at least, so show data of parent directory instead of exist application.
                parentPath = path.Substring(0, path.LastIndexOf("/"));
            }
            else if (pathSegments.Length == 2)
            {
                parentPath = path.Substring(0, 1);
            }
            return parentPath;
        }

        /// <summary>
        /// Convert bytes to readable KB/MB/GB.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ConvertBytesToKMGB(long bytes)
        {
            float measure = 1024f;
            double ret;
            string unit;
            if (bytes < measure)
            {
                ret = bytes;
                unit = "B";
            }
            else if (bytes / measure < measure)
            {
                ret = bytes / measure;
                unit = "KB";
            }
            else if (bytes / measure / measure < measure)
            {
                ret = bytes / measure / measure;
                unit = "MB";
            }
            else
            {
                ret = bytes / measure / measure / measure;
                unit = "GB";
            }
            ret = (int)(ret * 10 + 0.5) / 10.0;
            return ret + unit;
        }
    }
}
