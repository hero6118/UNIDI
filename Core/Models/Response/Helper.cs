namespace Core.Models.Response
{
    public class StatusRequest
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public dynamic Result { get; set; }
    }
    public class LoginResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public bool? Require2FA { get; set; }
        public dynamic Result { get; set; }
    }
}
