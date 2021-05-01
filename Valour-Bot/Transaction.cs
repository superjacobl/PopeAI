using System;
using Newtonsoft.Json; 

namespace PopeAI
{
    class Transaction
    {
        public ulong From {get; set;}

        public ulong To {get; set;}

        public double Amount {get; set;}

        public DateTime Timestamp {get; set;}

        public double Fee {get; set;}

        public string ConvertToJson() {
            return JsonConvert.SerializeObject(this);
        }
    }
}