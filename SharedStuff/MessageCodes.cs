using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedStuff
{
    public class MessageCodes
    {
        public const String 
            // 00:Password:Name:ClipboardPort:VideoPort
            LOGIN_REQUEST =                     "00",
            LOGIN_RESPONSE_OK =                 "01",
            LOGIN_RESPONSE_ERROR =              "02",
            LOGIN_RESPONSE_CHANGE_NAME =        "03",
            LOGIN_RESPONSE_CHANGE_PASSWORD =    "04",

            // 10:Message
            MSG_CLIENT2SERVER_BROADCAST =       "10",
            // 11:Name:Message
            MSG_CLIENT2SERVER_PRIVATE =         "11",
            // 12:Name:Message
            MSG_SERVER2CLIENT =                 "12",

            // 20:Name
            STATUS_USER_CONNECTED =             "20",
            // 21:Name
            STATUS_USER_DISCONNECTED =          "21",

            LOGOUT_REQUEST =                    "50",
            LOGOUT_RESPONSE_OK =                "51",
            LOGOUT_RESPONSE_ERROR =             "52"
            ;
        public const Int32
            LOGIN_REQUEST_FIELDS =                  5,
            LOGIN_RESPONSE_FIELDS =                 1,

            MSG_CLIENT2SERVER_BROADCAST_FIELDS =    2,
            MSG_CLIENT2SERVER_PRIVATE_FIELDS =      3,
            MSG_SERVER2CLIENT_FIELDS =              3,

            STATUS_FIELDS =                         2,
            
            LOGOUT_FIELDS =                         1
            ;
    }
}
