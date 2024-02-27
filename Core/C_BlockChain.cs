using Core.Models.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static Core.Models.Request.InfoCoinRequest;

namespace Core
{
    public class C_BlockChain
    {
        public static string TokenKey = "B27277D0-B3BA-4DE0-BF82-6F2D5BA473BE";
        public static string CoinmarketcapAPIKey = "d2d5659b-3f41-4c6f-9a33-f5b5f037fb24";
        public static async Task<WalletAddress> CreateAddress(string userId, string chain, string symbol)
        {
            using (var de = new DataEntities())
            {
                var coin = de.ListCoins.AsNoTracking().FirstOrDefault(p => p.ChainName == chain && p.SymbolLabel == symbol); // chưa có coin thì null
                if (coin == null) return null;

                var userWalet = de.WalletAddresses.AsNoTracking().FirstOrDefault(p => p.UserId == userId && p.ChainName == chain && p.Symbol == coin.Symbol);
                if (userWalet != null) return userWalet; // có ví thì show ra

                try
                {
                    var a = de.WalletAddresses.AsNoTracking().FirstOrDefault(p => p.UserId == userId && p.ChainName == chain);
                    if (a != null)
                    {
                        userWalet = new WalletAddress
                        {
                            Id = Tool.NewGuid(userId + symbol + chain),
                            Address = a.Address,
                            Balance = 0,
                            ChainName = chain,
                            DateCreate = DateTime.Now,
                            Decimals = coin.Decimals,
                            PrivateKey = a.PrivateKey,
                            SmartContract = coin.Contract,
                            Symbol = coin.Symbol,
                            UserId = userId,
                            SymbolLabel = coin.SymbolLabel
                        };
                    }
                    else
                    {
                        var header = new Dictionary<string, string>
                        {
                            { "Authorization", TokenKey }
                        };
                        var request = await C_Request.GetDataHttpClient("https://bsc.uto.vn/Account/Create?Chain=" + chain, header);
                        if (string.IsNullOrEmpty(request)) return null;

                        var wallet = JsonConvert.DeserializeObject<dynamic>(request);
                        if (wallet.status != true) return null;

                        userWalet = new WalletAddress
                        {
                            Id = Tool.NewGuid(userId + symbol + chain),
                            Address = wallet.result.address.ToString(),
                            Balance = 0,
                            ChainName = chain,
                            DateCreate = DateTime.Now,
                            Decimals = coin.Decimals,
                            PrivateKey = Tool.EncryptString(wallet.result.privateKey.ToString()),
                            SmartContract = coin.Contract,
                            Symbol = coin.Symbol,
                            UserId = userId,
                            SymbolLabel = coin.SymbolLabel
                        };
                    }
                    de.WalletAddresses.Add(userWalet);
                    de.SaveChanges();
                }
                catch
                {
                    return null;
                }
                return userWalet;
            }
        }
        public static async Task<List<BlockChainTransaction>> GetTransaction(string chainName, string walletAddress, string contractAddress, string symbol)
        {
            try
            {
                var header = new Dictionary<string, string>
                    {
                        { "Authorization", TokenKey }
                    };
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("Chain", chainName),
                    new KeyValuePair<string, string>("Address", walletAddress),
                    new KeyValuePair<string, string>("Limit", "2"),
                    new KeyValuePair<string, string>("Category", "0"),
                    new KeyValuePair<string, string>("ContractAddress", contractAddress),
                    new KeyValuePair<string, string>("Symbol", symbol)
                });
                var request = await C_Request.PostDataHttpClient("https://bsc.uto.vn/Transaction/History", content, header);
                if (string.IsNullOrEmpty(request)) return new List<BlockChainTransaction>();

                var history = JsonConvert.DeserializeObject<BlockChainTransactionResponse>(request);
                if (history.Status != true) return new List<BlockChainTransaction>();

