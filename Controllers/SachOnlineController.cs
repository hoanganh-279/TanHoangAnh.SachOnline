using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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

        public ActionResult Index()
        {
            var listSachMoi = LaySachMoi(6);
            return View(listSachMoi);
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

        public ActionResult SachTheoChuDe(int id)
        {
            ViewBag.TenChuDe = (from cd in db.CHUDEs where cd.MaCD == id select cd.TenChuDe).FirstOrDefault();
            var kq = (from s in db.SACHes where s.MaCD == id select s).ToList();
            return View(kq);
        }

        public ActionResult SachTheoNhaXuatBan(int id)
        {

            var kq = (from s in db.SACHes where s.MaNXB == id select s).ToList();
            return View(kq);
        }

        public ActionResult ChiTietSach(int id)
        {
            var sach = from s in db.SACHes where s.MaSach == id select s;
            return View(sach.Single());
        }
    }

}