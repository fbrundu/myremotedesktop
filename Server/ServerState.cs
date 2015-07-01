using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Drawing;

namespace Server
{
    /// <summary>
    /// Server state class
    /// </summary>
    internal class ServerState
    {
        // Server main informations
        internal String PortNumber;
        internal String Password;
        
        // Server main port that listen to incoming client connections
        internal TcpListener MainPort;
        
        // Server data structure about clients informations
        internal Dictionary<String, TaskInfo> Clients;
        
        // Handle for chat and clipboard window
        internal ServerChatClipboard wChatClipboard;

        // Flag that indicates we are already transmitting
        // clipboard data
        internal volatile Boolean Transmitting;

        // Flag used to end gracefully all background threads
        internal volatile Boolean WorkEnd;

        // Flag that indicates if video streaming is on
        internal volatile Boolean VideoOn;

        // Flag that indicates that user is selecting desktop area to stream
        internal volatile Boolean SelectingArea;

        // Last two bitmaps
        internal Int32[] PreviousBMP;
        internal Int32[] CurrentBMP;

        // Semaphore used by Streamer routine
        internal Semaphore StreamingSem;

        // Spawner and transfer threads handles
        internal Thread Spawner;
        internal Thread Transfer;
        internal Thread Streamer;

        // Streamed area info/window handle
        internal Rectangle Area;
        internal IntPtr hWnd;
        internal Int32 SelectedWindowOffsetX;
        internal Int32 SelectedWindowOffsetY;

        // Delegate dUpdateHistory declaration:
        // Invoked to update history textbox in chat and clipboard window with a new message
        internal delegate void HistoryDelegate(String Message);
        internal HistoryDelegate dUpdateHistory;

        // Delegate dUpdateClipboard declaration:
        // Invoked to set clipboard data
        internal delegate void UpdateClipboardDelegate(Byte DataType, Object Data);
        internal UpdateClipboardDelegate dUpdateClipboard;

        // Delegate dClipboard declaration:
        // Invoked to send clipboard data to all clients
        internal delegate void SendClipboardData(TcpClient Sender, Byte DataType, Object Data);
        internal SendClipboardData dSendClipboard;

        // Delegate dDisconnect declaration:
        // This delegate is invoked by a thread in order to make the GUI thread
        // disconnect from the server and reset all GUI fields related to connection
        internal delegate void DisconnectDelegate();
        internal DisconnectDelegate dDisconnect;

        internal ServerState()
        {
            this.PortNumber = String.Empty;
            this.Password = String.Empty;
            this.MainPort = null;
            this.Clients = new Dictionary<String, TaskInfo>();
            this.wChatClipboard = null;
            this.Transmitting = false;
            this.WorkEnd = false;
            this.VideoOn = false;
            this.SelectingArea = false;
            this.StreamingSem = null;
            this.Spawner = null;
            this.Transfer = null;
            this.Streamer = null;
            this.Area = new Rectangle(0,0,0,0);
            this.hWnd = IntPtr.Zero;
            this.dUpdateHistory = null;
            this.dUpdateClipboard = null;
            this.dSendClipboard = null;
            this.PreviousBMP = null; 
            this.CurrentBMP = null;
            this.SelectedWindowOffsetX = 0;
            this.SelectedWindowOffsetY = 0;
        }
        
    }
}
