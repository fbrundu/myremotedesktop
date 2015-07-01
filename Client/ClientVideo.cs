using SharedStuff;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Client
{
    internal partial class ClientVideo : Form
    {
        private ClientState _state;

        internal ClientVideo(ClientState pState)
        {
            InitializeComponent();

            this._state = pState;

            this._state.dUpdateVideo = this.UpdateVideo;

            this.Text = this._state.Nickname + " : " + this.Text;
        }

        private void UpdateVideo(Bitmap pBmp)
        {
            Image old = this.VideoPictureBox.Image;
            this.VideoPictureBox.Image = pBmp;
            if (old != null)
                old.Dispose();
        }

        private void ClientVideo_ResizeEnd(object sender, EventArgs e)
        {
            this.VideoPictureBox.Width = this.Width - Constants.VIDEO_CLIENT_WIDTH_OFFSET;
            this.VideoPictureBox.Height = this.Height - Constants.VIDEO_CLIENT_HEIGHT_OFFSET;
        }

        private void ClientVideo_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this._state.WorkEnd == false)
            {
                this._state.WorkEnd = true;
                this.Invoke(this._state.dDisconnect);
            }
        }

        private void ClientVideo_Resize(object sender, EventArgs e)
        {
            this.VideoPictureBox.Width = this.Width - Constants.VIDEO_CLIENT_WIDTH_OFFSET;
            this.VideoPictureBox.Height = this.Height - Constants.VIDEO_CLIENT_HEIGHT_OFFSET;
        }

    }
}
