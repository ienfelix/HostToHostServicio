using Comun;
using System.Web.Mvc;

namespace HostToHostServicio.Controllers
{
    public class ValuesController : Controller
    {
        [HttpGet]
        public JsonResult Index()
        {
            return Json(Constante.MENSAJE_SERVICIO_OK, JsonRequestBehavior.AllowGet);
        }
    }
}
