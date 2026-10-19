using System;
using System.Drawing;

namespace SportZone.WinForms.DesignTokens
{
    public static class Typography
    {
        private const string FontFamilyName = "Segoe UI";

        // V9 target px @100% DPI: page title 24–28, section 18–20, card 15–17,
        // body 13–15, table 13–14, secondary 12–13, button 13–14, KPI 28–34.
        // WinForms point ≈ px * 96/72, so scale ONCE here (px * 0.75).
        public static readonly Font HeroTitle = new Font(FontFamilyName, 21f, FontStyle.Bold);   // 28px KPI hero
        public static readonly Font PageTitle = new Font(FontFamilyName, 18f, FontStyle.Bold);   // 24px
        public static readonly Font SectionTitle = new Font(FontFamilyName, 13.5f, FontStyle.Bold); // 18px
        public static readonly Font CardTitle = new Font(FontFamilyName, 11.25f, FontStyle.Bold); // 15px
        public static readonly Font SubTitle = new Font(FontFamilyName, 13f, FontStyle.Bold);
        public static readonly Font KpiValue = new Font(FontFamilyName, 22f, FontStyle.Bold);     // 29px
        public static readonly Font KpiValueSmall = new Font(FontFamilyName, 17f, FontStyle.Bold); // 23px

        public static readonly Font Button = new Font(FontFamilyName, 9.75f, FontStyle.Bold);     // 13px
        public static readonly Font ButtonSmall = new Font(FontFamilyName, 8.75f, FontStyle.Bold); // 12px

        public static readonly Font Body = new Font(FontFamilyName, 9.75f, FontStyle.Regular);     // 13px
        public static readonly Font BodyBold = new Font(FontFamilyName, 9.75f, FontStyle.Bold);
        public static readonly Font BodyMedium = new Font(FontFamilyName, 9.75f, FontStyle.Regular);

        public static readonly Font Secondary = new Font(FontFamilyName, 9f, FontStyle.Regular);   // 12px
        public static readonly Font SecondaryBold = new Font(FontFamilyName, 9f, FontStyle.Bold);
        public static readonly Font CaptionBold = SecondaryBold;

        public static readonly Font Micro = new Font(FontFamilyName, 8.25f, FontStyle.Regular);    // 11px
        public static readonly Font MicroBold = new Font(FontFamilyName, 8.25f, FontStyle.Bold);
        public static readonly Font SectionHeader = new Font(FontFamilyName, 7.75f, FontStyle.Bold);

        // Table typography V9: 13–14px
        public static readonly Font Table = new Font(FontFamilyName, 9.75f, FontStyle.Regular);    // 13px
        public static readonly Font TableHeader = new Font(FontFamilyName, 9f, FontStyle.Bold);    // 12px
    }
}
