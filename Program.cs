using System;
using System.Windows.Forms;
using Microsoft.Win32;

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
            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "-powerBoot":
                        SetPowerBoot(args[1] == "True");
                        Environment.Exit(0);
                        break;
                    default: throw new NotImplementedException();
                }
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
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
    }
}