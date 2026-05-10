using TanHoangAnh.SachOnline.Models;
using System.Linq;
using System.Web.Mvc;

namespace TanHoangAnh.SachOnline.Areas.Admin.Controllers
{
    public class DonHangController : Controller
    {
        SachOnlineEntities db = new SachOnlineEntities();

        public ActionResult Index()
        {
            ViewBag.ActiveMenu = "DonHang";
            var listDH = db.DONDATHANGs.OrderByDescending(d => d.NgayDat).ToList();
            return View(listDH);
        }

        public ActionResult Details(int id)
        {
            ViewBag.ActiveMenu = "DonHang";
            var dh = db.DONDATHANGs
                .Include("KHACHHANG")
                .SingleOrDefault(d => d.MaDonHang == id);
            if (dh == null) return HttpNotFound();

            // Include SACH để hiển thị tên sách đúng trong View
            var chiTiet = db.CHITIETDATHANGs
                .Include("SACH")
                .Where(c => c.MaDonHang == id)
                .ToList();
            ViewBag.ChiTiet = chiTiet;

            return View(dh);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            ViewBag.ActiveMenu = "DonHang";
            var dh = db.DONDATHANGs.SingleOrDefault(d => d.MaDonHang == id);
            if (dh == null) return HttpNotFound();
            return View(dh);
        }

        [HttpPost]
        public ActionResult Edit(DONDATHANG dh)
        {
            ViewBag.ActiveMenu = "DonHang";
            var dhUpdate = db.DONDATHANGs.SingleOrDefault(d => d.MaDonHang == dh.MaDonHang);
            if (dhUpdate != null)
            {
                dhUpdate.TinhTrangGiaoHang = dh.TinhTrangGiaoHang;
                dhUpdate.NgayGiao = dh.NgayGiao;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(dh);
        }
    }
}