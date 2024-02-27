using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using Core;
using Core.Models.Request;
using static OpenCvSharp.Stitcher;
using Newtonsoft.Json;
using System.Reflection.Metadata.Ecma335;
using System.Collections.Generic;
using static OpenCvSharp.FileStorage;
using Microsoft.AspNetCore.Mvc.RazorPages;
using X.PagedList;
using System.Net.Http.Headers;
using static Core.Models.Request.JsonParse;
using Microsoft.AspNetCore.Identity;
using MailKit;
using System;
using Core.Models.Response;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using Swashbuckle.AspNetCore.Annotations;
using OfficeOpenXml.ConditionalFormatting;
using Org.BouncyCastle.Crypto.Parameters;

namespace UniDi.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CartController : ControllerBase
    {
        //       private static readonly HttpClient client = new HttpClient();

        //       //Like a ORDER FOR THE USERR
        [HttpPost("[action]")]
        [SwaggerOperation(Description = "")]
        public async Task<ActionResult> CreateNewOrder([FromForm] OrderListShop model)
        {
            try
            {
                var user = C_User.Auth(Request.Headers["Authorization"]);
                if (user == null)
                    return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

               
                string currency = "USDT";
                if (!string.IsNullOrEmpty(model.Currency))
                    currency = model.Currency;

                if (model.ShopId != null && model.CodeVoucher != null && model.ShopId.Count != model.CodeVoucher.Count)
                    return Ok(new { check = false, ms = "Error parameter" });

                using var de = new DataEntities();
                var listinv = de.CartUsers.Where(p => p.UserId == user.Id && p.Checked == true && p.Currency == currency).ToList();
                if (listinv == null || listinv.Count == 0)
                {
                    return Ok(new { check = false, ms = "Can't found item in Cart" });
                }
                // CREATE A INVOICE
                if ( model.LogisticId == null )
                    return Ok(new { check = false, ms = "Please choose Logistic" });

                var logicticsID = model.LogisticId;

                if (model.ShopId == null)
                    return Ok(new { check = false, ms = "Can't found shop" });
                if (model.LogisticId.Count != model.ShopId.Count)
                    return Ok(new { check = false, ms = "Invalid Parameter" });             
                var indexShop = 0;

                foreach (var item in model.ShopId) // DANH SÁCH SHOP
                {
                    var shop = de.AspNetUsers.AsNoTracking().FirstOrDefault(p => p.Id == item);

                    if (shop == null || !listinv.Any(p => p.ShopId == item))
                        return Ok(new { check = false, ms = "Can't found shop" });

                    var address = de.UserAddresses.AsNoTracking().FirstOrDefault(p => p.Id == model.AddressId);
                    if (address == null)
                        return Ok(new { check = false, ms = "Invalid address" });

                    // SHOPPP LOGICTICS
                    for (var i = 0; i < model.ShopId.Count; i++)
                    {
                        Guid itemlogic = logicticsID[i];
                        // lỗi ở đâyyyy
                        var logistic = de.ShopLogistics.AsNoTracking().FirstOrDefault(p => p.Id == itemlogic && p.ShopId == item);
                        // lỗi ở đâyyyy
                        if (logistic == null)
                            return Ok(new { check = false, ms = "Invalid Shipping company" });



                        var codesku = shop.UserName.ToUpper() + Tool.GetRandomNumber(8);
                        while (de.Invoices.AsNoTracking().Any(p => p.ShopId == item && p.Code == codesku))
                            codesku = shop.UserName.ToUpper() + Tool.GetRandomNumber(8);

                        var ddh = new Invoice   // create a invoice by id shop
                        {
                            Id = Guid.NewGuid(),
                            UserId = user.Id,
                            Note = model.Note,  // ghi lại thông tin cho shop
                            DeliveryStatus = Enum_DeliveryStatus.New,
                            DateCreate = DateTime.Now,
                            DateUpdate = DateTime.Now,
                            PaidStatus = Enum_PaidStatus.Paid,
                            Currency = currency,
                            City = address.City,
                            Code = codesku,
                            Country = address.Country,

                            FullAddress = address.FullAddress,
                            Phone = address.Phone,
                            Province = address.Province,
                            Receiver = address.Receiver,
                            ShopId = item,
                            Street = address.Street,
                            ZipCode = address.ZipCode
                        };

                        // list cart user // check list cart user 
                        var carts = listinv.Where(p => p.ShopId == item && p.Currency == currency);
                        var invoiceDetail = new List<InvoiceDetail>();



                        var minPoolPrice = 0.0;
                        foreach (var cart in carts)
                        {
                            var proinfo = de.Products.FirstOrDefault(p => p.Id == cart.ProductId && p.UserId == cart.ShopId);
                            if (proinfo?.QuantityAvailable == null || proinfo.QuantityAvailable <= 0)
                                return Ok(new { check = false, ms = "Product: " + proinfo?.Name + " is out of stock" });
                            if (proinfo == null)
                                return Ok(new { check = false, ms = "Can't found product" });
                            cart.ProductInfo = proinfo;



                            // get price pool //
                            if (currency != "USDT")
                            {
                                // nếu có currency != USDT thì lấy giá của nó
                                var poolinfo = C_Pool.GetAllPoolAcceptProduct(de, proinfo).FirstOrDefault(p => p.Currency == cart.Currency);
                                if (poolinfo == null)
                                    return Ok(new { check = false, ms = "Don't found price pool ! please buy in another currency" });

                                cart.Pool = poolinfo;
                            }
                            else
                            {
                                cart.Pool = new Pool
                                {
                                    Price = 1
                                };
                            }

                            if (minPoolPrice == 0 || minPoolPrice < cart.Pool.Price)
                                minPoolPrice = cart.Pool.Price ?? 1;

                            if (cart.PropertyId != null)
                            {
                                var property = de.ProductProperties.FirstOrDefault(p => p.ProductId == cart.ProductId && p.Id == cart.PropertyId);
                                if (property == null)
                                    return Ok(new { check = false, ms = "Error From Property Products!" });

                                if (property.Quantity < cart.Quantity)
                                    return Ok(new { check = false, ms = "The product is out of stock" });

                                property.Quantity -= cart.Quantity; // trừ sl trong property
                                cart.ProductInfo.SalePrice = property?.Price;
                            }

                            var subTotalUSD = cart.ProductInfo.SalePrice * cart.Quantity;
                            var subToken = subTotalUSD / cart.Pool.Price;
                            var weight = (proinfo.Weight ?? 0) * cart.Quantity;

                            //

                            var ctdh = new InvoiceDetail
                            {
                                Id = Guid.NewGuid(),
                                InvoiceId = ddh.Id,
                                ProductId = proinfo?.Id,
                                Quantity = cart.Quantity,
                                SubTotal = subToken,
                                SubTotalUSD = subTotalUSD,
                                SKU = proinfo?.SKU,
                                Currency = cart.Currency,
                                PoolId = cart.Pool?.Id,
                                TotalUSD = subTotalUSD,
                                Total = subToken,
                                Weight = weight,
                            };

                            //

                            invoiceDetail.Add(ctdh);
                            if (proinfo != null)
                            {
                                proinfo.QuantityAvailable -= ctdh.Quantity;
                            }
                        }

                        ddh.SubTotal = invoiceDetail.Sum(p => p.SubTotal) ?? 0; // sub cho đơn hàng : coin
                        ddh.SubTotalUSD = invoiceDetail.Sum(p => p.SubTotalUSD) ?? 0;  // sub cho đơn hàng : USD

                        // Ship
                        ddh.TotalWeight = invoiceDetail.Sum(p => p.Weight) ?? 0;
                        var feeShip = logistic.ShipPrice ?? 0;
                        if (ddh.TotalWeight > logistic.MaxWeight && logistic.StepWeight > 0 && logistic.StepPrice > 0)
                        {
                            var a = ddh.TotalWeight - logistic.MaxWeight;
                            var b = Math.Round((double)(a / logistic.StepWeight));
                            var c = b * logistic.StepPrice ?? 0;
                            feeShip += c;
                        }
                        ddh.FeeShip = feeShip;




                        // Voucher
                        double? rateDiscount = 0.0;
                        if (model.CodeVoucher != null && indexShop < model.CodeVoucher.Count)
                        {
                            string Codevoucher = model.CodeVoucher[indexShop];
                            if (!string.IsNullOrEmpty(Codevoucher))
                            {
                                var cvh = de.ShopVouchers.FirstOrDefault(p => p.Code == Codevoucher && p.ShopId == item);  // lấy ra voucher
                                if (!string.IsNullOrEmpty(Codevoucher) && cvh == null)
                                    return Ok(new { check = false, ms = "Wrong code voucher" });
                                if (cvh != null && cvh.Count <= 0)
                                    return Ok(new { check = false, ms = "Discount code has expired" });

                                if (cvh != null && cvh.Price != null && cvh.Price > 0 && ddh.SubTotal != null)
                                {
                                    ddh.DiscountUSD = cvh.Price; // cho giá usd của discount vào 
                                    cvh.Count -= 1;

                                    rateDiscount = ddh.DiscountUSD / ddh.SubTotalUSD;
                                }
                            }
                        }

                        if (rateDiscount > 0)
                        {
                            foreach (var product in invoiceDetail)
                            {
                                var discountUSD = rateDiscount * product.SubTotalUSD;
                                product.DiscountUSD = discountUSD;

                                var pool = de.Pools.FirstOrDefault(p => p.Id == product.PoolId);
                                if (pool != null || product.PoolId != Guid.Empty)
                                {
                                    if (pool?.IsPriceRealTime == true)
                                    {
                                        pool.Price = de.ListCoins.AsNoTracking().FirstOrDefault(p => p.SymbolLabel == currency)?.Price;
                                        if (pool.Price == null)
                                            pool.Status = Enum_PoolStatus.Cancel;
                                    }
                                    product.Discount = product.DiscountUSD / pool?.Price;
                                }



                                if (product.Discount > product.SubTotal) // nếu giảm giá lớn hơn giá thì đơn giá sẽ bằng 0
                                {
                                    product.SubTotal = 0;
                                    product.SubTotalUSD = 0;
                                    product.Total = 0;
                                    product.TotalUSD = 0;
                                }
                                else
                                {
                                    product.Total = product.SubTotal - (product.Discount ?? 0);
                                    product.TotalUSD = product.SubTotalUSD - (product.DiscountUSD ?? 0);
                                }
                            }
                        }
                        de.InvoiceDetails.AddRange(invoiceDetail);

                        ddh.Discount = invoiceDetail.Sum(p => p.Discount) ?? 0;
                        ddh.DiscountUSD = invoiceDetail.Sum(p => p.DiscountUSD) ?? 0;


                        if (ddh.Discount > ddh.SubTotal) // nếu giảm giá lớn hơn giá thì đơn giá sẽ bằng 0
                        {
                            ddh.SubTotal = 0;
                            ddh.SubTotalUSD = 0;
                            ddh.Total = 0;
                            ddh.TotalUSD = 0;
                        }
                        else
                        {
                            ddh.Total = ddh.SubTotal - (ddh.Discount ?? 0);
                            ddh.TotalUSD = ddh.SubTotalUSD - (ddh.DiscountUSD ?? 0);
                        }

                        ddh.TotalUSD += feeShip;
                        ddh.Total += (feeShip / minPoolPrice);

                        de.Invoices.Add(ddh);

                        var code = "IV" + Tool.GetRandomNumber(8); // Buy product
                        while (de.Transactions.AsNoTracking().Any(p => p.Code == code))
                            code = "IV" + Tool.GetRandomNumber(8);

                        var balance = C_UserBalance.GetBalanceByWallet(de, user.Id, currency);
                        if (currency != "USDT")
                        {
                            if (balance < ddh.Total)
                                return Ok(new { check = false, ms = "Insufficient balance" });

                            // add transaction from user login to user recieve
                            await C_UserBalance.Add_UserBalance(de, user.Id, user.Id, Enum_TransactionType.BuyProduct, currency, -ddh.Total, 0, code, DateTime.Now);
                        }
                        else
                        {
                            if (balance < ddh.TotalUSD)
                                return Ok(new { check = false, ms = "Insufficient balance" });
                            // add transaction from user login to user recieve
                            await C_UserBalance.Add_UserBalance(de, user.Id, user.Id, Enum_TransactionType.BuyProduct, currency, -ddh.TotalUSD, 0, code, DateTime.Now);
                        }

                        indexShop++;
                    }
                }
                de.CartUsers.RemoveRange(listinv);
                // sau khi đặt hàng thành công sẽ xóa sp ra khỏi giỏ hàng


                de.SaveChanges();
                return Ok(new { check = true, ms = "New Order has been created, You have placed your order successfully" });
            }
            catch (Exception ex)
            {
                var ms = ex.Message + "\n" + ex.InnerException?.Message + "\n" + ex.InnerException?.InnerException?.Message;
                return Ok(new { Check = false, Ms = ms });
            }

        }

        [HttpPost("[action]")]
        [SwaggerOperation(Description = "If type != null get quanity by the number you enter|| += quantity")]
        public ActionResult UpdateCart([FromForm] CartRequest model)
        {
            using var de = new DataEntities();
            //check
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });
            double? totaltpriceewithproperty = 0.0; // Tính lại tổng tiền khi thêm sp, + sl sản phẩm
            double? totaltpriceeNoProperty = 0.0; // Tính lại tổng tiền khi thêm sp, + sl sản phẩm

            double? priceCoin = 1;
            double? totalpricecoin = 0.0;
            model.Quantity ??= 1;
            if (string.IsNullOrEmpty(model.TokenName))
                model.TokenName = "USDT";

            var product = de.Products.AsNoTracking().FirstOrDefault(p => p.Id == model.ProductId);
            if (product == null)
                return Ok(new { check = false, ms = "Can't found product" });
           
            if (model.TokenName != "USDT")
            {
                // nếu có currency != USDT thì lấy giá của nó
                var poolinfo = C_Pool.GetAllPoolAcceptProduct(de, product).FirstOrDefault(p => p.Currency == model.TokenName);
                if (poolinfo == null)
                    return Ok(new { check = false, ms = "Don't found pool ! please add cart in another currency" });
                priceCoin = poolinfo.Price;
            }

            var productprop = de.ProductProperties.AsNoTracking().FirstOrDefault(p => p.ProductId == product.Id); // trường không nhập thuộc tính cho sản phẩm
            
            if (productprop != null && (model.PropertyId == null || model.ProductId == Guid.Empty) )
                return Ok(new { check = false, ms = "Please select product attributes" });

            if (model.PropertyId != null)
            {
                var productproperty = de.ProductProperties.AsNoTracking().FirstOrDefault(p => p.Id == model.PropertyId && p.ProductId == product.Id); // trường hợp sản phẩm có thuộc tính
                if(productproperty == null)
                    return Ok(new { check = false, ms = "Product doesn't have property" });


                if (product.QuantityAvailable < model.Quantity || productproperty?.Quantity < model.Quantity)
                    return Ok(new { check = false, ms = "Not enough quantity" });
            }
            //check

            var check = de.CartUsers.FirstOrDefault(p =>                     // Check CArt

            (model.IdCart == null || model.IdCart == p.Id)
            && (model.PropertyId == null || p.PropertyId == model.PropertyId)
            && (p.ProductId == model.ProductId && p.UserId == user.Id)
            && (p.Currency == model.TokenName)
            );

            if (model.IdCart != null && check == null)
                return Ok(new { check = false, ms = "Can't found This Cart" });

            if ( (check != null && model.IdCart != null)
                
            ||  (de.CartUsers.AsNoTracking().Any(p => p.UserId == user.Id && p.ProductId == model.ProductId 
            && ((p.PropertyId == null && model.PropertyId == null)|| p.PropertyId == model.PropertyId)
            
            ))

            && model.TokenName == check?.Currency)  // trường hợp đã tồn tại sản phẩm trong giỏ hàng
            {
                if (!string.IsNullOrEmpty(model.Type)) // nếu có type cho tự nhập sl
                {
                    check.Quantity = model.Quantity;
                }
                else
                {
                    check.Quantity += model.Quantity;
                }
            }
            else // chưa tồn tại sp trong giỏ hàng
            {
                var cUser = new CartUser
                {
                    UserId = user.Id,
                    Quantity = model.Quantity,
                    ProductId = model.ProductId,
                    Checked = false,
                    Currency = model.TokenName,
                    ShopId = product.UserId,
                };

                if (model.PropertyId != null)
                    cUser.PropertyId = model.PropertyId;

                de.CartUsers.Add(cUser);
            }
            de.SaveChanges();

            var cartitem = de.CartUsers.AsNoTracking().Where(p => p.UserId == user.Id && model.TokenName == p.Currency).ToList();
          
            foreach (var item in cartitem)
            {
                if (item.PropertyId != null)  // trường hợp sản phẩm có thuộc tính
                {
                    var priceproper = de.ProductProperties.FirstOrDefault(p => p.Id == item.PropertyId && p.ProductId == item.ProductId)?.Price;
                    if (priceproper != null)
                        totaltpriceewithproperty += item.Quantity * priceproper; // giá $
                }
                else // trường hợp sản phẩm không có thuộc tính
                {
                    var price = de.Products.FirstOrDefault(p => p.Id == item.ProductId)?.SalePrice;
                    if (price != null)
                        totaltpriceeNoProperty += item.Quantity * price; // giá $
                }
            }

            var totalUSD = totaltpriceewithproperty + totaltpriceeNoProperty;
            totalpricecoin = totalUSD / priceCoin;
            return Ok(new { check = true, ms = "Update success! ", TotalpriceUSD = totalUSD, ToTalpriceCoin = totalpricecoin, ToTalItems = cartitem.Count });
        }
        //       //ToTal CArt
        [HttpGet("[action]")]
        public ActionResult ToTalCart(string? NameToken = "USDT")
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });
            var de = new DataEntities();
            double? pricepool = 1;
            var countItem = 0;
            double? totaltpricee = 0.0;

            var count = de.CartUsers.AsNoTracking().Where(p => p.UserId == user.Id && p.Currency == NameToken).ToList();

            foreach (var item in count)
            {
                var price = de.Products.FirstOrDefault(p => p.Id == item.ProductId);

                var poolinfo = C_Pool.GetAllPoolAcceptProduct(de, price).FirstOrDefault(p => p.Currency == item.Currency);
                if (poolinfo != null && item.Currency != "USDT")
                    pricepool = poolinfo.Price;      

                var propertyproduct = de.ProductProperties.AsNoTracking().FirstOrDefault(p => p.Id == item.PropertyId);
                if (propertyproduct != null)
                {
                    totaltpricee += propertyproduct.Price * item.Quantity;
                    countItem++;
                }
                else
                {
                    totaltpricee += (price?.SalePrice ?? price?.OldPrice) * item.Quantity;
                    countItem++;
                }
            }


            var totalcoin = totaltpricee / pricepool; 

            return Ok(new { check = true, ms = "get total success!", totalItems = countItem, totalprice = totaltpricee, totalcoin = totalcoin });
        }

        [HttpPost("[action]")]
        public ActionResult DeleteMultiItemCart(List<int> CartId)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            using var de = new DataEntities();
            foreach (var ide in CartId)
            {

                var cartpro = de.CartUsers.FirstOrDefault(s => s.Id == ide && s.UserId == user.Id);
                if (cartpro != null)
                    de.CartUsers.Remove(cartpro);

            }
            de.SaveChanges();
            return Ok(new { check = true, ms = "Delete multi CartUser complete! " });
        }

        ////    view cart of the user
        [HttpGet("[action]")]
        [SwaggerOperation (Description = "if typecheck != null get all cart by check = true")]
        public ActionResult GetCartByUserId(string? typecheck, string? typetoken = "USDT")
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            using var de = new DataEntities();
            de.Configuration.ProxyCreationEnabled = false;
            de.Configuration.LazyLoadingEnabled = false;
            var namecoin = de.ListCoins.AsNoTracking().FirstOrDefault(p => p.SymbolLabel == typetoken);
            if (string.IsNullOrEmpty(namecoin?.SymbolLabel))
                return Ok(new { check = false, ms = " Can't found token" });
            /* CartList cartList = new CartList();*/
            double? totalUSD = 0.0;
            double? ToTalCoin = 0.0;
            var liste = de.CartUsers.AsNoTracking().Where(p => p.UserId == user.Id
            && p.Currency == typetoken
            && (string.IsNullOrEmpty(typecheck) || p.Checked == true)).ToList();

            foreach (var item in liste)
            {
                var product = de.Products.FirstOrDefault(p => p.Id == item.ProductId);

                if (product != null)
                {
                    var img = de.ProductImages.AsNoTracking().FirstOrDefault(p => p.ProductId == item.ProductId);
                    if (img != null)
                        product.Image = img.Link;
                }
                else
                {
                    return Ok(new { check = false, ms = "Don't found product" });
                }
                if (typetoken != "USDT")
                    item.SingleInfoPool = C_Pool.GetAllPoolAcceptProduct(de, product).FirstOrDefault(p => p.Currency == item.Currency);

                var bus = de.BusinessLicenses.AsNoTracking().FirstOrDefault(p => p.ShopId == product.UserId);

                if (bus == null)
                    return Ok(new { check = false, ms = "Don't found info shop" });

                item.ShopInfo = de.BusinessLicenses.AsNoTracking().FirstOrDefault(p => p.Id == bus.Id);   //Name SHOP
                item.ProductInfo = product;

                item.ProductInfo.ProductProperty = de.ProductProperties.AsNoTracking().Where(p => p.ProductId == item.ProductId && p.Id == item.PropertyId).ToList();

                if (item.ProductInfo.ProductProperty?.Count > 0)
                {
                    foreach (var im in item.ProductInfo.ProductProperty)
                    {
                        totalUSD += item.Quantity * im.Price;
                    }
                }
                else
                {
                    totalUSD += item.Quantity * product.SalePrice;
                }


                if (item.SingleInfoPool != null)
                {
                    ToTalCoin += totalUSD / item.SingleInfoPool.Price;
                }

            }
            var totalprice = new Dictionary<string, double?>
            {
                {"TotalUSD", totalUSD}
                , {"ToTalCoin", ToTalCoin}
            };

            return Ok(new { check = true, ms = "list here", data = liste, total = liste.Count, ToTalPrice = totalprice });
        }

        // TICK THE PRODUCT YOU WANT TO BUY
        [HttpGet("[action]")]
        [SwaggerOperation (Description = " (if type != null  && Idcart != null ==> check = true) || check = false \n\n || typeall != null : check all")]
        public ActionResult TickCart(int? IdCart, string? typeall, string? type, string? tokenname = "USDT")
        {

            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });
            if (tokenname == null)
                return Ok(new {check  = false, ms = " FE Please enter token name"});
            using var de = new DataEntities();
            var cprod = de.CartUsers.FirstOrDefault(p => p.Id == IdCart && p.UserId == user.Id && p.Currency == tokenname);
            if (IdCart != null)
            {
                if (cprod != null)
                {
                    var pro = de.Products.FirstOrDefault(p => p.Id == cprod.ProductId);
                    if (pro == null || pro.QuantityAvailable < 1)
                    {
                        return Ok(new { check = false, ms = "The product does not exist or out of stock !" });
                    }

                    if (!string.IsNullOrEmpty(type))  //check
                        cprod.Checked = true;
                    else // uncheck
                    {
                        cprod.Checked = false;
                        de.SaveChanges();
                        return Ok(new { check = true, ms = "Untick success!" });
                    }
                    de.SaveChanges();
                    return Ok(new { check = true, ms = "Choose  products successful!" });
                }
                else
                {
                    return Ok(new { check = false, ms = "Cant found your cart! " });
                }

            }
            else
            {
                if (!string.IsNullOrEmpty(typeall)) // có type all => check all
                {
                    var ctpr = de.CartUsers.Where(p => p.UserId == user.Id && p.Currency == tokenname).ToList();
                    var checktrue = ctpr.All(p => p.Checked == true);
                    foreach (var imt in ctpr)
                    {
                        if (checktrue)
                            imt.Checked = false;
                        else
                        {
                            if (imt.Checked != true)
                                imt.Checked = true;
                        }

                    }
                    de.SaveChanges();
                    return Ok(new { check = true, ms = "CHECK OR UNCHECK ALL SUCCESS!!" });
                }
                else
                {
                    return Ok(new { check = false, ms = "Missing parameter" });
                }


            }
            /*if (shopId != null)
            {
                foreach(var ipr in shopId)
                {
                    var pros = de.Products.FirstOrDefault(p => p.UserId == ipr);
                    var idpp = pros.Id.ToString();
                    var csh = de.CartUsers.Where(p => p.ProductId == idpp);
                    foreach (var csitem in csh)
                    {
                        if (type != "")
                        { 
                            csitem.CheckTick = true;
                        }
                        else
                        {
                            csitem.CheckTick = false;   
                        }
                    }
                }    
            }*/
        }
        
    }
}
