using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_Minecraft_Launcher_Library
{
    public class FileSha1
    {
        /// <summary>
        /// 获取文件SHA1信息
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public static string GetFileSha1(string path)
        {
            string sha1 = "";
            if (System.IO.File.Exists(path))
            {
                using (System.IO.FileStream filestream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    System.Security.Cryptography.SHA1 security_sha1 = System.Security.Cryptography.SHA1.Create();
                    Byte[] buffer = security_sha1.ComputeHash(filestream);
                    StringBuilder stringBuilder = new StringBuilder();
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        stringBuilder.Append(buffer[i].ToString("x2"));
                    }
                    sha1 = stringBuilder.ToString();
                }
            }
            return sha1;
        }
    }
}
