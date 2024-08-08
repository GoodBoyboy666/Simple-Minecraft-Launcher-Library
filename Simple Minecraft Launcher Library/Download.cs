using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.IO.Compression;
using Newtonsoft.Json;
using System.Reflection;
using System.IO;

namespace Simple_Minecraft_Launcher_Library
{
    public delegate void TaskDownLoadAssetCompletedCallback(bool result);
    public delegate void TaskDownLoadLibCompletedCallback(bool result);

    public enum SystemPlatform
    {
        Windows, OSX, Linux
    }
    public class Download
    {
        public int libcount { get; internal set; }
        public int assetcount { get; internal set; }
        string game_json, asset_url;
        JObject game;
        List<AssetItem> items;
        List<Library> libs;

        /// <summary>
        /// 下载游戏类库
        /// </summary>
        /// <param name="game_input">游戏清单文件JSON</param>
        /// <param name="min_work_thread">最小工作线程</param>
        /// <param name="max_work_thread">最大工作线程</param>
        /// <param name="min_io_thread">最小IO线程</param>
        /// <param name="max_io_thread">最大IO线程</param>
        /// <exception cref="Exception"></exception>
        public Download(string game_input, int min_work_thread=1, int max_work_thread=4, int min_io_thread = 1,int max_io_thread=4)
        {
            game_json = game_input;
            game = Json.DeserializeObject<JObject>(game_json) ?? throw new Exception("Can not get Game info !");
            asset_url = GetAssetIndex()?.url ?? "";
            items = Task.Run(async () => { return Asset.GetAssetItem(await HttpRequest.GetRequestString(asset_url)); }).Result;
            assetcount = items.Count;
            libs = GetLib();
            libcount= libs.Count;
            ThreadPool.SetMinThreads(min_work_thread, min_io_thread);
            ThreadPool.SetMaxThreads(max_work_thread, max_io_thread);
        }


        /// <summary>
        /// 获取Client文件信息
        /// </summary>
        /// <returns></returns>
        public Client? GetClientInfo()
        {
            return game?["downloads"]?["client"]?.ToObject<Client>();
        }

        /// <summary>
        /// 获取AssetIndex信息
        /// </summary>
        /// <returns></returns>
        public AssetIndex? GetAssetIndex()
        {
            return game?["assetIndex"]?.ToObject<AssetIndex>();
        }

        /// <summary>
        /// 获取Log4j2File信息
        /// </summary>
        /// <returns></returns>
        public Log4j2File? GetLog4j2File()
        {
            return game?["logging"]?["client"]?["file"]?.ToObject<Log4j2File>();
        }

