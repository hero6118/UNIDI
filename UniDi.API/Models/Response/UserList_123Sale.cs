using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UNIONTEK.API.Models.Response
{
    public class UserList_123Sale
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PasswordHash { get; set; }
        public Guid? IdSponsor { get; set; }
        public string? SponsorAddress { get; set; }
        public int? TangSponsor { get; set; }
        public DateTime DateCreate { get; set; }
        public string? Address { get; set; }
        public string? Commune { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }
        public bool? Activity { get; set; }
    }
    public class UserList_SE_Request
    {
        public bool Status { get; set; }
        public string? Message { get; set; }
        public List<UserList_SE>? Result { get; set; }
    }
    public class UserList_SE
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PasswordHash { get; set; }
        public Guid? IdSponsor { get; set; }
        public Guid? IdPlacement { get; set; }
        public DateTime DateCreate { get; set; }
        public string? Street { get; set; }
        public string? Commune { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }
        public bool Activity { get; set; }
        public string? GoogleAuthenticatorSecretKey { get; set; }
        public bool? IsGoogleAuthenticatorEnabled { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountHolder { get; set; }
        public string? BankBranch { get; set; }
        public string? BankAccountNumber { get; set; }
    }
}
