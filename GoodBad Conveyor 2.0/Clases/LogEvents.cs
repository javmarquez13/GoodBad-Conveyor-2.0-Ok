using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace GoodBad_Conveyor_2._0
{
    class LogEvents
    {
        public static void RegisterEvent(int LANE, string LOG_STRING) 
        {
            string logText = string.Empty;
            if(LANE == 0) logText = DateTime.Now.ToString() + " - DAQ IO:" + LOG_STRING;
            if (LANE == 1 || LANE == 2) logText = DateTime.Now.ToString() + " - LANE " + LANE + ": " + LOG_STRING;
            if(LANE == 13) logText = DateTime.Now.ToString() + " - OK: " + LOG_STRING;
            if(LANE == 14) logText = DateTime.Now.ToString() + " - ERROR: " + LOG_STRING;

            string LogName = DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + "_GoodBadConveyor.txt";
            string LogPath = @"C:\GoodBadConveyor\AppLogFiles\"+LogName;

            if (!Directory.Exists(@"C:\GoodBadConveyor\AppLogFiles\")) Directory.CreateDirectory(@"C:\GoodBadConveyor\AppLogFiles\");

            try 
            {
                using (StreamWriter _sr = File.AppendText(LogPath))
                {
                    _sr.WriteLine(logText);
                    _sr.WriteLine("");
                }
            }
            catch(IOException ex)
            {

            }        
        }
    }
}
