using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kanboxwp.Entities
{
    public class KbListInfo : KbStatusInfo
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }

        private List<KbListContentInfo> _Contents = new List<KbListContentInfo>();
        [JsonProperty("contents")]
        public List<KbListContentInfo> Contents
        {
            get
            {
                return _Contents;
            }
        }
    }
}
