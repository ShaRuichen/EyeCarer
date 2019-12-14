using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace EyeCarer
{

    /// <summary>
    /// 获取系统使用情况
    /// </summary>
    public static class SystemUsage
    {

        /// <summary>
        /// 支持间隔多少毫秒获取一次操作
        /// </summary>
        public const int INTERVAL = 1000;

        /// <summary>
        /// 支持间隔多少秒获取一次操作
        /// </summary>
        public const int INTERVAL_SECONDS = 1;

        /// <summary>
        /// 获取系统现在是否有多媒体应用程序正在播放
        /// </summary>
        public static bool PlayingVideo
        {
            get
            {
                int ret = CallNtPowerInformation(16, IntPtr.Zero, 0, out int state, 4);
                if (ret != 0)
                {
                    throw new Win32Exception(ret);
                }
                return (state & 2) == 2;
            }
        }

        /// <summary>
        /// 获取INTERVAL间隔内是否有用户输入（键盘点击或鼠标移动或点击）
        /// </summary>
        public static bool UserInput
        {
            get
            {
                var lastInputInfo = new LastInptInfo();
                lastInputInfo.cbSize = Marshal.SizeOf(lastInputInfo);
                if (!GetLastInputInfo(ref lastInputInfo))
                {
                    throw new Win32Exception();
                }
                int freeMiliseconds = Environment.TickCount - lastInputInfo.dwTime;
                return freeMiliseconds < INTERVAL;
            }
        }

        private struct LastInptInfo
        {
            public int cbSize;
            public int dwTime;
        }

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LastInptInfo lastInptInfo);

        [DllImport("powrprof.dll")]
        private static extern int CallNtPowerInformation(int level, IntPtr inpbuf, int inpbuflen, out int state, int outbuflen);
    }
}
