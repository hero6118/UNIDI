namespace UNIONTEK.API.Models.Request
{
    //public class CheckoutRequest
    //{
    //    public bool Status { get; set; }
    //    public string? Message { get; set; }
    //    public CheckoutResult Result { get; set; }
    //}
    public class CheckoutRequest
    {
        public int Wallet { get; set; }
        public string? Code { get; set; }
        public string? Note { get; set; }
        public double Amount { get; set; }
        public int Type { get; set; }
    }
}
