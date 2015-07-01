using SharedStuff;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace Server
{
    internal partial class ServerMain : Form
    {
        /// <summary>
        /// Routine executed by Task threads:
        /// + initializes connection (waits for a login and use MS_READ_TIMEOUT ms as a timeout 
        ///     between two messages)
        /// + sets message loop
        /// </summary>
        /// <param name="pStateInfo">TaskInfo object</param>
        private void TaskRoutine(Object pStateInfo)
        {
            Boolean LoopEnd = false;
            TaskInfo Context = (TaskInfo)pStateInfo;
            SmartDebug.DWL("Count = " + Context.State.Clients.Count);
            if (Context.State.Clients.Count < MAX_CLIENTS)
            {
               

                // Read will wait for data MS_READ_TIMEOUT ms until client registers
                Context.ChatControl.ReceiveTimeout = Constants.MS_READ_TIMEOUT;
                try
                {
                    BinaryReader FromClient = new BinaryReader(Context.ChatControl.GetStream());
                    while (!LoopEnd && !this._state.WorkEnd)
                    {
                        // Reads command and splits it from payload using ':' as separator
                        LoopEnd = Dispatch(FromClient.ReadString().Split(":".ToCharArray(), 2), Context);
                    }
                }
                catch (Exception catchedException)
                {
                    SmartDebug.DWL(catchedException.Message);
                    SmartDebug.DWL(catchedException.StackTrace);
                }

                // Try to delete user from database
                try
                {
                    Logout(Context);
                }
                catch (Exception catchedException)
                {
                    SmartDebug.DWL(catchedException.Message);
                    SmartDebug.DWL(catchedException.StackTrace);
                }
            }
            else
            {
                SmartDebug.DWL("Too many user connected!");
            }
            SmartDebug.DWL("Task exiting");
        }

        /// <summary>
        /// Dispatch received message:
        /// + login
        /// + message
        /// - status
        /// </summary>
        /// <param name="pCommand"></param>
        /// <param name="pContext"></param>
        /// <returns></returns>
        private Boolean Dispatch(String[] pCommand, TaskInfo pContext)
        {

            Boolean End = false;

            // Checks command length 
            // TODO : split + check numero comandi 
            if (pCommand.Length > 0)
            {
                // Login request
                if (pCommand[0].Equals(MessageCodes.LOGIN_REQUEST))
                {
                    String[] LoginInfo = pCommand[1].Split(":".ToCharArray());
                    if (LoginInfo.Length != MessageCodes.LOGIN_REQUEST_FIELDS - 1)
                    {
                        SmartDebug.DWL("Invalid login request");
                        End = true;
                        Routines.WriteLocking(pContext.ChatControl, MessageCodes.LOGOUT_RESPONSE_ERROR);
                    }
                    else
                    {
                        SmartDebug.DWL("Received login request -> Nick:" + LoginInfo[1] + " Pass:" + LoginInfo[0]
                            + " ClipboardPort:" + LoginInfo[2] + " VideoPort:" + LoginInfo[3]);
                        End = Login(LoginInfo[0], LoginInfo[1], LoginInfo[2], LoginInfo[3], pContext);
                        if (End == false)
                        {
                            this.Invoke(this._state.dUpdateHistory, pContext.Name + " connected");
                            foreach (var Client in this._state.Clients)
                            {
                                Routines.WriteLocking(Client.Value.ChatControl, MessageCodes.STATUS_USER_CONNECTED + ":" + pContext.Name);
                            }
                        }
                    }
                }
                // Broadcast message from client
                else if (pCommand[0].Equals(MessageCodes.MSG_CLIENT2SERVER_BROADCAST) && pContext.Name != String.Empty)
                {
                    SmartDebug.DWL("Received broadcast message");
                    End = NewBroadcastMessage(pCommand[1], pContext);
                }
                // Private message from client
                else if (pCommand[0].Equals(MessageCodes.MSG_CLIENT2SERVER_PRIVATE) && pContext.Name != String.Empty)
                {
                    SmartDebug.DWL("Received private message");
                    End = NewPrivateMessage(pCommand[1], pContext);
                }
                // Logout request
                else if (pCommand[0].Equals(MessageCodes.LOGOUT_REQUEST) && pContext.Name != String.Empty)
                {
                    SmartDebug.DWL("Received logout request");

                    // Calling stack has to terminate
                    End = true;
                }
                // Add new functionalities here
                else
                {
                    SmartDebug.DWL("Invalid request");
                    //End = true;
                    //TODO: informare client
                }
            }
            else
            {
                SmartDebug.DWL("Fields count not positive");
                End = true;
            }
            return End;
        }

        /// <summary>
        /// Handles Login request
        /// </summary>
        /// <param name="pLoginInfo"></param>
        /// <param name="pContext"></param>
        /// <returns></returns>
        private Boolean Login(String pPassword, String pName, String pClipboardPort, String pVideoPort, TaskInfo pContext)
        {
            Boolean Error = false;

            // If password is correct
            if (Routines.ValidPassword(pPassword) && pPassword == pContext.State.Password)
            {
                // If username can be inserted 
                if (pName.Length > 0 && pName.Length <= Constants.MAX_NICKNAME_LENGTH && InsertUser(pName, pClipboardPort, pVideoPort, pContext) == false)
                {
                    Error = Routines.WriteLocking(pContext.ChatControl, MessageCodes.LOGIN_RESPONSE_OK);
                    pContext.Name = pName;
                    SmartDebug.DWL("Correctly inserted user " + pName);
                    // Read timeout is now set to infinite because client is now
                    // registered on database
                    pContext.ChatControl.ReceiveTimeout = 0;
                }
                // If username cannot be inserted
                else
                {
                    Routines.WriteLocking(pContext.ChatControl, MessageCodes.LOGIN_RESPONSE_CHANGE_NAME);
                    Error = true;
                    SmartDebug.DWL("Invalid name, cannot establish clipboard connection or there are not free slots");
                }
            }
            // If password is not correct
            else
            {
                Routines.WriteLocking(pContext.ChatControl, MessageCodes.LOGIN_RESPONSE_CHANGE_PASSWORD);
                Error = true;
                SmartDebug.DWL("Invalid password");
            }

            return Error;
        }

        /// <summary>
        /// Inserts user in database and creates clipboard connection
        /// </summary>
        /// <param name="pNickname"></param>
        /// <param name="pContext"></param>
        /// <returns></returns>
        private Boolean InsertUser(String pNickname, String pClipboardPort, String pVideoPort, TaskInfo pContext)
        {
            Boolean Error = false;

            if (pContext != null && pContext.State != null && pContext.State.Clients != null && pNickname.Equals("admin") == false)
            {
                TaskInfo Client = null;

                // Is client name already in database?
                pContext.State.Clients.TryGetValue(pNickname, out Client);

                // No clients registered with this name
                if (Client == null)
                {
                    try
                    {
                        String lClientAddress = pContext.ChatControl.Client.RemoteEndPoint.ToString().Split(":".ToCharArray())[0];
                        pContext.Clipboard = new TcpClient(lClientAddress, UInt16.Parse(pClipboardPort));
                        pContext.Video = new TcpClient(lClientAddress, UInt16.Parse(pVideoPort));

                        lock (pContext.State.Clients)
                        {
                            // There is a free slot for it
                            if (pContext.State.Clients.Count <= MAX_CLIENTS)
                                pContext.State.Clients[pNickname] = pContext;
                            // Client cannot be inserted: no free slots
                            else
                            {
                                Error = true;
                                pContext.Clipboard.Close();
                            }
                        }
                    }
                    catch (Exception catchedException)
                    {
                        Error = true;
                        SmartDebug.DWL(catchedException.Message);
                        SmartDebug.DWL(catchedException.StackTrace);
                    }

                }
                // Client name already present
                else
                    Error = true;
            }
            else
                Error = true;

            return Error;
        }

        /// <summary>
        /// Server task thread handles a new message received from its client
        /// + sends message to server chat window
        /// + sends message to all clients
        /// </summary>
        /// <param name="pCommand"></param>
        /// <param name="pContext"></param>
        /// <returns></returns>
        private Boolean NewBroadcastMessage(String pMessage, TaskInfo pContext)
        {
            Boolean Error = false;

            this.Invoke(this._state.dUpdateHistory, (pContext.Name + ": " + pMessage));
            foreach (var Client in pContext.State.Clients)
            {
                Routines.WriteLocking(Client.Value.ChatControl, MessageCodes.MSG_SERVER2CLIENT + ":" + pContext.Name + ":" + pMessage);
            }

            return Error;
        }

        private Boolean NewPrivateMessage(String pCommand, TaskInfo pContext)
        {
            Boolean Error = false;
            // TODO
            return Error;
        }

        /// <summary>
        /// Logouts client using its context
        /// </summary>
        /// <param name="pContext"></param>
        private void Logout(TaskInfo pContext)
        {
            // If user has been deleted correctly
            if (DeleteUser(pContext) == false)
            {
                // Let server user know about it
                this.Invoke(this._state.dUpdateHistory, pContext.Name + " disconnected");

                // Let other users know about it
                foreach (var Client in this._state.Clients)
                {
                    Routines.WriteLocking(Client.Value.ChatControl, MessageCodes.STATUS_USER_DISCONNECTED + ":" + pContext.Name);
                }
            }
            else
            {
                SmartDebug.DWL("Unable to delete user");
            }
        }

        /// <summary>
        /// Deletes user using its context
        /// </summary>
        /// <param name="pContext"></param>
        /// <returns></returns>
        private Boolean DeleteUser(TaskInfo pContext)
        {
            Boolean Error = true;

            if (pContext != null && pContext.State != null && pContext.State.Clients != null)
            {
                lock (pContext.State.Clients)
                {
                    // If client is registered
                    if (pContext.State.Clients.ContainsValue(pContext) == true)
                    {
                        String Key = null;

                        // Finds it
                        foreach (var Entry in pContext.State.Clients)
                        {
                            if (Entry.Value == pContext)
                            {
                                Key = Entry.Key;
                                this.CleanClient(Entry.Value);
                                Error = false;
                            }
                        }

                        // Removes it
                        try
                        {
                            pContext.State.Clients.Remove(Key);
                            SmartDebug.DWL("Deleted user " + Key);
                        }
                        catch (Exception catchedException)
                        {
                            SmartDebug.DWL(catchedException.Message);
                            SmartDebug.DWL(catchedException.StackTrace);
                        }
                    }
                }
            }
            else
                Error = true;

            return Error;
        }

        private void CleanClient(TaskInfo pClient)
        {
            if (pClient == null)
                return;

            Routines.WriteLocking(pClient.ChatControl, MessageCodes.LOGOUT_RESPONSE_OK);

            if (pClient.ChatControl != null)
            {
                pClient.ChatControl.Close();
                pClient.ChatControl = null;
            }

            if (pClient.Clipboard != null)
            {
                pClient.Clipboard.Close();
                pClient.Clipboard = null;
            }
        }
    }
}