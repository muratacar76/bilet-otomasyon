using Microsoft.AspNetCore.Mvc;

namespace FlightBooking.Web.Controllers
{
    public class LegalController : Controller
    {
        [HttpGet]
        public IActionResult Privacy()
        {
            ViewData["Title"] = "Gizlilik Politikası - BULUTBİLET.COM";
            return View();
        }

        [HttpGet]
        public IActionResult Terms()
        {
            ViewData["Title"] = "Kullanım Şartları - BULUTBİLET.COM";
            return View();
        }

        [HttpGet]
        public IActionResult Cookies()
        {
            ViewData["Title"] = "Çerez Politikası - BULUTBİLET.COM";
            return View();
        }

        [HttpGet]
        public IActionResult KVKK()
        {
            ViewData["Title"] = "KVKK Aydınlatma Metni - BULUTBİLET.COM";
            return View();
        }
    }
}