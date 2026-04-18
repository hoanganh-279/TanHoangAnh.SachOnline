using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TanHoangAnh.SachOnline.Models;

namespace TanHoangAnh.SachOnline.Controllers
{
    public class UserController : Controller
    {
        TanHoangAnh.SachOnline.Models.SachOnlineEntities db = new TanHoangAnh.SachOnline.Models.SachOnlineEntities();

        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        // Đăng ký
        [HttpGet]
        public ActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DangKy(FormCollection collection, KHACHHANG kh)
        {
            var sHoTen = collection["HoTen"];
            var sTenDN = collection["TenDN"];
            var sMatKhau = collection["MatKhau"];
            var sMatKhauNhapLai = collection["MatKhauNL"];
            var sDiaChi = collection["DiaChi"];
            var sEmail = collection["Email"];
            var sDienThoai = collection["DienThoai"];
            var sNgaySinh = String.Format("{0:MM/dd/yyyy}", collection["NgaySinh"]);

            if (String.IsNullOrEmpty(sHoTen))
                ViewData["err1"] = "Họ tên không được rỗng";
            else if (String.IsNullOrEmpty(sTenDN))
                ViewData["err2"] = "Tên đăng nhập không được rỗng";
            else if (String.IsNullOrEmpty(sMatKhau))
                ViewData["err3"] = "Phải nhập mật khẩu";
            else if (sMatKhau != sMatKhauNhapLai)
                ViewData["err4"] = "Mật khẩu nhập lại không khớp";
            else if (String.IsNullOrEmpty(sEmail))
                ViewData["err5"] = "Email không được rỗng";
            else if (String.IsNullOrEmpty(sDienThoai))
                ViewData["err6"] = "Số điện thoại không được rỗng";
            else if (db.KHACHHANGs.SingleOrDefault(n => n.TaiKhoan == sTenDN) != null)
                ViewBag.ThongBao = "Tên đăng nhập đã tồn tại";
            else if (db.KHACHHANGs.SingleOrDefault(n => n.Email == sEmail) != null)
                ViewBag.ThongBao = "Email đã được sử dụng";
            else
            {
                kh.HoTen = sHoTen;
                kh.TaiKhoan = sTenDN;
                kh.MatKhau = sMatKhau;
                kh.Email = sEmail;
                kh.DiaChi = sDiaChi;
                kh.DienThoai = sDienThoai;
                kh.NgaySinh = DateTime.Parse(sNgaySinh);
                db.KHACHHANGs.Add(kh);
                db.SaveChanges();
                return RedirectToAction("DangNhap");
            }
            return View();
        }

        // Đăng nhập
        // Nhận thêm tham số url để sau khi đăng nhập quay lại đúng trang
        [HttpGet]
        public ActionResult DangNhap(string url)
        {
            ViewBag.url = url; // Lưu url vào ViewBag để truyền vào form
            return View();
        }

        [HttpPost]
        public ActionResult DangNhap(FormCollection collection)
        {
            var sTenDN = collection["TenDN"];
            var sMatKhau = collection["MatKhau"];
            // Lấy url được truyền vào từ hidden field trong form
            var url = collection["url"];

            if (String.IsNullOrEmpty(sTenDN))
            {
                ViewData["Err1"] = "Bạn chưa nhập tên đăng nhập";
            }
            else if (String.IsNullOrEmpty(sMatKhau))
            {
                ViewData["Err2"] = "Phải nhập mật khẩu";
            }
            else
            {
                KHACHHANG kh = db.KHACHHANGs
                    .SingleOrDefault(n => n.TaiKhoan == sTenDN && n.MatKhau == sMatKhau);

                if (kh != null)
                {
                    Session["TaiKhoan"] = kh;
                    Session["TenDN"] = kh.TaiKhoan;
                    Session["MatKhau"] = kh.MatKhau;

                    // Nếu có url thì quay lại trang đó, không thì về trang chủ
                    if (!string.IsNullOrEmpty(url))
                        return Redirect(url);

                    return RedirectToAction("Index", "SachOnline");
                }
                else
                {
                    ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng";
                    ViewBag.url = url; // Giữ lại url khi đăng nhập thất bại
                }
            }

            return View();
        }

        public ActionResult DangXuat()
        {
            Session.Clear();
            return RedirectToAction("Index", "SachOnline");
        }
    }
}