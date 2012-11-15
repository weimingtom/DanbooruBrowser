﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace booruReader.Helpers.DebugUtils
{
    /// <summary>
    /// This class is used to log the events throughout the booruReader
    /// </summary>
    class Logger
    {
        private static Logger instance;
        private string _path;

        private Logger()
        {
            _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BooruReader") + @"\Log.txt";
        }

        public static Logger Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Logger();
                }
                return instance;
            }
        }

        /// <summary>
        /// Basic logging function
        /// </summary>
        /// <param name="functionName">Name of the function that the call is in</param>
        /// <param name="message">Error message</param>
        /// <param name="extraDetails">Extra data</param>
        public void LogEvent(string functionName, string message, string extraDetails = null)
        {
            CheckLogFile();

            string lineOut = TimeStamp() + "Current call: " + functionName + " Result: " + message;

            if (extraDetails != null)
                lineOut += " Misc details: " + extraDetails;

            lineOut += ";" + Environment.NewLine;

            File.AppendAllText(_path, lineOut);
        }

        /// <summary>
        /// Log current event with current runtime parameters
        /// </summary>
        /// <param name="functionName">Name of the function that the call is in</param>
        /// <param name="message">Error message</param>
        /// <param name="extraDetails">Extra data</param>
        public void LogEventWithRuntime(string functionName, string message, string extraDetails = null)
        {
            //Note:NOT DONE!
            CheckLogFile();


        }

        /// <summary>
        /// Checks if log file exists and creates one if it doesnt
        /// </summary>
        private void CheckLogFile()
        {
            if (!File.Exists(_path))
            {
                File.Create(_path).Dispose();
            }
        }

        /// <summary>
        /// Returns formatted current time and date timestamp
        /// </summary>
        private string TimeStamp()
        {
            string retVal = DateTime.Now.ToString() + ": ";

            return retVal;
        }
    }
}
