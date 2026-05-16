import pathlib
import re
import subprocess

BASE = pathlib.Path(r"e:\TH_LT_WEB\lab 5\TanHoangAnh.SachOnline")
VIEWS = BASE / "Views"


def u(s: str) -> str:
    return s.encode("utf-8").decode("unicode_escape")


def write_bom(path: pathlib.Path, text: str) -> None:
    path.write_text(text, encoding="utf-8-sig")


SACH_THEO_NXB = u(r"""@using PagedList.Mvc;
@using TanHoangAnh.SachOnline.Models;
@model PagedList.IPagedList<SACH>

@{
    ViewBag.Title = "S\u00e1ch theo nh\u00e0 xu\u1ea5t b\u1ea3n";
    Layout = "~/Views/Shared/_LayoutUser.cshtml";
}

@Styles.Render("~/Content/PagedList.css")

<style type="text/css">
    .imgbook {
        transition: all 1s ease-in;
        width: 400px;
    }

        .imgbook:hover {
            transform: scale(0.9);
            cursor: pointer;
        }
</style>

@section NhaXuatBanPartial {
    @Html.Action("NhaXuatBanPartial", "SachOnline")}

@section SachBanNhieuPartial {
    @Html.Action("SachBanNhieuPartial", "SachOnline")}

@section SliderPartial {
    @Html.Action("SliderPartial", "SachOnline")}

@section NavPartial {
    @Html.Action("NavPartial", "SachOnline")}

@section FooterPartial {
    @Html.Action("FooterPartial", "SachOnline")}

<h2 class="text-center">S\u00c1CH THEO NH\u00c0 XU\u1ea4T B\u1ea2N: @ViewBag.TenNXB</h2>
<hr>
<div class="row text-center">
    @foreach (var sach in Model)
    {
        <motion.div class="col-sm-4 col-md-4 col-lg-4 col-xs-6">
            <div class="thumbnail">
                <img src="~/Images/@sach.AnhBia" alt="@sach.TenSach"
                     class="img-responsive img-rounded imgbook"
                     style="width: 100%; height: 300px; object-fit: contain; padding: 10px;">
                <div class="caption">
                    <h4 style="min-height:70px;">
                        <a href="@Url.Action("ChiTietSach", "SachOnline", new { id = sach.MaSach })" style="text-decoration: none; font-weight: bold;">
                            @sach.TenSach
                        </a>
                    </h4>
                    <p>
                        <a href="#" class="btn btn-primary" role="button">
                            <span class="glyphicon glyphicon-shopping-cart" aria-hidden="true">
                            </span> Add to Cart
                        </a>
                    </p>
                </div>
            </div>
        </div>
    }
</div>
@*T\u1ea1o li\u00ean k\u1ebft c\u00e1c trang*@
<div class="text-center" style="margin-top: 20px;">
    <p>Trang @(Model.PageCount < Model.PageNumber ? 0 : Model.PageNumber) / @Model.PageCount</p>
    @Html.PagedListPager(Model, page => Url.Action("SachTheoNhaXuatBan", new { id = ViewBag.MaNXB, page = page }),
    new PagedListRenderOptions { UlElementClasses = new[] { "pagination" } })
</div>
""").replace("motion.div", "motion.div").replace("<motion.div", "<motion.div")


def git_text(rel: str) -> str:
    raw = subprocess.check_output(["git", "-C", str(BASE), "show", f"HEAD:{rel}"])
    for enc in ("utf-8-sig", "utf-8", "cp1252", "latin-1"):
        try:
            return raw.decode(enc)
        except UnicodeDecodeError:
            continue
    return raw.decode("latin-1")


def fix_chude():
    text = git_text("Views/SachOnline/SachTheoChuDe.cshtml")
    text = re.sub(
        r'ViewBag\.Title\s*=\s*"[^"]*";',
        u('ViewBag.Title = "S\u00e1ch theo ch\u1ee7 \u0111\u1ec1";'),
        text,
        count=1,
    )
    text = re.sub(
        r'<h2 class="text-center">[^<]*@ViewBag\.TenChuDe\s*</h2>',
        u('<h2 class="text-center">S\u00c1CH THEO CH\u1ee6 \u0110\u1ec0: @ViewBag.TenChuDe</h2>'),
        text,
        count=1,
    )
    text = re.sub(r"@\*[^*]*\*@", u("@*T\u1ea1o li\u00ean k\u1ebft c\u00e1c trang*@"), text, count=1)
    write_bom(VIEWS / "SachOnline" / "SachTheoChuDe.cshtml", text)


def main():
    nxb = SACH_THEO_NXB.replace("motion.div", "div")
    write_bom(VIEWS / "SachOnline" / "SachTheoNhaXuatBan.cshtml", nxb)
    fix_chude()

    xn = VIEWS / "GioHang" / "XacNhanDonHang.cshtml"
    t = xn.read_text(encoding="utf-8-sig")
    t = re.sub(
        r'ViewBag\.Title\s*=\s*"[^"]*";',
        u('ViewBag.Title = "X\u00e1c nh\u1eadn \u0111\u01a1n h\u00e0ng";'),
        t,
        count=1,
    )
    write_bom(xn, t)


if __name__ == "__main__":
    main()
