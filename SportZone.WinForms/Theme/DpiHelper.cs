using System;
using System.Drawing;
using System.Windows.Forms;

namespace SportZone.WinForms.DesignTokens
{
    /// <summary>
    /// Tiện ích hỗ trợ quản lý DPI và tính toán kích thước co giãn thống nhất
    /// </summary>
    public static class DpiHelper
    {
        public const float StandardDpi = 96.0f;

        /// <summary>
        /// Lấy hệ số tỉ lệ DPI hiện tại của control/form
        /// </summary>
        public static float GetScaleFactor(Control control)
        {
            if (control == null) return 1.0f;
            using (Graphics g = control.CreateGraphics())
            {
                return g.DpiX / StandardDpi;
            }
        }

        /// <summary>
        /// Áp dụng chế độ DPI đồng bộ cho Form
        /// </summary>
        public static void ApplyDpiMode(Form form)
        {
            if (form == null) return;
            form.AutoScaleMode = AutoScaleMode.Dpi;
            form.AutoScaleDimensions = new SizeF(StandardDpi, StandardDpi);
        }

        /// <summary>
        /// Áp dụng chế độ DPI đồng bộ cho UserControl
        /// </summary>
        public static void ApplyDpiMode(UserControl uc)
        {
            if (uc == null) return;
            uc.AutoScaleMode = AutoScaleMode.Dpi;
            uc.AutoScaleDimensions = new SizeF(StandardDpi, StandardDpi);
        }

        /// <summary>
        /// Đảm bảo Form không bao giờ vượt quá vùng làm việc của màn hình (WorkingArea),
        /// ngăn ngừa việc header hoặc footer của popup bị cắt mất trên màn hình nhỏ hoặc DPI cao.
        /// </summary>
        public static void ClampToScreen(Form form, int marginX = 32, int marginY = 48)
        {
            if (form == null) return;

            Rectangle workArea = Screen.FromControl(form).WorkingArea;
            int maxW = workArea.Width - marginX;
            int maxH = workArea.Height - marginY;

            int targetW = Math.Min(form.Width, maxW);
            int targetH = Math.Min(form.Height, maxH);

            if (targetW != form.Width || targetH != form.Height)
            {
                form.Size = new Size(targetW, targetH);
            }

            // Căn giữa màn hình nếu cần
            int newX = Math.Max(workArea.Left + marginX / 2, workArea.Left + (workArea.Width - form.Width) / 2);
            int newY = Math.Max(workArea.Top + marginY / 2, workArea.Top + (workArea.Height - form.Height) / 2);
            form.Location = new Point(newX, newY);
        }
    }
}
