using Core;
using Core.Models.Request;
using Core.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using X.PagedList;
using System.Globalization;
using System.Text;
using Swashbuckle.AspNetCore.Annotations;
using System.Diagnostics.Metrics;
using MailKit.Search;
using PayPalCheckoutSdk.Orders;


namespace UniDi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        public string? _host;
        public IHttpContextAccessor? _accessor;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("[action]")]
        public ActionResult GetAllCountry(string? SearchCountry)
        {

            using var de = new DataEntities();
            de.Configuration.LazyLoadingEnabled = false; // 1 cai doc 1 cai ghi
            de.Configuration.ProxyCreationEnabled = false;
            var searches = Tool.LocDau(SearchCountry);
            var listcountry = de.Countries.Where(p => string.IsNullOrEmpty(SearchCountry) || p.Nicename.Contains(searches)).ToList();

            return Ok(new { check = true, ms = "Get country success!", data = listcountry });

        }
        [HttpPost("[action]")]
        public async Task<ProductResponse> UpdateProduct([FromForm] ProductRequest model)
        {
            try
            {
                var user = C_User.Auth(Request.Headers["Authorization"]);
                if (user == null)
                    return new ProductResponse { Check = false, Ms = "Your login session has expired, please login again!" };

                using var de = new DataEntities();
                var license = de.BusinessLicenses.AsNoTracking().FirstOrDefault(p => p.ShopId == user.Id);
                if (license == null)
                {
                    return new ProductResponse { Check = false, Ms = "Please waiting for active License " };
                }

                if (model.OldPrice == null || model.OldPrice <= 0)
                    return new ProductResponse { Check = false, Ms = "Please enter price > 0" };
                if (model.SalePrice <= 0)
                    return new ProductResponse { Check = false, Ms = "Sale Price must >0" };

                if (model.SalePrice == null)
                    model.SalePrice = model.OldPrice;

                if (model.SalePrice > model.OldPrice)
                    return new ProductResponse { Check = false, Ms = "Discount price must be less than selling price" };

                if (model.Image == null || model.Image.Length == 0)
                    return new ProductResponse { Check = false, Ms = "Please choose a image" };

                if (model.ImagePreview == null || model.ImagePreview.Count == 0)
                    return new ProductResponse { Check = false, Ms = "Please choose a image preview" };

                if (license.Status == Enum_BusinessLicense.Active)
                {
                    if (string.IsNullOrEmpty(model.Name))
                        return new ProductResponse { Check = false, Ms = "Field name cannot be left blank" };
                    var c = 0;
                    var slug = model.Slug;
                    if (string.IsNullOrEmpty(slug))
                        slug = Tool.LocDauUrl(model.Name);

                    var cate = de.Categories.FirstOrDefault(p => p.Id == model.CategoryId);
                    if (cate == null)
                        return new ProductResponse { Check = false, Ms = "Category does not exist" };

                    if (model.Id == null || model.Id == Guid.Empty)
                    {
                        while (de.Products.AsNoTracking().Any(p => p.Slug == slug))
                        {
                            c++;
                            slug = Tool.LocDauUrl(model.Name) + "-" + c;
                        }

                        var item = new Product
                        {
                            Id = Guid.NewGuid(),
                            Name = model.Name,
                            Description = model.Description,
                            Status = Enum_ProductStatus.Waiting,
                            DateCreate = DateTime.Now,
                            Slug = slug,
                            CategoryId = model.CategoryId,
                            CateNode = cate.CateNode,
                            CountryId = model.CountryId,
                            Guarantee = model.Guarantee,
                            OldPrice = model.OldPrice,
                            SalePrice = model.SalePrice,
                            Origin = model.Origin,
                            BrandNameId = model.BrandId,
                            Weight = model.Weight,
                            Rating = 0,
                            CountRating = 0,
                            Unit = model.Unit,
                            SKU = model.SKU,

                            Expiry = model.Expiry,
                            ExpiryType = model.ExpiryType,
                            CountView = model.CountView,


                            UserId = user.Id
                        };


                        item.DiscountPercent = model.SalePrice / model.OldPrice * 100;
                        item.KValuePercent = model.DiscountForWeb;
                        item.KValue = model.SalePrice * model.DiscountForWeb / 100;

                        if (model.Image != null)
                        {

                            var requestImg = await C_Request.UploadImage(model.Image, 500);
                            if (!requestImg.Status)
                                return new ProductResponse { Check = false, Ms = requestImg.Message };
                            item.Image = requestImg.Result.Url;
                        }

                        if (model.ImagePreview != null)
                        {
                            var requestImg = await C_Request.UploadImage(model.ImagePreview, 1200);
                            foreach (var img in requestImg.Result)
                            {
                                var itemImg = new ProductImage
                                {
                                    Id = new int(),
                                    ProductId = item.Id,
                                    Link = img.Url,
                                };
                                de.ProductImages.Add(itemImg);
                            }
                        }

                        var properties = new List<ProductProperty>();

                        if (model.ProductProperties_Color != null)
                        {
                            for (int i = 0; i < model.ProductProperties_Color.Count; i++)
                            {
                                var color = model.ProductProperties_Color[i] ?? null;
                                var size = "NonSize";
                                if (model.ProductProperties_Size != null)
                                {
                                    size = model.ProductProperties_Size[i];
                                }
                                if (model.ProductProperties_Quantity == null)
                                {
                                    return new ProductResponse { Check = false, Ms = "Please enter quantity greater than 0" };
                                }
                                if (model.ProductProperties_Price == null)
                                {
                                    return new ProductResponse { Check = false, Ms = "Please enter Price greater than 0" };
                                }

                                var quantityup = model.ProductProperties_Quantity[i];
                                var price = model.ProductProperties_Price[i];

                                if (price == null || price <= 0)
                                    return new ProductResponse { Check = false, Ms = "Please enter price greater than 0" };
                                if (quantityup == null || quantityup <= 0)
                                    return new ProductResponse { Check = false, Ms = "Please enter Quantity greater than 0" };

                                var imt = new ProductProperty
                                {
                                    Id = Guid.NewGuid(),
                                    ProductId = item.Id,
                                    Price = price,
                                    Color = color,
                                    Size = size,
                                    Quantity = quantityup,
                                };
                                properties.Add(imt);
                            }
                            item.QuantityTotal = properties.Sum(p => p.Quantity);
                            item.QuantityAvailable = item.QuantityTotal;
                        }

                        else
                        {
                            if (model.TotalQuanity == null || model.TotalQuanity <= 0)
                                return new ProductResponse { Check = false, Ms = "Please enter Quantity greater than 0" };

                            item.QuantityTotal = model.TotalQuanity;
                            item.QuantityAvailable = item.QuantityTotal;
                        }

                        item.QuantitySold = 0;
                        de.ProductProperties.AddRange(properties);
                        de.Products.Add(item);
                        de.SaveChanges();
                        return new ProductResponse { Check = true, Ms = "Create Successfully" };
                    }
                    //Edit product
                    else
                    {

                        var pro = de.Products.FirstOrDefault(p => p.Id == model.Id);

                        if (pro != null)
                        {
                            while (de.Products.Any(p => p.Slug == slug && p.Id != pro.Id))
                            {
                                c++;
                                slug = Tool.LocDauUrl(model.Name) + "-" + c;
                            }
                            pro.Name = model.Name;
                            pro.Description = model.Description;
                            if (model.StatusStock != null)
                            {
                                pro.Status = model.StatusStock;
                            }
                            else
                            {
                                pro.Status = Enum_ProductStatusStock.Available;
                            }

                            pro.Slug = slug;

                            pro.CategoryId = model.CategoryId;
                            pro.CateNode = cate.CateNode;
                            pro.CountryId = model.CountryId;

                            pro.OldPrice = model.OldPrice;
                            pro.SalePrice = model.SalePrice;

                            pro.Guarantee = model.Guarantee;

                            pro.Origin = model.Origin;
                            if (model.ManufactureDate.HasValue && model.ManufactureDate >= DateTime.Now)
                                pro.ManufactureDate = model.ManufactureDate;
                            pro.BrandNameId = model.BrandId;

                            pro.Weight = model.Weight;

                            pro.Unit = model.Unit;
                            pro.SKU = model.SKU;
                            pro.DiscountPercent = model.SalePrice / model.OldPrice * 100;

                            pro.KValuePercent = model.DiscountForWeb;
                            pro.KValue = model.SalePrice * model.DiscountForWeb / 100;



                            pro.Expiry = model.Expiry;
                            pro.ExpiryType = model.ExpiryType;
                            pro.CountView = model.CountView;

                            pro.Status = Enum_ProductStatus.Waiting;
                            pro.UserId = user.Id;


                            if (model.Image != null)
                            {

                                var requestImg = await C_Request.UploadImage(model.Image, 500);
                                if (!requestImg.Status)
                                    return new ProductResponse { Check = false, Ms = requestImg.Message };
                                pro.Image = requestImg.Result.Url;
                            }

                            if (model.ImagePreview != null)
                            {
                                var requestImg = await C_Request.UploadImage(model.ImagePreview, 1200);
                                foreach (var img in requestImg.Result)
                                {
                                    var itemImg = new ProductImage
                                    {
                                        Id = new int(),
                                        ProductId = pro.Id,
                                        Link = img.Url,
                                    };
                                    de.ProductImages.Add(itemImg);
                                }
                            }

                            var productpr = de.ProductProperties.FirstOrDefault(p => p.ProductId == pro.Id);

                            if (productpr != null)
                            {

                                int? toltalquantity = 0;
                                // khi có property của sản phẩm
                                if (model.PropertyId != null)
                                {
                                    foreach (var imtp in model.PropertyId)
                                    {
                                        int i = 0;

                                        var prpt = de.ProductProperties.FirstOrDefault(p => p.Id == imtp && p.ProductId == pro.Id);
                                        if (prpt == null)
                                            return new ProductResponse { Check = false, Ms = " Can't found property to Edit " };

                                        if (model.ProductProperties_Quantity == null || model.ProductProperties_Quantity[i] <= 0)
                                            return new ProductResponse { Check = false, Ms = "Please enter quantity greater than 0" };

                                        if (model.ProductProperties_Price == null || model.ProductProperties_Price[i] <= 0)
                                            return new ProductResponse { Check = false, Ms = "Please enter Price greater than 0" };

                                        prpt.Price = model.ProductProperties_Price[i];
                                        prpt.Quantity = model.ProductProperties_Quantity[i];
                                        toltalquantity += prpt.Quantity;
                                        i++;
                                    }
                                    pro.QuantityTotal = toltalquantity;
                                    pro.QuantityAvailable = toltalquantity;
                                }
                            }
                            else
                            {
                                if (model.TotalQuanity == null || model.TotalQuanity <= 0)
                                    return new ProductResponse { Check = false, Ms = "Please enter Quantity greater than 0" };
                                pro.QuantityTotal = model.TotalQuanity;
                                pro.QuantityAvailable = model.TotalQuanity;
                            }

                            de.SaveChanges();
                            return new ProductResponse { Check = true, Ms = "Update Successfully " };
                        }
                        else
                        {
                            return new ProductResponse { Check = false, Ms = "Can't found product o edit " };
                        }
                    }
                }
                else
                    return new ProductResponse { Check = false, Ms = license.Status.ToString() };
            }
            catch (Exception ex)
            {
                var ms = ex.Message + "\n" + ex.InnerException?.Message + "\n" + ex.InnerException?.InnerException?.Message;
                return new ProductResponse { Check = false, Ms = ms };
            }
        }
        // delete 1 product
        [HttpPost("[action]")]
        public ActionResult DeleteProduct(Guid id)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!" });
            using var de = new DataEntities();
            var product = de.Products.First(m => m.Id == id);
            if (de.InvoiceDetails.Any(p => p.ProductId == id))
                product.Status = Enum_ProductStatus.Delete;
            else
            {
                de.Products.Remove(product);
                var listImg = de.ProductImages.Where(p => p.ProductId == id);
                de.ProductImages.RemoveRange(listImg);
            }

            de.SaveChanges();
            return Ok(new { check = true, ms = "Delete category complete! " });
        }
        // delete multiple product
        [HttpPost("[action]")]
        public ActionResult DeleteMultiProduct(List<Guid> ProductRecordDeletebyId)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, ms = "Your login session has expired, please login again!", redirect = "/Login" });
            using var de = new DataEntities();
            foreach (var id in ProductRecordDeletebyId)
            {
                var product = de.Products.Single(s => s.Id == id);

                if (de.InvoiceDetails.Any(p => p.ProductId == id))
                    product.Status = Enum_ProductStatus.Delete;
                else
                {
                    de.Products.Remove(product);
                    var listImg = de.ProductImages.Where(p => p.ProductId == id);
                    de.ProductImages.RemoveRange(listImg);
                }
            }
            de.SaveChanges();
            return Ok(new { check = true, ms = "Delete multi product complete! " });
        }
        // get product detail
        [HttpGet("[action]")]
        [SwaggerOperation(Description = "Id product, curency == null cant found info pool of this currency")]
        public ActionResult ProductDetail(Guid? id)
        {
            using var de = new DataEntities();
            de.Configuration.LazyLoadingEnabled = false; // 1 cai doc 1 cai ghi
            de.Configuration.ProxyCreationEnabled = false;

            var product = de.Products.FirstOrDefault(p => p.Id == id);

            if (product == null)
                return Ok(new { check = false, ms = "Product does not exist" });
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            var ipv4 = remoteIpAddress?.MapToIPv4()?.ToString() ?? "N/A";

            // thêm view
            if (!de.CViewLogs.Any(e => e.ProductId == product.Id && e.IdAddress == ipv4))
            {
                var viewLog = new CViewLog
                {
                    ProductId = product.Id,
                    IdAddress = ipv4,
                    Timestamp = DateTime.Now
                };
                de.CViewLogs.Add(viewLog);
                de.SaveChanges();
            }
            var viewCountFromIp = de.CViewLogs
            .Count(log => log.ProductId == product.Id); // đếm view

            product.CountView = viewCountFromIp;
            de.SaveChanges();

            product.ProductProperty = de.ProductProperties.AsNoTracking().Where(p => p.ProductId == product.Id).ToList();
            product.Category = de.Categories.AsNoTracking().FirstOrDefault(p => p.Id == product.CategoryId); // info Cattegory

            product.CountryInfo = de.Countries.AsNoTracking().FirstOrDefault(p => p.Id == product.CountryId); // info COuntry Name

            product.ListImages = de.ProductImages.AsNoTracking().Where(p => p.ProductId == product.Id).Select(p => p.Link).ToList();
            product.BusinessLicense = de.BusinessLicenses.AsNoTracking().FirstOrDefault(p => p.ShopId == product.UserId); // info of shop

            if (product.BusinessLicense != null)
                product.BusinessLicense.Userinfo = de.AspNetUsers.AsNoTracking().FirstOrDefault(p => p.Id == product.UserId);

            if (product.Category != null)
                product.GetAllInfoCate = C_Category.GetAllParentCategory(de, product.CategoryId);
            product.PoolInfo = C_Pool.GetAllPoolAcceptProduct(de, product);

            var relatedproducts = de.Products.AsNoTracking().Where(p => p.Id != id && p.CateNode.StartsWith(product.CateNode)).Take(5).ToList();

            return Ok(new { check = true, ms = "success!", data = product, ProductRelate = relatedproducts });
        }
        // get all product by seller id

        [HttpGet("[action]")]
        public ProductSellerResponse GetAllProductBySeller(Guid? productid, string? nameproduct, string? sku, DateTime? from, DateTime? to, int page = 1, int limit = 20)
        {
            using var de = new DataEntities();


            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return new ProductSellerResponse { Check = false, Status = 201, Ms = "Your login session has expired, please login again!" };

            if (C_User.CheckAccept("Seller", user.Id) != true)
                return new ProductSellerResponse { Check = false, Status = 202, Ms = "Accept denied! " };
            if (to != null)
                to = to.Value.Add(new TimeSpan(23, 59, 59));

            de.Configuration.ProxyCreationEnabled = false;
            de.Configuration.LazyLoadingEnabled = false; // 1 cai doc 1 c

            var getallbyseller = de.Products.AsNoTracking().Where(p => p.UserId == user.Id &&
            (productid == null || productid == p.Id)
            && (string.IsNullOrEmpty(nameproduct) || p.Name.Contains(nameproduct))
            && (string.IsNullOrEmpty(sku) || p.SKU == sku)
            && (from == null || p.DateCreate > from)
            && (to == null || p.DateCreate <= to)

            ).OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);



            foreach (var item in getallbyseller)
            {
                item.CountryInfo = de.Countries.AsNoTracking().FirstOrDefault(p => p.Id == item.CountryId);
                item.BrandInfo = de.Brands.AsNoTracking().FirstOrDefault(p => p.BrandId == item.BrandNameId);
            }



            return new ProductSellerResponse { Check = true, Ms = "get all success", Data = getallbyseller, Total = getallbyseller.TotalItemCount };
        }

        //create or update category
        [HttpPost("[action]")]
        public async Task<ActionResult> UpdateCategory([FromForm] CategoryRequest model)
        {

            using (var de = new DataEntities())
            {
                var c = 0;
                var slug = "";
                if (string.IsNullOrEmpty(slug))
                    slug = Tool.LocDauUrl(model.Name);
                if (model.Id == null || model.Id == Guid.Empty)
                {
                    while (de.Categories.AsNoTracking().Any(p => p.Slug == slug))
                    {
                        c++;
                        slug = Tool.LocDauUrl(model.Name) + "-" + c;
                    }
                    var count = de.Categories.AsNoTracking().Count();
                    var node = count + "";
                    var nodechl = "";
                    if (model.ParentId == null || model.ParentId == Guid.Empty)
                    {
                       
                        while (de.Categories.AsNoTracking().Any(p => p.CateNode == node))
                        {
                            count++;
                            node = count + "";
                        }

                    }
                    else
                    {
                        var sponsor = de.Categories.FirstOrDefault(p => p.Id == model.ParentId);
                        if (sponsor == null)
                            return Ok(new  { check = false, ms = "Don't found category" });

                        var countchl = (de.Categories.Count(p => p.ParentId == sponsor.Id) + 1);
                         nodechl = sponsor.CateNode + '-' + countchl;
                        while (de.Categories.Any(p => p.CateNode == nodechl)) // check xem IdUser này đã có chưa, nếu đã bị xóa. sẽ tiếp tục +
                        {
                            count++;
                            nodechl = sponsor.CateNode + '-' + countchl;
                        }
                    }
                   


                    var item = new Category
                    {
                        Id = Guid.NewGuid(),
                        Name = model.Name,
                        ParentId = model.ParentId,
                        SlugCate = slug,
                        Slug = model.Slug,
                       
                    };
                    if(model.ParentId!= null)
                    {
                        item.CateNode = nodechl;
                      
                        int countlevelcate = nodechl.Split('-').Count(s => int.TryParse(s, out _));
                        item.LevelCate = countlevelcate;
                    }    
                    else
                    {
                        item.CateNode = node;
                        int countlevelcate = node.Split('-').Count(s => int.TryParse(s, out _));
                        item.LevelCate = countlevelcate;
                    }


            


                    if (model.Image != null)
                    {
                        var requestImg = await C_Request.UploadImage(model.Image, 500);
                        if (!requestImg.Status)
                            return Ok(new { check = false, ms = requestImg.Message });

                        item.Image = requestImg.Result.Url;
                    }
                    /*   if (model.Imgaee != null)
                       {
                           var requestImg = await C_Request.UploadImage3(model.Imgaee, 500);
                          *//* if (!requestImg.Status)
                               return Ok(new { check = false, ms = requestImg.Message });*//*
                          if(requestImg != null)
                           {
                               foreach(var img in requestImg)
                               {
                                   var itemImg = new ProductImage
                                   {
                                       Id = new int(),
                                       ProductId = item.Id.ToString(),
                                       Link = img,

                                   };
                                   de.ProductImages.Add(itemImg);
                               }

                           }

                       }*/
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
                        var count = de.Categories.AsNoTracking().Where(p => p.ParentId == cate.Id).Count();
                        var node = count + "";
                        while (de.Categories.AsNoTracking().Any(p => p.CateNode == node))
                        {
                            count++;
                            node = count + "";
                        }

                        cate.CateNode = node;
                        cate.Name = model.Name;
                        cate.LevelCate = model.levelcate;
                        cate.Slug = "T8";
                        cate.SlugCate = slug;
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
        }
        /* [HttpPut("[action]")]
         public async Task<ActionResult> UpdateCategory2(List< CategoryRequest> model)
         {
             using (var de = new DataEntities())
             {
                 foreach (var item1 in model)
                 {
                     var item = new Category
                     {
                         Id = Guid.NewGuid(),
                         Name = item1.Name,
                         ParentId = item1.ParentId,

                     };
                     de.Categories.Add(item);
                     de.SaveChanges();
                 }
             }
             return Ok(new { check = true, ms = "Update Successfully" });
         }*/
        //delete category
        [AllowAnonymous]
        [HttpGet("[action]")]
        public ActionResult DeleteCategory(Guid id)
        {

            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return Ok(new { check = false, status = 201, ms = "Your login session has expired, please login again!", redirect = "/Login" });

            if (C_User.CheckAccept("AdminProduct", user.Id) == false)
            {
                return Ok(new { check = false, status = 202, ms = "Accept denied! " });
            }

            using (var de = new DataEntities())
            {
                var CateId = de.Categories.First(m => m.Id == id);
                if (CateId != null)
                {
                    var idcateU = "baa60180-ac14-4e36-8c26-92b75f9547d0"; // CateUndefine
                    var catechild = de.Categories.Where(m => m.ParentId == id).ToList();

                    foreach (var itmes in catechild)
                    {
                        itmes.ParentId = Guid.Parse(idcateU);
                    }
                    var prod = de.Products.Where(p => p.CategoryId == CateId.Id).ToList();
                    foreach (var items in prod)
                    {
                        items.CategoryId = Guid.Parse(idcateU);
                    }
                }

                de.Categories.Remove(CateId);
                de.SaveChanges();
                return Ok(new { check = true, ms = "Delete category complete! " });
            }
        }
        // FIllter by product with condition having a Id of shop
        [HttpDelete("[action]")]
        public ActionResult DeleteMultiCate(IEnumerable<Guid> CateId)
        {
            using (var de = new DataEntities())
            {
                foreach (var id in CateId)
                {
                    var catee = de.Categories.Single(s => s.Id == id);

                    de.Categories.Remove(catee);
                }
                de.SaveChanges();
                return Ok(new { check = true, ms = "Delete multi category complete! " });
            }
        }

        [HttpPost("[action]")]
        public ActionResult DeleteMultiImageProduct(IEnumerable<int> imgproduct)
        {
            using (var de = new DataEntities())
            {

                foreach (var id in imgproduct)
                {

                    var ipro = de.ProductImages.FirstOrDefault(s => s.Id == id);
                    if (ipro != null)
                    {

                        de.ProductImages.Remove(ipro);
                    }
                }
                de.SaveChanges();
                return Ok(new { check = true, ms = "Delete multi category complete! " });
            }

        }

        //        [HttpGet("[action]")]
        //        public ActionResult FillterHome(Guid? typetoken = null,string? key ="",string? value ="",string? keysearch ="",string? valu0="",string? value0="", string? value1 = "", string? value2= "",string? value3 = "", string? value4 = "",  string? searchfor ="", int pagereq = 1, int limitreq = 20)
        //        {
        //            using var de = new DataEntities();
        //            de.Configuration.LazyLoadingEnabled = false; // 1 cai doc 1 cai ghi
        //            de.Configuration.ProxyCreationEnabled = false;
        //            var count = 0;
        //            var limit = limitreq;
        //            var page = pagereq;
        //            if (typetoken != null)
        //            {
        //                limit = int.MaxValue;
        //                page = 1;
        //            }


        //            var query = de.Products.AsQueryable().AsNoTracking();

        //            List<Product> products = new();

        //            if (key != "")
        //            {
        //                if (key == "searchproduct")
        //                {
        //                    if (searchfor == "catename" && value != "")
        //                    {
        //                        var catedeltail = de.Categories.AsNoTracking().FirstOrDefault(p => p.SlugCate == value);
        //                        if (catedeltail != null)
        //                        {
        //                            if (catedeltail.LevelCate == 1)
        //                            {
        //                                var procate = de.Products.AsNoTracking().OrderByDescending(p => p.DateCreate).Where(p => p.CategoryId == catedeltail.Id && p.StatusAcpt == "Active").ToPagedList(page, limit);
        //                                count = procate.TotalItemCount;
        //                                products.AddRange(procate);
        //                            }
        //                            else
        //                            {
        //                                var cate = catedeltail.Id.ToString();

        //                                var procate = de.Products.AsNoTracking().OrderByDescending(p => p.DateCreate).Where(p => p.CateChildId == cate && p.StatusAcpt == "Active").ToPagedList(page, limit);
        //                                count = procate.TotalItemCount;
        //                                products.AddRange(procate);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            return Ok(new { check = false, ms = "ERORR FROM SERVER !!!" });
        //                        }
        //                    }
        //                    else if (searchfor == "brand" && value != "")
        //                    {
        //                        if (value2 == "increase")
        //                        {
        //                            var brandproduct = de.Products.AsNoTracking().OrderBy(p => p.PriceSale).Where(p => p.StatusAcpt == "Active" && p.Brand.ToUpper().Contains(value.ToUpper())).ToPagedList(page, limit);
        //                            count = brandproduct.TotalItemCount;
        //                            products.AddRange(brandproduct);
        //                        }
        //                        else if (value2 == "decrease")
        //                        {
        //                            var brandproduct = de.Products.AsNoTracking().OrderByDescending(p => p.PriceSale).Where(p => p.StatusAcpt == "Active" && p.Brand.ToUpper().Contains(value.ToUpper())).ToPagedList(page, limit);
        //                            count = brandproduct.TotalItemCount;
        //                            products.AddRange(brandproduct);
        //                        }
        //                        else
        //                        {
        //                            var Nas = Tool.LocDau(value);
        //                            var brandproduct = de.Products.AsNoTracking().OrderByDescending(p => p.DateCreate).Where(p => p.StatusAcpt == "Active" && p.Brand.ToUpper().Contains(value.ToUpper())).ToPagedList(page, limit);
        //                            count = brandproduct.TotalItemCount;
        //                            products.AddRange(brandproduct);
        //                        }
        //                    }
        //                    else if (searchfor == "price")
        //                    {

        //                        if (value != "" && value2 != "")
        //                        {
        //                            var val = Double.Parse(value);

        //                            var val2 = Double.Parse(value2);

        //                            if (val > 0 && val2 > 0 && val2 > val)
        //                            {
        //                                var listprice = query.Where(p => p.Price >= val && p.Price < val2).ToPagedList(page, limit);
        //                                count = listprice.TotalItemCount;
        //                                products.AddRange(listprice);
        //                            }
        //                            else
        //                            {
        //                                return Ok(new { check = false, ms = "can't find any product" });
        //                            }
        //                        }

        //                        else if (value == "" && value2 != "")
        //                        {

        //                            var val2 = Double.Parse(value2);

        //                            if (val2 > 0)
        //                            {
        //                                var listprice = query.Where(p => p.Price <= val2).ToPagedList(page, limit);
        //                                count = listprice.TotalItemCount;
        //                                products.AddRange(listprice);
        //                            }
        //                        }
        //                        else if (value != "" && value2 == "")
        //                        {
        //                            if (value == "increase")    //
        //                            {
        //                                var listprice = query.OrderBy(p => p.PriceSale).ToPagedList(page, limit);
        //                                count = listprice.TotalItemCount;
        //                                products.AddRange(listprice);
        //                            }
        //                            else if (value == "decrease")
        //                            {
        //                                var listprice = query.OrderByDescending(p => p.PriceSale).ToPagedList(page, limit);
        //                                count = listprice.TotalItemCount;
        //                                products.AddRange(listprice);
        //                            }
        //                            else
        //                            {
        //                                var val = Double.Parse(value);
        //                                if (val > 0)
        //                                {
        //                                    var listprice = query.Where(p => p.Price >= val).ToPagedList(page, limit);
        //                                    count = listprice.TotalItemCount;
        //                                    products.AddRange(listprice);
        //                                }
        //                            }
        //                        }

        //                        else
        //                        {
        //                            var pros = de.Products.AsNoTracking().OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);
        //                            count = pros.TotalItemCount;
        //                            products.AddRange(pros);
        //                        }

        //                    }
        //                    else if (searchfor == "combine") // search with combine cateid
        //                    {
        //                        /*if (!string.IsNullOrEmpty(value))
        //                        {
        //                            query = query.OrderByDescending(p => p.DateCreate).Where(p => p.StatusAcpt == "Active" && SqlFunctions.StringConvert((double)SqlFunctions.Ascii(p.Name.ToUpper())).Contains(SqlFunctions.StringConvert((double)SqlFunctions.Ascii(value.ToUpper()))));
        //                        }*/

        //                        if (value0 == "searchproduct")
        //                        {
        //                            /* query = query.Where(p => p.StatusAcpt == "Active" && SqlFunctions.StringConvert((double)SqlFunctions.Ascii(p.Name.ToUpper())).Contains(SqlFunctions.StringConvert((double)SqlFunctions.Ascii(valu0.ToUpper()))));*/

        //                            // var pros = de.Products.AsNoTracking().OrderByDescending(p=>p.DateCreate).Where(p => p.Name.ToUpper().Contains(value.ToUpper())).ToPagedList(page, limit);

        //                            var normalizedKeyword = RemoveDiacritics(keysearch.ToLower());
        //                            var allProducts = de.Products.AsNoTracking().ToList();

        //                            // Lọc và tìm kiếm trong danh sách đã tiền xử lý
        //                            var sc = allProducts.Where(item =>
        //                                RemoveDiacritics(item.Name.ToLower()).Contains(normalizedKeyword));

        //                            count = query.Count();

        //                        }

        //                        if (value == "catename")
        //                        {
        //                            var catedeltail = de.Categories.AsNoTracking().FirstOrDefault(p => p.SlugCate == value1);
        //                            if (catedeltail != null)
        //                            {
        //                                if (catedeltail.LevelCate == 1)
        //                                {
        //                                    query = query.OrderByDescending(p => p.DateCreate).Where(p => p.CategoryId == catedeltail.Id && p.StatusAcpt == "Active");
        //                                    count = query.Count();

        //                                }
        //                                else
        //                                {
        //                                    var cate = catedeltail.Id.ToString();

        //                                    query = query.AsNoTracking().OrderByDescending(p => p.DateCreate).Where(p => p.CateChildId == cate && p.StatusAcpt == "Active");
        //                                    count = query.Count();

        //                                }
        //                            }
        //                        }
        //                        if (value2 == "increase")
        //                        {
        //                            query = query.OrderBy(p => p.PriceSale).Where(p => p.StatusAcpt == "Active");
        //                            count = query.Count();

        //                        }
        //                        if (value2 == "decrease")
        //                        {
        //                            query = query.OrderByDescending(p => p.PriceSale).Where(p => p.StatusAcpt == "Active");
        //                            count = query.Count();

        //                        }
        //                        if (value3 != "" && value4 != "")
        //                        {
        //                            var val3 = Double.Parse(value3);
        //                            var val4 = Double.Parse(value4);
        //                            query = query.OrderByDescending(p => p.DateCreate).Where(p => p.Price >= val3 && p.Price <= val4 && p.StatusAcpt == "Active");
        //                            count = query.Count();

        //                        }
        //                        products.AddRange(query.OrderByDescending(p => p.DateCreate).ToPagedList(page, limit));
        //                        /*
        //                                                    else
        //                                                    {
        //                                                        return Ok(new { check = false, ms = "ERORR FROM SERVER !!!" });
        //                                                    }*/
        //                        /* if (value3 == "increase" && value4 =="")
        //                         {
        //                             query = query.OrderBy(p => p.PriceSale).OrderByDescending(p => p.DateCreate);
        //                         }
        //                         if (value4 == "decrease" && value3 =="")
        //                         {
        //                             query = query.OrderByDescending(p => p.PriceSale).OrderByDescending(p => p.DateCreate);
        //                         }
        //                         if (value3 != "" && value4 !="")
        //                         {
        //                             var val1 = Double.Parse(value3);
        //                             var val2 = Double.Parse(value4);
        //                             query = query.Where(p => p.Price >= val1 && p.Price <= val2);
        //                         }*/

        //                        // products.AddRange(query.ToList());
        //                    }

        //                    else
        //                    {
        //                        var normalizedKeyword = RemoveDiacritics(keysearch.ToLower());
        //                        var allProducts = de.Products.AsNoTracking().ToList();

        //                        // Lọc và tìm kiếm trong danh sách đã tiền xử lý
        //                        var searchResults = allProducts.Where(item =>
        //                            RemoveDiacritics(item.Name.ToLower()).Contains(normalizedKeyword)
        //                        ).ToPagedList(page, limit);
        //                        // return Ok(new { check = true, ms = "", data = searchResults, total = searchResults.TotalItemCount });
        //                        return Ok(new { check = true, ms = "Search success!", data = searchResults, total = searchResults.TotalItemCount });
        //                        // return Ok(new { check = true, ms = "All Data!", data = searchResults });

        //                        /*var pros = products.Where(p =>p.StatusAcpt == "Active").ToPagedList(page, limit);

        //                      // var pros = de.Products.AsNoTracking().OrderByDescending(p=>p.DateCreate).Where(p => p.Name.ToUpper().Contains(value.ToUpper())).ToPagedList(page, limit);
        //                      count = pros.TotalItemCount;
        //                      products.AddRange(pros);*/
        //                    }
        //                }
        //                else if (key == "searchshop" && value != "")
        //                {
        //                    var shop = de.BusinessLicenses.AsNoTracking().FirstOrDefault(p => p.Name.Contains(value.ToLower()));
        //                    var shoppro = de.Products.AsNoTracking().Where(p => p.UserId == shop.ShopId && p.StatusAcpt == "Active").ToPagedList(page, limit);
        //                    return Ok(new { check = true, ms = "search success!", shop = shop, proofshop = shoppro });
        //                    // products.AddRange(shoppro);
        //                }


        //            }

        //            /*  if(typetoken !="")
        //              {
        //                  var licoin = de.ListCoins.FirstOrDefault(p => p.Id == typetoken);


        //                  if (licoin != null)
        //                  {

        //                      List<ProductPool> proo = new List<ProductPool>();

        //                      if(value =="searchcategory")
        //                      {
        //                          var procoinw = de.ProductPools.AsNoTracking().OrderByDescending(p => p.DateCreate).Where(p => p.TokenId == typetoken ).ToList();

        //                          foreach (var imm in procoinw)
        //                          {
        //                              var idp = Guid.Parse(imm.IdProduct);
        //                              var idcate = Guid.Parse(valu0);
        //                              var procs = imm.productinfo = de.Products.AsNoTracking().FirstOrDefault(p => p.CategoryId == idcate && p.Id == idp);
        //                          }
        //                          proo.AddRange(procoinw);

        //                      }

        //                      else
        //                      {
        //                          var procoinw = de.ProductPools.AsNoTracking().OrderByDescending(p => p.DateCreate).Where(p => p.TokenId == typetoken).ToList();
        //                          foreach (var imm in procoinw)
        //                          {
        //                              var idp = Guid.Parse(imm.IdProduct);
        //                              var procs = imm.productinfo = de.Products.AsNoTracking().FirstOrDefault(p => p.Id == idp);

        //                          }
        //                          proo.AddRange(procoinw);
        //                      }

        //                      return Ok(new { check = true, ms = "test", data = proo,Toltall = proo.Count });
        //                  }
        //                  else
        //                  {
        //                      return Ok(new {check = true, ms = "Cant found Coin"});
        //                  }    

        //              }
        //*/
        //            else
        //            {
        //                var pros = de.Products.AsNoTracking().Where(p => p.StatusAcpt == "Active").OrderByDescending(p => p.DateCreate).ToPagedList(page, limit);
        //                count = pros.TotalItemCount;
        //                products.AddRange(pros);
        //            }
        //            if (products != null)
        //            {
        //                foreach (var item in products)
        //                {
        //                    //  var username = de.AspNetUsers.FirstOrDefault(p => p.Id == item.UserId);

        //                    item.Category = de.Categories.AsNoTracking().FirstOrDefault(p => p.Id == item.CategoryId);

        //                    item.CountryInfo = de.Countries.AsNoTracking().FirstOrDefault(p => p.Id == item.CountryId);

        //                    var listImg = de.ProductImages.AsNoTracking().Where(p => p.ProductId == item.Id).ToList();
        //                    var newList = new List<string>();
        //                    foreach (var img in listImg)
        //                    {
        //                        newList.Add(img.Link);
        //                    }
        //                    item.ListImages = newList;
        //                    if (newList.Count() > 0)
        //                    {
        //                        item.Image = newList.FirstOrDefault();
        //                    }
        //                }
        //            }

        //            var getInfoPool = new Dictionary<string, string>();

        //            var nlistproduct = new List<Product>();

        //            if (typetoken != null)
        //            {
        //                foreach (var tk in products)
        //                {
        //                    var productwaiting = de.ProductPools.AsNoTracking().FirstOrDefault(p => p.CoinId == typetoken && p.ProductId == tk.Id);

        //                    if (productwaiting != null)
        //                    {

        //                        var pool = de.Pools.FirstOrDefault(p => p.Id == productwaiting.PoolId);
        //                        if (pool != null)
        //                        {
        //                            var poolInfo = new Dictionary<string, string>();

        //                            poolInfo.Add("id", pool.Id.ToString());
        //                            poolInfo.Add("price", pool.Price.ToString());
        //                            poolInfo.Add("percent", pool.PercentBalance.ToString());

        //                            tk.PoolInfo = poolInfo;
        //                            nlistproduct.Add(tk);
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                nlistproduct.AddRange(products);
        //                // count = nlistproduct.Count;
        //            }
        //            var totalresponse = 0;
        //            List<Product> dataRes = new List<Product>();

        //            var totalPage = Math.Ceiling((double)count / limit);
        //            var nextPage = page;
        //            var prevPage = page;
        //            if (page < totalPage)
        //            {
        //                nextPage = page + 1;
        //            }
        //            if (prevPage > 1)
        //            {
        //                prevPage = page - 1;
        //            }

        //            if (typetoken != null)
        //            {
        //                totalresponse = nlistproduct.Count;
        //                totalPage = Math.Ceiling((double)totalresponse / limitreq);
        //                nextPage = pagereq;
        //                prevPage = pagereq;
        //                if (pagereq < totalPage)
        //                {
        //                    nextPage = pagereq + 1;
        //                }
        //                if (prevPage > 1)
        //                {
        //                    prevPage = pagereq - 1;
        //                }
        //                if (totalresponse > limitreq)
        //                {
        //                    if (totalresponse - (pagereq - 1) * limitreq >= limitreq)
        //                    {
        //                        dataRes.AddRange(nlistproduct.GetRange((pagereq - 1) * limitreq, limitreq));
        //                    }
        //                    else
        //                    {
        //                        dataRes.AddRange(nlistproduct.GetRange((pagereq - 1) * limitreq, totalresponse - (pagereq - 1) * limitreq));
        //                        //dataRes.AddRange(nlistproduct.GetRange((page - 1) * limitreq, totalresponse) );
        //                    }
        //                }
        //                else
        //                {
        //                    dataRes.AddRange(nlistproduct);
        //                }
        //            }
        //            else
        //            {
        //                totalresponse = count;
        //                dataRes.AddRange(nlistproduct);
        //            }

        //            var result = new
        //            {
        //                Total = totalresponse,
        //                TotalItem = dataRes.Count,
        //                TotalPage = totalPage,
        //                NextPage = nextPage,
        //                PrevPage = prevPage,
        //                CurrentPage = pagereq,
        //            };

        //            return Ok(new { check = true, ms = "success!", data = dataRes, total = totalresponse, pagination = result });
        //        }

        /*  [HttpGet("[action]")]
          [SwaggerOperation(Description = "if isExactly = true get by excactly score || key = Name country, Id cate, Name shop, Name product")]
          public ActionResult FillterHome(int? page, int? limit, string? key, Guid? cateId, string? currency = "USDT", bool isExactly = false)
          {
              try
              {
                  key = Tool.LocDau(key).Replace("-", "").Replace("*", "").Replace("/", "");
                  using var de = new DataEntities();
                  de.Configuration.LazyLoadingEnabled = false; // 1 cai doc 1 cai ghi
                  de.Configuration.ProxyCreationEnabled = false;
                  var shopId = de.BusinessLicenses.AsNoTracking().FirstOrDefault(p => p.Name.Contains(key))?.ShopId;
                  var cateNode = de.Categories.AsNoTracking().FirstOrDefault(p => p.Id == cateId)?.CateNode;
                  var countryId = de.Countries.AsNoTracking().FirstOrDefault(p => p.Nicename.Contains(key))?.Id;

                  //var product = de.Products.AsNoTracking().Where(p =>
                  //(string.IsNullOrEmpty(key) || EF.Functions.Like(p.Name, $"%{key}%") || p.UserId == shopId || p.CateNode.StartsWith(cateNode) || p.CountryId == countryId)
                  //&& (string.IsNullOrEmpty(currency) || pool.Any(c => c.ProductId == p.Id))
                  //).OrderByDescending(p => p.DateCreate).ToPagedList(page ?? 1, limit ?? 20);
                  var query = @$"select * from products where ( Name COLLATE Latin1_general_CI_AI like '%{key}%'  ";
                  if (shopId != null) query += @$" or UserId = '{shopId}' ";
                  if (countryId != null) query += @$" or CountryId = '{countryId}' ";
                  query += " ) ";

                  if (!string.IsNullOrEmpty(currency))
                      query += @$" and EXISTS (SELECT 1 FROM PoolAcceptProduct WHERE products.Id = PoolAcceptProduct.ProductId and Currency = '{currency}') ";
                  if (cateNode != null)
                      query += @$" and (CateNode like '{cateNode}-%' or CategoryId = '{cateId}') ";


                  var product = de.Database.SqlQuery<Product>(query)
                      .OrderByDescending(p => p.DateCreate).ToPagedList(page ?? 1, limit ?? 20);

                  if (product.TotalItemCount == 0 && de.PoolAcceptProducts.Any(p => p.Currency == currency))
                  {
                      var split = key.Split(' ').Where(p => !string.IsNullOrEmpty(p));
                      var tempProduct = new List<Product>();
                      foreach (var item in split)
                      {
                          var pro = de.Database.SqlQuery<Product>(@$" select * from products where Name COLLATE Latin1_general_CI_AI like '%{item}%' ").ToList();
                          tempProduct.AddRange(pro);
                      }

                      var list = tempProduct.GroupBy(p => p.Id).Select(p => new ScoreSearch
                      {
                          Product = p.FirstOrDefault(),
                          Score = p.Count(),
                          ScorePercent = p.Count() / split.Count()
                      }).Where(p => !isExactly || p.ScorePercent == 1).OrderByDescending(p => p.Score).ToPagedList(page ?? 1, limit ?? 20);

                      product = list.Select(p => p.Product);
                  }

                  return Ok(new { check = true, ms = "success!", data = product, total = product.TotalItemCount });
              }
              catch (Exception ex)
              {
                  return Ok(new { check = false, ms = "Error!\n" + ex.Message + "\n" + ex.InnerException?.Message + "\n" + ex.InnerException?.InnerException?.Message });
              }
          }
  */
        [HttpGet("[action]")]  // this using
        [SwaggerOperation(Description = " priceto and pricefrom \n\n|if <b>popular</b>!= null get all product by totalsold | if <b>countview</b>  != null get product by countview \n\n| nameproduct = searchname product   \n\n| key = Name country, Name shop, Name brand. Key must be unicode to search name shop, nambrand | ")]
        public ActionResult FillterHome(double? priceto, double? pricefrom, string? countview, string? popular, string? nameproduct, string? key, Guid? cateId, string currency = "USDT", int page = 1, int limit = 20)
        {
            try
            {

                using var de = new DataEntities();
                de.Configuration.LazyLoadingEnabled = false; // 1 cai doc 1 cai ghi
                de.Configuration.ProxyCreationEnabled = false;

                var shopId = de.BusinessLicenses.AsNoTracking().FirstOrDefault(p => p.Name.Contains(key) || (p.ShopId == key))?.ShopId;

                var cateNode = de.Categories.AsNoTracking().FirstOrDefault(p => p.Id == cateId)?.CateNode;

                var countryId = de.Countries.AsNoTracking().FirstOrDefault(p => p.Nicename.Contains(key))?.Id;

                var BrandId = de.Brands.AsNoTracking().FirstOrDefault(p => p.BrandName.Contains(key))?.BrandId;

                string normalizedSearchTerm = "";

                if (nameproduct != null)
                    normalizedSearchTerm = RemoveDiacritics(nameproduct.ToLower());

                var itemsFromDatabase = de.Products.AsNoTracking().Where(e =>
                    (shopId == null || e.UserId == shopId)
                && e.Status == Enum_ProductStatus.Active
                && (BrandId == null || e.BrandNameId == BrandId)
                && (cateNode == null || e.CateNode == cateNode)
                && (countryId == null || e.CountryId == countryId)
                && (priceto == null || e.SalePrice <= priceto)
                && (pricefrom == null || e.SalePrice >= pricefrom)
                ).ToList();

                var newlisttoken = new List<Product>();
                if (currency != "USDT")
                {
                    foreach (var item in itemsFromDatabase)
                    {
                        var sds = C_Pool.GetAllPoolAcceptProduct(de, item).FirstOrDefault(p => p.Currency == currency);
                        if (sds != null)
                            newlisttoken.Add(item);
                    }
                }
                else
                {
                    newlisttoken = itemsFromDatabase;
                }    

                // đếm ngxem shop
                var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
                var ipv4 = remoteIpAddress?.MapToIPv4()?.ToString() ?? "N/A";
                var allshop = de.AspNetUsers.FirstOrDefault(p => p.Id == shopId)?.Click;
                if (shopId != null && !de.CViewLogs.Any(e => e.ShopId == shopId && e.IdAddress == ipv4))
                {
                    var viewLog = new CViewLog
                    {
                        ShopId = shopId,
                        IdAddress = ipv4,
                        Timestamp = DateTime.Now
                    };
                    de.CViewLogs.Add(viewLog);
                    de.SaveChanges();
                }

                if (shopId != null)
                {
                    var viewCountFromIp = de.CViewLogs
                    .Count(log => log.ShopId == shopId); // đếm view
                    allshop = viewCountFromIp;
                }
                de.SaveChanges();
                //

                var productbytoken = new List<Product>();



                var product = newlisttoken.Where(e =>
                (nameproduct == null || RemoveDiacritics(e.Name.ToLower()).Contains(normalizedSearchTerm)));

                if (!string.IsNullOrEmpty(countview))
                {
                    product.OrderByDescending(p => p.CountView);
                }
                else if (!string.IsNullOrEmpty(popular))
                {
                    product.OrderByDescending(p => p.QuantitySold);
                }
                else
                {
                    product.OrderByDescending(p => p.DateCreate);
                }



                var endlist = product.ToPagedList(page, limit);

                /*  if (endlist.TotalItemCount == 0  && nameproduct != null)
                  {
                       key = Tool.LocDau(nameproduct).Replace("-", "").Replace("*", "").Replace("/", "");
                      var split = key.Split(' ').Where(p => !string.IsNullOrEmpty(p));
                      var tempProduct = new List<Product>();
                      foreach (var item in split)
                      {
                          //var pro = itemsFromDatabase.Where(p=>p.Name.Contains(item));
                          var pro = de.Database.SqlQuery<Product>(@$" select * from products where Name COLLATE Latin1_general_CI_AI like '%{item}%' ").ToList();
                          tempProduct.AddRange(pro);
                      }

                      var list = tempProduct.GroupBy(p => p.Id).Select(p => new ScoreSearch
                      {
                          Product = p.FirstOrDefault(),
                          Score = p.Count(),
                          ScorePercent = p.Count() / split.Count()
                      }).Where(p => !isExactly || p.ScorePercent == 1).OrderByDescending(p => p.Score).ToPagedList(page, limit);

                      endlist = list.Select(p => p.Product);
                  }
  */


                foreach (var itm in endlist)
                {
                    itm.BusinessLicense = de.BusinessLicenses.FirstOrDefault(p => p.ShopId == itm.UserId);
                    itm.BrandInfo = de.Brands.FirstOrDefault(p => p.BrandId == itm.BrandNameId);
                    itm.ListImages = de.ProductImages.AsNoTracking().Where(p => p.ProductId == itm.Id).Select(p => p.Link).ToList();
                    itm.ProductProperty = de.ProductProperties.AsNoTracking().Where(p => p.ProductId == itm.Id).ToList();
                    itm.TempDiscountPercent = (1 - itm.SalePrice / itm.OldPrice) * 100;  // giá sale ước chừng
                }


                return Ok(new { check = true, ms = "success!", data = endlist, total = endlist.TotalItemCount, ToTalViewShop = allshop ?? 0 });
            }
            catch (Exception ex)
            {
                return Ok(new { check = false, ms = "Error!\n" + ex.Message + "\n" + ex.InnerException?.Message + "\n" + ex.InnerException?.InnerException?.Message });
            }
        }
        /*
                [HttpGet("[action]")]
                public ActionResult FillBycateChild(string? CateNameID = "")
                {
                    using (var de = new DataEntities())
                    {
                        de.Configuration.ProxyCreationEnabled = false;
                        de.Configuration.LazyLoadingEnabled = false;
                        var cateid = Guid.Parse(CateNameID);
                        var checkCate = de.Categories.AsNoTracking().FirstOrDefault(p => p.Id == cateid);
                        var checkCateChild =de.Categories.AsNoTracking().Where(p=>p.ParentId == checkCate.Id).ToList();

                        foreach(var cate in checkCateChild) 
                        {
                            cate.Products = de.Products.AsNoTracking().Where(p => p.CategoryId == cate.Id).ToList();
                            foreach (var item in cate.Products)
                            {
                                    item.CountryInfo = de.Countries.AsNoTracking().FirstOrDefault(p => p.Id == item.CountryId);

                                    var listImg = de.ProductImages.AsNoTracking().Where(p => p.ProductId == item.Id).ToList();
                                    var newList = new List<string>();
                                    foreach (var img in listImg)
                                    {
                                        newList.Add(img.Link);
                                    }
                                    item.ListImages = newList;
                                    if (newList.Count() > 0)
                                    {
                                        item.Image = newList.FirstOrDefault();
                                    }
                            }
                        }
                        return Ok(new {check = true, ms = "list product", data =checkCateChild});
                    }
                }
        */
        /*[HttpGet("[action]")]
        public ActionResult GetAllUnit()
        {
            var de = new DataEntities();
            de.Configuration.ProxyCreationEnabled = false;
            de.Configuration.LazyLoadingEnabled = false;
            var listunit = de.UnitProducts.AsNoTracking().ToList();
            return Ok(new {check = true, ms = "get all success! ", data = listunit});
        }
      */
        [HttpPost("[action]")]
        public async Task<UsingtResponse> AddBrand([FromForm] BrandReqest model)
        {
            var user = C_User.Auth(Request.Headers["Authorization"]);
            if (user == null)
                return new UsingtResponse { Check = false, Status = 201, Ms = "Your login session has expired, please login again!" };

            if (C_User.CheckAccept("Seller", user.Id) == false)
            {
                return new UsingtResponse { Check = false, Status = 202, Ms = "Accept denied! " };
            }

            using var de = new DataEntities();

            if (string.IsNullOrEmpty(model.BrandName))
            {
                return new UsingtResponse { Check = false, Ms = "please enter Name brand" };
            }

            var b = new Brand
            {
                BrandId = Guid.NewGuid(),
                BrandName = model.BrandName,
            };

            if (model.Image != null)
            {
                var requestImg = await C_Request.UploadImage(model.Image, 5000);
                if (!requestImg.Status)
                    return new UsingtResponse { Check = false, Ms = requestImg.Message };
                b.Image = requestImg.Result.Url;
            }
            de.Brands.Add(b);
            de.SaveChanges();

            return new UsingtResponse { Check = true, Ms = "Add Brand Successfully!!" };
        }

        [HttpGet("[action]")]
        public UsingtResponse GetAllBrand(string? keysearch)
        {
            using var de = new DataEntities();

            var allbrand = de.Brands.AsNoTracking().Where(p => string.IsNullOrEmpty(keysearch) || p.BrandName.Contains(keysearch)).ToList();

            return new UsingtResponse { Check = true, Ms = "Get all brand success!", Data = allbrand, Total = allbrand.Count };
        }
        //
        [HttpGet("[action]")]
        [SwaggerOperation(Description = "if IdCate == null get the largest category (ParentID == null)  ,if have a IdCate => get all cate that a Idparent == idCate")]
        public CateResponse GetAllCategory(Guid? IdCate, string? keysearch)
        {
            using var de = new DataEntities();
            de.Configuration.ProxyCreationEnabled = false;
            de.Configuration.LazyLoadingEnabled = false;

            try
            {
                if (IdCate == null || !de.Categories.AsNoTracking().Any(p => p.Id == IdCate))
                    IdCate = null;

                var get = de.Categories.AsNoTracking().Where(p => IdCate == p.ParentId && (string.IsNullOrEmpty(keysearch) || p.Name.Contains(keysearch))).ToList();
                foreach (var category in get)
                {

                    var cprduct = de.Products.AsNoTracking().Where(p => p.CateNode.StartsWith(category.CateNode)).ToList();
                    category.CountProduct = cprduct.Count;

                    if (de.Categories.Any(p => p.CateNode.StartsWith(category.CateNode + "-")))
                        category.TempChild = true;
                }
                if (get == null)
                    return new CateResponse { Check = false, Ms = "Fail!, wrong name cate " };

                return new CateResponse { Check = true, Ms = "Get all brand success!", Data = get, Total = get.Count };
            }

            catch (Exception ex)
            {
                var ms = ex.Message + "\n" + ex.InnerException?.Message + "\n" + ex.InnerException?.InnerException?.Message;
                return new CateResponse { Check = false, Ms = ms };
            }
        } // will be using

        [HttpGet("[action]")] // get by recursive to tree data
        public ActionResult GetAllCateGoryTree()
        {
            using var de = new DataEntities();
            de.Configuration.ProxyCreationEnabled = false;
            de.Configuration.LazyLoadingEnabled = false;
            var category = de.Categories.AsNoTracking().ToList();
            var categorytree = BuildCategoryTree(category);
            return Ok(new { check = true, ms = "Get all success", data = categorytree });
        }
        [NonAction]
        public List<TreeCategory> BuildCategoryTree(List<Category> categories, Guid? parentId = null)
        {
            var tree = new List<TreeCategory>();
            var filteredCategories = categories.Where(c => c.ParentId == parentId).ToList();

            foreach (var category in filteredCategories)
            {
                var treeCategory = new TreeCategory
                {
                    Category = category,
                    Children = BuildCategoryTree(categories, category.Id)
                };

                tree.Add(treeCategory);
            }

            return tree;
        }

        [NonAction]
        static string RemoveDiacritics(string text)
        {
            string normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();

            foreach (char c in normalizedString)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        [HttpGet("[action]")]
        public ActionResult GetShopInfo(string? id = "")
        {
            using (var de = new DataEntities())
            {
                de.Configuration.LazyLoadingEnabled = false; // 1 cai doc 1 cai ghi
                de.Configuration.ProxyCreationEnabled = false;

                var data = new LicenInfo();

                if (id == "")
                    return Ok(new { check = false, ms = "Missing parameter!" });

                //var getcate = from r in de.Categories select r;
                var busl = de.BusinessLicenses.AsNoTracking().FirstOrDefault(p => p.ShopId == id);
                if (busl == null)
                    return Ok(new { check = false, ms = "Cant found Info user account" });

                data.Info = busl;
                var useraccount = de.AspNetUsers.AsNoTracking().FirstOrDefault(p => p.Id == busl.ShopId); //get info user create a shop
                if (useraccount == null)
                    return Ok(new { check = false, ms = "Cant found Info user account" });


                data.InfoUser = useraccount;

                var prototal = de.Products.AsNoTracking().Where(p => p.UserId == useraccount.Id).ToList(); // get total of product
                data.Total = prototal.Count;

                return Ok(new { check = true, ms = "Get Detail License Success!", data = data });
            }

        }

        [HttpDelete("[action]")]
        public ActionResult DeletePropertyProduct([FromForm] deletePropertyRequest model)
        {
            using var de = new DataEntities();
            foreach (var item in model.IdProperty)
            {
                var delpropert = de.ProductProperties.FirstOrDefault(p => p.Id == item);
                if (item != Guid.Empty && delpropert != null)
                {
                    var pro = de.Products.FirstOrDefault(p => p.Id == model.Idproduct);
                    if (pro != null)
                        pro.QuantityTotal -= delpropert.Quantity;

                    de.ProductProperties.Remove(delpropert);
                    de.SaveChanges();
                }
                else
                    return Ok(new { check = false, ms = "Can't found product property" });
            }
            if (model.Idproduct != Guid.Empty)
            {
                var pro = de.Products.FirstOrDefault(p => p.Id == model.Idproduct);
                if (pro != null)
                    pro.QuantityAvailable = de.ProductProperties.Sum(p => p.Quantity);
                de.SaveChanges();
            }
            return Ok(new { check = true, ms = "Delete property success!" });
        }
        [NonAction]
        private int CountProducts(Guid categoryId)
        {
            using var de = new DataEntities();
            var cateid = categoryId.ToString();
            var node = de.Categories.AsNoTracking().FirstOrDefault(p => p.Id == categoryId)?.CateNode + "-";
            if (node == null) return 0;
            return de.Products.Count(p => p.CateNode.StartsWith(node) || p.CategoryId == categoryId);
        }
    }
}