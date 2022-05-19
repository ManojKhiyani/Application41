using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BOS.StarterCode.Data
{
    public class Product
    {
        public Guid Id { get; set; }
        [Display(Name = "Product Name")]
        public string Name { get; set; }
        [Display(Name = "Retail Price")]
        public decimal MRP { get; set; }
        public int Quantity { get; set; }
    }
}
