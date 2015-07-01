using SharedStuff;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Specialized;

namespace Server
{
    internal partial class ServerChatClipboard : Form
    {
        private ServerState _state;

        internal ServerChatClipboard(ServerState pState)
        {
            InitializeComponent();
            this._state = pState;

            this._state.dUpdateHistory = this.UpdateHistory;
            this._state.dUpdateClipboard = this.UpdateClipboard;
        }

        private void ChatSendButton_Click(object sender, EventArgs e)
        {
            String Message = this.ChatTextBox.Text;
            if (Message.Length > 0)
            {
                // Updates server history
                this.UpdateHistory("admin: " + Message);
                // Sends message to all clients
                foreach (var Client in this._state.Clients)
                {
                    lock (Client.Value)
                    {
                        try
                        {
                            (new BinaryWriter(Client.Value.ChatControl.GetStream())).Write(MessageCodes.MSG_SERVER2CLIENT + ":admin:" + Message);
                        }
                        catch (Exception catchedException)
                        {
                            SmartDebug.DWL(catchedException.Message);
                            SmartDebug.DWL(catchedException.StackTrace);
                        }
                    }
                }
                this.ChatTextBox.Text = String.Empty;
            }
            this.ChatTextBox.Focus();
        }

        private void UpdateHistory(String pMessage)
        {
            // TODO update new page when text length reaches maxlength
            if (this.HistoryTextBox.TextLength + pMessage.Length + 2 < this.HistoryTextBox.MaxLength)
            {
                this.HistoryTextBox.Text += pMessage + "\r\n";
                this.HistoryTextBox.SelectionStart = this.HistoryTextBox.Text.Length;
                this.HistoryTextBox.ScrollToCaret();
                this.HistoryTextBox.Refresh();
            }
        }

        /// <summary>
        /// Updates clipboard data: can be made only by a STA Thread
        /// </summary>
        /// <param name="pDataType"></param>
        /// <param name="pClipboardData"></param>
        private void UpdateClipboard(Byte pDataType, Object pClipboardData)
        {
            if(pDataType == Constants.TYPE_TEXT)
                Clipboard.SetText((String)pClipboardData);
            else if (pDataType == Constants.TYPE_BMP)
            {
                Clipboard.SetImage((Image)pClipboardData);
            }
            else if (pDataType == Constants.TYPE_FILE)
            {
                StringCollection lFileDropList = Clipboard.GetFileDropList();
                lFileDropList.Add((String)pClipboardData);
                Clipboard.SetFileDropList(lFileDropList);
            }
        }

        private void ClipboardPasteButton_Click(object sender, EventArgs e)
        {
            if (this._state.Transmitting == false)
            {
                this._state.Transmitting = true;
                this.GetContentFromServer();
            }
            else
                this.UpdateHistory("Unable to send clipboard data: previous transmission is still open");
        }

        /// <summary>
        /// Gets content from server clipboard and makes send it to all connected clients
        /// </summary>
        private void GetContentFromServer()
        {
            if (Clipboard.ContainsText())
            {
                this.Invoke(this._state.dSendClipboard, new Object[3] {null, Constants.TYPE_TEXT, Clipboard.GetText()});
                this._state.Transmitting = false;
            }
            else if (Clipboard.ContainsImage())
            {
                Image img = Clipboard.GetImage();
                ThreadPool.QueueUserWorkItem(GetImageFromServer, (Object) img);
            }
            else if (Clipboard.ContainsFileDropList())
            {
                StringCollection lCurrentFileDropList = Clipboard.GetFileDropList();
                ThreadPool.QueueUserWorkItem(GetFilesFromServer, (Object)lCurrentFileDropList);
            }
            else
            {
                MessageBox.Show("Clipboard format not supported");
                this._state.Transmitting = false;
                return;
            }
        }

        private void GetImageFromServer(Object imgParam)
        {
            Image img = (Image) imgParam;
            MemoryStream ms = new MemoryStream();
            //TESTING
            //img.RotateFlip(RotateFlipType.Rotate180FlipY);
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            this.Invoke(this._state.dSendClipboard, new Object[3] {null, Constants.TYPE_BMP, ms.GetBuffer()});
            this._state.Transmitting = false;
        }

        private void GetFilesFromServer(Object lCurrentFileDropList)
        {
            foreach (String lPathFilename in (StringCollection)lCurrentFileDropList)
            {
                Byte[] lFileBytes = Routines.FileToBytes(lPathFilename);
                if (lFileBytes == null)
                    MessageBox.Show("Unable to copy " + lPathFilename);
                else
                {
                    String lFilename = lPathFilename.Split("\\".ToCharArray())[lPathFilename.Split("\\".ToCharArray()).Length - 1];
                    this.Invoke(this._state.dSendClipboard, new Object[3] { null, Constants.TYPE_FILE, new Object[2] { lFileBytes, lFilename } });
                }
            }
            this._state.Transmitting = false;
        }

        private void ServerChatClipboard_FormClosed(object sender, FormClosedEventArgs e)
        {
            this._state.wChatClipboard = null;
            this.Invoke(this._state.dDisconnect);
        }

    }
}
