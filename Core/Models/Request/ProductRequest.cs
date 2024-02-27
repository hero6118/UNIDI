using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Request
{
    public class ProductRequest
    {
        public Guid? Id { get; set; }
        public List<Guid?> PropertyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? BrandId { get; set; }
        public Nullable<int> CountryId { get; set; }
        public int? StatusStock { get; set; }
        public IFormFile Image { get; set; }
        public List<IFormFile> ImagePreview { get; set; }
        public string Guarantee { get; set; }
        public Nullable<double> OldPrice { get; set; }
        public Nullable<double> SalePrice { get; set; }
        public string Origin { get; set; }
        public Nullable<System.DateTime> ManufactureDate { get; set; }
        public Nullable<double> Weight { get; set; }
        public Nullable<double> DiscountForWeb { get; set; }  
        public string SKU { get; set; } 
        public Nullable<int> Expiry { get; set; }
        public Nullable<int> ExpiryType { get; set; }
        public Nullable<int> CountView { get; set; }
        public Nullable<double> FeeShipSpecial { get; set; }
        public string Slug { get; set; }
        public int? TotalQuanity { get; set; }

        public List<string> ProductProperties_Color { get; set; }
        public List<double?> ProductProperties_Price { get; set; }
        public List<int?> ProductProperties_Quantity { get; set; }
        public List<string> ProductProperties_Size { get; set; }


        //edit property



    }
    public class DetailProductRequest
    {
        public string IdDetailPro { get; set; }
        public string IdMap { get; set; }
        public string Value { get; set;}
        public Nullable<double> priceMap { get; set; }
        public Nullable<int> quantityMap { get; set; }
        public List<string> ImageUrls { get; set; }

        /// <summary>
        /// /
        /// </summary>
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? CategoryId { get; set; }
        //  public string Status { get; set; }
        public Nullable<int> Counts { get; set; }
        public string Brand { get; set; }
        public string NameModel { get; set; }

        public string Size { get; set; }
        public string Color { get; set; }   
        public string Port { get; set; }
        public string Storage { get; set; }
        public string OperatorSystem { get; set; }
        public string TypeStorage { get; set; }
        public string Processor { get; set; }
        public string Pin { get; set; }
        public string FrequencyCPU { get; set; }
        // public IFormFile Image { get; set; }

        // [StringLength( ,ErrorMessage = "Need a Picture.")]
        public List<IFormFile> Image { get; set; }
        public string LinkImage { get; set; }
        public IFormFile ImageCover { get; set; }
        //public Nullable<System.DateTime> DateCreate { get; set; }

        public string Guarantee { get; set; }
        public string FaceWatch { get; set; }
        public string TypeConnect { get; set; }
        public string TypeLen { get; set; }
        public string Manufacture { get; set; }
        public string TextEditor { get; set; }

        public Nullable<double> Price { get; set; }
        public Nullable<double> Sale { get; set; }
        public Nullable<double> PriceSale { get; set; }
        public string Origin { get; set; }
        public string Material { get; set; }
        public string Style { get; set; }
        public Nullable<System.DateTime> ManufactureDate { get; set; }
        public string Country { get; set; }
        public Nullable<System.Guid> BrandNameId { get; set; }
        public Nullable<int> Type { get; set; }
        public Nullable<double> Weight { get; set; }

        public Nullable<bool> Hot { get; set; }
        public Nullable<double> Rating { get; set; }
        public Nullable<int> CountRating { get; set; }
        public int UnitId { get; set; }
        public Nullable<double> discount { get; set; }
        public string SKU { get; set; }

        public Nullable<double> QuantityAvailable { get; set; }
        public Nullable<double> QuantitySold { get; set; }
        public Nullable<double> QuantityTotal { get; set; }
        public Nullable<System.Guid> PartnerId { get; set; }

        public string PartnerCode { get; set; }
        public Nullable<bool> IsVoucher { get; set; }
        public Nullable<double> VoucherDiscountPercent { get; set; }
        public Nullable<int> Expiry { get; set; }
        public Nullable<int> ExpiryType { get; set; }
        public Nullable<int> CountView { get; set; }
        public Nullable<double> KValueAgency { get; set; }
        public Nullable<double> FeeShipSpecial { get; set; }
        public Nullable<System.Guid> WareHouseId { get; set; }
        public string Slug { get; set; }
        public Nullable<int> CountryId { get; set; }
        public string CateChildId { get; set; }

        public List<int> ListImages { get; set; }
      

    }
    public class deletePropertyRequest
    {
        public Guid Idproduct { get; set; }
        public List<Guid> IdProperty { get; set; }
    }
    public class BrandReqest
    {

        public string BrandName { get; set; }
        public string Image { get; set; }
    }
}

