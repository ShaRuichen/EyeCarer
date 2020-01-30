using System;

using Microsoft.Win32;

using EyeCarer.Properties;


namespace EyeCarer
{

    /// <summary>
    /// 当前是否正被使用的枚举（由程序判断、被使用、不被使用）
    /// </summary>
    internal enum Usage { Judge, Used, Unused }

    /// <summary>
    /// 获取已使用时间的类
    /// </summary>
    internal static class UsedTime
    {

        public static int ToYelloTime { get; set; } = Settings.Default.ToYelloTime;
        public static int ToRedTime { get; set; } = Settings.Default.ToRedTime;
        public static int ToSleepTime { get; set; } = Settings.Default.ToSleepTime;
        public static double RestEfficiency { get; set; } = Settings.Default.RestEfficiency;
        private const int RedWarningInterval = 300;
        private const int JudgeRestTime = 90;

        /// <summary>
        /// 图标颜色的枚举
        /// </summary>
        public enum IconColor { Blue, Yello, Red }

        /// <summary>
        /// 获取已使用的总秒数（每秒必须调用一次）
        /// </summary>
        /// <param name="isUsed">返回当前程序是否正在被使用</param>
        /// <returns>总秒数</returns>
        public static int CountUsedTime(out bool isUsed)
        {
            int result;

            if (Usage == Usage.Used)
            {
                isUsed = true;
                result = ContinueUse();
            }
            else if (Usage == Usage.Unused)
            {
                isUsed = false;
                result = ContinueRest();
            }
            else if (Usage == Usage.Judge)
            {
                #region 系统使用中
                if (SystemUsage.PlayingVideo || SystemUsage.UserInput)
                {
                    isUsed = true;
                    #region 之前被认为在休息
                    if (IsRest)
                    {
                        IsRest = false;
                        result = ContinueUse();
                    }
                    #endregion
                    #region 之前被认为在使用电脑
                    else
                    {
                        #region 曾有过中断操作
                        if (gapTime > 0)
                        {
                            int addSeconds = gapTime + 1;
                            gapTime = 0;
                            usedTime += addSeconds;
                            CountCacheUsedTime();
                            result = cacheUsedTime;
                        }
                        #endregion
                        #region 不曾有中断操作
                        else
                        {
                            result = ContinueUse();
                        }
                        #endregion
                    }
                    #endregion
                }
                #endregion
                #region 系统空闲中
                else
                {
                    isUsed = false;
                    #region 判定为休息眼睛中
                    if (IsRest)
                    {
                        #region 完全休息好时
                        if (usedTime == 0)
                        {
                            result = 0;
                        }
                        #endregion
                        #region 尚未完全休息好时
                        else
                        {
                            restTime++;
                            JudgeCompleteRest();
                            result = cacheUsedTime;
                        }
                        #endregion
                    }
                    #endregion
                    #region 判定为使用电脑中
                    else
                    {
                        gapTime++;
                        #region 判定现在为休息状态了
                        if (gapTime == JudgeRestTime)
                        {
                            IsRest = true;
                            restTime += JudgeRestTime;
                            gapTime = 0;
                            JudgeCompleteRest();
                            result = cacheUsedTime;
                        }
                        #endregion
                        #region 判定现在仍在使用电脑
                        else
                        {
                            result = ++cacheUsedTime;
                        }
                        #endregion
                    }
                    #endregion
                }
                #endregion
            }
            else
            {
                throw new NotImplementedException();
            }

            #region 分发事件
            int time = result - gapTime;
            if (time >= ToSleepTime)
            {
                mustSleepTime = Environment.TickCount;
                Sleep();
                return result;
            }
            if (time < ToYelloTime && iconColor != IconColor.Blue)
            {
                iconColor = IconColor.Blue;
                ChangeIconColor(IconColor.Blue);
            }
            else if (time >= ToYelloTime && time < ToRedTime && iconColor != IconColor.Yello)
            {
                iconColor = IconColor.Yello;
                ChangeIconColor(IconColor.Yello);
            }
            else if (time >= ToRedTime && iconColor != IconColor.Red)
            {
                iconColor = IconColor.Red;
                ChangeIconColor(IconColor.Red);
            }
            if (isUsed)
            {
                if (time >= ToYelloTime && time < ToRedTime && warningTimes == 0)
                {
                    warningTimes++;
                    Warning(time);
                }
                else if (time >= ToRedTime)
                {
                    int shouldTimes = (time - ToRedTime) / RedWarningInterval + 2;
                    if (shouldTimes > warningTimes)
                    {
                        warningTimes++;
                        Warning(time);
                    }
                }
            }
            else
            {
                if (time < ToYelloTime && warningTimes > 0)
                {
                    warningTimes = 0;
                }
                else if (time >= ToYelloTime && time < ToRedTime && warningTimes > 1)
                {
                    warningTimes = 1;
                }
                else if (time >= ToRedTime)
                {
                    int shouldTimes = (time - ToRedTime) / RedWarningInterval + 2;
                    if (shouldTimes < warningTimes)
                    {
                        warningTimes = shouldTimes;
                    }
                }
            }
            #endregion

            return result;
        }

