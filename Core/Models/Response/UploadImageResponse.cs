using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Response
{
    public class UploadImageResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public UploadImageResult Result { get; set; }
       
    }
    public class UploadImageResult
    {
        public string Url { get; set; }
        public List<string> Urrl { get; set; }
        public string Hash { get; set; }
        public string IPFS { get; set; }

    }
    //
    

    public class UploadImageListResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public List<string> url { get; set; }
        public List<UploadImageResult> Result { get; set; }
    }

    public class UploadImageResultee
    {
        public string Url { get; set; }
        public List<string> Urrl { get; set; }
        public string Hash { get; set; }
        public string IPFS { get; set; }

    }

    public class UploadVideoResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public UploadVideoResult Result { get; set; }

    }
    public class UploadVideoResult
    {
        public string PreviewUrl { get; set; }
        public string StreamUrl { get; set; }
        public string Hash { get; set; }
        public string IPFS { get; set; }

    }
}
