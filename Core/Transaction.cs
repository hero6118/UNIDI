//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Core
{
    using System;
    using System.Collections.Generic;
    
    public partial class Transaction
    {
        public System.Guid Id { get; set; }
        public string UserId { get; set; }
        public string UserIdInccured { get; set; }
        public string ShopId { get; set; }
        public string Currency { get; set; }
        public string Code { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<bool> ConfirmPayment { get; set; }
        public Nullable<double> Amount { get; set; }
        public Nullable<double> RealAmount { get; set; }
        public Nullable<double> Fee { get; set; }
        public string Txid { get; set; }
        public Nullable<System.DateTime> DateCreate { get; set; }
        public Nullable<int> Type { get; set; }
        public Nullable<int> CommissionType { get; set; }
        public string Note { get; set; }
    }
}
