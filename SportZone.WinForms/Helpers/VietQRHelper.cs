using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

namespace SportZone.WinForms.Helpers
{
    public static class VietQRHelper
    {
        /// <summary>
        /// Tạo chuỗi chuẩn EMVCo VietQR theo quy chuẩn Napas 247
        /// </summary>
        public static string GenerateEmvcoString(string bankBin, string accountNumber, decimal amount, string description)
        {
            if (string.IsNullOrWhiteSpace(bankBin) || string.IsNullOrWhiteSpace(accountNumber))
            {
                throw new ArgumentException("Chưa cấu hình mã định danh ngân hàng (BIN) hoặc số tài khoản thụ hưởng.");
            }

            // Field 38: Consumer Account Information (Napas format)
            string guid = "A000000727";
            string subField00 = FormatTlv("00", guid);
            string subField01 = FormatTlv("01", bankBin + accountNumber);
            string subField02 = FormatTlv("02", "QRIBFTTA");
            string field38Value = subField00 + subField01 + subField02;
            string field38 = FormatTlv("38", field38Value);

            // Field 53: Currency (704 = VND)
            string field53 = FormatTlv("53", "704");

            // Field 54: Amount
            long amountVal = (long)Math.Max(0, Math.Round(amount));
            string field54 = amountVal > 0 ? FormatTlv("54", amountVal.ToString()) : "";

            // Field 58: Country Code
            string field58 = FormatTlv("58", "VN");

            // Field 62: Additional Data (Transfer description)
            string cleanDesc = RemoveDiacritics(description ?? "SPORTFIELD").Trim();
            if (cleanDesc.Length > 25) cleanDesc = cleanDesc.Substring(0, 25);
            string sub62_08 = FormatTlv("08", cleanDesc);
            string field62 = FormatTlv("62", sub62_08);

            // Payload before CRC
            string rawPayload = "000201" + "010212" + field38 + field53 + field54 + field58 + field62 + "6304";
            string crc = ComputeCrc16(rawPayload);

            return rawPayload + crc;
        }

        private static string FormatTlv(string tag, string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            string lengthStr = value.Length.ToString("D2");
            return tag + lengthStr + value;
        }

        private static string ComputeCrc16(string str)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(str);
            ushort crc = 0xFFFF;
            for (int i = 0; i < bytes.Length; i++)
            {
                crc ^= (ushort)(bytes[i] << 8);
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x8000) != 0)
                        crc = (ushort)((crc << 1) ^ 0x1021);
                    else
                        crc <<= 1;
                }
            }
            return crc.ToString("X4");
        }

        public static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            string normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (char c in normalized)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    if (c == 'đ' || c == 'Đ') sb.Append('D');
                    else sb.Append(c);
                }
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        public static Bitmap GenerateQrBitmap(string bankBin, string accountNumber, decimal amount, string description, int width, int height)
        {
            if (string.IsNullOrWhiteSpace(bankBin) || string.IsNullOrWhiteSpace(accountNumber))
            {
                return null;
            }
            string payload = GenerateEmvcoString(bankBin, accountNumber, amount, description);
            return GenerateQrBitmap(payload, width, height);
        }

        /// <summary>
        /// Tạo Bitmap mã QR Code thật chuẩn EMVCo/Napas 247 sử dụng QRCoder
        /// </summary>
        public static Bitmap GenerateQrBitmap(string content, int width, int height)
        {
            if (string.IsNullOrEmpty(content)) content = "SPORTFIELD";
            if (width <= 0) width = 240;
            if (height <= 0) height = 240;

            using (var qrGenerator = new QRCoder.QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(content, QRCoder.QRCodeGenerator.ECCLevel.M);
                using (var qrCode = new QRCoder.QRCode(qrCodeData))
                {
                    int moduleCount = qrCodeData.ModuleMatrix.Count;
                    int pixelsPerModule = Math.Max(1, Math.Min(width, height) / (moduleCount + 4));
                    Bitmap rawBmp = qrCode.GetGraphic(pixelsPerModule, Color.Black, Color.White, true);
                    if (rawBmp.Width == width && rawBmp.Height == height)
                    {
                        return rawBmp;
                    }

                    Bitmap canvas = new Bitmap(width, height);
                    using (Graphics g = Graphics.FromImage(canvas))
                    {
                        g.Clear(Color.White);
                        g.InterpolationMode = InterpolationMode.NearestNeighbor;
                        g.PixelOffsetMode = PixelOffsetMode.Half;
                        int x = Math.Max(0, (width - rawBmp.Width) / 2);
                        int y = Math.Max(0, (height - rawBmp.Height) / 2);
                        g.DrawImage(rawBmp, x, y, Math.Min(rawBmp.Width, width), Math.Min(rawBmp.Height, height));
                    }
                    rawBmp.Dispose();
                    return canvas;
                }
            }
        }
    }
}
