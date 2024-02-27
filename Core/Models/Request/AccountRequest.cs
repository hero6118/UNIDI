using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace Core.Models.Request
{
    public class RegisterRequest
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Paasword must from {2} character.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
      //  [Compare("Password", ErrorMessage = "Confirm password not correct.")]
        public string ConfirmPassword { get; set; }
       
        public string FullName { get; set; }
        [MaxLength(12, ErrorMessage = "Max length 12")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "PhoneNumber must be numeric")]
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        [Required]
        public string Email { get; set; }
        public string Sponsor { get; set; }
        public int? Type { get; set; }
        public int? CountryId { get; set; }
    }
    public class LoginRequest
    {
        public string UserName { get; set; }
        public string Email { get; set; }
     //   public string FullName { get; set; }
        public string Avatar { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string Passcode { get; set; }
        public bool? RememberMe { get; set; }
    }
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string UserName { get; set; }
    }

    public class UpdateUserRequest
    {
        public string UserId { get; set; }
        public string RoleId { get; set; }
        public int Status { get; set; } 
    }

    public class ProfileRequest
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; } 
        public string Email { get; set; }
        public DateTime? BirthDay { get; set; }
        public string gender { get;set; }
        public IFormFile Avatar { get; set; }   
        public string Country { get; set; } 
        public bool? lockstatus { get; set; }
    }

    public class ProfileHomeRequest
    {

        public string FullName { get; set; }
        [MaxLength(12, ErrorMessage = "Max length 12")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "PhoneNumber must be numeric")]
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime? BirthDay { get; set; }
        public string Gender { get; set; }
        public IFormFile Avatar { get; set; }
    }
    public class GoogleAuthenRequest
    {
        public string Password { get; set; }
        public string Code { get; set; }
        public bool? Enable { get; set; }
    }

    public class AccountBankRequest
    {
        public string FullName { get; set; }
        public string IdCard { get; set; }  
        public string BankName { get; set; }
        public int? NumberAccount { get; set; } 
        public string NameAccount { get; set; }
    }

    public class ResetPasswordRequest
    {
        [Required]
        [StringLength(100, ErrorMessage = "Password must from {2} character.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password confirm is not correct")]
        public string ConfirmPassword { get; set; }
        public string ResetPasswordKey { get; set; }
    }
}
