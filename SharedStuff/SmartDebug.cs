using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace SharedStuff
{
    public class SmartDebug
    {
        public static void DWL(String Message)
        {
            String MethodName = (new StackTrace()).GetFrame(1).GetMethod().Name;
            Debug.WriteLine("[DEBUG:" + Thread.CurrentThread.Name + ":" + MethodName + "] " + Message);
        }
    }
}
