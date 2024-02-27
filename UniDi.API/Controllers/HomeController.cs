using Core;
using Core.Models;
using Core.Models.Request;
using Core.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.RegularExpressions;
using X.PagedList;
using static QRCoder.PayloadGenerator;

namespace UniDi.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {



        [HttpPost("[action]")]
        public ActionResult UpdateAddress([FromForm] AddressRequest model)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });


            using var de = new DataEntities();
            de.Configuration.ProxyCreationEnabled = false;
            de.Configuration.LazyLoadingEnabled = false;

            var listAddress = de.UserAddresses.Where(p => p.UserId == user.Id).ToList();
            /*  if (!string.IsNullOrEmpty(model.Type))
              {
                  var chec = de.ListAddresses.Where(p => p.UserId == user.Id && p.Type == "Default");
                  if(chec != null)
                  {
                      foreach (var item in chec)
                      {
                          item.Type = "UnDefault";
                      }
                  }    
              }*/

            if (string.IsNullOrEmpty(model.Id))
            {

                var pe = new UserAddress
                {
                    Id = Guid.NewGuid().ToString(),
                    Receiver = model.NameReCevive,
                    UserId = user.Id,
                    Country = model.Country,
                    City = model.City,
                    Province = model.Province,
                    ZipCode = model.ZipCode,

                    Street = model.DetailAddress,
                };

                string normalizedPhoneNumber = Regex.Replace(model.Phone, @"\s|-", "");

                // Kiểm tra xem chuỗi có chứa chỉ chữ số và có độ dài hợp lệ không
                if (!Regex.IsMatch(normalizedPhoneNumber, @"^\d+$"))
                {
                    return Ok(new { check = false, ms = "Phone number invalid!" });
                }
                pe.Phone = model.Phone;

                if (listAddress.Count > 0)
                {
                    if (!string.IsNullOrEmpty(model.Type)) // có model.type
                    {
                        foreach (var item in listAddress)
                        {
                            item.Type = "UnDefault";
                        }
                        pe.Type = "Default";
                    }
                    else
                        pe.Type = "UnDefault";

                }
                else
                {
                    pe.Type = "Default";
                }
                de.UserAddresses.Add(pe);
                de.SaveChanges();
                return Ok(new { check = true, ms = "Add New Address Success!", data = pe });
            }
            else
            {
                var ladd = de.UserAddresses.Single(p => p.Id == model.Id);
                ladd.Country = model.Country;
                ladd.Receiver = model.NameReCevive;
                ladd.City = model.City;
                ladd.Province = model.Province;

                ladd.Street = model.DetailAddress;
                ladd.ZipCode = model.ZipCode;
                string normalizedPhoneNumber = Regex.Replace(model.Phone.ToString(), @"\s|-", "");

                if (!Regex.IsMatch(normalizedPhoneNumber, @"^\d+$"))//|| normalizedPhoneNumber.Length < 10)
                {
                    return Ok(new { check = false, ms = "Phone number invalid!" });
                }
                ladd.Phone = model.Phone;

                if (listAddress != null && listAddress.Count > 1)
                {
                    if (string.IsNullOrEmpty(model.Type))
                        ladd.Type = "UnDefault";
                    else
                    {
                        foreach (var item in listAddress)
                        {
                            item.Type = "UnDefault";
                        }
                        ladd.Type = "Default";
                    }
                    de.SaveChanges();
                    return Ok(new { check = true, ms = "Update Address Success!" });
                }
                else
                {
                    de.SaveChanges();
                    return Ok(new { check = true, ms = "Update Address Success! Can't set this to Undefault!" });
                }

            }
        }
        ///         ADDRESSSSSSSSSSSS

        [HttpGet("[action]")]
        public ActionResult GetAllAddressByUser(string? Idaddress)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            using var de = new DataEntities();
            de.Configuration.ProxyCreationEnabled = false;
            de.Configuration.LazyLoadingEnabled = false;

            var listAddress = de.UserAddresses.AsNoTracking().Where(p => p.UserId == user.Id && (string.IsNullOrEmpty(Idaddress) || p.Id == Idaddress)).ToList();
            foreach (var imt in listAddress)
            {
                imt.ContryInfo = de.Countries.AsNoTracking().FirstOrDefault(p => p.Nicename == imt.Country);
            }

            return Ok(new { check = true, ms = "get all success!", data = listAddress, total = listAddress.Count });
        }


        [HttpGet("[action]")]
        public ActionResult SetDefaultAddress(string? id )
        {
            using var de = new DataEntities();
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });


            var set = de.UserAddresses.FirstOrDefault(p => p.Id == id);
            if (set == null)
            {
                return Ok(new { check = false, ms = "Cant found this address" });
            }
            else
            {
                if (set.Type == "Default")
                {
                    return Ok(new { check = false, ms = "Cant set an default address!" });

                }

                set.Type = "Default";
                var oldde = de.UserAddresses.FirstOrDefault(p => p.UserId == user.Id && p.Type == "Default");
                if (oldde != null)
                    oldde.Type = "UnDefault";

                de.SaveChanges();
                return Ok(new { check = true, ms = "Set Address Default success!" });
            }
        }

        [HttpGet("[action]")]
        public ActionResult DeleteAddress(string? id = "")
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });


            using var de = new DataEntities();
            var del = de.UserAddresses.FirstOrDefault(p => p.UserId == user.Id && p.Id == id);
            if (del != null)
            {

                if (del.Type == "Default")
                {
                    return Ok(new { check = false, ms = "Cant not delete default address" });
                }

                de.UserAddresses.Remove(del);
                de.SaveChanges();
            }

            return Ok(new { check = true, ms = "Delete address success!" });
        }

        // Create upt delete commet
        [HttpPost("[Action]")]
        [SwaggerOperation(Description = "")]
        public ActionResult UpdateComment([FromForm] Comment model, [FromForm] string? type = "")
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            using var de = new DataEntities();
            if (type == "" && model.Id == null)
            {
                var p = new Comment()
                {
                    Id = Guid.NewGuid().ToString(),
                    Text = model.Text,
                    ProductId = model.ProductId,
                    UserId = user.Id,
                    DateCreate = DateTime.Now,
                };

                de.Comments.Add(p);
                de.SaveChanges();
                return Ok(new { check = true, ms = "Comment success" });

            }
            else
            {
                var cm = de.Comments.FirstOrDefault(p => p.UserId == user.Id && p.Id == model.Id);
                de.Comments.Remove(cm);
                de.SaveChanges();

                return Ok(new { check = true, ms = "Delete Comment success" });

            }
        }

        //get all comment in product detail
        [HttpGet("[action]")]
        public ActionResult GetallComment(Guid? id , int page = 1, int limit = 20)
        {
            using var de = new DataEntities();
            de.Configuration.ProxyCreationEnabled = false;
            de.Configuration.LazyLoadingEnabled = false;



            var list = de.Comments.AsNoTracking().Where(p => p.ProductId == id).OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);
            foreach (var p in list)
            {
                var user = de.AspNetUsers.AsNoTracking().FirstOrDefault(e => e.Id == p.UserId);
                if (user != null)
                {
                    p.IdUser = user.Id;
                    p.NameUser = user.FullName;
                    p.Avatar = user.CoverImage;
                }

            }

            return Ok(new { check = true, ms = "get all success", data = list, total = list.TotalItemCount });
        }
        [HttpGet("[action]")]    
        public ActionResult RatingProduct (Guid? productId,int? rating,string? comment)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            using var de = new DataEntities();
            var proinv = de.Invoices.Where(p => p.UserId == user.Id).ToList();
            bool chk = false ;
            foreach(var item in proinv)
            {
                chk = de.InvoiceDetails.Any(p =>p.InvoiceId == item.Id && p.ProductId == productId);
                break;
            }

            var ratinge = de.Comments.FirstOrDefault(p => p.ProductId == productId  && p.UserId == user.Id && p.Rating != null);
            if(ratinge != null)
                return Ok(new { check = false, ms = "You cannot rate further!" });
            else
            {
                var p = new Comment
                {
                    Id = Guid.NewGuid().ToString(),
                    DateCreate = DateTime.Now,
                    Text = comment,
                    Rating = rating,
                    UserId = user.Id,
                    ProductId = productId,
                };
                de.Comments.Add(p);
            }
            de.SaveChanges();
            
            return Ok(new {check = false, ms = "Rating success!"});
        }

        [HttpPost("[action]")]
        public ActionResult FavoriteProduct([FromForm] FavoriteRequest model)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            using var de = new DataEntities();
            
            if(model.Id == null || model.Id == Guid.Empty)
            {
                var p = new ProductFavorite
                {
                    ProductId = model.ProductId,
                    DateCreate = DateTime.Now,
                    UserId = user.Id,
                };
                de.ProductFavorites.Add(p);
                de.SaveChanges();
                return Ok(new { check = true, ms = " Farvorite Success!"});

            }
            else
            {
                var depro = de.ProductFavorites.FirstOrDefault(p => p.UserId == user.Id && p.ProductId == model.ProductId);
                if(depro != null)
                    de.ProductFavorites.Remove(depro);

                de.SaveChanges();
                return Ok(new { check = true, ms = " UnFarvorite Success!" });

            }


        }
        [HttpGet("[action]")]
        public ActionResult GetAllFavorite()
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });
            using var de = new DataEntities();

            var depro = de.ProductFavorites.Where(p => p.UserId == user.Id  ).ToList();
            foreach(var item in depro)
            {
               item.Product = de.Products.AsNoTracking().FirstOrDefault(p=>p.Id == item.ProductId) ;   
            }
            return Ok(new { check = true, ms = "get all success!" ,data = depro});

        }

    }
}
