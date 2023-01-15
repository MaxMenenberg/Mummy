using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mummy
{
    public class RecentActivityLogEntry
    {

        public string? action { get; set; }

        public DateTime time { get; set; }

        public string? input { get; set; }

        public string? ouput { get; set; }

        public void createFromLogString(string logString) {
            string[] LogParams = logString.Split(new char[] { ',' });

            action = LogParams[0];

            time = Convert.ToDateTime(LogParams[1]);

            input = LogParams[2];

            ouput = LogParams[3];
        }
    }
}
