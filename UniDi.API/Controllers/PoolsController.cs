using Core;
using Core.Models.Request;
using Core.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using X.PagedList;

namespace UniDi.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PoolsController : ControllerBase
    {
       
        // Get All List Coin is using
        [HttpGet("[action]")]
        [SwaggerOperation (Description = "<b>type<\b> != null get by active")]
        public ActionResult GetAllListCoin(string? type ,int? transactionType, string? search  , int page = 1, int limit = 20)

        {
            using var de = new DataEntities();

            de.Configuration.ProxyCreationEnabled = false;
            de.Configuration.LazyLoadingEnabled = false;

            var up = de.ListCoins.AsNoTracking().Where(p =>
            (string.IsNullOrEmpty(type) || (p.Status == Enum_ListCoinStatus.Active && p.DateActive <= DateTime.Now))

            && (string.IsNullOrEmpty(search) || p.SymbolLabel.Contains(search))

           && (transactionType == null 
           || (transactionType == Enum_TransactionType.Deposit && p.IsDeposit == true)
           || (transactionType == Enum_TransactionType.Withdraw && p.IsWithdraw == true) 
           || (transactionType == Enum_TransactionType.Transfer && p.IsTransfer == true))

            ).OrderBy(p => p.DateActive)
            .GroupBy(p => p.SymbolLabel).Select(p => p.FirstOrDefault()).ToPagedList(page, limit);
            if (up.Count == 0)
                return Ok(new { check = false, ms = "can't found any listcoin" });
       /*     foreach (var item in up)
            {
                if (!string.IsNullOrEmpty(item.PriceLink) && item.PriceLink.Contains("coinmarketcap"))
                    item.Percent_24h = await C_BlockChain.GetVolume24h(item.SymbolLabel);
                else
                {
                    item.Percent_24h = 0;
                }
            }*/
            //   

            return Ok(new { check = true, ms = "Search active list success!", data = up, up.TotalItemCount });
        }
        [HttpGet("[action]")]
        public ActionResult GetChainByCoin(string symbol)
        {
            using var de = new DataEntities();
            var tss = new List<dynamic>();
            var up = de.ListCoins.AsNoTracking().Where(p => p.SymbolLabel == symbol).Select(p=>p.ChainName).ToList();
            return Ok(new { check = true, ms = "Search active list success!", data = up });
        }
        // CREATE LIST COIN
        [HttpPost("[action]")]
        public ActionResult UpdateListCoin([FromForm] ListCoinRequest model)
        {
            using var de = new DataEntities();

            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, status = 201, ms = "Your login session has expired, please login again!", redirect = "/Login" });


            model.EmailUser = model.EmailUser.Trim();
            if (!Tool.IsValidEmail(model.EmailUser))
                return Ok(new { status = false, ms = "Invalid email" });
            if (!model.EmailUser.Split('@')[1].Contains('.'))
                return Ok(new { status = false, ms = "Invalid email" });


            var item = new ListCoin
            {
                Id = Guid.NewGuid(),
                EmailUser = model.EmailUser,
                UserId = user.Id,
                RelationshipProject = model.RelationshipProject,
                DateLaunch = model.DateLaunch,
                ProjectName = model.ProjectName,
                Symbol = model.Symbol,
                DetailProjectDescription = model.DetailProjectDescription,
                Platform = model.Platform,
                MediaCoverage = model.MediaCoverage,
                Country = model.Country,
                Website1 = model.Website1,
                Website2 = model.Website2,
                PlatformOfContract = model.PlatformOfContract,
                Address = model.Address,
                PhoneNumber = model.PhoneNumber,
                BlockExplorer = model.BlockExplorer,
                Affiliation = model.Affiliation,
                Whitepaper = model.Whitepaper,
                Decimals = model.Decimals,
                Contract = model.Contract,
                ChainName = model.ChainName,
                Twitter = model.Twitter,
                Telegram = model.Telegram,
                Reddit = model.Reddit,
                Facebook = model.Facebook,
                ProjectVideo = model.ProjectVideo,
                Linkedln = model.Linkedln,
                Status = Enum_ListCoinStatus.New,
                DateCreate = DateTime.Now,
                DateUpdate = DateTime.Now,
                PaymentStatus = Enum_PaidStatus.UnPaid,
                IsDeposit = true,
                Logo = model.Logo
            };
                if(model.PriceLink != null)
                {
                    item.PriceLink = model.PriceLink;
                } 
                else
                {
                    item.PriceLink = null;
                    item.Price = 0;
                 }    

            item.SymbolLabel = item.Symbol;


            de.ListCoins.Add(item);

            var exp = (DateTime.Now);

            var claim = new List<Claim> 
            {
              new Claim("id", item.Id.ToString()),
              new Claim("email", item.EmailUser),
            };
            var jwt = Tool.EnJwtToken(claim, exp);

            de.SaveChanges();

            //SEND MAIL

            /* string emailShop = "thuannguyenTHCST4@gmail.com";
             string passWordShop = "zcewtpskkqwebiip";
             MailMessage mailMessage = new MailMessage(emailShop, item.EmailUser);

             mailMessage.Subject = "[UNIDI] MESSAGE";
             mailMessage.Body = $"Please click on the link below to go to deposit! \n\n " +

                "<a href='" + urlFrontEnd + "?verify=" + jwt + "  ' style='color:blue;'>Deposit</a>" +
                "\n + " +
                 "------------------------------------------------\n" +
                 "";
             mailMessage.IsBodyHtml = true;
             using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
             {
                 System.Net.NetworkCredential nc = new NetworkCredential(emailShop, passWordShop);
                 smtp.Credentials = nc;
                 smtp.EnableSsl = true;
                 smtp.Send(mailMessage);
             }*/

            return Ok(new { check = true, ms = "Send success!", data = jwt });
        }
        
        // GET INFO COIN for list coin
        [HttpGet("[action]")]
        public ActionResult GetInfoCoin(string token = "")
        {
            var de = new DataEntities();
            de.Configuration.ProxyCreationEnabled = false;
            de.Configuration.LazyLoadingEnabled = false;
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(token);
            string? tokenInfo = jwtSecurityToken?.Payload?.FirstOrDefault(p => p.Key == "id").Value?.ToString();
            if(tokenInfo == null)
                return Ok(new { check = false, ms = "Failed! wrong token info" });
            var tokenId = Guid.Parse(tokenInfo);
            var listcoin = de.ListCoins.AsNoTracking().FirstOrDefault(p => p.Id == tokenId);

            if (listcoin == null)
                return Ok(new { check = false, ms = "Failed! wrong token listcoin" });

            var exp = (DateTime.Now);
            var claim = new List<Claim> {
                            new Claim("id", listcoin.Id.ToString()),
                            new Claim("type", "payment")
                };
            var jwt = Tool.EnJwtToken(claim, exp);
            return Ok(new { check = true, ms = "Send success!", price = 1560, data = listcoin, tokenPayment = jwt });
        }
        [HttpPost("[action]")]
        public async Task<UsingtResponse> BuyPackageListCoin([FromForm] BuyPackageListCoinRequest model) // BUY A PACKAGE WHEN LIST COIN
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return new UsingtResponse { Check = false, Ms = "Your login session has expired, please login again!" };

            using var de = new DataEntities();
            var package = de.Packages.AsNoTracking().FirstOrDefault(p => p.Id == model.PackageId);
            if (package == null)
                return new UsingtResponse { Check = false, Ms = "Package invalid" };
            var currency = "USDT";
            var coin = de.ListCoins.AsNoTracking().FirstOrDefault(p => p.SymbolLabel == model.Symbol);
            if (coin == null || coin.SymbolLabel != currency)
                return new UsingtResponse { Check = false, Ms = "Symbol invalid" };
            var token = de.ListCoins.FirstOrDefault(p => p.Id == model.ListCoinId);
            if (token == null)
                return new UsingtResponse { Check = false, Ms = "The token does not exist" };

            // Scan kiểm tra nạp tiền
            var wallet = await C_BlockChain.CreateAddress(user.Id, model.Chain, model.Symbol);
            await C_BlockChain.Loop_ScanWallet(wallet); 

            var balance = C_UserBalance.GetBalanceByWallet(de, user.Id, currency);
            if (balance < package.PriceSale)
                return new UsingtResponse { Check = false, Ms = "The system is waiting to receive money" };

            var code = "P" + Tool.GetRandomNumber(8); // BUY PACKAGE FOR THE LISTCOIN
            while (de.Transactions.AsNoTracking().Any(p => p.Code == code))
                code = "P" + Tool.GetRandomNumber(8);
            
            // trừ tiền  
            await C_UserBalance.Add_UserBalance(de, user.Id, user.Id, Enum_TransactionType.BuyPackageListCoin, currency, -package.PriceSale, 0, code, DateTime.Now); // Sau khi nap se tru tien

            // cập nhật gói
            token.PackageId = model.PackageId;
            token.Status = Enum_ListCoinStatus.Active;
            token.DateActive = DateTime.Now.AddDays(7);
            token.PaymentStatus = Enum_PaidStatus.Paid;
            token.DateUpdate = DateTime.Now;
            de.SaveChanges();

            var pricepackage = package.PriceSale;
            
            // gửi mail thông báo thanh toán thành công


            var content = $" <div class=\" font-family: Arial, sans-serif;background-color: #f0f0f0;margin: 0;padding: 0\">\r\n        <div class=\"container\"\r\n            style=\"  max-width: 600px;margin: 0 auto; background-color: #fff;border: 1px solid #ddd;box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);\">\r\n            <div class=\"header\" style=\" background-color: #007bff;text-align: center;\r\n            color: #fff;\r\n            padding: 20px;\">\r\n                <img style=\"max-width: 150px;\" src=\"https://unidi.net/Content/overview/img/Unidi%20Logo1.png\"\r\n                    alt=\"Company Logo\">\r\n            </div>\r\n            <div class=\"content\" style=\"padding: 0 16px; \">\r\n                <div class=\"header\">\r\n                    <h1 style=\"text-align: center;\"> Successful Payment - Thank You for Your Support!</h1>\r\n                </div>\r\n                <div class=\"content\" style=\"margin: 26px 0;\">\r\n                    <p><strong>Dear</strong> "+user.FullName+",</p>\r\n                    <p>We are thrilled to inform you that your recent payment has been successfully processed!</p>\r\n                    <p>\r\n                        <strong>Transaction CODE:</strong> "+code+"\r\n                    </p>\r\n                    <p> <b>Payment Amount:</b> $"+ pricepackage + "</p>\r\n                    <p> <strong>Date:</strong>" + token.DateUpdate + " </p>\r\n                    <p> <b>Coin listing date projection: </b>\r\n                        "+token.DateActive+"\r\n                    </p>\r\n                    <p style=\"padding-bottom: 20px;\">If you need assistance or have any questions, please don't hesitate\r\n                        to\r\n                        contact us at <a href=\"mailto:[Support Email]\">udini@gmail.com</a> or call us at <a\r\n                            href=\"tel:[Support Phone Number]\">0909090</a>. We are always here to assist you.</p>\r\n                </div>\r\n\r\n            </div>\r\n            <div class=\"footer\" style=\"text-align: left;  background-color: ghostwhite;\r\n            padding:10px 16px;\">\r\n                <p>Thank you for choosing <b>UNIDI</b> as your partner. <br> We look forward to providing you with the\r\n                    best experiences.</p>\r\n                <p>Best regards,<br>"+user.FullName+"<br>UNIDI</p>\r\n                <p><a style=\" color: #007bff; cursor: pointer;\r\n                    text-decoration: none;\" href=\"https://unidi.net/\">unidi.net</a></p>\r\n            </div>\r\n        </div>\r\n    </div>";

            Tool.SendMail("[UNIDI] MESSAGE", content, token.EmailUser);
            //Gui Mail

            //
            return new UsingtResponse { Check = true, Ms = "Success", Data = code };
        }
        //     
        // CREATE A POOL
        [HttpPost("[action]")]
        public async Task<ActionResult> UpdatePool([FromForm] CreatePoolRequest model)
        {
            using var de = new DataEntities();

            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            if (model.USDAmount == null || model.USDAmount <= 0)
                return Ok(new { check = false, ms = "Please enter balance Greater than 0" });
            if (string.IsNullOrEmpty(model.Currency))
                return Ok(new { check = false, ms = "Please enter currency to create pool" });
            var coin = de.ListCoins.AsNoTracking().FirstOrDefault(p => p.SymbolLabel == model.Currency && p.Status == Enum_ListCoinStatus.Active && p.DateActive <= DateTime.Now);
            if (coin == null)
                return Ok(new { check = false, ms = "Can not find currency" });
            if (model.IsPriceRealTime == true && (coin.Price == null || coin.Price == 0))
                return Ok(new { check = false, ms = "The system is maintenance" });
            if (model.IsPriceRealTime != true && (model.Price == null || model.Price <= 0))
                return Ok(new { check = false, ms = "Invalid Price" });
            if(model.PercentAcceptUSDPay == null || model.PercentAcceptUSDPay < 0)
                return Ok(new { check = false, ms = "Percent accepting payments in USD is not valid" });
            var balance = C_UserBalance.GetBalanceByWallet(de, user.Id, "USDT"); // check wallet
            if (balance < model.USDAmount)
                return Ok(new { Check = false, Ms = "Insufficient balance" });

            if (model.Id == null)
            {

                var code = "P" + Tool.GetRandomNumber(8); // Create pool
                while (de.Transactions.AsNoTracking().Any(p => p.Code == code))
                    code = "P" + Tool.GetRandomNumber(8);

                var P = new Pool
                {
                    Id = Guid.NewGuid(),
                    Price = model.Price,
                    IsPriceRealTime = model.IsPriceRealTime,
                    DateCreate = DateTime.Now,
                    UserId = user.Id,
                    Currency = model.Currency,
                    CollectedCoin = 0, // coin đã thu thập
                    Code = code,
                    PercentAcceptUSDPay = model.PercentAcceptUSDPay,
                    Status = Enum_PoolStatus.Runing,
                    USDRemaning = model.USDAmount,
                    USDTotal = model.USDAmount
                };
                de.Pools.Add(P);
                await C_UserBalance.Add_UserBalance(de, user.Id, user.Id, Enum_TransactionType.CreatePool, P.Currency, -model.USDAmount, 0, code, DateTime.Now);
                de.SaveChanges();
                return Ok(new { check = true, ms = "Create Pool Success!", idpool = P.Id });
            }
            else 
            {
                var poolc = de.Pools.FirstOrDefault(p => p.Id == model.Id);
                if(poolc == null)
                    return Ok(new { check = false, ms = "Access denied!" });

                // cộng thêm tiền vào pool
                poolc.USDTotal += model.USDAmount; 
                poolc.USDRemaning += model.USDAmount;

                await C_UserBalance.Add_UserBalance(de, user.Id, user.Id, Enum_TransactionType.CreatePool, poolc.Currency, -model.USDAmount, 0, poolc.Code, DateTime.Now);        
                de.SaveChanges();
                return Ok(new { check = true, ms = "Update Pools Success!" });
            }
        }
        //THÊM PRODUCT VÀO POOL
        [HttpPost("[action]")]
        public ActionResult AddProductInPool([FromForm] AddPoolProductRequest model)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            using var de = new DataEntities();

            var poolpro = de.Pools.FirstOrDefault(p => p.Id == model.PoolId);
            if (poolpro == null)
                return Ok(new { check = false, ms = "Please create a pool first" });
            if (poolpro.UserId != user.Id)
                return Ok(new { check = false, ms = "YOU DONT HAVE PERMISSION FOR THIS POOL" });
            if(model.AcceptAllCountry != true && (model.ListCountryId == null || model.ListCountryId.Count == 0))
                return Ok(new { check = false, ms = "Please select a country" });
            if (model.AcceptAllCategory != true && (model.ListCategoryId == null || model.ListCategoryId.Count == 0))
                return Ok(new { check = false, ms = "Please select a category" });
            if (model.AcceptAllShop != true && (model.ListShopId == null || model.ListShopId.Count == 0))
                return Ok(new { check = false, ms = "Please select a shop" });
            if (model.AcceptAllProduct != true && (model.ListProductId == null || model.ListProductId.Count == 0))
                return Ok(new { check = false, ms = "Please select a product" });

            if(model.AcceptAllCountry != true)
            {
                foreach (var item in model.ListCountryId)
                {
                    de.PoolAcceptCountries.Add(new PoolAcceptCountry { 
                        Id = Tool.NewGuid(poolpro.Id + "" + item),
                        CountryId = item,
                        PoolId = poolpro.Id
                    });
                }
            }
            if (model.AcceptAllCategory != true)
            {
                foreach (var item in model.ListCategoryId)
                {
                    var cateId = Guid.Parse(item);
                    de.PoolAcceptCategories.Add(new PoolAcceptCategory
                    {                        
                        Id = Tool.NewGuid(poolpro.Id + "" + item),
                        CategoryId = cateId,
                        PoolId = poolpro.Id,
                        CateNode = de.Categories.AsNoTracking().FirstOrDefault(c => c.Id == cateId)?.CateNode,
                    });
                }
            }
            if (model.AcceptAllShop != true)
            {
                foreach (var item in model.ListShopId)
                {
                    de.PoolAcceptShops.Add(new PoolAcceptShop 
                    { 
                        Id = Tool.NewGuid(poolpro.Id + "" + item),
                        ShopId = item,
                        PoolId = poolpro.Id
                    });
                }
            }
            if (model.AcceptAllProduct != true)
            {
                foreach (var item in model.ListProductId)
                {
                    var productId = Guid.Parse(item);
                    de.PoolAcceptProducts.Add(new PoolAcceptProduct
                    {
                        Id = Tool.NewGuid(poolpro.Id + "" + item),
                        ProductId = productId,
                        PoolId = poolpro.Id,
                        DateCreate = DateTime.Now,
                        Currency = poolpro.Currency
                    });
                }
            }
            poolpro.AcceptAllCountry = model.AcceptAllCountry;
            poolpro.AcceptAllCategory = model.AcceptAllCategory;
            poolpro.AcceptAllShop = model.AcceptAllShop;
            poolpro.AcceptAllProduct = model.AcceptAllProduct;
            poolpro.TotalProduct = de.PoolAcceptProducts.AsNoTracking().Count(p => p.PoolId == poolpro.Id);

            de.SaveChanges();
            return Ok(new { check = true, ms = "Add Success!" });
        }
        [HttpPost("[action]")]
        [SwaggerOperation(Description = "type = country | category | shop | product")]
        public ActionResult DeletePoolItem([FromForm] DeletePoolItemRequest model)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            using var de = new DataEntities();

            var poolpro = de.Pools.AsNoTracking().FirstOrDefault(p => p.Id == model.PoolId);
            if (poolpro == null)
                return Ok(new { check = false, ms = "Please create a pool first" });
            if (poolpro.UserId != user.Id)
                return Ok(new { check = false, ms = "YOU DONT HAVE PERMISSION FOR THIS POOL" });
            if(model.Type == "country")
            {
                var item = de.PoolAcceptCountries.FirstOrDefault(p => p.PoolId == poolpro.Id && p.CountryId == model.IntId);
                if (item != null)
                    de.PoolAcceptCountries.Remove(item);
            } 
            else if (model.Type == "category")
            {
                var cateId = Guid.Parse(model.StringId);
                var item = de.PoolAcceptCategories.FirstOrDefault(p => p.PoolId == poolpro.Id && p.CategoryId == cateId);
                if (item != null)
                    de.PoolAcceptCategories.Remove(item);
            }
            else if (model.Type == "shop")
            {
                var item = de.PoolAcceptShops.FirstOrDefault(p => p.PoolId == poolpro.Id && p.ShopId == model.StringId);
                if (item != null)
                    de.PoolAcceptShops.Remove(item);
            }
            else if (model.Type == "product")
            {
                var productId = Guid.Parse(model.StringId);
                var item = de.PoolAcceptProducts.FirstOrDefault(p => p.PoolId == poolpro.Id && p.ProductId == productId);
                if (item != null)
                    de.PoolAcceptProducts.Remove(item);
            }

            poolpro.TotalProduct = de.PoolAcceptProducts.AsNoTracking().Count(p => p.PoolId == poolpro.Id);
            de.SaveChanges();
            return Ok(new { check = true, ms = "Delete success" });
        }
        /*   [HttpPost("[action]")]
           public ActionResult AddProductInPool([FromForm] )*/
        /*  //Get All POOL
          [HttpGet("[action]")]
          public ActionResult GetAllPool(int page =1, int limit = 20)
          {
              using (var de = new DataEntities())
              {
                  de.Configuration.ProxyCreationEnabled = false;
                  de.Configuration.LazyLoadingEnabled = false;

                  var getallpool = de.Pools.AsNoTracking().OrderByDescending(p => p.DateUpdate).ToPagedList(page, limit);

                  return Ok(new {check = true, ms = "Get all pool success!", data = getallpool, total = getallpool.TotalItemCount});
              }
          }*/
        // get all pool by userid
        [HttpGet("[action]")]
        [SwaggerOperation (Description = "type == null => get all, type !=null get by userLogin " )]
        public ActionResult GetAllPool(string? type = "", int page = 1, int limit = 20)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!" });

            using var de = new DataEntities();
            de.Configuration.ProxyCreationEnabled = false;
            de.Configuration.LazyLoadingEnabled = false;

            // GET POOL BY ACCOUNT
            var getpoolbyuserid = de.Pools.AsNoTracking().Where(p => string.IsNullOrEmpty(type) || p.UserId == user.Id).OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);
            foreach (var im in getpoolbyuserid)
            {
                var infocoin = de.ListCoins.AsNoTracking().FirstOrDefault(p => p.SymbolLabel == im.Currency);
                im.ImgCoin = infocoin?.Logo;
            }
            return Ok(new { check = true, ms = "Get all pool by user success!", data = getpoolbyuserid, total = getpoolbyuserid.TotalItemCount });
        }
        // Check pool detail 

    /*    [HttpGet("[action]")]
        public ActionResult GetPoolById(Guid? IdPool)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!" });

            using var de = new DataEntities();
            de.Configuration.ProxyCreationEnabled = false;
            de.Configuration.LazyLoadingEnabled = false;

            var getpool = de.Pools.AsNoTracking().FirstOrDefault(p => p.Id == IdPool);
            if (getpool == null)
                return Ok(new { check = false, ms = "cant found pool" });

            var coin = de.ListCoins.AsNoTracking().FirstOrDefault(p => p.Id == getpool.CoinId);

            var checkpropool = de.AddPoolProducts.AsNoTracking().Where(p => p.PoolId == getpool.Id);
            *//*       if(checkpropool != null)
                   {

                       foreach(var check in checkpropool)
                       {
                               if (check.IdType == "1")
                               {
                                   getpool.ProductOfPool = de.Products.AsNoTracking().OrderByDescending(p => p.DateCreate).ToList();
                                   getpool.totalproduct = getpool.ProductOfPool.Count;
                               }

                                if (check.IdType == "2")
                               {
                                   var listitem = de.ListItemPools.AsNoTracking().FirstOrDefault(p => p.Id == check.Id && p.IdPool == check.IdPool);
                                   var idprods = Guid.Parse(listitem.IdItem);
                                   getpool.ProductOfPool = de.Products.AsNoTracking().Where(p => p.Id == idprods).ToList();
                                   getpool.totalproduct = getpool.ProductOfPool.Count;
                               }
                               if (check.IdType == "3")
                               {
                                   var listitem = de.ListItemPools.AsNoTracking().FirstOrDefault(p => p.Id == check.Id && p.IdPool == check.IdPool);
                                   var idprods = Guid.Parse(listitem.IdItem);
                                   getpool.ProductOfPool = de.Products.AsNoTracking().Where(p => p.CategoryId == idprods).ToList();
                                   getpool.totalproduct = getpool.ProductOfPool.Count;
                               }
                               if (check.IdType == "4")
                               {
                                   var listitem = de.ListItemPools.AsNoTracking().FirstOrDefault(p => p.Id == check.Id && p.IdPool == check.IdPool);
                                   var idprods = Guid.Parse(listitem.IdItem);
                                   getpool.ProductOfPool = de.Products.AsNoTracking().Where(p => p.CategoryId == idprods).ToList();
                                   getpool.totalproduct = getpool.ProductOfPool.Count;

                               }


                       }

                   }
                   else
                   {
                       return Ok(new { check = false, ms = "cant found pool from checkpropool" });
                   }
   *//*
            getpool.NameCoin = coin.Symbol;
            getpool.ImgCoin = coin.Logo;
            return Ok(new { check = true, ms = "get pool success!", data = getpool });
        }*/
       
        //[HttpGet("[action]")]
        //public ActionResult GetInfoWallet(string? Type = "", int page = 1, int limit = 20)
        //{

        //    var user = C_User.Auth(Request.Headers["Authorization"]);
        //    if (user == null)
        //        return Ok(new { check = false, ms = "Your login session has expired, please login again!" });

        //    using (var de = new DataEntities())
        //    {
        //        de.Configuration.ProxyCreationEnabled = false;
        //        de.Configuration.LazyLoadingEnabled = false;

        //        if (Type != "")
        //        {
        //            var infouser = de.Wallets.AsNoTracking().Where(p => p.UserId == user.Id && p.Role == "User").ToList();
        //            foreach (var inm in infouser)
        //            {
        //                inm.Imgcoin = de.ListCoins.FirstOrDefault(p => p.Id == inm.IdListCoin).Logo;
        //            }
        //            return Ok(new { check = true, ms = "get info success", data = infouser, total = infouser.Count });
        //        }
        //        else
        //        {

        //            var allinfo = de.Wallets.AsNoTracking().OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);
        //            foreach (var inm in allinfo)
        //            {
        //                inm.Imgcoin = de.ListCoins.FirstOrDefault(p => p.Id == inm.IdListCoin).Logo;
        //            }
        //            return Ok(new { check = true, ms = "get all info success", data = allinfo, total = allinfo.TotalItemCount });
        //        }

        //    }
        //}

       
        // GET ALL CHAIN
        
        [HttpGet("[action]")]
        public ActionResult GetAllChain(string? typechain = "")
        {
            var de = new DataEntities();
            de.Configuration.ProxyCreationEnabled = false;
            de.Configuration.LazyLoadingEnabled = false;
            if(typechain != "")
            {
               var lstchain = de.Chains.AsNoTracking().Where(p => p.Active == true).ToList();
                return Ok(new { check = true, ms = "get all active chain success!", data = lstchain });
            }
            else
            {
               var lstchain =  de.Chains.AsNoTracking().ToList();
                return Ok(new { check = true, ms = "get all chain success!", data = lstchain });
            }
        }
        [HttpGet("[action]")]
        public UsingtResponse GetCoinByChain(string? chainname,int page =1, int limit = 20)
        {
            var de = new DataEntities();
            var chaine = de.Chains.AsNoTracking().FirstOrDefault(p => p.ChainName == chainname);
            if (chaine == null)
                return new UsingtResponse { Check = false, Ms = "dont found chain, Failed!" };
            var liscoin = de.ListCoins.AsNoTracking().Where(p => p.ChainName == chaine.ChainName && p.Status == 2 && p.DateActive < DateTime.Now).OrderBy(p=>p.DateActive).ToPagedList(page,limit);
           var List = new List<dynamic>();   
            foreach(var item in liscoin)
            {
                 var heh = new
                {
                    Id = item.Id,
                    Name = item.ProjectName,
                    Symbol = item.SymbolLabel,
                    Price = item.Price,
                    Logo = item.Logo,
                    Decimail = item.Decimals,
                };
                List.Add(heh);
            }
            return new UsingtResponse { Check = true, Ms = "Get Coin by chain success!", Data = List , Total = liscoin.TotalItemCount };
        }
        [HttpGet("[action]")]
        public async Task<UsingtResponse> GetInfoWallet(string? chainname,string? symbol, string? token = "")
        {
            string? tk = (string?)Request.Headers["Authorization"] ?? token;
            var user = C_User.Auth(tk);
            if (user == null)
                return  new UsingtResponse { Check = false, Ms = "Your login session has expired, please login again!" };


            using var de = new DataEntities();
            var chaine = de.WalletAddresses.AsNoTracking().FirstOrDefault(p =>p.UserId == user.Id && p.ChainName == chainname && p.SymbolLabel == symbol);
            if(chaine == null)
            {
                var a = await C_BlockChain.CreateAddress(user.Id, chainname, symbol);
                if (a != null) return await GetInfoWallet(chainname, symbol, tk);
                else
                    return new UsingtResponse { Check = false, Ms = "Error" };
            }
            return new UsingtResponse { Check = true, Ms = "Get info wallet success!", Data = chaine};
        }
        public static bool CheckAccept(string role, string id)
        {
            using (var de = new DataEntities())
            {
                var getAllRole = de.AspNetUserRoles.Where(p => p.UserId == id).ToList();
                var check = false;
                foreach (var rolen in getAllRole)
                {
                    var name = de.AspNetRoles.FirstOrDefault(p => p.Id == rolen.RoleId)?.Name;
                    if (name == "Pool")
                    {
                        check = true;
                    }

                }
                if (check == false)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

        }
        
        // auto update about a time OF LIST COIN
        [NonAction]
        private void UpdateStatusCallback(object target)
        {
            var de = new DataEntities();
           
            var newTarget = (ListCoin)target;
            var targetToUpdate = de.ListCoins.FirstOrDefault(t => t.Id == newTarget.Id);
            if (targetToUpdate != null)
            {
                targetToUpdate.Status = Enum_ListCoinStatus.Active; // UPDATE STATUS Active
                de.SaveChanges(); //
            }
        }
        private string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }

}
