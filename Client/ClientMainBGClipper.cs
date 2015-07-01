using SharedStuff;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
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
        /// Routine executed by clipper thread
        /// </summary>
        private void ClipperRoutine()
        {
            Boolean lLoopEnd = false;

            this._state.dSendClipboard = this.SendClipboardData;
            
            while (lLoopEnd == false)
            {
                try
                {
                    BinaryReader lBRClipboard = new BinaryReader(this._state.Clipboard.GetStream());
                    Byte lDataType = lBRClipboard.ReadByte();
                    
                    switch (lDataType)
                    {
                        case Constants.TYPE_TEXT:
                            this.ContentIsText(lBRClipboard);
                            break;
                        case Constants.TYPE_BMP:
                            this.ContentIsBMP(lBRClipboard);
                            break;
                        case Constants.TYPE_FILE:
                            this.ContentIsFile(lBRClipboard);
                            break;
                        default:
                            break;
                    }
                }
                catch
                {
                    if (this._state.WorkEnd == false)
                    {
                        this._state.WorkEnd = true;
                        this.BeginInvoke(this._state.dDisconnect);
                    }
                    lLoopEnd = true;
                    SmartDebug.DWL("SenderClipboardException");
                }
            }
        }

        /// <summary>
        /// Sends clipboard data to server
        /// </summary>
        private void SendClipboardData()
        {
            BinaryWriter bw = new BinaryWriter(this._state.Clipboard.GetStream());
            
            if (Clipboard.ContainsText())
            {
                bw.Write(Constants.TYPE_TEXT);
                bw.Write(Clipboard.GetText());
            }
            else if (Clipboard.ContainsImage())
            {
                Image img = Clipboard.GetImage();
                MemoryStream ms = new MemoryStream();
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                Byte[] imgBuffer = ms.GetBuffer();
                bw.Write(Constants.TYPE_BMP);
                bw.Write(imgBuffer.Length);
                bw.Write(imgBuffer);
            }
            else if (Clipboard.ContainsFileDropList())
            {
                StringCollection lCurrentFileDropList = Clipboard.GetFileDropList();
                
                foreach(String lPathFilename in lCurrentFileDropList)
                {
                    Byte[] lFileBytes = Routines.FileToBytes(lPathFilename);
                    if (lFileBytes == null)
                        MessageBox.Show("Unable to copy " + lPathFilename);
                    else
                    {
                        bw.Write(Constants.TYPE_FILE);
                        bw.Write(lPathFilename.Split("\\".ToCharArray())[lPathFilename.Split("\\".ToCharArray()).Length - 1]);
                        bw.Write(lFileBytes.Length);
                        bw.Write(lFileBytes);
                    }
                }
            }
            else
            {
                MessageBox.Show("Clipboard format not supported");
                this._state.Transmitting = false;
                return;
            }
            this._state.Transmitting = false;
            this.Invoke(this._state.dUpdateHistory, "Clipboard data sent");
        }
 
        /// <summary>
        /// Content received from server is text, gets it
        /// </summary>
        /// <param name="pBR"></param>
        private void ContentIsText(BinaryReader pBR)
        {
            String lClipboardText = pBR.ReadString();
            this.Invoke(this._state.dUpdateClipboard, new Object[2] { Constants.TYPE_TEXT, lClipboardText });
            this.Invoke(this._state.dUpdateHistory, "New content on clipboard: text");
        }

        /// <summary>
        /// Content received from server is BMP, gets it
        /// </summary>
        /// <param name="pBR"></param>
        private void ContentIsBMP(BinaryReader pBR)
        {
            Int32 lDataLength = pBR.ReadInt32();
            if (lDataLength > 0)
            {
                Byte[] lImgBytes = pBR.ReadBytes(lDataLength);
                Image lBMP = Image.FromStream(new MemoryStream(lImgBytes));
                this.Invoke(this._state.dUpdateClipboard, new Object[2] { Constants.TYPE_BMP, lBMP });
                this.Invoke(this._state.dUpdateHistory, "New content on clipboard: bitmap");
            }
        }

        /// <summary>
        /// Content received from server is a file, gets it
        /// </summary>
        /// <param name="pBR"></param>
        private void ContentIsFile(BinaryReader pBR)
        {
            String lFilename = pBR.ReadString();
            Int32 lDataLength = pBR.ReadInt32();
            if (lDataLength > 0)
            {
                Byte[] lFileBytes = pBR.ReadBytes(lDataLength);
                this.Invoke(this._state.dUpdateClipboard, new Object[2] { 
                    Constants.TYPE_FILE, 
                    Routines.BytesToFile(lFilename, lFileBytes, Directory.GetCurrentDirectory() + "\\" + Constants.CLIPBOARD_FILES_DIR + "\\") });
                this.Invoke(this._state.dUpdateHistory, "New content on clipboard: file ");
            }
        }

    }
}
