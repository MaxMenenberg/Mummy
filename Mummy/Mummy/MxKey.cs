using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace Mummy
{

    public enum MxKeyType
    {
        NonKeyDefault,  //0
        AES,            //1
        ECDH,           //2
        ECDSA           //3
    }

    internal class MxKey
    {
        private string keyFileStart = "-----------BEGIN KEY----------";
        private string keyFileEnd = "-----------END KEY----------";
        private string[] keyTypeList = new string[3] { "AES", "ECDH", "ECDSA" };

        public MxKeyType KeyType;

        public byte[]? Key { get; set; }

        public bool IsPrivate { get; set; }

        public string KeyToHexString() {
            if (Key != null)
            {
                return Convert.ToHexString(Key);
            }
            else {
                throw new ArgumentNullException("Value of Key is null");
            }
        }

        public bool ExportKeyToFile(string filePath) {

            if (File.Exists(filePath)) {
                return false;
            }

            using (File.Create(filePath)) { 
            }
                
            string[] keyFileLines = new string[5]
            { keyFileStart,
                "Private: " + IsPrivate.ToString(),
                "Type: " + keyTypeList[(int)KeyType-1],
                "Key: " + KeyToHexString(), "" +
            keyFileEnd };

            using (StreamWriter sw = new StreamWriter(filePath)) {
                for (int n = 0; n < keyFileLines.Length; n++) {
                    sw.WriteLineAsync(keyFileLines[n]);
                }
            }
            return true;
        
        }

        public bool ImportKeyFromFile(string filePath) {

            if (!File.Exists(filePath)) {
                return false;
            }

            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string l1 = sr.ReadLine();
                    string l2 = sr.ReadLine();
                    string l3 = sr.ReadLine();
                    string l4 = sr.ReadLine();


                    string[] l2Words = l2.Split(' ');
                    if (l2Words[1] == "True")
                    {
                        IsPrivate = true;
                    }
                    else {
                        IsPrivate = false;
                    }

                    string[] l3Words = l3.Split(' ');
                    string keyTypeTemp = l3Words[1];
                    for (int n = 0; n < keyTypeList.Length; n++) {
                        if (keyTypeTemp.Equals(keyTypeList[n]))
                        {
                            KeyType = (MxKeyType)(n+1);
                        }
                        break;
                    }

                    string[] l4Words = l4.Split(' ');
                    Key = Utils.StringToByteArray(l4Words[1]);

                }
            }

            catch {
                return false;
            }
            return true;

        }

    }
}
