using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kanboxwp.Entities
{
    public class KbListContentInfo
    {
        public string FullPath { get; set; }
        public DateTime ModificationDate { get; set; }
        public long FileSize { get; set; }
        public bool IsFolder { get; set; }
        public bool IsShared { get; set; }
    }
}
