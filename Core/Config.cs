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
    
    public partial class Config
    {
        public int Id { get; set; }
        public string EmailAccount { get; set; }
        public string EmailPassword { get; set; }
        public string SMTPServer { get; set; }
        public Nullable<int> SMTPPort { get; set; }
        public string CompanyName { get; set; }
        public Nullable<double> FeeWithdrawPercent { get; set; }
    }
}
