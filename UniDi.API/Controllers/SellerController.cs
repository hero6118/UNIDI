using Core.Models.Request;
using Core;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;
using Swashbuckle.AspNetCore.Annotations;
using Core.Models.Response;
using UniDi.API.Models;
using System.Runtime.Intrinsics.X86;
using Microsoft.VisualBasic;

namespace UniDi.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SellerController : ControllerBase
    {
        // update licensee //
        [HttpPost("[Action]")]
        [SwaggerOperation(Description = "if type == null update License shop, else update profile shop")]
        public async Task<ActionResult> UpdateBusinessLicense([FromForm] LicenseRequest model, string? type = "")
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            if (string.IsNullOrEmpty(model.Name))
                return Ok(new { check = false, ms = "Field cannot be left blank" });

            using (var de = new DataEntities())
            {
                var license = de.BusinessLicenses.FirstOrDefault(p => p.ShopId == user.Id);

                if (string.IsNullOrEmpty(model.Name))
                {
                    return Ok(new { check = false, ms = "Field cannot be null" });
                }

                var rolesta = de.AspNetUserRoles.FirstOrDefault(p => p.UserId == user.Id && p.Status == "Active");

                if (rolesta != null)
                {
                    if (license != null)
                    {
                        if (type == "")    //if type = null update license
                        {
                            license.Name = model.Name;
                            license.ShopId = user.Id;
                            license.Role = model.Role;
                            license.Status = Enum_BusinessLicense.Waiting;
                            license.BusinessCode = model.BusinessCode;
                            license.DateUpdate = DateTime.Now;
                            license.NoteStatus = "";
                            if (model.LicenseImage != null)
                            {
                                var requestImg = await C_Request.UploadImage(model.LicenseImage, 500);
                                if (!requestImg.Status)
                                    return Ok(new { check = false, ms = requestImg.Message });
                                license.LicenseImage = requestImg.Result.Url;
                            }
                            if (model.DistributionAgentContractImage != null)
                            {
                                var requestImg = await C_Request.UploadImage(model.DistributionAgentContractImage, 200);
                                if (!requestImg.Status)
                                    return Ok(new { check = false, ms = requestImg.Message });
                                license.DistributionAgentContractImage = requestImg.Result.Url;
                            }
                            if (model.Logo != null)
                            {
                                var requestImg = await C_Request.UploadImage(model.Logo, 200);
                                if (!requestImg.Status)
                                    return Ok(new { check = false, ms = requestImg.Message });
                                license.Logo = requestImg.Result.Url;
                            }

                        }
                        else   // if type != null  update profile of shop
                        {
                            license.Name = model.Name;
                            license.Description = model.Description;
                            license.DateUpdate = DateTime.Now;
                            if (model.Logo != null)
                            {
                                var requestImg = await C_Request.UploadImage(model.Logo, 200);
                                if (!requestImg.Status)
                                    return Ok(new { check = false, ms = requestImg.Message });
                                license.Logo = requestImg.Result.Url;
                            }
                        }
                        de.SaveChanges();
                        return Ok(new { check = true, ms = "Update License Successfully" });
                    }
                    else
                        return Ok(new { check = false, ms = "You are not seller" });
                }
                else
                    return Ok(new { check = false, ms = "Wating to active" });
            }
        }
        [HttpGet("[action]")]
        [SwaggerOperation(Description = "get info bussinesslicense shop")]
        public BussinesInfoShppResponse InfoShopBussinesLicense()
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return new BussinesInfoShppResponse { Check = false, Ms = "Your login session has expired, please login again!" };

            using var de = new DataEntities();
            var infoshop = de.BusinessLicenses.AsNoTracking().FirstOrDefault(p => p.ShopId == user.Id);
            if (infoshop == null)
                return new BussinesInfoShppResponse { Check = false, Ms = "You lack a business license" };

            return new BussinesInfoShppResponse { Check = true, Ms = "You lack a business license",Data = infoshop };

        }


        //get status of license
        [HttpGet("[Action]")]
        [SwaggerOperation(Description = "enum to string to read")]
        public ActionResult GetStatusLicense()
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!" });

            using (var de = new DataEntities())
            {
                de.Configuration.LazyLoadingEnabled = false; // 1 cai doc 1 cai ghi
                de.Configuration.ProxyCreationEnabled = false;

                var license = de.BusinessLicenses.AsNoTracking().FirstOrDefault(p => p.ShopId == user.Id);
                if (license == null)
                {
                    return Ok(new { check = false, ms = "Waiting for active seller account!" });
                }
                return Ok(new { check = true, data = license.Status, ms = "Success!" });
            }
        }
        // Update license
        /*  [HttpPost("[Action]")]
          public async Task<ActionResult> UpdateProductLicense([FromForm] ProductLicenseRequest model)
          {
              var user = C_User.Auth(Request.Headers["Authorization"]);
              if (user == null)
                  return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

              if (string.IsNullOrEmpty(model.Name))
                  return Ok(new { check = false, ms = "Field cannot be left blank" });

              using (var de = new DataEntities())
              {
                  var item = new ProductLicense
                  {
                      Id = Guid.NewGuid(),
                      Name = model.Name,
                      ShopId = user.Id,
                      Status = "pending approval",
                  };

                  if (model.LicenseImage != null)
                  {
                      var requestImg = await C_Request.UploadImage(model.LicenseImage, 500);
                      if (!requestImg.Status)
                          return Ok(new { check = false, ms = requestImg.Message });
                      item.LicenseImage = requestImg.Result.Url;
                  }

                  if (model != null)
                  {
                      var requestImg = await C_Request.UploadImage(model.ProductImage, 200);
                      if (!requestImg.Status)
                          return Ok(new { check = false, ms = requestImg.Message });
                      item.ProductImage = requestImg.Result.Url;
                  }

                  de.ProductLicenses.Add(item);
                  de.SaveChanges();
                  return Ok(new { check = true, ms = "Update License Successfully" });
              }
          }*/
        //Staticial about order and product
        [HttpGet("[Action]")]
        public ActionResult GetToTal()
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            using (var de = new DataEntities())
            {
                var ss = new Dictionary<string, string>();
                var up = new Dictionary<string, string>();

                //ORDER
                var tconfirm = de.Invoices.AsNoTracking().Where(p => p.ShopId == user.Id && p.DeliveryStatus == Enum_DeliveryStatus.New);
                var tconfirmwaiting = de.Invoices.AsNoTracking().Where(p => p.ShopId == user.Id && p.DeliveryStatus == Enum_DeliveryStatus.Confirmed);
                var tcancel = de.Invoices.AsNoTracking().Where(p => p.ShopId == user.Id && p.DeliveryStatus == Enum_DeliveryStatus.Cancelled);

                var processed = de.Invoices.AsNoTracking().Where(p => p.ShopId == user.Id && p.DeliveryStatus == Enum_DeliveryStatus.Delivering);
                var tcancelled = de.Invoices.AsNoTracking().Where(p => p.ShopId == user.Id && p.DeliveryStatus == Enum_DeliveryStatus.CustomerCancels);
                var ordcancelled = de.Invoices.AsNoTracking().Where(p => p.ShopId == user.Id && p.DeliveryStatus == Enum_DeliveryStatus.Cancelled);
                //PRODUCTS
                var product = de.Products.AsNoTracking().Where(p => p.UserId == user.Id);
                var tprocancelled = product.Where(p => p.Status == Enum_ProductStatus.Cancel);
                var productactive = product.Where(p => p.Status == Enum_ProductStatus.Active);
                var productwaiting = product.Where(p => p.Status == Enum_ProductStatus.Waiting);
                var tprooutofstock = product.Where(p => p.QuantityAvailable == 0);

                var totalview = de.CViewLogs.AsNoTracking().Where(p => p.ShopId == user.Id);
                var totalviewproduct = product.Sum(p => p.CountView) ?? 0;
                var NameHighestViewProduct = product.OrderByDescending(p => p.CountView).FirstOrDefault();

                ss.Add("NewOrder", tconfirm.Count().ToString());
                ss.Add("ConfirmOrder", tconfirmwaiting.Count().ToString());
                ss.Add("Delivering", processed.Count().ToString());
                ss.Add("CustomerCancelOrder", tcancelled.Count().ToString());
                ss.Add("OrderCancel", ordcancelled.Count().ToString());

                //PRODUCT
                ss.Add("ProCancelled", tprocancelled.Count().ToString());
                ss.Add("ProOutOfStock", tprooutofstock.Count().ToString());
                ss.Add("AllProduct", product.Count().ToString());
                ss.Add("ProductActive", productactive.Count().ToString());
                ss.Add("ProductWaiting", productwaiting.Count().ToString());
                ss.Add("CountViewProduct", totalviewproduct.ToString());

                //Count view
                ss.Add("ToTalViewShop", totalview.Count().ToString());

                if (NameHighestViewProduct != null)
                {
                    up.Add("NameHighestViewProduct", NameHighestViewProduct.Name);
                    up.Add("CountViewOfNameHightest", NameHighestViewProduct?.CountView?.ToString() ?? "N/A");
                }

                var alldict = ss.Concat(up).ToDictionary(p => p.Key, p => p.Value);


                return Ok(new { check = true, ms = "get dashboard", data = alldict });
            }
        }

        //Get  all transanctions
        /*  [HttpGet("[action]")]
          public ActionResult GetAllTransactions(int page = 1, int limit = 20, int? Sku = null, string? fromdate = "")
          {
              using (var de = new DataEntities())
              {
                  de.Configuration.LazyLoadingEnabled = false;
                  de.Configuration.ProxyCreationEnabled = false;
                  var list = new List<Transaction>();

                  var user = C_User.Auth(Request.Headers["Authorization"]);
                  if (user == null)
                      return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });
                  if (Sku != null)
                  {
                      var seare = de.Invoices.AsNoTracking().Single(p => p.ShopId == user.Id && p.SKUInvoice == Sku).Id.ToString();
                      var searchlist = de.Transactions.AsNoTracking().Where(p => p.InvoiceId == seare).ToPagedList(page, limit);
                      list = new List<Transaction>(searchlist);
                  }
                  else if (fromdate != "")
                  {
                      var Datew = DateTime.Parse(fromdate);
                      var datee = de.Transactions.AsNoTracking().Where(p => p.ShopId == user.Id && p.DateCreate > Datew).ToPagedList(page, limit);
                      list = new List<Transaction>(datee);
                  }
                  else
                  {
                      var getalltransactions = de.Transactions.AsNoTracking().Where(p => p.ShopId == user.Id).OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);
                      list = new List<Transaction>(getalltransactions);
                  }
                  return Ok(new { check = true, ms = "get all transactions!!", data = list, total = list.Count });
              }
          }
  */
        //Get  all transanctions
        [HttpGet("[action]")]
        public ActionResult GetAllstatuspayment(int page = 1, int limit = 20)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            using (var de = new DataEntities())
            {
                de.Configuration.ProxyCreationEnabled = false;
                de.Configuration.LazyLoadingEnabled = false;

                var im = new Dictionary<string, double?>();

                // var totalpending = de.Invoices.AsNoTracking().Where(p=>p.ShopId == user.Id && )
                var total = de.Invoices.AsNoTracking().Where(p => p.ShopId == user.Id && p.DeliveryStatus == Enum_DeliveryStatus.Delivered).OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);
                double? totalitm = 0.0;
                foreach (var item in total)
                {
                    totalitm += item.TotalUSD;
                    //* var toll=  item.total = totalitm + item.total;*//* //
                    // im.Add("totalMoney", toll);
                }

                var datewk = DateTime.Now.AddDays(-7);
                var datemth = DateTime.Now.AddMonths(-1);
                var totalweek = de.Invoices.AsNoTracking().Where(p => p.ShopId == user.Id && p.DateCreate < datewk).OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);
                double? weeks = 0;
                foreach (var item in totalweek)
                {
                    weeks = weeks + item.TotalUSD;
                }
                var totalmonth = de.Invoices.AsNoTracking().Where(p => p.ShopId == user.Id && p.DateCreate < datemth).OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);
                double? months = 0;
                foreach (var item in totalmonth)
                {
                    months = months + item.TotalUSD;
                }

                var ToltalUnpaid = de.Invoices.AsNoTracking().Where(p => p.ShopId == user.Id && p.PaidStatus == Enum_PaidStatus.UnPaid).OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);
                double? piad = 0;
                foreach (var item in ToltalUnpaid)
                {
                    piad = piad + item.TotalUSD;
                }

                var ToTalPaid = de.Invoices.AsNoTracking().Where(p => p.ShopId == user.Id && p.PaidStatus == Enum_PaidStatus.Paid).OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);
                double? totalpaid = 0;
                foreach (var item in ToTalPaid)
                {
                    totalpaid = totalpaid + item.TotalUSD;
                }

                var yesterday = DateTime.Now.AddDays(-1);

                var ToTalInvoice = de.Invoices.AsNoTracking().Where(p => p.ShopId == user.Id && p.DateCreate < yesterday).OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);
                var countIv = ToTalInvoice.TotalItemCount;

                im.Add("PaidTotal", totalitm);
                im.Add("PaidWeek", weeks);
                im.Add("PaidMonth", months);
                im.Add("unPaid", piad);
                im.Add("tottalpaid", totalpaid);
                im.Add("countInvoice", countIv);

                return Ok(new { check = true, ms = "Status invoice of shop", data = im });
            }

        }
        [HttpPost("[action]")]
        public BasicResponse CreateVoucher([FromForm] CreateVoucherRequest model)
        {
            using var de = new DataEntities();

            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return new BasicResponse { Check = false, Ms = "Your login session has expired, please login again!" };
            if (model.PriceDiscount == null)
                return new BasicResponse { Check = false, Ms = "Please enter Price Discount" };
            if (model.Count == null)
                return new BasicResponse { Check = false, Ms = "Please enter count" };
            if (string.IsNullOrEmpty(model.NameVoucher))
                return new BasicResponse { Check = false, Ms = "Please enter Name voucher" };
            if (string.IsNullOrEmpty(model.Code))
                return new BasicResponse { Check = false, Ms = "Please enter Code voucher" };

            if (model.IdVoucher == null)
            {
                var nvoucher = new ShopVoucher()
                {
                    Id = Guid.NewGuid(),
                    Price = model.PriceDiscount,
                    Count = model.Count,
                    DateCreate = DateTime.Now,
                    NameVoucher = model.NameVoucher,
                    Code = model.Code,
                    ShopId = user.Id,
                };
                de.ShopVouchers.Add(nvoucher);
            }
            else
            {
                var voucher = de.ShopVouchers.FirstOrDefault(p => p.Id == model.IdVoucher);
                if (voucher == null)
                    return new BasicResponse { Check = false, Ms = "Cant found voucher to edit" };
                voucher.Code = model.Code;
                voucher.Count = model.Count;
                voucher.NameVoucher = model.NameVoucher;
                voucher.Price = model.PriceDiscount;
            }

            de.SaveChanges();
            return new BasicResponse { Check = true, Ms = " Success!" };
        }

        //Statictical about something
        [HttpGet("[action]")]
        public ActionResult StaticticalProductMost()
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });
            // a product was the best selling in this month
            using (var de = new DataEntities())
            {

                var result = from d in de.InvoiceDetails
                             from p in de.Products
                             from i in de.Invoices
                             where d.ProductId == p.Id && i.DateCreate!.Value.Month == DateTime.Now.Month
                             && i.DateCreate.Value.Year == DateTime.Now.Year && i.ShopId == user.Id
                             group d by d.InvoiceId into g
                             select new SellingProduct
                             {
                                 Id = g.FirstOrDefault()!.ProductId!.Value
                                 ,
                                 Quantity = g.Sum(p => p.Quantity)!.Value
                             };


                if (result.Count() > 0)
                {
                    var IdPro = result.OrderByDescending(p => p.Quantity).Select(p => p.Id).FirstOrDefault();

                    var namePro = de.Products.FirstOrDefault(p => p.Id == IdPro)?.Name;
                    return Ok(new { check = true, ms = "This product is the best salling for the month", BestSellingProduct = namePro });

                }
                else
                {
                    return Ok(new { Check = false, ms = "Dont have a best selling products for the month" });
                }
            }

        }
        //Statictical about customer
        //something went wrong
        [HttpGet("[action]")]
        public ActionResult StatictialByCustomer()
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            using (var de = new DataEntities())
            {
                var mm = de.Invoices.FirstOrDefault(p => p.ShopId == user.Id);
                if (mm != null)
                {

                    var Result1 = from i in de.Invoices
                                  from u in de.AspNetUsers
                                  where i.UserId == u.Id
                                  && i.DateCreate!.Value.Month == DateTime.Now.Month
                                  && i.DateCreate.Value.Year == DateTime.Now.Year
                                  && i.ShopId == user.Id
                                  group i by i.UserId into g
                                  select new Customer
                                  {
                                      Id = g.FirstOrDefault()!.UserId,
                                      Total = g.Sum(p => p.TotalUSD ?? 0),
                                  };

                    if (Result1.Count() > 0)
                    {
                        var IdUser = Result1.OrderByDescending(p => p.Total).Select(p => p.Id).FirstOrDefault();
                        // var nameUser = de.Products.FirstOrDefault(p => p.UserId == IdUser);
                        var nameUser = de.AspNetUsers.FirstOrDefault(p => p.Id == IdUser);
                        var nus = new
                        {
                            UserName = nameUser?.UserName,
                            FullName = nameUser?.FullName
                        };

                        return Ok(new { check = true, ms = "This is the customer who buys the most orders in this month :", UserBuyAcc = nus });
                    }
                    else
                        return Ok(new { Check = false, ms = "Dont have any customer bought anything in this month" });
                }
                return Ok(new { check = false, ms = "ERROR!!" });
                // Customers buy the most products in the ladder
            }
        }

        // stactictial shop
        [HttpGet("[action]")]
        public ActionResult Statictial()
        {
            try
            {
                using var de = new DataEntities();
                var user = C_User.Auth(Request.Headers["Authorization"]);
                if (user == null)
                    return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

                var statictials = new Dictionary<string, string>();

                var invoiceshop = de.Invoices.AsNoTracking().Where(p => p.ShopId == user.Id);
                var totalmoney = invoiceshop.Sum(p => p.TotalUSD ?? 0);
                var moneybymonth = invoiceshop.Where(p => p.DateCreate != null && p.DateCreate.Value.Month == DateTime.Now.Month).Sum(p => p.TotalUSD) ?? 0;
                var startDate = DateTime.Now.StartOfWeek(DayOfWeek.Monday);

                var endDate = startDate.AddDays(6);

                var moneybyweek = invoiceshop.Where(p => p.DateCreate != null && p.DateCreate >= startDate && p.DateCreate < endDate).Sum(p => p.TotalUSD) ?? 0;

                statictials.Add("ToTalMoney", totalmoney.ToString());
                statictials.Add("ToTalInMonth", moneybymonth.ToString());
                statictials.Add("ToTalWeek", moneybyweek.ToString());

                return Ok(new { check = true, ms = "Get success!", Data = statictials });
            }
            catch (Exception ex)
            {
                var ms = ex.Message + "\n" + ex.InnerException?.Message + "\n" + ex.InnerException?.InnerException?.Message;
                return Ok(new { Check = false, Ms = ms });
            }
        }
        [HttpGet("[action]")]
        public ActionResult GetTransationSeller(DateTime? to, DateTime? from, string? search, int page = 1, int limit = 20)
        {
            using var de = new DataEntities();

            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });


            var getalltrans = de.Transactions.Where(p => (string.IsNullOrEmpty(search) || p.Code.Contains(search))
            && (to == null || p.DateCreate >= to)
            && (from == null || p.DateCreate <= from)
            && p.Type == Enum_TransactionType.BuyProduct && p.UserIdInccured == user.Id).OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);

            if (getalltrans == null)
                return Ok(new { check = false, ms = "Can't found transaction of seller" });

            var totalmoney = de.Transactions.Where(p => p.Type == Enum_TransactionType.BuyProduct && p.UserIdInccured == user.Id).Sum(p => -p.RealAmount);
            return Ok(new { check = true, ms = "get all transaction success!", data = getalltrans, total = getalltrans.TotalItemCount, money = totalmoney });
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> Logictics([FromForm] LogisticRequest model)
        {
            using var de = new DataEntities();

            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });
            if (string.IsNullOrEmpty(model.LogisticName) || model.StepPrice == null || model.ShipPrice == null || model.MaxWeight == null || model.StepWeight == null)
                return Ok(new { check = false, ms = "Please enter full filled" });
            if (model.StepPrice <= 0 || model.StepWeight <= 0 || model.StepWeight <= 0 || model.MaxWeight <= 0)
                return Ok(new { check = false, ms = "Please enter greater than 0" });
            if (model.Id == null || model.Id == Guid.Empty)
            {
                var p = new ShopLogistic
                {
                    Id = Guid.NewGuid(),
                    DateCreate = DateTime.Now,
                    DateUpdate = DateTime.Now,
                    LogisticName = model.LogisticName,
                    MaxWeight = model.MaxWeight,
                    ShopId = user.Id,
                    ShipPrice = model.ShipPrice,
                    StepPrice = model.StepPrice,
                    StepWeight = model.StepWeight,
                };
                // moi don hang tang len thi tinh them tien cho ship

                de.ShopLogistics.Add(p);
                if (model.Logo != null)
                {
                    var requestImg = await C_Request.UploadImage(model.Logo, 500); // 500 px
                    if (!requestImg.Status)
                        return Ok(new { check = false, ms = requestImg.Message });
                    p.Logo = requestImg.Result.Url;
                }
                de.SaveChanges();
                return Ok(new { check = true, ms = "Create success!" });

            }
            else
            {
                var checklogistic = de.ShopLogistics.FirstOrDefault(P => P.Id == model.Id);
                if (checklogistic == null)
                    return Ok(new { check = false, ms = " Can't found logistic" });
                checklogistic.LogisticName = model.LogisticName;
                checklogistic.MaxWeight = model.MaxWeight;
                checklogistic.StepWeight = model.StepWeight;
                checklogistic.ShipPrice = model.ShipPrice;
                checklogistic.StepPrice = model.StepPrice;
                checklogistic.DateUpdate = DateTime.Now;
                de.SaveChanges();
                return Ok(new { check = true, ms = "Update success!" });
            }


        }
    }
}

