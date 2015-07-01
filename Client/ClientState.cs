using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    /// <summary>
    /// Client State Class
    /// </summary>
    internal class ClientState
    {
        // Client user informations
        internal String Server;
        internal String Port;
        internal String Nickname;
        internal String Password;

        // Connections
        internal TcpClient ControlChat;
        internal TcpClient Clipboard;
        internal TcpClient Video;

        // Passive ports (listeners) for clipboard and video
        internal TcpListener ClipboardPort;
        internal TcpListener VideoPort;

        // Flag that indicates we are already transmitting
        // clipboard data
        internal Boolean Transmitting; 

        // Background threads end
        internal volatile Boolean WorkEnd;

        // Windows handles
        internal ClientChatClipboard wChatClipboard;
        internal ClientVideo wVideo;

        // Worker threads handles
        internal Thread Clipper;
        internal Thread Reader;
        internal Thread Watcher;

        // Delegate dHistory declaration:
        // Invoked to update history with a new message
        internal delegate void HistoryDelegate(String Message);
        internal HistoryDelegate dUpdateHistory;

        // Delegate dSendClipboard declaration:
        // Invoked to send clipboard data
        internal delegate void SendClipboardDelegate();
        internal SendClipboardDelegate dSendClipboard;

        // Delegate dUpdateClipboard declaration:
        // Invoked to update clipboard data (only STAThread can set clipboard data)
        internal delegate void UpdateClipboardDelegate(Byte DataType, Object Data);
        internal UpdateClipboardDelegate dUpdateClipboard;

        // Delegate dUpdateVideo declaration:
        // Invoked to update clipboard data (only STAThread can set clipboard data)
        internal delegate void UpdateVideoDelegate(Bitmap Bmp);
        internal UpdateVideoDelegate dUpdateVideo;

        // Delegate dDisconnect declaration:
        // This delegate is invoked by a thread in order to make the GUI thread
        // disconnect from the server and reset all GUI fields related to connection
        internal delegate void DisconnectDelegate();
        internal DisconnectDelegate dDisconnect;

        internal ClientState()
        {
            this.Server = String.Empty;
            this.Port = String.Empty;
            this.Nickname = String.Empty;
            this.Password = String.Empty;
            this.ControlChat = null;
            this.Clipboard = null;
            this.Video = null;
            this.ClipboardPort = null;
            this.VideoPort = null;
            this.Transmitting = false;
            this.WorkEnd = false; 
            this.wChatClipboard = null;
            this.wVideo = null;
            this.Clipper = null;
            this.Reader = null;
            this.Watcher = null;
            this.dDisconnect = null;
            this.dUpdateHistory = null;
            this.dSendClipboard = null;
            this.dUpdateVideo = null;
        }
        
    }
}
