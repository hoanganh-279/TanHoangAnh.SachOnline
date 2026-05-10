using TanHoangAnh.SachOnline.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace TanHoangAnh.SachOnline.Areas.Admin.Controllers
{
    // ✅ [SỬA LỖI 1] Thay anonymous type bằng ViewModel riêng.
    // Anonymous type có scope 'internal' → Razor view không truy cập được
    // property → ném RuntimeBinderException khi render Dashboard.
    public class RecentOrderViewModel
    {
        public int MaDonHang { get; set; }
        public string TenKhachHang { get; set; }
        public decimal TongTien { get; set; }
        public int? TinhTrangGiaoHang { get; set; }
    }

    public class HomeController : Controller
    {
        SachOnlineEntities db = new SachOnlineEntities();

        // ✅ [SỬA LỖI 2] Hash mật khẩu SHA-256 thay vì so sánh plain-text.
        // ⚠ LƯU Ý: Cần migration dữ liệu DB trước khi deploy —
        //   chạy script C# để hash lại toàn bộ cột MatKhau đang lưu plain-text.
        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        [HttpGet]
        public ActionResult Login()
        {
            // Nếu đã đăng nhập, chuyển thẳng vào Dashboard
            if (Session["Admin"] != null)
                return RedirectToAction("Dashboard", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // ✅ [SỬA LỖI 3] Chống tấn công CSRF
        public ActionResult Login(FormCollection f)
        {
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
                // ✅ [SỬA LỖI 2] So sánh hash thay vì plain-text
                // ⚠ MIGRATION: Hỗ trợ cả hashed (mới) và plain-text (cũ) passwords
                var hashedPassword = HashPassword(sMatKhau);
                var ad = db.ADMINs.SingleOrDefault(n => n.TenDN == sTenDN);

                if (ad != null)
                {
                    // Thử hashed password trước (mới)
                    if (ad.MatKhau == hashedPassword)
                    {
                        Session["Admin"] = ad;
                        return RedirectToAction("Dashboard", "Home");
                    }
                    // Fallback: plain-text password (cũ) - cho phép migration
                    else if (ad.MatKhau == sMatKhau)
                    {
                        Session["Admin"] = ad;
                        // TODO: Log warning để admin biết cần hash password này
                        // Log: User logged in with plain-text password - should be migrated
                        return RedirectToAction("Dashboard", "Home");
                    }
                }

                ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng!";
            }
            return View();
        }

        public ActionResult Dashboard()
        {
            if (Session["Admin"] == null)
                return RedirectToAction("Login", "Home");

            ViewBag.TongDonHang = db.DONDATHANGs.Count();
            ViewBag.TongDoanhThu = db.CHITIETDATHANGs
                                     .Sum(c => (decimal?)(c.SoLuong ?? 0) * (c.DonGia ?? 0)) ?? 0;
            ViewBag.SoKhachHang = db.KHACHHANGs.Count();
            ViewBag.SoSach = db.SACHes.Count();

            // Lấy số lượng bán theo chủ đề
            var sachByCategory = (from s in db.SACHes
                                  join ct in db.CHITIETDATHANGs on s.MaSach equals ct.MaSach
                                  join cd in db.CHUDEs on s.MaCD equals cd.MaCD
                                  group ct by new { cd.MaCD, cd.TenChuDe } into g
                                  select new
                                  {
                                      TenChuDe = g.Key.TenChuDe,
                                      SoLuongBan = g.Sum(x => x.SoLuong)
                                  })
                                  .OrderByDescending(x => x.SoLuongBan)
                                  .ToList();

            var jsonSerializer = new JavaScriptSerializer();
            if (sachByCategory.Any())
            {
                ViewBag.SachByCategoryLabelsJson = jsonSerializer.Serialize(
                    sachByCategory.Select(x => x.TenChuDe).ToArray());
                ViewBag.SachByCategoryDataJson = jsonSerializer.Serialize(
                    sachByCategory.Select(x => x.SoLuongBan).ToArray());
            }
            else
            {
                ViewBag.SachByCategoryLabelsJson = "['Chưa có dữ liệu']";
                ViewBag.SachByCategoryDataJson = "[0]";
            }

            // Doanh thu 7 ngày gần nhất
            var today = DateTime.Today;
            // ✅ [SỬA LỖI 4] Xóa biến revenueByDay khai báo nhưng không dùng (dead code)
            var revenueLabels = new List<string>();
            var revenueData = new List<decimal>();

            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var dateNext = date.AddDays(1);

                var tongTienNgay = (from d in db.DONDATHANGs
                                    join ct in db.CHITIETDATHANGs on d.MaDonHang equals ct.MaDonHang
                                    where d.NgayDat >= date && d.NgayDat < dateNext
                                    select (decimal?)(ct.SoLuong ?? 0) * (ct.DonGia ?? 0))
                                   .Sum() ?? 0;

                revenueLabels.Add(date.ToString("dd/MM", CultureInfo.InvariantCulture));
                revenueData.Add(tongTienNgay);
            }

            ViewBag.RevenueLabelsJson = jsonSerializer.Serialize(revenueLabels);
            ViewBag.RevenueDataJson = jsonSerializer.Serialize(revenueData);

            // Lấy 5 khách hàng mới nhất
            ViewBag.RecentCustomers = db.KHACHHANGs
                .OrderByDescending(k => k.MaKH)
                .Take(5)
                .ToList();

            // ✅ [SỬA LỖI 1] Dùng RecentOrderViewModel thay vì anonymous type
            var recentOrdersRaw = db.DONDATHANGs
                .Include("KHACHHANG")
                .Include("CHITIETDATHANGs")
                .OrderByDescending(d => d.NgayDat)
                .Take(5)
                .ToList();

            ViewBag.RecentOrders = recentOrdersRaw.Select(d => new RecentOrderViewModel
            {
                MaDonHang = d.MaDonHang,
                TenKhachHang = d.KHACHHANG != null ? d.KHACHHANG.HoTen : "N/A",
                TongTien = d.CHITIETDATHANGs != null
                                        ? d.CHITIETDATHANGs.Sum(c => (c.SoLuong ?? 0) * (c.DonGia ?? 0))
                                        : 0,
                TinhTrangGiaoHang = d.TinhTrangGiaoHang
            }).ToList();

            return View();
        }

        // ✅ [SỬA LỖI 5] Kiểm tra session trước khi xóa, tránh thao tác thừa
        public ActionResult Logout()
        {
            if (Session["Admin"] != null)
                Session["Admin"] = null;
            return RedirectToAction("Login", "Home");
        }
    }
}