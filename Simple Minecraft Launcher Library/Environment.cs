using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Simple_Minecraft_Launcher_Library
{
    public class Environment
    {
        /// <summary>
        /// 从用户环境变量读取JAVA_HOME
        /// </summary>
        /// <returns></returns>
        public static string? GetJavaHomeFromUser()
        {
            return System.Environment.GetEnvironmentVariable("JAVA_HOME", EnvironmentVariableTarget.User) ?? null;
        }
        /// <summary>
        /// 从进程环境变量读取JAVA_HOME
        /// </summary>
        /// <returns></returns>
        public static string? GetJavaHomeFromProcess()
        {
            return System.Environment.GetEnvironmentVariable("JAVA_HOME", EnvironmentVariableTarget.Process) ?? null;
        }
        /// <summary>
        /// 从系统环境变量读取JAVA_HOME
        /// </summary>
        /// <returns></returns>
        public static string? GetJavaHomeFromMachine()
        {
            return System.Environment.GetEnvironmentVariable("JAVA_HOME", EnvironmentVariableTarget.Machine) ?? null;
        }
        /// <summary>
        /// 获取JAVA_HOME
        /// </summary>
        /// <returns></returns>
        public static string? GetJavaHome()
        {
            if (GetJavaHomeFromProcess() != null)
            {
                return GetJavaHomeFromProcess();
            }
            else if (GetJavaHomeFromUser() != null)
            {
                return GetJavaHomeFromUser();
            }
            else if (GetJavaHomeFromMachine() != null)
            {
                return GetJavaHomeFromMachine();
            }
            return null;
        }
        /// <summary>
        /// 获取JavaPath
        /// </summary>
        /// <returns></returns>
        public static string? GetJavaPath()
        {
            string? Java_Home = GetJavaHome();
            if (Java_Home == null)
            {
                return null;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Java_Home + "\\bin\\java.exe";
            }
            else
            {
                return Java_Home + "/bin/java";
            }
        }
        /// <summary>
        /// 获取默认路径
        /// </summary>
        /// <param name="game_main_directory">游戏主文件（定位到.minecraft）</param>
        /// <param name="version_name">版本名称</param>
        /// <returns></returns>
        public static AllGamePath GetDefalutAllGamePath(string game_main_directory, string version_name)
        {
            AllGamePath gamePath = new AllGamePath();
            gamePath.MainGameDirectory = game_main_directory;
            gamePath.GameJarFileDirectory = gamePath.MainGameDirectory + "/versions/" + version_name;
            gamePath.NativeDirectory = gamePath.GameJarFileDirectory + "/natives";
            gamePath.LibrariesDirectory = gamePath.MainGameDirectory + "/libraries";
            gamePath.Log4jConfigurationFile = gamePath.GameJarFileDirectory + "/log4j2.xml";
            gamePath.AssetsDirectory = gamePath.MainGameDirectory + "/assets";
            gamePath.GameJarFile = gamePath.GameJarFileDirectory + "/" + version_name + ".jar";
            return gamePath;
        }

        public class AllGamePath
        {
            public string MainGameDirectory { get; set; }

            public string NativeDirectory { get; set; }

            public string LibrariesDirectory { get; set; }

            public string Log4jConfigurationFile { get; set; }

            public string AssetsDirectory { get; set; }

            public string GameJarFileDirectory { get; set; }

            public string GameJarFile { get; set; }

            public AllGamePath()
            {
                MainGameDirectory = "";
                NativeDirectory = "";
                LibrariesDirectory = "";
                Log4jConfigurationFile = "";
                AssetsDirectory = "";
                GameJarFileDirectory = "";
                GameJarFile = "";
            }
        }
    }
}
