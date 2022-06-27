using System.Text.Json.Serialization;

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
        [JsonInclude]
        public string Message { get; set; }

        [JsonInclude]
        public bool Success { get; set; }

        [JsonInclude]
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