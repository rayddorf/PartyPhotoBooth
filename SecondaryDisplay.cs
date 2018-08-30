using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PartyPhotoBooth
{
    public partial class SecondaryDisplay : Form
    {
        public SecondaryDisplay()
        {
            InitializeComponent();
        }

        public void setPicture(Bitmap bitmap)
        {
            pictureBox1.Invoke(new Action(() =>
            {
                pictureBox1.Image = bitmap;
            }));
        }

        private void SecondaryDisplay_Load(object sender, EventArgs e)
        {
            if (Screen.AllScreens.Length > 1)
            {
                this.StartPosition = FormStartPosition.Manual;
                foreach (Screen screen in Screen.AllScreens)
                {
                    if (screen != System.Windows.Forms.Screen.PrimaryScreen)
                    {
                        this.Bounds = screen.Bounds;
                        this.TopMost = true;
                        this.FormBorderStyle = FormBorderStyle.None;
                        this.WindowState = FormWindowState.Maximized;
                    }
                }
            }
        }
    }
    
}
