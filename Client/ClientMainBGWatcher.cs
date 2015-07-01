using SharedStuff;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Client
{
    internal partial class ClientMain : Form
    {
        // Set true only every MS_VIDEO_UPDATE
        // Volatile in order to avoid cache optimization by compiler
        private volatile Boolean _canUpdate;

        private void WatcherRoutine()
        {
            // Reader used to read from Video socket
            BinaryReader lReader = new BinaryReader(this._state.Video.GetStream());

            // MemoryStream used for operation on video frames
            MemoryStream lStream = new MemoryStream();

            Int32 lMouseOldPositionX = -1;
            Int32 lMouseOldPositionY = -1;

            Boolean lPaintPointer = false;
            // Graphics used to modify video frame
            Graphics lGraphics = null;
            // Current frame of video to display
            Bitmap lCurrent = null;

            _canUpdate = true;

            // Sets timer for video frame updated
            System.Timers.Timer lVideoUpdateTimer = new System.Timers.Timer();
            // On timer elapsed executes OnVideoUpdateEvent
            lVideoUpdateTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnVideoUpdateEvent);
            // Timer elapses every MS_VIDEO_UPDATE ms
            lVideoUpdateTimer.Interval = Constants.MS_VIDEO_UPDATE;
            // Timer restarts when elapsed
            lVideoUpdateTimer.AutoReset = true;
            // Enables timer
            lVideoUpdateTimer.Enabled = true;

            // Calls garbage collector
            GC.Collect();

            // While state flag is false
            while (this._state.WorkEnd == false)
            {
                try
                {
                    // Reads frame type
                    Byte lFrameType = lReader.ReadByte();
                    switch (lFrameType)
                    {
                        // If it is a big frame
                        case Constants.BIGFRAME:
                            {
                                // Reads frame length in bytes
                                Int32 lFrameLength = lReader.ReadInt32();
                                if (lFrameLength > 0)
                                {
                                    // Reads frame bytes
                                    Byte[] lBytes = lReader.ReadBytes(lFrameLength);

                                    // Stores them in the memory stream
                                    lStream.Write(lBytes, 0, lBytes.Length);

                                    // Creates bitmap from the memory stream
                                    if (lGraphics != null)
                                    {
                                        lGraphics.Dispose();
                                        lGraphics = null;
                                    }
                                    if (lCurrent != null)
                                    {
                                        lCurrent.Dispose();
                                        lCurrent = null;
                                    }
                                    lCurrent = new Bitmap(Image.FromStream(lStream));
                                    lGraphics = Graphics.FromImage(lCurrent);

                                    // Cleans the memory stream
                                    lStream.SetLength(0);
                                }
                            }
                            break;
                        // If it is a diff Frame
                        case Constants.DIFFFRAME:
                            {
                                // Reads frame info and length (in bytes)
                                Int32 lDifframeStartX = lReader.ReadInt32();
                                Int32 lDifframeStartY = lReader.ReadInt32();
                                Int32 lWidth = lReader.ReadInt32();
                                Int32 lHeight = lReader.ReadInt32();
                                Int32 lVideoLength = lReader.ReadInt32();
                                
                                if (lVideoLength > 0)
                                {
                                    // Reads frame bytes
                                    Byte[] lBytes = lReader.ReadBytes(lVideoLength);

                                    // Stores them in memory stream
                                    lStream.Write(lBytes, 0, lBytes.Length);
                                    if (lGraphics != null)
                                        lGraphics.DrawImage(new Bitmap(Image.FromStream(lStream), lWidth, lHeight), lDifframeStartX, lDifframeStartY, lWidth, lHeight);
                                    // Cleans the memory stream
                                    lStream.SetLength(0);
                                }
                            }
                            break;
                        // If it is data about cursor position 
                        case Constants.CURSOR:
                            {
                                // Reads cursor position
                                Int32 lMouseNewPositionX = lReader.ReadInt32();
                                Int32 lMouseNewPositionY = lReader.ReadInt32();

                                // Updates mouse position
                                if (lMouseNewPositionX != lMouseOldPositionX || lMouseNewPositionY != lMouseOldPositionY)
                                {
                                    lMouseOldPositionX = lMouseNewPositionX;
                                    lMouseOldPositionY = lMouseNewPositionY;
                                    lPaintPointer = true;
                                }
                            }
                            break;
                        // Fatal error: socket data has no meaning
                        default:
                            throw new Exception("Fatal error: Unrecognized frame type");
                    }

                    if (_canUpdate && lGraphics != null)
                    {
                        Bitmap lModified = new Bitmap(lCurrent);

                        // Creates Graphics from copy of current big frame
                        Graphics lModifiedGraphics = Graphics.FromImage(lModified);
                        try
                        {
                            // Draws cursor position
                            if (lPaintPointer)
                            {
                                lModifiedGraphics.DrawLine(new Pen(Color.Black, Constants.PEN_WIDTH), lMouseOldPositionX, 0, lMouseOldPositionX, lModified.Height);
                                lModifiedGraphics.DrawLine(new Pen(Color.Black, Constants.PEN_WIDTH), 0, lMouseOldPositionY, lModified.Width, lMouseOldPositionY);
                            }
                            // Saves graphics in the copy of current big frame
                            this.BeginInvoke(this._state.dUpdateVideo, lModified);
                        }
                        catch (Exception ex)
                        {
                            SmartDebug.DWL(ex.Message);
                        }
                        finally
                        {
                            lModifiedGraphics.Dispose();
                        }
                        _canUpdate = false;
                    }
                }
                catch (Exception e)
                {
                    // If something throws an exception ends work
                    // TODO : must be tolerant to some exceptions?
                    if (this._state.WorkEnd == false)
                    {
                        this._state.WorkEnd = true;
                        this.BeginInvoke(this._state.dDisconnect);
                    }
                    SmartDebug.DWL(e.Message);
                }
            }
            lVideoUpdateTimer.Stop();
            lVideoUpdateTimer.Close();
            lVideoUpdateTimer.Dispose();
            if (lGraphics != null)
            {
                lGraphics.Dispose();
                lGraphics = null;
            }
            if (lCurrent != null)
            {
                lCurrent.Dispose();
                lCurrent = null;
            }
        }

        private void OnVideoUpdateEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            _canUpdate = true;
        }
    }
}