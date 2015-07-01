using SharedStuff;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;

public class Routines
{
    /// <summary>
    /// Write to a TcpClient locking it
    /// </summary>
    /// <param name="pClient"></param>
    /// <param name="pMessage"></param>
    public static Boolean WriteLocking(TcpClient pClient, String pMessage)
    {
        Boolean Error = false;

        if (pClient == null || pMessage == null || pMessage == String.Empty)
            return true;
        
        lock (pClient)
        {
            try
            {
                (new BinaryWriter(pClient.GetStream())).Write(pMessage);
            }
            catch (Exception catchedException)
            {
                SmartDebug.DWL(catchedException.Message);
                SmartDebug.DWL(catchedException.StackTrace);
                Error = true;
            }
        }

        return Error;
    }

    /// <summary>
    /// Is it a valid password?
    /// </summary>
    /// <param name="pPassword"></param>
    /// <returns></returns>
    public static Boolean ValidPassword(String pPassword)
    {
        return pPassword != null 
            && pPassword != String.Empty 
            && pPassword.Length > 0 
            && pPassword.Length <= Constants.MAX_PASSWORD_LENGTH;
    }

    /// <summary>
    /// Is it a valid port number?
    /// </summary>
    public static Boolean ValidPortNumber(String pPort)
    {
        return pPort != null
            && pPort != String.Empty
            && UInt16.Parse(pPort) > 0
            && UInt16.Parse(pPort) <= UInt16.MaxValue;
    }

    /// <summary>
    /// Convert file bytes to a new file
    /// </summary>
    /// <param name="pFileBytes"></param>
    /// <returns></returns>
    public static String BytesToFile(String pFilename, Byte[] pFileBytes, String pDestDir)
    {
        String rFilename = null;
        FileStream lFileStream = null;
        Boolean lLoopEnd = false;

        while (lLoopEnd == false)
        {
            try
            {
                rFilename = pDestDir + pFilename;
                lFileStream = new FileStream(rFilename, FileMode.CreateNew, FileAccess.Write);
                lLoopEnd = true;
                lFileStream.Write(pFileBytes, 0, pFileBytes.Length);
                lFileStream.Close();
            }
            catch { }
        }

        
        return rFilename;
    }

    /// <summary>
    /// File to byte array
    /// </summary>
    /// <param name="pFilename"></param>
    /// <returns></returns>
    public static Byte[] FileToBytes(String pFilename)
    {
        Byte[] rFileBytes = null;
        try
        {
            FileStream lFileStream = new FileStream(pFilename, FileMode.Open, FileAccess.Read);

            if (lFileStream.Length > 0)
            {
                // Creates a byte array of file stream length
                rFileBytes = new Byte[lFileStream.Length];

                // Reads block of bytes from stream into the byte array
                lFileStream.Read(rFileBytes, 0, System.Convert.ToInt32(lFileStream.Length));
            }
            // Closes the File Stream
            lFileStream.Close();
        }
        catch
        {
            rFileBytes = null;
        }
        
        return rFileBytes;
    }

}