using System;
using System.Drawing;

namespace SportZone.WinForms.DesignTokens
{
    public static class Typography
    {
        private const string FontFamilyName = "Segoe UI";

        public static readonly Font HeroTitle = new Font(FontFamilyName, 22f, FontStyle.Bold);
        public static readonly Font PageTitle = new Font(FontFamilyName, 17f, FontStyle.Bold);
        public static readonly Font SectionTitle = new Font(FontFamilyName, 11.5f, FontStyle.Bold);
        public static readonly Font SubTitle = new Font(FontFamilyName, 13f, FontStyle.Bold);
        public static readonly Font KpiValue = new Font(FontFamilyName, 21f, FontStyle.Bold);
        public static readonly Font KpiValueSmall = new Font(FontFamilyName, 16f, FontStyle.Bold);

        public static readonly Font Button = new Font(FontFamilyName, 9.5f, FontStyle.Bold);
        public static readonly Font ButtonSmall = new Font(FontFamilyName, 8.5f, FontStyle.Bold);

        public static readonly Font Body = new Font(FontFamilyName, 9.5f, FontStyle.Regular);
        public static readonly Font BodyBold = new Font(FontFamilyName, 9.5f, FontStyle.Bold);
        public static readonly Font BodyMedium = new Font(FontFamilyName, 9.5f, FontStyle.Regular);

        public static readonly Font Secondary = new Font(FontFamilyName, 8.75f, FontStyle.Regular);
        public static readonly Font SecondaryBold = new Font(FontFamilyName, 8.75f, FontStyle.Bold);
        public static readonly Font CaptionBold = SecondaryBold;

        public static readonly Font Micro = new Font(FontFamilyName, 8f, FontStyle.Regular);
        public static readonly Font MicroBold = new Font(FontFamilyName, 8f, FontStyle.Bold);
        public static readonly Font SectionHeader = new Font(FontFamilyName, 7.75f, FontStyle.Bold);
    }
}
