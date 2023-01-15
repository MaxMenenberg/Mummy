using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Mummy
{
    public static class Utils
    {

        public static byte[] StringToByteArray(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        public static string GetFileFromDialog(string filter = "all") {
            string fileName = "default";

            var dialog = new Microsoft.Win32.OpenFileDialog();

            if (filter.Equals("key"))
            {
                dialog.Filter = "Key Files(.key)|*.key|All Files(*.*)|*.*";
            }
            else if (filter.Equals("sig"))
            {
                dialog.Filter = "Digital Signature (.sig)|*.sig|All Files(*.*)|*.*";
            }
            else if (filter.Equals("ver"))
            {
                dialog.Filter = "Signature Verification Key (.ver)|*.ver|All Files(*.*)|*.*";
            }
            else if (filter.Equals("crypt")) {
                dialog.Filter = "Encrypted Files (.crypt)|*.crypt|All Files(*.*)|*.*";
            }

            else
            {
                dialog.Filter = "All Files(*.*)|*.*";
            }

            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                fileName= dialog.FileName;
            }

            return fileName;
        }

        public static byte[] plaintextPwTo265bitKey(string pw)
        {
            byte[] pwBytes = Encoding.ASCII.GetBytes(pw);
            SHA512 hashFunc = SHA512.Create();
            byte[] hashResults = hashFunc.ComputeHash(pwBytes);

            byte[] hashRes265Bit = new byte[32];
            for (int n = 0; n < 32; n++)
            {
                hashRes265Bit[n] = hashResults[n];
            }

            return hashRes265Bit;
        }

        public static void clearActivityLog()
        {
            try
            {
                if (File.Exists("RecentActivityLog.csv")) {
                    File.Delete("RecentActivityLog.csv");
                    using (File.Create("RecentActivityLog.csv")) { 

                    }
                }
            }
            catch {
                Console.WriteLine("Could not clear activity log");
            }
        }

        public static void writeEntryToLog(RecentActivityLogEntry LogEntry, bool appendEntry) {

            string logString = LogEntry.action + "," + LogEntry.time.ToString() + "," +
                LogEntry.input?.ToString() + "," + LogEntry.ouput?.ToString();

            if (appendEntry)
            {
                using (StreamWriter sw = File.AppendText("RecentActivityLog.csv"))
                {
                    sw.WriteLine(logString);
                }
            }
            else
            {
                using (StreamWriter sw = new StreamWriter("RecentActivityLog.csv"))
                {
                    sw.WriteLine(logString);
                }
            }
        
        }

        public static ObservableCollection<RecentActivityLogEntry> importLogActivity(string logActivityFile) {

            ObservableCollection<RecentActivityLogEntry> log = new ObservableCollection<RecentActivityLogEntry>();

            using (StreamReader sw = new StreamReader(logActivityFile)) {
                string line;
                while ((line = sw.ReadLine()) != null) {
                    RecentActivityLogEntry tempEntry = new RecentActivityLogEntry();
                    tempEntry.createFromLogString(line);
                    if (log.Count == 0)
                    {
                        log.Add(tempEntry);
                    }
                    else
                    {
                        log.Insert(0, tempEntry);
                    }
                }
            }
            return log;
        }

        }
}
