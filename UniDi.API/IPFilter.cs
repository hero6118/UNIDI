using Microsoft.Extensions.Options;

namespace UNIONTEK.API
{
    public class IPFilter
    {
        private readonly RequestDelegate _next;
        private readonly ApplicationOptions _applicationOptions;
        public IPFilter(RequestDelegate next, IOptions<ApplicationOptions> applicationOptionsAccessor)
        {
            _next = next;
            _applicationOptions = applicationOptionsAccessor.Value;
        }
        public List<string> _WhiteList = new() {
            "/users/login", "/api/image", "/api/video", "/api/previewvideo", "/api/sitemap", "/api/sitemap.xml", "/location/",
            "/point/autoapprovebanking"
        };
        public async Task Invoke(HttpContext context)
        {
            if (context == null) return;

            var path = context.Request.Path.ToString().ToLower();
            if (!_WhiteList.Any(p => path.StartsWith(p)))
            {
                if (context != null && context.Connection.RemoteIpAddress != null)
                {
                    var ipAddress = context.Connection.RemoteIpAddress;
                    string remoteIpAddress = ipAddress.MapToIPv4().ToString();
                    if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
                        remoteIpAddress = context.Request.Headers["X-Forwarded-For"].ToString().Trim();
                    if (_applicationOptions != null && _applicationOptions.Whitelist != null)
                    {
                        List<string> whiteListIPList = _applicationOptions.Whitelist;
                        var isInwhiteListIPList = whiteListIPList.Any(p => p.Equals(remoteIpAddress));
                        if (!isInwhiteListIPList)
                        {
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync("{\"status\":false, \"message\":\"Access Denied\", \"result\": \"Your IP: " + remoteIpAddress + "\"}");
                            return;
                        }
                    }
                }
            }
            if (context != null)
            {
                try
                {
                    if (!context.Response.HasStarted)
                        await _next.Invoke(context);
                }
                catch (Exception ex)
                {
                    if (!ex.Message.ToLower().Contains("statuscode cannot be set because the response has already started"))
                    {
                        var token = context.Request.Headers["Authorization"];
                        var body = "";
                        using (var reader = new StreamReader(context.Request.Body))
                        {
                            body = reader.ReadToEnd();
                        }
                        //await Tool.SendTelegram("IPFilter Lỗi: " + context.Request.Host + context.Request.Path + "\n" + ex.Message + "\n" + ex.InnerException?.Message + "\n" + ex.InnerException?.InnerException?.Message + "Token: " + token + "\nBody: " + body, "error");
                    }
                }
            }
        }
    }
}
