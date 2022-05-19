using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using BOS.StarterCode.Data;
using BOS.StarterCode.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BOS.StarterCode.Controllers
{
    public class ProductController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _contextAccessor;
        private Logger Logger;
        public ProductController(IConfiguration configuration, IHttpContextAccessor contextAccessor)
        {
            _configuration = configuration;
            _contextAccessor = contextAccessor;
            Logger = new Logger();
        }

        // GET: Product
        //public ActionResult Index()
        //{
        //    try
        //    {
        //        List<Product> productList = new List<Product>();
        //        // Seed data
        //        //DbInitializer.Initialize(DbContext);
        //        productList = DbContext.Product.ToList();
        //        return View(productList);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogException("Product", "Index", ex);
        //        dynamic model = new ExpandoObject();
        //        model.Message = ex.Message;
        //        model.StackTrace = ex.StackTrace;
        //        return View("ErrorPage", model);
        //    }
        //}
    }
}