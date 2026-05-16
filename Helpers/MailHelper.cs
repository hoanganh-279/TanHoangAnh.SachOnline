using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace TanHoangAnh.SachOnline.Helpers
{
    public static class MailHelper
    {
        public static bool GuiLienHe(string hoTen, string email, string dienThoai, string chuDe, string noiDung, out string loi)
        {
            loi = null;

            var toEmail = ConfigurationManager.AppSettings["ContactEmailTo"];
            var smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            var smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
            var smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"];
            var smtpFrom = ConfigurationManager.AppSettings["SmtpFrom"] ?? smtpUser;

            if (string.IsNullOrWhiteSpace(toEmail)
                || string.IsNullOrWhiteSpace(smtpHost)
                || string.IsNullOrWhiteSpace(smtpUser)
                || string.IsNullOrWhiteSpace(smtpPassword))
            {
                loi = "Website chưa cấu hình gửi email. Quản trị cần thiết lập SMTP trong Web.config.";
                return false;
            }

            int smtpPort = 587;
            int.TryParse(ConfigurationManager.AppSettings["SmtpPort"], out smtpPort);
            if (smtpPort <= 0) smtpPort = 587;

            bool enableSsl = true;
            bool.TryParse(ConfigurationManager.AppSettings["SmtpEnableSsl"], out enableSsl);

            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(smtpFrom.Trim(), "SachOnline - Liên hệ");
                    message.To.Add(toEmail.Trim());
                    message.Subject = "[SachOnline] Tin nhắn liên hệ: " + TenChuDe(chuDe);
                    message.Body = TaoNoiDungEmail(hoTen, email, dienThoai, chuDe, noiDung);
                    message.IsBodyHtml = true;
                    message.BodyEncoding = Encoding.UTF8;
                    message.SubjectEncoding = Encoding.UTF8;
                    message.ReplyToList.Add(new MailAddress(email, hoTen));

                    using (var smtp = new SmtpClient(smtpHost.Trim(), smtpPort))
                    {
                        smtp.EnableSsl = enableSsl;
                        smtp.UseDefaultCredentials = false;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.Timeout = 30000;
                        smtp.Credentials = new NetworkCredential(
                            smtpUser.Trim(),
                            smtpPassword.Trim().Replace(" ", ""));
                        smtp.Send(message);
                    }
                }

                return true;
            }
            catch (SmtpException ex)
            {
                loi = ThongBaoLoiSmtp(ex);
                return false;
            }
            catch (Exception)
            {
                loi = "Có lỗi khi gửi email. Vui lòng thử lại sau.";
                return false;
            }
        }

        private static string ThongBaoLoiSmtp(SmtpException ex)
        {
            var msg = ex.Message ?? "";
            var inner = ex.InnerException?.Message ?? "";
            var combined = (msg + " " + inner).ToLowerInvariant();

            if (combined.Contains("not authenticated")
                || combined.Contains("authentication required")
                || combined.Contains("5.7.0")
                || combined.Contains("username and password"))
            {
                return "Gmail từ chối đăng nhập SMTP. Hãy bật xác minh 2 bước cho tài khoản Google, "
                    + "tạo Mật khẩu ứng dụng (16 ký tự) tại myaccount.google.com/apppasswords "
                    + "và dán vào SmtpPassword trong Web.config (không dùng mật khẩu đăng nhập Gmail thường).";
            }

            return "Không gửi được email. Kiểm tra lại cấu hình SMTP (host, cổng, tài khoản, mật khẩu ứng dụng).";
        }

        private static string TenChuDe(string chuDe)
        {
            switch (chuDe)
            {
                case "dat-hang": return "Đặt hàng / Giao hàng";
                case "doi-tra": return "Đổi trả sản phẩm";
                case "gop-y": return "Góp ý website";
                case "khac": return "Khác";
                default: return string.IsNullOrWhiteSpace(chuDe) ? "Không chọn" : chuDe;
            }
        }

        private static string TaoNoiDungEmail(string hoTen, string email, string dienThoai, string chuDe, string noiDung)
        {
            var dt = string.IsNullOrWhiteSpace(dienThoai) ? "(không có)" : HttpUtility.HtmlEncode(dienThoai);
            var sb = new StringBuilder();
            sb.Append("<html><body style='font-family:Segoe UI,Tahoma,sans-serif;font-size:14px;color:#333;'>");
            sb.Append("<h2 style='color:#1e3a5f;'>Tin nhắn liên hệ từ SachOnline</h2>");
            sb.Append("<table cellpadding='6' cellspacing='0' style='border-collapse:collapse;'>");
            sb.AppendFormat("<tr><td><strong>Họ tên:</strong></td><td>{0}</td></tr>", HttpUtility.HtmlEncode(hoTen));
            sb.AppendFormat("<tr><td><strong>Email:</strong></td><td>{0}</td></tr>", HttpUtility.HtmlEncode(email));
            sb.AppendFormat("<tr><td><strong>Điện thoại:</strong></td><td>{0}</td></tr>", dt);
            sb.AppendFormat("<tr><td><strong>Chủ đề:</strong></td><td>{0}</td></tr>", HttpUtility.HtmlEncode(TenChuDe(chuDe)));
            sb.AppendFormat("<tr><td valign='top'><strong>Nội dung:</strong></td><td>{0}</td></tr>",
                HttpUtility.HtmlEncode(noiDung).Replace("\n", "<br/>"));
            sb.AppendFormat("<tr><td><strong>Thời gian:</strong></td><td>{0:dd/MM/yyyy HH:mm:ss}</td></tr>", DateTime.Now);
            sb.Append("</table>");
            sb.Append("<p style='color:#888;font-size:12px;'>Email tự động từ form Liên hệ SachOnline.</p>");
            sb.Append("</body></html>");
            return sb.ToString();
        }
    }
}
