using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace SportZone.WinForms.Controls
{
    public static class AssetManager
    {
        private static readonly Dictionary<string, Image> Cache = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);

        private static string GetAssetsPath()
        {
            // First check application directory Assets
            string path1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
            if (Directory.Exists(path1)) return path1;

            // Second check project folder Assets
            string path2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\Assets");
            if (Directory.Exists(path2)) return Path.GetFullPath(path2);

            string path3 = @"D:\bangoc\SportField_WinForms_Merged_Final\SportZone.WinForms\Assets";
            if (Directory.Exists(path3)) return path3;

            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static Image GetImage(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return null;

            if (Cache.TryGetValue(fileName, out Image img))
            {
                return img;
            }

            string assetsDir = GetAssetsPath();
            string fullPath = Path.Combine(assetsDir, fileName);

            if (File.Exists(fullPath))
            {
                try
                {
                    using (Image original = Image.FromFile(fullPath))
                    {
                        Bitmap bmp = new Bitmap(original);
                        Cache[fileName] = bmp;
                        return bmp;
                    }
                }
                catch
                {
                    // ignored
                }
            }

            return null;
        }

        public static Bitmap GetStadiumHero()
        {
            return GetImage("stadium_night_hero.jpg") as Bitmap;
        }

        public static Bitmap GetFootballPitch()
        {
            return GetImage("football_pitch.jpg") as Bitmap;
        }

        public static Bitmap GetBadmintonCourt()
        {
            return GetImage("badminton_court.jpg") as Bitmap;
        }

        public static Bitmap GetTennisCourt()
        {
            return GetImage("tennis_court.jpg") as Bitmap;
        }

        public static Bitmap GetCustomerAvatar()
        {
            return GetImage("customer_avatar.png") as Bitmap;
        }

        public static Bitmap GetStaffAvatar()
        {
            return GetImage("staff_avatar.png") as Bitmap;
        }

        public static Bitmap GetLogo()
        {
            return GetImage("sportfield_logo.png") as Bitmap;
        }

        public static Bitmap GetBrandMark()
        {
            return GetImage("brand_mark.png") as Bitmap;
        }

        public static Bitmap GetStadiumHeroImage() => GetStadiumHero();
        public static Bitmap GetLoginCleanBackground() => (GetImage("login_bg_clean.png") ?? GetImage("login_mockup.jpg")) as Bitmap;
        public static Bitmap GetLoginMockup() => GetImage("login_mockup.jpg") as Bitmap;
        public static Bitmap GetPitchImage() => GetFootballPitch();
        public static Bitmap GetBadmintonImage() => GetBadmintonCourt();
        public static Bitmap GetTennisImage() => GetTennisCourt();

        public static Image GetFieldImage(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName)) return GetFootballPitch();
            string lower = fieldName.ToLower();
            if (lower.Contains("cầu lông") || lower.Contains("badminton")) return GetBadmintonCourt();
            if (lower.Contains("tennis") || lower.Contains("quần vợt")) return GetTennisCourt();
            if (lower.Contains("pickleball")) return GetTennisCourt();
            return GetFootballPitch();
        }
    }
}
