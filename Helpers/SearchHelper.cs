using System;
using System.Globalization;
using System.Text;

namespace TanHoangAnh.SachOnline.Helpers
{
    public static class SearchHelper
    {
        public static string BoDau(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Khớp từ khóa trong chuỗi nguồn (không phân biệt hoa thường, có/không dấu).
        /// Hỗ trợ tìm theo ký tự bất kỳ hoặc chữ cái đầu trong tên.
        /// </summary>
        public static bool KhopTuKhoa(string source, string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return false;
            if (string.IsNullOrEmpty(source))
                return false;

            var src = BoDau(source).ToLowerInvariant();
            var key = BoDau(keyword.Trim()).ToLowerInvariant();

            if (src.Contains(key))
                return true;

            // Khớp theo chữ cái đầu từng từ (vd: "nt" -> "Ngoại ngữ tin học")
            var words = src.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (key.Length <= words.Length)
            {
                bool matchFirstLetters = true;
                for (int i = 0; i < key.Length; i++)
                {
                    if (words[i][0] != key[i])
                    {
                        matchFirstLetters = false;
                        break;
                    }
                }
                if (matchFirstLetters)
                    return true;
            }

            return false;
        }
    }
}
