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
        /// Routine executed by spawner thread:
        /// + accepts connection on port
        /// + submits connection/task to queue of working task threads
        /// </summary>
        private void SpawnerRoutine()
        {
            while (Interlocked.Equals(this._state.WorkEnd, false))
            {
                try
                {
                    // If port is open and clients count is less than MAX_CLIENTS
                    while (this._state.MainPort != null )
                    {
                        
                            // Accepts a new client
                            TcpClient Client = this._state.MainPort.AcceptTcpClient();
                            if (this._state.Clients.Count < MAX_CLIENTS)
                            {
                            SmartDebug.DWL("New connection");

                            // Creates thread context
                            TaskInfo Context = new TaskInfo(Client, this._state);

                            // Submits task to a pool of NCLIENTS threads
                            ThreadPool.QueueUserWorkItem(new WaitCallback(TaskRoutine), Context);
                        }
                        else
                        {
                            SmartDebug.DWL("Closing connection");
                            Client.Close();
                        }
                    }
                }
                catch (Exception catchedException)
                {
                    SmartDebug.DWL(catchedException.Message);
                    SmartDebug.DWL(catchedException.StackTrace);
                }
            }
            SmartDebug.DWL("Spawner thread exiting");
        }

    }
}