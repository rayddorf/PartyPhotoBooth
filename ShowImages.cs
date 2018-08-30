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
    public partial class ShowImages : Form
    {
        int thumbHeight;
        int thumbWidth;
        int numRows = 2;
        int numCols = 2;
        PictureBox[] pictureBoxes;


        public ShowImages()
        {
            InitializeComponent();
        }

        private void ShowImages_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

            thumbWidth = (int)(this.Size.Width / numCols);
            thumbHeight = (int)(this.Size.Height / numRows);

            for (int i=0;i<numCols;i++)
            {
                for(int j=0;j<numRows;i++)
                {
                    PictureBox pictureBox = new PictureBox();
                    pictureBox.Size = new Size(thumbWidth, thumbHeight );
                    pictureBox.Location = new Point(thumbWidth * i, thumbHeight * j);

                    this.Controls.Add(pictureBox);
                }
            }
        }
    }
}
