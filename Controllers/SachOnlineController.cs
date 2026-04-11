using Microsoft.Ajax.Utilities;
using PagedList;
using PagedList.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using TanHoangAnh.SachOnline.Models;

namespace TanHoangAnh.SachOnline.Controllers
{
    public class SachOnlineController : Controller
    {

        TanHoangAnh.SachOnline.Models.SachOnlineEntities db = new TanHoangAnh.SachOnline.Models.SachOnlineEntities();
        // GET: SachOnline

        public ActionResult ChuDePartial()
        {
            var listChuDe = db.CHUDEs.ToList();
            return PartialView(listChuDe);
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
            var listNXB = db.NHAXUATBANs.ToList();
            return PartialView(listNXB);
        }

        public ActionResult FooterPartial()
        {
            return PartialView();
        }

        private List<TanHoangAnh.SachOnline.Models.SACH> LaySachMoi (int count)
        {
            return db.SACHes.OrderByDescending(a => a.NgayCapNhat).Take(count).ToList();
        }

        public ActionResult Index(int page = 1)
        {
            int size = 6;
            var listSachMoi = db.SACHes.OrderByDescending(a => a.NgayCapNhat).ToList();
            return View(listSachMoi.ToPagedList(page, size));
        }

        private List<TanHoangAnh.SachOnline.Models.SACH> LaySachBanNhieu(int count)
        {
            return db.SACHes.OrderByDescending(a => a.SoLuongBan).Take(count).ToList();
        }

        public ActionResult SachBanNhieuPartial()
        {
            var listSachBanNhieu = LaySachBanNhieu(6);

            return PartialView(listSachBanNhieu);
        }

        public ActionResult SachTheoChuDe(int id, int page = 1)
        {
            int size = 2;
            ViewBag.TenCD = db.CHUDEs.Where(cd => cd.MaCD == id).SingleOrDefault().TenChuDe;
            ViewBag.MaCD = id;
            var kq = (from s in db.SACHes where s.MaNXB == id select s).ToList();

            return View(kq.ToPagedList(page, size));
        }

        public ActionResult SachTheoNhaXuatBan(int id, int page = 1)
        {
            int size = 3;
            ViewBag.MaNXB = id;
            var kq = db.SACHes.Where(s => s.MaNXB == id).ToList();
            return View(kq.ToPagedList(page, size));
        }

        public ActionResult ChiTietSach(int id)
        {
            var sach = from s in db.SACHes where s.MaSach == id select s;
            return View(sach.Single());
        }

        public ActionResult LoginLogout()
        {
            return PartialView("LoginLogoutPartial");
        }
    }

}