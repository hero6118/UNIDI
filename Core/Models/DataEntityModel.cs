using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using static Core.Models.Request.InfoCoinRequest;

namespace Core
{
    public partial class DataEntities : DbContext
    {
        public DataEntities(string connectionString, bool write = true, bool lazyLoading = true)
        {
            Database.Connection.ConnectionString = connectionString;
            if (!write)
                Configuration.LazyLoadingEnabled = write;
            if (!lazyLoading)
            {
                Configuration.ProxyCreationEnabled = lazyLoading;
                Configuration.AutoDetectChangesEnabled = lazyLoading;
            }
        }
        public static string ConnectionString(string host, int? databaseName)
        {
            var entityCnxStringBuilder = new EntityConnectionStringBuilder(System.Configuration.ConfigurationManager.ConnectionStrings["DataEntities"].ConnectionString);
            SqlConnectionStringBuilder sqlBuilder = null;
            //TransparentNetworkIPResolution = false, // Phương thức không được hỗ trợ ở phiên bản mới
            if (databaseName == Enum_Database.Main)
            {
                //sqlBuilder = new SqlConnectionStringBuilder(entityCnxStringBuilder.ProviderConnectionString)
                //{
                //    DataSource = @"42.117.2.219\SQL2012,2210",
                //    InitialCatalog = Enum_Database.Label[(int)databaseName],
                //    IntegratedSecurity = false,
                //    UserID = "sa",
                //    Password = Tool.DecryptString("wa0HsuFxvy5fLJScI0D4Lw==")
                //};
                if (host.Contains("localhost") || host.Contains("42.117.2.219"))
                {
                    sqlBuilder = new SqlConnectionStringBuilder(entityCnxStringBuilder.ProviderConnectionString)
                    {
                        DataSource = @"42.117.2.219\SQL2012,2210",
                        InitialCatalog = Enum_Database.Label[(int)databaseName],
                        IntegratedSecurity = false,
                        UserID = "sa",
                        Password = "Thuong@123" //Tool.DecryptString("wa0HsuFxvy5fLJScI0D4Lw==")
                    };
                }
                else
                {
                    sqlBuilder = new SqlConnectionStringBuilder(entityCnxStringBuilder.ProviderConnectionString)
                    {
                        DataSource = @"42.117.2.214\SQL2012,2210",
                        InitialCatalog = Enum_Database.Label[(int)databaseName],
                        IntegratedSecurity = false,
                        MaxPoolSize = 50000,
                        Pooling = true,
                        UserID = "sa",
                        Password = "Thuong@123" //Tool.DecryptString("wa0HsuFxvy5fLJScI0D4Lw==")
                    };
                }
            }
            return sqlBuilder?.ConnectionString;
        }
        public static DataEntities Create(string host, int? databaseName, bool write = true, bool lazyLoading = true)
        {
            return new DataEntities(ConnectionString(host, databaseName), write, lazyLoading);
        }
        public bool Exists<T>(T entity) where T : class
        {
            var check = Set<T>().Local.IndexOf(entity) > -1;
            return check;
        }
        public ObservableCollection<T> Get<T>() where T : class
        {
            var check = Set<T>().Local;
            return check;
        }
    }
    public partial class Product
    {
        public Country CountryInfo { get; set; }
        public Brand BrandInfo { get; set; }    
        public List<string> ListImages { get; set; }
        public BusinessLicense BusinessLicense { get; set; }
        public List<Category> GetAllInfoCate { get; set; }
        public List<Pool> PoolInfo { get; set; }
        public List<ProductProperty> ProductProperty { get; set; }  
        public double? TempDiscountPercent { get; set; }
    }

    public partial class PoolInfo
    {
        public string IdPool { get; set; }
        public string Price { get; set; }
        public string Percent { get; set; }
    }

    public partial class BusinessLicense
    {
        public AspNetUser Userinfo { get; set; }
    }
    public partial class Category

    {
        public int CountProduct { get; set; } = 0;
        public bool TempChild { get; set; }
        public List<Category> SubCategory { get; set; } = new List<Category>();
    }

    public partial class ReferralLink

    {

        public ReferralLink Parent { get; set; }
        public List<ReferralLink> Children { get; set; } = new List<ReferralLink>();
    }
    public class TreeCategory
    {
        public Category Category { get; set; }
        public List<TreeCategory> Children { get; set; } = new List<TreeCategory>();
    }
    public partial class AspNetUser
    {
        //public Category Category { get; set; }
        public AspNetUserRole AspNetUserRole { get; set; }
        public AspNetRole Role { get; set; }
        public bool TempCheckAfiliate { get; set; }

    }
    public partial class AspNetUserRole
    {
        public AspNetRole Role { get; set; }
        public string NameRole { get; set; }
        public AspNetUser InfoUser { get; set; }

    }

    public partial class Invoice
    {
        public AspNetUser User { get; set; }
        public AspNetUser Shop { get; set; }
    }
    public partial class DataRoles
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public partial class ResponseImg
    {
        public bool Status { get; set; }
        public string Ms { get; set; }
        public List<string> ListUrlImages { get; set; }

    }


    public partial class InvoiceDetail
    {

    }

    public partial class ProductLicense
    {
        // public Product Product { get; set; }
        public Product ProductLicensee { get; set; }
    }
    public partial class CartUser
    {
        // public CartUser CartUser { get; set; }

        public Product ProductInfo { get; set; }
        public BusinessLicense ShopInfo { get; set; }
        public List<ProductImage> ProImg { get; set; }
        public double? Totalprice { get; set; }
        public Pool SingleInfoPool { get; set; }
       
    }


    public partial class GetCateInfo
    {


        public Category CateParent { get; set; }
        public List<Category> CateChildren { get; set; }

    }
    public partial class TreeNode
    {
        public Category CateParent { get; set; }
        public List<TreeNode> Children { get; set; } = new List<TreeNode>();
    }

    public partial class Comment
    {
        public string IdUser { get; set; }
        public string NameUser { get; set; }
        public string Avatar { get; set; }
    }
    public partial class LicenInfo
    {
        //public Category Category { get; set; }
        public BusinessLicense Info { get; set; }
        public AspNetUser InfoUser { get; set; }
        public int TotalComent { get; set; }
        public int Total { get; set; }
    }
    public partial class ListAddress
    {
        public Country CountryInfo { get; set; }
    }
    public partial class ProductPool
    {
        public Product ProductInfo { get; set; }
    }

    public partial class Pool
    {
        public ListCoin TempToken { get; set; }
        public List<Product> TempListProduct { get; set; }
        public string ImgCoin { get; set; }     
    }
    public partial class ListCoin
    {
      //  public double? Percent_24h { get; set; }
        public double? TempBalance { get; set; }
        public double? TempEstimate { get; set;}
    }
    public partial class Transaction
    {
        public TransactionDetail TransDetail { get; set; }
    }
    public partial class TestClass
    {
        public string Colum1e { get; set; }
        public string Colum2e { get; set; }
    }
    public partial class CartUser
    {
        public Pool Pool { get; set; }
    }
    public partial class UserAddress
    {
        public Country ContryInfo {get; set;}
    }
    public partial class ProductFavorite
    {
        public Product Product { get; set; }
    }
}
