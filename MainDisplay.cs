using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using Timer = System.Windows.Forms.Timer;

namespace PartyPhotoBooth
{
    public partial class MainDisplay : Form
    {
        public ICameraDevice CameraDevice { get; set; }
        private Timer _liveViewTimer = new Timer();
        private Boolean liveViewActive = false;

        private Form1 form1;
        private SecondaryDisplay secondaryDisplay;

        private bool isFocussing = false;

        public MainDisplay(ICameraDevice cameraDevice, Form1 form1, SecondaryDisplay secondaryDisplay)
        {
            //set live view default frame rate to 15
            _liveViewTimer.Interval = 1000 / 24;
            _liveViewTimer.Stop();
            _liveViewTimer.Tick += _liveViewTimer_Tick;
            CameraDevice = cameraDevice;
            CameraDevice.CameraDisconnected += CameraDevice_CameraDisconnected;

            this.form1 = form1;
            this.secondaryDisplay = secondaryDisplay;
            InitializeComponent();
        }

        private void LiveViewForm_Load(object sender, EventArgs e)
        {
            if (Screen.AllScreens.Length > 1)
            {
                this.StartPosition = FormStartPosition.Manual;
                foreach(Screen screen in Screen.AllScreens)
                {
                    if(screen == System.Windows.Forms.Screen.PrimaryScreen)
                    {
                        this.Bounds = screen.Bounds;
                        //this.TopMost = true;
                        this.FormBorderStyle = FormBorderStyle.None;
                        this.WindowState = FormWindowState.Maximized;
                    }
                }
            }
            /* 
             */
            new Thread(StartLiveView).Start();
        }
        void CameraDevice_CameraDisconnected(object sender, DisconnectCameraEventArgs eventArgs)
        {
            MethodInvoker method = delegate
            {
                _liveViewTimer.Stop();
                Thread.Sleep(100);
                Close();
            };
            if (InvokeRequired)
                BeginInvoke(method);
            else
                method.Invoke();
        }

        void _liveViewTimer_Tick(object sender, EventArgs e)
        {
            LiveViewData liveViewData = null;
            try
            {
                liveViewData = CameraDevice.GetLiveViewImage();
            }
            catch (Exception)
            {
                return;
            }

            if (liveViewData == null || liveViewData.ImageData == null)
            {
                return;
            }
            try
            {
                pictureBox1.Image = new Bitmap(new MemoryStream(liveViewData.ImageData,
                                                                liveViewData.ImageDataPosition,
                                                                liveViewData.ImageData.Length -
                                                                liveViewData.ImageDataPosition));
                //secondaryDisplay.setPicture(new Bitmap(pictureBox1.Image));
            }
            catch (Exception)
            {

            }
        }

        private void btn_start_Click(object sender, EventArgs e)
        {
            hideButtons();
            new Thread(StartLiveView).Start();
        }

        private void StartLiveView()
        {
            bool retry;
            do
            {
                retry = false;
                try
                {
                    if(CameraDevice.IsConnected)
                    {
                        CameraDevice.StartLiveView();
                    }
                    else
                    {
                        MessageBox.Show("Kamera ist nicht verbunden");
                        Close();
                    }
                }
                catch (DeviceException exception)
                {
                    if (exception.ErrorCode == ErrorCodes.MTP_Device_Busy || exception.ErrorCode == ErrorCodes.ERROR_BUSY)
                    {
                        // this may cause infinite loop
                        Thread.Sleep(100);
                        retry = true;
                    }
                    else
                    {
                        MessageBox.Show("Error occurred :" + exception.Message);
                    }
                }

            } while (retry);
            liveViewActive = true;
            MethodInvoker method = () => _liveViewTimer.Start();
            if (InvokeRequired)
                BeginInvoke(method);
            else
                method.Invoke();
        
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            new Thread(StopLiveView).Start();
        }

        private void StopLiveView()
        {
            if (!liveViewActive) return;
            bool retry;
            do
            {
                retry = false;
                try
                {
                    _liveViewTimer.Stop();
                    // wait for last get live view image
                    //Thread.Sleep(500);
                    if(CameraDevice.IsConnected)
                    {
                        CameraDevice.StopLiveView();
                    }
                }
                catch (DeviceException exception)
                {
                    if (exception.ErrorCode == ErrorCodes.MTP_Device_Busy || exception.ErrorCode == ErrorCodes.ERROR_BUSY)
                    {
                        // this may cause infinite loop
                        Thread.Sleep(100);
                        retry = true;
                    }
                    else
                    {
                        MessageBox.Show("Error occurred :" + exception.Message);
                    }
                }

            } while (retry);
            liveViewActive = false;
        }

        private void LiveViewForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            btn_stop_Click(null, null);
        }

        private void MainDisplay_KeyDown(object sender, KeyEventArgs e)
        {
            if (isFocussing) return;
            isFocussing = true;
            bool retry;
            do
            {
                retry = false;
                try
                {
                    _liveViewTimer.Stop();
                    // wait for last get live view image
                    //Thread.Sleep(500);
                    if (CameraDevice.IsConnected)
                    {
                        CameraDevice.AutoFocus();
                    }
                }
                catch (DeviceException exception)
                {
                    if (exception.ErrorCode == ErrorCodes.MTP_Device_Busy || exception.ErrorCode == ErrorCodes.ERROR_BUSY)
                    {
                        // this may cause infinite loop
                        Thread.Sleep(100);
                        retry = true;
                    }
                    else
                    {
                        MessageBox.Show("Error occurred :" + exception.Message);
                    }
                }

            } while (retry);

        }
        private void MainDisplay_KeyUp(object sender, KeyEventArgs e)
        {
            if (liveViewActive)
            {
                form1.btn_capture_Click(null, null);
                btn_stop_Click(null, null);

            }
            else
            {
                btn_start_Click(null, null);
            }
        }


        public void setPicture(Bitmap bitmap)
        {
            pictureBox1.Image = bitmap;
        }

        public void showButtons()
        {
            btn_print.Invoke(new Action(() =>
            {
                btn_print.Visible = true;
            }));
            btn_start.Invoke(new Action(() =>
            {
                btn_start.Visible = true;
            }));
        }

        public void hideButtons()
        {
            btn_print.Visible = false;
            btn_start.Visible = false;
        }

        private void btn_print_Click(object bsender, EventArgs e)
        {
            PrintDocument pd = new PrintDocument();
            pd.DefaultPageSettings.PrinterSettings.PrinterName = "PhotoBooth";
            
            pd.DefaultPageSettings.Landscape = true; //or false!
            pd.PrintPage += (sender, args) =>
            {
                Image i = Image.FromFile(form1.capturedPhotoPath);

                //Rectangle m = args.MarginBounds;
                Rectangle m = args.PageBounds;

                if ((double)i.Width / (double)i.Height > (double)m.Width / (double)m.Height) // image is wider
                {
                    m.Height = (int)((double)i.Height / (double)i.Width * (double)m.Width);
                }
                else
                {
                    m.Width = (int)((double)i.Width / (double)i.Height * (double)m.Height);
                }
                args.Graphics.DrawImage(i, m);
            };
            pd.Print();
        }


    }
}
