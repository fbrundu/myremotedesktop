using SharedStuff;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace Server
{
    internal partial class ServerMain : Form
    {
        private Int32 _mouseOldPositionX = -1;
        private Int32 _mouseOldPositionY = -1;

        private void StreamerRoutine()
        {
            // Sets timer for big frame streaming
            System.Timers.Timer lBigFrameTimer = new System.Timers.Timer();
            // On timer elapsed executes OnBigFrameEvent
            lBigFrameTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnBigFrameEvent);
            // Timer elapses every MS_VIDEO_BIGFRAME ms
            lBigFrameTimer.Interval = Constants.MS_VIDEO_BIGFRAME;
            // Timer restarts when elapsed
            lBigFrameTimer.AutoReset = true;
            // Enables timer
            lBigFrameTimer.Enabled = true;

            // Sets timer for cursor position frame
            System.Timers.Timer lCursorTimer = new System.Timers.Timer();
            // On timer elapsed executes OnCursorPositionEvent
            lCursorTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnCursorPositionEvent);
            // Timer elapses every MS_CURSOR_POSITION
            lCursorTimer.Interval = Constants.MS_CURSOR_POSITION;
            // Timer restarts when elapsed
            lCursorTimer.AutoReset = true;
            // Enables timer FIXME
            lCursorTimer.Enabled = true;

            // Creates and initialise streaming semaphore: used to make start every 
            // group of diff frame calculation only when previous group has finished
            this._state.StreamingSem = new Semaphore(0, Constants.NUM_ROWS_GRID * Constants.NUM_ROWS_GRID);

            // While server is online
            while (this._state.WorkEnd == false)
            {
                // If video streaming is on
                if (this._state.VideoOn == true)
                {
                    if (this._state.hWnd == IntPtr.Zero)
                    {
                        // Gets an array of Int32 that symbolizes current snapshot of area to stream
                        if (this._state.CurrentBMP == null)
                            this._state.CurrentBMP = this.GetBitmapInt32ArrayFromScreen();
                        this._state.PreviousBMP = this._state.CurrentBMP;
                        // Gets another array of Int32 that symbolizes current snapshot of area to stream
                        this._state.CurrentBMP = this.GetBitmapInt32ArrayFromScreen();
                    }
                    else
                    {
                        if (this._state.CurrentBMP == null)
                            this._state.CurrentBMP = this.GetBitmapInt32ArrayFromWindow(this._state.hWnd);
                        this._state.PreviousBMP = this._state.CurrentBMP;
                        this._state.CurrentBMP = this.GetBitmapInt32ArrayFromWindow(this._state.hWnd);
                    }
                    // Gets width and height of sub areas to give to ThreadPool worker threads
                    Int32 lWidth = this._state.Area.Width / Constants.NUM_ROWS_GRID;
                    Int32 lHeight = this._state.Area.Height / Constants.NUM_ROWS_GRID;
                    Int32 lWRem = this._state.Area.Width % Constants.NUM_ROWS_GRID;
                    Int32 lHRem = this._state.Area.Height % Constants.NUM_ROWS_GRID;

                    //TODO : optimize this
                     //Sends work items to ThreadPool worker threads
                    for (Int32 lX = 0; lX + lWidth + lWRem <= this._state.Area.Width; lX += lWidth)
                        for (Int32 lY = 0; lY + lHeight + lHRem <= this._state.Area.Height; lY += lHeight)
                            ThreadPool.QueueUserWorkItem(CreateDiffFrame,
                                (Object)new Rectangle(this._state.Area.X + lX, this._state.Area.Y + lY, lWidth + lWRem, lHeight + lHRem));

                    // Waits that previous group of diff frame calculations has finished
                    for (int i = 0; i < Constants.NUM_ROWS_GRID * Constants.NUM_ROWS_GRID; i++)
                        this._state.StreamingSem.WaitOne();
                    Thread.Sleep(Constants.MS_SLEEP_TIMEOUT_DIFFFRAME);
                }
                else
                    Thread.Sleep(Constants.MS_SLEEP_TIMEOUT_BG_THREADS);
            }

            // Releases resources
            this._state.StreamingSem.Close();
            this._state.StreamingSem.Dispose();
            lBigFrameTimer.Stop();
            lBigFrameTimer.Close();
            lBigFrameTimer.Dispose();
            lCursorTimer.Stop();
            lCursorTimer.Close();
            lCursorTimer.Dispose();
        }

        private Image CaptureWindow(IntPtr pHandle)
        {
            // Gets window size
            User32ImportClass.RECT lWindowRect = new User32ImportClass.RECT();
            User32ImportClass.GetWindowRect(pHandle, ref lWindowRect);
            int lWindowWidth = lWindowRect.right - lWindowRect.left;
            int lWindowHeight = lWindowRect.bottom - lWindowRect.top;

            // Gets hDC of target window
            IntPtr lHdcSrc = User32ImportClass.GetWindowDC(pHandle);

            // Creates destination device context
            IntPtr lHdcDest = GDI32ImportClass.CreateCompatibleDC(lHdcSrc);

            // Creates destination bitmap
            IntPtr lHBitmap = GDI32ImportClass.CreateCompatibleBitmap(lHdcSrc, lWindowWidth, lWindowHeight);
            // Selects bitmap
            IntPtr lHOldObject = GDI32ImportClass.SelectObject(lHdcDest, lHBitmap);
            // Captures window
            GDI32ImportClass.BitBlt(lHdcDest, 0, 0, lWindowWidth, lWindowHeight, lHdcSrc, 0, 0, GDI32ImportClass.SRCCOPY);
            // Restores old selection
            GDI32ImportClass.SelectObject(lHdcDest, lHOldObject);
            // Cleans up 
            GDI32ImportClass.DeleteDC(lHdcDest);
            User32ImportClass.ReleaseDC(pHandle, lHdcSrc);

            // Gets a Image object
            Image rImage = Image.FromHbitmap(lHBitmap);
            // Frees Bitmap object
            GDI32ImportClass.DeleteObject(lHBitmap);

            return rImage;
        }

        private Int32[] GetBitmapInt32ArrayFromWindow(IntPtr pHandle)
        {
            // Gets window size
            User32ImportClass.RECT lWindowRect = new User32ImportClass.RECT();
            User32ImportClass.GetWindowRect(pHandle, ref lWindowRect);
            this._state.Area = new Rectangle(0, 0, lWindowRect.right - lWindowRect.left, lWindowRect.bottom - lWindowRect.top);
            this._state.SelectedWindowOffsetX = lWindowRect.left;
            this._state.SelectedWindowOffsetY = lWindowRect.top;

            // Gets hDC of target window
            IntPtr lHdcSrc = User32ImportClass.GetWindowDC(pHandle);

            // Creates destination device context
            IntPtr lHdcDest = GDI32ImportClass.CreateCompatibleDC(lHdcSrc);

            // Creates destination bitmap
            IntPtr lHBitmap = GDI32ImportClass.CreateCompatibleBitmap(lHdcSrc, this._state.Area.Width, this._state.Area.Height);
            // Selects bitmap
            IntPtr lHOldObject = GDI32ImportClass.SelectObject(lHdcDest, lHBitmap);
            // Captures window
            GDI32ImportClass.BitBlt(lHdcDest, 0, 0, this._state.Area.Width, this._state.Area.Height, lHdcSrc, 0, 0, GDI32ImportClass.SRCCOPY);
            // Restores old selection
            GDI32ImportClass.SelectObject(lHdcDest, lHOldObject);
            // Cleans up 
            GDI32ImportClass.DeleteDC(lHdcDest);
            User32ImportClass.ReleaseDC(pHandle, lHdcSrc);

            //Gets bitmap
            Bitmap lBMP = Bitmap.FromHbitmap(lHBitmap);
            // Frees Bitmap object
            GDI32ImportClass.DeleteObject(lHBitmap);

            // Gets bitmap data
            BitmapData lBMPData = lBMP.LockBits(new Rectangle(0, 0, lBMP.Width, lBMP.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            // Creates new array of Int32 to return
            Int32[] rArray = new Int32[this._state.Area.Width * this._state.Area.Height];

            // Copies bitmap data to Int32 array
            Marshal.Copy(lBMPData.Scan0, rArray, 0, rArray.Length);

            // Returns Int32 array
            return rArray;
        }

        private Int32[] GetBitmapInt32ArrayFromScreen()
        {
            // Creates bitmap for area
            Bitmap lBMP = new Bitmap(this._state.Area.Width, this._state.Area.Height);
            // Creates Graphics from bitmap
            Graphics lGraphics = Graphics.FromImage(lBMP);

            // Copies screen area to Graphics and then to bitmap
            lGraphics.CopyFromScreen(this._state.Area.X, this._state.Area.Y, 0, 0, this._state.Area.Size, CopyPixelOperation.SourceCopy);
            // Disposes Graphics
            lGraphics.Dispose();

            // Gets bitmap data
            BitmapData lBMPData = lBMP.LockBits(new Rectangle(0, 0, lBMP.Width, lBMP.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            // Creates new array of Int32 to return
            Int32[] rArray = new Int32[this._state.Area.Width * this._state.Area.Height];

            // Copies bitmap data to Int32 array
            Marshal.Copy(lBMPData.Scan0, rArray, 0, rArray.Length);

            // Returns Int32 array
            return rArray;
        }

        private void CreateDiffFrame(Object pState)
        {
            // Gets sub area to analyze
            Rectangle lSubArea = (Rectangle)pState;
            
            try
            {
                // Analyzes sub area
                for (Int32 row = 0; row < lSubArea.Height; row += Constants.PIXEL_STEP)
                {
                    for (Int32 column = 0; column < lSubArea.Width; column += Constants.PIXEL_STEP)
                    {
                        Int32 offset = ((row + lSubArea.Y - this._state.Area.Y) * this._state.Area.Width) + column + lSubArea.X - this._state.Area.X;
                        if (this._state.PreviousBMP[offset] != this._state.CurrentBMP[offset])
                        {
                            this.StreamDiffFrame(Constants.DIFFFRAME, this.GetDiffFrame(lSubArea), lSubArea);
                            // Ends fors
                            row = lSubArea.Height;
                            column = lSubArea.Width;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                SmartDebug.DWL(e.Message);
            }
            // Asks for new data 
            this._state.StreamingSem.Release(1);
        }

        //private Rectangle TestBitmap(Rectangle pSubArea)
        //{
        //    int i, j, minX, maxX, minY, maxY, offset;

        //    if (this._state.PreviousBMP.Length != this._state.CurrentBMP.Length)
        //        return new Rectangle(0, 0, 0, 0);

        //    maxY = -1;
        //    maxX = -1;
        //    minY = pSubArea.Height;
        //    minX = pSubArea.Width;
        //    for (i = 0; i < pSubArea.Height; i++)
        //    {
        //        for (j = 0; j < minX; j++)
        //        {
        //            offset = ((i + pSubArea.Y - this._state.Area.Y) * this._state.Area.Width) + j + pSubArea.X - this._state.Area.X;
        //            if (this._state.PreviousBMP[offset] != this._state.CurrentBMP[offset])
        //            {
        //                if (i < minY)
        //                    minY = i;
        //                if (j < minX)
        //                    minX = j;
        //            }
        //        }
        //    }
        //    if (minY != pSubArea.Height)
        //    {
        //        maxX = minX;
        //        maxY = minY;
        //        for (i = pSubArea.Height - 1; i > minY; i--)
        //        {
        //            for (j = pSubArea.Width - 1; j > maxX; j--)
        //            {
        //                offset = ((i + pSubArea.Y - this._state.Area.Y) * this._state.Area.Width) + j + pSubArea.X - this._state.Area.X;
        //                if (this._state.PreviousBMP[offset] != this._state.CurrentBMP[offset])
        //                {
        //                    if (i > maxY)
        //                        maxY = i;
        //                    if (j > maxX)
        //                        maxX = j;
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        return new Rectangle(0, 0, 0, 0);
        //    }
        //    return new Rectangle(pSubArea.X + minX, pSubArea.Y + minY, maxX - minX, maxY - minY);
        //}

        private void StreamDiffFrame(Byte pFrameType, Byte[] pFrame, Rectangle pSubArea)
        {
            // Foreach client
            foreach (var ClientInfo in this._state.Clients)
            {
                try
                {
                    // Creates writer on client video socket
                    BinaryWriter lWriter = new BinaryWriter(ClientInfo.Value.Video.GetStream());
                    lock (ClientInfo.Value.Video)
                    {
                        try
                        {
                            // Writes diff frame to client
                            lWriter.Write((Byte)pFrameType);
                            lWriter.Write((Int32)pSubArea.X - this._state.Area.X);
                            lWriter.Write((Int32)pSubArea.Y - this._state.Area.Y);
                            lWriter.Write((Int32)pSubArea.Width);
                            lWriter.Write((Int32)pSubArea.Height);
                            lWriter.Write((Int32)pFrame.Length);
                            lWriter.Write((Byte[])pFrame);
                        }
                        catch (Exception e)
                        {
                            SmartDebug.DWL(e.Message);
                        }
                    }
                }
                catch (Exception e)
                {
                    SmartDebug.DWL(e.Message);
                }
            }
        }

        private Byte[] GetDiffFrame(Rectangle pSubArea)
        {
            // Creates bitmap for diff frame
            Bitmap lBMP = new Bitmap(pSubArea.Width, pSubArea.Height);
            // Creates Graphics for diff frame
            Graphics lGraphics = Graphics.FromImage(lBMP);

            if (this._state.hWnd == IntPtr.Zero)
            {
                // Copies sub area in Graphics
                lGraphics.CopyFromScreen(pSubArea.X, pSubArea.Y, 0, 0, pSubArea.Size, CopyPixelOperation.SourceCopy);
                // Disposes Graphics
                lGraphics.Dispose();
            }
            else
            {
                //// Gets window size
                //User32ImportClass.RECT lWindowRect = new User32ImportClass.RECT();
                //User32ImportClass.GetWindowRect(this._state.hWnd, ref lWindowRect);
                //this._state.Area = new Rectangle(0, 0, lWindowRect.right - lWindowRect.left, lWindowRect.bottom - lWindowRect.top);

                // Gets hDC of target window
                IntPtr lHdcSrc = User32ImportClass.GetWindowDC(this._state.hWnd);

                // Creates destination device context
                IntPtr lHdcDest = GDI32ImportClass.CreateCompatibleDC(lHdcSrc);

                // Creates destination bitmap
                IntPtr lHBitmap = GDI32ImportClass.CreateCompatibleBitmap(lHdcSrc, pSubArea.Width, pSubArea.Height);
                // Selects bitmap
                IntPtr lHOldObject = GDI32ImportClass.SelectObject(lHdcDest, lHBitmap);
                // Captures window
                GDI32ImportClass.BitBlt(lHdcDest, 0, 0, pSubArea.Width, pSubArea.Height, lHdcSrc, pSubArea.X, pSubArea.Y, GDI32ImportClass.SRCCOPY);
                // Restores old selection
                GDI32ImportClass.SelectObject(lHdcDest, lHOldObject);
                // Cleans up 
                GDI32ImportClass.DeleteDC(lHdcDest);
                User32ImportClass.ReleaseDC(this._state.hWnd, lHdcSrc);

                // Gets bitmap
                lBMP = Bitmap.FromHbitmap(lHBitmap);
                // Frees Bitmap object
                GDI32ImportClass.DeleteObject(lHBitmap);
            }
            // Saves bitmap in jpeg format in a MemoryStream
            MemoryStream ms = new MemoryStream();
            lBMP.Save(ms, ImageFormat.Jpeg);

            // Returns it as an array of bytes
            return ms.GetBuffer();
        }

        private void OnBigFrameEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                // If streaming is on streams big frame
                if (this._state.VideoOn == true)
                    this.StreamBigFrame(Constants.BIGFRAME, this.GetBigFrame());
            }
            catch { }
        }

        private void OnCursorPositionEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                // If streaming is on streams cursor position
                if (this._state.VideoOn == true)
                    this.StreamCursorPosition(Constants.CURSOR, Cursor.Position);
            }
            catch { }
        }

        private void StreamCursorPosition(Byte pFrameType, Point pPosition)
        {
            if (pPosition.X != this._mouseOldPositionX || pPosition.Y != this._mouseOldPositionY)
            {
                // Foreach client
                foreach (var ClientInfo in this._state.Clients)
                {
                    try
                    {
                        // Creates writer
                        BinaryWriter lWriter = new BinaryWriter(ClientInfo.Value.Video.GetStream());
                        lock (ClientInfo.Value.Video)
                        {
                            try
                            {
                                // Writes cursor position to client
                                lWriter.Write((Byte)pFrameType);
                                if (this._state.hWnd == IntPtr.Zero)
                                {
                                    lWriter.Write((Int32)pPosition.X - this._state.Area.X);
                                    lWriter.Write((Int32)pPosition.Y - this._state.Area.Y);
                                }
                                else
                                {
                                    lWriter.Write((Int32)pPosition.X - this._state.SelectedWindowOffsetX);
                                    lWriter.Write((Int32)pPosition.Y - this._state.SelectedWindowOffsetY);
                                }
                            }
                            catch (Exception e)
                            {
                                SmartDebug.DWL(e.Message);
                            }
                        }
                    }
                    catch { }
                }
            }
        }

        private Byte[] GetBigFrame()
        {
            // Creates bitmap for big frame
            Bitmap lBMP = new Bitmap(this._state.Area.Width, this._state.Area.Height);
            // Creates Graphics for big frame
            Graphics lGraphics = Graphics.FromImage(lBMP);

            if (this._state.hWnd == IntPtr.Zero)
            {
                // Copies area to bitmap
                lGraphics.CopyFromScreen(this._state.Area.X, this._state.Area.Y, 0, 0, this._state.Area.Size, CopyPixelOperation.SourceCopy);
                // Disposes Graphics
                lGraphics.Dispose();
            }
            else
            {
                //// Gets window size
                //User32ImportClass.RECT lWindowRect = new User32ImportClass.RECT();
                //User32ImportClass.GetWindowRect(this._state.hWnd, ref lWindowRect);
                //this._state.Area = new Rectangle(0, 0, lWindowRect.right - lWindowRect.left, lWindowRect.bottom - lWindowRect.top);

                // Gets hDC of target window
                IntPtr lHdcSrc = User32ImportClass.GetWindowDC(this._state.hWnd);

                // Creates destination device context
                IntPtr lHdcDest = GDI32ImportClass.CreateCompatibleDC(lHdcSrc);

                // Creates destination bitmap
                IntPtr lHBitmap = GDI32ImportClass.CreateCompatibleBitmap(lHdcSrc, this._state.Area.Width, this._state.Area.Height);
                // Selects bitmap
                IntPtr lHOldObject = GDI32ImportClass.SelectObject(lHdcDest, lHBitmap);
                // Captures window
                GDI32ImportClass.BitBlt(lHdcDest, 0, 0, this._state.Area.Width, this._state.Area.Height, lHdcSrc, 0, 0, GDI32ImportClass.SRCCOPY);
                // Restores old selection
                GDI32ImportClass.SelectObject(lHdcDest, lHOldObject);
                // Cleans up 
                GDI32ImportClass.DeleteDC(lHdcDest);
                User32ImportClass.ReleaseDC(this._state.hWnd, lHdcSrc);

                //Gets bitmap
                lBMP = Bitmap.FromHbitmap(lHBitmap);
                // Frees Bitmap object
                GDI32ImportClass.DeleteObject(lHBitmap);
            }

            // Creates MemoryStream and saves big frame on it in jpeg format
            MemoryStream ms = new MemoryStream();
            lBMP.Save(ms, ImageFormat.Jpeg);

            // Returns MemoryStream as a Byte array
            return ms.GetBuffer();
        }

        private void StreamBigFrame(Byte pFrameType, Byte[] pFrame)
        {
            // Foreach client
            foreach (var ClientInfo in this._state.Clients)
            {
                try
                {
                    // Creates writer to client
                    BinaryWriter lWriter = new BinaryWriter(ClientInfo.Value.Video.GetStream());
                    lock (ClientInfo.Value.Video)
                    {
                        try
                        {
                            // Writes big frame to client
                            lWriter.Write((Byte)pFrameType);
                            lWriter.Write((Int32)pFrame.Length);
                            lWriter.Write((Byte[])pFrame);
                        }
                        catch (Exception e)
                        {
                            SmartDebug.DWL(e.Message);
                        }
                    }
                }
                catch { }
            }
        }

    }
}