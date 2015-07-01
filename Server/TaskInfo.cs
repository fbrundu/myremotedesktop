using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    /// <summary>
    /// Generic context informations for the task
    /// </summary>
    internal class TaskInfo
    {
        private TcpClient _chatControl;
        private TcpClient _clipboard;
        private TcpClient _video;
        private ServerState _state;
        private String _name;

        internal TaskInfo(TcpClient pChatControl, ServerState pState)
        {
            this._chatControl = pChatControl;
            this._clipboard = null;
            this._video = null;
            this._state = pState;
            this._name = String.Empty;
        }
        
        internal TcpClient ChatControl
        {
            get
            {
                return this._chatControl;
            }
            set
            {
                this._chatControl = value;
            }
        }

        internal TcpClient Clipboard
        {
            get
            {
                return this._clipboard;
            }
            set
            {
                this._clipboard = value;
            }
        }

        internal TcpClient Video
        {
            get
            {
                return this._video;
            }
            set
            {
                this._video = value;
            }
        }

        internal ServerState State
        {
            get
            {
                return this._state;
            }
        }

        internal String Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }
    }
}
