using BOS.Auth.Client;
using BOS.Auth.Client.ClientModels;
using BOS.Email.Client;
using BOS.Email.Client.ClientModels;
using BOS.IA.Client;
using BOS.StarterCode.Helpers;
using BOS.StarterCode.Models;
using BOS.StarterCode.Models.BOSModels;
using BOS.StarterCode.Models.BOSModels.Permissions;
using BOS.StarterCode.Web.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace BOS.StarterCode.Controllers
{
    /// <summary>
    /// One of the most important controllers in the source code that deals with User Authentication and Authorization
    /// </summary>
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _contextAccessor;
        private Logger Logger;
        public AuthClient _bosAuthClient;
        public IAClient _bosIAClient;
        public EmailClient _bosEmailClient;
        private readonly SessionHelpers _sessionHelpers;
        public AuthController(IConfiguration configuration, IHttpContextAccessor contextAccessor, SessionHelpers sessionHelpers)
        {
            _configuration = configuration;
            _contextAccessor = contextAccessor;
            _sessionHelpers = sessionHelpers;
            SetAuthClient();
            SetIAClient();
            SetEmailClient();
            Logger = new Logger();
        }
        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Returns the "Login" view - The landing page of the application
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _sessionHelpers.GetGeneratedToken();
                //Check if user is authenticated then redirect him to clients
                if (User != null && User.Identity.IsAuthenticated)
                {
                    Guid UserId = UserId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "UserId").Value);
                    if (UserId != null)
                    {
                        var status = await SetModulePermissions(UserId);
                        if (status != "")
                        {
                            return RedirectToAction("SignOut", "Auth");
                        }
                        return RedirectToAction("Index", "Dashboard");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Auth", "Index", ex);
                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
            //If the user is not logged in, then navigate him to the "Login" Page
            return View("_Index");
        }
        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggers when the Login button is clicked
        /// </summary>
        /// <param name="authObj"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AuthenticateUser(AuthModel authObj)
        {
            try
            {
                /* Checking if the Cookie Concent has been accepted.
                 * If it is not, before logging-in, we ask the user to accept the terms.
                 * The reason for this is that, we are storing pieces of information in sessions through out the application
                 * In order to be confromant with the GDPR Laws, it is required that we get the constent from the user                  
                 */
                if (HttpContext != null && !HttpContext.Request.Cookies.ContainsKey(".AspNet.Consent"))
                {
                    ModelState.AddModelError("CustomError", "Before proceeding, please 'Accept' our Cookies' terms.");
                    return View("_Index", new AuthModel());
                }
                else if (authObj != null && authObj.Username != null && authObj.Password != null) //Checking if the authObj is null.
                {
                    if (_bosAuthClient == null)
                    {
                        ModelState.AddModelError("CustomError", "Username or Password is incorrect");
                        //return RedirectToAction("Index");
                        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); //Clears all the authentication data.
                        ClearSessionData();
                        return View("_Index", new AuthModel());
                    }
                    var result = await _bosAuthClient.SignInAsync(authObj.Username.Trim(), authObj.Password); //Making the call to the BOS Auth API to verify the user's login credentials
                    if (result != null && result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        ModelState.AddModelError("CustomError", "Username or Password is incorrect");
                        return RedirectToAction("SignOut", "Auth");
                    }
                    if (result.IsVerified)
                    {
                        /* ------- LOGIC ------
                         * First, check for non-null credentials sent as input paramters
                         * Make an API call to BOS Auth to verify the credentials
                         * On successful validation, get the user's roles, based on his Id
                         * After that, make another API to get the user's permissions-set. This API returns all the modules and operations that the user is permitted
                         * Finally, navigating him to the Dashboard
                         */
                        var userRoles = await _bosAuthClient.GetUserByIdWithRolesAsync<User>(result.UserId.Value); //On successful authentication, fetching the user's role
                        if (userRoles != null && userRoles.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            return RedirectToAction("SignOut", "Auth");
                        }
                        if (userRoles != null && userRoles.User != null && userRoles.User.Roles != null)
                        {
                            var user = userRoles.User;
                            var roles = user.Roles;
                            // Convert Roles Array into a comma separated string containing roles
                            string rolesString = string.Empty;
                            if (roles != null && roles.Count > 0)
                            {
                                foreach (UserRole userRole in roles)
                                {
                                    RoleUser role = userRole.Role;
                                    rolesString = (!string.IsNullOrEmpty(rolesString)) ? (rolesString + "," + role.Name) : (role.Name);
                                }
                            }
                            //Create Claims Identity. Saving all the information in the Claims object
                            var claims = new List<Claim>{ new Claim("CreatedOn", DateTime.UtcNow.ToString()),
                                              new Claim("Email", user.Email),
                                              new Claim("Initials", (!string.IsNullOrEmpty(user.FirstName) ?  user.FirstName[0].ToString() : "")
                                              + (!string.IsNullOrEmpty(user.LastName) ?  user.LastName[0].ToString() : "").ToUpper()),
                                              new Claim("Name", user.FirstName +" " + user.LastName),
                                              new Claim("Role", rolesString),
                                              new Claim("UserId", user.Id.ToString()),
                                              new Claim("Username", user.Username.ToString()),
                                              new Claim("IsAuthenticated", "True")
                                            };
                            var userIdentity = new ClaimsIdentity(claims, "Auth");
                            ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);
                            //Sign In created claims Identity Principal with cookie Authentication scheme
                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
                            {
                                ExpiresUtc = DateTime.UtcNow.AddMinutes(140),
                                IsPersistent = false,
                                AllowRefresh = false
                            });
                            var status = await SetModulePermissions(result.UserId.Value);
                            if (status != "")
                            {
                                return RedirectToAction("SignOut", "Auth");
                            }
                            return RedirectToAction("Index", "Dashboard"); //Finally, redirecting the user to the Dashboard page
                        }
                        else
                        {
                            ModelState.AddModelError("CustomError", "User Details Fetch Error");
                            return View("_Index", new AuthModel());
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("CustomError", "Username or Password is incorrect"); //Returning back to the Login page with an error message
                        return View("_Index", new AuthModel());
                    }
                }
                else
                {
                    ModelState.AddModelError("CustomError", "Username or Password is incorrect");
                    return View("_Index", new AuthModel());
                    //return RedirectToAction("Index");
                }
            }
            catch (ArgumentNullException ex)
            {
                Logger.LogException("Auth", "AuthenticateUser", ex);
                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
            catch (Exception ex)
            {
                Logger.LogException("Auth", "AuthenticateUser", ex);
                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }
        private async Task<string> SetModulePermissions(Guid UserId)
        {
            string status = "";
            if (HttpContext?.Session != null && HttpContext.Session.GetObject<List<Module>>("ModuleOperations") == null)
            {
                try
                {
                    if (UserId != null && UserId != Guid.Empty)
                    {
                        var enabledBOSapis = _configuration.GetSection("BOS:EnabledAPIs").Get<List<string>>();
                        if (enabledBOSapis != null)
                        {
                            if (enabledBOSapis.Contains("IA"))
                            {
                                if (_bosIAClient == null)
                                {
                                    SetIAClient();
                                }
                                //Getting the permissions
                                var permissionSet = await _bosIAClient.GetOwnerPermissionsSetsAsync<Permissions>(UserId); //Making BOS API call to fetch the user's permission
                                if (permissionSet?.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                {
                                    return status = "Token Expired";
                                }
                                if (permissionSet?.IsSuccessStatusCode == true)
                                {
                                    //Set Permissions in Sessions
                                    if (permissionSet?.Permissions?.Components != null)
                                    {
                                        var defaultSite = permissionSet.Permissions.Components.FirstOrDefault(x => x.Code == "DFLTS");
                                        if (defaultSite?.ChildComponents?.Count > 0)
                                        {
                                            var defaultModules = defaultSite.ChildComponents;
                                            if (defaultModules?.Count > 0)                         
                                                HttpContext.Session.SetObject("ModuleOperations", defaultModules); 
                                            else
                                                HttpContext.Session.SetObject("ModuleOperations", defaultSite.ChildComponents);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //Set Permissions in Sessions
                                HttpContext.Session.SetObject("ModuleOperations", SetModules()); //On success, saving it in the Session Object
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    status = ex.Message;
                }
            }
            return status;
        }
        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: When clicked on the 'Register' link on the page, navigates to the 'Register view
        /// </summary>
        /// <returns></returns>
        public ActionResult Register()
        {
            return View(); //Returning to the "Register" view
        }
        [HttpGet]
        public async Task<IActionResult> SetApplicationToken()
        {
            var response = await _sessionHelpers.GetGeneratedToken();
            SetAuthClient();
            SetIAClient();
            SetEmailClient();
            return Ok(); //Returning to the "Register" view
        }
        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggers when the Register button is clicked
        /// </summary>
        /// <param name="registerObj"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterUser(RegistrationModel registerObj)
        {
            try
            {
                if (HttpContext != null && !HttpContext.Request.Cookies.ContainsKey(".AspNet.Consent"))
                {
                    if (_bosAuthClient == null)
                    {
                        var response = await _sessionHelpers.GetGeneratedToken();
                        SetAuthClient();
                    }
                    ModelState.AddModelError("CustomError", "Before proceeding, please 'Accept' our Cookies' terms.");
                    return View("Register");
                }
                //Removing the whitespaces in the form-data
                registerObj.FirstName = HttpUtility.HtmlEncode(registerObj?.FirstName?.Trim());
                registerObj.LastName = HttpUtility.HtmlEncode(registerObj?.LastName?.Trim());
                registerObj.EmailAddress = HttpUtility.HtmlEncode(registerObj?.EmailAddress?.Trim());

                var password = CreatePassword();
                /* --------- LOGIC
                 * Make a call to the BOS Auth API to create a new user record
                 * Then extend the user's attributes with demographic information like FirstName and the like
                 * On success, set-up the user's role to the default "user" role
                 * After this, send an email to the user with a link to verify his email and setup a new password to the application
                 *       - Get the templatedID from BOS that will be used in the email
                 *       - Get the Service ProviderId that will be used to send the email
                 *       - Prepare the EmailObj that will be used to send the email
                 */
                var result = await _bosAuthClient.AddNewUserAsync<BOSUser>(registerObj.EmailAddress, registerObj.EmailAddress, password, registerObj.FirstName, registerObj.LastName); //Making the BOS API Call to add the user's record
                if (result != null)
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return RedirectToAction("SignOut", "Auth");
                    }
                    if (result.IsSuccessStatusCode)
                    {
                        List<Role> roleList = new List<Role>();
                        var availableRoles = await _bosAuthClient.GetRolesAsync<Role>();
                        if (availableRoles != null && availableRoles.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            return RedirectToAction("SignOut", "Auth");
                        }
                        if (availableRoles.IsSuccessStatusCode)
                        {
                            Role defaultRole = availableRoles.Roles.FirstOrDefault(i => i.Name == "User"); //Setting the registered user's role to the BOS default "User" role
                            roleList.Add(defaultRole);
                            var roleResponse = await _bosAuthClient.AssociateUserToMultipleRolesAsync(result.User.Id, roleList);
                            if (roleResponse != null && roleResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            {
                                return RedirectToAction("SignOut", "Auth");
                            }
                            if (roleResponse.IsSuccessStatusCode)
                            {
                                var slugResponse = await _bosAuthClient.CreateSlugAsync(registerObj.EmailAddress); //Creating a Slug that will be used in the verification process
                                if (slugResponse != null && slugResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                {
                                    return RedirectToAction("SignOut", "Auth");
                                }
                                if (slugResponse.IsSuccessStatusCode)
                                {
                                    var slug = slugResponse.Slug;
                                    //Preparing the Email object to send the registered user an email with verification link using BOS Email API
                                    Models.BOSModels.Email emailObj = new Models.BOSModels.Email
                                    {
                                        Deleted = false,
                                        From = new From
                                        {
                                            Email = "startercode@bosframework.com",
                                            Name = "StarterCode Team",
                                        },
                                        To = new List<To>
                                            {
                                                new To
                                                {
                                                    Email = registerObj.EmailAddress,
                                                    Name = registerObj.FirstName + " " + registerObj.LastName
                                                }
                                            }
                                    };
                                    var templateResponse = await _bosEmailClient.GetTemplateAsync<Template>();
                                    if (templateResponse != null && templateResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                    {
                                        return RedirectToAction("SignOut", "Auth");
                                    }
                                    if (templateResponse.IsSuccessStatusCode)
                                    {
                                        emailObj.TemplateId = templateResponse.Templates.Where(i => i.Name == "UserRegistration").Select(i => i.Id).ToList()[0];
                                    }
                                    else
                                    {
                                        ModelState.AddModelError("CustomError", "Sorry! We could not send you an email. Please try again later");
                                        return View("_Index");
                                    }
                                    var spResponse = await _bosEmailClient.GetServiceProviderAsync<ServiceProvider>(true);
                                    if (spResponse != null && spResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                    {
                                        return RedirectToAction("SignOut", "Auth");
                                    }
                                    if (spResponse.IsSuccessStatusCode)
                                    {
                                        emailObj.ServiceProviderId = spResponse.ServiceProvider[0].Id;
                                    }
                                    else
                                    {
                                        ModelState.AddModelError("CustomError", "Sorry! We could not send you an email. Please try again later");
                                        return View("_Index");
                                    }
                                    string hostUrl = _contextAccessor.HttpContext.Request.Host.ToString();
                                    string baseUrl = string.Format("{0}://{1}", hostUrl.Contains("localhost") ? "http" : "https", hostUrl);
                                    string logoUrl = baseUrl + "/images/logo.png";
                                    string appName = _configuration["ApplicationName"];
                                    var appConfigSession = _contextAccessor.HttpContext.Session.GetString("ApplicationConfig");
                                    if (appConfigSession != null)
                                    {
                                        var appconfig = JsonConvert.DeserializeObject<WhiteLabel>(appConfigSession);
                                        if (appconfig != null)
                                        {
                                            logoUrl = appconfig.Logo;
                                            appName = appconfig.Name;
                                        }
                                    }
                                    emailObj.Substitutions = new List<Substitution>();
                                    emailObj.Substitutions.Add(new Substitution { Key = "companyUrl", Value = baseUrl });
                                    emailObj.Substitutions.Add(new Substitution { Key = "companyLogo", Value = logoUrl });
                                    emailObj.Substitutions.Add(new Substitution { Key = "usersName", Value = registerObj.FirstName + " " + registerObj.LastName });
                                    emailObj.Substitutions.Add(new Substitution { Key = "applicationName", Value = appName });
                                    emailObj.Substitutions.Add(new Substitution { Key = "activationUrl", Value = baseUrl + "/Password/Reset?slug=" + slug.Value + "&set=true" });
                                    emailObj.Substitutions.Add(new Substitution { Key = "thanksCredits", Value = "Team StarterCode" });
                                    var emailResponse = await _bosEmailClient.SendEmailAsync<IEmail>(emailObj);
                                    if (emailResponse != null && emailResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                    {
                                        return RedirectToAction("SignOut", "Auth");
                                    }
                                    if (!emailResponse.IsSuccessStatusCode)
                                    {
                                        ModelState.AddModelError("CustomError", emailResponse.BOSErrors[0].Message);
                                    }
                                    ViewBag.Message = "Welcome! You have been successfully registered with us. Check you inbox for an activation link.";
                                    return View("_Index"); //On sucess, redirecting the user back to the Login Page
                                }
                            }
                        }
                        return View("Register");
                    }
                    else
                    {
                        ModelState.AddModelError("CustomError", result.BOSErrors[0].Message);
                        return View("Register");
                    }
                }
                else
                {
                    ModelState.AddModelError("CustomError", "Something went wrong. We are currently unable to register you. Please try again later.");
                    return View("Register");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Auth", "RegisterUser", ex);
                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }
        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: When clicked on the 'Register' link on the page, navigates to the 'Register view
        /// </summary>
        /// <returns></returns>
        public ActionResult ForgotPassword()
        {
            return View(); //Retuning the "ForgotPassword" view
        }
        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggers when the Register button is clicked
        /// </summary>
        /// <param name="forgotPasswordObj"></param>
        /// <returns></returns>
        public async Task<ActionResult> ForgotPasswordAction(ForgotPassword forgotPasswordObj)
        {
            try
            {
                if (HttpContext != null && !HttpContext.Request.Cookies.ContainsKey(".AspNet.Consent"))
                {
                    if (_bosAuthClient == null)
                    {
                        var response = await _sessionHelpers.GetGeneratedToken();
                    }
                    ModelState.AddModelError("CustomError", "Before proceeding, please 'Accept' our Cookies' terms.");
                    return View("ForgotPassword");
                }
                if (ModelState.IsValid)
                {
                    string emailAddress = forgotPasswordObj.EmailAddress.Trim(); //Trimming the email input
                    if (forgotPasswordObj != null)
                    {
                        if (_bosAuthClient == null)
                        {
                            var response = await _sessionHelpers.GetGeneratedToken();
                            return RedirectToAction("ForgotPassword");
                        }
                        var userResponse = await _bosAuthClient.GetUserByEmailAsync<BOSUser>(emailAddress); //Mkaing a call to the BOS API to validate the entered email address
                        if (userResponse != null && userResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            return RedirectToAction("SignOut", "Auth");
                        }
                        if (userResponse != null && userResponse.Users != null && userResponse.Users.Count > 0)
                        {
                            var slugResponse = await _bosAuthClient.CreateSlugAsync(emailAddress); //On success, creating a slug object that will be used while resetting the password
                            if (slugResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            {
                                return RedirectToAction("SignOut", "Auth");
                            }
                            if (slugResponse != null && slugResponse.IsSuccessStatusCode)
                            {
                                var slug = slugResponse.Slug;
                                //Creating the email object to send the email
                                Models.BOSModels.Email emailObj = new Models.BOSModels.Email
                                {
                                    Deleted = false,
                                    From = new From
                                    {
                                        Email = "startercode@bosframework.com",
                                        Name = "StarterCode Team",
                                    },
                                    To = new List<To>
                                    {
                                        new To
                                        {
                                            Email = emailAddress,
                                            Name = ""
                                        }
                                    }
                                };
                                var templateResponse = await _bosEmailClient.GetTemplateAsync<Template>();
                                if (templateResponse != null && templateResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                {
                                    return RedirectToAction("SignOut", "Auth");
                                }
                                if (templateResponse != null && templateResponse.IsSuccessStatusCode)
                                {
                                    emailObj.TemplateId = templateResponse.Templates.Where(i => i.Name == "ForgotPassword").Select(i => i.Id).ToList()[0];
                                }
                                else
                                {
                                    ModelState.AddModelError("CustomError", "Sorry! We could not send you an email. Please try again later");
                                    return View("_Index");
                                }
                                var spResponse = await _bosEmailClient.GetServiceProviderAsync<ServiceProvider>(true);
                                if (spResponse != null && spResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                {
                                    return RedirectToAction("SignOut", "Auth");
                                }
                                if (spResponse != null && spResponse.IsSuccessStatusCode)
                                {
                                    emailObj.ServiceProviderId = spResponse.ServiceProvider[0].Id;
                                }
                                else
                                {
                                    ModelState.AddModelError("CustomError", "Sorry! We could not send you an email. Please try again later");
                                    return View("_Index");
                                }
                                string hostUrl = _contextAccessor.HttpContext.Request.Host.ToString();
                                string baseUrl = string.Format("{0}://{1}", hostUrl.Contains("localhost") ? "http" : "https", hostUrl);
                                string logoUrl = baseUrl + "/images/logo.png";
                                string appName = _configuration["ApplicationName"];
                                var appConfigSession = _contextAccessor.HttpContext.Session.GetString("ApplicationConfig");
                                if (appConfigSession != null)
                                {
                                    var appconfig = JsonConvert.DeserializeObject<WhiteLabel>(appConfigSession);
                                    if (appconfig != null)
                                    {
                                        logoUrl = appconfig.Logo;
                                        appName = appconfig.Name;
                                    }
                                }
                                var userDetails = userResponse.Users.FirstOrDefault();
                                emailObj.Substitutions = new List<Substitution>();
                                emailObj.Substitutions.Add(new Substitution { Key = "companyUrl", Value = baseUrl });
                                emailObj.Substitutions.Add(new Substitution { Key = "companyLogo", Value = logoUrl });
                                emailObj.Substitutions.Add(new Substitution { Key = "usersName", Value = userDetails != null ? userDetails.Username.Split("@")[0] : "" });
                                emailObj.Substitutions.Add(new Substitution { Key = "applicationName", Value = appName });
                                emailObj.Substitutions.Add(new Substitution { Key = "resetUrl", Value = baseUrl + "/Password/Reset?slug=" + slug.Value + "&set=false" });
                                emailObj.Substitutions.Add(new Substitution { Key = "thanksCredits", Value = "Team StarterCode" });
                                var emailResponse = await _bosEmailClient.SendEmailAsync<IEmail>(emailObj);
                                if (emailResponse != null && emailResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                {
                                    return RedirectToAction("SignOut", "Auth");
                                }
                                if (!emailResponse.IsSuccessStatusCode)
                                {
                                    ModelState.AddModelError("CustomError", emailResponse.BOSErrors[0].Message);
                                    return View("_Index");
                                }
                            }
                        }
                    }
                    else
                    {
                    }
                }
                //Even if the email adrress entered is not a valid one, we show the same sucess message. This is a form of securing the user's information
                ViewBag.Message = "Check your inbox for an email with a link to reset your password.";
                return View("_Index");
            }
            catch (Exception ex)
            {
                Logger.LogException("Auth", "ForgotPasswordAction", ex);
                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }
        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggers after the user has inputted the email address on the View to recieve the email with a link to reset the password
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> ForcePasswordChange([FromBody] JObject data)
        {
            try
            {
                if (data != null)
                {
                    StringConversion stringConversion = new StringConversion();
                    string userId = stringConversion.DecryptString(data["userId"].ToString()); //Decrypting the userId sent from the View
                    string password = data["password"].ToString();
                    var response = await _bosAuthClient.ForcePasswordChangeAsync(Guid.Parse(userId), password); //Making an call to the BOS API to ForceChange the Password. This is done because at this point there is no way of knowing the user's original password
                    if (response != null && response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return "Token Expired, Please login again";
                    }
                    if (response != null && response.IsSuccessStatusCode)
                    {
                        return "Password updated successfully"; //On success, returing a message
                    }
                    else
                    {
                        Logger.LogException("Auth", "ForcePasswordChange", null);
                        return "Something went wrong. We are not able to change the password at this moment. Please try again later.";
                    }
                }
                else
                {
                    return "Data cannot be null";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Auth", "ForcePasswordChange", ex);
                return "Something went wrong. We are not able to change the password at this moment. Please try again later.";
            }
        }
        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Checks if the current session has expired
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public bool HasSessionExpired()
        {
            //An explicitally called to see if the sessuin is still active
            bool flag = false;
            if (HttpContext.Session == null || !HttpContext.Session.TryGetValue("ModuleOperations", out byte[] val))
            {
                flag = true;
            }
            return flag;
        }
        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Sign out
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> SignOut()
        {
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); //Clears all the authentication data.
                HttpContext.Session.Clear();
                ClearSessionData();
                return GoToReturnUrl("/Auth");
            }
            catch (Exception ex)
            {
                Logger.LogException("Auth", "SignOut", ex);
                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }
        private void ClearSessionData()
        {
            _contextAccessor.HttpContext.Session.Remove("ApplicationToken");
            HttpContext.Session.Remove("ModuleOperations");
        }
        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: To navigate the user back to the Login Page.
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        private IActionResult GoToReturnUrl(string returnUrl)
        {
            try
            {
                if (Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl); //Takes the user to the URL sent as the input paramter
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Auth", "GoToReturnUrl", ex);
                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
            return RedirectToAction("_Index");
        }
        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: A private method that randomly generates Password string during User Registration
        /// </summary>
        /// <returns></returns>
        private string CreatePassword()
        {
            //A private method to generate random passwords. This uses MS .Net Core's Identity reference
            PasswordOptions opts = new PasswordOptions()
            {
                RequiredLength = 10,
                RequiredUniqueChars = 4,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true
            };
            string[] randomChars = new[] {
                "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
                "abcdefghijkmnopqrstuvwxyz",    // lowercase
                "0123456789",                   // digits
                "!@$?_-"                        // non-alphanumeric
            };
            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();
            if (opts.RequireUppercase)
            {
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);
            }
            if (opts.RequireLowercase)
            {
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);
            }
            if (opts.RequireDigit)
            {
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);
            }
            if (opts.RequireNonAlphanumeric)
            {
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);
            }
            for (int i = chars.Count; i < opts.RequiredLength
                || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }
            return new string(chars.ToArray());
        }
        private List<Module> SetModules()
        {
            List<Module> modules = new List<Module>();
            var enabledBOSapis = _configuration.GetSection("BOS:EnabledAPIs").Get<List<string>>();
            if (enabledBOSapis != null)
            {
                if (enabledBOSapis.Contains("UserManagement"))
                {
                    Operation operation = new Operation
                    {
                        Name = "ALL",
                        Code = "ALL"
                    };
                    Module module = new Module
                    {
                        Code = "USERS",
                        Id = Guid.NewGuid(),
                        Name = "Users",
                        Operations = new List<IA.Client.ClientModels.IOperation>() { operation }
                    };
                    modules.Add(module);
                    module = new Module
                    {
                        Code = "MYPFL",
                        Id = Guid.NewGuid(),
                        Name = "My Profile",
                        Operations = new List<IA.Client.ClientModels.IOperation>() { operation }
                    };
                    modules.Add(module);
                }
            }
            return modules;
        }
        public ActionResult CheckTokenExpiry(string errorMessage)
        {
            if (errorMessage.Contains("Unexpected character encountered while parsing value: Y. Path"))//Token Expired
            {
                return RedirectToAction("SignOut", "Äuth");
            }
            return null;
        }
        public IActionResult SetAuthClient()
        {
            try
            {
                var token = "";
                try
                {
                    token = _contextAccessor.HttpContext.Session.GetString("ApplicationToken");
                }
                catch
                {
                    return null;
                }
                if (token != null)
                {
                    var tokenResult1 = JsonConvert.DeserializeObject<TokenResponse>(token);
                    if (tokenResult1 != null)
                    {
                        string bosServiceURL = _configuration["BOS:ServiceBaseURL"];
                        var client = new HttpClient();
                        client.BaseAddress = new Uri("" + bosServiceURL + _configuration["BOS:AuthRelativeURL"]);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult1.data);
                        client.DefaultRequestHeaders.Add("clientsecret", "" + _configuration["AppCredentials:ClientSecret"]);
                        _bosAuthClient = new AuthClient(client);
                        return null;
                    }
                    else
                    {
                        return RedirectToAction("SignOut", "Auth");
                    }
                }
                else
                {
                    return RedirectToAction("SignOut", "Auth");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public IActionResult SetIAClient()
        {
            try
            {
                var token = "";
                try
                {
                    token = _contextAccessor.HttpContext.Session.GetString("ApplicationToken");
                }
                catch
                {
                    return null;
                }
                if (token != null)
                {
                    var tokenResult1 = JsonConvert.DeserializeObject<TokenResponse>(token);
                    if (tokenResult1 != null)
                    {
                        string bosServiceURL = _configuration["BOS:ServiceBaseURL"];
                        var client = new HttpClient();
                        client.BaseAddress = new Uri("" + bosServiceURL + _configuration["BOS:IARelativeURL"]);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult1.data);
                        _bosIAClient = new IAClient(client);
                        return null;
                    }
                    else
                    {
                        return RedirectToAction("SignOut", "Auth");
                    }
                }
                else
                {
                    return RedirectToAction("SignOut", "Auth");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public IActionResult SetEmailClient()
        {
            try
            {
                var token = "";
                try
                {
                    token = _contextAccessor.HttpContext.Session.GetString("ApplicationToken");
                }
                catch
                {
                    return null;
                }
                if (token != null)
                {
                    var tokenResult1 = JsonConvert.DeserializeObject<TokenResponse>(token);
                    if (tokenResult1 != null)
                    {
                        string bosServiceURL = _configuration["BOS:ServiceBaseURL"];
                        var client = new HttpClient();
                        client.BaseAddress = new Uri("" + bosServiceURL + _configuration["BOS:EmailRelativeURL"]);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult1.data);
                        _bosEmailClient = new EmailClient(client);
                        return null;
                    }
                    else
                    {
                        return RedirectToAction("SignOut", "Auth");
                    }
                }
                else
                {
                    return RedirectToAction("SignOut", "Auth");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}