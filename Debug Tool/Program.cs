using Newtonsoft.Json.Linq;
using Simple_Minecraft_Launcher_Library;
using System.Diagnostics.Metrics;
using static Simple_Minecraft_Launcher_Library.Environment;
namespace Debug_Tool
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("获取Minecraft版本列表");
            Version_Manifest versions = await Simple_Minecraft_Launcher_Library.Version.GetVersionManifest() ?? new Version_Manifest();
            string Root_Directory = "C:\\Users\\GoodBoyboy\\Desktop\\test";//Change it, the last path don't need '/'
            string Java_Path=Simple_Minecraft_Launcher_Library.Environment.GetJavaPath()??"";//maybe need change
            string Player_Name = "GoodBlueSky";//Change it
            string UUID = "\"6715ce10cbe14474a7be5b4f7f02bd31\"";//Change it
            string Access_Token = " ";//Change it
            string game_url = "";
            Console.WriteLine("请选择版本类型：1.正式版 2.快照版 3.远古版");
            string type = "";
            switch (Console.ReadLine())
            {
                case "1":
                    type = "release";
                    break;
                case "2":
                    type = "snapshot";
                    break;
                case "3":
                    type = "old_beta";
                    break;
                default:
                    throw new Exception("错误的选项");

            }
            foreach (var version in versions?.versions ?? new List<VersionsItem>())
            {
                if(version.type==type)
                Console.WriteLine(version.id);
            }
            Console.WriteLine("请输入启动版本");
            string ver= Console.ReadLine()??"";
            foreach (var version in versions?.versions ?? new List<VersionsItem>())
            {
                if (version.id == ver)
                {
                    game_url = version.url ?? "";
                }
            }
            if (game_url == "")
            {
                throw new Exception("No Game url !");
            }

            string game_json = await HttpRequest.GetRequestString(game_url);

            AllGamePath allGamePath = GetDefalutAllGamePath(Root_Directory, ver);

            Console.WriteLine("下载Minecraft");

            Download d = new Download(game_json);

            if (!await d.DownLoadClientJar(allGamePath.GameJarFile))
            {
                Console.WriteLine("Download ClientJar error!");
            }
            else
            {
                Console.WriteLine("Download ClientJar successful !");
            }

            //if (!await d.DownLoadLog4j2File(allGamePath.Log4jConfigurationFile))
            //{
            //    Console.WriteLine("Download Log4jConfigurationFile error!");
            //}
            //else
            //{
            //    Console.WriteLine("Download Log4jConfigurationFile successful !");
            //}

            if (!Download.ExtractLog4j2File(allGamePath.Log4jConfigurationFile))
            {
                Console.WriteLine("Extract Log4jConfigurationFile error!");
            }
            else
            {
                Console.WriteLine("Extract Log4jConfigurationFile successful !");
            }
            int asset_downloaded_count = 0;
            int asset_total = d.assetcount;
            bool asset_isok = true;
            object lockObject = new object();
            TaskDownLoadAssetCompletedCallback taskDownLoadAssetCompletedCallback = (bool result) =>
            {
                if(!result)
                    asset_isok=false;

                lock (lockObject)
                {
                    asset_downloaded_count++;
                }
                Console.WriteLine(asset_downloaded_count + "/" + asset_total);//多线程打印问题
            };
            d.DownLoadAsset(allGamePath.AssetsDirectory + "/", taskDownLoadAssetCompletedCallback);
            if (!asset_isok)
            {
                Console.WriteLine("Download Assets error!");
            }
            else
            {
                Console.WriteLine("Download Assets successful !");
            }

            int lib_downloaded_count = 0;
            int lib_total=d.libcount;
            bool lib_isok = true;
            TaskDownLoadLibCompletedCallback taskDownLoadLibCompletedCallback = (bool result) =>
            {
                if (!result) 
                    lib_isok=false;
                lib_downloaded_count++;
                Console.WriteLine(lib_downloaded_count + "/" + lib_total);//多线程打印问题
            };
            d.DownloadLib(allGamePath.LibrariesDirectory + "/", allGamePath.NativeDirectory + "/", taskDownLoadLibCompletedCallback);
            if (!lib_isok)
            {
                Console.WriteLine("Download Libraries error!");
            }
            else
            {
                Console.WriteLine("Download Libraries successful !");
            }

            Console.WriteLine("启动游戏");

            Game game = new Game(game_json, "3068m", allGamePath.NativeDirectory, allGamePath.LibrariesDirectory + "/", allGamePath.GameJarFile, "SMCL", "1.0.0", allGamePath.Log4jConfigurationFile, Player_Name, allGamePath.MainGameDirectory, allGamePath.AssetsDirectory, UUID, Access_Token);
            string test = game.GetArguments();
            game.Start(Java_Path, "-Dsun.stdout.encoding=GB18030 -Dsun.stderr.encoding=GB18030 ");
        }
        //static void Main(string[] args)
        //{ 
        //    AllGamePath allGamePath = GetDefalutAllGamePath("C:\\Users\\GoodBoyboy\\Desktop\\test", "1.21");

        //    Download.ExtractLog4j2File(allGamePath.Log4jConfigurationFile);
        //}

    }
}
