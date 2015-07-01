using SharedStuff;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Server
{
    internal partial class ServerAreaSelector : Form
    {
        private ServerState _state;
        private Int32 _top;
        private Int32 _bottom;
        private Int32 _left;
        private Int32 _right;
        private Object _sync;
        private System.Timers.Timer _updatePreviewTimer;

        internal ServerAreaSelector(ServerState pState)
        {
            InitializeComponent();

            this._state = pState;

            this._sync = new Object();

            this._top = this._state.Area.Y;
            this._bottom = this._state.Area.Y + this._state.Area.Height;
            this._left = this._state.Area.X;
            this._right = this._state.Area.X + this._state.Area.Width;

            this.TopTrackBar.Maximum = Screen.PrimaryScreen.Bounds.Height;
            this.BottomTrackBar.Maximum = Screen.PrimaryScreen.Bounds.Height;
            this.LeftTrackBar.Maximum = Screen.PrimaryScreen.Bounds.Width;
            this.RightTrackBar.Maximum = Screen.PrimaryScreen.Bounds.Width;

            this.SetTrackbars();

            this._updatePreviewTimer = new System.Timers.Timer();
            this._updatePreviewTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnUpdatePreviewEvent);
            this._updatePreviewTimer.Interval = Constants.MS_PREVIEW_AREA_SELECTOR;
            this._updatePreviewTimer.AutoReset = true;
            this._updatePreviewTimer.Enabled = true;
        }

        private void OnUpdatePreviewEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            lock (this._sync)
            {
                // Creates bitmap for area
                Bitmap lBMP = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                // Creates Graphics from bitmap
                Graphics lGraphics = Graphics.FromImage(lBMP);

                // Copies screen area to Graphics and then to bitmap
                lGraphics.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);

                lGraphics.DrawRectangle(new Pen(Color.Red, Constants.PEN_WIDTH), this._left, this._top, this._right - this._left, this._bottom - this._top);
                // Disposes Graphics
                lGraphics.Dispose();

               this.VideoPictureBox.Image = lBMP;
            }
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            lock (this._sync)
            {
                this._top = 0;
                this._bottom = Screen.PrimaryScreen.Bounds.Height;
                this._left = 0;
                this._right = Screen.PrimaryScreen.Bounds.Width;
            }
            this.SetTrackbars();
            this._state.hWnd = IntPtr.Zero;
            this._state.SelectedWindowOffsetX = 0;
            this._state.SelectedWindowOffsetY = 0;
        }

        private void SetTrackbars()
        {
            lock (this._sync)
            {
                this.TopTrackBar.Value = this._top;
                this.BottomTrackBar.Value = this._bottom;
                this.LeftTrackBar.Value = this._left;
                this.RightTrackBar.Value = this._right;
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            lock (this._sync)
            {
                this._state.Area = new Rectangle(this._left, this._top, this._right - this._left, this._bottom - this._top);
            }
            this.ReleaseResources();
        }

        private void CancelAreaButton_Click(object sender, EventArgs e)
        {
            this.ReleaseResources();
        }

        private void TopTrackBar_Scroll(object sender, EventArgs e)
        {
            lock (this._sync)
            {
                if (this.TopTrackBar.Value < this._bottom)
                    this._top = this.TopTrackBar.Value;
                else
                {
                    this._top = this._bottom;
                    this.TopTrackBar.Value = this._bottom;
                }
            }
        }

        private void BottomTrackBar_Scroll(object sender, EventArgs e)
        {
            lock (this._sync)
            {
                if (this.BottomTrackBar.Value > this._top)
                    this._bottom = this.BottomTrackBar.Value;
                else
                {
                    this._bottom = this._top;
                    this.BottomTrackBar.Value = this._top;
                }
            }
        }

        private void LeftTrackBar_Scroll(object sender, EventArgs e)
        {
            lock (this._sync)
            {
                if (this.LeftTrackBar.Value < this._right)
                    this._left = this.LeftTrackBar.Value;
                else
                {
                    this._left = this._right;
                    this.LeftTrackBar.Value = this._right;
                }
            }
        }

        private void RightTrackBar_Scroll(object sender, EventArgs e)
        {
            lock (this._sync)
            {
                if (this.RightTrackBar.Value > this._left)
                    this._right = this.RightTrackBar.Value;
                else
                {
                    this._right = this._left;
                    this.RightTrackBar.Value = this._left;
                }
            }
        }

        private void ServerAreaSelector_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.ReleaseResources();
        }

        private void ReleaseResources()
        {
            this._updatePreviewTimer.Stop();
            this._updatePreviewTimer.Close();
            this._updatePreviewTimer.Dispose();
            this.Dispose();
        }
    }
}
