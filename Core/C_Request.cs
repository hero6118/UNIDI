using Core.Models.Response;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Core
{
    public class C_Request
    {
        public static async Task<string> GetDataHttpClient(string url, Dictionary<string, string> headers = null)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            using (var client = new HttpClient())
            {
                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> entry in headers)
                    {
                        client.DefaultRequestHeaders.Add(entry.Key, entry.Value);
                    }
                }
                var result = await client.GetAsync(url);
                string resultContent = await result.Content.ReadAsStringAsync();
                return resultContent;
            }
        }
        public static async Task<string> PostDataHttpClient(string url, FormUrlEncodedContent param, Dictionary<string, string> headers = null)
        {
            using (var client = new HttpClient())
            {
                //var content = new FormUrlEncodedContent(new[]
                //{
                //    new KeyValuePair<string, string>("", "login")
                //});
                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> entry in headers)
                    {
                        client.DefaultRequestHeaders.Add(entry.Key, entry.Value);
                    }
                }
                var result = await client.PostAsync(url, param);
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    string resultContent = await result.Content.ReadAsStringAsync();
                    return resultContent;
                }
                else
                {
                    return result.ReasonPhrase;
                }
            }
        }
        public static async Task<string> PostDataHttpClient(string url, MultipartFormDataContent param, Dictionary<string, string> headers = null)
        {
            using (var client = new HttpClient())
            {
                //var requestContent = new MultipartFormDataContent
                //{
                //    { new StringContent(txtEnterpriseCode), "EnterpriseCode" },
                //    { new StringContent(txtConsignType), "ConsignType" }
                //};
                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> entry in headers)
                    {
                        client.DefaultRequestHeaders.Add(entry.Key, entry.Value);
                    }
                }
                var result = await client.PostAsync(url, param);
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    string resultContent = await result.Content.ReadAsStringAsync();
                    return resultContent;
                }
                else
                {
                    return result.StatusCode + " - " + result.ReasonPhrase;
                }
            }
        }
        public static async Task<string> PostData(string url, FormUrlEncodedContent param)
        {
            //var content = new FormUrlEncodedContent(new[]
            //{
            //    new KeyValuePair<string, string>("", "login")
            //});
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            using (var client = new HttpClient())
            {
                var result = await client.PostAsync(url, param);
                string resultContent = await result.Content.ReadAsStringAsync();
                return resultContent;
            }
        }
        public static string PostData(string url, WebHeaderCollection header, NameValueCollection param)
        {
            //var param = new NameValueCollection();
            //param.Add("userName", "");
            using (var client = new WebClient())
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                client.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
                if (header != null)
                {
                    client.Headers = header;
                }
                byte[] responsebytes = client.UploadValues(url, "POST", param);
                var data = Encoding.UTF8.GetString(responsebytes);
                return data;
            }
        }
        public static async Task<string> GetData(string url)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
                var responsebytes = await client.DownloadStringTaskAsync(url);
                return responsebytes;
            }
        }
        public static string GetData2(string url)
        {
            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
                var responsebytes = client.DownloadString(url);
                return responsebytes;
            }
        }

        public static string PostRawData(string url, string rawData)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(rawData);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                return streamReader.ReadToEnd();
            }
        }

        public static string APIUploadImage = "https://api.uto.vn/api/uploadimage";
        public static async Task<UploadImageResponse> UploadImage(IFormFile file, int size)
        {
            try
            {
                var requestContent = new MultipartFormDataContent();
                byte[] data;
                var stream = file.OpenReadStream();
                using (var br = new BinaryReader(stream))
                {
                    data = br.ReadBytes((int)stream.Length);
                }
                ByteArrayContent bytes = new ByteArrayContent(data);
                requestContent.Add(bytes, "ImageFile", file.FileName);
                requestContent.Add(new StringContent(size.ToString()), "Size");

                var request = await PostDataHttpClient(APIUploadImage, requestContent);
                var dataRequest = JsonConvert.DeserializeObject<UploadImageResponse>(request);
                return dataRequest;
            }
            catch (Exception ex)
            {
                return new UploadImageResponse { Status = false, Message = "uploadimage: " + ex.Message };
            }

        }
        public static async Task<UploadImageListResponse> UploadImage(List<IFormFile> files, int size)
        {
            try
            {
                var requestContent = new MultipartFormDataContent();

                foreach (var item in files)
                {
                    var stream = item.OpenReadStream();
                    byte[] data;
                    using (var br = new BinaryReader(stream))
                    {
                        data = br.ReadBytes((int)stream.Length);
                    }
                    ByteArrayContent bytes = new ByteArrayContent(data);
                    requestContent.Add(bytes, "ImageList", item.FileName);
                }

                requestContent.Add(new StringContent(size.ToString()), "Size");

                var request = await PostDataHttpClient("https://api.uto.vn/API/UploadImageList", requestContent);
                var dataRequest = JsonConvert.DeserializeObject<UploadImageListResponse>(request);
                return dataRequest;
            }
            catch (Exception ex)
            {
                return new UploadImageListResponse { Status = false, Message = "uploadimage: " + ex.Message };
            }
        }
        public static async Task<UploadImageResponse> UploadImage2(HttpPostedFile file, int size)
        {
            try
            {
                
                var requestContent = new MultipartFormDataContent();
                byte[] data;
                using (var br = new BinaryReader(file.InputStream))
                {
                    data = br.ReadBytes((int)file.InputStream.Length);
                }

                ByteArrayContent bytes = new ByteArrayContent(data);
                requestContent.Add(bytes, "ImageFile", file.FileName);
                requestContent.Add(new StringContent(size.ToString()), "Size");

                var request = await PostDataHttpClient(APIUploadImage, requestContent);
                var dataRequest = JsonConvert.DeserializeObject<UploadImageResponse>(request);
                return dataRequest;
            }
            catch (Exception ex)
            {
                return new UploadImageResponse { Status = false, Message = "uploadimage: " + ex.Message };
            }

        }
        public static async Task<UploadImageListResponse> UploadImage2(List<HttpPostedFileBase> files, int size)
        {
            try
            {
                var requestContent = new MultipartFormDataContent();

                foreach (var item in files)
                {
                    
                    byte[] data;
                    using (var br = new BinaryReader(item.InputStream))
                    {
                        data = br.ReadBytes((int)item.InputStream.Length);
                    }
                    ByteArrayContent bytes = new ByteArrayContent(data);
                    requestContent.Add(bytes, "ImageList", item.FileName);
                }

                requestContent.Add(new StringContent(size.ToString()), "Size");

                var request = await PostDataHttpClient(APIUploadImage, requestContent);
                var dataRequest = JsonConvert.DeserializeObject<UploadImageListResponse>(request);
                return dataRequest;
            }
            catch (Exception ex)
            {
                return new UploadImageListResponse { Status = false, Message = "uploadimage: " + ex.Message };
            }

        }


        /*  }
          catch (Exception ex)
          {
              return new UploadImageListResponse { Status = false, Message = "uploadimage: " + ex.Message };
          }*/

        /*

           public static async Task<UploadImageListResponse> UploadMultiImage(List<IFormFile> file, int size)
           {
               try
               {
                   var requestContent = new MultipartFormDataContent();

                   foreach (var item in file)
                   {

                       byte[] data;
                       using (var br = new BinaryReader(item.OpenReadStream()))
                       {
                           data = br.ReadBytes((int)item.OpenReadStream().Length);
                       }
                       ByteArrayContent bytes = new ByteArrayContent(data);
                       requestContent.Add(bytes, "ImageList", item.FileName);
                   };
                   requestContent.Add(new StringContent(size.ToString()), "Size");

                   var request = await PostDataHttpClient(APIUploadImage, requestContent);
                   var dataRequest = JsonConvert.DeserializeObject<UploadImageListResponse>(request);
                   return dataRequest;
               }

               catch (Exception ex)
               {
                   return new UploadImageListResponse { Status = false, Message = "uploadimage: " + ex.Message };
               }

           }*/

        public static async Task<UploadImageResponse> UploadImage(string base64, int size)
        {
            try
            {
                var requestContent = new MultipartFormDataContent
                {
                    { new StringContent(size.ToString()), "Size" },
                    { new StringContent(base64), "ImageBase64" }
                };

                var request = await PostDataHttpClient(APIUploadImage, requestContent);
                var dataRequest = JsonConvert.DeserializeObject<UploadImageResponse>(request);
                return dataRequest;
            }
            catch (Exception ex)
            {
                return new UploadImageResponse { Status = false, Message = ex.Message };
            }

        }

    }
    //new
   


    public class ResultDepositToPhone
    {
        public bool Check { get; set; }
        public string Ms { get; set; }
    }
    public class ResultDepositToPhoneCheckStatus
    {
        public int Check { get; set; }
        public string Ms { get; set; }
    }
}
