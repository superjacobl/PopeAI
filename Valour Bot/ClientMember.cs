using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using PopeAI;

namespace PopeAI
{
    public class ClientMember
    {

        public ulong planet_id { get; set;}
        public ulong id { get; set; }
        public ulong user_id { get; set; }
        public string Nickname { get; set; }
        public string member_Pfp {get; set;}
        

    }
}
