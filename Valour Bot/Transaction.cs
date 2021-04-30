using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;  
namespace PopeAI
{
    class Transaction
    {
        public ulong From {get; set;}

        public ulong To {get; set;}

        public double Amount {get; set;}

        public DateTime timestamp {get; set;}

        public double Fee {get; set;}

        public string ConvertToJson() {
            return JsonConvert.SerializeObject(this);
        }
    }
}