using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SportZone.WinForms.Controls;
using SportZone.WinForms.DesignTokens;

namespace SportZone.WinForms
{
    public static class Ui
    {
        public static GraphicsPath RoundRect(Rectangle bounds, int radius)
        {
            return GraphicsUtility.CreateRoundedRectangle(bounds, radius);
        }

        public static Label Label(string text, float size, FontStyle style, Color color)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font("Segoe UI", size, style),
                ForeColor = color,
                BackColor = Color.Transparent,
                UseMnemonic = false
            };
        }

        public static Label Label(string text)
        {
            return Label(text, 9.5f, FontStyle.Regular, Colors.TextPrimary);
        }

        public static AccentButton Primary(string text)
        {
            return new AccentButton
            {
                Text = text,
                Variant = SFButtonVariant.Primary,
                Height = Metrics.ButtonHeight
            };
        }

        public static Button Secondary(string text)
        {
            return new SFButton
            {
                Text = text,
                Variant = SFButtonVariant.Secondary,
                Height = Metrics.ButtonHeight
            };
        }

        public static Button DangerButton(string text)
        {
            return new SFButton
            {
                Text = text,
                Variant = SFButtonVariant.Danger,
                Height = Metrics.ButtonHeight
            };
        }

        public static TextBox TextBox(string placeholder)
        {
            TextBox t = new TextBox
            {
                Text = placeholder,
                Font = Typography.Body,
                BorderStyle = BorderStyle.FixedSingle,
                ForeColor = Colors.TextSecondary,
                Height = 34
            };
            t.GotFocus += delegate { if (t.Text == placeholder) { t.Text = ""; t.ForeColor = Colors.TextPrimary; } };
            t.LostFocus += delegate { if (string.IsNullOrWhiteSpace(t.Text)) { t.Text = placeholder; t.ForeColor = Colors.TextSecondary; } };
            return t;
        }

        public static ComboBox Combo(params string[] items)
        {
            SFComboBox c = new SFComboBox { Height = Metrics.InputHeight };
            if (items != null) c.Items.AddRange(items);
            if (c.Items.Count > 0) c.SelectedIndex = 0;
            return c;
        }

        public static Panel Header(string title, string subtitle)
        {
            Panel p = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = Colors.AppBackground,
                Padding = new Padding(0, 0, 0, 14)
            };

            Label t = Label(title, 17f, FontStyle.Bold, Colors.TextPrimary);
            t.Location = new Point(0, 2);

            Label s = Label(subtitle, 9f, FontStyle.Regular, Colors.TextSecondary);
            s.Location = new Point(1, 34);

            p.Controls.Add(t);
            p.Controls.Add(s);
            return p;
        }

        public static CardPanel Kpi(string title, string value, string foot, Color accent)
        {
            CardPanel c = new CardPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 14, 0),
                Padding = new Padding(16, 12, 16, 12)
            };

            // Top accent indicator line
            Panel stripe = new Panel { Dock = DockStyle.Top, Height = 3, BackColor = accent };
            c.Controls.Add(stripe);

            Label t = Label(title.ToUpper(), 8f, FontStyle.Bold, Colors.TextSecondary);
            t.Location = new Point(16, 16);
            c.Controls.Add(t);

            Label v = Label(value, 20f, FontStyle.Bold, Colors.TextPrimary);
            v.Location = new Point(16, 38);
            c.Controls.Add(v);

            bool isPositive = !foot.Contains("▼") && !foot.Contains("-");
            BadgeLabel f = new BadgeLabel
            {
                Text = foot,
                BadgeFore = isPositive ? Colors.DeepEmerald : Colors.Danger,
                BadgeBack = isPositive ? Colors.SuccessBg : Colors.DangerBg
            };
            f.Location = new Point(16, 76);
            c.Controls.Add(f);

            return c;
        }

        public static CardPanel MiniKpi(string title, string value, string foot, Color accent)
        {
            CardPanel c = new CardPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 12, 0),
                Padding = new Padding(14)
            };

            Label t = Label(title.ToUpper(), 8f, FontStyle.Bold, Colors.TextSecondary);
            t.Location = new Point(14, 14);
            c.Controls.Add(t);

            Label v = Label(value, 16.5f, FontStyle.Bold, accent);
            v.Location = new Point(14, 34);
            c.Controls.Add(v);

            Label f = Label(foot, 8.2f, FontStyle.Regular, Colors.TextMuted);
            f.Location = new Point(14, 66);
            c.Controls.Add(f);

            return c;
        }

        public static Label Badge(string text, Color fore, Color back)
        {
            return new BadgeLabel { Text = text, BadgeFore = fore, BadgeBack = back };
        }

        public static DataGridView Grid(string[] cols, string[][] rows)
        {
            SFDataGridView g = new SFDataGridView();
            if (cols != null)
            {
                for (int i = 0; i < cols.Length; i++)
                {
                    g.Columns.Add("C" + i, cols[i]);
                }
            }

            if (rows != null)
            {
                foreach (string[] row in rows)
                {
                    g.Rows.Add(row);
                }
            }
            return g;
        }

        public static CardPanel Section(string title)
        {
            CardPanel c = new CardPanel { Dock = DockStyle.Fill };
            Panel head = new Panel { Dock = DockStyle.Top, Height = 32, BackColor = Color.Transparent };
            Label t = Label(title, 11f, FontStyle.Bold, Colors.TextPrimary);
            t.Location = new Point(0, 4);
            head.Controls.Add(t);
            c.Controls.Add(head);
            return c;
        }

        public static FlowLayoutPanel Toolbar()
        {
            return new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 56,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 2, 0, 10),
                BackColor = Colors.AppBackground
            };
        }

        public static Panel Notice(string title, string body, Color accent, Color background)
        {
            Panel p = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = background,
                Padding = new Padding(16, 10, 16, 10)
            };

            Panel stripe = new Panel { Dock = DockStyle.Left, Width = 4, BackColor = accent };
            p.Controls.Add(stripe);

            Label t = Label(title, 9.2f, FontStyle.Bold, accent);
            t.Location = new Point(16, 8);

            Label b = Label(body, 8.6f, FontStyle.Regular, Colors.TextSecondary);
            b.Location = new Point(16, 30);

            p.Controls.Add(t);
            p.Controls.Add(b);
            return p;
        }

        public static void Toast(IWin32Window owner, string text)
        {
            Form f = owner as Form;
            if (f == null && owner is Control c) f = c.FindForm();
            SFToast.Show(f, text, "SPORTFIELD", SFIcon.Check, Colors.Emerald);
        }
    }
}
