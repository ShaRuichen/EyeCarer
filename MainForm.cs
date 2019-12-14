using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using static EyeCarer.Properties.Resources;

namespace EyeCarer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        
        private void MainForm_Load(object sender, EventArgs e)
        {
            #region 主窗口的Rectangle
            var screenRectangle = SystemInformation.WorkingArea;
            var screenWidth = screenRectangle.Width;
            var screenHeight = screenRectangle.Height;
            Width = screenWidth / 4;
            Height = screenHeight / 4;
            Left = screenWidth - Width;
            Top = screenHeight - Height;
            WindowState = FormWindowState.Minimized;
            #endregion

            var clientWidth = ClientSize.Width;
            var clientHeight = ClientSize.Height;

            #region 已用时间标签
            var usedTimeLabel = new Label()
            {
                AutoSize = true,
                Font = new Font("宋体", 0.0003F * (Width * Height)),
                Text = "已用时间：",
                Location = new Point(0, clientHeight / 8),
            };
            Controls.Add(usedTimeLabel);
            #endregion
            #region 已用时间数字
            var usedSecondsLabel = new Label()
            {
                AutoSize = true,
                Font = usedTimeLabel.Font,
                Text = "0",
                Location = usedTimeLabel.Location + new Size(usedTimeLabel.Width, 0),
            };
            Controls.Add(usedSecondsLabel);
            #endregion
            #region 显示系统是否被检测为正在使用的标签
            var isUsedLabel = new Label()
            {
                AutoSize = true,
                Font = new Font("宋体", 0.0002F * (Width * Height)),
                Location = usedTimeLabel.Location + new Size(0, usedTimeLabel.Height + clientHeight / 8),
            };
            Controls.Add(isUsedLabel);
            #endregion
            #region 系统的使用状态下拉栏
            var usageComboBox = new ComboBox()
            {
                Font = isUsedLabel.Font,
                Location = usedSecondsLabel.Location + new Size(0, usedSecondsLabel.Height + clientHeight / 8),
                DropDownStyle = ComboBoxStyle.DropDownList,
            };
            var usageMenuItems = new ToolStripMenuItem[]
            {
                new ToolStripMenuItem("自动判断"),
                new ToolStripMenuItem("使用中"),
                new ToolStripMenuItem("未使用"),
            };
            void UsageComboBox_SelectedIndexChanged(object sender1, EventArgs e1)
            {
                var oldMenu = usageMenuItems[(int)UsedTime.Usage];
                oldMenu.Checked = false;
                oldMenu.Enabled = true;
                UsedTime.Usage = (Usage)usageComboBox.SelectedIndex;
                var newMenu = usageMenuItems[usageComboBox.SelectedIndex];
                newMenu.Checked = true;
                newMenu.Enabled = false;
            }
            usageComboBox.SelectedIndexChanged += UsageComboBox_SelectedIndexChanged;
            usageComboBox.Items.AddRange(new object[] { "自动判断", "使用中", "未使用" });
            usageComboBox.SelectedIndex = 0;
            Controls.Add(usageComboBox);
            #endregion
            #region 开机自启选项
            var powerBootCheckBox = new CheckBox()
            {
                AutoSize = true,
                Font = isUsedLabel.Font,
                Text = "开机自启",
                Location = isUsedLabel.Location + new Size(0, isUsedLabel.Height * 2),
            };
            var registryKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            powerBootCheckBox.Checked = registryKey.GetValue("SRCEyeCarer") != null;
            registryKey.Close();
            var powerBootMenuItem = new ToolStripMenuItem("开机自启");
            void PowerBootCheckBox_Click(object sender1, EventArgs e1)
            {
                powerBootMenuItem.Checked = powerBootCheckBox.Checked;
                var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                string args = $"-powerBoot {powerBootCheckBox.Checked}";
                if (principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    Program.SetPowerBoot(powerBootCheckBox.Checked);
                }
                else
                {
                    var startInfo = new ProcessStartInfo(Application.ExecutablePath, args)
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
                        if (errCode == 1223)
                        {
                            powerBootCheckBox.Checked = !powerBootCheckBox.Checked;
                            powerBootMenuItem.Checked = powerBootCheckBox.Checked;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            powerBootCheckBox.Click += PowerBootCheckBox_Click;
            Controls.Add(powerBootCheckBox);
            #endregion
            #region 设置选项
            var setupButton = new Button()
            {
                Location = usageComboBox.Location + new Size(0, usageComboBox.Height + clientHeight / 8),
                Size = usageComboBox.Size,
                Text = "设置",
                Font = usageComboBox.Font,
            };
            void Setup_Click(object sender1, EventArgs e1)
            {
                var setupForm = new SetupForm();
                setupForm.ShowDialog();
            }
            setupButton.Click += Setup_Click;
            Controls.Add(setupButton);
            #endregion
            #region 任务栏图标
            notifyIcon = new NotifyIcon()
            {
                Icon = BlueEye,
                Visible = false,
                Text = "护眼宝",
            };
            void NotifyIcon_DoubleClick(object sender1, EventArgs e1)
            {
                notifyIcon.Visible = false;
                WindowState = FormWindowState.Normal;
                ShowInTaskbar = true;
            }
            #region 右键菜单
            notifyIcon.ContextMenuStrip = new ContextMenuStrip
            {
                Font = isUsedLabel.Font
            };

            powerBootMenuItem.Checked = powerBootCheckBox.Checked;
            void PowerBootMenuItem_Click(object sender1, EventArgs e1)
            {
                powerBootCheckBox.Checked = !powerBootCheckBox.Checked;
                PowerBootCheckBox_Click(null, new EventArgs());
            }
            powerBootMenuItem.Click += PowerBootMenuItem_Click;

            void UsageMenuItem_Click(object sender1, EventArgs e1)
            {
                var clickingMenuItem = (ToolStripMenuItem)sender1;
                int i;
                for (i = 0; i < 2; i++)
                {
                    if (usageMenuItems[i] == clickingMenuItem)
                    {
                        break;
                    }
                }
                usageComboBox.SelectedIndex = i;
            }

            usageMenuItems[0].Click += UsageMenuItem_Click;
            usageMenuItems[1].Click += UsageMenuItem_Click;
            usageMenuItems[2].Click += UsageMenuItem_Click;
            usageMenuItems[0].Checked = true;
            usageMenuItems[0].Enabled = false;

            var exitMenuItem = new ToolStripMenuItem("退出");
            void ExitMenuItem_Click(object sender1, EventArgs e1)
            {
                Close();
            }
            exitMenuItem.Click += ExitMenuItem_Click;

            var setupMenuItem = new ToolStripMenuItem("设置");
            setupMenuItem.Click += Setup_Click;

            var timeMenuItem = new ToolStripMenuItem();

            notifyIcon.ContextMenuStrip.Items.Add(timeMenuItem);
            notifyIcon.ContextMenuStrip.Items.Add(powerBootMenuItem);
            notifyIcon.ContextMenuStrip.Items.AddRange(usageMenuItems);
            notifyIcon.ContextMenuStrip.Items.Add(setupMenuItem);
            notifyIcon.ContextMenuStrip.Items.Add(exitMenuItem);
            #endregion
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
            void NotifyIcon_Click(object sender1, EventArgs e1)
            {
                var mouse = (MouseEventArgs)e1;
                if (mouse.Button == MouseButtons.Left)
                {
                    NotifyIcon_DoubleClick(sender1, e1);
                }
            }
            notifyIcon.Click += NotifyIcon_Click;
            #endregion
            #region 接收事件
            void UsedTime_ChangeIconColor(UsedTime.IconColor iconColor)
            {
                Icon icon;
                switch (iconColor)
                {
                    case UsedTime.IconColor.Blue: icon = BlueEye; break;
                    case UsedTime.IconColor.Red: icon = RedEye; break;
                    case UsedTime.IconColor.Yello: icon = YelloEye; break;
                    default: throw new NotImplementedException();
                }
                Icon = icon;
                notifyIcon.Icon = icon;
            }
            UsedTime.ChangeIconColor += UsedTime_ChangeIconColor;
            void UsedTime_Warning(int time)
            {
                var warningForm = new Form()
                {
                    FormBorderStyle = FormBorderStyle.None,
                    Height = screenHeight / 4,
                    Width = screenWidth / 2,
                    StartPosition = FormStartPosition.CenterScreen,
                    TopMost = true,
                    ShowIcon = false,
                };
                if (time < UsedTime.TO_RED_TIME)
                {
                    warningForm.BackColor = Color.Yellow;
                }
                else
                {
                    warningForm.BackColor = Color.Red;
                }
                var label = new Label()
                {
                    AutoSize = true,
                    Font = new Font("宋体", 0.00025F * (warningForm.Width * warningForm.Height)),
                    Text = $"你已经使用{time / 60}分钟电脑了！",
                };
                label.Location = new Point(warningForm.Width / 8, warningForm.Height / 4);
                warningForm.Controls.Add(label);
                var iKonwButton = new Button()
                {
                    Font = label.Font,
                    Text = "知道了",
                    AutoSize = true,
                };
                iKonwButton.Location = new Point(warningForm.Width * 2 / 5, warningForm.Height * 2 / 3);
                void IKonwButton_Click(object sender1, EventArgs e1)
                {
                    warningForm.Close();
                    warningForm.Dispose();
                }
                iKonwButton.Click += IKonwButton_Click;
                warningForm.Controls.Add(iKonwButton);
                warningForm.Show();
            }
            UsedTime.Warning += UsedTime_Warning;
            void UsedTime_Sleep()
            {
                Application.SetSuspendState(PowerState.Hibernate, false, false);
            }
            UsedTime.Sleep += UsedTime_Sleep;
            #endregion
            #region 计时器
            void Timer_Tick(object sender1, EventArgs e1)
            {
                int usedSeconds = (int)(UsedTime.CountUsedTime(out bool isUsed) * SystemUsage.INTERVAL_SECONDS);
                string usage = isUsed ? "正在使用" : "未使用";
                if (WindowState == FormWindowState.Minimized)
                {
                    notifyIcon.Text = $"护眼宝\n{usedSeconds}\n{usage}";
                    timeMenuItem.Text = $"已使用：{usedSeconds}";
                }
                else if (WindowState == FormWindowState.Normal)
                {
                    usedSecondsLabel.Text = usedSeconds.ToString();
                    isUsedLabel.Text = usage;
                }
            }
            var timer = new Timer()
            {
                Interval = SystemUsage.INTERVAL,
                Enabled = true,
            };
            timer.Tick += Timer_Tick;
            #endregion
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                ShowInTaskbar = false;
                notifyIcon.Visible = true;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            notifyIcon.Dispose();
        }

        private NotifyIcon notifyIcon;
    }
}
