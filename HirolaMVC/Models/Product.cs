﻿using HirolaMVC.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace HirolaMVC.Models
{
    public class Product:BaseEntity
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string ProductCode { get; set; }
        public bool ISAvailable { get; set; }
        //relational
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public int BrandId { get; set; }
        public Brand Brand { get; set; }
        public List<ProductImage> ProductImages { get; set; }
        public List<ProductTag> ProductTags { get; set; }
        public List<ProductColor> ProductColors { get; set; }
      
    }
}
