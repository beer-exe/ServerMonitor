namespace ServerMonitorApp.Application.Wrappers
{
    public class Response<T>
    {
        public bool Succeeded { get; set; }
        public string? Message { get; set; }
        public string[]? Errors { get; set; }
        public T? Data { get; set; }

        public Response() { }

        public Response(T data, string? message = null)
        {
            Succeeded = true;
            Message = message;
            Data = data;
            Errors = null;
        }

        public Response(string message)
        {
            Succeeded = false;
            Message = message;
            Errors = new[] { message };
        }

        public Response(string message, string[] errors)
        {
            Succeeded = false;
            Message = message;
            Errors = errors;
        }
    }
}
