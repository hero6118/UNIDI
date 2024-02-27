using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class C_UserBalance
    {
        public static async Task<Transaction> Add_UserBalance(DataEntities de, string userId, string userIdInccured, int transactionType, string currency,
            double? amount, double fee, string code, DateTime? datecreate, bool status = true, bool confirmPayment = true, int? commissionType = null, string tx = null, string note = "", bool firewall = true)
        {
            if (amount == 0 && transactionType != Enum_TransactionType.Deposit) return null;

            var date = datecreate ?? Tool.Curtime();
            var newDate = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
            try
            {
                using (MD5 md5 = MD5.Create())
                {
                    string input = "";
                    Guid id = Guid.Empty;
                    if (firewall == true)
                    {
                        if (transactionType == Enum_TransactionType.Withdraw
                        || transactionType == Enum_TransactionType.Transfer)
                        {
                            newDate = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 1);// 1);
                            input = newDate.Ticks.ToString() + userId.ToString() + currency;
                        }
                        else
                        {
                            input = newDate.Ticks.ToString() + userId.ToString() + userIdInccured.ToString() + transactionType.ToString() + currency + commissionType.ToString() + amount.ToString() + status + code;
                        }
                        byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(input));
                        id = new Guid(hash);
                    }
                    else
                    {
                        id = Guid.NewGuid();
                    }

                    var item = new Transaction
                    {
                        Id = id,
                        UserId = userId,
                        UserIdInccured = userIdInccured,
                        Type = transactionType,
                        Currency = currency,
                        CommissionType = commissionType,
                        Amount = amount,
                        RealAmount = amount - fee,
                        Fee = fee,
                        Code = code,
                        DateCreate = newDate,
                        Status = status,
                        ConfirmPayment = confirmPayment,
                        Txid = tx,
                        Note = note
                    };
                    de.Transactions.Add(item);
                    if (status == true)
                    {
                         AddBalance(de, item, currency, (double)amount);
                    }
                   
                    return item;
                }
            }
            catch// (Exception ex)
            {
                //_ = Task.Run(() => Tool.SendTelegram("Error add user balance\nType: " + Enum_TransactionType.Label[transactionType] + "\n" + ex.Message, "error"));
                return null;
            }
        }
            //async Task
        public static void AddBalance (DataEntities de, Transaction ut, string currency, double amount)  // THÊm số dư || THAY DDOORI 
        {
            var userBalance = de.UserWallets.FirstOrDefault(p => p.UserId == ut.UserId && p.Currency == currency);
            if (userBalance != null)
            {
                userBalance.Balance = (userBalance.Balance ?? 0) + amount;

                if (userBalance.Balance < 0)
                {
                    //await Tool.SendTelegram("[UTO] Error Balance\nType: " + Enum_TransactionType.Label[ut.TransactionType ?? 0] + "\nBalance: " + userBalance.Balance + " " + currency + "\nCode: " + ut.Code, "error");
                }
            }
            else
            {
                // Kiểm tra tồn tại trong bảng tạm (Db Transaction)
                var ub = de.Get<UserWallet>().FirstOrDefault(p => p.UserId == ut.UserId && p.Currency == currency);
                if (ub == null)
                {
                    var ubId = Tool.NewGuid(ut.UserId + currency);
                    de.UserWallets.Add(new UserWallet
                    {
                        Id = ubId,
                        UserId = ut.UserId,
                        Balance = amount,
                        Currency = currency
                    });
                }
                else
                {
                    ub.Balance += amount;
                }
            }
        }
        public static double GetBalanceByWallet(DataEntities de, string userId, string currency)
        {
            var wallet = de.UserWallets.FirstOrDefault(p => p.UserId == userId && p.Currency == currency);
            if (wallet != null)
            {
                var balance = Math.Round(wallet.Balance ?? 0, 3);
                if (balance < 0) balance = 0;
                return balance;
            }
            return 0.0;
        }
        public static double GetBalanceByWallet(string userId, string currency)
        {
            using (var de = new DataEntities())
            {
                return GetBalanceByWallet(de, userId, currency);
            }
        }
        public static void UpdateWallet(string userId) // Đảm bảo số dư của ví sẽ chính xác
        {
            using (var de = new DataEntities())
            {
                var listCoin = de.ListCoins;
                foreach (var item in listCoin)
                {
                    var balance = de.Transactions.Where(p => p.UserId == userId && p.Currency == item.SymbolLabel && p.Status == true).Sum(p => p.Amount) ?? 0;
                    var wallets = de.UserWallets.FirstOrDefault(p => p.UserId == userId && p.Currency == item.SymbolLabel);
                    if (wallets != null)
                        wallets.Balance = balance;
                    else if(balance != 0)
                    {
                        de.UserWallets.Add(new UserWallet
                        {
                            Id = Tool.NewGuid(userId + item.SymbolLabel),
                            Balance = balance,
                            UserId = userId,
                            Currency = item.SymbolLabel
                        });
                    }
                }
                de.SaveChanges();
            }
        }
        public static TransactionDetail GetDetailTransactionById(Guid id)
        {
            using (var de = new DataEntities())
            {
                return de.TransactionDetails.FirstOrDefault(p => p.TransactionId == id) ?? new TransactionDetail();
            }
        }

    }
    public partial class Transaction
    {
        public AspNetUser User { get; set; }
        public AspNetUser UserInccured { get; set; }
    }
}

