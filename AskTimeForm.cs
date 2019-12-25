using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EyeCarer
{
    public partial class AskTimeForm : Form
    {
        public AskTimeForm()
        {
            InitializeComponent();

            Init();
        }

        private void Init()
        {
            Size = Screen.PrimaryScreen.Bounds.Size;
//            TopMost = true;

            var textBox = new TextBox()
            {
                Size = new Size(Size.Width / 4, Size.Height / 4),
                Font = new Font("宋体", 0.00003F * Size.Width * Size.Height),
                Location = new Point(Size.Width * 3 / 8, Size.Height * 3 / 8),
            };
            Controls.Add(textBox);

            var button = new Button()
            {
                Size = textBox.Size,
                Font = textBox.Font,
                Location = textBox.Location + new Size(0, Size.Height / 8),
                Text = "确定",
            };
            void Button_Click(object sender, EventArgs e)
            {
                if (int.TryParse(textBox.Text, out int leftTime))
                {
                    if (leftTime > 0 && leftTime < 216000)
                    {
                        UsedTime.LeftTime = leftTime;
                        Dispose();
                    }
                }
            }
            button.Click += Button_Click;
            Controls.Add(button);
        }
    }
}
