using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Simple_Minecraft_Launcher_Library
{
    public class Version
    {
        /// <summary>
        /// 版本类型
        /// </summary>
        public enum _Version
        {
            Latest, Snapshot
        }

        /// <summary>
        /// 获取VersionManifest并返回String
        /// </summary>
        /// <returns>string</returns>
        public static async Task<string> GetVersionManifestString()
        {
            return await HttpRequest.GetRequestString("https://piston-meta.mojang.com/mc/game/version_manifest.json");
        }

        /// <summary>
        /// 获取VersionManifest
        /// </summary>
        /// <returns>Version_Manifest</returns>
        public static async Task<Version_Manifest?> GetVersionManifest()
        {
            return Json.DeserializeObject<Version_Manifest>(await GetVersionManifestString());
        }

        /// <summary>
        /// 获取最新版本ID
        /// </summary>
        /// <param name="version">版本类型</param>
        /// <returns>string</returns>
        public static async Task<string?> GetLatestVersionID(_Version version)
        {
            Version_Manifest? version_Manifest = await GetVersionManifest();
            switch (version)
            {
                case _Version.Latest:
                    {
                        return version_Manifest?.latest?.release;
                    }
                case _Version.Snapshot:
                    {
                        return version_Manifest?.latest?.snapshot;
                    }
                default:
                    {
                        return null;
                    }
            }
        }
    }

    #region Version实体类
    public class Latest
    {
        /// <summary>
        /// 正式版
        /// </summary>
        public string? release { get; set; }
        /// <summary>
        /// 快照
        /// </summary>
        public string? snapshot { get; set; }
    }
    public class VersionsItem
    {
        /// <summary>
        /// Version ID
        /// </summary>
        public string? id { get; set; }
        /// <summary>
        /// 版本类型
        /// </summary>
        public string? type { get; set; }
        /// <summary>
        /// 游戏文件清单
        /// </summary>
        public string? url { get; set; }
        /// <summary>
        /// 版本更新时间
        /// </summary>
        public string? time { get; set; }
        /// <summary>
        /// 版本发布时间
        /// </summary>
        public string? releaseTime { get; set; }
    }
    public class Version_Manifest
    {
        /// <summary>
        /// 
        /// </summary>
        public Latest? latest { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<VersionsItem>? versions { get; set; }
    }
    #endregion
}
