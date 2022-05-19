using BOS.StarterCode.Models;
using BOS.StarterCode.Web.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace BOS.StarterCode.Data
{

    public class Context : DbContext
    {
        //Entities
        //public DbSet<Product> Product { get; set; }

        public Context(DbContextOptions<Context> options) : base(options)
        {
        }

        //***If your Application is Multitenant Application, please comment the above context constructor and uncomment the below lines 22 to 54***
        //private readonly IConfiguration _configuration;
        //private readonly IHttpContextAccessor _contextAccessor;
        //private TokenResponse _tenant;
        //private ILogger<Context> _logger;
        //private readonly SessionHelpers _sessionHelpers;
        //public Context(IConfiguration configuration,
        //    SessionHelpers sessionHelpers,
        //    IHttpContextAccessor contextAccessor,
        //    ILogger<Context> logger)
        //{
        //    _configuration = configuration;
        //    _contextAccessor = contextAccessor;
        //    _sessionHelpers = sessionHelpers;
        //    _tenant = _sessionHelpers.GetGeneratedToken().Result;
        //    //Note: if you are using code first approach to run migration scripts/creating Scaffolding templates to generate views,
        //    //comment above one line(API call) and un-comment below 2 lines and paste your db connection string(Tenant DB connection string)
        //    //_tenant = new TokenResponse();
        //    //_tenant.ConnectionString = "Server=dvlp1976999db.cfwozs7ywm6n.us-east-1.rds.amazonaws.com;User Id=mysql_admin;Password=paaQaqc#bq9*Rzzmk9Q;Database=dvlp1976999";
        //    _logger = logger;
        //}

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!string.IsNullOrEmpty(_tenant?.ConnectionString))
        //    {
        //        optionsBuilder.UseMySql(_tenant.ConnectionString);
        //        base.OnConfiguring(optionsBuilder);
        //    }
        //}
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);
        //}
    }
}
