using System.Collections.Generic;

namespace Core.Models.Response
{
    public class DefaultResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public dynamic Result { get; set; }
    }

    public class BussinesInfoShppResponse
    {
        public bool Check { get; set; }
        public string Ms { get; set; }
        public BusinessLicense Data { get; set; }
    }


    public class ListBalanceResponse
    {
        public bool Check { get; set; }
        public string Ms { get; set; }
        public List<ListCoin> Data { get; set; }
        public double EstimateUSD { get; set; }
    }

    public class UsingtResponse
    {
        public bool Check { get; set; }
        public int Status { get; set; }
        public string Ms { get; set; }
        public dynamic Data { get; set; }
        public int Total { get; set; }

    }

    public class CateResponse
    {
        public bool Check { get; set; }
        public string Ms { get; set; }
        public dynamic Data { get; set; }
        public int Total { get; set; }

    }

    public class ProductResponse
    {
        public bool Check { get; set; }
        public string Ms { get; set; }
        public dynamic Data { get; set; }
        public int Total { get; set; }
        public string Redirect { get; set; }
    }

    public class WithDrawResponse
    {
        public bool Check { get; set; }
        public string Ms { get; set; }
        public dynamic Data { get; set; }
        public int Total { get; set; }
        public bool? Require2FA { get; set; }

    }
    public class TransferResponse
    {
        public bool Check { get; set; }
        public string Ms { get; set; }

        public dynamic Data { get; set; }
        public int Total { get; set; }
        public bool? Require2FA { get; set; }
    }
    public class BasicResponse
    {
        public bool Check { get; set; }
        public string Ms { get; set; }
        public dynamic Data { get; set; }
    } 
    public class ProductSellerResponse
    {
        public bool Check { get; set; }
        public int? Status { get; set; }
        public string Ms { get; set; }
        public dynamic Data { get; set; }
        public int? Total { get; set; }
    }
}
