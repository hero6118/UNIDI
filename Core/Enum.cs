using System.Collections.Generic;

namespace Core
{
    public class Enum_PaidStatus
    {
        public static int UnPaid = 0;
        public static int Paid = 1;
        public static List<string> Label = new List<string> { "UnPaid", "Paid" };
    }
    public class Enum_Database
    {
        public static int Main = 0;
        public static List<string> Label = new List<string> { "UniDi" };
    }
    public class Enum_UserType
    {
        public static int Admin = -1;
        public static int Member = 0;
        public static int Seller = 1;
        public static int Pool = 2;
        public static List<string> Label = new List<string> { "Member", "Seller", "Pool" };
    }
    public class Enum_TransactionType
    {
        public static int Deposit = 0;
        public static int Withdraw = 1;
        public static int Transfer = 2;
        public static int BuyProduct = 3;
        public static int CreatePool = 4;
        public static int BuyPackageListCoin = 5;
        public static int Commission = 6;
        public static List<string> Label = new List<string> 
        { 
            "Deposit",
            "WithDraw",
            "Transfer",
            "Buying",
            "Create Pool",
            "Buy Package",
            "Commission"
        };
    }
    public class Enum_ProductStatus
    {
        public static int Waiting = 0;
        public static int Active = 1;
        public static int Cancel = 2;
        public static int Delete = 3;
        public static List<string> label = new List<string>
        {
            "Waiting",
            "Actived",
            "Rejected",
            "Deleted"
        };
    }
    public class Enum_ProductStatusStock
    {
        public static int OutOfStock = 0;
        public static int Available = 1;
        public static int Pause = 2;
        public static List<string> Label = new List<string>
        {
            "Out of Stock",
            "Available",
            "Pause",
        };
    }   
    public class Enum_ListCoinStatus
    {
        public static int New = 0;
        public static int Review = 1;
        public static int Active = 2;
        public static int Reject = 3;
        public static List<string> Label = new List<string> 
        {
            "New",
            "Awaiting Review",
            "Actived",
            "Rejected"
        };
    }

    public class Enum_BusinessLicense
    {
        public static int New = 0;
        public static int Waiting = 1;
        public static int Active = 2;   
        public static int Reject = 3;
        public static List<string> Label = new List<string>
        {
            "new",
            "Waitting",
            "Active",
            "Reject"
        };
    }
    public class Enum_PoolStatus
    {
        public static int Pause = 0;
        public static int Runing = 1;
        public static int Cancel = 2;
        public static List<string> Label = new List<string>
        {
            "Pause",
            "Runing",
            "Canceled"
        };
    }
    public class Enum_DeliveryStatus
    {
        public static int New = 0;
        public static int Confirmed = 1;
        public static int Packaged = 2;
        public static int Delivering = 3;
        public static int Delivered =4;
        public static int Failure = 5;
        public static int Cancelled = 6;
        public static int CustomerCancels = 7;
        public static List<string> Label = new List<string>
        {
            "New",
            "Confirmed",
            "Packaged",
            "Delivering",
            "Delivered",
            "Failure",
            "Cancelled",
            "Customer Cancels"
        };
    }

}
