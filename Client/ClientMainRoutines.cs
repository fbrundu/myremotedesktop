using SharedStuff;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace Client
{
    internal partial class ClientMain : Form
    {
        /// <summary>
        /// Opens all connections and disables connection related GUI fields 
        /// </summary>
        private void Connect()
        {
            this._state.WorkEnd = false;
            this.SetNetworking();
            if (this._state.ControlChat.Connected)
            this.SetBGThreads();
            if (this._state.ControlChat.Connected)
            this.SetGUI();
            if (this._state.ControlChat.Connected)
            this.OpenWindows();
        }

        /// <summary>
        /// Closes all connections and enables connection related GUI fields
        /// </summary>
        private void Disconnect()
        {
            this._state.WorkEnd = true;
            this.CloseWindows();
            this.ResetNetworking();
            this.ResetBGThreads();
            this.ResetSecondaryThreadsDelegates();
            this.ResetGUI();
        }

        /// <summary>
        /// Opens all connections and ports
        /// </summary>
        private void SetNetworking()
        {
            if (this._state.ControlChat != null 
                || this._state.Clipboard != null 
                || this._state.ClipboardPort != null
                || this._state.Video != null)
                this.ResetNetworking();
            
            this._state.ControlChat = new TcpClient(this._state.Server, UInt16.Parse(this._state.Port));

            UInt16 lClipboardPortNumber = UInt16.Parse(this._state.Port);
            while (this._state.ClipboardPort == null && lClipboardPortNumber < UInt16.MaxValue)
            {
                lClipboardPortNumber++;
                try
                {
                    this._state.ClipboardPort = new TcpListener(IPAddress.Any, lClipboardPortNumber);
                    this._state.ClipboardPort.Start();
                }
                catch
                {
                    this._state.ClipboardPort = null;
                }
            }

            UInt16 lVideoPortNumber = lClipboardPortNumber;
            while (this._state.VideoPort == null && lVideoPortNumber < UInt16.MaxValue)
            {
                lVideoPortNumber++;
                try
                {
                    this._state.VideoPort = new TcpListener(IPAddress.Any, lVideoPortNumber);
                    this._state.VideoPort.Start();
                }
                catch
                {
                    this._state.VideoPort = null;
                }
            }

            try
            {
                (new BinaryWriter(this._state.ControlChat.GetStream()))
                        .Write(MessageCodes.LOGIN_REQUEST + ":" + this._state.Password + ":" + this._state.Nickname + ":" + lClipboardPortNumber + ":" + lVideoPortNumber);

            }
            catch
            {
                this.ResetNetworking();
            }
            // Opening of clipboard connection and video connection is delegated to control and chat message loop
        }

        
        /// <summary>
        /// Closes all open connections and ports
        /// </summary>
        private void ResetNetworking()
        {
            if (this._state.ControlChat != null)
            {
                this._state.ControlChat.Close();
                this._state.ControlChat = null;
            }

            if (this._state.ClipboardPort != null)
            {
                this._state.ClipboardPort.Stop();
                this._state.ClipboardPort = null;
            }
            
            if (this._state.Clipboard != null)
            {
                this._state.Clipboard.Close();
                this._state.Clipboard = null;
            }
            
            if (this._state.VideoPort != null)
            {
                this._state.VideoPort.Stop();
                this._state.VideoPort = null;
            }

            if (this._state.Video != null)
            {
                this._state.Video.Close();
                this._state.Video = null;
            }
        }

        /// <summary>
        /// Creates background threads: reader
        /// </summary>
        private void SetBGThreads()
        {
            if (this._state.Reader != null 
                || this._state.Clipper != null
                || this._state.Watcher != null)
                this.ResetBGThreads();

            this._state.Reader = new Thread(new ThreadStart(ReaderRoutine));
            this._state.Reader.IsBackground = true;
            this._state.Reader.Name = "Client:Reader";
            this._state.Reader.Start();
        }

        /// <summary>
        /// Ends gracefully background threads: reader, sender, watcher
        /// </summary>
        private void ResetBGThreads()
        {
            if (this._state.Reader != null)
            {
                this._state.Reader.Join();
                this._state.Reader = null;
            }

            if (this._state.Clipper != null)
            {
                this._state.Clipper.Join();
                this._state.Clipper = null;
            }

            if (this._state.Watcher != null)
            {
                this._state.Watcher.Join();
                this._state.Watcher = null;
            }
        }

        /// <summary>
        /// Opens windows: chat and clipboard
        /// </summary>
        private void OpenWindows()
        {
            if (this._state.wChatClipboard != null 
                || this._state.wVideo != null)
                this.CloseWindows();

            this._state.wChatClipboard = new ClientChatClipboard(this._state);
            this._state.wChatClipboard.Show();

            this._state.wVideo = new ClientVideo(this._state);
            this._state.wVideo.Show();
        }

        /// <summary>
        /// Close windows: chat and clipboard
        /// </summary>
        private void CloseWindows()
        {
            if (this._state.wChatClipboard != null)
            {
                this._state.wChatClipboard.Close();
                this._state.wChatClipboard = null;
            }

            if (this._state.wVideo != null)
            {
                this._state.wVideo.Close();
                this._state.wVideo = null;
            }
        }

        /// <summary>
        /// Sets GUI fields
        /// </summary>
        private void SetGUI()
        {
            this.ConnectButton.Enabled = false;
            this.DisconnectButton.Enabled = true;
            this.ServerTextBox.Enabled = false;
            this.PortTextBox.Enabled = false;
            this.NicknameTextBox.Enabled = false;
            this.PasswordTextBox.Enabled = false;
            this.setDefaultValuesToolStripMenuItem.Enabled = false;
            this.Text = this._state.Nickname + " : " + this.Text;
        }

        /// <summary>
        /// Unsets GUI fields
        /// </summary>
        private void ResetGUI()
        {
            this.ConnectButton.Enabled = true;
            this.DisconnectButton.Enabled = false;
            this.ServerTextBox.Enabled = true;
            this.PortTextBox.Enabled = true;
            this.NicknameTextBox.Enabled = true;
            this.PasswordTextBox.Enabled = true;
            this.setDefaultValuesToolStripMenuItem.Enabled = true;
            this.Text = "Client Main Window";
        }

        /// <summary>
        /// Resets delegates that handle methods of secondary threads
        /// </summary>
        private void ResetSecondaryThreadsDelegates()
        {
            this._state.dUpdateHistory = null;
            this._state.dSendClipboard = null;
            this._state.dUpdateVideo = null;
        }
    }
}