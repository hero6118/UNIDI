using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class C_Pool
    {
        public static List<Pool> GetAllPoolAcceptProduct(Product product)
        {
            using (var de = new DataEntities())
            {
                return GetAllPoolAcceptProduct(product);
            }
        }
        public static List<Pool> GetAllPoolAcceptProduct(DataEntities de, Product product)
        {
            var listPool = new List<Pool>();
            var listPoolAcceptAllProduct = de.Pools.AsNoTracking().Where(p => p.Status == Enum_PoolStatus.Runing && (p.AcceptAllProduct == true || de.PoolAcceptProducts.Any(c => c.ProductId == product.Id)));
            listPool.AddRange(listPoolAcceptAllProduct);
            foreach (var item in listPool)
            {
                var checkRemove = false;
                if (item.AcceptAllCategory != true)
                {
                    var listPoolAcceptCategory = de.PoolAcceptCategories.AsNoTracking().Where(p => p.PoolId == item.Id);
                    foreach (var cate in listPoolAcceptCategory)
                    {
                        if (cate.CategoryId != product.CategoryId)
                        {
                            item.Status = Enum_PoolStatus.Cancel;
                            checkRemove = true;
                            break;
                        }
                    }
                }
                if (checkRemove)
                    continue;

                if (item.AcceptAllCountry != true)
                {
                    var listPoolAcceptCountry = de.PoolAcceptCountries.AsNoTracking().Where(p => p.PoolId == item.Id);
                    foreach (var country in listPoolAcceptCountry)
                    {
                        if (country.CountryId != product.CountryId)
                        {
                            item.Status = Enum_PoolStatus.Cancel;
                            checkRemove = true;
                            break;
                        }
                    }
                }

                if (checkRemove)
                    continue;

                if (item.AcceptAllShop != true)
                {
                    var listShop = de.PoolAcceptShops.AsNoTracking().Where(p => p.PoolId == item.Id);
                    foreach (var shop in listShop)
                    {
                        if (shop.ShopId != product.UserId)
                        {
                            item.Status = Enum_PoolStatus.Cancel;
                            checkRemove = true;
                            break;
                        }
                    }
                }

                if (checkRemove)
                    continue;


                // update Price
                if(item.IsPriceRealTime == true)
                {
                    item.Price = de.ListCoins.AsNoTracking().FirstOrDefault(p => p.SymbolLabel == item.Currency)?.Price;
                    if(item.Price == null)
                        item.Status = Enum_PoolStatus.Cancel;
                }
            }

            listPool = listPool.Where(p=>p.Status != Enum_PoolStatus.Cancel).OrderByDescending(p => p.Price).GroupBy(p => p.Currency).Select(p => p.FirstOrDefault()).ToList();

            return listPool;
        }
    }
}
