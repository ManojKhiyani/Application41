using System;
using System.Collections.Generic;
using System.Linq;
using BOS.StarterCode.Models;
namespace BOS.StarterCode.Data
{
    public static class DbInitializer
    {
        public static void Initialize(Context context)
        {
            //if (!context.Product.Any())
            //{
            //    var person = new List<Product>
            //    {
            //        new Product { Id = Guid.Parse("79865406-e01b-422f-bd09-92e116a0664a"), Name = "Samsung", MRP = 100, Quantity = 10 },
            //        new Product { Id = Guid.Parse("d5674750-7f6b-43b9-b91b-d27b7ac13572"), Name = "Nokia", MRP = 100, Quantity = 20 },
            //        new Product { Id = Guid.Parse("e41446f9-c779-4ff6-b3e5-752a3dad97bb"), Name = "Motorola", MRP = 100, Quantity = 10 }
            //    };
            //    context.AddRange(person);
            //    context.SaveChanges();
            //}
        }
    }
}
