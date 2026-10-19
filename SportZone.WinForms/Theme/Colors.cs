using System;
using System.Drawing;

namespace SportZone.WinForms.DesignTokens
{
    public static class Colors
    {
        // Base Neutrals (60%)
        public static readonly Color AppBackground = ColorTranslator.FromHtml("#F3F6FA");
        public static readonly Color Canvas = AppBackground;
        public static readonly Color Surface = Color.FromArgb(255, 255, 255);
        public static readonly Color SurfaceElevated = ColorTranslator.FromHtml("#F8FAFC");
        public static readonly Color SurfaceAlt = ColorTranslator.FromHtml("#F1F5F9");
        public static readonly Color TextPrimary = ColorTranslator.FromHtml("#0B1220");
        public static readonly Color TextSecondary = ColorTranslator.FromHtml("#526071");
        public static readonly Color TextMuted = ColorTranslator.FromHtml("#8492A6");
        public static readonly Color Border = ColorTranslator.FromHtml("#DDE5EE");
        public static readonly Color BorderDark = ColorTranslator.FromHtml("#CBD5E1");

        // Sidebar Navigation
        public static readonly Color SidebarBackground = ColorTranslator.FromHtml("#07111F");
        public static readonly Color SidebarSecondary = ColorTranslator.FromHtml("#0C1A2B");
        public static readonly Color SidebarHover = ColorTranslator.FromHtml("#12243A");
        public static readonly Color SidebarActive = ColorTranslator.FromHtml("#087A55");
        public static readonly Color SidebarActiveBorder = ColorTranslator.FromHtml("#16B364");
        public static readonly Color SidebarText = ColorTranslator.FromHtml("#8A99AD");
        public static readonly Color SidebarTextBright = Color.White;
        public static readonly Color SidebarSectionTitle = ColorTranslator.FromHtml("#718096");

        // Brand Palette (30%)
        public static readonly Color Emerald = ColorTranslator.FromHtml("#16B364");
        public static readonly Color DeepEmerald = ColorTranslator.FromHtml("#087A55");
        public static readonly Color Mint = ColorTranslator.FromHtml("#A6F4C5");
        public static readonly Color Primary = Emerald;
        public static readonly Color PrimaryHover = ColorTranslator.FromHtml("#139A56");
        public static readonly Color PrimaryPressed = DeepEmerald;

        // Electric Accents (10%)
        public static readonly Color Cyan = ColorTranslator.FromHtml("#06B6D4");
        public static readonly Color ElectricBlue = ColorTranslator.FromHtml("#2563EB");
        public static readonly Color Indigo = ColorTranslator.FromHtml("#4F46E5");
        public static readonly Color Purple = ColorTranslator.FromHtml("#7C3AED");
        public static readonly Color Coral = ColorTranslator.FromHtml("#F43F5E");
        public static readonly Color Orange = ColorTranslator.FromHtml("#F97316");
        public static readonly Color Amber = ColorTranslator.FromHtml("#F59E0B");

        // Status Colors & Light Badges
        public static readonly Color Success = Emerald;
        public static readonly Color SuccessBg = ColorTranslator.FromHtml("#DCFCE7");
        public static readonly Color Warning = Amber;
        public static readonly Color WarningBg = ColorTranslator.FromHtml("#FEF3C7");
        public static readonly Color Danger = Coral;
        public static readonly Color DangerBg = ColorTranslator.FromHtml("#FEE2E2");
        public static readonly Color Info = Cyan;
        public static readonly Color InfoBg = ColorTranslator.FromHtml("#E0F2FE");
        public static readonly Color Slate = ColorTranslator.FromHtml("#64748B");
        public static readonly Color SlateBg = ColorTranslator.FromHtml("#F1F5F9");
        public static readonly Color PurpleBg = ColorTranslator.FromHtml("#F3E8FF");
        public static readonly Color OrangeBg = ColorTranslator.FromHtml("#FFEDD5");

        // Role Flavors
        // Admin: Command Center (Navy + Emerald + Cyan + Violet)
        public static readonly Color AdminAccent = Cyan;
        // Staff: Operations / POS (Slate + Electric Blue + Cyan + Amber)
        public static readonly Color StaffAccent = ElectricBlue;
        // Customer: Sport Lifestyle (White + Emerald + Sky + Coral)
        public static readonly Color CustomerAccent = Emerald;
    }
}
