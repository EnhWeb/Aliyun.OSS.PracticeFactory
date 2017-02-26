using Aliyun.OSS;
using System;
using System.Security.Cryptography;
using System.Text;

namespace WebEnhAliyunOss
{
    /// <summary>
    /// WebEnh OSS 文件上传 帮助类
    /// </summary>
    public class WebEnhOssUploadsHelper
    {
        #region [==========OSS 配置 后期改为从Web.config 中读取，或从 app.config 中读取==========]
        /// <summary>
        /// OSS 配置 后期改为从Web.config 中读取，或从 app.config 中读取 #TODO
        /// </summary>
        public static class OssConfig
        {
            //OSS外网域名: practicefactory-images.oss-cn-hangzhou.aliyuncs.com	OSS内网域名:practicefactory-images.oss-cn-hangzhou-internal.aliyuncs.com
            //此区域内网EndPoint，主要在SDK中使用
            //http://practicefactory-images.oss-cn-hangzhou.aliyuncs.com/Uploads/Images/BA90F74F1489E668/4B2A7B9FD0BC51021DB8DD89975A1E4A635B6EC280D77174928A34AA960F4319853F144E0D96C5E2.jpg

            /// <summary>
            /// 密匙
            /// </summary>
            public static string AccessKeyId = "LTAIfouLaw85PVgp";
            /// <summary>
            /// 密钥
            /// </summary>
            public static string AccessKeySecret = "qz65Loptt7oZ3rJxtynsLztrL57ySM";
            /// <summary>
            /// 入口地址
            /// OSS开通Region和Endpoint对照表
            /// https://help.aliyun.com/document_detail/31837.html?spm=5176.2020520105.147.4.973UWx
            /// </summary>
            public static string Endpoint = "oss-cn-shenzhen.aliyuncs.com"; //"oss.aliyuncs.com";
            /// <summary>
            /// 容器名称，可以更改，自动创建 #TODO 还需上传时检查，然后没有则创建
            /// </summary>
            public static string BucketName = "practicefactory-images";
            /// <summary>
            /// 外网访问域名
            /// </summary>
            public static string OssWlanDNS = "practicefactory-images.oss-cn-shenzhen.aliyuncs.com";
            /// <summary>
            /// 内网访问域名
            /// </summary>
            public static string OssLanDNS = "practicefactory-images.oss-cn-shenzhen-internal.aliyuncs.com";
            /// <summary>
            /// 图片保存到的 vPath 路径，不包括文件名
            /// </summary>
            public static string ImagesSaveTo_Root_vPath = $"Uploads/Images/{DESEncrypt.Encrypt(DateTime.Now.ToString("yyyy-MM"))}/";//$"Uploads/Images/{DateTime.Now.ToString("yyyy-MM")}/";
        }
        #endregion

        #region [==========上传成功或失败的返回结果==========]
        /// <summary>
        /// 上传成功或失败的返回结果 实体对像
        /// </summary>
        public class OssUploadResult
        {
            /// <summary>
            /// 状态，上传成功返回 true，上传失败返回 false。
            /// </summary>
            public bool State { get; set; } = false;
            /// <summary>
            /// 消息，成功或失败后的提示！
            /// </summary>
            public string Message { get; set; } = "";
            /// <summary>
            /// 上传成功后，文件的外网访问完整网址
            /// </summary>
            public string FileWLanUrl { get; set; } = "";
            /// <summary>
            /// 图片在OSS容器中，的相对路径  如：Uploads/Images/2017-02/abcd.jpg，不包括域名，和开头的“/”
            /// </summary>
            public string vPath { get; set; } = "";

            /// <summary>
            /// 转换为字符串
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return $"上传：{(State?"成功":"失败")}，{Message}，访问网址：{FileWLanUrl}";
            }
        } 
        #endregion

