using HirolaMVC.Models;
using HirolaMVC.Models.Base;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel;
using System;

namespace HirolaMVC.Models
{
    public class Slide:BaseEntity
    {
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int Order { get; set; }
    }
}







