using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mummy
{
    internal class RecentActivityLogEntry
    {
        public enum cryptoAction { 
            Encryption,                         //0
            Decryption,                         //1
            KeyExchange,                        //2 
            DigitalSignature,                   //3
            DigitalSignatureVerification,       //4  
        }

        public DateTime time { get; set; }

        public string? input { get; set; }

        public string? ouput { get; set; }
    }
}
