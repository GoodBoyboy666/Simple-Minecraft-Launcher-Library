using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Simple_Minecraft_Launcher_Library
{
    /// <summary>
    /// 游戏相关类
    /// </summary>
    /// <param name="gamejson">版本信息的清单</param>
    /// <param name="xmx">最小内存</param>
    /// <param name="natives_directory">natives库文件夹路径</param>
    /// <param name="libraries_directory">普通库文件夹路径（末尾包含/）</param>
    /// <param name="game_jar_file">游戏主Jar文件</param>
    /// <param name="launcher_name">启动器名称</param>
    /// <param name="launcher_version">启动器版本</param>
    /// <param name="log4jconfigurationFile">log4jconfigurationFile文件路径</param>
    /// <param name="auth_player_name">玩家名称</param>
    /// <param name="game_directory">游戏主文件夹</param>
    /// <param name="assets_root">assets文件夹</param>
    /// <param name="auth_uuid">玩家UUID</param>
    /// <param name="auth_access_token">Access_Token</param>
    /// <param name="versionType">启动器+版本号</param>
    /// <param name="user_type">认证方式</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <param name="clientid">客户端ID</param>
    /// <param name="auth_xuid">XUID</param>
    public class Game(string gamejson, string xmx, string natives_directory, string libraries_directory, string game_jar_file, string launcher_name, string launcher_version, string log4jconfigurationFile, string auth_player_name, string game_directory, string assets_root, string auth_uuid, string auth_access_token, string versionType = "", string user_type = "msa", string width = "854", string height = "480", string clientid = "${clientid}", string auth_xuid = "${auth_xuid}")
    {
        JObject? game_JObject = Json.DeserializeObject<JObject>(gamejson) ?? throw new Exception("Can not translate Game Json to JObject !");

        /// <summary>
        /// 拼接游戏参数
        /// </summary>
        /// <returns></returns>
        internal string GetGameArguments()
        {
            return String.Format("--username \"{0}\" --version {1} --gameDir \"{2}\" --assetsDir \"{3}\" --assetIndex {4} --uuid {5} --accessToken \"{6}\" --clientId {7} --xuid {8} --userType {9} --versionType \"{10}\" --width {11} --height {12}", auth_player_name, game_JObject?["id"]?.Value<string>() ?? "Null", game_directory, assets_root, game_JObject?["assetIndex"]?["id"]?.Value<string>(), auth_uuid, auth_access_token, clientid, auth_xuid, user_type, versionType != "" ? versionType : launcher_name + " " + launcher_version, width, height) + " --userProperties {}";
        }

        /// <summary>
        /// 拼接JVM参数
        /// </summary>
        /// <param name="classpath">ClassPath</param>
        /// <param name="custom">自定义参数（末尾需留一个空格）</param>
        /// <returns></returns>
        internal string GetJVMArguments(string classpath, string custom = "")
        {
            string jvm_Arguments = String.Format("-Xmx{0} -XX:+UseG1GC -XX:-UseAdaptiveSizePolicy -XX:-OmitStackTraceInFastThrow {1}", xmx, custom == "" ? "" : custom);
            JArray? jvm = game_JObject?["arguments"]?["jvm"]?.Value<JArray>();
            jvm_Arguments +="-Dminecraft.client.jar="+ "\"" + game_jar_file + "\"" + " ";
            jvm_Arguments += "-Dlog4j2.formatMsgNoLookups=true" + " ";
            jvm_Arguments += game_JObject?["logging"]?["client"]?["argument"]?.Value<string>()  + " ";
            if (jvm != null)
            {
                for (int i = 0; i < jvm?.Count; i++)
                {
                    var jvm_obj = jvm[i];
                    if (jvm_obj is JObject)
                    {

                        if (jvm_obj?["rules"]?[0]?["os"]?["name"]?.Value<string>() == "windows" && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            if (jvm_obj?["value"] is JObject)
                                jvm_Arguments += "\""+jvm_obj?["value"]?.Value<string>()+ "\"" + " ";
                            else if (jvm_obj?["value"] is JArray)
                            {
                                foreach (var value_item in jvm_obj?["value"]??new JArray())
                                {
                                    jvm_Arguments += "\"" + value_item?.Value<string>() + "\"" + " ";
                                }
                            }
                            continue;
                        }

                        if (jvm_obj?["rules"]?[0]?["os"]?["name"]?.Value<string>() == "osx" && RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                        {
                            if (jvm_obj?["value"] is JObject)
                                jvm_Arguments += "\""+jvm_obj?["value"]?.Value<string>()+ "\"" + " ";
                            else if (jvm_obj?["value"] is JArray)
                            {
                                foreach (var value_item in jvm_obj?["value"] ?? new JArray())
                                {
                                    jvm_Arguments += "\"" + value_item?.Value<string>() + "\""+  " ";
                                }
                            }
                            continue;
                        }
                    }
                    else
                    {
                        jvm_Arguments += "\"" + jvm_obj?.ToString()+ "\"" + " ";
                    }
                }
            }
            else
            {
                jvm_Arguments += "-Djava.library.path=\"${natives_directory}\"" + " ";
                jvm_Arguments += "-Djna.tmpdir=\"${natives_directory}\"" + " ";
                jvm_Arguments += "-Dorg.lwjgl.system.SharedLibraryExtractPath=\"${natives_directory}\"" + " ";
                jvm_Arguments += "-Dio.netty.native.workdir=\"${natives_directory}\"" + " ";
                jvm_Arguments += "-Dminecraft.launcher.brand=\"${launcher_name}\"" + " ";
                jvm_Arguments += "-Dminecraft.launcher.version=\"${launcher_version}\"" + " ";
                jvm_Arguments += "-cp" + " ";
                jvm_Arguments += "\"${classpath}\"" + " ";
            }
            jvm_Arguments += game_JObject?["mainClass"]?.Value<string>() + " ";
            jvm_Arguments = jvm_Arguments.Replace("${natives_directory}", natives_directory);
            jvm_Arguments = jvm_Arguments.Replace("${launcher_name}", launcher_name);
            jvm_Arguments = jvm_Arguments.Replace("${launcher_version}",  launcher_version);
            jvm_Arguments = jvm_Arguments.Replace("${path}", log4jconfigurationFile);
            jvm_Arguments = jvm_Arguments.Replace("${classpath}", classpath);
            return jvm_Arguments;
        }

        /// <summary>
        /// 拼接完整参数
        /// </summary>
        /// <param name="custom">自定义参数</param>
        /// <returns></returns>
        public string GetArguments(string custom = "")
        {
            string cp = "";
            Download down = new Download(gamejson);
            List<Library> libs = down.GetLib();
            foreach (Library lib in libs)
            {
                if (lib.downloads?.artifact?.path != null)
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        if (!cp.Contains(lib.downloads.artifact.path))
                            cp += libraries_directory + lib.downloads?.artifact?.path + ";";
                        else
                        if (!cp.Contains(lib.downloads.artifact.path))
                            cp += libraries_directory + lib.downloads?.artifact?.path + ":";
                }
            }
            cp += game_jar_file;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return (GetJVMArguments(cp, custom) + GetGameArguments()).Replace("/", "\\");
            else
                return GetJVMArguments(cp, custom) + GetGameArguments();
        }

        /// <summary>
        /// 启动游戏
        /// </summary>
        /// <param name="JavaPath">Java 路径</param>
        /// <param name="custom">自定义参数</param>
        /// <exception cref="Exception"></exception>
        public void Start(string JavaPath = "", string custom = "")
        {
            string arguments = GetArguments(custom);
            if (JavaPath == "")
            {
                JavaPath = Environment.GetJavaPath() ?? throw new Exception("Please set Java Path !");
            }

            //string cmd = "Set-Location -Path " + "'" + game_directory + "'" + "\r\n & " + "'" + JavaPath + "'" + " " + arguments;

            Process process = new Process();
            process.StartInfo.FileName = JavaPath;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = false;
            process.Start();
            process.WaitForExit();
        }
    }

}
