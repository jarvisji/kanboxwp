using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kanboxwp.Entities
{
    public class KbAccountInfo
    {
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("spaceQuota")]
        public long SpaceQuota { get; set; }
        [JsonProperty("spaceUsed")]
        public long SpaceUsed { get; set; }
        [JsonProperty("emailIsActive")]
        public int EmailIsActive { get; set; }
        [JsonProperty("phoneIsActive")]
        public int PhoneIsActive { get; set; }
    }
}
