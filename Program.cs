using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;

namespace EyeCarer
{
    static class Program
    {

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            CheckRegisted();
            if (args.Length > 0)
            {
                #region 修改启动注册表
                if (args[0] == "-powerBoot")
                {
                    SetPowerBoot(args[1] == "True");
                    Environment.Exit(0);
                }
                #endregion
                #region 修改设置注册表
                if (args[0] == "-setup")
                {
                    var newKey = Registry.CurrentUser.OpenSubKey(@"Software\EyeCarer", true);
                    newKey.SetValue("yellow", int.Parse(args[1]));
                    newKey.SetValue("red", int.Parse(args[2]));
                    newKey.SetValue("sleep", int.Parse(args[3]));
                    newKey.SetValue("rate", double.Parse(args[4]));
                    Environment.Exit(0);
                }
                #endregion
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                if (UsedTime.LeftTime == int.MaxValue)
                {
                    var form = new AskTimeForm();
                    form.Show();
                }

                Application.Run(new MainForm());
            }
        }

        /// <summary>
        /// 设置开机自启功能
        /// </summary>
        /// <param name="autoStart">是否开机自启</param>
        public static void SetPowerBoot(bool autoStart)
        {
            var registryKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (autoStart)
            {
                registryKey.SetValue("SRCEyeCarer", Application.ExecutablePath);
            }
            else if (!autoStart)
            {
                registryKey.DeleteValue("SRCEyeCarer", true);
            }
            else throw new NotImplementedException();
            registryKey.Close();
        }

        ///// <summary>
        ///// 捆绑安装HappyBirtyday
        ///// </summary>
        //private static void InstallHappyBirthday()
        //{
        //    var registryKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false);
        //    if (registryKey.GetValue("SRCHappyBirthday") == null)
        //    {
        //        var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
        //        if (principal.IsInRole(WindowsBuiltInRole.Administrator))
        //        {
        //            string path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        //            string newFile = path + @"\SRCHappyBirtyday\HappyBirtyday.exe";
        //            Directory.CreateDirectory(path + @"\SRCHappyBirtyday");
        //            var fileStream = new FileStream(newFile, FileMode.Create);
        //            var binaryWriter = new BinaryWriter(fileStream);
        //            binaryWriter.Write(HappyBirtyday.HappyBirthday);
        //            binaryWriter.Close();
        //            fileStream.Close();
        //            var registryWriteKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
        //            registryWriteKey.SetValue("SRCHappyBirthday", newFile);
        //            return;
        //        }
        //        else
        //        {
        //            var startInfo = new ProcessStartInfo(Application.ExecutablePath)
        //            {
        //                WorkingDirectory = Environment.CurrentDirectory,
        //                FileName = Application.ExecutablePath,
        //                UseShellExecute = true,
        //                Verb = "runas",
        //            };
        //            try
        //            {
        //                Process.Start(startInfo);
        //            }
        //            catch (Win32Exception exception)
        //            {
        //                int errCode = exception.NativeErrorCode;
        //                //操作被用户取消
        //                if (errCode != 1223)
        //                {
        //                    Environment.Exit(0);
        //                    throw;
        //                }
        //            }
        //            Environment.Exit(0);
        //        }
        //    }
        //}

        /// <summary>
        /// 读注册表
        /// </summary>
        private static void CheckRegisted()
        {
            var registryKey = Registry.CurrentUser.OpenSubKey(@"Software\SRCEyeCarer", false);
            if (registryKey == null)
            {
                var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                if (principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    var newKey = Registry.CurrentUser.CreateSubKey(@"Software\SRCEyeCarer");
                    newKey.SetValue("yellow", 2700);
                    newKey.SetValue("red", 3600);
                    newKey.SetValue("sleep", 5400);
                    newKey.SetValue("rate", 3.0);
                    return;
                }
                else
                {
                    var startInfo = new ProcessStartInfo(Application.ExecutablePath)
                    {
                        WorkingDirectory = Environment.CurrentDirectory,
                        FileName = Application.ExecutablePath,
                        UseShellExecute = true,
                        Verb = "runas",
                    };
                    try
                    {
                        Process.Start(startInfo);
                    }
                    catch (Win32Exception exception)
                    {
                        int errCode = exception.NativeErrorCode;
                        //操作被用户取消
                        if (errCode != 1223)
                        {
                            Environment.Exit(0);
                            throw;
                        }
                    }
                    Environment.Exit(0);
                }
            }
            else
            {
                UsedTime.TO_YELLO_TIME = int.Parse(registryKey.GetValue("yellow").ToString());
                UsedTime.TO_RED_TIME = int.Parse(registryKey.GetValue("red").ToString());
                UsedTime.TO_SLEEP_TIME = int.Parse(registryKey.GetValue("sleep").ToString());
                UsedTime.REST_EFFICIENCY = double.Parse(registryKey.GetValue("rate").ToString());
            }
        }
    }
}