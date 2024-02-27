using Core;
using Core.Models.Request;
using Core.Models.Response;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Server;
using OpenCvSharp;
using Svg.ExCSS;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Drawing;
using System.Globalization;
using System.Net.Mail;
using System.Net;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using X.PagedList;
using static OpenCvSharp.FileStorage;
using static OpenCvSharp.Stitcher;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using static Core.Models.Request.JsonParse;
using System.Net.Http.Headers;
using OpenCvSharp.Aruco;

using System.Reflection.Metadata.Ecma335;

namespace UniDi.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private string urlFrontEnd = "https://unidi.net/Request/packages";

        //get all License
        [HttpGet("[Action]")]
        public ActionResult GetAllLicense(string? fromDate, string? ToDate, string? SearchLicense ,  int page = 1, int limit = 20)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!" });

            using var de = new DataEntities();
            de.Configuration.LazyLoadingEnabled = false; // 1 cai doc 1 cai ghi
            de.Configuration.ProxyCreationEnabled = false;
            var dt = fromDate != null ? DateTime.Parse(fromDate) : (DateTime?)null;
            DateTime? dtt = ToDate != null ? DateTime.Parse(ToDate) : null;
            if (dtt != null)
                dtt = dtt.Value.Add(new TimeSpan(23, 59, 59));

            var search = de.BusinessLicenses.Where(p => (string.IsNullOrEmpty(fromDate) || p.DateCreate > dt)
            && (string.IsNullOrEmpty(ToDate) || p.DateCreate <= dtt)
            && (string.IsNullOrEmpty(SearchLicense) || p.BusinessCode.Contains(SearchLicense))

            ).ToPagedList(page, limit);

            return Ok(new { check = true, ms = "success!", data = search, total = search.TotalItemCount });

        }

        /*        [HttpGet("[Action]")]
                public ActionResult GetAllLicenseTEST(string? SearchLicense , string? fromDate, int page = 1, int limit = 20)
                {
                    var user = C_User.Auth(Request.Headers["Authorization"]);
                    if (user == null)
                        return Ok(new { check = false, ms = "Your login session has expired, please login again!" });

                    using (var de = new DataEntities())
                    {
                        de.Configuration.LazyLoadingEnabled = false; // 1 cai doc 1 cai ghi
                        de.Configuration.ProxyCreationEnabled = false;

                             var search = de.BusinessLicenses.AsNoTracking().Where(p => (   (string.IsNullOrEmpty(SearchLicense) || p.BusinessCode.ToUpper().Contains(SearchLicense.ToUpper())) || string.IsNullOrEmpty(fromDate)  )

                                ).OrderByDescending(p=>p.DateCreate).ToPagedList(page,limit);
                                foreach (var item in search)
                                {
                                    item.userinfo = de.AspNetUsers.AsNoTracking().FirstOrDefault(p => p.Id == item.ShopId);
                                }


                            return Ok(new { check = true, ms = "success!", data = search, total = search.TotalItemCount });

                      *//*  else if (fromDate != "")
                        {
                            var dt = DateTime.Parse(fromDate);
                            var searchfromdate = de.BusinessLicenses.Where(p => p.DateCreate > dt).ToPagedList(page, limit);
                            foreach (var item in searchfromdate)
                            {
                                item.userinfo = de.AspNetUsers.AsNoTracking().FirstOrDefault(p => p.Id == item.ShopId);
                            }
                            return Ok(new { check = true, ms = "success!", data = searchfromdate, total = searchfromdate.TotalItemCount });
                        }*//*

                    }

                }
        */
        //Updata status Business License
        [HttpPost("[action]")]
        public ActionResult UpdateStatusBusinessLicense(Guid? LicenseId, int? status, string? notelicense = "")
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!" });
            using var de = new DataEntities();
            if (LicenseId != null && status != null)
            {
                var license = de.BusinessLicenses.Where(p => p.Id == LicenseId).FirstOrDefault();
                if (license == null)
                    return Ok(new { check = false, ms = "Can't found license" });
                switch (status)
                {
                    case 0:
                        license.Status = Enum_BusinessLicense.New; //Tro ve trang thai ban dau
                        de.SaveChanges();
                        break;
                    case 1:
                        license.Status = Enum_BusinessLicense.Active; //
                        de.SaveChanges();
                        break;

                    case 2:
                        license.Status = Enum_BusinessLicense.Active; //Huy
                        license.NoteStatus = notelicense;  // note a reason why this license aree denied


                        de.SaveChanges();
                        break;

                }

            }
            return Ok(new { check = true, ms = "Update Status success !!!" });
        }
        //Get Detail License
        [HttpGet("[action]")]
        public ActionResult GetDeTailLicense(Guid? id)
        {
            using var de = new DataEntities();
            de.Configuration.LazyLoadingEnabled = false; // 1 cai doc 1 cai ghi
            de.Configuration.ProxyCreationEnabled = false;

            var datas = new LicenInfo();

            //var getcate = from r in de.Categories select r;
            var busl = de.BusinessLicenses.AsNoTracking().FirstOrDefault(p => p.Id == id);
            if (busl == null)
                return Ok(new { check = false, ms = "Can't found license" });
            datas.Info = busl;

            var useraccount = de.AspNetUsers.AsNoTracking().FirstOrDefault(p => p.Id == busl.ShopId);
            if (useraccount == null)
                return Ok(new { check = false, ms = "Can't found license" });
            datas.InfoUser = useraccount;

            var prototal = de.Products.AsNoTracking().Where(p => p.UserId == useraccount.Id).ToList();

            foreach (var item in prototal)
            {
                var totalcmt = de.Comments.AsNoTracking().Where(p => p.ProductId == item.Id).ToList();
                datas.TotalComent += totalcmt.Count;
            }

            datas.Total = prototal.Count;

            return Ok(new { check = true, ms = "Get Detail License Success!", data = datas });

        }
        //Delete License from the admin
        [HttpPost("[Action]")]
        public ActionResult DeleteLicense(Guid? Id)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            using var de = new DataEntities();
            var Lincense = de.BusinessLicenses.First(m => m.Id == Id);
            de.BusinessLicenses.Remove(Lincense);
            de.SaveChanges();
            return Ok(new { check = true, ms = "Delete Lincense complete! " });
        }
        //Delete Multilicense
        [HttpPost("[Action]")]
        public ActionResult DeleteMultiLicense(IEnumerable<Guid> Id)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            using var de = new DataEntities();
            foreach (var id in Id)
            {
                var Lincense = de.BusinessLicenses.Single(m => m.Id == id);
                de.BusinessLicenses.Remove(Lincense);
            }
            de.SaveChanges();
            return Ok(new { check = true, ms = "Delete multi category complete! " });
        }

        [HttpGet("[action]")]
        public ActionResult GetDetailProduct(Guid? id)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!" });

            using var de = new DataEntities();
            de.Configuration.LazyLoadingEnabled = false;
            de.Configuration.ProxyCreationEnabled = true;

            var getpro = de.Products.AsNoTracking().Where(p => p.Id == id).ToList();
            foreach (var item in getpro)
            {
                item.Category = de.Categories.AsNoTracking().FirstOrDefault(p => p.Id == item.CategoryId);
                item.CountryInfo = de.Countries.AsNoTracking().FirstOrDefault(p => p.Id == item.CountryId);
                var proimg = de.ProductImages.AsNoTracking().Where(p => p.ProductId == item.Id).ToList();
                var newlist = new List<string>();
                foreach (var imgg in proimg)
                {
                    newlist.Add(imgg.Link);
                }
                item.ListImages = newlist;
                if (newlist.Count > 0)
                {
                    item.Image = newlist.FirstOrDefault();
                }
            }
            return Ok(new { check = true, ms = "success!", data = getpro, t = Request.Headers["Authorization"] });

        }
        //Update status Product
        [HttpGet("[action]")]
        public ActionResult UpdateStatusProduct(Guid? ProductId, int? status)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!" });
            if (ProductId == null)
                return Ok(new { check = false, ms = "Field cannot be left blank" });

            if (status == null)
                return Ok(new { check = false, ms = "Field cannot be left blank" });

            using var de = new DataEntities();

            if (ProductId != null && status != null)
            {
                var statuslicense = de.Products.FirstOrDefault(p => p.Id == ProductId);
                if (statuslicense == null)
                    return Ok(new { check = false, ms = "Product does not exist" });

                switch (status)
                {
                    case 1:
                        statuslicense.Status = Enum_ProductStatus.Active; //
                        de.SaveChanges();
                        break;

                    case 2:
                        statuslicense.Status = Enum_ProductStatus.Cancel; //Huy
                        de.SaveChanges();
                        break;
                }
                return Ok(new { check = true, ms = "Update Status success !!!" });
            }
            return Ok(new { check = false, ms = "Update Status false !!!" });
        }
        [HttpPost("[action]")]
        public ActionResult DeleteMultiProduct(IEnumerable<Guid> ProductRecordDeletebyId)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            using var de = new DataEntities();
            /*var getAllRole = de.AspNetUserRoles.Where(p=>p.UserId == user.Id).ToList();
            var check = false;
            var check1 = false;
            foreach (var role in getAllRole)
            {
                var name = de.AspNetRoles.FirstOrDefault(p => p.Id == role.RoleId).Name;
                if (name == "Admin")
                {
                    check = true;
                }
                if (name == "Admin0")
                {
                    check1 = true;
                }                   
            }
            if(check == false || check1 ==false)
            {
                return Ok(new { check = false, ms = "Accept denied! " });
            }*/
            foreach (var id in ProductRecordDeletebyId)
            {
                var products = de.Products.Single(s => s.Id == id);

                de.Products.Remove(products);
            }
            de.SaveChanges();
            return Ok(new { check = true, ms = "Delete multi category complete! " });
        }
        //Get All order
        [HttpGet("[action]")]
        public ActionResult GetAllInvoice(int page = 1, int limit = 20)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            using var de = new DataEntities();
            de.Configuration.LazyLoadingEnabled = false; // 1 cai doc 1 cai ghi
            de.Configuration.ProxyCreationEnabled = false;

            //var getcate = from r in de.Categories select r;
            var list = de.Invoices.AsNoTracking().OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);

            foreach (var item in list)
            {
                item.User = de.AspNetUsers.AsNoTracking().FirstOrDefault(p => p.Id == item.UserId);
                item.InvoiceDetails = de.InvoiceDetails.AsNoTracking().Where(p => p.InvoiceId == item.Id).ToList();
                foreach (var detail in item.InvoiceDetails)
                {

                    detail.Product = de.Products.AsNoTracking().FirstOrDefault(p => p.Id == detail.ProductId);

                }
            }

            //   var listorderdetail = de.InvoiceDetails.AsNoTracking().FirstOrDefault();
            return Ok(new { check = true, ms = "success!", data = list, total = list.TotalItemCount });

        }
        //[HttpGet("[action]")]
        //public ActionResult FillterInvoiceAdmin(string? SearchSKU = "", string? Searching = "", string? SearchName = "", string? fromDate = "", string? status = "", string? DeliveryStatus = "", int page = 1, int limit = 20)
        //{


        //    var user = C_User.Auth(Request.Headers["Authorization"]);
        //    if (user == null)
        //        return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

        //    using (var de = new DataEntities())
        //    {
        //        de.Configuration.LazyLoadingEnabled = false; // 1 cai doc 1 cai ghi
        //        de.Configuration.ProxyCreationEnabled = false;

        //        //Search by InvoiceId

        //        /*  (status == "Confirm" || status == "Confirmed" || status == "Cancelled") || (DeliveryStatus == "Unfulfilled" || DeliveryStatus == "Shipping" || DeliveryStatus == "Shipped" || DeliveryStatus == "Returnning" || DeliveryStatus == "Returned")*/

        //        var list = new List<Invoice>();
        //        if (!string.IsNullOrEmpty(Searching))
        //        {
        //            var FindId = Guid.Parse(Searching);
        //            var SearchList = de.Invoices.AsNoTracking().Where(p => p.Id == FindId
        //            ).ToList();
        //            list = new List<Invoice>(SearchList);

        //        }
        //        // search name of the invoice
        //        else if (!string.IsNullOrEmpty(SearchName))
        //        {
        //            var SearchList = de.Invoices.AsNoTracking().Where(p => p.Name.Contains(SearchName)).ToList();
        //            list = new List<Invoice>(SearchList);
        //        }

        //        // Fillter by Status
        //        else if (status == "Waiting" || status == "Confirmed" || status == "Cancelled")
        //        {
        //            var listStatus = de.Invoices.AsNoTracking().Where(p => p.Status == status
        //            ).OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);
        //            foreach (var item in listStatus)
        //            {
        //                item.User = de.AspNetUsers.AsNoTracking().FirstOrDefault(p => p.Id == item.UserId);
        //                item.InvoiceDetails = de.InvoiceDetails.AsNoTracking().Where(p => p.InvoiceId == item.Id).ToList();
        //                foreach (var detail in item.InvoiceDetails)
        //                {
        //                    detail.Product = de.Products.AsNoTracking().FirstOrDefault(p => p.Id == detail.ProductId);
        //                }
        //            }
        //            list = new List<Invoice>(listStatus);
        //            /**/ //return Ok(new { check = true, ms = "This List for Filler Status success!", data = listStatus, total = listStatus.TotalItemCount });
        //        }
        //        //Fillter by DeliveryStatus
        //        else if (DeliveryStatus == "Unfulfilled" || DeliveryStatus == "Shipping" || DeliveryStatus == "Shipped" || DeliveryStatus == "Returnning" || DeliveryStatus == "Returned")
        //        {
        //            //var newlist = new List<Invoice>();
        //            var listStatus = de.Invoices.AsNoTracking().Where(p => p.DeliveryStatus == DeliveryStatus
        //            ).OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);
        //            foreach (var item in listStatus)
        //            {
        //                item.User = de.AspNetUsers.AsNoTracking().FirstOrDefault(p => p.Id == item.UserId);
        //                item.InvoiceDetails = de.InvoiceDetails.AsNoTracking().Where(p => p.InvoiceId == item.Id).ToList();
        //                foreach (var detail in item.InvoiceDetails)
        //                {
        //                    detail.Product = de.Products.AsNoTracking().FirstOrDefault(p => p.Id == detail.ProductId);
        //                }
        //            }
        //            list = new List<Invoice>(listStatus);
        //        }

        //        //  // From Date to This Date 


        //        // Show List
        //        else
        //        {
        //            var listAll = de.Invoices.AsNoTracking().OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);
        //            foreach (var item in listAll)
        //            {
        //                item.User = de.AspNetUsers.AsNoTracking().FirstOrDefault(p => p.Id == item.UserId);
        //                item.InvoiceDetails = de.InvoiceDetails.AsNoTracking().Where(p => p.InvoiceId == item.Id).ToList();
        //                foreach (var detail in item.InvoiceDetails)
        //                {
        //                    detail.Product = de.Products.AsNoTracking().FirstOrDefault(p => p.Id == detail.ProductId);
        //                }
        //            }
        //            list = new List<Invoice>(listAll);

        //            // return Ok(new { check = true, ms = "This is a List with didnt condition!", data = list, total = list.TotalItemCount });
        //        }


        //        var newList = new List<Invoice>();

        //        if (fromDate != "")
        //        {
        //            var Datew = DateTime.Parse(fromDate);


        //            foreach (var item in list)
        //            {
        //                if (item.DateCreate > Datew)
        //                {
        //                    newList.Add(item);
        //                }

        //            }


        //            /* foreach (var item in list)
        //             {
        //                 item.User = de.AspNetUsers.AsNoTracking().FirstOrDefault(p => p.Id == item.UserId);
        //                 item.InvoiceDetails = de.InvoiceDetails.AsNoTracking().Where(p => p.InvoiceId == item.Id).ToList();
        //                 foreach (var detail in item.InvoiceDetails)
        //                 {
        //                     detail.Product = de.Products.AsNoTracking().FirstOrDefault(p => p.Id == detail.ProductId);
        //                 }
        //             }
        //             return Ok(new { check = true, ms = "FromDay to This day", data = list, total = list.TotalItemCount });*/
        //        }
        //        else
        //        {
        //            newList = new List<Invoice>(list);
        //        }

        //        return Ok(new { check = true, ms = " Fillter Success!", data = newList, total = newList.Count });

        //    }
        //}
       
        [HttpGet("[action]")]
        public ActionResult GetDetailInvoice(Guid? id)
        {
            using var de = new DataEntities();
            de.Configuration.LazyLoadingEnabled = false;
            de.Configuration.ProxyCreationEnabled = false;

            var list = de.Invoices.AsNoTracking().Where(p => p.Id == id).ToList();
            foreach (var item in list)
            {
                item.User = de.AspNetUsers.AsNoTracking().FirstOrDefault(p => p.Id == item.UserId);
                item.InvoiceDetails = de.InvoiceDetails.AsNoTracking().Where(p => p.InvoiceId == item.Id).ToList();
                foreach (var detail in item.InvoiceDetails)
                {
                    detail.Product = de.Products.AsNoTracking().FirstOrDefault(p => p.Id == detail.ProductId);
                }
            }
            //   var listorderdetail = de.InvoiceDetails.AsNoTracking().FirstOrDefault();
            return Ok(new { check = true, ms = "success!", data = list });
        }
        //Update status Order
        [HttpPost("[action]")]
        public ActionResult UpdateStatusInvoice(Guid? InvoiceId, int? status)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!" });
            using var de = new DataEntities();
            if (InvoiceId != null && status != null)
            {
                var invoice = de.Invoices.Where(p => p.Id == InvoiceId).FirstOrDefault();
                if (invoice == null)
                    return Ok(new { check = false, ms = "Can't found invoice" });
                switch (status)
                {
                    case 1:
                        invoice.DeliveryStatus = Enum_DeliveryStatus.Confirmed; //
                        invoice.DateUpdate = DateTime.Now;
                        de.SaveChanges();
                        break;
                    case 2:
                        invoice.DeliveryStatus = Enum_DeliveryStatus.Cancelled; //
                        invoice.DateUpdate = DateTime.Now;

                        de.SaveChanges();
                        break;

                }
            }
            return Ok(new { check = true, ms = "Update Status success !!!" });

        }
        //[HttpPost("[action]")]
        //public ActionResult UpdateStatusDeliveryDeInvoice(Guid? InvoiceId, int? status)
        //{
        //    var user = C_User.Auth(Request.Headers["Authorization"]);
        //    if (user == null)
        //        return Ok(new { check = false, ms = "Your login session has expired, please login again!" });
        //    using (var de = new DataEntities())
        //    {
        //        if (InvoiceId != null && status != null)
        //        {
        //            var invoice = de.Invoices.Where(p => p.Id == InvoiceId).FirstOrDefault();
        //            switch (status)
        //            {
        //                case 1:
        //                    invoice.DeliveryStatus = "Deliveried"; //
        //                    de.SaveChanges();

        //                    if (invoice.DeliveryStatus == "Deliveried")
        //                    {

        //                        var upindetail = de.InvoiceDetails.Where(p => p.InvoiceId == InvoiceId).ToList();
        //                        foreach (var up in upindetail)
        //                        {
        //                            var pro = de.Products.Where(p => p.Id == up.ProductId).ToList();
        //                            foreach (var upd in pro)
        //                            {
        //                                //upd.Counts = upd.Counts - 1;
        //                                //if (upd.Counts == 0)
        //                                //{
        //                                //    upd.Status = "Out-off";
        //                                //}
        //                                de.SaveChanges();
        //                            }
        //                        }
        //                    }

        //                    break;
        //                case 2:
        //                    invoice.DeliveryStatus = "Cancelled"; //
        //                    de.SaveChanges();
        //                    break;
        //            }
        //        }
        //        return Ok(new { check = true, ms = "Update Status success !!!" });
        //    }

        //}
        public static bool CheckAccept(string role, string id)
        {
            using var de = new DataEntities();
            var getAllRole = de.AspNetUserRoles.Where(p => p.UserId == id).ToList();
            var check = false;
            var check1 = false;
            foreach (var rolen in getAllRole)
            {
                var name = de.AspNetRoles.FirstOrDefault(p => p.Id == rolen.RoleId)?.Name;
                if (name == null)
                    return false;
                if (name == "Admin")
                {
                    check = true;
                }
                if (name == role || name == "AdminAll")
                {
                    check1 = true;
                }

            }
            if (check == false || check1 == false)
            {
                return false;
            }
            else
            {
                return true;
            }

        }
        //Get all User
        [HttpGet("[action]")]
        public ActionResult GetAllUser(int page = 1, int limit = 20, string? searchName = "", string? searchCountry = "", string? searchRole = "", string? statusRole = "")
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, status = 201, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            using var de = new DataEntities();
            var listall = new List<AspNetUser>();

            /* var getAllRole = de.AspNetUserRoles.Where(p => p.UserId == user.Id).ToList();
             var check = false;
             var check1 = false;
             foreach (var rolen in getAllRole)
             {
                 var name = de.AspNetRoles.FirstOrDefault(p => p.Id == rolen.RoleId).Name;
                 if (name == "Admin")
                 {
                     check = true;
                 }
                 if (name == "AdMangerUser"||name == "AdminAll")
                 {
                     check1 = true;
                 }

             }*/
            if (CheckAccept("AdMangerUser", user.Id) == false)
            {
                return Ok(new { check = false, status = 202, ms = "Accept denied! " });
            }

            de.Configuration.LazyLoadingEnabled = false; // 1 cai doc 1 cai ghi
            de.Configuration.ProxyCreationEnabled = false;
            int count = 0;
            //var getcate = from r in de.Categories select r;
            if (!string.IsNullOrEmpty(searchName))
            {
                var list = de.AspNetUsers.AsNoTracking().OrderByDescending(p => p.DateCreate).Where(p => p.FullName.Contains(searchName)).OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);
                count = list.TotalItemCount;

                foreach (var item in list)
                {
                    item.AspNetUserRoles = de.AspNetUserRoles.AsNoTracking().Where(p => p.UserId == item.Id).ToList();
                    foreach (var detail in item.AspNetUserRoles)
                    {
                        detail.AspNetRole = de.AspNetRoles.AsNoTracking().FirstOrDefault(p => p.Id == detail.RoleId);
                    }

                }
                listall = new List<AspNetUser>(list);
            }
            else if (!string.IsNullOrEmpty(searchCountry))
            {
                var list = de.AspNetUsers.AsNoTracking().Where(p => p.Country.Contains(searchCountry)).OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);
                count = list.TotalItemCount;
                foreach (var item in list)
                {
                    item.AspNetUserRoles = de.AspNetUserRoles.AsNoTracking().Where(p => p.UserId == item.Id).ToList();
                    foreach (var detail in item.AspNetUserRoles)
                    {
                        detail.AspNetRole = de.AspNetRoles.AsNoTracking().FirstOrDefault(p => p.Id == detail.RoleId);
                    }
                }
                listall = new List<AspNetUser>(list);
            }
            ///
            else if (!string.IsNullOrEmpty(searchRole))
            {

                var listnamerole = de.AspNetRoles.AsNoTracking().FirstOrDefault(p => p.Name == searchRole);
                if (listnamerole == null)
                    return Ok(new { check = false, ms = "Can't found this role" });
                var listrole = de.AspNetUserRoles.AsNoTracking().Where(p => p.RoleId == listnamerole.Id).OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);

                foreach (var itme in listrole)
                {
                    itme.NameRole = de.AspNetRoles.AsNoTracking().FirstOrDefault(p => p.Id == itme.RoleId)?.Name;

                    itme.InfoUser = de.AspNetUsers.AsNoTracking().FirstOrDefault(p => p.Id == itme.UserId);
                }
                return Ok(new { check = true, ms = " Search by role ", data = listrole, total = listrole.TotalItemCount });
            }

            else if (statusRole != "")
            {

                // var listrole = de.AspNetUserRoles.AsNoTracking().Where(p =>  p.Status == statusRole).ToPagedList(page, limit);
                var listrolee = (from i in de.AspNetUsers
                                 join p in de.AspNetUserRoles on i.Id equals p.UserId
                                 join r in de.AspNetRoles on p.RoleId equals r.Id
                                 where p.Status == statusRole
                                 select i).ToList();

                count = listrolee.Count;

                foreach (var item in listrolee)
                {
                    item.AspNetUserRoles = de.AspNetUserRoles.AsNoTracking().Where(p => p.UserId == item.Id).ToList();
                    foreach (var detail in item.AspNetUserRoles)
                    {
                        detail.AspNetRole = de.AspNetRoles.AsNoTracking().FirstOrDefault(p => p.Id == detail.RoleId);
                    }
                }
                listall = new List<AspNetUser>(listrolee);








                /* foreach (var itme in listrole)
                 {
                     itme.NameRole = de.AspNetRoles.AsNoTracking().FirstOrDefault(p => p.Id == itme.RoleId).Name;

                     itme.infoUser = de.AspNetUsers.AsNoTracking().FirstOrDefault(p => p.Id == itme.UserId);
                 }*/
                //  return Ok(new { check = true, ms = " okok", data = listrolee, total = listrolee.Count() });
            }

            else
            {
                var list = de.AspNetUsers.AsNoTracking().OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);
                count = list.TotalItemCount;

                foreach (var item in list)
                {
                    item.AspNetUserRoles = de.AspNetUserRoles.AsNoTracking().Where(p => p.UserId == item.Id).ToList();
                    foreach (var detail in item.AspNetUserRoles)
                    {
                        detail.AspNetRole = de.AspNetRoles.AsNoTracking().FirstOrDefault(p => p.Id == detail.RoleId);
                    }
                }
                listall = new List<AspNetUser>(list);

            }

            return Ok(new { check = true, ms = "success!", data = listall, total = count });

        }
        //Get Detail user
        [HttpGet("[action]")]
        public ActionResult GetDetailUser(string? UserId = "")
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            using var de = new DataEntities();
            if (CheckAccept("AdMangerUser", user.Id) == false)
            {
                return Ok(new { check = false, ms = "Accept denied! " });
            }


            de.Configuration.LazyLoadingEnabled = false;
            de.Configuration.ProxyCreationEnabled = false;

            var list = de.AspNetUsers.AsNoTracking().Where(p => p.Id == UserId).ToList();
            return Ok(new { check = true, ms = "Get Detail user success!", data = list });

        }
        [HttpPost("[action]")]
        public ActionResult DeleteUser(string? IdUser = "")
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            using var de = new DataEntities();
            var UserId = de.AspNetUsers.First(m => m.Id == IdUser);
            de.AspNetUsers.Remove(UserId);
            de.SaveChanges();
            return Ok(new { check = true, ms = "Delete user complete! " });
        }
        /* [HttpPost("[action]")]
         public ActionResult DeleteMultiUser(IEnumerable<string> UsersId)
         {
             var user = C_User.Auth(Request.Headers["Authorization"]);
             if (user == null)
                 return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });
             using (var de = new DataEntities())
             {
                 foreach (var id in UsersId)
                 {
                     var users = de.AspNetUsers.Single(s => s.Id == id );

                     de.AspNetUsers.Remove(users);
                 }
                 de.SaveChanges();
                 return Ok(new { check = true, ms = "Delete multi users complete! " });
             }
         }*/
        [HttpPost("[action]")]
        public ActionResult DeleteMultiUser(IEnumerable<string> UsersId)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });
            using var de = new DataEntities();
            if (CheckAccept("AdMangerUser", user.Id) == false)
            {
                return Ok(new { check = false, ms = "Accept denied! " });
            }

            foreach (var id in UsersId)
            {
                var users = de.AspNetUsers.Single(s => s.Id == id);
                var proseller = de.Products.Where(p => p.UserId == id).ToList();
                foreach (var product in proseller)
                {
                    de.Products.Remove(product);
                    var imgs = de.ProductImages.Where(p => p.ProductId == product.Id).ToList();
                    foreach (var delimg in imgs)
                    {
                        de.ProductImages.Remove(delimg);
                    }
                }
                var userinvoice = de.Invoices.Where(p => p.ShopId == id).ToList();
                if (userinvoice != null)
                {
                    foreach (var invoice in userinvoice)
                    {
                        de.Invoices.Remove(invoice);
                        var deltailinvoice = de.InvoiceDetails.Where(p => p.InvoiceId == invoice.Id).ToList();
                        foreach (var detailiv in deltailinvoice)
                        {
                            de.InvoiceDetails.Remove(detailiv);
                        }
                    }
                }


                de.AspNetUsers.Remove(users);
            }
            de.SaveChanges();
            return Ok(new { check = true, ms = "Delete multi users complete! " });
        }

        [HttpGet("[action]")]
        public ActionResult GetAllRole()
        {
            using var de = new DataEntities();
            de.Configuration.LazyLoadingEnabled = false;
            de.Configuration.ProxyCreationEnabled = false;
            //    var getrole = de.AspNetRoles.AsNoTracking().OrderByDescending(p => p.Name).ToList();
            var ge = from i in de.AspNetRoles.ToList()
                     select new DataRoles
                     {
                         Id = i.Id,
                         Name = i.Name
                     };
            return Ok(new { check = true, status = 200, ms = "check success", data = ge });

        }
        [HttpPost("[action]")]
        public ActionResult ChangeRole(string? type = "", string? userId = "", string? role = "")
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            using var de = new DataEntities();
            if (CheckAccept("AdminUser", user.Id) == false)
            {
                return Ok(new { check = false, ms = "Accept denied! " });
            }

            if (type == "delete")
            {
                if (role == null && userId == null)
                    return Ok(new { check = false, ms = "Field cannot be left blank" });
                else
                {
                    var del = de.AspNetUserRoles.FirstOrDefault(p => p.UserId == userId && p.RoleId == role);
                    de.AspNetUserRoles.Remove(del);
                    de.SaveChanges();
                    return Ok(new { check = true, ms = "Delete Role user successfully!" });
                }
            }
            else
            {
                var item7 = new AspNetUserRole
                {
                    UserId = userId,
                    RoleId = role,
                    Status = "Active",
                };
                de.AspNetUserRoles.Add(item7);
                de.SaveChanges();
                return Ok(new { check = true, ms = "Add role user successfully!" });
                /* switch (role)
                 {

                     case "1":

                         var item3 = new AspNetUserRole
                         {
                             UserId = userId,
                             RoleId = "EFB46479-B57F-499F-AD70-B93022134EE1", //ADmin
                             Status = "Active",

                         };

                         de.AspNetUserRoles.Add(item3);
                         de.SaveChanges();
                           return Ok(new { check = true, ms = "Add role user successfully!" });
                        // break;
                     case "3":

                         var item = new AspNetUserRole
                         {
                             UserId = userId,
                             RoleId = "4B328254-E686-497B-9580-7BD97611D74A", //seller
                             Status = "Active",

                         };



                           var checklicense=   de.BusinessLicenses.FirstOrDefault(p => p.ShopId == userId);
                         if (checklicense == null )
                           {
                                 var license = new BusinessLicense
                                   {
                                       Id = Guid.NewGuid(),
                                       ShopId = userId,
                                       Status = "Open"
                                   };
                           de.BusinessLicenses.Add(license);
                             de.SaveChanges();
                         }

                         de.AspNetUserRoles.Add(item);
                         de.SaveChanges();
                         return Ok(new { check = true, ms = "Add role user successfully!" });
                        // break;
                     case "4":
                         var item2 = new AspNetUserRole
                         {
                             UserId = userId,
                             RoleId = "4HB46479-B57F-499F-AD70-B93022134EE1", //pool
                             Status = "Active",
                         };
                         de.AspNetUserRoles.Add(item2);
                         de.SaveChanges();
                         return Ok(new { check = true, ms = "Add role user successfully!" });
                       //  break;

                    case "5":

                         var adminuser = de.AspNetUserRoles.FirstOrDefault(p => p.RoleId == "EFB46479-B57F-499F-AD70-B93022134EE1" && p.UserId == userId);
                         if (adminuser != null && adminuser.Status == "Active")
                         {
                             var item5 = new AspNetUserRole
                             {
                                 UserId = userId,
                                 RoleId = "EDFDS35-B57F-499F-AD70-B93022134EE1",
                                 Status = "Active",
                             };
                             de.AspNetUserRoles.Add(item5);
                         }

                         de.SaveChanges();
                         return Ok(new { check = true, ms = "Add role user successfully!" });
                     // break;
                     default:
                         return Ok(new { check = false, ms = "add false!" });
                 }
*/
            }

        }
        //update active role
        [HttpPost("[action]")]
        public ActionResult UpdateUser(string? UserId = "", string? RoleId = "", int? Status = 0)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });
            using var de = new DataEntities();
            if (CheckAccept("AdMangerUser", user.Id) == false)
            {
                return Ok(new { check = false, ms = "Accept denied! " });
            }
            var users = de.AspNetUserRoles.FirstOrDefault(p => p.UserId == UserId
                                                         && p.RoleId == RoleId);
            if (users != null)
            {
                switch (Status)
                {
                    case 1:
                        users.Status = "Active";

                        var license = new BusinessLicense()
                        {
                            Id = Guid.NewGuid(),
                            ShopId = UserId,
                            Status = Enum_BusinessLicense.New,
                            BusinessCode = "Unknow",
                            Name = "Unknow",
                            Role = "Unknow",
                            DateUpdate = DateTime.Now,
                            DateCreate = DateTime.Now,

                        };
                        de.BusinessLicenses.Add(license);

                        de.SaveChanges();
                        break;
                    case 2:
                        users.Status = "Deny";
                        de.SaveChanges();
                        break;
                }
            }
            return Ok(new { check = true, ms = "User permissions have been updated!" });
        }
        // Update profile of someone
        [HttpPost("[action]")]
        public async Task<ActionResult> ProfileUser([FromForm] ProfileRequest model)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            using (var de = new DataEntities())
            {

                if (CheckAccept("AdMangerUser", user.Id) == false)
                {
                    return Ok(new { check = false, ms = "Accept denied! " });
                }

                var userx = de.AspNetUsers.FirstOrDefault(p => p.Id == model.Id);

                if (userx != null && model.lockstatus == true)
                {
                    userx.Lock = true;
                    de.SaveChanges();
                }

                else if (userx != null)
                {
                    if (model.lockstatus == false)
                    {
                        userx.Lock = false;
                        de.SaveChanges();
                    }
                    else
                    {
                        userx.FullName = model.FullName;
                        userx.PhoneNumber = model.PhoneNumber;
                        userx.Email = model.Email;
                        userx.Address = model.Address;
                        userx.Country = model.Country;
                        userx.BirthDay = model.BirthDay;
                        userx.Gender = model.gender;

                        if (model.Avatar != null)
                        {
                            var requestImg = await C_Request.UploadImage(model.Avatar, 500);
                            if (!requestImg.Status)
                                return Ok(new { check = false, ms = requestImg.Message });
                            userx.Avatar = requestImg.Result.Url;
                        }
                        de.SaveChanges();
                    }


                }


            }

            //     if(model.lockstatus== )

            return Ok(new { check = true, ms = "Update Successfully" });

        }
        //Update Category
        [HttpPost("[action]")]
        public async Task<ActionResult> UpdateCategory([FromForm] CategoryRequest model)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            if (string.IsNullOrEmpty(model.Name))
                return Ok(new { check = false, ms = "Field cannot be left blank" });

            using var de = new DataEntities();

            if (de.Categories.Any(p => p.Name.ToUpper() == model.Name.ToUpper()))
                return Ok(new { check = false, ms = "Cate name already exists" });

            var c = 0;
            var slug = model.Slug;
            if (string.IsNullOrEmpty(slug))
                slug = Tool.LocDauUrl(model.Name);
            if (model.Id == null || model.Id == Guid.Empty)
            {


                var item = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    ParentId = model.ParentId,
                    Slug = model.Slug
                };

                if (model.Image != null)
                {
                    var requestImg = await C_Request.UploadImage(model.Image, 500);
                    if (!requestImg.Status)
                        return Ok(new { check = false, ms = requestImg.Message });

                    item.Image = requestImg.Result.Url;

                }

                if (model.Icon != null)
                {
                    var requestImg = await C_Request.UploadImage(model.Icon, 200);
                    if (!requestImg.Status)
                        return Ok(new { check = false, ms = requestImg.Message });
                    item.Icon = requestImg.Result.Url;
                }
                de.Categories.Add(item);
            }
            else
            {
                var cate = de.Categories.FirstOrDefault(p => p.Id == model.Id);
                if (cate != null)
                {
                    while (de.Categories.AsNoTracking().Any(p => p.Slug == slug && p.Id != cate.Id))
                    {
                        c++;
                        slug = Tool.LocDauUrl(model.Name) + "-" + c;
                    }

                    cate.Name = model.Name;
                    cate.Slug = slug;

                    if (model.ParentId != null)
                        cate.ParentId = model.ParentId;

                    if (model.Image != null)
                    {
                        var requestImg = await C_Request.UploadImage(model.Image, 500);
                        if (!requestImg.Status)
                            return Ok(new { check = false, ms = requestImg.Message });
                        cate.Image = requestImg.Result.Url;
                    }

                    if (model.Icon != null)
                    {
                        var requestImg = await C_Request.UploadImage(model.Icon, 200);
                        if (!requestImg.Status)
                            return Ok(new { check = false, ms = requestImg.Message });
                        cate.Icon = requestImg.Result.Url;
                    }

                }
            }
            de.SaveChanges();
            return Ok(new { check = true, ms = "Update Successfully" });

        }

        // Get All List Coin
        /* [HttpGet("[action]")]
         public ActionResult GetAllListCoin(int page = 1, int limit = 20,string type = "")
         {
             using (var de = new DataEntities())
             {
                 if(type != "")
                 {
                     var upde = de.ListCoins.AsNoTracking().OrderByDescending(p => p.DateCreate).Where(p => p.Status == "Active").ToPagedList(page, limit);
                     return Ok(new { check = true, ms = "Get all success!", data = upde });
                 }
                 else
                 {
                     var upde = de.ListCoins.AsNoTracking().OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);
                     return Ok(new { check = true, ms = "Get all success!", data = upde });
                 }

             }
         }*/


        // Route accept List coin

        // 
        [HttpGet("[action]")] // update status list coin
        public ActionResult UpdateStatusListCoin(Guid Id, int status, string? reasonreject)
        {
            using var de = new DataEntities();

            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            /*  if (CheckAccept("A", user.Id) == false)
              {
                  return Ok(new { check = false, ms = "Accept denied! " });
              }*/

            var upd = de.ListCoins.FirstOrDefault(p => p.Id == Id);
            if (upd == null)
                return Ok(new { check = false, ms = "Can't found List coin" });
            var nameu = de.AspNetUsers.FirstOrDefault(p => p.Id == upd.UserId)?.FullName;
            if (upd != null)
            {
                if (status == Enum_ListCoinStatus.Reject)
                {
                    var content = $"Your project has been Cancelled \n\n " +
                        "\n" +
                       "" +
                       "\n + " +
                        "------------------------------------------------\n" +
                        "";
                    var nrj = new RejectReaseon
                    {
                        Id = Guid.NewGuid(),
                        IdListcoin = upd.Id,
                        Reason = reasonreject,
                        DateCreate = DateTime.Now,
                    };
                    de.RejectReaseons.Add(nrj);


                    Tool.SendMail("[UNIDI] MESSAGE", content, upd.EmailUser);
                }
                else if (status == Enum_ListCoinStatus.Review)
                {
                    var exp = (DateTime.Now);

                    var claim = new List<Claim> {
                                new Claim("id", upd.Id.ToString()),
                                new Claim("email", upd.EmailUser)
                            };
                    var jwt = Tool.EnJwtToken(claim, exp);


                    var content = $"<div class=\" font-family: Arial, sans-serif;background-color: #f0f0f0;margin: 0;padding: 0\">\r\n    <div class=\"container\"\r\n        style=\"  width:100%;margin: 0 auto; background-color: #fff;border: 1px solid #ddd;box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);\">\r\n        <div class=\"header\" style=\" background-color: #007bff;text-align: center;\r\n        color: #fff;\r\n        padding: 20px;\">\r\n            <img style=\"max-width: 150px;\" src=\"https://unidi.net/Content/overview/img/Unidi%20Logo1.png\"\r\n                alt=\"Company Logo\">\r\n        </div>\r\n        <div class=\"content\" style=\"padding: 0 16px; \">\r\n            <p>Dear" + nameu + " </p>\r\n            <h3>We are pleased to inform you that you have successfully registered an account with UNIDI.</h3>\r\n            <p>Please select a package and proceed with payment here.</p>\r\n            <a href=\"" + urlFrontEnd + "?verify=" + jwt + "\">\r\n                <button class=\"custom-button\"\r\n                    style=\" display: inline-block;padding: 10px 20px;font-size: 16px; background-color: #007bff;color: white;border: 2px solid #007bff; border-radius:5px; cursor: pointer;\">Choose\r\n                    Package</button>\r\n            </a>\r\n            <p>If you need assistance or have any questions, please don't hesitate to contact us at <a\r\n                    href=\"mailto:[Support Email]\">udini@gmail.com</a> or call us at <a\r\n                    href=\"tel:[Support Phone Number]\">0909090</a>. We are always here to assist you.</p>\r\n        </div>\r\n        <div class=\"footer\" style=\"text-align: left;  background-color: #f0f0f0;\r\n        padding:10px;\">\r\n            <p>Thank you for choosing <b>UNIDI</b> as your partner. <br> We look forward to providing you with the\r\n                best experiences.</p>\r\n            <p>Best regards,<br>" + nameu + "<br>UNIDI</p>\r\n            <p><a style=\" color: #007bff; cursor: pointer;\r\n                text-decoration: none;\" href=\"https://unidi.net/\">unidi.net</a></p>\r\n        </div>\r\n    </div>\r\n</div>";

                    Tool.SendMail("[UNIDI] MESSAGE", content, upd.EmailUser);

                }
                else if (status == Enum_ListCoinStatus.Active)   //ACTIVECOIN
                {
                    upd.DateActive = DateTime.Now;
                }

                upd.Status = status;
                de.SaveChanges();
                return Ok(new { check = true, ms = "Update success!" });
            }
            else
            {
                return Ok(new { check = false, ms = "Dont find list coin" });
            }
        }
        //
        [HttpGet("[action]")]
        public ActionResult DeleteListCoin(Guid? IdListCoin)
        {
            var de = new DataEntities();

            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            var lcoin = de.ListCoins.FirstOrDefault(p => p.Id == IdListCoin);
            if (lcoin == null)
            {
                return Ok(new { check = false, ms = "Cant found to delete" });
            }

            de.ListCoins.Remove(lcoin);
            de.SaveChanges();
            return Ok(new { check = true, ms = "Delete Success!", });
        }
        //
        [HttpGet("[action]")]
        public ActionResult DetailListCoin(Guid? Id)
        {
            using var de = new DataEntities();
            var def = de.ListCoins.FirstOrDefault(p => p.Id == Id);
            if (def == null)
                return Ok(new { check = false, ms = "Can't Found List coin " });
            var chainl = de.Chains.FirstOrDefault(p => p.ChainName == def.ChainName);
            if (chainl == null)
            {
                return Ok(new { check = false, ms = "Can't found this chain" });
            }

            return Ok(new { check = true, ms = "detail listcoin: ", data = def });
        }

        // ACtive transaction to add  money to user wallet                  ERORRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRR
        //[HttpPost("[action]")]
        //public ActionResult UpdateStatusTransaction(Guid Id, string status = "", string? note = "")
        //{
        //    using (var de = new DataEntities())
        //    {
        //        var upt = de.Transactions.FirstOrDefault(p => p.Id == Id && p.Status == "Open" && p.Type == 1);  // WITHDRAW


        //        if (upt != null)
        //        {
        //            if (status == "deny")
        //            {
        //                upt.Status = "Denied";

        //                if (note != "")
        //                {
        //                    upt.Note = note;
        //                }

        //                upt.DateUpdate = DateTime.Now;
        //                de.SaveChanges();
        //                return Ok(new { check = true, ms = "Deny success!" });
        //            }
        //            else if (status == "active")  // ONLY WITHDRAW
        //            {
        //                upt.Status = "Active";
        //                upt.DateUpdate = DateTime.Now;

        //                var upe = de.Wallets.FirstOrDefault(p => p.UserId == upt.UserId && p.Id == upt.WalletType);  // id wallet == id transaction wallettype

        //                var upad = de.Wallets.FirstOrDefault(p => p.Role == "Admin" && p.IdListCoin == upe.IdListCoin); // WALLET ADMIN
        //                if (upe != null)
        //                {
        //                    upe.Balance += upt.Amount;    // wall was add amount
        //                    upe.DateCreate = DateTime.Now;
        //                    upad.Balance -= upe.Balance;  // minus balance from admin and add to user

        //                }

        //                de.SaveChanges();
        //                return Ok(new { check = true, ms = "Active success!" });
        //            }
        //            else
        //            {
        //                return Ok(new { check = false, ms = "Missing parameter..." });
        //            }
        //        }
        //        else
        //            return Ok(new { check = false, ms = "dont found thing to updatestatus" });
        //    }


        //}

        [HttpPost("[action]")]
        public ActionResult UpdateDetailPackage([FromForm] AddDetailRequest model)
        {
            using var de = new DataEntities();
            if (string.IsNullOrEmpty(model.Id))
            {
                var p = new DetailPakage
                {
                    Id = Guid.NewGuid().ToString(),
                    Service = model.Service,
                    Details = model.Details
                };
                de.DetailPakages.Add(p);
            }
            else
            {
                var chk = de.DetailPakages.FirstOrDefault(p => p.Id == model.Id);
                if (chk != null)
                {
                    chk.Service = model.Service;
                    chk.Details = model.Details;
                }
                else
                {
                    return Ok(new { check = false, ms = "wrong id or this one is not exist" });
                }
            }


            de.SaveChanges();
            return Ok(new { check = true, ms = "Update Success!" });
        }
       
    }
}



