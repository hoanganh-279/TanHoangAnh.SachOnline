using TanHoangAnh.SachOnline.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Data.Entity; // Nếu dùng Cách 2

namespace TanHoangAnh.SachOnline.Controllers
{
    public class HomeController : Controller
    {
        SachOnlineEntities db = new SachOnlineEntities();

        public ActionResult Dashboard()
        {
            ViewBag.ActiveMenu = "Dashboard";

            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            ViewBag.TongDonHang = db.DONDATHANGs.Count();
            ViewBag.TongDoanhThu = db.CHITIETDATHANGs.Sum(c => (decimal?)(c.SoLuong * c.DonGia)) ?? 0;
            ViewBag.SoKhachHang = db.KHACHHANGs.Count();
            ViewBag.SoSach = db.SACHes.Count();

            ViewBag.RevenueChartData = GetRevenueLast14Days();
            ViewBag.TopicChartData = GetBooksByTopic();

            return View();
        }

        private List<ChartPoint> GetRevenueLast14Days()
        {
            var today = DateTime.Today;
            var result = new List<ChartPoint>();

            for (int i = 13; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var nextDay = date.AddDays(1);

                var revenue = db.DONDATHANGs
                    .Where(d => d.NgayDat.HasValue
                        && d.NgayDat.Value >= date
                        && d.NgayDat.Value < nextDay
                        && d.DaThanhToan == true)
                    .Join(db.CHITIETDATHANGs,
                        d => d.MaDonHang,
                        c => c.MaDonHang,
                        (d, c) => c)
                    .Sum(c => (decimal?)(c.SoLuong * c.DonGia)) ?? 0;

                result.Add(new ChartPoint
                {
                    Label = date.ToString("dd/MM"),
                    Value = (double)revenue
                });
            }
            return result;
        }

        private List<ChartPoint> GetBooksByTopic()
        {
            return db.CHUDEs
                .Select(cd => new ChartPoint
                {
                    Label = cd.TenChuDe,
                    Value = cd.SACHes.Count()
                })
                .OrderByDescending(x => x.Value)
                .Take(5)
                .ToList();
        }

        [HttpGet]
        public ActionResult Login()
        {
            ViewBag.ActiveMenu = "Dashboard";
            return View();
        }

        [HttpPost]
        public ActionResult Login(FormCollection f)
        {
            ViewBag.ActiveMenu = "Dashboard";
            var sTenDN = f["UserName"];
            var sMatKhau = f["Password"];

            if (string.IsNullOrEmpty(sTenDN))
            {
                ViewBag.ThongBao = "Vui lòng nhập tên đăng nhập!";
            }
            else if (string.IsNullOrEmpty(sMatKhau))
            {
                ViewBag.ThongBao = "Vui lòng nhập mật khẩu!";
            }
            else
            {
                var ad = db.ADMINs.SingleOrDefault(n => n.TenDN == sTenDN && n.MatKhau == sMatKhau);
                if (ad != null)
                {
                    Session["Admin"] = ad;
                    return RedirectToAction("Dashboard", "Home");
                }
                else
                {
                    ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng!";
                }
            }
            return View();
        }

        public ActionResult Logout()
        {
            ViewBag.ActiveMenu = "Dashboard";
            Session["Admin"] = null;
            return RedirectToAction("Login", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class ChartPoint
    {
        public string Label { get; set; }
        public double Value { get; set; }
    }
}