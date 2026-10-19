using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SportZone.WinForms.Controls
{
    public enum SFIcon
    {
        None,
        Search,
        Plus,
        Edit,
        Trash,
        Eye,
        Check,
        Close,
        Calendar,
        Clock,
        Dollar,
        User,
        Users,
        Field,
        Chart,
        Filter,
        Refresh,
        Bell,
        Settings,
        Lock,
        QrCode,
        LogOut,
        Ticket,
        ChevronDown,
        ChevronRight,
        ChevronLeft,
        Printer,
        TrendingUp,
        AlertCircle,
        Info,
        Star,
        Menu
    }

    public static class VectorIcons
    {
        public static SportZone.WinForms.Helpers.SportFieldIcon? ToSportFieldIcon(SFIcon icon)
        {
            switch (icon)
            {
                case SFIcon.Search: return SportZone.WinForms.Helpers.SportFieldIcon.Search;
                case SFIcon.Plus: return SportZone.WinForms.Helpers.SportFieldIcon.Add;
                case SFIcon.Edit: return SportZone.WinForms.Helpers.SportFieldIcon.Edit;
                case SFIcon.Trash: return SportZone.WinForms.Helpers.SportFieldIcon.Delete;
                case SFIcon.Eye: return SportZone.WinForms.Helpers.SportFieldIcon.Eye;
                case SFIcon.Check: return SportZone.WinForms.Helpers.SportFieldIcon.Success;
                case SFIcon.Close: return SportZone.WinForms.Helpers.SportFieldIcon.Close;
                case SFIcon.Calendar: return SportZone.WinForms.Helpers.SportFieldIcon.Calendar;
                case SFIcon.Clock: return SportZone.WinForms.Helpers.SportFieldIcon.Clock;
                case SFIcon.Dollar: return SportZone.WinForms.Helpers.SportFieldIcon.Payment;
                case SFIcon.User: return SportZone.WinForms.Helpers.SportFieldIcon.User;
                case SFIcon.Users: return SportZone.WinForms.Helpers.SportFieldIcon.Users;
                case SFIcon.Field: return SportZone.WinForms.Helpers.SportFieldIcon.Field;
                case SFIcon.Chart: return SportZone.WinForms.Helpers.SportFieldIcon.Chart;
                case SFIcon.Refresh: return SportZone.WinForms.Helpers.SportFieldIcon.Refresh;
                case SFIcon.Bell: return SportZone.WinForms.Helpers.SportFieldIcon.Bell;
                case SFIcon.Settings: return SportZone.WinForms.Helpers.SportFieldIcon.Settings;
                case SFIcon.Lock: return SportZone.WinForms.Helpers.SportFieldIcon.Lock;
                case SFIcon.QrCode: return SportZone.WinForms.Helpers.SportFieldIcon.Vietqr;
                case SFIcon.LogOut: return SportZone.WinForms.Helpers.SportFieldIcon.Logout;
                case SFIcon.Ticket: return SportZone.WinForms.Helpers.SportFieldIcon.Voucher;
                case SFIcon.Printer: return SportZone.WinForms.Helpers.SportFieldIcon.Print;
                case SFIcon.TrendingUp: return SportZone.WinForms.Helpers.SportFieldIcon.Revenue;
                case SFIcon.AlertCircle: return SportZone.WinForms.Helpers.SportFieldIcon.Warning;
                case SFIcon.Info: return SportZone.WinForms.Helpers.SportFieldIcon.Info;
                case SFIcon.Star: return SportZone.WinForms.Helpers.SportFieldIcon.Star;
                default: return null;
            }
        }

        public static void Draw(Graphics g, SFIcon icon, Rectangle rect, Color color, float strokeWidth = 1.8f)
        {
            if (icon == SFIcon.None || rect.Width <= 2 || rect.Height <= 2) return;

            var sfIcon = ToSportFieldIcon(icon);
            if (sfIcon.HasValue)
            {
                try
                {
                    SportZone.WinForms.Helpers.SportFieldIconProvider.Draw(g, sfIcon.Value, rect, color);
                    return;
                }
                catch
                {
                    // Fallback to vector drawing if PNG drawing fails
                }
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (Pen pen = new Pen(color, strokeWidth))
            using (SolidBrush brush = new SolidBrush(color))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                pen.LineJoin = LineJoin.Round;

                float cx = rect.X + rect.Width / 2f;
                float cy = rect.Y + rect.Height / 2f;
                float s = Math.Min(rect.Width, rect.Height) * 0.45f;

                switch (icon)
                {
                    case SFIcon.Search:
                        float r = s * 0.65f;
                        g.DrawEllipse(pen, cx - r * 0.9f, cy - r * 0.9f, r * 1.5f, r * 1.5f);
                        g.DrawLine(pen, cx + r * 0.35f, cy + r * 0.35f, cx + s, cy + s);
                        break;

                    case SFIcon.Plus:
                        g.DrawLine(pen, cx - s, cy, cx + s, cy);
                        g.DrawLine(pen, cx, cy - s, cx, cy + s);
                        break;

                    case SFIcon.Check:
                        PointF[] checkPts = { new PointF(cx - s, cy), new PointF(cx - s * 0.2f, cy + s * 0.8f), new PointF(cx + s, cy - s * 0.7f) };
                        g.DrawLines(pen, checkPts);
                        break;

                    case SFIcon.Close:
                        g.DrawLine(pen, cx - s * 0.75f, cy - s * 0.75f, cx + s * 0.75f, cy + s * 0.75f);
                        g.DrawLine(pen, cx + s * 0.75f, cy - s * 0.75f, cx - s * 0.75f, cy + s * 0.75f);
                        break;

                    case SFIcon.ChevronDown:
                        g.DrawLines(pen, new[] { new PointF(cx - s * 0.7f, cy - s * 0.3f), new PointF(cx, cy + s * 0.4f), new PointF(cx + s * 0.7f, cy - s * 0.3f) });
                        break;

                    case SFIcon.ChevronRight:
                        g.DrawLines(pen, new[] { new PointF(cx - s * 0.3f, cy - s * 0.7f), new PointF(cx + s * 0.4f, cy), new PointF(cx - s * 0.3f, cy + s * 0.7f) });
                        break;

                    case SFIcon.ChevronLeft:
                        g.DrawLines(pen, new[] { new PointF(cx + s * 0.3f, cy - s * 0.7f), new PointF(cx - s * 0.4f, cy), new PointF(cx + s * 0.3f, cy + s * 0.7f) });
                        break;

                    case SFIcon.Calendar:
                        g.DrawRectangle(pen, cx - s, cy - s * 0.7f, s * 2, s * 1.6f);
                        g.DrawLine(pen, cx - s, cy - s * 0.2f, cx + s, cy - s * 0.2f);
                        g.DrawLine(pen, cx - s * 0.5f, cy - s, cx - s * 0.5f, cy - s * 0.5f);
                        g.DrawLine(pen, cx + s * 0.5f, cy - s, cx + s * 0.5f, cy - s * 0.5f);
                        break;

                    case SFIcon.Clock:
                        g.DrawEllipse(pen, cx - s, cy - s, s * 2, s * 2);
                        g.DrawLine(pen, cx, cy - s * 0.55f, cx, cy);
                        g.DrawLine(pen, cx, cy, cx + s * 0.45f, cy);
                        break;

                    case SFIcon.Dollar:
                        g.DrawEllipse(pen, cx - s, cy - s, s * 2, s * 2);
                        using (Font f = new Font("Segoe UI", s * 0.95f, FontStyle.Bold))
                        {
                            StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                            g.DrawString("đ", f, brush, new RectangleF(cx - s, cy - s, s * 2, s * 2), sf);
                        }
                        break;

                    case SFIcon.User:
                        g.DrawEllipse(pen, cx - s * 0.45f, cy - s * 0.85f, s * 0.9f, s * 0.9f);
                        using (GraphicsPath p = new GraphicsPath())
                        {
                            p.AddArc(cx - s * 0.85f, cy + s * 0.1f, s * 1.7f, s * 1.5f, 180, 180);
                            g.DrawPath(pen, p);
                        }
                        break;

                    case SFIcon.TrendingUp:
                        g.DrawLines(pen, new[] { new PointF(cx - s, cy + s * 0.5f), new PointF(cx - s * 0.3f, cy), new PointF(cx + s * 0.2f, cy + s * 0.3f), new PointF(cx + s, cy - s * 0.6f) });
                        g.DrawLine(pen, cx + s * 0.4f, cy - s * 0.6f, cx + s, cy - s * 0.6f);
                        g.DrawLine(pen, cx + s, cy - s * 0.6f, cx + s, cy);
                        break;

                    case SFIcon.Field:
                        g.DrawRectangle(pen, cx - s, cy - s * 0.7f, s * 2, s * 1.4f);
                        g.DrawLine(pen, cx, cy - s * 0.7f, cx, cy + s * 0.7f);
                        g.DrawEllipse(pen, cx - s * 0.35f, cy - s * 0.35f, s * 0.7f, s * 0.7f);
                        break;

                    case SFIcon.Chart:
                        g.DrawLine(pen, cx - s, cy + s * 0.8f, cx + s, cy + s * 0.8f);
                        g.DrawLine(pen, cx - s * 0.6f, cy + s * 0.8f, cx - s * 0.6f, cy - s * 0.2f);
                        g.DrawLine(pen, cx - s * 0.1f, cy + s * 0.8f, cx - s * 0.1f, cy - s * 0.7f);
                        g.DrawLine(pen, cx + s * 0.4f, cy + s * 0.8f, cx + s * 0.4f, cy + s * 0.1f);
                        break;

                    case SFIcon.Ticket:
                        g.DrawRectangle(pen, cx - s, cy - s * 0.6f, s * 2, s * 1.2f);
                        g.DrawLine(pen, cx - s * 0.2f, cy - s * 0.6f, cx - s * 0.2f, cy + s * 0.6f);
                        g.FillEllipse(brush, cx - s * 0.6f, cy - s * 0.15f, s * 0.3f, s * 0.3f);
                        break;

                    case SFIcon.QrCode:
                        g.DrawRectangle(pen, cx - s, cy - s, s * 2, s * 2);
                        g.DrawRectangle(pen, cx - s * 0.7f, cy - s * 0.7f, s * 0.5f, s * 0.5f);
                        g.DrawRectangle(pen, cx + s * 0.2f, cy - s * 0.7f, s * 0.5f, s * 0.5f);
                        g.DrawRectangle(pen, cx - s * 0.7f, cy + s * 0.2f, s * 0.5f, s * 0.5f);
                        break;

                    case SFIcon.Settings:
                        g.DrawEllipse(pen, cx - s * 0.45f, cy - s * 0.45f, s * 0.9f, s * 0.9f);
                        g.DrawEllipse(pen, cx - s, cy - s, s * 2, s * 2);
                        for (int i = 0; i < 4; i++)
                        {
                            double rad = i * Math.PI / 2;
                            float x1 = (float)(cx + Math.Cos(rad) * s * 0.6f);
                            float y1 = (float)(cy + Math.Sin(rad) * s * 0.6f);
                            float x2 = (float)(cx + Math.Cos(rad) * s * 1.1f);
                            float y2 = (float)(cy + Math.Sin(rad) * s * 1.1f);
                            g.DrawLine(pen, x1, y1, x2, y2);
                        }
                        break;

                    case SFIcon.Bell:
                        g.DrawArc(pen, cx - s * 0.7f, cy - s * 0.7f, s * 1.4f, s * 1.4f, 180, 180);
                        g.DrawLine(pen, cx - s * 0.7f, cy, cx - s * 0.9f, cy + s * 0.5f);
                        g.DrawLine(pen, cx + s * 0.7f, cy, cx + s * 0.9f, cy + s * 0.5f);
                        g.DrawLine(pen, cx - s * 0.9f, cy + s * 0.5f, cx + s * 0.9f, cy + s * 0.5f);
                        g.DrawLine(pen, cx - s * 0.25f, cy + s * 0.65f, cx + s * 0.25f, cy + s * 0.65f);
                        break;

                    case SFIcon.AlertCircle:
                        g.DrawEllipse(pen, cx - s, cy - s, s * 2, s * 2);
                        g.DrawLine(pen, cx, cy - s * 0.45f, cx, cy + s * 0.1f);
                        g.FillEllipse(brush, cx - pen.Width / 2f, cy + s * 0.35f, pen.Width, pen.Width);
                        break;

                    case SFIcon.Eye:
                        g.DrawArc(pen, cx - s, cy - s * 0.5f, s * 2, s, 0, 180);
                        g.DrawArc(pen, cx - s, cy - s * 0.5f, s * 2, s, 180, 180);
                        g.FillEllipse(brush, cx - s * 0.3f, cy - s * 0.3f, s * 0.6f, s * 0.6f);
                        break;

                    case SFIcon.Edit:
                        g.DrawLine(pen, cx - s * 0.8f, cy + s * 0.8f, cx - s * 0.8f, cy + s * 0.3f);
                        g.DrawLine(pen, cx - s * 0.8f, cy + s * 0.8f, cx - s * 0.3f, cy + s * 0.8f);
                        g.DrawLine(pen, cx - s * 0.4f, cy + s * 0.4f, cx + s * 0.6f, cy - s * 0.6f);
                        break;

                    case SFIcon.Trash:
                        g.DrawLine(pen, cx - s, cy - s * 0.5f, cx + s, cy - s * 0.5f);
                        g.DrawLine(pen, cx - s * 0.3f, cy - s * 0.8f, cx + s * 0.3f, cy - s * 0.8f);
                        g.DrawRectangle(pen, cx - s * 0.7f, cy - s * 0.5f, s * 1.4f, s * 1.3f);
                        g.DrawLine(pen, cx - s * 0.25f, cy - s * 0.2f, cx - s * 0.25f, cy + s * 0.5f);
                        g.DrawLine(pen, cx + s * 0.25f, cy - s * 0.2f, cx + s * 0.25f, cy + s * 0.5f);
                        break;

                    case SFIcon.Refresh:
                        g.DrawArc(pen, cx - s * 0.8f, cy - s * 0.8f, s * 1.6f, s * 1.6f, 40, 280);
                        g.DrawLine(pen, cx + s * 0.5f, cy - s * 0.8f, cx + s * 0.9f, cy - s * 0.5f);
                        g.DrawLine(pen, cx + s * 0.9f, cy - s * 0.5f, cx + s * 0.5f, cy - s * 0.2f);
                        break;

                    case SFIcon.Printer:
                        g.DrawRectangle(pen, cx - s * 0.7f, cy - s * 0.3f, s * 1.4f, s * 0.9f);
                        g.DrawRectangle(pen, cx - s * 0.45f, cy - s * 0.8f, s * 0.9f, s * 0.5f);
                        g.DrawRectangle(pen, cx - s * 0.45f, cy + s * 0.2f, s * 0.9f, s * 0.6f);
                        break;

                    case SFIcon.Star:
                        PointF[] starPts = new PointF[]
                        {
                            new PointF(cx, cy - s),
                            new PointF(cx + s * 0.3f, cy - s * 0.3f),
                            new PointF(cx + s, cy - s * 0.3f),
                            new PointF(cx + s * 0.45f, cy + s * 0.15f),
                            new PointF(cx + s * 0.65f, cy + s * 0.85f),
                            new PointF(cx, cy + s * 0.45f),
                            new PointF(cx - s * 0.65f, cy + s * 0.85f),
                            new PointF(cx - s * 0.45f, cy + s * 0.15f),
                            new PointF(cx - s, cy - s * 0.3f),
                            new PointF(cx - s * 0.3f, cy - s * 0.3f)
                        };
                        g.DrawPolygon(pen, starPts);
                        break;

                    case SFIcon.Menu:
                        g.DrawLine(pen, cx - s * 0.8f, cy - s * 0.55f, cx + s * 0.8f, cy - s * 0.55f);
                        g.DrawLine(pen, cx - s * 0.8f, cy, cx + s * 0.8f, cy);
                        g.DrawLine(pen, cx - s * 0.8f, cy + s * 0.55f, cx + s * 0.8f, cy + s * 0.55f);
                        break;

                    case SFIcon.Filter:
                        PointF[] fPts = new PointF[]
                        {
                            new PointF(cx - s * 0.8f, cy - s * 0.7f),
                            new PointF(cx + s * 0.8f, cy - s * 0.7f),
                            new PointF(cx + s * 0.25f, cy + s * 0.05f),
                            new PointF(cx + s * 0.25f, cy + s * 0.75f),
                            new PointF(cx - s * 0.25f, cy + s * 0.55f),
                            new PointF(cx - s * 0.25f, cy + s * 0.05f)
                        };
                        g.DrawPolygon(pen, fPts);
                        break;

                    case SFIcon.Users:
                        // Primary user (right)
                        g.DrawEllipse(pen, cx - s * 0.1f, cy - s * 0.85f, s * 0.75f, s * 0.75f);
                        using (GraphicsPath p1 = new GraphicsPath())
                        {
                            p1.AddArc(cx - s * 0.45f, cy, s * 1.45f, s * 1.3f, 180, 180);
                            g.DrawPath(pen, p1);
                        }
                        // Secondary user (left background)
                        g.DrawArc(pen, cx - s * 0.85f, cy - s * 0.75f, s * 0.65f, s * 0.65f, 90, 270);
                        using (GraphicsPath p2 = new GraphicsPath())
                        {
                            p2.AddArc(cx - s * 1.05f, cy + s * 0.15f, s * 1.1f, s * 1.1f, 180, 90);
                            g.DrawPath(pen, p2);
                        }
                        break;

                    case SFIcon.Lock:
                        // Shackle
                        g.DrawArc(pen, cx - s * 0.45f, cy - s * 0.85f, s * 0.9f, s * 0.9f, 180, 180);
                        g.DrawLine(pen, cx - s * 0.45f, cy - s * 0.4f, cx - s * 0.45f, cy - s * 0.1f);
                        g.DrawLine(pen, cx + s * 0.45f, cy - s * 0.4f, cx + s * 0.45f, cy - s * 0.1f);
                        // Lock Body
                        g.DrawRectangle(pen, cx - s * 0.65f, cy - s * 0.1f, s * 1.3f, s * 0.9f);
                        // Keyhole
                        g.FillEllipse(brush, cx - s * 0.15f, cy + s * 0.15f, s * 0.3f, s * 0.3f);
                        break;

                    case SFIcon.Info:
                        g.DrawEllipse(pen, cx - s, cy - s, s * 2, s * 2);
                        g.FillEllipse(brush, cx - pen.Width / 2f, cy - s * 0.5f, pen.Width, pen.Width);
                        g.DrawLine(pen, cx, cy - s * 0.1f, cx, cy + s * 0.5f);
                        break;

                    case SFIcon.LogOut:
                        // Door frame bracket
                        g.DrawLines(pen, new[]
                        {
                            new PointF(cx - s * 0.1f, cy - s * 0.8f),
                            new PointF(cx - s * 0.75f, cy - s * 0.8f),
                            new PointF(cx - s * 0.75f, cy + s * 0.8f),
                            new PointF(cx - s * 0.1f, cy + s * 0.8f)
                        });
                        // Arrow pointing out
                        g.DrawLine(pen, cx - s * 0.3f, cy, cx + s * 0.8f, cy);
                        g.DrawLine(pen, cx + s * 0.4f, cy - s * 0.4f, cx + s * 0.8f, cy);
                        g.DrawLine(pen, cx + s * 0.4f, cy + s * 0.4f, cx + s * 0.8f, cy);
                        break;

                    default:
                        g.DrawEllipse(pen, cx - s * 0.7f, cy - s * 0.7f, s * 1.4f, s * 1.4f);
                        break;
                }
            }
        }
    }
}
