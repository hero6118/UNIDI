using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Request
{
    public class CategoryRequest
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string NameVi { get; set; }
        public IFormFile Image { get; set; } 
      //  public List<IFormFile>Imgaee { get; set; }  
        public IFormFile Icon { get; set; }
        public Guid? ParentId { get; set; }
        public Nullable<int> levelcate { get; set; }
        public string Slug { get; set; }
    }

    public class CategoryTReee
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ParentId { get; set; }
        public int Level { get; set; } // Level property
        public List<Category> Children { get; set; } = new List<Category>();
    }
}
