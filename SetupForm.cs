﻿using System;
using System.Drawing;
using System.Windows.Forms;

using EyeCarer.Properties;


namespace EyeCarer
{
    public partial class SetupForm : Form
    {
        public SetupForm()
        {
            InitializeComponent();

            var screenSize = Screen.PrimaryScreen.Bounds.Size;
            Size = new Size(screenSize.Width / 4, screenSize.Height / 3);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;

            void TextBox_TextChanged(object sender, EventArgs e)
            {
                var textBox = (TextBox)sender;
                if (textBox.Text.Length == 0 || textBox.Text[textBox.Text.Length - 1] != 's')
                {
                    string str = "";
                    foreach (char ch in textBox.Text)
                    {
                        if (ch != 's')
                        {
                            str += ch;
                        }
                    }
                    textBox.Text = str + 's';
                    textBox.Select(textBox.Text.Length - 1, 0);
                }
            }
            void TextBox_KeyPress(object sender, KeyPressEventArgs e)
            {
                if (!Char.IsDigit(e.KeyChar) && e.KeyChar != '\b')
                {
                    e.Handled = true;
                }
            }
            #region 黄色
            var yellowWarningLabel = new Label()
            {
                Text = "黄色警告时间：",
                Font = new Font("宋体", 0.00015F * (Width * Height)),
                AutoSize = true,
                Location = new Point(0, 0),
            };
            Controls.Add(yellowWarningLabel);
            var yellowWarningTextBox = new TextBox()
            {
                Font = yellowWarningLabel.Font,
                AutoSize = true,
                Location = yellowWarningLabel.Location + new Size(Width / 2, 0),
                Text = UsedTime.ToYelloTime.ToString() + 's',
            };
            yellowWarningTextBox.TextChanged += TextBox_TextChanged;
            yellowWarningTextBox.KeyPress += TextBox_KeyPress;
            Controls.Add(yellowWarningTextBox);
            #endregion
            #region 红色
            var redWarningLabel = new Label()
            {
                Text = "红色警告时间：",
                Font = yellowWarningLabel.Font,
                AutoSize = true,
                Location = yellowWarningLabel.Location + new Size(0, yellowWarningLabel.Height + Height / 10),
            };
            Controls.Add(redWarningLabel);
            var redWarningTextBox = new TextBox()
            {
                Font = yellowWarningLabel.Font,
                AutoSize = true,
                Location = redWarningLabel.Location + new Size(Width / 2, 0),
                Text = UsedTime.ToRedTime.ToString() + 's',
            };
            redWarningTextBox.TextChanged += TextBox_TextChanged;
            redWarningTextBox.KeyPress += TextBox_KeyPress;
            Controls.Add(redWarningTextBox);
            #endregion
            #region 睡眠
            var sleepLabel = new Label()
            {
                Text = "强制休眠时间：",
                Font = yellowWarningLabel.Font,
                AutoSize = true,
                Location = redWarningLabel.Location + new Size(0, redWarningLabel.Height + Height / 10),
            };
            Controls.Add(sleepLabel);
            var sleepTextBox = new TextBox()
            {
                Font = yellowWarningLabel.Font,
                AutoSize = true,
                Location = sleepLabel.Location + new Size(Width / 2, 0),
                Text = UsedTime.ToSleepTime.ToString() + 's',
            };
            sleepTextBox.TextChanged += TextBox_TextChanged;
            sleepTextBox.KeyPress += TextBox_KeyPress;
            Controls.Add(sleepTextBox);
            #endregion
            #region 效率
            var rateLabel = new Label()
            {
                Text = "休息效率：",
                Font = yellowWarningLabel.Font,
                AutoSize = true,
                Location = sleepLabel.Location + new Size(0, sleepLabel.Height + Height / 10),
            };
            Controls.Add(rateLabel);
            var rateTextBox = new TextBox()
            {
                Font = yellowWarningLabel.Font,
                AutoSize = true,
                Text = UsedTime.RestEfficiency.ToString(),
                Location = rateLabel.Location + new Size(Width / 2, 0),
            };
            void rateTextBox_KeyPress(object sender, KeyPressEventArgs e)
            {
                if (!Char.IsDigit(e.KeyChar) && e.KeyChar != '\b' && e.KeyChar != '.')
                {
                    e.Handled = true;
                }
                if (e.KeyChar == '.')
                {
                    var textBox = (TextBox)sender;
                    foreach (char ch in textBox.Text)
                    {
                        if (ch == '.')
                        {
                            e.Handled = true;
                        }
                    }
                }
            }
            void rateTextBox_TextChanged(object sender, EventArgs e)
            {
                var textBox = (TextBox)sender;
                if (textBox.Text.Length > 0 && textBox.Text[0] == '.')
                {
                    textBox.Text = '0' + textBox.Text;
                }
            }
            rateTextBox.KeyPress += rateTextBox_KeyPress;
            rateTextBox.TextChanged += rateTextBox_TextChanged;
            Controls.Add(rateTextBox);
            #endregion
            #region 恢复
            var defaultButton = new Button()
            {
                Font = yellowWarningLabel.Font,
                AutoSize = true,
                Text = "复原",
                Location = rateTextBox.Location + new Size(0, rateTextBox.Height + Height / 10),
            };
            void DefaultButton_Click(object sender, EventArgs e)
            {
                yellowWarningTextBox.Text = UsedTime.ToYelloTime.ToString() + 's';
                redWarningTextBox.Text = UsedTime.ToRedTime.ToString() + 's';
                sleepTextBox.Text = UsedTime.ToSleepTime.ToString() + 's';
                rateTextBox.Text = UsedTime.RestEfficiency.ToString();
            }
            defaultButton.Click += DefaultButton_Click;
            Controls.Add(defaultButton);
            #endregion
            #region 确认
            var OKButton = new Button()
            {
                Font = yellowWarningLabel.Font,
                AutoSize = true,
                Text = "确认",
                Location = defaultButton.Location - new Size(Width / 2, 0),
            };
            void OKButton_Click(object sender, EventArgs e)
            {
                int yellow = 0, red = 0, sleep = 0;
                double rate = 0.0;
                try
                {
                    yellow = int.Parse(yellowWarningTextBox.Text.TrimEnd('s'));
                    red = int.Parse(redWarningTextBox.Text.TrimEnd('s'));
                    sleep = int.Parse(sleepTextBox.Text.TrimEnd('s'));
                    rate = double.Parse(rateTextBox.Text);
                    if (yellow >= red)
                    {
                        throw new Exception("黄色警告时间必须小于红色警告时间");
                    }
                    if (red >= sleep)
                    {
                        throw new Exception("红色警告时间必须小于强制休眠时间");
                    }
                    if (rate == 0)
                    {
                        throw new Exception("效率不能是0");
                    }
                    if (sleep / rate > 19800)
                    {
                        throw new Exception("强制休眠的总时间不能超过6个小时");
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, "错误");
                    DefaultButton_Click(null, null);
                    return;
                }
                Settings.Default.ToYelloTime = yellow;
                Settings.Default.ToRedTime = red;
                Settings.Default.ToSleepTime = sleep;
                Settings.Default.RestEfficiency = rate;
                Settings.Default.Save();
                UsedTime.ToYelloTime = yellow;
                UsedTime.ToRedTime = red;
                UsedTime.ToSleepTime = sleep;
                UsedTime.RestEfficiency = rate;
                UsedTime.ResetWarning();
                Dispose();
            }
            OKButton.Click += OKButton_Click;
            Controls.Add(OKButton);
            #endregion
        }
    }
}
