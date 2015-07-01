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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Server
{
    internal partial class ServerMain : Form
    {
        private const Int32 MAX_CLIENTS = 4;
        private ServerState _state;

        internal ServerMain()
        {
            InitializeComponent();

            // Server state class, locked only by event callback functions 
            // when modifying GUI fields not by subroutines
            this._state = new ServerState();

            // Setting of delegates
            this._state.dSendClipboard = this.SendClipboardDataToAll;
            this._state.dDisconnect = this.ServerDown;

            // Setting default streaming area
            this._state.Area = new Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            // Setting ThreadPool threads minimum
            ThreadPool.SetMinThreads(MAX_CLIENTS + Constants.NUM_ROWS_GRID + 1, MAX_CLIENTS + Constants.NUM_ROWS_GRID + 1);

            // Creates or empties Clipboard Shared Items directory
            if (Directory.Exists(Directory.GetCurrentDirectory() + "\\" + Constants.CLIPBOARD_FILES_DIR))
                Directory.Delete(Directory.GetCurrentDirectory() + "\\" + Constants.CLIPBOARD_FILES_DIR, true);
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\" + Constants.CLIPBOARD_FILES_DIR);
        }

        /// <summary>
        /// Server user inserted server port number, locks this.State
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PortTextBox_Leave(object sender, EventArgs e)
        {
            lock (this._state)
            {
                try
                {
                    this._state.PortNumber = PortTextBox.Text;
                }
                catch
                {
                    SmartDebug.DWL("Port text box not read correctly");
                }
            }
        }

        /// <summary>
        /// Server user inserted server password, locks this.State
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
                    SmartDebug.DWL("Password text box not read correctly");
                }
            }
        }

        /// <summary>
        /// Server user clicked online button, locks this.State
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnlineButton_Click(object sender, EventArgs e)
        {
            lock (this._state)
            {
                try
                {
                    if (Routines.ValidPassword(this._state.Password)
                        && Routines.ValidPortNumber(this._state.PortNumber))
                    {
                        // No further checks, if server is down it will be
                        // set up by ServerUp routine
                        this.ServerUp();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to start server");
                    SmartDebug.DWL(ex.Message);
                }
            }
        }

        /// <summary>
        /// Server user clicked offline button, locks this.State
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OfflineButton_Click(object sender, EventArgs e)
        {
            lock (this._state)
            {
                try
                {
                    // No checks, if server is up will be set down
                    // by ServerDown routine
                    this.ServerDown();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to stop server");
                    SmartDebug.DWL(ex.Message);
                }
            }
        }
        
        /// <summary>
        /// Executed when server user closes server main window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerMainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            lock (this._state)
            {
                try
                {
                    // No checks, if server is up will be set down
                    // by ServerDown routine
                    this.ServerDown();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to stop server");
                    SmartDebug.DWL(ex.Message);
                }
            }
            Environment.Exit(0);
        }

        /// <summary>
        /// Executed when server user clicks submenu entry; fills all field with debug values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void setDefaultValuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lock (this._state)
            {
                this._state.PortNumber = Constants.DEFAULT_SERVER_PORT;
                this._state.Password = Constants.DEFAULT_PASSWORD;

                this.PortTextBox.Text = Constants.DEFAULT_SERVER_PORT;
                this.PasswordTextBox.Text = Constants.DEFAULT_PASSWORD;
            }
        }

        private void areaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new ServerAreaSelector(this._state).ShowDialog();
        }


        private void windowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new ServerWindowSelector(this._state).ShowDialog();
        }

        private void startVideoStreamingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this._state.Clients.Count == 0)
            {
                MessageBox.Show("No clients connected");
                return;
            }
            if (this._state.VideoOn == false)
            {
                this.selectToolStripMenuItem.Enabled = false;
                this._state.VideoOn = true;
                this.startVideoStreamingToolStripMenuItem.Text = "Stop video streaming";
                this.OnBigFrameEvent(null, null);
            }
            else
            {
                this._state.VideoOn = false;
                this.startVideoStreamingToolStripMenuItem.Text = "Start video streaming";
                this.selectToolStripMenuItem.Enabled = true; 
            }
        }

    }
}
