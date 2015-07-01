using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedStuff
{
    public class Constants
    {
        // Generic Int32 constants
        public const Int32 
            MS_READ_TIMEOUT = 30000,
            MS_CLIPBOARD_TIMEOUT = 1000,
            MS_VIDEO_BIGFRAME = 5000,
            MS_VIDEO_UPDATE = 20,
            MS_CURSOR_POSITION = 500,
            MS_PREVIEW_AREA_SELECTOR = 50,
            MAX_NICKNAME_LENGTH = 8,
            MAX_PASSWORD_LENGTH = 8,
            NUM_ROWS_GRID = 4,
            VIDEO_CLIENT_WIDTH_OFFSET = 32,
            VIDEO_CLIENT_HEIGHT_OFFSET = 58,
            MS_SLEEP_TIMEOUT_BG_THREADS = 2000,
            MS_SLEEP_TIMEOUT_DIFFFRAME = 100,
            PIXEL_STEP = 10;
        
        // Generic Int16 constants
        public const Int16
            CLIPBOARD_PORT_OFFSET = 1;

        // Generic String constants
        public const String
            DEFAULT_SERVER = "localhost",
            DEFAULT_SERVER_PORT = "5000",
            // before to change this check password max length above 
            DEFAULT_PASSWORD = "password",
            CLIPBOARD_FILES_DIR = "ClipFiles";
    
        // Clipboard type constants
        public const Byte
            TYPE_TEXT = 0,
            TYPE_BMP = 1,
            TYPE_FILE = 2;

        // Video type constants
        public const Byte
            BIGFRAME = 0,
            DIFFFRAME = 1,
            CURSOR = 2;

        public const float 
            PEN_WIDTH = 5;
    }
}
