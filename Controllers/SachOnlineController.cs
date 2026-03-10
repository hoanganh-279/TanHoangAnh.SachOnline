using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TanHoangAnh.SachOnline.Controllers
{
    public class SachOnlineController : Controller
    {
        // GET: SachOnline
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ChuDePartial()
        {
            return PartialView();
        }

        public ActionResult SachBanNhieuPartial()
        {
            return PartialView();
        }

        public ActionResult NavPartial()
        {
            return PartialView();
        }

        public ActionResult SliderPartial()
        {
            return PartialView();
        }

        public ActionResult NhaXuatBanPartial()
        {
            return PartialView();
        }

        public ActionResult FooterPartial()
        {
            return PartialView();
        }
    }
}