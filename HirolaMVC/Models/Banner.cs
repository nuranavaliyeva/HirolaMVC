﻿using HirolaMVC.Models.Base;

namespace HirolaMVC.Models
{
    public class Banner:BaseEntity
    {
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int Order {  get; set; }
        
    }
}