        /// <summary>
        /// 获取或设置系统的使用情况
        /// </summary>
        public static Usage Usage
        {
            get => usage;
            set
            {
                if (value == Usage.Unused)
                {
                    IsRest = true;
                }
                else if (value == Usage.Used)
                {
                    IsRest = false;
                }
                usage = value;
            }
        }

        public delegate void WarningHandler(int time);
        /// <summary>
        /// 需要弹出警告窗口时
        /// </summary>
        public static event WarningHandler Warning;

        public delegate void ChangeIconColorHandler(IconColor iconColor);
        /// <summary>
        /// 需要将图标变色时
        /// </summary>
        public static event ChangeIconColorHandler ChangeIconColor;

        public delegate void SleepHandler();
        /// <summary>
        /// 需要睡眠时
        /// </summary>
        public static event SleepHandler Sleep;

        static UsedTime()
        {
            #region 应对休眠
            void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
            {
                var mode = e.Mode;
                if (mode == PowerModes.Suspend)
                {
                    sleepTime = Environment.TickCount;
                    gapTime = 0;
                }
                else if (mode == PowerModes.Resume)
                {
                    if (mustSleepTime > 0)
                    {
                        if ((Environment.TickCount - mustSleepTime) / 1000 * RestEfficiency >= ToSleepTime)
                        {
                            mustSleepTime = 0;
                            usedTime = 0;
                            gapTime = 0;
                            restTime = 0;
                            cacheUsedTime = 0;
                            sleepTime = 0;
                            warningTimes = 0;
                            iconColor = IconColor.Blue;
                            ChangeIconColor(IconColor.Blue);
                        }
                        else
                        {
                            Sleep();
                        }
                    }
                    else
                    {
                        int wakeTime = Environment.TickCount;
                        int sleepMiliseconds = wakeTime - sleepTime;
                        sleepTime = 0;
                        int hasSleptTime = sleepMiliseconds / SystemUsage.Interval;
                        restTime += hasSleptTime;
                        JudgeCompleteRest();
                        if (cacheUsedTime < ToYelloTime)
                        {
                            warningTimes = 0;
                        }
                        else if (cacheUsedTime < ToRedTime)
                        {
                            warningTimes = 1;
                        }
                        else
                        {
                            warningTimes = 2 + (cacheUsedTime - ToRedTime) / RedWarningInterval;
                        }
                    }
                }
            }
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            #endregion
        }

        /// <summary>
        /// 执行系统被使用时的操作并返回总时间
        /// </summary>
        /// <return>总时间</return>
        private static int ContinueUse()
        {
            usedTime++;
            return ++cacheUsedTime;
        }

        /// <summary>
        /// 执行休息时的操作并返回总时间
        /// </summary>
        /// <return>总时间</return>
        private static int ContinueRest()
        {
            #region 完全休息好时
            if (cacheUsedTime == 0)
            {
                return 0;
            }
            #endregion
            #region 尚未完全休息好时
            else
            {
                restTime++;
                JudgeCompleteRest();
                return cacheUsedTime;
            }
            #endregion
        }

        /// <summary>
        /// 计算总的使用秒数并将其设置为cachaUsedSeconds的值
        /// </summary>
        private static void CountCacheUsedTime()
        {
            cacheUsedTime = (int)(usedTime - restTime * RestEfficiency);
        }

        /// <summary>
        /// 判断是否完全休息好了并将cacheUsedTime设为总的使用秒数
        /// <para>若休息好了还将把usedTime和restTime的值清零</para>
        /// </summary>
        /// <returns>总的使用秒数</returns>
        private static void JudgeCompleteRest()
        {
            #region 若现在完全休息好了
            if (restTime * RestEfficiency >= usedTime)
            {
                restTime = 0;
                usedTime = 0;
                cacheUsedTime = 0;
            }
            #endregion
            #region 现在还没完全休息好
            else
            {
                CountCacheUsedTime();
            }
            #endregion
        }

        private static bool IsRest = false;
        private static int usedTime = 0;
        private static int gapTime = 0;
        private static int restTime = 0;
        private static int cacheUsedTime = 0;
        private static int sleepTime = 0;
        private static int warningTimes = 0;
        private static Usage usage = Usage.Judge;
        private static IconColor iconColor = IconColor.Blue;
        private static int mustSleepTime = 0;
    }
}