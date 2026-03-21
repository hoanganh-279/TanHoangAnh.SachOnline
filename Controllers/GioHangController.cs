using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TanHoangAnh.SachOnline.Models;

namespace TanHoangAnh.SachOnline.Controllers
{
    public class GioHangController : Controller
    {
        SachOnlineEntities db = new SachOnlineEntities();

        // GET: GioHang
        public ActionResult GioHangPartial()
        {
            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongTien = TongTien();
            return PartialView();
        }

        //Lấy giỏ hàng
        public List<GioHang> LayGioHang()
        {
            List<GioHang> listGioHang = Session["GioHang"] as List<GioHang>;
            if (listGioHang == null)
            {
                listGioHang = new List<GioHang>();
                Session["GioHang"] = listGioHang;
            }
            return listGioHang;
        }

        //Thêm giỏ hàng
        public ActionResult ThemGioHang(int id, string url)
        {
            List<GioHang> listCart = LayGioHang();
            GioHang sp = listCart.Find(n => n.iMaSach == id);

            if (sp == null)
            {
                sp = new GioHang(id);
                listCart.Add(sp);
            }
            else
            {
                sp.iSoLuong++;
            }

            return Redirect(url);
        }

        //Tính tổng số lượng
        private int TongSoLuong()
        {
            int iTongSoLuong = 0;
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;

            if (lstGioHang != null)
            {
                iTongSoLuong = lstGioHang.Sum(n => n.iSoLuong);
            }

            return iTongSoLuong;
        }

        //Tính tổng tiền
        private double TongTien()
        {
            double dTongTien = 0;
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;

            if (lstGioHang != null)
            {
                dTongTien = lstGioHang.Sum(n => n.dThanhTien);
            }

            return dTongTien;
        }

        //Hiển thị giỏ hàng
        public ActionResult GioHang()
        {
            List<GioHang> lstGioHang = LayGioHang();

            if (lstGioHang.Count == 0)
            {
                return RedirectToAction("Index", "SachOnline");
            }

            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongTien = TongTien();

            return View(lstGioHang);
        }

        /* ==========================================================
           CÁC HÀM BỔ SUNG ĐỂ XỬ LÝ NÚT BẤM TRONG TRANG GIỎ HÀNG 
           ========================================================== */

        // 1. Xóa một sản phẩm khỏi giỏ hàng
        public ActionResult XoaGioHang(int iMaSP)
        {
            List<GioHang> lstGioHang = LayGioHang();
            // Kiểm tra xem sách này có trong giỏ không
            GioHang sp = lstGioHang.SingleOrDefault(n => n.iMaSach == iMaSP);
            if (sp != null)
            {
                lstGioHang.RemoveAll(n => n.iMaSach == iMaSP);
                // Nếu xóa xong mà giỏ hàng rỗng thì quay về trang chủ
                if (lstGioHang.Count == 0)
                {
                    return RedirectToAction("Index", "SachOnline");
                }
            }
            return RedirectToAction("GioHang");
        }

        // 2. Cập nhật số lượng sản phẩm
        [HttpPost]
        public ActionResult CapnhatGiohang(int iMaSP, FormCollection f)
        {
            List<GioHang> lstGioHang = LayGioHang();
            GioHang sp = lstGioHang.SingleOrDefault(n => n.iMaSach == iMaSP);
            if (sp != null)
            {
                // Lấy số lượng mới từ thẻ input name="txtSoLuong" bên View
                sp.iSoLuong = int.Parse(f["txtSoLuong"].ToString());
            }
            return RedirectToAction("GioHang");
        }

        // 3. Xóa toàn bộ giỏ hàng
        public ActionResult XoaTatCaGioHang()
        {
            List<GioHang> lstGioHang = LayGioHang();
            lstGioHang.Clear(); // Xóa sạch List
            return RedirectToAction("Index", "SachOnline");
        }

        // 4. Chuyển sang trang Đặt Hàng
        [HttpGet]
        public ActionResult DatHang()
        {
            // Kiểm tra xem khách hàng đã đăng nhập chưa (dựa vào Session["Taikhoan"] bạn đã tạo ở bài trước)
            if (Session["Taikhoan"] == null || Session["Taikhoan"].ToString() == "")
            {
                // Chưa đăng nhập thì bắt sang trang Đăng Nhập
                return RedirectToAction("DangNhap", "User");
            }

            // Kiểm tra xem giỏ hàng có trống không
            if (Session["GioHang"] == null)
            {
                return RedirectToAction("Index", "SachOnline");
            }

            // Nếu đã đăng nhập và có đồ trong giỏ thì lấy giỏ hàng ra để chuẩn bị thanh toán
            List<GioHang> lstGioHang = LayGioHang();
            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongTien = TongTien();

            return View(lstGioHang); // Trả về trang DatHang.cshtml (bạn sẽ tạo sau)
        }

        public ActionResult xoaGioHang()
        {
            List<GioHang> listGioHang = LayGioHang();
            listGioHang.Clear();
            return RedirectToAction("Index", "SachOnline");
        }
    }
}