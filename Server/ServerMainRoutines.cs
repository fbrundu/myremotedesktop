using SharedStuff;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace Server
{
    internal partial class ServerMain : Form
    {
        /// <summary>
        /// Executed by the server to set up
        /// </summary>
        private void ServerUp()
        {
            this.SetNetworking();
            SmartDebug.DWL("Network set");
            this.SetBGThreads();
            SmartDebug.DWL("Background threads set");
            this.ClearData();
            this.SetGUI();
            this.OpenWindows();
        }

        /// <summary>
        /// Executed by the server to go down
        /// </summary>
        private void ServerDown()
        {
            this.CloseWindows();
            this.ResetNetworking();
            SmartDebug.DWL("Network unset");
            this.ResetSecondaryThreadsDelegates();
            this.ResetBGThreads();
            SmartDebug.DWL("Background threads unset");
            this.ClearData();
            this.ResetGUI();
        }

        /// <summary>
        /// Opens server main port
        /// </summary>
        private void SetNetworking()
        {
            if (this._state.MainPort != null)
                this.ResetNetworking();

            this._state.MainPort = new TcpListener(IPAddress.Any, UInt16.Parse(this._state.PortNumber));
            this._state.MainPort.Start();
        }

        /// <summary>
        /// Closes server main port
        /// </summary>
        private void ResetNetworking()
        {
            if (this._state.MainPort != null)
            {
                this._state.MainPort.Stop();
                this._state.MainPort = null;
            }
        }

        /// <summary>
        /// Starts background threads: spawner, streamer, transfer
        /// </summary>
        private void SetBGThreads()
        {
            if (this._state.Spawner != null 
                || this._state.Transfer != null
                || this._state.Streamer != null)
                this.ResetBGThreads();
            
            this._state.WorkEnd = false;
            
            this._state.Spawner = new Thread(new ThreadStart(this.SpawnerRoutine));
            this._state.Spawner.IsBackground = true;
            this._state.Spawner.Name = "Server:Spawner";
            this._state.Spawner.Start();

            this._state.Transfer = new Thread(new ThreadStart(this.TransferRoutine));
            this._state.Transfer.IsBackground = true;
            this._state.Transfer.Name = "Server:Transfer";
            this._state.Transfer.Start();
 
            this._state.Streamer = new Thread(new ThreadStart(this.StreamerRoutine));
            this._state.Streamer.IsBackground = true;
            this._state.Streamer.Name = "Server:Streamer";
            this._state.Streamer.Start();
        }

        /// <summary>
        /// Makes end gracefully background threads: spawner, streamer,transfer
        /// </summary>
        private void ResetBGThreads()
        {
            this._state.WorkEnd = true;

            if (this._state.Spawner != null)
            {
                this._state.Spawner.Join();
                this._state.Spawner = null;
            }

            if (this._state.Transfer != null)
            {
                this._state.Transfer.Join();
                this._state.Transfer = null;
            }

            if (this._state.Streamer != null)
            {
                this._state.Streamer.Join();
                this._state.Streamer = null;
            }
        }

        /// <summary>
        /// Closes all client connections and clears data structure
        /// </summary>
        private void ClearData()
        {
            foreach (var ClientInfo in this._state.Clients)
            {
                if (ClientInfo.Value != null)
                {
                    try
                    {
                        ClientInfo.Value.ChatControl.Close();
                    }
                    catch { }
                    try
                    {
                        ClientInfo.Value.Clipboard.Close();
                    }
                    catch { }
                }
            }
            this._state.Clients.Clear();
        }

        /// <summary>
        /// Opens windows: chat and clipboard window
        /// </summary>
        private void OpenWindows()
        {
            if (this._state.wChatClipboard != null)
                this.CloseWindows();
            this._state.wChatClipboard = new ServerChatClipboard(this._state);
            this._state.wChatClipboard.Show();
        }

        /// <summary>
        /// Closes windows: chat and clipboard window
        /// </summary>
        private void CloseWindows()
        {
            if (this._state.wChatClipboard != null)
            {
                this._state.wChatClipboard.Close();
                this._state.wChatClipboard = null;
            }
        }

        /// <summary>
        /// Sets GUI controls
        /// </summary>
        private void SetGUI()
        {
            this.OfflineButton.Enabled = true;
            this.OnlineButton.Enabled = false;
            this.PortTextBox.Enabled = false;
            this.PasswordTextBox.Enabled = false;
            this.setDefaultValuesToolStripMenuItem.Enabled = false;
            //this.selectToolStripMenuItem.Enabled = false;
        }
        
        /// <summary>
        /// Unsets GUI controls
        /// </summary>
        private void ResetGUI()
        {
            this.OfflineButton.Enabled = false;
            this.OnlineButton.Enabled = true;
            this.PortTextBox.Enabled = true;
            this.PasswordTextBox.Enabled = true;
            this.setDefaultValuesToolStripMenuItem.Enabled = true;
            //this.selectToolStripMenuItem.Enabled = true;
        }

        /// <summary>
        /// Resets delegates that handle methods of secondary threads
        /// </summary>
        private void ResetSecondaryThreadsDelegates()
        {
            this._state.dUpdateHistory = null;
            this._state.dUpdateClipboard = null;
        }

    }
}