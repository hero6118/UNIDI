using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Request
{
    public class AddTransactionRequest
    {
        public int TransactionType { get; set; }
        public string Currency { get; set; }
        public double? Amount { get; set; }
        public double Fee { get; set; }
        public string Code { get; set; }
        public DateTime? DateCreate { get; set; }
        public bool Status { get; set; }
        public bool ConfirmPayment { get; set; }
        public int? CommissionType { get; set; }
        public string Tx { get; set; }
        public string Note { get; set; }
    }
    public class BuyPackageListCoinRequest
    {
        public Guid ListCoinId { get; set; }
        public int PackageId { get; set; }
        public string Chain { get; set; }
        public string Symbol { get; set; }
    }

    public class WithdrawRequest
    {
        public string WalletAddress { get; set; }
        public string Currency { get; set; }
        public string Chain { get; set; }

 
        public double? Amount { get; set; }  

        public string Code2FA { get; set; }

    }
    public class TransferRequest
    {
        public string WalletCurrency { get; set; }
        public string RecipientUsername { get; set; }
    //    public string RecipientName { get; set; }
        public double? Amount { get; set; }
        public string chainname { get; set; }
        public string Code2FA { get; set; }

    }

}
