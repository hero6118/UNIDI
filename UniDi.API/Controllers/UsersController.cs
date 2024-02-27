using Core;
using Core.Models.Request;
using Core.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using X.PagedList;
using Google.Authenticator;
using Swashbuckle.AspNetCore.Annotations;

namespace UniDi.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        public string? _host;
        public IHttpContextAccessor? _accessor;

        public UsersController(IHttpContextAccessor accessor)
        {
            if (accessor != null && accessor.HttpContext != null)
                _host = accessor.HttpContext.Request.Host.ToString();
        }
        [AllowAnonymous]
        [HttpPost("[action]")]
        public LoginResponse Login([FromForm] LoginRequest model, string? type = "")
        {
            if (_host == null)
                return new LoginResponse { Status = false, Message = "Please reload the page" };
            try
            {
                if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
                    return new LoginResponse { Status = false, Message = "Field cannot be left blank" };

                using var de = new DataEntities();
                if (!de.AspNetUsers.Any(p => p.UserName == model.UserName))
                    return new LoginResponse { Status = false, Message = "Username does not exist" };

                var user = de.AspNetUsers.Single(p => p.UserName == model.UserName);
                var now = Tool.Curtime();
                var today = new DateTime(now.Year, now.Month, now.Day);
                if (user.LockoutEndDateUtc != null && user.LockoutEndDateUtc > now)
                {
                    var time = (Convert.ToDateTime(user.LockoutEndDateUtc) - now).TotalSeconds;
                    return new LoginResponse { Status = false, Message = "The account is temporarily locked due to entering the wrong password many times. Log in again in " + Math.Round(time) + " second." };
                }
                if (user.Lock == true)
                    return new LoginResponse { Status = false, Message = "Account has been locked. Please contact support." };

                var hasher = new PasswordHasher<string>();
                var check = hasher.VerifyHashedPassword(model.UserName, user.PasswordHash, model.Password);

                if (check == 0)
                {
                    //var check1 = C_Config.PassDefault == model.Password;
                    //if (!check1)
                    {
                        if (user.AccessFailedCount < 5)
                        {
                            user.AccessFailedCount += 1;
                            de.SaveChanges();
                            return new LoginResponse { Status = false, Message = "Incorrect account or password" };
                        }
                        else
                        {
                            user.LockoutEndDateUtc = now.AddHours(1);
                            user.AccessFailedCount = 0;
                            de.SaveChanges();
                            return new LoginResponse { Status = false, Message = "Account is temporarily locked for 1 hour" };
                        }

                    }
                }
                user.LockoutEndDateUtc = now;
                user.AccessFailedCount = 0;

                de.SaveChanges();

                var exp = (model.RememberMe == true ? now.AddMonths(1) : now.AddDays(1));

                var claim = new List<Claim> {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim("fullname", user.FullName),
                };
                var jwt = Tool.EnJwtToken(claim, exp);
                claim = new List<Claim> {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, model.UserName),
                    new Claim("fullname", user.FullName),
                    new Claim("type", Enum_UserType.Label[user.Type ?? 0]),
                    new Claim("token", jwt)
                };
                if (de.AspNetUserRoles.Any(p => p.UserId == user.Id))
                {
                    var role = (from ur in de.AspNetUserRoles.Where(p => p.UserId == user.Id).ToList()
                                from r in de.AspNetRoles.Where(p => p.Id == ur.RoleId).ToList()
                                select new Claim(ClaimTypes.Role, r.Name)).ToList();
                    claim.AddRange(role);
                }

                jwt = Tool.EnJwtToken(claim, exp);

                //    {
                //        Id = user.Id,
                //        Active = user.Active,
                //        AgencyActive = user.AgencyActive,
                //        AgencyDiscount = user.AgencyDiscount,
                //        AgencySetting = user.AgencySetting,
                //        Avatar = user.Avatar,
                //        DateCreate = user.DateCreate,
                //        FullName = user.FullName,
                //        GoogleAuthenticator = user.GoogleAuthenticator,
                //        IsVerify = user.UserIdentity != null ? user.UserIdentity.IsVerify : null,
                //        Lock = user.Lock,
                //        AccessFailedCount = user.AccessFailedCount,
                //        LockEndDateUtc = user.LockEndDateUtc,
                //        LockTransfer = user.LockTransfer,
                //        LockWithdraw = user.LockWithdraw,
                //        Sales = user.Sales,
                //        Type = user.Type,
                //        UserName = user.UserName,
                //        UserType = user.UserType,
                //        GoogleAuthenticatorSecretKey = user.GoogleAuthenticatorSecretKey,
                //        Password = user.Password,
                //        Password2 = user.Password2,
                //        UserContact = user.UserContact,
                //        UserIdentity = user.UserIdentity,
                //        UserAffiliate = user.UserAffiliate,
                //        UserAddress = user.UserAddress
                //    };

                if (user.IsGoogleAuthenticator == true)
                {
                    if (string.IsNullOrEmpty(model.Passcode))
                        return new LoginResponse { Status = false, Message = "Enter 2FA code", Require2FA = true };

                    TwoFactorAuthenticator tfa = new();
                    bool isValid = tfa.ValidateTwoFactorPIN(user.GoogleAuthenticatorSecretKey, model.Passcode);
                
                    if (!isValid)
                        return new LoginResponse { Status = false, Message = "2FA code is incorrect" };
                }

                /* var listt = new List<AspNetRole>();


                 var o = de.AspNetUserRoles.Where(p => p.UserId == user.Id).ToList();
                 foreach(var item in o)
                 {
                     var rolee = de.AspNetRoles.Single(p => p.Id == item.RoleId);
                     listt.Add(rolee);
                 }
                 foreach(var items in listt)
                 {

                 }*/

                var o = de.AspNetUserRoles.AsNoTracking().Where(p => p.UserId == user.Id).ToList();

                var listName = new List<string>();
                foreach (var item in o)
                {
                    var rolee = de.AspNetRoles.FirstOrDefault(p => p.Id == item.RoleId);
                    if (rolee == null)
                        return new LoginResponse { Status = false, Message = "Can't found rolee" };

                    // var roleeee = de.AspNetRoles.Where(p => p.Id == item.RoleId).ToList();
                    listName.Add(rolee.Name);
                }
                var userInfo = new Dictionary<string, string>();
                var cx = de.AspNetUserRoles.Where(p => p.UserId == user.Id).ToList();

                /* userInfo.Add("FullName", user.FullName);
                 userInfo.Add("Email", user.FullName);
                 userInfo.Add("UserInfo", user.FullName);
                 userInfo.Add("UserInfo", user.FullName);
                 userInfo.Add("UserInfo", user.FullName);*/

                if (type != null)
                {
                    if (type == "seller")
                    {
                        var checkRole = false;
                        foreach (var role in listName)
                        {
                            if (role.ToLower() == "seller")
                            {
                                checkRole = true;
                                break;
                            }
                        }
                        if (checkRole == false)
                        {
                            var rolenids = de.AspNetRoles.AsNoTracking().FirstOrDefault(p => p.Name == "Seller");
                            if (rolenids == null)
                                return new LoginResponse { Status = false, Message = "Can't found role Seller!" };

                            var roleSeller = new AspNetUserRole()
                            {
                                UserId = user.Id,
                                RoleId = rolenids.Id,
                                Status = "Waiting"
                            };
                            listName.Add("Seller");
                            de.AspNetUserRoles.Add(roleSeller);

                        }
                    }
                    if (type == "pool")
                    {
                        var checkRole = false;
                        foreach (var role in listName)
                        {
                            if (role.ToLower() == "pool")
                            {
                                checkRole = true;
                                break;
                            }
                        }
                        if (checkRole == false)
                        {
                            var rolidp = de.AspNetRoles.AsNoTracking().FirstOrDefault(p => p.Name == "Pool");
                            if (rolidp == null)
                                return new LoginResponse { Status = false, Message = "Can't found role Pool!" };

                            var rolePool = new AspNetUserRole()
                                {
                                    UserId = user.Id,
                                    RoleId = rolidp.Id,
                                    Status = "Waiting"
                                };
                            listName.Add("Pool");
                            de.AspNetUserRoles.Add(rolePool);
                        }
                    }
                    de.SaveChanges();
                }

                LoginData p = new LoginData()
                {
                    Jwt = jwt,
                    //  UserInfo=userInfo
                    //  User= user1,
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Gender = user.Gender,
                    Address = user.Address,
                    phone = user.PhoneNumber,
                    Image = user.CoverImage,
                    UserName = user.UserName,
                    BirthDay = user.BirthDay,
                    Role = listName,
                    Check2FA = user.IsGoogleAuthenticator ?? false,
                    //Role2 = role2.Name,
                 //   Redirect = "../home",
                };

                return new LoginResponse { Status = true, Message = "Login successfully!", Result = p };
            }
            catch (Exception ex)
            {
                var ms = ex.Message + "\n" + ex.InnerException?.Message + "\n" + ex.InnerException?.InnerException?.Message;
                return new LoginResponse { Status = false, Message = ms };
            }
        }
        [AllowAnonymous]
        [HttpGet("[action]")]
        public StatusRequest CheckLogin(string? url = "")  // send name
        {
            if (_host == null)
                return new StatusRequest { Status = false, Message = "Please reload the page" };
            using var de = new DataEntities();
            {
                de.Configuration.LazyLoadingEnabled = false; // 1 cai doc 1 cai ghi
                de.Configuration.ProxyCreationEnabled = false;
                try
                {
                    var user = C_User.Auth(Request.Headers["Authorization"]);
                    if (user == null)
                        return new StatusRequest { Status = false, Message = "" };

                    /*    var o = de.AspNetUserRoles.Single(p => p.UserId == user.Id);
                        var rolee = de.AspNetRoles.Single(p => p.Id == o.RoleId);*/
                    var o = de.AspNetUserRoles.AsNoTracking().Where(p => p.UserId == user.Id).ToList();
                    //var check = false;
                    var listName = new List<string>();
                    foreach (var item in o)
                    {
                        // var rolee = de.AspNetRoles.FirstOrDefault(p => p.Id == item.RoleId);                      
                        var roleeee = de.AspNetRoles.Where(p => p.Id == item.RoleId).ToList();

                        foreach (var xx in roleeee)
                        {
                            listName.Add(xx.Name);
                            //var namee = xx.Name;
                            //if (namee.ToUpper() == url.ToUpper())
                            //{
                            //    check = true;
                            //    break;
                            //    // return new StatusRequest { Status = true, Message = "ok" };
                            //}
                        }
                        //var redirect = "";
                    }
                    //if (check == true)
                    {
                        return new StatusRequest { Status = true, Message = "Ok", Result = listName };
                    }
                    //else
                    //{
                    //    return new StatusRequest { Status = false, Message = "../home/login", Result = listName };
                    //}
                    /*        var redirect = "";
                            if(url != null)
                            {

                                if (rolee.Name != null)
                                {
                                    if (rolee.Name == "Pool" && url =="pool")
                                    {
                                            redirect = "";
                                        }
                                    else if (rolee.Name == "Seller" && url =="seller")
                                    {
                                            redirect = "";

                                        }
                                    else if (rolee.Name == "Admin" && url =="admin")
                                    {
                                            redirect = "";
                                        }
                                    else
                                    {
                                        redirect = "../home/login";
                                    }
                                }
                            }
                            else
                            {
                                if(rolee.Name != "Member")
                                {
                                    redirect = null;
                                }
                                else
                                {
                                    redirect = "../home/login";
                                }
                            }
                            return new StatusRequest { Status = true ,Message=redirect};*/
                }
                catch (Exception ex)
                {
                    var ms = ex.Message + "\n" + ex.InnerException?.Message + "\n" + ex.InnerException?.InnerException?.Message;
                    return new StatusRequest { Status = false, Message = ms };
                }

            }

        }
        [AllowAnonymous]
        [HttpPut("Login")]
        public LoginResponse Login2([FromBody] LoginRequest model)
        {
            return Login(model);
        }
        [AllowAnonymous]
        [HttpPost("Register")]
        public StatusRequest Register([FromForm] RegisterRequest model)
        {
            if (_host == null)
                return new StatusRequest { Status = false, Message = "Please reload the page" };
            try
            {
                if (string.IsNullOrEmpty(model.UserName)
                || string.IsNullOrEmpty(model.Password)
                //  || string.IsNullOrEmpty(model.FullName)
                || string.IsNullOrEmpty(model.Email)
                || string.IsNullOrEmpty(model.ConfirmPassword)
                )
                    return new StatusRequest { Status = false, Message = "Field cannot be left blank" };
                if (string.IsNullOrEmpty(model.Sponsor))
                    model.Sponsor = C_Config.DefaultSponsor;

                if (string.IsNullOrWhiteSpace(model.UserName) || model.UserName.Contains(' '))
                    return new StatusRequest { Status = false, Message = "Username cannot contain spaces" };

                foreach (var item in C_Config.BlackList)
                {
                    if (model.UserName.ToLower().Contains(item))
                        return new StatusRequest { Status = false, Message = "Username contains invalid words" };
                }

                if (model.UserName.Length < 6)
                    return new StatusRequest { Status = false, Message = "Username must be 6 characters or more" };
                using var de = new DataEntities();
                if (de.AspNetUsers.Any(p => p.UserName == model.UserName))
                    return new StatusRequest { Status = false, Message = "Username already exists" };
                
                var locdau = Tool.LocDau(model.UserName);
                var patem = "^[a-zA-Z0-9]*$";
                var colCDN = new Regex(patem);
                var a = colCDN.IsMatch(model.UserName);
                if (locdau != model.UserName.ToLower() || model.UserName.Contains('.') || !a)
                    return new StatusRequest { Status = false, Message = "Username cannot contain ampersands or special characters" };

                if (model.Password != model.ConfirmPassword)
                    return new StatusRequest { Status = false, Message = "Confirmation password is incorrect" };
                model.Email = model.Email.Trim();
                if (!Tool.IsValidEmail(model.Email))
                    return new StatusRequest { Status = false, Message = "Invalid email" };
                if (!model.Email.Split('@')[1].Contains('.'))
                    return new StatusRequest { Status = false, Message = "Invalid email" };

                //
                if (de.AspNetUsers.Any(p => p.Email == model.Email))
                    return new StatusRequest { Status = false, Message = "Email already exists" };
                if (!string.IsNullOrEmpty(model.Sponsor))
                {
                    if (!de.AspNetUsers.Any(p => p.UserName == model.Sponsor))
                        return new StatusRequest { Status = false, Message = "Sponsor ID does not exist" };
                }

                if(model.CountryId != null)
                {
                    if (!de.Countries.Any(p => p.Id == model.CountryId))
                        return new StatusRequest { Status = false, Message = "Country does not exist" };
                }    
                
                if (!string.IsNullOrEmpty(model.PhoneNumber))
                {
                    model.PhoneNumber = model.PhoneNumber.Replace(" ", "");
                    //   var regexPhone = "^[0-9]{10,12}*$";

                    /* if (!regexPhone.IsMatch(model.PhoneNumber))
                     {
                         return new StatusRequest { Status = false, Message = "Wrong format phone number!" };
                     }*/
                    if (de.AspNetUsers.Any(p => p.PhoneNumber == model.PhoneNumber))
                        return new StatusRequest { Status = false, Message = "Phone number already exists" };
                }
                var hasher = new PasswordHasher<string>();
                var pass = hasher.HashPassword(model.UserName, model.Password);
                //  var googleAuthenticatorSecretKey = Tool.GetRandomString(12);
                var activeCode = Tool.GetRandomNumber(6);
                var userId = Guid.NewGuid().ToString();
                var now = Tool.Curtime();
                string referer = "";
                if (!string.IsNullOrEmpty(Request.Headers["Referer"]))
                {
                    referer = new Uri(Request.Headers["Referer"].ToString()).Host;
                }
                var appUser = new AspNetUser()
                {
                    Id = userId,
                    UserName = model.UserName,

                    PasswordHash = pass,
                    AccessFailedCount = 0,
                    //   GoogleAuthenticatorSecretKey = googleAuthenticatorSecretKey,
                    Activity = false,
                    IsGoogleAuthenticator = false,
                    DateCreate = now,
                    Lock = false,
                    LockWithdraw = false,
                    Type = Enum_UserType.Member,
                    ResetPasswordKey = activeCode,
                    Email = model.Email,
                    EmailConfirmed = false,
                    PhoneNumberConfirmed = false,
                    PhoneNumber = model.PhoneNumber,
                    CoverImage = "https://cdn-icons-png.flaticon.com/512/147/147142.png",
                   
                };
                if(model.CountryId!= null)
                {
                    appUser.Country = de.Countries.FirstOrDefault(p => p.Id == model.CountryId)?.Nicename ?? null;
                }    
                if (string.IsNullOrEmpty(model.FullName))
                {
                    appUser.FullName = model.UserName;
                }
                else
                {
                    appUser.FullName = model.FullName;
                }


                var role = de.AspNetRoles.AsNoTracking().FirstOrDefault(p => p.Name == "Member");
                if (role == null)
                    return new StatusRequest { Status = false, Message = "The system is maintenance" };

                var rolee = new AspNetUserRole()
                {
                    UserId = userId,
                    RoleId = role.Id,
                    Status = "Active"
                };

                if (!string.IsNullOrEmpty(model.Sponsor))  // REFERAL LINK
                {
                    var sponsor = de.AspNetUsers.FirstOrDefault(p => p.UserName == model.Sponsor);
                    if (sponsor == null)
                        return new StatusRequest { Status = false, Message = "Sponsor ID not activated" };

                    var count = (de.AspNetUsers.Count(p => p.SponsorId == sponsor.Id) + 1);
                    var sponsorAddress = sponsor.SponsorAddress + '-' + count;
                    while (de.AspNetUsers.Any(p => p.SponsorAddress == sponsorAddress)) // check xem IdUser này đã có chưa, nếu đã bị xóa. sẽ tiếp tục +
                    {
                        count++;
                        sponsorAddress = sponsor.SponsorAddress + '-' + count;
                    }
                    appUser.SponsorAddress = sponsorAddress;
                    appUser.SponsorId = sponsor.Id;
                    appUser.SponsorFloor = (sponsor.SponsorFloor ?? 0) + 1;
                }


                // request to become partner

                de.AspNetUserRoles.Add(rolee);
                de.AspNetUsers.Add(appUser);
                de.SaveChanges();
            }
            catch (Exception ex)
            {
                var ms = ex.Message + "\n" + ex.InnerException?.Message + "\n" + ex.InnerException?.InnerException?.Message;
                return new StatusRequest { Status = false, Message = ms };
            }

            LoginData p = new LoginData()
            {

                Redirect = "/Home/Login",
            };





            return new StatusRequest { Status = true, Message = "Signup Success", Result = p };
        }
        [AllowAnonymous]
        [HttpPut("Register")]
        public StatusRequest Register2([FromBody] RegisterRequest model)
        {
            return Register(model);
        }

        //Switch to the other like Seller and Pool
        // Only for the buying using this funtion NOT all USER
        [HttpPost("[Action]")]
        public ActionResult SwitchToOther(int? sta)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!" });

            using (var de = new DataEntities())
            {
                if (sta != null)
                {

                    switch (sta)
                    {
                        case 3: //Seller
                            var rseller = de.AspNetRoles.FirstOrDefault(p => p.Name == "Seller");
                            if (rseller == null)
                                return Ok(new { check = false, ms = "Server is under maintenance" });
                            var role = new AspNetUserRole()
                            {
                                UserId = user.Id,
                                RoleId = rseller.Id.ToString(), //SELLER
                                Status = "Waiting"
                            };
                            de.AspNetUserRoles.Add(role);

                            de.SaveChanges();
                            return Ok(new { check = true, ms = "Send request to become seller success!, please waiting for Confirm!" });

                        case 4:
                            var rpool = de.AspNetRoles.FirstOrDefault(p => p.Name == "Pool");
                            if (rpool == null)
                                return Ok(new { check = false, ms = "Server is under maintenance" });
                            var role1 = new AspNetUserRole()
                            {
                                UserId = user.Id,
                                RoleId = rpool.Id, //POOL
                                Status = "Waiting"
                            };
                            de.AspNetUserRoles.Add(role1);
                            de.SaveChanges();
                            return Ok(new { check = true, ms = "Send request to pool success!, please waiting for Confirm!" });

                        default:
                            return Ok(new { check = false, ms = "Missing parameter" });
                    }

                }
                else
                    return Ok(new { check = false, ms = "ERROR" });
            }

        }

        [AllowAnonymous]
        [HttpPost("ReSendEmail")]
        public StatusRequest ReSendEmail([FromForm] ForgotPasswordRequest model)
        {
            if (_host == null)
                return new StatusRequest { Status = false, Message = "Please reload the page" };

            using var de = new DataEntities();
            try
            {
                if (string.IsNullOrEmpty(model.Email))
                    return new StatusRequest { Status = false, Message = "Please enter Email" };

                var countResend = HttpContext.Session.GetInt32("countResend") ?? 0;
                if (countResend >= 5)
                    return new StatusRequest { Status = false, Message = "You have done too many times, please try again later" };
                HttpContext.Session.SetInt32("countResend", countResend + 1);

                var userContact = de.AspNetUsers.FirstOrDefault(p => p.Email == model.Email);
                if (userContact == null)
                    return new StatusRequest { Status = false, Message = "Email does not exist in the system" };
                if (userContact.EmailConfirmed == true)
                    return new StatusRequest { Status = false, Message = "This email has been verified" };
                var code = Tool.GetRandomString(12);
                while (de.AspNetUsers.Any(p => p.ResetPasswordKey == code))
                {
                    code = Tool.GetRandomString(12);
                }
                userContact.ResetPasswordKey = code;
                de.SaveChanges();
                return new StatusRequest { Status = true, Message = "Success", Result = code };
            }
            catch (Exception ex)
            {
                return new StatusRequest { Status = false, Message = ex.Message };
            }
        }
        [AllowAnonymous]
        [HttpPut("ReSendEmail")]
        public StatusRequest ReSendEmail2([FromBody] ForgotPasswordRequest model)
        {
            return ReSendEmail(model);
        }
        [AllowAnonymous]
        [HttpPost("VerifyEmail")]
        public StatusRequest VerifyEmail([FromForm] VerifyEmailRequest model)
        {
            if (_host == null)
                return new StatusRequest { Status = false, Message = "Please reload the page" };

            if (string.IsNullOrEmpty(model.Key))
                return new StatusRequest { Status = false, Message = "Please enter Email" };
            using var de = new DataEntities();
            var keyActive = de.AspNetUsers.FirstOrDefault(p => p.ResetPasswordKey == model.Key);
            if (keyActive == null)
                return new StatusRequest { Status = false, Message = "The verification code is invalid or has expired" };

            keyActive.EmailConfirmed = true;
            keyActive.ResetPasswordKey = null;
            de.SaveChanges();
            return new StatusRequest { Status = true, Message = "Email verification successful" };
        }
        [AllowAnonymous]
        [HttpPut("VerifyEmail")]
        public StatusRequest VerifyEmail2([FromBody] VerifyEmailRequest model)
        {
            return VerifyEmail(model);
        }
        [AllowAnonymous]
        [HttpPost("ForgotPassword")]
        public ActionResult ForgotPassword([FromForm] ForgotPasswordRequest model)
        {
            if (_host == null)
                return Ok(new { Check = false, Ms = "Please reload the page" });
            if (string.IsNullOrEmpty(model.UserName))
                return Ok(new { Check = false, Ms = "Please enter UserName" });
            if (string.IsNullOrEmpty(model.Email))
                return Ok(new { Check = false, Ms = "Please enter Email" });
            using var de = new DataEntities();
            de.Configuration.LazyLoadingEnabled = false;
            var ct = de.AspNetUsers.FirstOrDefault(p => p.UserName == model.UserName);
            if (ct == null)
            {
                return Ok(new { Check = false, Ms = "Cant found your username" });
            }
            if (ct.Email != model.Email)
                return Ok(new { Check = false, Ms = "Email does not exist" });
            ct.ResetPasswordKey = ct.UserName.Substring(0, 2) + Tool.GetRandomNumber(6);
            while (de.AspNetUsers.AsNoTracking().Any(p => p.ResetPasswordKey == ct.ResetPasswordKey))
                ct.ResetPasswordKey = ct.UserName.Substring(0, 2) + Tool.GetRandomNumber(6);
            ct.DateExp = DateTime.Now.AddMinutes(5);
            de.SaveChanges();
            var content = "<body>\r\n    <div class=\" font-family: Arial, sans-serif;background-color: #f0f0f0;margin: 0;padding: 0\">\r\n        <div class=\"container\"\r\n            style=\"  max-width: 100%;margin: 0 auto; background-color: #fff;border: 1px solid #ddd;box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);\">\r\n            <div class=\"header\" style=\" background-color: #007bff;text-align: center;\r\n            color: #fff;\r\n            padding: 20px;\">\r\n                <img style=\"max-width: 150px;\" src=\"https://unidi.net/Content/overview/img/Unidi%20Logo1.png\"\r\n                    alt=\"Company Logo\">\r\n            </div>\r\n            <div class=\"content\" style=\"padding: 0 16px; \">\r\n                <div class=\"header\">\r\n                    <h1 style=\"text-align: center;\"> Email Verification Code</h1>\r\n                </div>\r\n                <div class=\"content\" style=\"margin: 26px 0;\">\r\n                    <p><strong>Dear</strong> " + ct.FullName + ",</p>\r\n                    <p>You have requested to reset the password of your UNIDI account. Please use the following OTP to change your password</p>\r\n                    <br>\r\n                    <h1 style=\"text-align: center;\">" + ct.ResetPasswordKey + "</h1>\r\n                    <br>\r\n                    <p>This verification code will be expired in 5 minutes.</p>\r\n                    <br>\r\n                    <p style=\"padding-bottom: 20px;\">If you didn’t request this, you can ignore this email. If you need assistance or have any questions, please don't hesitate\r\n                        to\r\n                        contact us at <a href=\"mailto:[Support Email]\">udini@gmail.com</a> or call us at <a\r\n                            href=\"tel:[Support Phone Number]\">0909090</a>. We are always here to assist you.</p>\r\n                </div>\r\n\r\n            </div>\r\n            <div class=\"footer\" style=\"text-align: left;  background-color: ghostwhite;\r\n            padding:10px 16px;\">\r\n                <p>Thank you for choosing <b>UNIDI</b> as your partner. <br> We look forward to providing you with the\r\n                    best experiences.</p>\r\n                <p>Best regards,<br>" + ct.FullName + "<br>UNIDI</p>\r\n                <p><a style=\" color: #007bff; cursor: pointer;\r\n                    text-decoration: none;\" href=\"https://unidi.net/\">unidi.net</a></p>\r\n            </div>\r\n        </div>\r\n    </div>\r\n</body>";

            Tool.SendMail("[UNIDI] MESSAGE", content, model.Email);
            return Ok(new { Check = true, Ms = "Success, mail will sent a code to your email", Data = ct.ResetPasswordKey });
        }

        [AllowAnonymous]
        [HttpPut("ForgotPassword")]
        public ActionResult ForgotPassword2([FromBody] ForgotPasswordRequest model)
        {
            return ForgotPassword(model);
        }

        [AllowAnonymous]
        [HttpGet("[action]")]
        public UsingtResponse UpdateForgotPassword(string? code, string? password, string? confirmpassword)
        {
            var de = new DataEntities();

            if (string.IsNullOrEmpty(code) || !de.AspNetUsers.Any(p => p.ResetPasswordKey == code))
                return new UsingtResponse { Check = false, Ms = "Invalid recovery code" };
            var ucheck = de.AspNetUsers.FirstOrDefault(p => p.ResetPasswordKey == code);
            if (ucheck == null)
                return new UsingtResponse { Check = false, Ms = "Can't found this key " };

            if (DateTime.Now > ucheck.DateExp)
                return new UsingtResponse { Check = false, Ms = "Expired code, Please go back to the previous step to re-enter email and user to receive the code" };

            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmpassword))
                return new UsingtResponse { Check = false, Ms = "Please enter a password" };

            if (password != confirmpassword)
                return new UsingtResponse { Check = false, Ms = "Password incorrect, please try again" };

            var hasher = new PasswordHasher<string>();
            var pass = hasher.HashPassword(ucheck.UserName, password);
            ucheck.PasswordHash = pass;

            de.SaveChanges();
            return new UsingtResponse { Check = false, Ms = "Change password success!" };
        }

        [HttpGet("[action]")]
        public UsingtResponse ChangePassword(string? oldpassword, string? newpassword, string? confirmpassword)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return new UsingtResponse { Check = false, Ms = "Your login session has expired, please login again!" };

            var de = new DataEntities();
            if (string.IsNullOrEmpty(oldpassword) || string.IsNullOrEmpty(newpassword) || string.IsNullOrEmpty(confirmpassword))
                return new UsingtResponse { Check = false, Ms = "Please enter the full field" };

            var u = de.AspNetUsers.Single(p => p.Id == user.Id);
            var now = Tool.Curtime();
            var hasher = new PasswordHasher<string>();
            var check = hasher.VerifyHashedPassword(u.UserName, u.PasswordHash, oldpassword);

            if (check == 0)
            {
                //var check1 = C_Config.PassDefault == model.Password;
                //if (!check1)
                {
                    if (u.AccessFailedCount < 5)
                    {
                        u.AccessFailedCount += 1;
                        de.SaveChanges();
                        return new UsingtResponse { Check = false, Ms = "Incorrect password" };
                    }
                    else
                    {
                        u.LockoutEndDateUtc = now.AddHours(1);
                        u.AccessFailedCount = 0;
                        de.SaveChanges();
                        return new UsingtResponse { Check = false, Ms = "Account is temporarily locked for 1 hour" };
                    }

                }
            }
            u.LockoutEndDateUtc = now;
            u.AccessFailedCount = 0;

            if (newpassword != confirmpassword)
                return new UsingtResponse { Check = false, Ms = "Password and newpassword do not match" };

            var Chpass = hasher.HashPassword(u.UserName, newpassword);
            u.PasswordHash = Chpass;
            de.SaveChanges();
            return new UsingtResponse { Check = true, Ms = "Change password success!" };
        }

        [AllowAnonymous]
        [HttpPost("ResetPassword")]
        public StatusRequest ResetPassword([FromForm] ResetPasswordRequest model)
        {
            if (_host == null)
                return new StatusRequest { Status = false, Message = "Please reload the page" };

            using var de = new DataEntities();
            if (string.IsNullOrEmpty(model.ResetPasswordKey) || !de.AspNetUsers.Any(p => p.ResetPasswordKey == model.ResetPasswordKey))
                return new StatusRequest { Status = false, Message = "Invalid recovery code" };

            if (string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.ConfirmPassword))
                return new StatusRequest { Status = false, Message = "Please enter a new password" };
            if (model.Password != model.ConfirmPassword)
                return new StatusRequest { Status = false, Message = "Password incorrect, please try again" };
            var user = de.AspNetUsers.Single(p => p.ResetPasswordKey == model.ResetPasswordKey);

            var hasher = new PasswordHasher<string>();
            user.PasswordHash = hasher.HashPassword(user.UserName, model.ConfirmPassword);
            user.ResetPasswordKey = null;
            de.SaveChanges();
            return new StatusRequest { Status = true, Message = "Change password successfully" };
        }
        [AllowAnonymous]
        [HttpPut("ResetPassword")]
        public StatusRequest ResetPassword2([FromBody] ResetPasswordRequest model)
        {
            return ResetPassword(model);
        }


        /// <summary>
        ///                                PROFILEEEEEEEEEEEEEEEEEEEE
        /// </summary>
        /// <param name="IdUser"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("[action]")]
        public ActionResult GetProfileByUserId()
        {
            var de = new DataEntities();
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!" });
            de.Configuration.ProxyCreationEnabled = false;
            de.Configuration.LazyLoadingEnabled = false;
            var profile = de.AspNetUsers.AsNoTracking().FirstOrDefault(p => p.Id == user.Id);



            return Ok(new { check = true, ms = "get all success!", data = profile });
        }

        //UpDate Profile user
        [HttpPost("[action]")]
        public async Task<ActionResult> ProfileUser([FromForm] ProfileHomeRequest model)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            using (var de = new DataEntities())
            {

                var userx = de.AspNetUsers.FirstOrDefault(p => p.Id == user.Id) ?? null;

                if (userx != null)
                {

                    if (model.FullName != null)
                        userx.FullName = model.FullName;

                    if (model.PhoneNumber != null)
                        userx.PhoneNumber = model.PhoneNumber;

                    if (model.Address != null)
                        userx.Address = model.Address;

                    if (model.BirthDay != null)
                        userx.BirthDay = model.BirthDay;

                    if (model.Gender != null)
                        userx.Gender = model.Gender;

                    if (model.Avatar != null)
                    {
                        var requestImg = await C_Request.UploadImage(model.Avatar, 10000);
                        if (!requestImg.Status)
                            return Ok(new { check = false, ms = requestImg.Message });
                        userx.CoverImage = requestImg.Result.Url;
                    }
                    de.SaveChanges();

                    var p = new
                    {

                        Id = userx.Id,
                        FullName = userx.FullName,

                        Gender = userx.Gender,

                        phone = userx.PhoneNumber,
                        Image = user.CoverImage,
                        BirthDay = userx.BirthDay,
                        Check2FA = userx.IsGoogleAuthenticator ?? false,

                        //Role2 = role2.Name,

                    };


                    return Ok(new { check = true, ms = "Update Successfully", data = p });


                }
                else
                {
                    return Ok(new { check = false, ms = "Dont found your account" });
                }

            }

            //     if(model.lockstatus== )     
        }

        [AllowAnonymous]
        // GET LIST REFLINKK
        [HttpGet("[action]")]                   /// THIS USING
        public ActionResult GetAllRefereByUser(string? type, string? key, int page = 1, int limit = 20)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });


            using var de = new DataEntities();
            de.Configuration.LazyLoadingEnabled = false; // 1 cai doc 1 cai ghi
            de.Configuration.ProxyCreationEnabled = false;

            var referTree = de.AspNetUsers.AsNoTracking().Where(p => (!string.IsNullOrEmpty(type) || p.SponsorId == user.Id) && (string.IsNullOrEmpty(key) || p.UserName.Contains(key)))
                .OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);

            return Ok(new { check = true, ms = "Get ALL Success", data = referTree, total = referTree.TotalItemCount });
        }

        ///         ADDRESSSSSSSSSSSS
        /// ////////////////////////////////
        [HttpGet("[action]")]
        public ActionResult SendCode()  // SEND CODE FOR PASS2
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            var de = new DataEntities();

            var activeCode = Tool.GetRandomNumber(6);
            var expirationTime = DateTime.UtcNow.AddMinutes(3);

            var uscer = de.AspNetUsers.FirstOrDefault(p => p.Id == user.Id);
            if (uscer != null)
            {
                uscer.SecrectKey = activeCode;
                uscer.DateExp = expirationTime;
            }

            de.SaveChanges();

            string emailShop = "thuannguyenTHCST4@gmail.com";
            string passWordShop = "zcewtpskkqwebiip";
            MailMessage mailMessage = new MailMessage(emailShop, user.Email);

            mailMessage.Subject = "[UNIDI] MESSAGE";
            mailMessage.Body = "YOUR CODE TO CREATE A PASS 2 is: " + activeCode + "\n\n Please enter a code to create Pass2" + $"\n Your CODE will expire after 3 minutes";

            mailMessage.IsBodyHtml = true;

            using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                System.Net.NetworkCredential nc = new NetworkCredential(emailShop, passWordShop);
                smtp.Credentials = nc;
                smtp.EnableSsl = true;
                smtp.Send(mailMessage);
            }

            // Redirect to the UpdatePassword2 action with the activeCode in the query string
            return Ok(new { check = true, ms = "send mail success! " });
        }

        [HttpGet("[action]")]
        public ActionResult UpdatePassword2(string? code = "", string? pass2 = "", string? cprmpass2 = "") //UPDATE PASS 2
        {
            using (var de = new DataEntities())
            {
                var user = C_User.Auth(Request.Headers["Authorization"]);
                if (user == null)
                    return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

                var checkacc = de.AspNetUsers.Single(p => p.Id == user.Id);

                var hasher = new PasswordHasher<string>();
                var pass = hasher.HashPassword(checkacc.UserName, pass2);

                if (code != "") // Check if code is not empty
                {

                    if (DateTime.UtcNow > checkacc.DateExp)
                    {

                        return Ok(new { check = false, ms = "Date exp !!, please send mail again " });
                    }

                    if (code != checkacc.SecrectKey)
                    {
                        return Ok(new { check = false, ms = "WRONG KEY! PLEASE TRY AGAIN" });
                    }

                    if (pass2 != cprmpass2)
                    {
                        return Ok(new { check = false, ms = "Confirm pass2 wrong!, try again" });
                    }

                    if (DateTime.UtcNow <= checkacc.DateExp && code == checkacc.SecrectKey) // Compare the code from the URL with the entered code
                    {
                        checkacc.Pass2 = pass;
                        checkacc.SecrectKey = null;
                        checkacc.DateExp = null;
                        de.SaveChanges();
                        return Ok(new { check = true, ms = "Update password success!" });
                    }

                    else
                    {
                        return Ok(new { check = true, ms = "something wrong" });
                    }
                }
                else
                {
                    return Ok(new { check = false, ms = "Please enter the code!" });
                }
            }
        }

        [HttpGet("[action]")] // GET afiliate
        public ActionResult Affiliate(string? username)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { Check = false, Ms = "Your login session has expired, please login again!", redirect = "/Login" });
            using var de = new DataEntities();
            de.Configuration.ProxyCreationEnabled = false;
            de.Configuration.LazyLoadingEnabled = false;
            if (string.IsNullOrEmpty(username) || !de.AspNetUsers.AsNoTracking().Any(p => p.UserName == username))
                username = user.UserName;

            var userFillter = de.AspNetUsers.AsNoTracking().Single(p => p.UserName == username);
            var sponsorAddress = userFillter.SponsorAddress + "-";
            if (!sponsorAddress.StartsWith(user.SponsorAddress))
                return Ok(new { Check = false, Ms = "Access denied!" });

            var countall = de.AspNetUsers.AsNoTracking().Where(p => p.Id == user.Id);


            var get = de.AspNetUsers.Where(p => p.SponsorId == userFillter.Id).ToList();
            foreach (var item in get)
            {
                if (de.AspNetUsers.Any(p => p.SponsorAddress.StartsWith(item.SponsorAddress + "-")))
                    item.TempCheckAfiliate = true;
            }

            var count = new
            {
                countall = de.AspNetUsers.AsNoTracking().Count(p => p.SponsorAddress.StartsWith(sponsorAddress)),
                countunderusername = get.Count(),
            };
            if (get == null)
                return Ok(new { Check = false, Ms = "Fail!, wrong username " });

            return Ok(new { Check = true, Ms = "Get success!", data = get, total = count });
        }

        //Check password 
        [HttpGet("[action]")]
        public ActionResult Checkpass2(string? pass = "")
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
            {
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });
            }
            using (var de = new DataEntities())
            {
                if (pass == "")
                {
                    return Ok(new { check = false, ms = "please enter the pass2 " });

                }

                var chacc = de.AspNetUsers.FirstOrDefault(p => p.Id == user.Id && p.Pass2 == pass);
                if (chacc == null)
                {
                    return Ok(new { check = false, ms = "Incorect Pass2 " });

                }
                else
                {
                    return Ok(new { check = true, ms = "CHECK pass2 Success!!!" });
                }
            }
        }

        [HttpGet("[action]")]
        public ActionResult Security2FA()
        {
            using (var de = new DataEntities())
            {
                var userw = C_User.Auth(Request.Headers["Authorization"]);
                if (userw == null)
                    return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

                var user = de.AspNetUsers.Single(p => p.Id == userw.Id);
                TwoFactorAuthenticator tfa = new();
                if (string.IsNullOrEmpty(user.GoogleAuthenticatorSecretKey))
                {
                    user.GoogleAuthenticatorSecretKey = Tool.GetRandomString(12);
                    de.SaveChanges();
                }
                var setupInfo = tfa.GenerateSetupCode(HttpContext.Request.Host.ToString(), user.UserName, user.GoogleAuthenticatorSecretKey, false);

                return Ok(new { check = true, Ms = "Success!", data = setupInfo, enable = user.IsGoogleAuthenticator ?? false });
            }
        }
        [HttpPost("TwoFactorAuthentication")]
        [SwaggerOperation(Summary = "Google Authenticator", Description = "Two-factor authentication is a method for protecting your web account. When it is activated you need to enter not only your password, but also a special code. You can receive this code by in mobile app. Even if a third person will find your password, they can't access it with that code.")]
        public ActionResult UpdateTwoFactorAuthentication([FromForm] GoogleAuthenRequest model)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });


            if (string.IsNullOrEmpty(model.Password)
                || string.IsNullOrEmpty(model.Code)
                )
                return Ok(new { check = false, ms = "Field cannot be left blank" });
            using var de = new DataEntities();
            var u = de.AspNetUsers.Single(p => p.Id == user.Id);
            var hash = new PasswordHasher<string>();

            if (hash.VerifyHashedPassword(user.UserName, u.PasswordHash, model.Password) == PasswordVerificationResult.Failed)
                return Ok(new { check = false, ms = "Login password is incorrect" });

            TwoFactorAuthenticator tfa = new();
            bool isValid = tfa.ValidateTwoFactorPIN(user.GoogleAuthenticatorSecretKey, model.Code);
            if (!isValid)
                return Ok(new { check = false, ms = "2FA code is incorrect" });

            u.IsGoogleAuthenticator = model.Enable ?? true;
            de.SaveChanges();
            return Ok(new { check = true, ms = "Update success" });
        }
    }
}
