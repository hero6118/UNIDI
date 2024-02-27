using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using Core;
using System.IdentityModel.Tokens.Jwt;

namespace UniDi.API.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class EmailController : ControllerBase
    {
        [NonAction]
        public void Sendmail(string mailuser, string value)
        {

            /*Gửi Mail*/

            var emailShop = "thuannguyenTHCST4@gmail.com";
            var passWordShop = "zcewtpskkqwebiip";
            MailMessage mailMessage = new(emailShop, mailuser)
            {
                Subject = "[UNIDI] MESSAGE",

                Body = value,
                /* mailMessage.Body = $"Please click on the link below to confirm! \n\n " +

                    "<a href='" + urlFrontEnd + "?verify="  + "  ' style='color:blue;'>Verify</a>" +
                    "\n + " +
                     "------------------------------------------------\n" +
                     "CẢM ƠN!";*/
                IsBodyHtml = true
            };
            using SmtpClient smtp = new("smtp.gmail.com", 587);
            System.Net.NetworkCredential nc = new NetworkCredential(emailShop, passWordShop);
            smtp.Credentials = nc;
            smtp.EnableSsl = true;
            smtp.Send(mailMessage);

            return;

        }

        [HttpPost("[action]")]
        public ActionResult Testgmail(string email)
        {
            /* var user = C_User.Auth(Request.Headers["Authorization"]);
             if (user == null)
                 return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });*/

            using var de = new DataEntities();

            /*  var code = Tool.GetRandomString(12);
              while (de.AspNetUsers.Any(p => p.ResetPasswordKey == code))
              {
                  code = Tool.GetRandomString(12);
              }

              var exp = ( DateTime.Now);

              var userContact = de.AspNetUsers.FirstOrDefault(p => p.Email == email);

              var claim = new List<Claim> {
                          new Claim("id", userContact.Id),
                          new Claim("code", Tool.GetRandomString(12) )
              };
              var jwt = Tool.EnJwtToken(claim,exp);

              userContact.SecrectKey = jwt;
              userContact.ResetPasswordKey = code;

              token = code;
              */
            /*Gửi Mail*/
            /*
                              string emailShop = "thuannguyenTHCST4@gmail.com";
                              string passWordShop = "zcewtpskkqwebiip";
                            MailMessage mailMessage = new MailMessage(emailShop, userContact.Email);

                            mailMessage.Subject = "[UNIDI] MESSAGE";
                            mailMessage.Body = $"Please click on the link below to confirm! \n\n " +

                               "<a href='" + urlFrontEnd + "?verify=" + jwt + "  ' style='color:blue;'>Verify</a>" +
                               "\n + " +
                                "------------------------------------------------\n" +
                                "CẢM ƠN!";
                            mailMessage.IsBodyHtml = true;
                            using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                            {
                                System.Net.NetworkCredential nc = new NetworkCredential(emailShop, passWordShop);
                                smtp.Credentials = nc;
                                smtp.EnableSsl = true;
                                smtp.Send(mailMessage);
                            }*/


            var content = $"<div class=\" font-family: Arial, sans-serif;background-color: #f0f0f0;margin: 0;padding: 0\">\r\n        <div class=\"container\"\r\n            style=\"  max-width: 100%;margin: 0 auto; background-color: #fff;border: 1px solid #ddd;box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);\">\r\n            <div class=\"header\" style=\" background-color: #007bff;text-align: center;\r\n            color: #fff;\r\n            padding: 20px;\">\r\n                <img style=\"max-width: 150px;\" src=\"https://unidi.net/Content/overview/img/Unidi%20Logo1.png\"\r\n                    alt=\"Company Logo\">\r\n            </div>\r\n            <div class=\"content\" style=\"padding: 0 16px; \">\r\n                <div class=\"header\">\r\n                    <h1 style=\"text-align: center;\"> Successful Payment - Thank You for Your Support!</h1>\r\n                </div>\r\n                <div class=\"content\" style=\"margin: 26px 0;\">\r\n                    <p><strong>Dear</strong> [Customer Name],</p>\r\n                    <p>We are thrilled to inform you that your recent payment has been successfully processed!</p>\r\n                    <p>\r\n                        <strong>Transaction ID:</strong> [Transaction ID]\r\n                    </p>\r\n                    <p> <b>Payment Amount:</b> $[Payment Amount]</p>\r\n                    <p> <strong>Date:</strong> [Payment Date]</p>\r\n                    <p> <b>Coin listing date projection: </b>\r\n                        20/09/2023\r\n                    </p>\r\n                    <p style=\"padding-bottom: 20px;\">If you need assistance or have any questions, please don't hesitate\r\n                        to\r\n                        contact us at <a href=\"mailto:[Support Email]\">udini@gmail.com</a> or call us at <a\r\n                            href=\"tel:[Support Phone Number]\">0909090</a>. We are always here to assist you.</p>\r\n                </div>\r\n\r\n            </div>\r\n            <div class=\"footer\" style=\"text-align: left;  background-color: ghostwhite;\r\n            padding:10px 16px;\">\r\n                <p>Thank you for choosing <b>UNIDI</b> as your partner. <br> We look forward to providing you with the\r\n                    best experiences.</p>\r\n                <p>Best regards,<br>Mr.Been<br>UNIDI</p>\r\n                <p><a style=\" color: #007bff; cursor: pointer;\r\n                    text-decoration: none;\" href=\"https://unidi.net/\">unidi.net</a></p>\r\n            </div>\r\n        </div>\r\n    </div>";

            Tool.SendMail("[UNIDI] MESSAGE", content, email);


            // sendmail(email, "  ");
            //
            return Ok(new { ms = "Send mail success!" });
        }

        [HttpPost("[action]")]
        public ActionResult VerifyEmail(string token = "")
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(token);
            var tokenInfo = jwtSecurityToken.Payload.FirstOrDefault(p => p.Key == "id").Value.ToString();
            using var de = new DataEntities();


            if (jwtSecurityToken == null)
            {
                return Ok(new { check = false, ms = "  failed" });
            }

            if (tokenInfo == null)
            {
                return Ok(new { check = false, ms = "Token info is missing or null" });
            }

            var checkuser = de.AspNetUsers.FirstOrDefault(p => p.Id == tokenInfo);

            if (checkuser == null)
            {
                return Ok(new { check = false, ms = "User not found" });
            }

            if (checkuser.EmailConfirmed == true)
            {
                return Ok(new { check = false, ms = "You have verified the email" });
            }
            else
            {
                checkuser.EmailConfirmed = true;
                de.SaveChanges();
                return Ok(new { check = true, ms = "Verification successful" });



            }

            // send email
            // public bool SendEmail 
        }
    }
}
