using SharedStuff;
using System;
using System.Diagnostics;
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
        /// Routine executed by reader thread
        /// </summary>
        private void ReaderRoutine()
        {
            try
            {
                if (this._state.ControlChat != null)
                    this.ControlAndChatMessageLoop();
            }
            catch (Exception ce)
            {
                SmartDebug.DWL(ce.Message);
                SmartDebug.DWL(ce.StackTrace);
                SmartDebug.DWL("This exception can be caused if client has been closed.");
                SmartDebug.DWL("In this case it must be ignored.");
            }
            // Cannot call method directly because of race
            // condition between threads; calling this method
            // only if end is not requested by main thread
            if (this._state.WorkEnd == false)
            {
                this._state.WorkEnd = true;
                this.BeginInvoke(this._state.dDisconnect);
            }
        }

        /// <summary>
        /// Listens to socket for control and chat commands
        /// </summary>
        private void ControlAndChatMessageLoop()
        {
            Boolean lLoopEnd = false;
            BinaryReader brControlChat = new BinaryReader(this._state.ControlChat.GetStream());
     
            while (lLoopEnd == false)
            {
                // Splits the response using ':' as separator
                String[] Response = brControlChat.ReadString().Split(':');
                try
                {
                    if (this.LoginResponseOK(Response) == false
                        && this.LoginResponseError(Response) == false
                        && this.LoginResponseChangeName(Response) == false
                        && this.LoginResponseChangePassword(Response) == false
                        && this.NewMessage(Response) == false
                        && this.UserConnected(Response) == false
                        && this.UserDisconnected(Response) == false
                        // Add new features here
                        )
                    {
                        SmartDebug.DWL("Message not recognized: ");
                        foreach (var s in Response)
                            SmartDebug.DWL("-> " + s);
                    }
                }
                catch (Exception ce)
                {
                    SmartDebug.DWL(ce.Message);
                    SmartDebug.DWL(ce.StackTrace);
                    lLoopEnd = true;
                }
            }
        }

        /// <summary>
        /// Correct login
        /// </summary>
        /// <param name="pResponse"></param>
        /// <returns></returns>
        private Boolean LoginResponseOK(String[] pResponse)
        {
            if (pResponse[0].Equals(MessageCodes.LOGIN_RESPONSE_OK) 
                && pResponse.Length == MessageCodes.LOGIN_RESPONSE_FIELDS)
            {
                if (this._state.ClipboardPort != null 
                    && this._state.ClipboardPort.Pending()
                    && this._state.VideoPort.Pending())
                {
                    this._state.Clipboard = this._state.ClipboardPort.AcceptTcpClient();
                    this._state.Video = this._state.VideoPort.AcceptTcpClient();
                    
                    this._state.Clipper = new Thread(new ThreadStart(ClipperRoutine));
                    this._state.Clipper.IsBackground = true;
                    
                    try
                    {
                        this._state.Clipper.Name = "Client:Clipper";
                    }
                    finally
                    {
                        this._state.Clipper.Start();
                    }

                    this._state.Watcher = new Thread(new ThreadStart(WatcherRoutine));
                    this._state.Watcher.IsBackground = true;

                    try
                    {
                        this._state.Watcher.Name = "Client:Watcher";
                    }
                    finally
                    {
                        this._state.Watcher.Start();
                    }
                }
                else
                {
                    MessageBox.Show("Unable to establish clipboard connection");
                    throw new Exception();
                }

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Generic error on login
        /// </summary>
        /// <param name="pResponse"></param>
        /// <returns></returns>
        private Boolean LoginResponseError(String[] pResponse)
        {
            if (pResponse[0].Equals(MessageCodes.LOGIN_RESPONSE_ERROR) 
                && pResponse.Length == MessageCodes.LOGIN_RESPONSE_FIELDS)
            {
                MessageBox.Show("Generic error on login");
                throw new Exception();
            }
            else
                return false;
        }

        /// <summary>
        /// Nickname already present
        /// </summary>
        /// <param name="pResponse"></param>
        /// <returns></returns>
        private Boolean LoginResponseChangeName(String[] pResponse)
        {
            if (pResponse[0].Equals(MessageCodes.LOGIN_RESPONSE_CHANGE_NAME) 
                && pResponse.Length == MessageCodes.LOGIN_RESPONSE_FIELDS)
            {
                MessageBox.Show("Login name already present");
                throw new Exception();
            }
            else
                return false;
        }

        /// <summary>
        /// Password not correct
        /// </summary>
        /// <param name="pResponse"></param>
        /// <returns></returns>
        private Boolean LoginResponseChangePassword(String[] pResponse)
        {
            if (pResponse[0].Equals(MessageCodes.LOGIN_RESPONSE_CHANGE_PASSWORD) 
                && pResponse.Length == MessageCodes.LOGIN_RESPONSE_FIELDS)
            {
                MessageBox.Show("Wrong password");
                throw new Exception();
            }
            else
                return false;
                        
        }

        /// <summary>
        /// New message received
        /// </summary>
        /// <param name="pResponse"></param>
        /// <returns></returns>
        private Boolean NewMessage(String[] pResponse)
        {
            if (pResponse[0].Equals(MessageCodes.MSG_SERVER2CLIENT) 
                && pResponse.Length == MessageCodes.MSG_SERVER2CLIENT_FIELDS)
            {
                this.Invoke(this._state.dUpdateHistory, (pResponse[1] + ": " + pResponse[2]));
                
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// User connected
        /// </summary>
        /// <param name="pResponse"></param>
        /// <returns></returns>
        private Boolean UserConnected(String[] pResponse)
        {
            if (pResponse[0].Equals(MessageCodes.STATUS_USER_CONNECTED) 
                && pResponse.Length == MessageCodes.STATUS_FIELDS)
            {
                this.Invoke(this._state.dUpdateHistory, (pResponse[1] + " connected"));

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// User disconnected
        /// </summary>
        /// <param name="pResponse"></param>
        /// <returns></returns>
        private Boolean UserDisconnected(String[] pResponse)
        {
            if (pResponse[0].Equals(MessageCodes.STATUS_USER_DISCONNECTED) 
                && pResponse.Length == MessageCodes.STATUS_FIELDS)
            {
                this.Invoke(this._state.dUpdateHistory, (pResponse[1] + " disconnected"));

                return true;
            }
            else
                return false;
        }
    }
}