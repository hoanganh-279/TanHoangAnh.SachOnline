using Microsoft.Ajax.Utilities;
using PagedList;
using PagedList.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using TanHoangAnh.SachOnline.Helpers;
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

        public ActionResult NavPartial(string activeMenu = "")
        {
            ViewBag.ActiveMenu = activeMenu;
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
            ViewBag.ActiveMenu = "Index";
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
            int size = 6;
            var chuDe = db.CHUDEs.SingleOrDefault(cd => cd.MaCD == id);
            if (chuDe == null)
                return HttpNotFound();

            ViewBag.TenChuDe = chuDe.TenChuDe;
            ViewBag.MaCD = id;
            var kq = db.SACHes.Where(s => s.MaCD == id).OrderByDescending(s => s.NgayCapNhat).ToList();

            return View(kq.ToPagedList(page, size));
        }

        public ActionResult SachTheoNhaXuatBan(int id, int page = 1)
        {
            int size = 6;
            var nxb = db.NHAXUATBANs.SingleOrDefault(n => n.MaNXB == id);
            if (nxb == null)
                return HttpNotFound();

            ViewBag.TenNXB = nxb.TenNXB;
            ViewBag.MaNXB = id;
            var kq = db.SACHes.Where(s => s.MaNXB == id).OrderByDescending(s => s.NgayCapNhat).ToList();
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

        public ActionResult GioiThieu()
        {
            ViewBag.ActiveMenu = "GioiThieu";
            ViewBag.TongSach = db.SACHes.Count();
            ViewBag.TongChuDe = db.CHUDEs.Count();
            ViewBag.TongNXB = db.NHAXUATBANs.Count();
            return View();
        }

        public ActionResult LienHe()
        {
            ViewBag.ActiveMenu = "LienHe";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LienHe(FormCollection f)
        {
            ViewBag.ActiveMenu = "LienHe";
            var hoTen = (f["HoTen"] ?? "").Trim();
            var email = (f["Email"] ?? "").Trim();
            var dienThoai = (f["DienThoai"] ?? "").Trim();
            var chuDe = (f["ChuDe"] ?? "").Trim();
            var noiDung = (f["NoiDung"] ?? "").Trim();

            if (string.IsNullOrEmpty(hoTen))
                ViewBag.Loi = "Vui lòng nhập họ tên.";
            else if (string.IsNullOrEmpty(email))
                ViewBag.Loi = "Vui lòng nhập email.";
            else if (string.IsNullOrEmpty(noiDung))
                ViewBag.Loi = "Vui lòng nhập nội dung liên hệ.";
            else if (!MailHelper.GuiLienHe(hoTen, email, dienThoai, chuDe, noiDung, out string loiGui))
                ViewBag.Loi = loiGui;
            else
            {
                ViewBag.ThanhCong = true;
                ViewBag.HoTen = hoTen;
            }

            return View();
        }
    }

}