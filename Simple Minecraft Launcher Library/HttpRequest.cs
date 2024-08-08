using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;

namespace Simple_Minecraft_Launcher_Library
{
    public class HttpRequest
    {
        /// <summary>
        /// HTTP Get请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> GetRequest(string url)
        {
            using HttpClient client = new HttpClient();
            return await client.GetAsync(url);
        }

        /// <summary>
        /// HTTP Get请求（返回String）
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <returns></returns>
        public static async Task<string> GetRequestString(string url)
        {
            HttpResponseMessage response = await GetRequest(url);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="url">链接</param>
        /// <param name="path">路径</param>
        /// <returns>是否正常下载完成</returns>
        public static async Task<bool> DownloadFile(string url,string path)
        {
            using(HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    if(!Directory.Exists(Path.GetDirectoryName(path)))
                        Directory.CreateDirectory(Path.GetDirectoryName(path)??throw new Exception("Error Path!"));
                    using (var filestream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(filestream);
                    }
                    return true;

                }catch (Exception)
                {
                    return false;
                }
            }
            
        }
    }
}