        /// <summary>
        /// 获取Libraries
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<Library> GetLib()
        {
            List<Library> Libraries = new List<Library>();
            for (int i = 0; i < game?["libraries"]?.Count(); i++)
            {
                Library lib = game?["libraries"]?[i]?.ToObject<Library>() ?? throw new Exception("Object is null");

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && CanAddLib(lib, SystemPlatform.Windows))
                {
                    if (lib?.downloads?.classifiers?.natives_windows != null)
                    {
                        lib.downloads.classifiers.classifierInfo = new ClassifierInfo();
                        lib.downloads.classifiers.classifierInfo.path = lib.downloads.classifiers.natives_windows?.path;
                        lib.downloads.classifiers.classifierInfo.url = lib.downloads.classifiers.natives_windows?.url;
                        lib.downloads.classifiers.classifierInfo.sha1 = lib.downloads.classifiers.natives_windows?.sha1;
                        lib.downloads.classifiers.classifierInfo.size = lib.downloads.classifiers.natives_windows?.size;
                    }

                    if (lib != null)
                        Libraries.Add(lib);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && CanAddLib(lib, SystemPlatform.OSX))
                {
                    if (lib?.downloads?.classifiers?.natives_osx != null)
                    {
                        lib.downloads.classifiers.classifierInfo = new ClassifierInfo();
                        lib.downloads.classifiers.classifierInfo.path = lib.downloads.classifiers.natives_osx?.path;
                        lib.downloads.classifiers.classifierInfo.url = lib.downloads.classifiers.natives_osx?.url;
                        lib.downloads.classifiers.classifierInfo.sha1 = lib.downloads.classifiers.natives_osx?.sha1;
                        lib.downloads.classifiers.classifierInfo.size = lib.downloads.classifiers.natives_osx?.size;
                    }

                    if (lib != null)
                        Libraries.Add(lib);
                }
                else if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && CanAddLib(lib, SystemPlatform.Linux))
                {
                    if (lib?.downloads?.classifiers?.natives_linux != null)
                    {
                        lib.downloads.classifiers.classifierInfo = new ClassifierInfo();
                        lib.downloads.classifiers.classifierInfo.path = lib.downloads.classifiers.natives_linux?.path;
                        lib.downloads.classifiers.classifierInfo.url = lib.downloads.classifiers.natives_linux?.url;
                        lib.downloads.classifiers.classifierInfo.sha1 = lib.downloads.classifiers.natives_linux?.sha1;
                        lib.downloads.classifiers.classifierInfo.size = lib.downloads.classifiers.natives_linux?.size;
                    }

                    if (lib != null)
                        Libraries.Add(lib);
                }
            }
            return Libraries;
        }

        /// <summary>
        /// 判断Lib是否可在当前计算机系统使用
        /// </summary>
        /// <param name="lib">Library对象</param>
        /// <param name="platform">SystemPlatform枚举</param>
        /// <returns></returns>
        public bool CanAddLib(Library? lib, SystemPlatform platform)
        {
            if (lib == null)
            {
                return false;
            }

            if (lib.rules == null)
            {
                return true;
            }

            switch (platform)
            {
                case SystemPlatform.Windows:
                    {
                        bool able = false;
                        foreach (var rule in lib.rules)
                        {
                            if (rule.action == "disallow" && rule?.os?.name != "windows")
                                able = true;
                            else if (rule?.action == "allow" && rule.os == null || rule?.action == "allow" && rule?.os?.name == "windows")
                                able = true;
                        }
                        return able;
                    }
                case SystemPlatform.OSX:
                    {
                        bool able = true;
                        foreach (var rule in lib.rules)
                        {
                            if (rule.action == "disallow" && rule?.os?.name == "osx" || rule?.action == "allow" && rule.os != null && rule?.os?.name != "osx")
                                able = false;
                        }
                        return able;
                    }
                case SystemPlatform.Linux:
                    {
                        bool able = true;
                        foreach (var rule in lib.rules)
                        {
                            if (rule.action == "disallow" && rule?.os?.name == "linux" || rule?.action == "allow" && rule.os != null && rule?.os?.name != "linux")
                                able = false;
                        }
                        return able;
                    }
                default:
                    return false;
            }
        }

        /// <summary>
        /// 下载ClientJar
        /// </summary>
        /// <param name="path">保存路径（包含文件名）</param>
        /// <returns>是否正常完成下载</returns>
        public async Task<bool> DownLoadClientJar(string path)
        {
            bool isok = true;
            if (!File.Exists(path) || FileSha1.GetFileSha1(path) != GetClientInfo()?.sha1)
                isok = await HttpRequest.DownloadFile(GetClientInfo()?.url ?? "", path);

            if (isok)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 下载Log4j2File
        /// </summary>
        /// <param name="path">保存路径（包含文件名）</param>
        /// <returns>是否正常完成下载</returns>
        public async Task<bool> DownLoadLog4j2File(string path)
        {
            bool isok = true;
            if (!File.Exists(path) || FileSha1.GetFileSha1(path) != GetLog4j2File()?.sha1)
                isok = await HttpRequest.DownloadFile(GetLog4j2File()?.url ?? "", path);

            if (isok)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 释放自带的Log4j2File（推荐）
        /// </summary>
        /// <param name="path">Log4j2File文件路径</param>
        /// <returns></returns>
        public static bool ExtractLog4j2File(string path)
        {
            if (!File.Exists(path))
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    assembly?.GetManifestResourceStream("Simple_Minecraft_Launcher_Library.Resources.log4j2.xml")?.CopyTo(fileStream);
                }
            }
            return true;
        }

        /// <summary>
        /// 下载Asset
        /// </summary>
        /// <param name="directory">路径文件夹（末尾带/）</param>
        /// <returns></returns>
        public void DownLoadAsset(string directory, TaskDownLoadAssetCompletedCallback callback)
        {

            Task.Run(async () => { await HttpRequest.DownloadFile(asset_url, directory + "indexes/" + GetAssetIndex()?.id + ".json"); }).Wait();

            CountdownEvent countdownEvent = new CountdownEvent(items.Count);
            foreach (var item in items)
            {
                ThreadPool.QueueUserWorkItem(async (state) =>
                {
                    string asset_path = directory + "objects/" + item?.hash?.Substring(0, 2) + "/" + item?.hash;
                    bool ok = true;
                    if (!File.Exists(asset_path) || FileSha1.GetFileSha1(asset_path) != item?.hash)
                    {
                        int error_count = 0;
                        do
                        {
                            ok = await HttpRequest.DownloadFile("https://resources.download.minecraft.net/" + item?.hash?.Substring(0, 2) + "/" + item?.hash, asset_path);
                            if(!ok)
                                error_count++;
                            if (error_count >= 3)
                                break;
                        }
                        while (!ok); 
                    }
                    callback(ok);
                    countdownEvent.Signal();
                });
                //Console.WriteLine(now + "/" + items.Count);
            }
            countdownEvent.Wait();
        }

        /// <summary>
        /// 下载Library
        /// </summary>
        /// <param name="artifact_directory">普通库文件夹（末尾带/）</param>
        /// <param name="classifiers_directory">native文件夹（末尾带/）</param>
        /// <returns></returns>
        public void DownloadLib(string artifact_directory, string classifiers_directory,TaskDownLoadLibCompletedCallback callback)
        {
            CountdownEvent countdownEvent = new CountdownEvent(libs.Count);
            foreach (var lib in libs)
            {
                ThreadPool.QueueUserWorkItem(async (state) =>
                {
                    bool ok_artifact = true;
                    string artifact_path = artifact_directory + lib?.downloads?.artifact?.path ?? "";
                    if ((!File.Exists(artifact_path) || FileSha1.GetFileSha1(artifact_path) != lib?.downloads?.artifact?.sha1)&& lib?.downloads?.artifact!=null)
                    {
                        int error_count = 0;
                        do
                        {
                            ok_artifact = await HttpRequest.DownloadFile(lib?.downloads?.artifact?.url ?? "", artifact_path);
                            if (!ok_artifact)
                                error_count++;
                            if (error_count >= 3)
                                break;
                        }
                        while (!ok_artifact);
                    }
                    bool ok_classifiers = true;
                    if (lib?.downloads?.classifiers?.classifierInfo != null)
                    {
                        int error_count = 0;
                        do
                        {
                            string[]? temp = lib?.downloads?.classifiers?.classifierInfo?.path?.Split('/');
                            string path = classifiers_directory + temp?[temp.Count() - 1];
                        if (!File.Exists(path) || FileSha1.GetFileSha1(path) != lib?.downloads.classifiers.classifierInfo.sha1)
                        {
                            ok_classifiers = await HttpRequest.DownloadFile(lib?.downloads?.classifiers?.classifierInfo?.url ?? "", path);
                            ZipFile.ExtractToDirectory(path, classifiers_directory,true);
                            File.Delete(path);
                        }
                            if (!ok_classifiers)
                                error_count++;
                            if (error_count >= 3)
                                break;
                        }
                        while (!ok_classifiers);
                    }
                    bool all_ok = true;
                    if (ok_artifact == false || ok_classifiers == false)
                    {
                        all_ok = false;
                    }
                    callback(all_ok);
                    countdownEvent.Signal();
                });
            }
            countdownEvent.Wait();
        }

    }

    #region Client实体类
    public class Client
    {
        /// <summary>
        /// sha1散列
        /// </summary>
        public string? sha1 { get; set; }
        /// <summary>
        /// 大小
        /// </summary>
        public int size { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string? url { get; set; }
    }

    #endregion

    #region AssetIndex实体类
    public class AssetIndex
    {
        /// <summary>
        /// ID
        /// </summary>
        public string? id { get; set; }
        /// <summary>
        /// sha1散列
        /// </summary>
        public string? sha1 { get; set; }
        /// <summary>
        /// 大小
        /// </summary>
        public int size { get; set; }
        /// <summary>
        /// 总共大小
        /// </summary>
        public int totalSize { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string? url { get; set; }
    }
    #endregion

    #region log4j2File实体类
    public class Log4j2File
    {
        /// <summary>
        /// ID
        /// </summary>
        public string? id { get; set; }
        /// <summary>
        /// sha1散列
        /// </summary>
        public string? sha1 { get; set; }
        /// <summary>
        /// 大小
        /// </summary>
        public int size { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string? url { get; set; }
    }
    #endregion

    #region Library实体类
    public class Artifact
    {
        /// <summary>
        /// 
        /// </summary>
        public string? path { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? sha1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? size { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? url { get; set; }
    }

    public class Natives_linux
    {
        /// <summary>
        /// 
        /// </summary>
        public string? path { get; set; }
        /// <summary>
        ///
        /// </summary>
        public string? sha1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? size { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? url { get; set; }
    }

    public class Natives_osx
    {
        /// <summary>
        /// 
        /// </summary>
        public string? path { get; set; }
        /// <summary>
        ///
        /// </summary>
        public string? sha1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? size { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? url { get; set; }
    }

    public class Natives_windows
    {
        /// <summary>
        /// 
        /// </summary>
        public string? path { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? sha1 { get; set; }
        /// <summary>
        ///
        /// </summary>
        public int? size { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? url { get; set; }
    }

    public class Classifiers
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("natives-linux")]
        public Natives_linux? natives_linux { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("natives-osx")]
        public Natives_osx? natives_osx { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("natives-windows")]
        public Natives_windows? natives_windows { get; set; }

        public ClassifierInfo? classifierInfo { get; set; }
    }

    public class ClassifierInfo
    {
        public string? path { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? sha1 { get; set; }
        /// <summary>
        ///
        /// </summary>
        public int? size { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? url { get; set; }
    }

    public class Downloads
    {
        /// <summary>
        /// Artifact
        /// </summary>
        public Artifact? artifact { get; set; }
        /// <summary>
        /// Classifiers
        /// </summary>
        public Classifiers? classifiers { get; set; }
    }

    public class Extract
    {
        /// <summary>
        /// Exclude
        /// </summary>
        public List<string>? exclude { get; set; }
    }

    public class Natives
    {
        /// <summary>
        /// natives-linux
        /// </summary>
        public string? linux { get; set; }
        /// <summary>
        /// natives-osx
        /// </summary>
        public string? osx { get; set; }
        /// <summary>
        /// natives-windows
        /// </summary>
        public string? windows { get; set; }
    }

    public class Os
    {
        public string? name { get; set; }
    }
    public class Rules
    {
        /// <summary>
        /// allow
        /// </summary>
        public string? action { get; set; }
        public Os? os { get; set; }
    }

    public class Library
    {
        /// <summary>
        /// Downloads
        /// </summary>
        public Downloads? downloads { get; set; }
        /// <summary>
        /// Extract
        /// </summary>
        public Extract? extract { get; set; }
        /// <summary>
        /// org.lwjgl.lwjgl:lwjgl-platform:2.9.4-nightly-20150209
        /// </summary>
        public string? name { get; set; }
        /// <summary>
        /// Natives
        /// </summary>
        public Natives? natives { get; set; }
        /// <summary>
        /// Rules
        /// </summary>
        public List<Rules>? rules { get; set; }
    }

    #endregion
}
