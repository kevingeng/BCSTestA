using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BCSTestA
{
    class Program
    {
        static void Main(string[] args)
        {
            //ATestMethod();
            IStorageProvider st = StorageFactory.GetDefaultStorageProvider();
            st.PutFile("/FileDB/4376479.jpg", @"D:\E\Nut\三爱\Ftp Full Backup\Temp\4376479.jpg");
            Console.WriteLine("Put Done");
            st.GetFile("/FileDB/4376479.jpg", @"D:\E\Nut\三爱\Ftp Full Backup\Temp\4376479.Download.jpg");
            Console.WriteLine("Get Done");
            Console.ReadLine();
        }

        private static void ATestMethod()
        {
            var bucketuri = "http://bcs.duapp.com/site3ai";
            var apikey = "RvMmTbRgQoYvoxOHjWbk1nZ4";
            var secretkey = "ZFVi6jx13hxcaSRBO22FnN9nd7GICezR";
            var flag = "MBO";
            var method = "PUT";
            var bucket = "site3ai";

            // Signature=urlencode(base64_encode(hash_hmac('sha1', Content, SecretKey,true)))
            var hmacsha1 = HMACSHA1.Create();//must dispose

            var obj = "FileDB/434109.jpg";
            hmacsha1.Key = Encoding.ASCII.GetBytes(secretkey);
            var actionstring = string.Format("{0}\nMethod={1}\nBucket={2}\nObject=/{3}\n", flag, method, bucket, obj);

            var actionhash = hmacsha1.ComputeHash(Encoding.ASCII.GetBytes(actionstring));
            hmacsha1.Dispose();

            var signvalue = System.Net.WebUtility.UrlEncode(Convert.ToBase64String(actionhash));

            var query = string.Format("sign={0}:{1}:{2}", flag, apikey, signvalue);

            var actionurl = string.Format("{0}/{1}?{2}", bucketuri, System.Net.WebUtility.UrlEncode(obj), query);

            System.Net.WebClient wc = new System.Net.WebClient();
            try
            {
                var fn = @"D:\E\Nut\三爱\Ftp Full Backup\Temp\434109.jpg";

                var result = wc.UploadFile(actionurl, "PUT", fn);
                //Console.WriteLine(System.Text.Encoding.UTF8.GetString(result));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

    }
    public class StorageFactory {
        public static IStorageProvider GetDefaultStorageProvider()
        {
            return new BcsStorage();
        }
        public static T GetStorageProvider<T>() where T:IStorageProvider,new() {
            return new T();
        }
    }
    public interface IStorageProvider:IDisposable {
        void PutFile(string targetPath,string localPath);
        void PutData(string targetPath, byte[] data);
        void GetFile(string targetPath,string localPath);
        byte[] GetData(string targetPath);
        void DeleteFile(string targetPath);
        void Config(Dictionary<string,object> cfg);
    }
    public class BcsStorage : IStorageProvider {
        BcsClient bcs = new BcsClient();

        public void PutFile(string targetPath, string localPath)
        {
            PutData(targetPath, System.IO.File.ReadAllBytes(localPath));
        }

        public void PutData(string targetPath, byte[] data)
        {
            bcs.PutObject(targetPath, data);
        }

        public void GetFile(string targetPath, string localPath)
        {
            System.IO.File.WriteAllBytes(localPath, GetData(targetPath));
        }

        public byte[] GetData(string targetPath)
        {
            return bcs.GetObject(targetPath);
        }
        public void DeleteFile(string targetPath) {
            bcs.DeleteObject(targetPath);
        }

        public void Config(Dictionary<string, object> cfg)
        {
            
        }

        public void Dispose()
        {
            bcs.Dispose();
        }
    }
    public class LocalStorage : IStorageProvider
    {

        public void PutFile(string targetPath, string localPath)
        {
            PutData(targetPath, System.IO.File.ReadAllBytes(localPath));
        }

        public void PutData(string targetPath, byte[] data)
        {
            System.IO.File.WriteAllBytes(targetPath, data);
        }

        public void GetFile(string targetPath, string localPath)
        {
            System.IO.File.WriteAllBytes(localPath, GetData(targetPath));
        }

        public byte[] GetData(string targetPath)
        {
            return System.IO.File.ReadAllBytes(targetPath);
        }

        public void DeleteFile(string targetPath)
        {
            System.IO.File.Delete(targetPath);
        }

        public void Config(Dictionary<string, object> cfg)
        {

        }

        public void Dispose()
        {

        }
    }
    public class BcsClient : IDisposable
    {
        string bucketuri = "http://bcs.duapp.com/site3ai";
        string apikey = "RvMmTbRgQoYvoxOHjWbk1nZ4";
        string secretkey = "ZFVi6jx13hxcaSRBO22FnN9nd7GICezR";
        string flag = "MBO";
        string bucket = "site3ai";
        const string METHOD_PUT = "PUT";
        const string METHOD_GET = "GET";
        const string METHOD_DELETE = "DELETE";
        HMAC hmac = null;
        System.Net.WebClient wc = new System.Net.WebClient();

        public  BcsClient()
        {
            hmac = HMACSHA1.Create();
            hmac.Key = Encoding.ASCII.GetBytes(secretkey);
        }

        //public void PutObject(string BcsPathAndName, string fn)
        //{
        //    var apicallurl = CreateApiUrl(METHOD_PUT, BcsPathAndName);
        //    var result = wc.UploadFile(apicallurl, METHOD_PUT, fn);
        //}
        public void PutObject(string BcsPathAndName, byte[] data)
        {
            var apicallurl = CreateApiUrl(METHOD_PUT, BcsPathAndName);
            
            var result = wc.UploadData(apicallurl, METHOD_PUT,data );
        }
        //public void GetObject(string BcsPathAndName, string fn)
        //{
        //    var bs = GetObject(BcsPathAndName);
        //    System.IO.File.WriteAllBytes(fn,bs);
        //}
        public byte[] GetObject(string BcsPathAndName)
        {
            var apicallurl = CreateApiUrl(METHOD_GET, BcsPathAndName);
            var result = wc.DownloadData(apicallurl);
            return result;
        }
        public void DeleteObject(string BcsPathAndName) {
            var apicallurl = CreateApiUrl(METHOD_DELETE, BcsPathAndName);
            var result = wc.DownloadData(apicallurl);
        }

        //public void DownloadObject(string BcsPathAndName, string fn)
        //{ 

        //}
        private string CreateApiUrl(string method, string BcsPathAndName)
        {
            var obj = BcsPathAndName.TrimStart('/');
            var signval = BcsSignature(method, obj);

            var query = string.Format("sign={0}:{1}:{2}", flag, apikey, signval);

            var apicallurl = string.Format("{0}/{1}?{2}", bucketuri, System.Net.WebUtility.UrlEncode(obj), query);
            return apicallurl;
        }


        private string BcsSignature(string method, string obj)
        {
            var actionstring = string.Format("{0}\nMethod={1}\nBucket={2}\nObject=/{3}\n", flag, method, bucket, obj);
            var hash= hmac.ComputeHash(Encoding.ASCII.GetBytes(actionstring));
            return System.Net.WebUtility.UrlEncode(Convert.ToBase64String(hash));
        }
        public void Dispose()
        {
            if (null != hmac) hmac.Dispose();
            hmac = null;
        }
    }
    
}
