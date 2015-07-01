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
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Client
{
    internal partial class ClientMain : Form
    {
        private ClientState _state;
        
        public ClientMain()
        {
            InitializeComponent();
        
            // Creation of state class
            this._state = new ClientState();
            
            // Reference to Disconnect() added to delegate dDisconnect 
            this._state.dDisconnect = this.Disconnect;

            // Creates or empties Clipboard Shared Items directory
            if (Directory.Exists(Directory.GetCurrentDirectory() + "\\" + Constants.CLIPBOARD_FILES_DIR))
                Directory.Delete(Directory.GetCurrentDirectory() + "\\" + Constants.CLIPBOARD_FILES_DIR, true);
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\" + Constants.CLIPBOARD_FILES_DIR);
        }

        /// <summary>
        /// Client user inserted server address, locks this.State
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerTextBox_Leave(object sender, EventArgs e)
        {
            lock (this._state)
            {
                try
                {
                    this._state.Server = ServerTextBox.Text;
                }
                catch
                {
                    SmartDebug.DWL("Server address not read correctly");
                }
            }
        }

        /// <summary>
        /// Client user inserted server port number, locks this.State
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PortTextBox_Leave(object sender, EventArgs e)
        {
            lock (this._state)
            {
                try
                {
                    this._state.Port = PortTextBox.Text;
                }
                catch
                {
                    SmartDebug.DWL("Server port not read correctly");
                }
            }
        }

        /// <summary>
        /// Client user inserted nickname, locks this.State
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NicknameTextBox_Leave(object sender, EventArgs e)
        {
            lock (this._state)
            {
                try
                {
                    this._state.Nickname = NicknameTextBox.Text;
                }
                catch
                {
                    SmartDebug.DWL("Nickname not read correctly");
                }
            }
        }

        /// <summary>
        /// Client user inserted password, locks this.State
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PasswordTextBox_Leave(object sender, EventArgs e)
        {
            lock (this._state)
            {
                try
                {
                    this._state.Password = PasswordTextBox.Text;
                }
                catch
                {
                    SmartDebug.DWL("Password not read correctly");
                }
            }
        }

        /// <summary>
        /// Client user clicked connect button, locks this.State
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectButton_Click(object sender, EventArgs e)
        {
            lock (this._state)
            {
                try
                {
                    // All GUI fields related to connection must be filled
                    // in order to connect to server
                    if (this._state.Server.Length > 0 && this._state.Port.Length > 0
                        && this._state.Password.Length > 0 && this._state.Nickname.Length > 0)
                        this.Connect();
                }
                catch (Exception catchedException)
                {
                    MessageBox.Show("Unable to start control/chat connection");
                    SmartDebug.DWL(catchedException.Message);
                    SmartDebug.DWL(catchedException.StackTrace);
                }
            }
        }
        
        /// <summary>
        /// Client user clicked disconnect button, locks this.State
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisconnectButton_Click(object sender, EventArgs e)
        {
            lock (this._state)
            {
                try
                {
                    // TODO: is it necessary? Makes logout request to server
                    (new BinaryWriter(this._state.ControlChat.GetStream())).Write(MessageCodes.LOGOUT_REQUEST);

                    // Disconnects
                    this.Disconnect();

                }
                catch (Exception catchedException)
                {
                    MessageBox.Show("Unable to stop control/chat connection");
                    SmartDebug.DWL(catchedException.Message);
                    SmartDebug.DWL(catchedException.StackTrace);
                }
            }
        }

        /// <summary>
        /// Executed when client user closes client main window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClientMainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            this._state.WorkEnd = true;
            this.Disconnect();
            Environment.Exit(0);
        }

        /// <summary>
        /// Executed when client user clicks submenu entry; fills all field with debug values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void setDefaultValuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lock (this._state)
            {
                this._state.Server = Constants.DEFAULT_SERVER;
                this._state.Port = Constants.DEFAULT_SERVER_PORT;
                this._state.Password = Constants.DEFAULT_PASSWORD;

                this.ServerTextBox.Text = Constants.DEFAULT_SERVER;
                this.PortTextBox.Text = Constants.DEFAULT_SERVER_PORT;
                this.PasswordTextBox.Text = Constants.DEFAULT_PASSWORD;
            }
        }
    }
}
