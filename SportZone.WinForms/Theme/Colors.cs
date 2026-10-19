using System;
using System.Drawing;

namespace SportZone.WinForms.DesignTokens
{
    /// <summary>
    /// V9 Design Tokens — màu chuẩn theo hợp đồng UI_V9.
    /// Bảng màu teal thể thao (#0F766E) thay cho emerald cũ; tên legacy
    /// (Emerald/Mint/DeepEmerald...) giữ lại làm alias để mọi View tiếp tục biên dịch.
    /// </summary>
    public static class Colors
    {
        // Base Neutrals
        public static readonly Color AppBackground = ColorTranslator.FromHtml("#F5F7FA");
        public static readonly Color Canvas = AppBackground;
        public static readonly Color Surface = Color.White;
        public static readonly Color SurfaceElevated = ColorTranslator.FromHtml("#F8FAFC");
        public static readonly Color SurfaceAlt = ColorTranslator.FromHtml("#F1F5F9");
        public static readonly Color TextPrimary = ColorTranslator.FromHtml("#172B36");
        public static readonly Color TextSecondary = ColorTranslator.FromHtml("#64748B");
        public static readonly Color TextMuted = ColorTranslator.FromHtml("#94A3B8");
        public static readonly Color Border = ColorTranslator.FromHtml("#E2E8F0");
        public static readonly Color BorderDark = ColorTranslator.FromHtml("#CBD5E1");

        // Sidebar Navigation
        public static readonly Color SidebarBackground = ColorTranslator.FromHtml("#101828");
        public static readonly Color SidebarSecondary = ColorTranslator.FromHtml("#182230");
        public static readonly Color SidebarHover = ColorTranslator.FromHtml("#22303F");
        public static readonly Color SidebarActive = Primary;
        public static readonly Color SidebarActiveBorder = Accent;
        public static readonly Color SidebarText = ColorTranslator.FromHtml("#94A3B8");
        public static readonly Color SidebarTextBright = Color.White;
        public static readonly Color SidebarSectionTitle = ColorTranslator.FromHtml("#64748B");

        // Brand Palette — Primary teal V9
        public static readonly Color Teal = ColorTranslator.FromHtml("#0F766E");
        public static readonly Color TealHover = ColorTranslator.FromHtml("#115E59");
        public static readonly Color TealPressed = ColorTranslator.FromHtml("#134E4A");
        public static readonly Color Emerald = Teal;                       // legacy alias
        public static readonly Color DeepEmerald = TealPressed;            // legacy alias
        public static readonly Color Mint = ColorTranslator.FromHtml("#CCFBF1"); // legacy alias
        public static readonly Color Primary = Teal;
        public static readonly Color PrimaryHover = TealHover;
        public static readonly Color PrimaryPressed = TealPressed;

        // Accent
        public static readonly Color Accent = ColorTranslator.FromHtml("#14B8A6");
        public static readonly Color AccentLight = ColorTranslator.FromHtml("#CCFBF1");
        public static readonly Color Cyan = Accent; // legacy alias (thu hút tương tự cũ)
        public static readonly Color ElectricBlue = ColorTranslator.FromHtml("#2563EB");
        public static readonly Color Indigo = ColorTranslator.FromHtml("#4F46E5");
        public static readonly Color Purple = ColorTranslator.FromHtml("#7C3AED");
        public static readonly Color Coral = ColorTranslator.FromHtml("#F43F5E");
        public static readonly Color Orange = ColorTranslator.FromHtml("#F97316");
        public static readonly Color Amber = ColorTranslator.FromHtml("#F59E0B");

        // Status Colors & Light Badges (V9: success #15803D, warning #B45309, danger #B91C1C, info #2563EB)
        public static readonly Color Success = ColorTranslator.FromHtml("#15803D");
        public static readonly Color SuccessBg = ColorTranslator.FromHtml("#DCFCE7");
        public static readonly Color Warning = ColorTranslator.FromHtml("#B45309");
        public static readonly Color WarningBg = ColorTranslator.FromHtml("#FEF3C7");
        public static readonly Color Danger = ColorTranslator.FromHtml("#B91C1C");
        public static readonly Color DangerBg = ColorTranslator.FromHtml("#FEE2E2");
        public static readonly Color Info = ColorTranslator.FromHtml("#2563EB");
        public static readonly Color InfoBg = ColorTranslator.FromHtml("#DBEAFE");
        public static readonly Color Slate = ColorTranslator.FromHtml("#64748B");
        public static readonly Color SlateBg = ColorTranslator.FromHtml("#F1F5F9");
        public static readonly Color PurpleBg = ColorTranslator.FromHtml("#F3E8FF");
        public static readonly Color OrangeBg = ColorTranslator.FromHtml("#FFEDD5");

        // Selected / hover row
        public static readonly Color Selected = ColorTranslator.FromHtml("#E6F4F1");
        public static readonly Color SelectedBg = Selected;

        // Legacy sport-bowl accents kept for chart series variety
        public static readonly Color SportBlue = ColorTranslator.FromHtml("#2563EB");

        // Role Flavors — đồng bộ theo V9
        public static readonly Color AdminAccent = Accent;
        public static readonly Color StaffAccent = ElectricBlue;
        public static readonly Color CustomerAccent = Accent;

        // Inverse text on primary surfaces
        public static readonly Color TextInverse = Color.White;
    }
}
