using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_Minecraft_Launcher_Library
{
    public class Json
    {
        /// <summary>
        /// 解析Json
        /// </summary>
        /// <typeparam name="T">想要解析成的类型</typeparam>
        /// <param name="json">JSON字符串</param>
        /// <returns></returns>
        public static T? DeserializeObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }
    }
}
