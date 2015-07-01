using SharedStuff;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
        /// Routine executed by transfer thread, listens for content on
        /// clients clipboard connection
        /// </summary>
        private void TransferRoutine()
        {
            while (this._state.WorkEnd == false)
            {
                try
                {
                    foreach (var ClientContext in this._state.Clients)
                    {
                        if (ClientContext.Value.Clipboard.Available > 0)
                            this.GetContentFromClient(ClientContext.Value.Clipboard);
                    }
                }
                catch (Exception e)
                {
                    SmartDebug.DWL("Transfer exception");
                    SmartDebug.DWL(e.Message);
                }
                Thread.Sleep(Constants.MS_SLEEP_TIMEOUT_BG_THREADS);
            }
            SmartDebug.DWL("Transfer thread exiting");
        }

        /// <summary>
        /// Gets clipboard data from client
        /// </summary>
        /// <param name="pClientClipboard"></param>
        private void GetContentFromClient(TcpClient pClientClipboard)
        {
            Byte lDataType = new BinaryReader(pClientClipboard.GetStream()).ReadByte();

            switch (lDataType)
            {
                case Constants.TYPE_TEXT:
                    this.ContentIsText(pClientClipboard);
                    break;
                case Constants.TYPE_BMP:
                    this.ContentIsBMP(pClientClipboard);
                    break;
                case Constants.TYPE_FILE:
                    this.ContentIsFile(pClientClipboard);
                    break;
                default:
                    SmartDebug.DWL("Clipboard content received format not recognized");
                    break;
            }
        }

        /// <summary>
        /// Gets clipboard data (text) from client
        /// </summary>
        /// <param name="pClientClipboard"></param>
        private void ContentIsText(TcpClient pClientClipboard)
        {
            String lClipboardText = new BinaryReader(pClientClipboard.GetStream()).ReadString();
            this.Invoke(this._state.dUpdateClipboard, new Object[2] {Constants.TYPE_TEXT, lClipboardText});
            this.Invoke(this._state.dUpdateHistory, "New content on clipboard: text");
            this._state.Transmitting = true;
            this.SendClipboardDataToAll(pClientClipboard, Constants.TYPE_TEXT, (Object)lClipboardText);
        }

        /// <summary>
        /// Gets clipboard data (BMP) from client
        /// </summary>
        /// <param name="pClientClipboard"></param>
        private void ContentIsBMP(TcpClient pClientClipboard)
        {
            Int32 lDataLength = new BinaryReader(pClientClipboard.GetStream()).ReadInt32();
            byte[] imgBytes = new BinaryReader(pClientClipboard.GetStream()).ReadBytes(lDataLength);
            Image lBMP = Image.FromStream(new MemoryStream(imgBytes));
            this.Invoke(this._state.dUpdateClipboard, new Object[2] {Constants.TYPE_BMP, (Object)lBMP});
            this.Invoke(this._state.dUpdateHistory, "New content on clipboard: bmp");
            this._state.Transmitting = true;
            this.SendClipboardDataToAll(pClientClipboard, Constants.TYPE_BMP, (Object)imgBytes);
        }

        /// <summary>
        /// Gets clipboard data (file) from client
        /// </summary>
        /// <param name="pClientClipboard"></param>
        private void ContentIsFile(TcpClient pClientClipboard)
        {
            String lFilename = new BinaryReader(pClientClipboard.GetStream()).ReadString();
            Int32 lDataLength = new BinaryReader(pClientClipboard.GetStream()).ReadInt32();
            if (lDataLength > 0)
            {
                Byte[] lFileBytes = new BinaryReader(pClientClipboard.GetStream()).ReadBytes(lDataLength);
                this.Invoke(this._state.dUpdateClipboard, new Object[2] { 
                    Constants.TYPE_FILE, 
                    Routines.BytesToFile(lFilename, lFileBytes, Directory.GetCurrentDirectory() + "\\" + Constants.CLIPBOARD_FILES_DIR + "\\") });
                this.Invoke(this._state.dUpdateHistory, "New content on clipboard: file");
                this._state.Transmitting = true;
                this.SendClipboardDataToAll(pClientClipboard, Constants.TYPE_FILE, new Object [2] { lFileBytes, lFilename });
            }
        }

        /// <summary>
        /// Sends clipboard data to all connected clients
        /// </summary>
        /// <param name="pSender"></param>
        /// <param name="pDataType"></param>
        /// <param name="pData"></param>
        private void SendClipboardDataToAll(TcpClient pSender, Byte pDataType, Object pData)
        {
            if (pData != null)
            {

                Dictionary<String, TaskInfo> lClients = new Dictionary<String, TaskInfo>();
                lock (this._state.Clients)
                {
                    foreach (var lEntry in this._state.Clients)
                    {
                        try
                        {
                            lClients.Add(lEntry.Key, lEntry.Value);
                        }
                        finally { }
                    }
                }


                foreach (var lClient in lClients)
                {
                    try
                    {
                        if (lClient.Value.Clipboard != pSender)
                            this.SendClipboardData(lClient.Value.Clipboard, pDataType, pData);
                    }
                    catch
                    {
                        SmartDebug.DWL("Transfer exception, sending clipboard data");
                    }

                }
            }

            this._state.Transmitting = false;
        }

        /// <summary>
        /// Sends data to a specific client
        /// </summary>
        /// <param name="pReceiver"></param>
        /// <param name="pDataType"></param>
        /// <param name="pData"></param>
        private void SendClipboardData(TcpClient pReceiver, Byte pDataType, Object pData)
        {
            switch (pDataType)
            {
                case Constants.TYPE_TEXT:
                    {
                        BinaryWriter bw = new BinaryWriter(pReceiver.GetStream());

                        bw.Write(pDataType);
                        bw.Write((String)pData);
                        this.Invoke(this._state.dUpdateHistory, "Clipboard data sent");
                    }
                    break;
                case Constants.TYPE_BMP:
                    {
                        BinaryWriter bw = new BinaryWriter(pReceiver.GetStream());

                        bw.Write(pDataType);
                        bw.Write(((Byte[])pData).Length);
                        bw.Write((Byte[])pData);
                        this.Invoke(this._state.dUpdateHistory, "Clipboard data sent");
                    }
                    break;
                case Constants.TYPE_FILE:
                    {
                        BinaryWriter bw = new BinaryWriter(pReceiver.GetStream());

                        bw.Write(pDataType);
                        Byte[] lFileBytes = (Byte[])((Object[])pData)[0];
                        String lFilename = (String)((Object[])pData)[1];
                        bw.Write(lFilename);
                        bw.Write(lFileBytes.Length);
                        bw.Write(lFileBytes);
                        this.Invoke(this._state.dUpdateHistory, "Clipboard data sent");
                    }
                    break;
                default:
                    SmartDebug.DWL("Clipboard content received format not recognized");
                    break;
            }
        }
    }
}
