using Core.Models.Request;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace Core.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string Address { get; set; }
        public string SponsorId { get; set; }
        public string SponsorAddress { get; set; }
        public int? SponsorFloor { get; set; }
        public string PlacementId { get; set; }
        public string PlacementAddress { get; set; }
        public int? PlacementFloor { get; set; }
        public DateTime? DateCreate { get; set; }
        public bool Activity { get; set; }
        public bool Lock { get; set; }
        public string SecrectKey { get; set; }
        public bool? IsGoogleAuthenticator { get; set; }
        public string GoogleAuthenticatorSecretKey { get; set; }
        public int? Type { get; set; }

        public static ApplicationUser MapUserFromViewModel(RegisterRequest model)
        {
            var appUser = new ApplicationUser
            {
                UserName = model.UserName,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                DateCreate = DateTime.Now,
                Type = model.Type ?? Enum_UserType.Member
            };

            using (var de = new DataEntities())
            {
                if (!string.IsNullOrEmpty(model.Sponsor))
                {
                    var parentSponsor = de.AspNetUsers.Single(p => p.UserName == model.Sponsor);
                    var countDownline = (de.AspNetUsers.Count(p => p.SponsorId == parentSponsor.Id) + 1);
                    var sponsorAddress = parentSponsor.SponsorAddress + '-' + countDownline;
                    while (de.AspNetUsers.Any(p => p.SponsorAddress == sponsorAddress))
                    {
                        countDownline++;
                        sponsorAddress = parentSponsor.SponsorAddress + '-' + countDownline;
                    }
                    appUser.SponsorAddress = sponsorAddress;
                    appUser.SponsorId = parentSponsor.Id;
                    appUser.SponsorFloor = Convert.ToInt32(parentSponsor.SponsorFloor) + 1;
                }

                //if (string.IsNullOrEmpty(model.Placement))
                //{
                //    model.Placement = model.Sponsor;
                //    var placement = de.AspNetUsers.Single(p => p.UserName == model.Placement);
                //    var placentAdd = placement.PlacementAddress + "-";
                //    if (de.AspNetUsers.Count(p => p.PlacementId == placement.Id) >= 2)
                //    {
                //        var listDownline = de.AspNetUsers.Where(p => p.PlacementAddress.StartsWith(placentAdd)).OrderBy(p => p.DateCreate);
                //        foreach (var item in listDownline)
                //        {
                //            var placementAdd = item.PlacementAddress + (model.Branch.ToLower() == "right" ? "-1" : "-0");
                //            if (de.AspNetUsers.Any(p => p.PlacementAddress == placementAdd))
                //                continue;

                //            model.Placement = item.UserName;
                //            break;
                //        }
                //    }
                //}

                //var parentPlacement = de.AspNetUsers.Single(p => p.UserName == model.Placement);
                //var branch = model.Branch?.ToLower();
                //var placementAddress = parentPlacement.PlacementAddress + (branch == "right" ? "-1" : "-0");

                //if (de.AspNetUsers.Any(p => p.PlacementAddress == placementAddress))
                //    return null;

                //appUser.PlacementAddress = placementAddress;
                //appUser.PlacementId = parentPlacement.Id;
                //appUser.PlacementFloor = Convert.ToInt32(parentPlacement.PlacementFloor) + 1;

                appUser.Activity = false;
                appUser.Lock = false;
                appUser.IsGoogleAuthenticator = false;
                appUser.GoogleAuthenticatorSecretKey = Tool.GetRandomString(12);
                appUser.EmailConfirmed = false;
                return appUser;
            }
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext() : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}