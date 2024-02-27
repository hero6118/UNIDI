using Core;
using Core.Models.Request;
using Core.Models.Response;
using Google.Authenticator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using X.PagedList;

namespace UniDi.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        [HttpGet("[action]")] // using for deposit
        public async Task<DefaultResponse> GetWalletAddress(string chain, string symbol)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return new DefaultResponse { Status = false, Message = "Your login session has expired, please login again!" };

            var wallet = await C_BlockChain.CreateAddress(user.Id, chain, symbol);
            if (wallet == null)
                return new DefaultResponse { Status = false, Message = "Symbol invalid" };
            return new DefaultResponse { Status = true, Message = "Add UserWallet success!", Result = new {
                wallet.Address,
                wallet.ChainName,
                wallet.SymbolLabel,
                wallet.Symbol
            }};
        }

        [HttpPost("AddTransaction")]
        public async Task<DefaultResponse> AddTransaction([FromForm] AddTransactionRequest model)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return new DefaultResponse { Status = false, Message = "Your login session has expired, please login again!" };

            if (model.Amount <= 0)
                return new DefaultResponse { Status = false, Message = "Amount invalid" };
            if (model.TransactionType == Enum_TransactionType.Deposit)     // || 
                return new DefaultResponse { Status = false, Message = "Access denied" };

            using var de = new DataEntities();
            var balance = C_UserBalance.GetBalanceByWallet(de, user.Id, model.Currency);
            if (balance < model.Amount)
                return new DefaultResponse { Status = false, Message = "Insufficient balance" };

            var code = Enum_TransactionType.Label[model.TransactionType][..1] + Tool.GetRandomNumber(8);
            while (de.Transactions.AsNoTracking().Any(p => p.Code == code))
                code = Enum_TransactionType.Label[model.TransactionType][..1] + Tool.GetRandomNumber(8);

            await C_UserBalance.Add_UserBalance(de, user.Id, user.Id, model.TransactionType, model.Currency.ToUpper(), -model.Amount, 0, code, DateTime.Now);  //  đang trừ
            de.SaveChanges();

            return new DefaultResponse { Status = true, Message = "Success", Result = code };
        }

        [HttpPut("AddTransaction")]
        public async Task<DefaultResponse> AddTransaction2([FromBody] AddTransactionRequest model)
        {
            return await AddTransaction(model);
        }

        [HttpGet("[action]")]
        public DefaultResponse GetAlllPackage()
        {
            var de = new DataEntities();
            var allpackage = de.Packages.AsNoTracking().ToList();
            return new DefaultResponse { Status = true, Message = "Get all package success!", Result = allpackage };
        }

        // using in transaction managerment
        [HttpGet("[action]")]
        public ListBalanceResponse ListBalanceToken()
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return new ListBalanceResponse { Check = false, Ms = "Your login session has expired, please login again!" };
            using var de = new DataEntities();
            de.Configuration.LazyLoadingEnabled = false;
            de.Configuration.ProxyCreationEnabled = false;
            var listToken = de.ListCoins.AsNoTracking().Where(p => p.DateActive <= DateTime.Now && p.Status == Enum_ListCoinStatus.Active).GroupBy(p => p.SymbolLabel).Select(p=>p.FirstOrDefault()).ToList();
            foreach (var item in listToken)
            {
                if (item == null) continue;
                item.TempBalance = C_UserBalance.GetBalanceByWallet(de, user.Id, item.SymbolLabel);
                item.TempEstimate = item.TempBalance * (item.Price ?? 0);
            }
            var estimate = listToken.Sum(p => p?.TempEstimate) ?? 0;
            return new ListBalanceResponse { Check = true, Ms = "Get all list token success!", Data = listToken , EstimateUSD = estimate };
        }
        [HttpGet("[action]")]
        [SwaggerOperation(Description = "if all = true select all, if all = false select by user login")]
        public TransactionResponse TransactionHistory(DateTime? from, DateTime? to, string? currency, string? searchcode, int? typetransaction, int page = 1, int limit = 20, bool? all = false)
        {
            using var de = new DataEntities();
            de.Configuration.ProxyCreationEnabled = false;
            de.Configuration.LazyLoadingEnabled = false;
            if (to != null)
                to = to.Value.Add(new TimeSpan(23,59,59));

            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return new TransactionResponse { Check = false, Ms = "Your login session has expired, please login again!" };
            if (currency != null)
            {
                var coin = de.ListCoins.AsNoTracking().FirstOrDefault(p => p.SymbolLabel == currency && p.Status == Enum_ListCoinStatus.Active);
                if (coin == null)
                {
                    return new TransactionResponse { Check = false, Ms = "Cant found currency!" };
                }
            }

            var trans = de.Transactions.AsNoTracking().Where(p =>
            (all == true || p.UserId == user.Id)
            && (string.IsNullOrEmpty(currency) || p.Currency == currency)
            && (from == null || p.DateCreate >= from)
            && (to == null || p.DateCreate <= to)
            && (typetransaction == null || p.Type == typetransaction) 
            && (string.IsNullOrEmpty(searchcode) || p.Code.Contains(searchcode)))
            .OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);

            foreach (var item in trans)
            {
                item.TransDetail = de.TransactionDetails.AsNoTracking().SingleOrDefault(p => p.TransactionId == item.Id) ?? null;
                item.UserInccured = de.AspNetUsers.AsNoTracking().FirstOrDefault(p => p.Id == item.UserIdInccured) ?? new AspNetUser();
               
            }
            var res = new TransactionResponse { Check = true, Ms = "Get transaction success!", Data = trans.ToList(), Total = trans.TotalItemCount };
            return res ;
        }

        [HttpGet("[action]")]
        public BalanceResponse GetBalanceByCurrency(string? currency)
        {
            using var de = new DataEntities();

            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return new BalanceResponse { Check = false, Ms = "Your login session has expired, please login again!" };
            if(string.IsNullOrEmpty(currency))
                return new BalanceResponse { Check = false, Ms = "Please enter currency!" };
            var balance = C_UserBalance.GetBalanceByWallet(user.Id, currency);
            return new BalanceResponse { Check = true, Ms = "Get success!",Data = balance };
        }

        [HttpGet("[action]")]
        public ActionResult GetDetailPackage(int? id)
        {
            var de = new DataEntities();
            var detail = de.Packages.AsNoTracking().FirstOrDefault(p => p.Id == id);

            var im = new
            {
                Id = detail?.Id,
                Name = detail?.Name,
                Price = detail?.Price,
                PriceSale = detail?.PriceSale,
            };


            if (detail == null)
            {
                return Ok(new { check = false, ms = "failed" });
            }

            return Ok(new { check = true, ms = "success", data = im });
        }
        [HttpPost("[action]")]
        public async Task<WithDrawResponse> Withdraw([FromForm] WithdrawRequest model)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return new WithDrawResponse { Check = false, Ms = "Your login session has expired, please login again!" };

            using var de = new DataEntities();
            // CHECK FIELD
            if (string.IsNullOrEmpty(model.Currency))
                return new WithDrawResponse { Check = false, Ms = "Please choose a currency" };
            if(model.Amount <= 0 || model.Amount == null)
                return new WithDrawResponse { Check = false, Ms = "Invalid amount" };
            var ubl = de.UserWallets.AsNoTracking().FirstOrDefault(p => p.UserId == user.Id && p.Currency == model.Currency);
            if (ubl == null)
                return new WithDrawResponse { Check = false, Ms = "You aren't have a UserWallet!" };
            var icoin = de.ListCoins.AsNoTracking().FirstOrDefault(p => p.SymbolLabel == ubl.Currency && p.ChainName == model.Chain && p.Status == Enum_ListCoinStatus.Active);
            if(icoin == null || icoin.IsWithdraw != true)
                return new WithDrawResponse { Check = false, Ms = "Access denied" };
            //var price = icoin.Price;
            //if (price == null) price = 0.0;
            if (string.IsNullOrEmpty(model.WalletAddress))
                return new WithDrawResponse { Check = false, Ms = "Please enter a wallet address!" };
            // CHECK FIELD
          
                if (user.IsGoogleAuthenticator == true)
                {
                    if (string.IsNullOrEmpty(model.Code2FA))
                        return new WithDrawResponse { Check = false, Ms = "Enter 2FA code", Require2FA = true };

                    TwoFactorAuthenticator tfa = new();
                    bool isValid = tfa.ValidateTwoFactorPIN(user.GoogleAuthenticatorSecretKey, model.Code2FA);
                    if (!isValid)
                        return new WithDrawResponse { Check = false, Ms = "2FA code is incorrect" };
                }

            //CHECK ADDRESS WALLET
            var validAddress = await C_BlockChain.CheckValidWalletAddress(model.Chain, model.WalletAddress);
            if (validAddress.Check == false)
                return new WithDrawResponse { Check = false, Ms = "Invalid wallet address" };
            //CHECK ADDRESS WALLET

            //CHECK BALANCE WALLET
            var balance = C_UserBalance.GetBalanceByWallet(de, user.Id, model.Currency);
            if (balance < model.Amount)
                return new WithDrawResponse { Check = false, Ms = "Insufficient balance" };
            //CHECK BALANCE WALLET

            var feePercent = de.Configs.AsNoTracking().FirstOrDefault()?.FeeWithdrawPercent ?? 1;
            double? fee = model.Amount * feePercent / 100;
            var code = "W" + Tool.GetRandomNumber(8); // WITHDRAW
            while (de.Transactions.AsNoTracking().Any(p => p.Code == code))
                code = "W" + Tool.GetRandomNumber(8);

            // addtransaction but dont - balance when Status = false;  
            var item = await C_UserBalance.Add_UserBalance(de, user.Id, user.Id, Enum_TransactionType.Withdraw, icoin.SymbolLabel, -model.Amount, (double)-fee, code, DateTime.Now, true,false);
            var price = (icoin.Price ?? 0);
            var tdetail = new TransactionDetail
            {
                TransactionId = item.Id,
                ChainName = icoin.ChainName,
                Price = price,
                EstimateUSD = model.Amount * price ,
                Symbol = icoin.SymbolLabel,
                WalletAddress = model.WalletAddress
            };
            de.TransactionDetails.Add(tdetail);

            de.SaveChanges();

            // Gửi mail rút tiền thành công
            var content = "<div class=\" font-family: Arial, sans-serif;background-color: #f0f0f0;margin: 0;padding: 0\">\r\n        <div class=\"container\"\r\n            style=\"  max-width: 100%;margin: 0 auto; background-color: #fff;border: 1px solid #ddd;box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);\">\r\n            <div class=\"header\" style=\" background-color: #007bff;text-align: center;\r\n            color: #fff;\r\n            padding: 20px;\">\r\n                <img style=\"max-width: 150px;\" src=\"https://unidi.net/Content/overview/img/Unidi%20Logo1.png\"\r\n                    alt=\"Company Logo\">\r\n            </div>\r\n            <div class=\"content\" style=\"padding: 0 16px; \">\r\n                <div class=\"header\">\r\n                    <h1 style=\"text-align: center;\"> Email \r\nWithdraw money successfully</h1>\r\n                </div>\r\n                <div class=\"content\" style=\"margin: 26px 0;\">\r\n                    <p><strong>Dear</strong> "+user.UserName+",</p>\r\n                    <p>\r\nThis is an email notifying you that you have successfully withdrawn money</p>\r\n<p>\r\nYou have withdrawn <b>"+model.Amount+"</b> to your wallet</p>\r\n                    \r\n                    <p style=\"padding-bottom: 20px;\">If you didn’t request this, you can ignore this email. If you need assistance or have any questions, please don't hesitate\r\n                        to\r\n                        contact us at <a href=\"mailto:[Support Email]\">udini@gmail.com</a> or call us at <a\r\n                            href=\"tel:[Support Phone Number]\">0909090</a>. We are always here to assist you.</p>\r\n                </div>\r\n\r\n            </div>\r\n            <div class=\"footer\" style=\"text-align: left;  background-color: ghostwhite;\r\n            padding:10px 16px;\">\r\n                <p>Thank you for choosing <b>UNIDI</b>  <br> We look forward to providing you with the\r\n                    best experiences.</p>\r\n                <p>Best regards,<br>"+model.Amount+"<br>UNIDI</p>\r\n                <p><a style=\" color: #007bff; cursor: pointer;\r\n                    text-decoration: none;\" href=\"https://unidi.net/\">unidi.net</a></p>\r\n            </div>\r\n        </div>\r\n    </div>";
                
            Tool.SendMail("UNIDI [MESSAGE]", content, user.Email);
           

            return new WithDrawResponse { Check = true, Ms = "Withdrawal successful!,Please wait for the administrator to confirm this transaction" };
        }
        [HttpPost("[action]")]
        [SwaggerOperation(Description = "This just for transfer")]
        public async Task<TransferResponse> Transfer([FromForm] TransferRequest model)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return new TransferResponse { Check = false, Ms = "Your login session has expired, please login again!" };

            using var de = new DataEntities();

            //CHECK  entry
            if(string.IsNullOrEmpty(model.WalletCurrency)) // transfer dont need chain
                return new TransferResponse { Check = false, Ms = "Choose your Wallet!" };
            if (string.IsNullOrEmpty(model.RecipientUsername))
                return new TransferResponse { Check = false, Ms = "Please enter recipient's username !" };

            var Reciever = de.AspNetUsers.AsNoTracking().FirstOrDefault(p => p.UserName == model.RecipientUsername);
            
            if (Reciever == null)
                return new TransferResponse { Check = false, Ms = " Recipient's username does not exist!" };

            if (Reciever.UserName == user.UserName)
                return new TransferResponse { Check = false, Ms = "Can't transfer to yourself!" };
            
            if (model.Amount <= 0)
                return new TransferResponse { Check = false, Ms = "Please Enter amount must be greater than 0" };


            var balance = C_UserBalance.GetBalanceByWallet(de, user.Id, model.WalletCurrency);
            if (balance < model.Amount)
                return new TransferResponse { Check = false, Ms = "Not enough money to transfer" };
            
            var coin = de.ListCoins.AsNoTracking().FirstOrDefault(p => p.SymbolLabel == model.WalletCurrency);
            
            if (coin == null || coin.IsTransfer !=true )
                return new TransferResponse { Check = false, Ms = "Access Denied!" };

            if (user.IsGoogleAuthenticator == true)
            {
                if (string.IsNullOrEmpty(model.Code2FA))
                    return new TransferResponse { Check = false, Ms = "Enter 2FA code", Require2FA = true };

                TwoFactorAuthenticator tfa = new();
                bool isValid = tfa.ValidateTwoFactorPIN(user.GoogleAuthenticatorSecretKey, model.Code2FA);
                if (!isValid)
                    return new TransferResponse { Check = false, Ms = "2FA code is incorrect" };
            }


            var code = "T" + Tool.GetRandomNumber(8); // TRansfer
            while (de.Transactions.AsNoTracking().Any(p => p.Code == code))
                code = "T" + Tool.GetRandomNumber(8);

            // add transaction from user login to user recieve
            await C_UserBalance.Add_UserBalance(de,user.Id, Reciever.Id, Enum_TransactionType.Transfer, model.WalletCurrency, -model.Amount, 0, code, DateTime.Now); // Sau khi nap se tru tien

            //add transaction from a reciever who recieve from a user login
            await C_UserBalance.Add_UserBalance(de, Reciever.Id, user.Id, Enum_TransactionType.Transfer, model.WalletCurrency, model.Amount, 0, code, DateTime.Now); //

            de.SaveChanges();   

            return new TransferResponse { Check = true, Ms = "Transfer Successfully!" };

        }

    }
}