        #region [==========变量==========]
        /// <summary>
        /// 全局 Oss 通信客户端
        /// </summary>
        private static OssClient client = new OssClient(
            endpoint: OssConfig.Endpoint,
            accessKeyId: OssConfig.AccessKeyId,
            accessKeySecret: OssConfig.AccessKeySecret
            );
        /// <summary>
        /// 全局 容器名称
        /// </summary>
        private static string bucketName = OssConfig.BucketName;
        #endregion

        #region ========== 上传文件到阿里云OSS对像库中 UploadFileToOSS 完整参数方法 ==========
        /// <summary>
        /// 上传文件到阿里云OSS对像库中
        /// </summary>
        /// <param name="_client">OSS 客户端，用于访问OSS</param>
        /// <param name="_bucketName">容器名称</param>
        /// <param name="_filePath">文件路径</param>
        /// <param name="_vPath">保存到OSS中的路径，不要以“/”开头，示例格式：Uploads/Images/20170226.2356.aaabbb.jpg，注：文件夹会自动创建。</param>
        /// <returns></returns>
        public static OssUploadResult UploadFileToOSS(OssClient _client, string _bucketName, string _filePath, string _vPath)
        {
            try
            {

                #region 检查需上传的文件是否存在
                var fileinfo = new System.IO.FileInfo(_filePath);
                if (!fileinfo.Exists)
                {
                    throw new Exception($"需要上传的文件不存在，路径：{_filePath}！");
                }
                #endregion

                #region 检查当前 buckets 容器是否存在，不存在则创建
                var buckets = client.ListBuckets();
                bool bucketIsExist = false;
                foreach (var bucket in buckets)
                {
                    Console.WriteLine($"Name:{bucket.Name},{bucket.Owner},{bucket.Location},{bucket.CreationDate}");
                    if(bucket.Name == _bucketName)
                    {
                        bucketIsExist = true;
                        break;
                    }
                }
                if (!bucketIsExist)//容器 不存在则创建
                {
                    CreateBucket(_bucketName);
                } 
                #endregion

                _client.PutObject(
                        bucketName: _bucketName,//容器名称
                        key: _vPath,//保存vPath路径，包括文件名，不要以“/”开头
                        fileToUpload: _filePath //需要上传的的文件路径
                    );

                return new OssUploadResult
                {
                    State = true,
                    Message = "文件上传成功",
                    //FileWLanUrl = $"http://{_bucketName}.{OssConfig.Endpoint}/{_vPath}"
                    FileWLanUrl = $"http://{OssConfig.OssWlanDNS}/{_vPath}",
                    vPath = _vPath
                };
            }
            catch (Exception ex)
            {
                return new OssUploadResult
                {
                    State = false,
                    Message = $"文件上传失败，错误消息：{ex.Message}，参数值：bucketName：{_bucketName}fileToUpload：{_filePath} Key：{_vPath}"
                };
            }
        }
        #endregion

        #region ========== 上传文件到阿里云OSS对像库中 UploadFileToOSS 重载参数方法 ==========
        public static OssUploadResult UploadFileToOSS(string _bucketName, string _filePath, string _vPath)
        {
            return UploadFileToOSS(client, _bucketName, _filePath, _vPath);
        }

        public static OssUploadResult UploadFileToOSS(string _filePath, string _vPath)
        {
            return UploadFileToOSS(client, bucketName, _filePath, _vPath);
        }

        public static OssUploadResult UploadFileToOSS(string _filePath)
        {
            return UploadFileToOSS(client, bucketName, _filePath, OssConfig.ImagesSaveTo_Root_vPath + GetDesFileName(_filePath));
        }
        #endregion

