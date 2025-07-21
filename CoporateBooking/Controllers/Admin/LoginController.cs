using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using DomainLayer.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OnionConsumeWebAPI.Extensions;
using ServiceLayer.Service.Interface;
using static System.Net.WebRequestMethods;

namespace OnionConsumeWebAPI.Controllers.Admin
{


    public class LoginController : Controller
    {

        //public IActionResult Index()
        //{
        //    return View();
        //}
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoginController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<IActionResult> UserLogin(string returnUrl = null)
        {

            if (returnUrl == null)
            {
                HttpContext.Session.Clear();
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();

        }

        [HttpPost]
        public async Task<IActionResult> UserLogin(string username, string password, string returnUrl = null)
        {
            using (HttpClient client = new HttpClient())
            {
                var loginRequest = new { businessEmail = username, Password = password };
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(loginRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(AppUrlConstant.Corporatelogin, content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    JObject jsonResult = JObject.Parse(result);
                    // var email = username; // or "email" if that's the correct key

                    // ✅ Save to session
                    // HttpContext.Session.SetString("LoggedInEmail", email);
                    //new Claim(ClaimTypes.Authentication, password)

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, Convert.ToString(jsonResult["userID"])),
                        new Claim(ClaimTypes.Name, Convert.ToString(jsonResult["firstName"]) + " " + Convert.ToString(jsonResult["lastName"])),
                        new Claim(ClaimTypes.Email, Convert.ToString(jsonResult["businessEmail"])),
                        new Claim("UserType", Convert.ToString(jsonResult["userType"]))
                    };

                    var identity = new ClaimsIdentity(claims, "Password");
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);



                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        // TempData["Email"] = email;
                        return RedirectToAction("Index", "FlightSearchIndex");
                        // return Redirect(returnUrl);
                    }
                    else
                    {
                        // TempData["Email"] = email;
                        return RedirectToAction("Index", "FlightSearchIndex");


                    }
                    //return RedirectToAction("SearchResultFlight", "FlightSearchIndex", new { email = email });


                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid login credentials";
                }
                return View();
            }



        }

        public IActionResult Dashboard()
        {
            return View();
        }
        public async Task<IActionResult> Logout()
        {

            // Sign out of the cookie authentication scheme
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Clear session data
            HttpContext.Session.Clear();

            // Optionally: Clear request cookies as well (e.g., auth cookie)
            Response.Cookies.Delete(".AspNetCore.Cookies");

            // Redirect to login page
            return RedirectToAction("UserLogin", "Login");
        }

        [HttpGet]
        public async Task<JsonResult> GetEmpId(string legalEntityCode, string employeeCode)
        {
            bool isAdmin = false;
            //TODO: Check the user if it is admin or normal user, (true-Admin, false- Normal user)
            string output = isAdmin ? "Welcome to the Admin User" : "Welcome to the User";

            string apiUrl = $"{AppUrlConstant.GetBillingEntity}?legalEntityCode={legalEntityCode}&employeeCode={employeeCode}";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();

                    // Optional: Deserialize JSON if you have a model
                    // var customers = JsonConvert.DeserializeObject<List<CustomerDetails>>(jsonData);
                    return Json(jsonData);
                }

            }

            return Json(output);
        }


    }
}
