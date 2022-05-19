using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BOS.StarterCode.Data;
using Microsoft.AspNetCore.Mvc;

namespace BOS.StarterCode.Controllers
{
    public class BaseController : Controller
    {
        protected Context DbContext => (Context)HttpContext.RequestServices.GetService(typeof(Context));
    }
}