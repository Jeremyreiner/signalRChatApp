using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace signalR.Controllers
{
    public class FallBackController : Controller
    {
        public ActionResult Index()
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html");

            return PhysicalFile(file, "text/HTML");
        }
    }
}
