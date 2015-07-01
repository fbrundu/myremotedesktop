using SharedStuff;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;
using System.Collections.Specialized;

namespace Client
{
    internal partial class ClientChatClipboard : Form
    {
        private ClientState _state;

        internal ClientChatClipboard(ClientState pState)
        {
            InitializeComponent();
            this._state = pState;

            // Reference to delegates 
            this._state.dUpdateClipboard = this.UpdateClipboardData;
            this._state.dUpdateHistory = this.UpdateHistory;

            this.Text = this._state.Nickname + " : " + this.Text;
        }

        /// <summary>
        /// Client user sends chat message to server, locks this.State.ControlChat
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChatSendButton_Click(object sender, EventArgs e)
        {
            String Message = this.ChatTextBox.Text;
            if (Message.Length > 0)
            {
                Routines.WriteLocking(this._state.ControlChat, MessageCodes.MSG_CLIENT2SERVER_BROADCAST + ":" + Message);
                this.ChatTextBox.Text = String.Empty;
            }
            this.ChatTextBox.Focus();
        }

        /// <summary>
        /// Routine called to add a message to client history
        /// </summary>
        /// <param name="pMessage"></param>
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
        /// Client user pastes clipboard data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClipboardPasteButton_Click(object sender, EventArgs e)
        {
            this.UpdateHistory("Sending clipboard data...");
            if (this._state.Transmitting == false)
            {
                this._state.Transmitting = true;
                this.BeginInvoke(this._state.dSendClipboard);
            }
            else
                this.UpdateHistory("Unable to send clipboard data: previous transmission is still open");
        }

        /// <summary>
        /// Updates clipboard data with pData provided
        /// </summary>
        /// <param name="pDataType"></param>
        /// <param name="pData"></param>
        private void UpdateClipboardData(Byte pDataType, Object pData)
        {
            switch (pDataType)
            {
                case Constants.TYPE_TEXT:
                    Clipboard.SetText((String)pData);
                    break;
                case Constants.TYPE_BMP:
                    Clipboard.SetImage((Image)pData);
                    break;
                case Constants.TYPE_FILE:
                    StringCollection lFileDropList = Clipboard.GetFileDropList();
                    lFileDropList.Add((String)pData);
                    Clipboard.SetFileDropList(lFileDropList);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// When chat and clipboard window is closed, client user is disconnected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClientChatClipboard_FormClosed(object sender, FormClosedEventArgs e)
        {
            this._state.wChatClipboard = null;
            this.Invoke(this._state.dDisconnect);
        }

    }
}
