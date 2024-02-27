using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class C_User
    {
        public static AspNetUser Auth(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return null;
                if (token.StartsWith("Bearer"))
                    token = token.Split(' ')[1];

                var user = Tool.DeJwtToken(token);
                if (user == null) return null;
                if (user.Payload.Exp != null)
                {
                    var time = Tool.ConvertTimeStampToDateTime((double)user.Payload.Exp);
                    if (time < DateTime.Now) return null;
                }
                var userId = user.Payload.ToList().FirstOrDefault(p => p.Key == "nameid").Value + "";
                using (var de = new DataEntities())
                {
                    return de.AspNetUsers.AsNoTracking().FirstOrDefault(p => p.Id == userId);
                }
            }
            catch
            {
                return null;
            }
        }

        public static bool CheckAccept(string role, string userid)
        {
            using (var de = new DataEntities())
            {
                var getAllRole = de.AspNetUserRoles.Where(p => p.UserId == userid).ToList();
                var check = false;
                foreach (var rolen in getAllRole)
                {
                    var name = de.AspNetRoles.FirstOrDefault(p => p.Id == rolen.RoleId).Name;
                    if (name == "Admin")
                        check = true;
                    if (name == role || name == "AdminAll")               
                        check = true;

                }
                if (check == false)
                    return false;
                else
                    return true;
            }

        }


    }



    public class Auth
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Id { get; set; }
        public DateTime ExpDate { get; set; }
        public List<string> Roles { get; set; }
    }
    public class LoginData
    {
        public string Id { get; set; }
        public string Jwt { get; set; }
        //   public List<AspNetUser> User { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Address { get; set; }

        public string Gender { get; set; }
        public DateTime? BirthDay { get; set; }
        public string phone { get; set; }
        public string singleRole { get; set; }
        public List<string> Role { get; set; }
        public string Image { get; set; }
        public string Redirect { get; set; }
        public Dictionary<string, string> UserInfo { get; set; }
        public bool Check2FA { get; set; }

        public LoginData() { }


    }

}
