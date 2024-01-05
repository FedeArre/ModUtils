using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    internal class CustomLogger
    {
        private static List<string> lines;

        public static void AddLine(string origin, string line, bool dontShowInLog = false)
        {
            string message = $"[{DateTime.Now.ToString("HH:mm:ss")} - {origin}] {line}";

            Debug.Log(message);

            if(!dontShowInLog)
                lines.Add(message);

            if (lines.Count > 15)
                lines.RemoveAt(0);
        }

        public static List<string> GetLines()
        {
            return lines;
        }
    }
}
