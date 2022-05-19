using BOS.StarterCode.Controllers;
using BOS.StarterCode.Models;
using BOS.StarterCode.Tests.HelperClass;
using BOS.StarterCode.Web.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BOS.StarterCode.Tests
{
    public class PasswordControllerTests
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly string bosAPIkey;
        private readonly SessionHelpers _sessionHelpers;
        private readonly MockHttpSession mockSession;
        private readonly TokenResponse tokenResponse;

        public PasswordControllerTests()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.test.json").Build();
            bosAPIkey = config["BOS:APIkey"];
            _configuration = config;
            _contextAccessor = new HttpContextAccessor();
            _sessionHelpers = new SessionHelpers();
            mockSession = new MockHttpSession();
            tokenResponse = new TokenResponse();
            tokenResponse.data = bosAPIkey;
            tokenResponse.message = "success";
            mockSession["ApplicationToken"] = JsonConvert.SerializeObject(tokenResponse);
            _contextAccessor.HttpContext = new DefaultHttpContext() { Session = mockSession };
        }

        [Fact]
        public async Task ResetPassword_redirects_to_error_view_when_passwordobj_is_null()
        {
            //Arrange
            var controller = new PasswordController(_configuration, _contextAccessor, _sessionHelpers);

            //Act
            var result = await controller.ResetPassword(null);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("ErrorPage", viewResult.ViewName); //Asserting that the returned view is "Error Page"
        }

        [Fact]
        public async Task ResetPassword_redirects_to_error_view_when_userId_is_null()
        {
            //Arrange
            var controller = new PasswordController(_configuration, _contextAccessor, _sessionHelpers);
            ChangePassword passwordObj = new ChangePassword
            {
                UserId = null,
                NewPassword = "password"
            };

            //Act
            var result = await controller.ResetPassword(passwordObj);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("ErrorPage", viewResult.ViewName); //Asserting that the returned view is "Error Page"
        }

        [Fact]
        public async Task ResetPassword_redirects_to_error_view_when_userId_is_incorrect()
        {
            //Arrange
            var controller = new PasswordController(_configuration, _contextAccessor, _sessionHelpers);
            ChangePassword passwordObj = new ChangePassword
            {
                UserId = Guid.NewGuid().ToString(),
                NewPassword = "password"
            };

            //Act
            var result = await controller.ResetPassword(passwordObj);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("ErrorPage", viewResult.ViewName); //Asserting that the returned view is "Error Page"
        }

        [Fact]
        public void GotBackToLogin_returns_login_view()
        {
            //Arrange
            var controller = new PasswordController(_configuration, _contextAccessor, _sessionHelpers);

            //Act
            var result = controller.GotBackToLogin();

            //Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result); //Asserting that the return is a View
            Assert.Equal("Auth", redirectResult.ControllerName); //Asserting that the returned Controller is "Auth"
            Assert.Equal("Index", redirectResult.ActionName); //Asserting that the Action Methond of the controller is "Index"
        }

        [Fact]
        public async Task Reset_redirects_to_error_view_when_slug_is_null()
        {
            //Arrange
            var controller = new PasswordController(_configuration, _contextAccessor, _sessionHelpers);

            //Act
            var result = await controller.Reset(null, null);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("ErrorPage", viewResult.ViewName); //Asserting that the returned view is "Error Page"
        }

        [Fact]
        public async Task Reset_redirects_to_error_view_when_slug_is_invalid()
        {
            //Arrange
            var controller = new PasswordController(_configuration, _contextAccessor, _sessionHelpers);

            //Act
            var result = await controller.Reset("random-slug-string", null);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("ResetPassword", viewResult.ViewName); //Asserting that the returned Controller is "ResetPassword"
            Assert.True(controller.ViewBag.Message != null); //
        }
    }
}