        /// <summary>
        /// 创建 Bucket 容器，权限默认为公开读 ，如果成功则返回 bucket ,失败则 throw 异常。
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        public static Bucket CreateBucket(string bucketName)
        {
            try
            {
                // 创建容器
                Bucket bucket = client.CreateBucket(bucketName);
                // 设置容器权限 公共读，任何人都可以读，有权限才可以写
                client.SetBucketAcl(bucketName, CannedAccessControlList.PublicRead);
                return bucket;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        #region [==========将文件名进行DES加密，然后再进行UrlEncode处理，返回加密处理后的文件名==========]
        /// <summary>
        /// 将文件名进行DES加密，然后再进行UrlEncode处理，返回加密处理后的文件名
        /// </summary>
        /// <param name="strFileName">本地文件完整路径</param>
        /// <returns></returns>
        public static string GetDesFileName(string strFileName)
        {
            var fileinfo = new System.IO.FileInfo(strFileName);

            strFileName = DESEncrypt.Encrypt($"{DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss")}.{new Random().Next(100000000, 999999999)}${fileinfo.Name}");

            //strFileName = DESEncrypt.UrlTokenEncode(strFileName);

            strFileName = strFileName + fileinfo.Extension;

            return strFileName;
        } 
        #endregion
    }

    /// <summary>
    /// DES加密/解密类。
    /// Copyright (C) Maticsoft
    /// </summary>
    public static class DESEncrypt
    {
        #region ========加密======== 

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static string Encrypt(string Text)
        {
            return Encrypt(Text, "ZDAP-PracticeFactoryForDotNet");
        }
        /// <summary> 
        /// 加密数据 
        /// </summary> 
        /// <param name="Text"></param> 
        /// <param name="sKey"></param> 
        /// <returns></returns> 
        public static string Encrypt(string Text, string sKey)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray;
            inputByteArray = Encoding.Default.GetBytes(Text);
            des.Key = ASCIIEncoding.ASCII.GetBytes(System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(sKey, "md5").Substring(0, 8));
            des.IV = ASCIIEncoding.ASCII.GetBytes(System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(sKey, "md5").Substring(0, 8));
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            StringBuilder ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            return ret.ToString();
        }

        #endregion

        #region ========解密======== 
        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static string Decrypt(string Text)
        {
            return Decrypt(Text, "ZDAP-PracticeFactoryForDotNet");
        }
        /// <summary> 
        /// 解密数据 
        /// </summary> 
        /// <param name="Text"></param> 
        /// <param name="sKey"></param> 
        /// <returns></returns> 
        public static string Decrypt(string Text, string sKey)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            int len;
            len = Text.Length / 2;
            byte[] inputByteArray = new byte[len];
            int x, i;
            for (x = 0; x < len; x++)
            {
                i = Convert.ToInt32(Text.Substring(x * 2, 2), 16);
                inputByteArray[x] = (byte)i;
            }
            des.Key = ASCIIEncoding.ASCII.GetBytes(System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(sKey, "md5").Substring(0, 8));
            des.IV = ASCIIEncoding.ASCII.GetBytes(System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(sKey, "md5").Substring(0, 8));
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Encoding.Default.GetString(ms.ToArray());
        }
        #endregion

        #region ========Md5加密========
        /// <summary>
        /// Md5加密 
        /// </summary>
        /// <param name="orgStrig"></param>
        /// <returns></returns>
        public static string Md5(string orgStrig)
        {
            if (string.IsNullOrEmpty(orgStrig))
            {
                return "";

            }
            byte[] result = Encoding.Default.GetBytes(orgStrig);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(result);
            return BitConverter.ToString(output).Replace("-", "").ToLower();
            // return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(orgStrig, "MD5").ToLower();
        }
        #endregion

        #region ========URL 编码，解码========
        /// <summary>
        /// Base64 URL编码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UrlTokenEncode(string str)
        {
            return System.Web.HttpServerUtility.UrlTokenEncode(System.Text.Encoding.Default.GetBytes(str));
        }

        /// <summary>
        /// Base64 URL解码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UrlTokenDecode(string str)
        {
            return System.Text.Encoding.Default.GetString(System.Web.HttpServerUtility.UrlTokenDecode(str));
        } 
        #endregion
    }
}
