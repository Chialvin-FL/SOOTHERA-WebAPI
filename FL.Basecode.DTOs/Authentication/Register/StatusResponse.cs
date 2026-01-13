namespace FL.Basecode.DTOs.Authentication
{
    public class StatusResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public object? Data { get; set; }
    }

}
