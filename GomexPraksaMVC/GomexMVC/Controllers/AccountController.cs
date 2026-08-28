using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;
using GomexPraksaMVC.Models;

namespace GomexPraksaMVC.GomexMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;

        public AccountController(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            var model = new LoginViewModel { ReturnUrl = returnUrl };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpFactory.CreateClient("GomexApi");

            var loginPayload = new
            {
                Email = model.UserName,
                Passsword = model.Password
            };

            var sw = System.Diagnostics.Stopwatch.StartNew();
            Console.WriteLine($"[MVC] Pre poziva ka API-ju: {DateTime.Now:HH:mm:ss.fff}");

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsJsonAsync("api/auth/login", loginPayload);
                Console.WriteLine($"[MVC] Posle poziva ka API-ju: {sw.ElapsedMilliseconds} ms");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MVC] IZUZETAK posle {sw.ElapsedMilliseconds} ms: {ex.GetType().Name} - {ex.Message}");
                ModelState.AddModelError(string.Empty, "Server trenutno nije dostupan.");
                return View(model);
            }        

        var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var token = doc.RootElement.GetProperty("token").GetString();

            if (string.IsNullOrWhiteSpace(token))
            {
                ModelState.AddModelError(string.Empty, "Prijava nije uspela.");
                return View(model);
            }

            HttpContext.Session.SetString("auth_token", token);
            HttpContext.Session.SetString("user_name", model.UserName);

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}