using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class C_Category
    {
        public static List<Category> GetAllParentCategory(Guid? cateId)
        {
            using (var de = new DataEntities())
            {
                return GetAllParentCategory(de, cateId);
            }
        }
        public static List<Category> GetAllParentCategory(DataEntities de,Guid? cateId)
        {
            var cate = de.Categories.AsNoTracking().FirstOrDefault(p => p.Id == cateId);
            var list = new List<Category>
            {
                cate
            };

            while (cate != null)
            {
                cate = de.Categories.AsNoTracking().FirstOrDefault(p => p.Id == cate.ParentId);
                if (cate != null)
                    list.Add(cate);
            }
            
            return list.OrderBy(p=>p.CateNode).ToList();
        }
    }
}
