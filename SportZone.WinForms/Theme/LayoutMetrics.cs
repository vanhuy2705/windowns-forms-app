using System;
using System.Drawing;
using System.Windows.Forms;

namespace SportZone.WinForms.DesignTokens
{
    /// <summary>
    /// Breakpoint và quy chuẩn layout đáp ứng (Responsive Layout Metrics)
    /// </summary>
    public static class LayoutMetrics
    {
        // Breakpoint chiều rộng không gian nội dung (Content Area Width)
        public const int BreakpointCompact = 1050;   // Dưới 1050px: Màn hình 1280x720 hoặc thu nhỏ
        public const int BreakpointNormal = 1450;    // 1050px - 1450px: Màn hình 1366x768 / 1440x900
        public const int BreakpointWide = 1800;      // Trên 1450px: Màn hình Full HD 1920x1080

        // Kích thước chuẩn cửa sổ chính
        public static readonly Size DefaultAppSize = new Size(1440, 900);
        public static readonly Size MinimumAppSize = new Size(1280, 720);

        // Kích thước chuẩn các Dialog
        public static readonly Size DialogSmall = new Size(480, 420);
        public static readonly Size DialogMedium = new Size(620, 560);
        public static readonly Size DialogLarge = new Size(840, 680);
        public static readonly Size DialogDetail = new Size(960, 700);

        /// <summary>
        /// Kiểm tra độ rộng có đang ở chế độ Compact hay không
        /// </summary>
        public static bool IsCompact(int width) => width < BreakpointCompact;

        /// <summary>
        /// Giới hạn giá trị trong khoảng [min, max]
        /// </summary>
        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// Tính số cột tự co giãn theo độ rộng vùng chứa
        /// </summary>
        public static int CalculateColumns(int containerWidth, int minColWidth, int gap, int maxCols = 4)
        {
            if (containerWidth <= 0 || minColWidth <= 0) return 1;
            int cols = (containerWidth + gap) / (minColWidth + gap);
            return Clamp(cols, 1, maxCols);
        }
    }
}
