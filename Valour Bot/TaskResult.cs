using System.Net.Http;
using Newtonsoft.Json;

namespace PopeAI 
{
    public class TaskResult : TaskResult<string>
    {
        public TaskResult(bool success, string response) : base(success, response, null)
        {

        }
    }

    public class TaskResult<T>
    {
        [JsonProperty]
        public string Message { get; set; }

        [JsonProperty]
        public bool Success { get; set; }

        [JsonProperty]
        public T Data { get; set; }

        public TaskResult(bool success, string response, T data)
        {
            Success = success;
            Message = response;
            Data = data;
        }

        public override string ToString()
        {
            if (Success)
            {
                return $"[SUCC] {Message}";
            }

            return $"[FAIL] {Message}";
        }
    }
}