                return history.Result ?? new List<BlockChainTransaction>();
            }
            catch
            {
                return new List<BlockChainTransaction>();
            }
        }
        public static async Task<UsingtResponse> CheckValidWalletAddress(string chain, string address)
        {
            try
            {
                var header = new Dictionary<string, string>
                {
                { "Authorization", TokenKey }};
                var request = await C_Request.GetDataHttpClient("https://bsc.uto.vn/Account/ValidAddress?Address=" + address + "&Chain=" + chain, header);
                var json = JsonConvert.DeserializeObject<dynamic>(request);
                return new UsingtResponse { Check = bool.Parse(json.status.ToString()), Ms = json.message.ToString() };
            }
            catch (Exception ex)
            {
                var ms = ex.Message + "\n" + ex.InnerException?.Message + "\n" + ex.InnerException?.InnerException?.Message;
                return new UsingtResponse { Check = false, Ms = ms };
            }
        }
        public static void ScanWallet(string userId = null)
        {
            using (var de = new DataEntities())
            {
                var list = de.WalletAddresses.Where(p => userId == null || p.UserId == userId);
                foreach (var item in list)
                {
                    Task.Run(() => Loop_ScanWallet(item));
                }
            }
        }
        public static async Task Loop_ScanWallet(WalletAddress wallet) // FOR DEPOSIT
        {
            using (var de = new DataEntities())
            {
                var history = await GetTransaction(wallet.ChainName, wallet.Address, wallet.SmartContract, wallet.Symbol);
                foreach (var item in history)
                {
                    if (!de.Transactions.AsNoTracking().Any(p => p.Txid == item.Hash))
                    {
                        var code = "D" + Tool.GetRandomNumber(8);
                        while (de.Transactions.AsNoTracking().Any(p => p.Code == code))
                            code = "D" + Tool.GetRandomNumber(8);

                        var u = de.AspNetUsers.SingleOrDefault(p => p.Id == wallet.UserId) ?? null;

                        // Cộng tiền cho thành viên         || DEPOSIT
                        await C_UserBalance.Add_UserBalance(de, wallet.UserId, wallet.UserId, Enum_TransactionType.Deposit, wallet.SymbolLabel, item.Amount, 0, code, DateTime.Now, true, true, null, item.Hash);
                        // Gửi mail nạp tiền thành công


                        var content = "<div class=\" font-family: Arial, sans-serif;background-color: #f0f0f0;margin: 0;padding: 0\">\r\n        <div class=\"container\"\r\n            style=\"  max-width: 100%;margin: 0 auto; background-color: #fff;border: 1px solid #ddd;box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);\">\r\n            <div class=\"header\" style=\" background-color: #007bff;text-align: center;\r\n            color: #fff;\r\n            padding: 20px;\">\r\n                <img style=\"max-width: 150px;\" src=\"https://unidi.net/Content/overview/img/Unidi%20Logo1.png\"\r\n                    alt=\"Company Logo\">\r\n            </div>\r\n            <div class=\"content\" style=\"padding: 0 16px; \">\r\n                <div class=\"header\">\r\n                    <h1 style=\"text-align: center;\"> Email Deposit successfully</h1>\r\n                </div>\r\n                <div class=\"content\" style=\"margin: 26px 0;\">\r\n                    <p><strong>Dear</strong> " + u.FullName + ",</p>\r\n                    <p>This is an email notifying you that you have successfully deposited money</p>\r\n                    <p><b>" + item.Amount + " " + wallet.SymbolLabel + "</b> will be deposited into your wallet</p>\r\n                    <br>\r\n                    <p style=\"padding-bottom: 20px;\">If you didn’t request this, you can ignore this email. If you need assistance or have any questions, please don't hesitate to contact us at <a href=\"mailto:[Support Email]\">udini@gmail.com</a> or call us at <a\r\n                            href=\"tel:[Support Phone Number]\">0909090</a>. We are always here to assist you.</p>\r\n                </div>\r\n\r\n            </div>\r\n            <div class=\"footer\" style=\"text-align: left;  background-color: ghostwhite;\r\n            padding:10px 16px;\">\r\n                <p>Thank you for choosing <b>UNIDI</b> as your partner. <br> We look forward to providing you with the\r\n                    best experiences.</p>\r\n                <p>Best regards,<br>" + u.FullName + "<br>UNIDI</p>\r\n                <p><a style=\" color: #007bff; cursor: pointer;\r\n                    text-decoration: none;\" href=\"https://unidi.net/\">unidi.net</a></p>\r\n            </div>\r\n        </div>\r\n    </div>";
                        Tool.SendMail("UNIDI [MESSAGE]", content, u.Email);

                        // Cộng tiền cho admin rút về ví tổng
                        var a = de.WalletAddresses.FirstOrDefault(p => p.ChainName == wallet.ChainName && p.Symbol == wallet.Symbol);
                        if (a != null)
                        {
                            a.Balance = (a.Balance ?? 0) + item.Amount;
                            de.SaveChanges();
                        }
                    }
                }
            }
        }
        public static async Task<CoinmarketcapResponse> CoinMarketCapGetPriceToken(string symbol)
        {
            var header = new Dictionary<string, string>
            {
                { "X-CMC_PRO_API_KEY", CoinmarketcapAPIKey },
                { "Accepts", "application/json" }
            };
            var request = await C_Request.GetDataHttpClient("https://pro-api.coinmarketcap.com/v2/tools/price-conversion?amount=1&convert=USD&symbol=" + symbol, header);
            var json = JsonConvert.DeserializeObject<CoinmarketcapResponse>(request);
            return json;
        }
        public static async Task<double?> GetVolume24h(string symbol)
        {
            var header = new Dictionary<string, string>
            {
                { "X-CMC_PRO_API_KEY", CoinmarketcapAPIKey },
                { "Accepts", "application/json" }
            };
            var request = await C_Request.GetDataHttpClient("https://pro-api.coinmarketcap.com/v1/cryptocurrency/quotes/latest?symbol=" + symbol + "&convert=USD", header);
            var json = JsonConvert.DeserializeObject<CryptoApiResponse>(request);


            if (json.Data.ContainsKey(symbol))
            {
                var cryptoData = json.Data[symbol];

                double? percent = cryptoData.Quote.USD.Percent_change_24h;
                return percent;
            }
            else
                return 0.0;
        }

        /* public static async Task<Decimal> GetCryptoPriceAsync(string symbol)
         {

                             var header = new Dictionary<string, string>
                                 {
                                     { "X-CMC_PRO_API_KEY", CoinmarketcapAPIKey },
                                     { "Accepts", "application/json" }
                                 };
                             var request = await C_Request.GetDataHttpClient("https://pro-api.coinmarketcap.com/v1/cryptocurrency/quotes/latest?symbol="+symbol+"&convert=USD", header);


                            var json = JsonConvert.DeserializeObject<CryptoApiResponse>(request);

                             decimal perercent  = json.Data[symbol].Quote.USD.percent_change_24h;
                             return json;

                 // Trả về 0 nếu không thành công hoặc không tìm thấy dữ liệu

         }*/

        public static async Task UpdatePriceToken()
        {
            using (var de = new DataEntities())
            {
                var list = de.ListCoins.Where(p => !string.IsNullOrEmpty(p.PriceLink));
                foreach (var item in list)
                {
                    if (item.PriceLink.Contains("coinmarketcap"))
                    {
                        var request = await CoinMarketCapGetPriceToken(item.SymbolLabel);
                        if (request.Data.Count > 0)
                        {
                            item.Price = request.Data[0].Quote.USD.Price;
                            item.DateUpdatePrice = DateTime.Now;
                        }
                    }
                    else
                    {

                    }
                }
                de.SaveChanges();
            }
        }



    }
    public class BlockChainTransactionResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public List<BlockChainTransaction> Result { get; set; }
    }
    public class BlockChainTransaction
    {
        public string Hash { get; set; }
        public double Amount { get; set; }
        public string TokenSymbol { get; set; }
    }
}
