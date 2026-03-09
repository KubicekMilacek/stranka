using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult kontakt()
        {
            return View();
        }

        public IActionResult Menu()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Registrace()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registrace(string jmeno, string heslo, string hesloZnovu)
        {
            if (string.IsNullOrWhiteSpace(jmeno) || string.IsNullOrWhiteSpace(heslo))
            {
                ViewBag.Chyba = "Vyplňte jméno a heslo.";
                return View();
            }

            if (heslo != hesloZnovu)
            {
                ViewBag.Chyba = "Hesla se neshodují.";
                return View();
            }

            if (await _db.Uzivatele.AnyAsync(u => u.Jmeno == jmeno))
            {
                ViewBag.Chyba = "Uživatel s tímto jménem již existuje.";
                return View();
            }

            _db.Uzivatele.Add(new Uzivatel { Jmeno = jmeno, Heslo = heslo });
            await _db.SaveChangesAsync();

            TempData["Uspech"] = "Registrace proběhla úspěšně! Nyní se můžete přihlásit.";
            return RedirectToAction("Prihlaseni");
        }

        [HttpGet]
        public IActionResult Prihlaseni()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Prihlaseni(string jmeno, string heslo)
        {
            if (string.IsNullOrWhiteSpace(jmeno) || string.IsNullOrWhiteSpace(heslo))
            {
                ViewBag.Chyba = "Vyplňte jméno a heslo.";
                return View();
            }

            var uzivatel = await _db.Uzivatele.FirstOrDefaultAsync(u => u.Jmeno == jmeno && u.Heslo == heslo);
            if (uzivatel == null)
            {
                ViewBag.Chyba = "Špatné jméno nebo heslo.";
                return View();
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, uzivatel.Jmeno)
            };
            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("Cookies", principal);

            return RedirectToAction("MujUcet");
        }

        public IActionResult MujUcet()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToAction("Prihlaseni");
            }

            return View();
        }

        public async Task<IActionResult> Odhlasit()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
