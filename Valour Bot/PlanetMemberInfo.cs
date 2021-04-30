using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using PopeAI;

namespace PopeAI
{
    public class PlanetMemberInfo
    {

        public ClientMember member { get; set;}
        public string state { get; set; }
        public List<ulong> roleIds {get; set;}

    }
}
