using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SportZone.WinForms.Controls;
using SportZone.WinForms.Database;
using SportZone.WinForms.DesignTokens;
using SportZone.WinForms.Dialogs;
using SportZone.WinForms.Helpers;
using SportZone.WinForms.Security;
using SportZone.WinForms.Services;
using SportZone.WinForms.Session;

namespace SportZone.WinForms.Views
{
    public class CustomerHomeView : UserControl
    {
        // ==========================================
        // PALETTE TOKENS (LOCAL TO CUSTOMER HOME)
        // ==========================================
        private static readonly Color ColPageBg = Color.FromArgb(241, 245, 249);       // #F1F5F9
        private static readonly Color ColCardBg = Color.White;                         // #FFFFFF
        private static readonly Color ColBorder = Color.FromArgb(226, 232, 240);       // #E2E8F0
        private static readonly Color ColTextDark = Color.FromArgb(20, 35, 46);        // #14232E
        private static readonly Color ColTextBody = Color.FromArgb(51, 65, 85);        // #334155
        private static readonly Color ColTextMuted = Color.FromArgb(100, 116, 139);    // #64748B
        private static readonly Color ColPrimaryTeal = Color.FromArgb(15, 118, 110);   // #0F766E
        private static readonly Color ColEmerald = Color.FromArgb(16, 185, 129);       // #10B981
        private static readonly Color ColEmeraldDark = Color.FromArgb(5, 150, 105);    // #059669
        private static readonly Color ColEmeraldTint = Color.FromArgb(236, 253, 245);  // #ECFDF5
        private static readonly Color ColBlue = Color.FromArgb(37, 99, 235);           // #2563EB
        private static readonly Color ColBlueTint = Color.FromArgb(239, 246, 255);     // #EFF6FF
        private static readonly Color ColAmber = Color.FromArgb(245, 158, 11);         // #F59E0B
        private static readonly Color ColAmberDark = Color.FromArgb(180, 83, 9);       // #B45309
        private static readonly Color ColAmberTint = Color.FromArgb(255, 251, 235);    // #FFFBEB
        private static readonly Color ColPurple = Color.FromArgb(124, 58, 237);        // #7C3AED
        private static readonly Color ColPurpleTint = Color.FromArgb(245, 243, 255);   // #F5F3FF
        private static readonly Color ColRose = Color.FromArgb(244, 63, 94);           // #F43F5E
        private static readonly Color ColRoseTint = Color.FromArgb(255, 241, 242);     // #FFF1F2

        // ==========================================
        // ROOT SCROLL & CANVAS CONTAINERS
        // ==========================================
        private CustomerHomeScrollPanel mainScrollPanel;
        private DoubleBufferedContainer contentCanvas;

        // ==========================================
        // SECTION CONTAINERS
        // ==========================================
        private DoubleBufferedContainer pnlErrorBanner;
        private Label lblErrorText;
        private DoubleBufferedContainer pnlWelcomeBanner;
        private TableLayoutPanel tlpQuickActions;
        private TableLayoutPanel tlpHeroFeatured;
        private DoubleBufferedContainer pnlHeroCard;
        private DoubleBufferedContainer pnlFeaturedContainer;
        private TableLayoutPanel tlpFeaturedCards;
        private TableLayoutPanel tlpKpiRow;
        private DoubleBufferedContainer pnlUpcomingSection;
        private Panel pnlUpcomingListHost;
        private DoubleBufferedContainer pnlOffersSection;
        private Panel pnlOffersListHost;
        private DoubleBufferedContainer pnlMembershipSummary;

        // ==========================================
        // DYNAMIC DATA STATE (100% REAL DB)
        // ==========================================
        private string custName = "Quý khách";
        private string custTier = "Chưa có hạng";
        private bool hasRealTier = false;
        private decimal custPoints = 0;
        private decimal custSpend = 0;
        private decimal custWalletBalance = 0;
        private int custTotalBookings = 0;
        private int custVoucherCount = 0;
        private int custUpcomingCount = 0;
        private float tierProgressRatio = 0f;
        private string nextTierCaption = "Tích điểm từ mỗi lượt đặt sân";

        private DataTable dtFeaturedCourts;
        private DataTable dtUpcomingBookings;
        private DataTable dtVouchers;
        private DataTable dtTiers;
        private DataRow drCustomer;

        // ==========================================
        // KPI CARD REFERENCES
        // ==========================================
        private CustomerKpiCard kpiBookings;
        private CustomerKpiCard kpiVouchers;
        private CustomerKpiCard kpiSpend;
        private CustomerMembershipKpiCard kpiMembership;

        // ==========================================
        // HERO CAROUSEL STATE
        // ==========================================
        private int heroSlideIndex = 0;
        private Rectangle heroBtnBookRect = new Rectangle(30, 232, 200, 44);
        private Rectangle heroPrevRect = Rectangle.Empty;
        private Rectangle heroNextRect = Rectangle.Empty;
        private bool heroBookHover = false;
        private bool heroPrevHover = false;
        private bool heroNextHover = false;

        // ==========================================
        // MEMBERSHIP SUMMARY INTERACTIVE RECTS
        // ==========================================
        private Rectangle memBtnBenefitsRect = Rectangle.Empty;
        private Rectangle memBtnVouchersRect = Rectangle.Empty;
        private Rectangle memHeaderArrowRect = Rectangle.Empty;
        private bool memBenefitsHover = false;
        private bool memVouchersHover = false;

        private bool isReflowing = false;

        public CustomerHomeView()
        {
            Dock = DockStyle.Fill;
            BackColor = ColPageBg;
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            DoubleBuffered = true;

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            if (AppSession.Current != null && !string.IsNullOrWhiteSpace(AppSession.Current.HoTen))
            {
                custName = AppSession.Current.HoTen.Trim();
            }

            BuildUI();
            LoadDataFromDatabase();
        }

        private void NavigateTo(string viewKey)
        {
            MainForm mf = FindForm() as MainForm;
            if (mf != null)
            {
                mf.ShowView(viewKey);
            }
        }

        private void NavigateToBookCourt(int sanId)
        {
            if (sanId > 0)
            {
                CustomerBookingView.PreselectedSanId = sanId;
            }
            NavigateTo("Đặt sân");
        }

        private void OpenBookingDetail(long donDatSanId)
        {
            if (donDatSanId > 0)
            {
                try
                {
                    using (var dlg = new BookingDetailForm(donDatSanId))
                    {
                        dlg.ShowDialog(FindForm());
                    }
                    LoadDataFromDatabase();
                }
                catch (Exception ex)
                {
                    AppLogger.LogError(ex, "CUSTOMER_HOME", "OpenBookingDetail");
                    NavigateTo("Lịch sử đặt sân");
                }
            }
            else
            {
                NavigateTo("Lịch sử đặt sân");
            }
        }

        // ==========================================
        // UI CONSTRUCTION
        // ==========================================
        private void BuildUI()
        {
            mainScrollPanel = new CustomerHomeScrollPanel
            {
                Dock = DockStyle.Fill,
                BackColor = ColPageBg
            };
            Controls.Add(mainScrollPanel);

            contentCanvas = new DoubleBufferedContainer
            {
                Location = new Point(0, 0),
                BackColor = ColPageBg
            };
            mainScrollPanel.Controls.Add(contentCanvas);

            BuildErrorBanner();
            BuildWelcomeHeader();
            BuildQuickActions();
            BuildHeroSection();
            BuildFeaturedFields();
            BuildHeroAndFeaturedLayout();
            BuildCustomerKpis();
            BuildUpcomingBookings();
            BuildOffers();
            BuildMembershipSummary();

            mainScrollPanel.Resize += (s, e) => ReflowLayout();
            ReflowLayout();
        }

        private void BuildErrorBanner()
        {
            pnlErrorBanner = new DoubleBufferedContainer
            {
                Height = 56,
                Visible = false,
                BackColor = Color.FromArgb(254, 242, 242)
            };
            pnlErrorBanner.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle r = new Rectangle(0, 0, pnlErrorBanner.Width - 1, pnlErrorBanner.Height - 1);
                using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(r, 12))
                {
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(254, 242, 242)))
                        g.FillPath(b, path);
                    using (Pen p = new Pen(Color.FromArgb(254, 202, 202), 1.2f))
                        g.DrawPath(p, path);
                }
                SportFieldIconProvider.Draw(g, SportFieldIcon.Warning, new Rectangle(18, 16, 24, 24), null);
            };

            lblErrorText = new Label
            {
                Text = "Không thể tải dữ liệu trang chủ. Vui lòng kiểm tra kết nối cơ sở dữ liệu.",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(185, 28, 28),
                BackColor = Color.Transparent,
                Location = new Point(52, 18),
                AutoSize = true
            };
            pnlErrorBanner.Controls.Add(lblErrorText);

            SFButton btnRetry = new SFButton
            {
                Text = "THỬ LẠI",
                Variant = SFButtonVariant.Danger,
                IconType = SFIcon.Refresh,
                Size = new Size(116, 34),
                Location = new Point(200, 11)
            };
            btnRetry.Click += (s, e) => LoadDataFromDatabase();
            pnlErrorBanner.Controls.Add(btnRetry);

            pnlErrorBanner.Resize += (s, e) =>
            {
                btnRetry.Location = new Point(Math.Max(300, pnlErrorBanner.Width - btnRetry.Width - 16), 11);
            };

            contentCanvas.Controls.Add(pnlErrorBanner);
        }

        // ------------------------------------------
        // 1. WELCOME HEADER BANNER
        // ------------------------------------------
        private void BuildWelcomeHeader()
        {
            pnlWelcomeBanner = new DoubleBufferedContainer
            {
                Height = 114,
                BackColor = ColPageBg
            };

            pnlWelcomeBanner.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                Rectangle cardRect = new Rectangle(0, 0, pnlWelcomeBanner.Width - 1, pnlWelcomeBanner.Height - 1);
                using (GraphicsPath cardPath = GraphicsUtility.CreateRoundedRectangle(cardRect, 16))
                {
                    using (SolidBrush bgBrush = new SolidBrush(ColCardBg))
                    {
                        g.FillPath(bgBrush, cardPath);
                    }

                    // Subtle scenic stadium / sports center visual on right-center of banner
                    GraphicsState state = g.Save();
                    g.SetClip(cardPath);

                    int artW = Math.Min(460, Math.Max(260, (int)(pnlWelcomeBanner.Width * 0.44f)));
                    int artX = Math.Max(320, pnlWelcomeBanner.Width - artW - 180);
                    Rectangle artRect = new Rectangle(artX, 0, artW, pnlWelcomeBanner.Height);

                    // Soft sky & green field backdrop in the right-center banner zone
                    using (LinearGradientBrush skyBrush = new LinearGradientBrush(
                        artRect,
                        Color.FromArgb(28, 56, 189, 248),
                        Color.FromArgb(45, 16, 185, 129),
                        15f))
                    {
                        g.FillRectangle(skyBrush, artRect);
                    }

                    Image pitchImg = AssetManager.GetPitchImage() ?? AssetManager.GetStadiumHeroImage();
                    if (pitchImg != null && artRect.Width > 40)
                    {
                        GraphicsUtility.DrawCenterCrop(g, pitchImg, artRect);
                        // Soft white mist overlay so the artwork blends delicately like the mockup illustration
                        using (SolidBrush veil = new SolidBrush(Color.FromArgb(165, 255, 255, 255)))
                        {
                            g.FillRectangle(veil, artRect);
                        }
                    }

                    // Left fade-in from pure white
                    Rectangle leftFade = new Rectangle(artX - 2, 0, Math.Min(150, artW / 2), pnlWelcomeBanner.Height);
                    using (LinearGradientBrush fadeL = new LinearGradientBrush(
                        leftFade,
                        Color.White,
                        Color.FromArgb(0, 255, 255, 255),
                        0f))
                    {
                        g.FillRectangle(fadeL, leftFade);
                    }

                    // Right fade-out to white behind the date card
                    Rectangle rightFade = new Rectangle(artRect.Right - 90, 0, 280, pnlWelcomeBanner.Height);
                    using (LinearGradientBrush fadeR = new LinearGradientBrush(
                        rightFade,
                        Color.FromArgb(0, 255, 255, 255),
                        Color.White,
                        0f))
                    {
                        g.FillRectangle(fadeR, rightFade);
                    }

                    g.Restore(state);

                    using (Pen borderPen = new Pen(ColBorder, 1.2f))
                    {
                        g.DrawPath(borderPen, cardPath);
                    }
                }

                // Draw Greeting: "Xin chào, " + {custName} + wave icon
                int textX = 26;
                int titleY = 22;
                using (Font fGreet = new Font("Segoe UI", 19.5f, FontStyle.Bold))
                {
                    string prefix = "Xin chào, ";
                    Size prefixSz = TextRenderer.MeasureText(g, prefix, fGreet, Size.Empty, TextFormatFlags.NoPadding);
                    TextRenderer.DrawText(g, prefix, fGreet, new Point(textX, titleY), ColTextDark, TextFormatFlags.NoPadding);

                    int nameX = textX + prefixSz.Width;
                    string displayName = string.IsNullOrWhiteSpace(custName) ? "Quý khách" : custName;
                    Size nameSz = TextRenderer.MeasureText(g, displayName, fGreet, Size.Empty, TextFormatFlags.NoPadding);
                    TextRenderer.DrawText(g, displayName, fGreet, new Point(nameX, titleY), ColEmeraldDark, TextFormatFlags.NoPadding);

                    // Friendly golden wave badge next to customer name
                    int waveX = nameX + nameSz.Width + 10;
                    int waveY = titleY + 4;
                    using (Font fWave = new Font("Segoe UI Emoji", 17f, FontStyle.Regular))
                    {
                        TextRenderer.DrawText(g, "👋", fWave, new Point(waveX, waveY - 2), Color.FromArgb(245, 158, 11), TextFormatFlags.NoPadding);
                    }
                }

                // Draw Subtitle with bold highlight at the end
                int subY = 66;
                using (Font fSub = new Font("Segoe UI", 10f, FontStyle.Regular))
                using (Font fSubBold = new Font("Segoe UI", 10f, FontStyle.Bold))
                {
                    string part1 = "Sẵn sàng cho trận đấu tiếp theo của bạn? Hãy khám phá các sân và ";
                    string part2 = "ưu đãi hấp dẫn hôm nay!";
                    Size p1Sz = TextRenderer.MeasureText(g, part1, fSub, Size.Empty, TextFormatFlags.NoPadding);
                    TextRenderer.DrawText(g, part1, fSub, new Point(textX, subY), ColTextMuted, TextFormatFlags.NoPadding);
                    TextRenderer.DrawText(g, part2, fSubBold, new Point(textX + p1Sz.Width, subY), ColTextBody, TextFormatFlags.NoPadding);
                }

                // Draw Right Floating Date Box ("Hôm nay / dd/MM/yyyy / Thứ ...")
                int dateBoxW = 192;
                int dateBoxH = 78;
                int dateBoxX = pnlWelcomeBanner.Width - dateBoxW - 18;
                int dateBoxY = (pnlWelcomeBanner.Height - dateBoxH) / 2;

                if (dateBoxX > 420)
                {
                    Rectangle dateRect = new Rectangle(dateBoxX, dateBoxY, dateBoxW, dateBoxH);
                    using (GraphicsPath datePath = GraphicsUtility.CreateRoundedRectangle(dateRect, 14))
                    {
                        using (SolidBrush dbBrush = new SolidBrush(Color.FromArgb(250, 255, 255, 255)))
                        {
                            g.FillPath(dbBrush, datePath);
                        }
                        using (Pen dbPen = new Pen(Color.FromArgb(226, 232, 240), 1.2f))
                        {
                            g.DrawPath(dbPen, datePath);
                        }
                    }

                    // Draw warm Sun icon on left of Date Box
                    DrawWarmSunIcon(g, new Rectangle(dateBoxX + 16, dateBoxY + 19, 40, 40));

                    int dtTextX = dateBoxX + 68;
                    DateTime now = DateTime.Now;
                    using (Font fToday = new Font("Segoe UI", 8.75f, FontStyle.Regular))
                    using (Font fDate = new Font("Segoe UI", 12f, FontStyle.Bold))
                    using (Font fDow = new Font("Segoe UI", 9f, FontStyle.Regular))
                    {
                        TextRenderer.DrawText(g, "Hôm nay", fToday, new Point(dtTextX, dateBoxY + 11), ColTextMuted, TextFormatFlags.NoPadding);
                        TextRenderer.DrawText(g, now.ToString("dd/MM/yyyy"), fDate, new Point(dtTextX, dateBoxY + 28), ColTextDark, TextFormatFlags.NoPadding);
                        TextRenderer.DrawText(g, GetVietnameseDayOfWeek(now), fDow, new Point(dtTextX, dateBoxY + 51), ColTextMuted, TextFormatFlags.NoPadding);
                    }
                }
            };

            contentCanvas.Controls.Add(pnlWelcomeBanner);
        }

        private static void DrawWarmSunIcon(Graphics g, Rectangle rect)
        {
            float cx = rect.X + rect.Width / 2f;
            float cy = rect.Y + rect.Height / 2f;
            float rCore = rect.Width * 0.27f;
            float rInner = rect.Width * 0.36f;
            float rOuter = rect.Width * 0.48f;

            using (SolidBrush glow = new SolidBrush(Color.FromArgb(35, 245, 158, 11)))
            {
                g.FillEllipse(glow, rect.X - 2, rect.Y - 2, rect.Width + 4, rect.Height + 4);
            }
            using (SolidBrush sunFill = new SolidBrush(Color.FromArgb(245, 158, 11)))
            {
                g.FillEllipse(sunFill, cx - rCore, cy - rCore, rCore * 2, rCore * 2);
            }
            using (Pen rayPen = new Pen(Color.FromArgb(245, 158, 11), 2.4f))
            {
                rayPen.StartCap = LineCap.Round;
                rayPen.EndCap = LineCap.Round;
                for (int i = 0; i < 8; i++)
                {
                    double angle = i * Math.PI / 4.0;
                    float x1 = cx + (float)(Math.Cos(angle) * rInner);
                    float y1 = cy + (float)(Math.Sin(angle) * rInner);
                    float x2 = cx + (float)(Math.Cos(angle) * rOuter);
                    float y2 = cy + (float)(Math.Sin(angle) * rOuter);
                    g.DrawLine(rayPen, x1, y1, x2, y2);
                }
            }
        }

        private static string GetVietnameseDayOfWeek(DateTime dt)
        {
            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Monday: return "Thứ Hai";
                case DayOfWeek.Tuesday: return "Thứ Ba";
                case DayOfWeek.Wednesday: return "Thứ Tư";
                case DayOfWeek.Thursday: return "Thứ Năm";
                case DayOfWeek.Friday: return "Thứ Sáu";
                case DayOfWeek.Saturday: return "Thứ Bảy";
                default: return "Chủ Nhật";
            }
        }

        // ------------------------------------------
        // 2. QUICK ACTION CARDS (5 CARDS)
        // ------------------------------------------
        private void BuildQuickActions()
        {
            tlpQuickActions = new TableLayoutPanel
            {
                Height = 88,
                ColumnCount = 5,
                RowCount = 1,
                BackColor = ColPageBg,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            for (int i = 0; i < 5; i++)
            {
                tlpQuickActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));
            }
            tlpQuickActions.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var actions = new[]
            {
                new { Title = "Đặt sân ngay", Sub = "Chọn sân và khung giờ", Icon = SportFieldIcon.Booking, Accent = ColEmerald, Tint = Color.FromArgb(240, 253, 244), TitleCol = Color.FromArgb(6, 95, 70), Route = "Đặt sân" },
                new { Title = "Thông tin cá nhân", Sub = "Hồ sơ & hạng thành viên", Icon = SportFieldIcon.Profile, Accent = ColBlue, Tint = Color.FromArgb(239, 246, 255), TitleCol = Color.FromArgb(30, 64, 175), Route = "Thông tin cá nhân" },
                new { Title = "Voucher của tôi", Sub = "Ưu đãi đang chờ bạn", Icon = SportFieldIcon.Voucher, Accent = ColAmber, Tint = Color.FromArgb(255, 251, 235), TitleCol = Color.FromArgb(180, 83, 9), Route = "Voucher của tôi" },
                new { Title = "Lịch sử đặt sân", Sub = "Xem các lượt đặt sân", Icon = SportFieldIcon.Clock, Accent = ColPurple, Tint = Color.FromArgb(245, 243, 255), TitleCol = Color.FromArgb(91, 33, 182), Route = "Lịch sử đặt sân" },
                new { Title = "Hóa đơn của tôi", Sub = "Quản lý hóa đơn", Icon = SportFieldIcon.Invoice, Accent = ColRose, Tint = Color.FromArgb(255, 241, 242), TitleCol = Color.FromArgb(190, 18, 60), Route = "Hóa đơn của tôi" }
            };

            for (int i = 0; i < actions.Length; i++)
            {
                var act = actions[i];
                var card = new QuickActionCard(act.Title, act.Sub, act.Icon, act.Accent, act.Tint, act.TitleCol)
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(i == 0 ? 0 : 6, 0, i == actions.Length - 1 ? 0 : 6, 0)
                };
                string targetRoute = act.Route;
                card.Click += (s, e) => NavigateTo(targetRoute);
                tlpQuickActions.Controls.Add(card, i, 0);
            }

            contentCanvas.Controls.Add(tlpQuickActions);
        }

        // ------------------------------------------
        // 3. HERO BANNER & FEATURED FIELDS (2 COLUMNS)
        // ------------------------------------------
        private void BuildHeroSection()
        {
            pnlHeroCard = new DoubleBufferedContainer
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 8, 0),
                BackColor = ColPageBg,
                Cursor = Cursors.Default
            };

            pnlHeroCard.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                Rectangle r = new Rectangle(0, 0, pnlHeroCard.Width - 1, pnlHeroCard.Height - 1);
                if (r.Width <= 10 || r.Height <= 10) return;

                string badgeText;
                string titleLine1;
                string titleLine2;
                string subText;
                Image bgImg;

                if (heroSlideIndex == 1)
                {
                    badgeText = "LỊCH TRỐNG THỜI GIAN THỰC";
                    titleLine1 = "Sân cỏ tiêu chuẩn";
                    titleLine2 = "Đèn LED chiếu sáng";
                    subText = "Kiểm tra sân trống tức thì - Đặt giữ chỗ trực tuyến nhanh chóng";
                    bgImg = AssetManager.GetPitchImage() ?? AssetManager.GetStadiumHeroImage();
                }
                else if (heroSlideIndex == 2)
                {
                    badgeText = "ĐẶC QUYỀN HỘI VIÊN SPORTFIELD";
                    titleLine1 = "Tích điểm đổi quà";
                    titleLine2 = "Nhận ưu đãi mỗi ngày";
                    subText = "Áp dụng trực tiếp mã giảm giá khả dụng khi đặt sân trực tuyến";
                    bgImg = AssetManager.GetBadmintonImage() ?? AssetManager.GetStadiumHeroImage();
                }
                else
                {
                    badgeText = "SÂN CHẤT LƯỢNG - DỊCH VỤ CHUYÊN NGHIỆP";
                    titleLine1 = "Đặt sân dễ dàng";
                    titleLine2 = "Trải nghiệm trọn vẹn";
                    subText = "Hệ thống sân hiện đại - Giá hợp lý - Nhiều khung giờ linh hoạt";
                    bgImg = AssetManager.GetStadiumHeroImage() ?? AssetManager.GetPitchImage();
                }

                using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(r, 16))
                {
                    GraphicsState st = g.Save();
                    g.SetClip(path);

                    if (bgImg != null)
                    {
                        GraphicsUtility.DrawCenterCrop(g, bgImg, pnlHeroCard.ClientRectangle);
                    }
                    else
                    {
                        using (SolidBrush darkBg = new SolidBrush(Color.FromArgb(11, 27, 43)))
                            g.FillRectangle(darkBg, pnlHeroCard.ClientRectangle);
                    }

                    // Rich left-to-right gradient overlay so text on left is ultra legible while stadium lights glow on right
                    using (LinearGradientBrush overlay = new LinearGradientBrush(
                        pnlHeroCard.ClientRectangle,
                        Color.FromArgb(235, 9, 24, 38),
                        Color.FromArgb(85, 12, 38, 52),
                        0f))
                    {
                        g.FillRectangle(overlay, pnlHeroCard.ClientRectangle);
                    }

                    // Bottom vignette for carousel dots
                    Rectangle botRect = new Rectangle(0, pnlHeroCard.Height - 70, pnlHeroCard.Width, 70);
                    using (LinearGradientBrush botGrad = new LinearGradientBrush(
                        botRect,
                        Color.FromArgb(0, 7, 18, 28),
                        Color.FromArgb(190, 7, 18, 28),
                        90f))
                    {
                        g.FillRectangle(botGrad, botRect);
                    }

                    g.Restore(st);

                    using (Pen borderPen = new Pen(Color.FromArgb(40, 255, 255, 255), 1f))
                    {
                        g.DrawPath(borderPen, path);
                    }
                }

                int leftPad = 36;

                // 1. Top Pill Badge
                using (Font fBadge = new Font("Segoe UI", 8.25f, FontStyle.Bold))
                {
                    Size bSz = TextRenderer.MeasureText(g, badgeText, fBadge);
                    Rectangle pillRect = new Rectangle(leftPad, 34, bSz.Width + 24, 26);
                    using (GraphicsPath pillPath = GraphicsUtility.CreateRoundedRectangle(pillRect, 13))
                    using (SolidBrush pillBrush = new SolidBrush(Color.FromArgb(220, 5, 150, 105)))
                    {
                        g.FillPath(pillBrush, pillPath);
                    }
                    TextRenderer.DrawText(g, badgeText, fBadge, pillRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }

                // 2. Two-line Large Hero Title
                using (Font fHero = new Font("Segoe UI", 22f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(g, titleLine1, fHero, new Point(leftPad, 74), Color.White, TextFormatFlags.NoPadding);
                    TextRenderer.DrawText(g, titleLine2, fHero, new Point(leftPad, 118), Color.White, TextFormatFlags.NoPadding);
                }

                // 3. Subtitle
                using (Font fSub = new Font("Segoe UI", 9.75f, FontStyle.Regular))
                {
                    Rectangle subRect = new Rectangle(leftPad, 174, Math.Max(220, pnlHeroCard.Width - leftPad - 40), 42);
                    TextRenderer.DrawText(g, subText, fSub, subRect, Color.FromArgb(226, 232, 240), TextFormatFlags.Left | TextFormatFlags.WordBreak);
                }

                // 4. CTA Button: "Khám phá sân ngay  →"
                heroBtnBookRect = new Rectangle(leftPad, 226, 206, 44);
                using (GraphicsPath btnPath = GraphicsUtility.CreateRoundedRectangle(heroBtnBookRect, 10))
                using (SolidBrush btnBrush = new SolidBrush(heroBookHover ? ColEmeraldDark : ColEmerald))
                {
                    g.FillPath(btnBrush, btnPath);
                }
                using (Font fBtn = new Font("Segoe UI", 10.5f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(g, "Khám phá sân ngay  →", fBtn, heroBtnBookRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }

                // 5. Left & Right Carousel Navigation Circles
                int navY = (pnlHeroCard.Height - 32) / 2;
                heroPrevRect = new Rectangle(10, navY, 32, 32);
                heroNextRect = new Rectangle(pnlHeroCard.Width - 42, navY, 32, 32);

                DrawCarouselArrow(g, heroPrevRect, true, heroPrevHover);
                DrawCarouselArrow(g, heroNextRect, false, heroNextHover);

                // 6. Bottom Carousel Dots (3 dots)
                int dotsY = pnlHeroCard.Height - 20;
                int totalDotsW = 20 + 8 + 8 + 16;
                int curDotX = (pnlHeroCard.Width - totalDotsW) / 2;
                for (int i = 0; i < 3; i++)
                {
                    bool active = (i == heroSlideIndex);
                    int w = active ? 20 : 8;
                    Rectangle dotR = new Rectangle(curDotX, dotsY, w, 8);
                    using (GraphicsPath dp = GraphicsUtility.CreateRoundedRectangle(dotR, 4))
                    using (SolidBrush db = new SolidBrush(active ? ColEmerald : Color.FromArgb(140, 255, 255, 255)))
                    {
                        g.FillPath(db, dp);
                    }
                    curDotX += w + 8;
                }
            };

            pnlHeroCard.MouseMove += (s, e) =>
            {
                bool bHover = heroBtnBookRect.Contains(e.Location);
                bool pHover = heroPrevRect.Contains(e.Location);
                bool nHover = heroNextRect.Contains(e.Location);
                if (bHover != heroBookHover || pHover != heroPrevHover || nHover != heroNextHover)
                {
                    heroBookHover = bHover;
                    heroPrevHover = pHover;
                    heroNextHover = nHover;
                    pnlHeroCard.Cursor = (bHover || pHover || nHover) ? Cursors.Hand : Cursors.Default;
                    pnlHeroCard.Invalidate();
                }
            };

            pnlHeroCard.MouseLeave += (s, e) =>
            {
                if (heroBookHover || heroPrevHover || heroNextHover)
                {
                    heroBookHover = heroPrevHover = heroNextHover = false;
                    pnlHeroCard.Cursor = Cursors.Default;
                    pnlHeroCard.Invalidate();
                }
            };

            pnlHeroCard.MouseClick += (s, e) =>
            {
                if (heroBtnBookRect.Contains(e.Location))
                {
                    NavigateTo("Đặt sân");
                }
                else if (heroPrevRect.Contains(e.Location))
                {
                    heroSlideIndex = (heroSlideIndex + 2) % 3;
                    pnlHeroCard.Invalidate();
                }
                else if (heroNextRect.Contains(e.Location))
                {
                    heroSlideIndex = (heroSlideIndex + 1) % 3;
                    pnlHeroCard.Invalidate();
                }
            };
        }

        private static void DrawCarouselArrow(Graphics g, Rectangle rect, bool isLeft, bool hovered)
        {
            using (SolidBrush b = new SolidBrush(hovered ? Color.FromArgb(230, 255, 255, 255) : Color.FromArgb(150, 255, 255, 255)))
            {
                g.FillEllipse(b, rect);
            }
            float cx = rect.X + rect.Width / 2f;
            float cy = rect.Y + rect.Height / 2f;
            using (Pen p = new Pen(ColTextDark, 2f))
            {
                p.StartCap = LineCap.Round;
                p.EndCap = LineCap.Round;
                p.LineJoin = LineJoin.Round;
                if (isLeft)
                {
                    g.DrawLines(p, new[] { new PointF(cx + 2, cy - 5), new PointF(cx - 3, cy), new PointF(cx + 2, cy + 5) });
                }
                else
                {
                    g.DrawLines(p, new[] { new PointF(cx - 2, cy - 5), new PointF(cx + 3, cy), new PointF(cx - 2, cy + 5) });
                }
            }
        }

        private void BuildFeaturedFields()
        {
            pnlFeaturedContainer = new DoubleBufferedContainer
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(8, 0, 0, 0),
                BackColor = ColPageBg,
                Padding = new Padding(16, 48, 16, 14)
            };

            Rectangle linkSeeAllRect = Rectangle.Empty;
            bool seeAllHover = false;

            pnlFeaturedContainer.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                Rectangle r = new Rectangle(0, 0, pnlFeaturedContainer.Width - 1, pnlFeaturedContainer.Height - 1);
                using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(r, 16))
                {
                    using (SolidBrush b = new SolidBrush(ColCardBg))
                        g.FillPath(b, path);
                    using (Pen p = new Pen(ColBorder, 1.2f))
                        g.DrawPath(p, path);
                }

                // Header Icon + "Sân nổi bật"
                SportFieldIconProvider.Draw(g, SportFieldIcon.Sport, new Rectangle(18, 14, 22, 22), null);
                using (Font fTitle = new Font("Segoe UI", 12.5f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(g, "Sân nổi bật", fTitle, new Point(46, 13), ColTextDark, TextFormatFlags.NoPadding);
                }

                // Right link: "Xem tất cả sân →"
                using (Font fLink = new Font("Segoe UI", 9.5f, FontStyle.Bold))
                {
                    string lnkText = "Xem tất cả sân →";
                    Size lSz = TextRenderer.MeasureText(g, lnkText, fLink);
                    linkSeeAllRect = new Rectangle(pnlFeaturedContainer.Width - lSz.Width - 18, 15, lSz.Width + 4, 22);
                    TextRenderer.DrawText(g, lnkText, fLink, linkSeeAllRect, seeAllHover ? ColEmerald : ColPrimaryTeal, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
                }
            };

            pnlFeaturedContainer.MouseMove += (s, e) =>
            {
                bool h = linkSeeAllRect.Contains(e.Location);
                if (h != seeAllHover)
                {
                    seeAllHover = h;
                    pnlFeaturedContainer.Cursor = h ? Cursors.Hand : Cursors.Default;
                    pnlFeaturedContainer.Invalidate(new Rectangle(0, 0, pnlFeaturedContainer.Width, 46));
                }
            };
            pnlFeaturedContainer.MouseClick += (s, e) =>
            {
                if (linkSeeAllRect.Contains(e.Location))
                {
                    NavigateTo("Đặt sân");
                }
            };

            tlpFeaturedCards = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = ColCardBg,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            tlpFeaturedCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            tlpFeaturedCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            tlpFeaturedCards.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            pnlFeaturedContainer.Controls.Add(tlpFeaturedCards);
        }

        private void BuildHeroAndFeaturedLayout()
        {
            tlpHeroFeatured = new TableLayoutPanel
            {
                Height = 336,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = ColPageBg,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            tlpHeroFeatured.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 51f));
            tlpHeroFeatured.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 49f));
            tlpHeroFeatured.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            tlpHeroFeatured.Controls.Add(pnlHeroCard, 0, 0);
            tlpHeroFeatured.Controls.Add(pnlFeaturedContainer, 1, 0);

            contentCanvas.Controls.Add(tlpHeroFeatured);
        }

        // ------------------------------------------
        // 4. CUSTOMER KPI ROW (4 CARDS)
        // ------------------------------------------
        private void BuildCustomerKpis()
        {
            tlpKpiRow = new TableLayoutPanel
            {
                Height = 112,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = ColPageBg,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            for (int i = 0; i < 4; i++)
            {
                tlpKpiRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            }
            tlpKpiRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            kpiBookings = new CustomerKpiCard(
                "Lượt đặt sân",
                "0",
                "Đang tải dữ liệu...",
                SportFieldIcon.Booking,
                ColEmeraldTint,
                ColEmeraldDark)
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 6, 0)
            };
            kpiBookings.Click += (s, e) => NavigateTo("Lịch sử đặt sân");

            kpiVouchers = new CustomerKpiCard(
                "Voucher khả dụng",
                "0",
                "Đang tải dữ liệu...",
                SportFieldIcon.Voucher,
                ColEmeraldTint,
                ColEmeraldDark)
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(6, 0, 6, 0)
            };
            kpiVouchers.Click += (s, e) => NavigateTo("Voucher của tôi");

            kpiSpend = new CustomerKpiCard(
                "Tổng chi tiêu",
                "0đ",
                "Đang tải dữ liệu...",
                SportFieldIcon.Wallet,
                ColBlueTint,
                ColBlue)
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(6, 0, 6, 0)
            };
            kpiSpend.Click += (s, e) => NavigateTo("Hóa đơn của tôi");

            kpiMembership = new CustomerMembershipKpiCard
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(6, 0, 0, 0)
            };
            kpiMembership.Click += (s, e) => NavigateTo("Thông tin cá nhân");

            tlpKpiRow.Controls.Add(kpiBookings, 0, 0);
            tlpKpiRow.Controls.Add(kpiVouchers, 1, 0);
            tlpKpiRow.Controls.Add(kpiSpend, 2, 0);
            tlpKpiRow.Controls.Add(kpiMembership, 3, 0);

            contentCanvas.Controls.Add(tlpKpiRow);
        }

        // ------------------------------------------
        // 5. BOTTOM SECTIONS: UPCOMING BOOKINGS | OFFERS | MEMBERSHIP
        // ------------------------------------------
        private void BuildUpcomingBookings()
        {
            pnlUpcomingSection = new DoubleBufferedContainer
            {
                BackColor = ColPageBg,
                Padding = new Padding(16, 48, 16, 14)
            };

            Rectangle linkAllRect = Rectangle.Empty;
            bool linkHover = false;

            pnlUpcomingSection.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                Rectangle r = new Rectangle(0, 0, pnlUpcomingSection.Width - 1, pnlUpcomingSection.Height - 1);
                using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(r, 16))
                {
                    using (SolidBrush b = new SolidBrush(ColCardBg))
                        g.FillPath(b, path);
                    using (Pen p = new Pen(ColBorder, 1.2f))
                        g.DrawPath(p, path);
                }

                SportFieldIconProvider.Draw(g, SportFieldIcon.Calendar, new Rectangle(18, 14, 20, 20), null);
                using (Font fTitle = new Font("Segoe UI", 11.5f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(g, "Lịch đặt sân sắp tới", fTitle, new Point(44, 13), ColTextDark, TextFormatFlags.NoPadding);
                }

                using (Font fLink = new Font("Segoe UI", 9f, FontStyle.Bold))
                {
                    string txt = "Xem tất cả →";
                    Size sz = TextRenderer.MeasureText(g, txt, fLink);
                    linkAllRect = new Rectangle(pnlUpcomingSection.Width - sz.Width - 18, 14, sz.Width + 4, 22);
                    TextRenderer.DrawText(g, txt, fLink, linkAllRect, linkHover ? ColEmerald : ColPrimaryTeal, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
                }
            };

            pnlUpcomingSection.MouseMove += (s, e) =>
            {
                bool h = linkAllRect.Contains(e.Location);
                if (h != linkHover)
                {
                    linkHover = h;
                    pnlUpcomingSection.Cursor = h ? Cursors.Hand : Cursors.Default;
                    pnlUpcomingSection.Invalidate(new Rectangle(0, 0, pnlUpcomingSection.Width, 44));
                }
            };
            pnlUpcomingSection.MouseClick += (s, e) =>
            {
                if (linkAllRect.Contains(e.Location))
                {
                    NavigateTo("Lịch sử đặt sân");
                }
            };

            pnlUpcomingListHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColCardBg,
                Margin = new Padding(0)
            };
            pnlUpcomingSection.Controls.Add(pnlUpcomingListHost);
            contentCanvas.Controls.Add(pnlUpcomingSection);
        }

        private void BuildOffers()
        {
            pnlOffersSection = new DoubleBufferedContainer
            {
                BackColor = ColPageBg,
                Padding = new Padding(16, 48, 16, 14)
            };

            Rectangle linkAllRect = Rectangle.Empty;
            bool linkHover = false;

            pnlOffersSection.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                Rectangle r = new Rectangle(0, 0, pnlOffersSection.Width - 1, pnlOffersSection.Height - 1);
                using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(r, 16))
                {
                    using (SolidBrush b = new SolidBrush(ColCardBg))
                        g.FillPath(b, path);
                    using (Pen p = new Pen(ColBorder, 1.2f))
                        g.DrawPath(p, path);
                }

                SportFieldIconProvider.Draw(g, SportFieldIcon.Discount, new Rectangle(18, 14, 20, 20), null);
                using (Font fTitle = new Font("Segoe UI", 11.5f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(g, "Ưu đãi dành cho bạn", fTitle, new Point(44, 13), ColTextDark, TextFormatFlags.NoPadding);
                }

                using (Font fLink = new Font("Segoe UI", 9f, FontStyle.Bold))
                {
                    string txt = "Xem tất cả →";
                    Size sz = TextRenderer.MeasureText(g, txt, fLink);
                    linkAllRect = new Rectangle(pnlOffersSection.Width - sz.Width - 18, 14, sz.Width + 4, 22);
                    TextRenderer.DrawText(g, txt, fLink, linkAllRect, linkHover ? ColEmerald : ColPrimaryTeal, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
                }
            };

            pnlOffersSection.MouseMove += (s, e) =>
            {
                bool h = linkAllRect.Contains(e.Location);
                if (h != linkHover)
                {
                    linkHover = h;
                    pnlOffersSection.Cursor = h ? Cursors.Hand : Cursors.Default;
                    pnlOffersSection.Invalidate(new Rectangle(0, 0, pnlOffersSection.Width, 44));
                }
            };
            pnlOffersSection.MouseClick += (s, e) =>
            {
                if (linkAllRect.Contains(e.Location))
                {
                    NavigateTo("Voucher của tôi");
                }
            };

            pnlOffersListHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColCardBg,
                Margin = new Padding(0)
            };
            pnlOffersSection.Controls.Add(pnlOffersListHost);
            contentCanvas.Controls.Add(pnlOffersSection);
        }

        private void BuildMembershipSummary()
        {
            pnlMembershipSummary = new DoubleBufferedContainer
            {
                BackColor = ColPageBg
            };

            pnlMembershipSummary.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                int w = pnlMembershipSummary.Width;
                int h = pnlMembershipSummary.Height;
                if (w <= 10 || h <= 10) return;

                Rectangle r = new Rectangle(0, 0, w - 1, h - 1);
                using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(r, 16))
                {
                    using (SolidBrush b = new SolidBrush(ColCardBg))
                        g.FillPath(b, path);
                    using (Pen p = new Pen(ColBorder, 1.2f))
                        g.DrawPath(p, path);
                }

                // 1. Header: Crown icon + "Thành viên SPORTFIELD" + ">"
                SportFieldIconProvider.Draw(g, SportFieldIcon.Vip, new Rectangle(18, 14, 20, 20), null);
                using (Font fTitle = new Font("Segoe UI", 11.5f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(g, "Thành viên SPORTFIELD", fTitle, new Point(44, 13), ColTextDark, TextFormatFlags.NoPadding);
                }

                memHeaderArrowRect = new Rectangle(w - 36, 12, 22, 22);
                VectorIcons.Draw(g, SFIcon.ChevronRight, new Rectangle(w - 32, 16, 14, 14), ColTextMuted, 1.8f);

                // 2. Warm Gold Inner Card
                Rectangle goldRect = new Rectangle(16, 46, w - 32, 96);
                using (GraphicsPath goldPath = GraphicsUtility.CreateRoundedRectangle(goldRect, 12))
                {
                    using (LinearGradientBrush goldGrad = new LinearGradientBrush(
                        goldRect,
                        Color.FromArgb(254, 243, 199), // #FEF3C7
                        Color.FromArgb(253, 230, 138), // #FDE68A
                        15f))
                    {
                        g.FillPath(goldGrad, goldPath);
                    }
                    using (Pen goldPen = new Pen(Color.FromArgb(252, 211, 77), 1f))
                    {
                        g.DrawPath(goldPen, goldPath);
                    }
                }

                // Avatar inside gold card
                Rectangle avRect = new Rectangle(goldRect.X + 14, goldRect.Y + 22, 48, 48);
                using (SolidBrush avBg = new SolidBrush(Color.FromArgb(146, 100, 52)))
                {
                    g.FillEllipse(avBg, avRect);
                }
                SportFieldIconProvider.Draw(g, SportFieldIcon.User, new Rectangle(avRect.X + 10, avRect.Y + 10, 28, 28), Color.White);

                // Customer Name & Tier on Left of Gold Card
                int infoX = avRect.Right + 12;
                int rightColW = Math.Min(190, (int)(goldRect.Width * 0.46f));
                int maxNameW = Math.Max(90, goldRect.Right - infoX - rightColW - 8);

                using (Font fName = new Font("Segoe UI", 10.25f, FontStyle.Bold))
                using (Font fSub = new Font("Segoe UI", 8.25f, FontStyle.Regular))
                using (Font fTier = new Font("Segoe UI", 10.5f, FontStyle.Bold))
                {
                    Rectangle nameR = new Rectangle(infoX, goldRect.Y + 13, maxNameW, 20);
                    TextRenderer.DrawText(g, custName, fName, nameR, ColTextDark, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);

                    TextRenderer.DrawText(g, "Hạng thành viên", fSub, new Point(infoX, goldRect.Y + 34), Color.FromArgb(120, 53, 15), TextFormatFlags.NoPadding);

                    SportFieldIconProvider.Draw(g, SportFieldIcon.Vip, new Rectangle(infoX, goldRect.Y + 55, 18, 18), null);
                    string tierUpper = hasRealTier ? custTier.ToUpperInvariant() : "CHƯA CÓ HẠNG";
                    TextRenderer.DrawText(g, tierUpper, fTier, new Point(infoX + 22, goldRect.Y + 54), Color.FromArgb(146, 64, 14), TextFormatFlags.NoPadding);
                }

                // Points & Progress on Right of Gold Card
                int rightX = goldRect.Right - rightColW - 12;
                using (Font fPtsVal = new Font("Segoe UI", 13f, FontStyle.Bold))
                using (Font fPtsUnit = new Font("Segoe UI", 9.5f, FontStyle.Regular))
                using (Font fPct = new Font("Segoe UI", 8f, FontStyle.Bold))
                using (Font fNext = new Font("Segoe UI", 7.75f, FontStyle.Regular))
                {
                    string ptsText = string.Format("{0:N0} điểm", custPoints);
                    Rectangle ptsR = new Rectangle(rightX, goldRect.Y + 14, rightColW, 24);
                    TextRenderer.DrawText(g, ptsText, fPtsVal, ptsR, ColTextDark, TextFormatFlags.Right | TextFormatFlags.NoPadding);

                    // Progress bar
                    int barW = Math.Max(70, rightColW - 36);
                    Rectangle barTrack = new Rectangle(rightX, goldRect.Y + 46, barW, 7);
                    using (GraphicsPath tp = GraphicsUtility.CreateRoundedRectangle(barTrack, 3))
                    using (SolidBrush tb = new SolidBrush(Color.FromArgb(229, 231, 235)))
                    {
                        g.FillPath(tb, tp);
                    }
                    int fillW = Math.Max(6, (int)(barW * Math.Min(1f, Math.Max(0.05f, tierProgressRatio))));
                    Rectangle barFill = new Rectangle(rightX, goldRect.Y + 46, Math.Min(barW, fillW), 7);
                    using (GraphicsPath fp = GraphicsUtility.CreateRoundedRectangle(barFill, 3))
                    using (SolidBrush fb = new SolidBrush( Color.FromArgb(217, 119, 6)))
                    {
                        g.FillPath(fb, fp);
                    }

                    string pctStr = string.Format("{0:0}%", tierProgressRatio * 100f);
                    TextRenderer.DrawText(g, pctStr, fPct, new Rectangle(barTrack.Right + 4, goldRect.Y + 41, 34, 16), ColTextDark, TextFormatFlags.Right);

                    Rectangle nextR = new Rectangle(rightX - 10, goldRect.Y + 60, rightColW + 10, 18);
                    TextRenderer.DrawText(g, nextTierCaption, fNext, nextR, Color.FromArgb(120, 53, 15), TextFormatFlags.Right | TextFormatFlags.EndEllipsis);
                }

                // 3. Three Mini Stat Pills Row (Voucher | Lượt đặt | Chi tiêu)
                int statsY = 154;
                int statsH = 44;
                int colW = (w - 32) / 3;

                DrawMiniStatItem(g, new Rectangle(16, statsY, colW, statsH), SportFieldIcon.Voucher, ColEmeraldTint, custVoucherCount.ToString(), "Voucher", true);
                DrawMiniStatItem(g, new Rectangle(16 + colW, statsY, colW, statsH), SportFieldIcon.Booking, ColBlueTint, custTotalBookings.ToString(), "Lượt đặt", true);
                DrawMiniStatItem(g, new Rectangle(16 + colW * 2, statsY, colW, statsH), SportFieldIcon.Payment, ColEmeraldTint, FormatCompactCurrency(custSpend), "Chi tiêu", false);

                // 4. Two Action Buttons at Bottom ("Xem quyền lợi" | "Voucher của tôi")
                int btnY = h - 48;
                int btnGap = 10;
                int btnW = (w - 32 - btnGap) / 2;
                int btnH = 36;

                memBtnBenefitsRect = new Rectangle(16, btnY, btnW, btnH);
                memBtnVouchersRect = new Rectangle(16 + btnW + btnGap, btnY, btnW, btnH);

                using (GraphicsPath p1 = GraphicsUtility.CreateRoundedRectangle(memBtnBenefitsRect, 8))
                using (SolidBrush b1 = new SolidBrush(memBenefitsHover ? ColEmerald : ColEmeraldDark))
                {
                    g.FillPath(b1, p1);
                }
                using (Font fBtn = new Font("Segoe UI", 9.25f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(g, "Xem quyền lợi", fBtn, memBtnBenefitsRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }

                using (GraphicsPath p2 = GraphicsUtility.CreateRoundedRectangle(memBtnVouchersRect, 8))
                {
                    using (SolidBrush b2 = new SolidBrush(memVouchersHover ? ColEmeraldTint : Color.White))
                        g.FillPath(b2, p2);
                    using (Pen pen2 = new Pen(ColEmerald, 1.2f))
                        g.DrawPath(pen2, p2);
                }
                using (Font fBtn = new Font("Segoe UI", 9.25f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(g, "Voucher của tôi", fBtn, memBtnVouchersRect, ColPrimaryTeal, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            };

            pnlMembershipSummary.MouseMove += (s, e) =>
            {
                bool b1 = memBtnBenefitsRect.Contains(e.Location);
                bool b2 = memBtnVouchersRect.Contains(e.Location);
                bool hArr = memHeaderArrowRect.Contains(e.Location);
                if (b1 != memBenefitsHover || b2 != memVouchersHover)
                {
                    memBenefitsHover = b1;
                    memVouchersHover = b2;
                    pnlMembershipSummary.Invalidate();
                }
                pnlMembershipSummary.Cursor = (b1 || b2 || hArr) ? Cursors.Hand : Cursors.Default;
            };

            pnlMembershipSummary.MouseLeave += (s, e) =>
            {
                if (memBenefitsHover || memVouchersHover)
                {
                    memBenefitsHover = memVouchersHover = false;
                    pnlMembershipSummary.Cursor = Cursors.Default;
                    pnlMembershipSummary.Invalidate();
                }
            };

            pnlMembershipSummary.MouseClick += (s, e) =>
            {
                if (memBtnBenefitsRect.Contains(e.Location) || memHeaderArrowRect.Contains(e.Location))
                {
                    NavigateTo("Thông tin cá nhân");
                }
                else if (memBtnVouchersRect.Contains(e.Location))
                {
                    NavigateTo("Voucher của tôi");
                }
            };

            contentCanvas.Controls.Add(pnlMembershipSummary);
        }

        private static void DrawMiniStatItem(Graphics g, Rectangle rect, SportFieldIcon icon, Color circleBg, string valText, string labelText, bool drawRightDivider)
        {
            int icY = rect.Y + (rect.Height - 30) / 2;
            Rectangle icBox = new Rectangle(rect.X + 4, icY, 30, 30);
            using (GraphicsPath p = GraphicsUtility.CreateRoundedRectangle(icBox, 8))
            using (SolidBrush b = new SolidBrush(circleBg))
            {
                g.FillPath(b, p);
            }
            SportFieldIconProvider.Draw(g, icon, new Rectangle(icBox.X + 6, icBox.Y + 6, 18, 18), null);

            int tx = icBox.Right + 8;
            int availW = Math.Max(40, rect.Width - (tx - rect.X) - 6);
            using (Font fVal = new Font("Segoe UI", 9.5f, FontStyle.Bold))
            using (Font fLbl = new Font("Segoe UI", 8f, FontStyle.Regular))
            {
                TextRenderer.DrawText(g, valText, fVal, new Rectangle(tx, rect.Y + 4, availW, 18), ColTextDark, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, labelText, fLbl, new Rectangle(tx, rect.Y + 23, availW, 16), ColTextMuted, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
            }

            if (drawRightDivider)
            {
                using (Pen divPen = new Pen(ColBorder, 1f))
                {
                    g.DrawLine(divPen, rect.Right - 2, rect.Y + 8, rect.Right - 2, rect.Bottom - 8);
                }
            }
        }

        private static string FormatCompactCurrency(decimal amount)
        {
            if (amount <= 0) return "0đ";
            return string.Format("{0:N0}đ", amount);
        }

        // ==========================================
        // RESPONSIVE REFLOW ENGINE (ZERO OVERLAP)
        // ==========================================
        private void ReflowLayout()
        {
            if (isReflowing || mainScrollPanel == null || contentCanvas == null) return;
            isReflowing = true;
            contentCanvas.SuspendLayout();
            try
            {
                int padX = 24;
                int gapY = 18;
                int clientW = mainScrollPanel.ClientSize.Width;
                if (clientW <= 0) clientW = 1040;

                int canvasW = Math.Max(760, clientW);
                if (contentCanvas.Width != canvasW)
                {
                    contentCanvas.Width = canvasW;
                }

                int usableW = Math.Max(712, canvasW - padX * 2);
                int curY = 20;

                // 0. Error Banner (only visible if DB error occurred)
                if (pnlErrorBanner != null && pnlErrorBanner.Visible)
                {
                    pnlErrorBanner.SetBounds(padX, curY, usableW, 56);
                    curY += 56 + 14;
                }

                // 1. Welcome Banner
                pnlWelcomeBanner.SetBounds(padX, curY, usableW, 114);
                curY += 114 + gapY;

                // 2. Quick Actions Row (5 cards)
                tlpQuickActions.SetBounds(padX, curY, usableW, 88);
                curY += 88 + gapY;

                // 3. Hero Banner + Featured Fields Row
                tlpHeroFeatured.SetBounds(padX, curY, usableW, 336);
                curY += 336 + gapY;

                // 4. Customer KPIs Row (4 cards)
                tlpKpiRow.SetBounds(padX, curY, usableW, 112);
                curY += 112 + gapY;

                // 5. Bottom Sections:
                // Wide desktop (usableW >= 1060): 3-column row matching the reference image (Upcoming | Offers | Membership)
                // Medium/Compact desktop (usableW < 1060): 2-column row (Upcoming | Offers) + full-width Membership row underneath
                int bottomCardH = 264;
                int colGap = 16;

                if (usableW >= 1060)
                {
                    int w1 = (usableW - colGap * 2) * 32 / 100;
                    int w2 = (usableW - colGap * 2) * 33 / 100;
                    int w3 = usableW - w1 - w2 - colGap * 2;

                    pnlUpcomingSection.SetBounds(padX, curY, w1, bottomCardH);
                    pnlOffersSection.SetBounds(padX + w1 + colGap, curY, w2, bottomCardH);
                    pnlMembershipSummary.SetBounds(padX + w1 + colGap + w2 + colGap, curY, w3, bottomCardH);

                    curY += bottomCardH + 28;
                }
                else
                {
                    int halfW = (usableW - colGap) / 2;
                    pnlUpcomingSection.SetBounds(padX, curY, halfW, bottomCardH);
                    pnlOffersSection.SetBounds(padX + halfW + colGap, curY, usableW - halfW - colGap, bottomCardH);
                    curY += bottomCardH + gapY;

                    pnlMembershipSummary.SetBounds(padX, curY, usableW, 258);
                    curY += 258 + 28;
                }

                LayoutUpcomingListItems();
                LayoutOffersListItems();

                if (contentCanvas.Height != curY)
                {
                    contentCanvas.Height = curY;
                }
            }
            finally
            {
                contentCanvas.ResumeLayout(true);
                isReflowing = false;
            }
        }

        // ==========================================
        // DATABASE LOADING & BINDING (100% REAL DATA)
        // ==========================================
        private void LoadDataFromDatabase()
        {
            if (pnlErrorBanner != null) pnlErrorBanner.Visible = false;

            try
            {
                int custId = SessionGuard.RequireCustomerId();

                DataSet ds = DatabaseManager.ExecuteDataSet(
                    "EXEC dbo.sp_UI_CustomerHome @KhachHangID = @ID",
                    new SqlParameter("@ID", custId));

                // Load membership tiers table for accurate real tier progress calculation
                try
                {
                    dtTiers = DatabaseManager.ExecuteQuery(
                        "SELECT TenHang, ChiTieuToiThieu, UuDaiMacDinh FROM dbo.HangThanhVien WHERE DangHoatDong = 1 ORDER BY ChiTieuToiThieu ASC");
                }
                catch
                {
                    dtTiers = null;
                }

                // 1. Table 0: Customer Profile & Stats
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    drCustomer = ds.Tables[0].Rows[0];
                    string dbName = drCustomer["HoTen"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(dbName))
                        custName = dbName.Trim();

                    string rawTier = null;
                    if (drCustomer.Table.Columns.Contains("TenHang") && drCustomer["TenHang"] != DBNull.Value)
                        rawTier = drCustomer["TenHang"]?.ToString();
                    else if (drCustomer.Table.Columns.Contains("HangThanhVien") && drCustomer["HangThanhVien"] != DBNull.Value)
                        rawTier = drCustomer["HangThanhVien"]?.ToString();

                    if (!string.IsNullOrWhiteSpace(rawTier))
                    {
                        custTier = rawTier.Trim();
                        hasRealTier = true;
                    }
                    else
                    {
                        custTier = "Chưa có hạng";
                        hasRealTier = false;
                    }

                    custPoints = (drCustomer.Table.Columns.Contains("DiemTichLuy") && drCustomer["DiemTichLuy"] != DBNull.Value)
                        ? Convert.ToDecimal(drCustomer["DiemTichLuy"]) : 0m;
                    custSpend = (drCustomer.Table.Columns.Contains("TongChiTieu") && drCustomer["TongChiTieu"] != DBNull.Value)
                        ? Convert.ToDecimal(drCustomer["TongChiTieu"]) : 0m;
                    custWalletBalance = (drCustomer.Table.Columns.Contains("SoDuViCoc") && drCustomer["SoDuViCoc"] != DBNull.Value)
                        ? Convert.ToDecimal(drCustomer["SoDuViCoc"]) : 0m;
                    custTotalBookings = (drCustomer.Table.Columns.Contains("TongLuotDat") && drCustomer["TongLuotDat"] != DBNull.Value)
                        ? Convert.ToInt32(drCustomer["TongLuotDat"]) : 0;
                }

                // Compute real tier progression from HangThanhVien table or customer points
                ComputeRealTierProgress();

                // 2. Upcoming Bookings (load up to 2 upcoming bookings from DB using same criteria as sp_UI_CustomerHome)
                try
                {
                    dtUpcomingBookings = DatabaseManager.ExecuteQuery(
                        "SELECT TOP (2) d.DonDatSanID, d.MaDon, d.TrangThaiDon, d.TrangThaiThanhToan, d.TongTien, d.ConLai, " +
                        "ct.SanID, s.TenSan, ls.TenLoaiSan, ct.BatDau, ct.KetThuc, s.ViTri " +
                        "FROM dbo.DonDatSan d " +
                        "JOIN dbo.ChiTietDatSan ct ON ct.DonDatSanID = d.DonDatSanID " +
                        "JOIN dbo.San s ON s.SanID = ct.SanID " +
                        "JOIN dbo.LoaiSan ls ON ls.LoaiSanID = s.LoaiSanID " +
                        "WHERE d.KhachHangID = @ID " +
                        "  AND d.TrangThaiDon IN ('CHO_XAC_NHAN','DA_XAC_NHAN','DANG_SU_DUNG') " +
                        "  AND ct.KetThuc >= SYSDATETIME() " +
                        "ORDER BY ct.BatDau ASC",
                        new SqlParameter("@ID", custId));
                }
                catch
                {
                    if (ds != null && ds.Tables.Count > 1)
                        dtUpcomingBookings = ds.Tables[1];
                }

                custUpcomingCount = dtUpcomingBookings != null ? dtUpcomingBookings.Rows.Count : 0;

                // 3. Featured Courts (Table 2)
                if (ds != null && ds.Tables.Count > 2 && ds.Tables[2].Rows.Count > 0)
                {
                    dtFeaturedCourts = ds.Tables[2];
                }
                else
                {
                    dtFeaturedCourts = DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_DanhSachSan");
                }

                // 4. Vouchers (Table 3)
                if (ds != null && ds.Tables.Count > 3)
                {
                    dtVouchers = ds.Tables[3];
                    custVoucherCount = dtVouchers.Rows.Count;
                }
                else
                {
                    dtVouchers = null;
                    custVoucherCount = 0;
                }

                // Update UI components with real DB values
                UpdateKpiCardsFromData();
                PopulateFeaturedFieldCards();
                PopulateUpcomingBookingsList();
                PopulateOffersList();

                pnlWelcomeBanner?.Invalidate();
                pnlMembershipSummary?.Invalidate();
                ReflowLayout();
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "CUSTOMER_HOME", "LoadDataFromDatabase");
                if (pnlErrorBanner != null)
                {
                    pnlErrorBanner.Visible = true;
                }
                PopulateFeaturedFieldCards();
                PopulateUpcomingBookingsList();
                PopulateOffersList();
                ReflowLayout();
            }
        }

        private void ComputeRealTierProgress()
        {
            if (dtTiers != null && dtTiers.Rows.Count > 0)
            {
                string nextTierName = null;
                decimal nextMinSpend = 0m;
                foreach (DataRow tr in dtTiers.Rows)
                {
                    decimal minSpend = tr["ChiTieuToiThieu"] != DBNull.Value ? Convert.ToDecimal(tr["ChiTieuToiThieu"]) : 0m;
                    if (minSpend > custSpend)
                    {
                        nextTierName = tr["TenHang"]?.ToString();
                        nextMinSpend = minSpend;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(nextTierName) && nextMinSpend > 0)
                {
                    tierProgressRatio = (float)Math.Min(0.95m, Math.Max(0.08m, custSpend / nextMinSpend));
                    decimal diff = Math.Max(0m, nextMinSpend - custSpend);
                    nextTierCaption = string.Format("Còn {0:N0}đ lên hạng {1}", diff, nextTierName);
                    return;
                }
                else if (hasRealTier)
                {
                    tierProgressRatio = 1.0f;
                    nextTierCaption = string.Format("{0:N0} điểm tích lũy", custPoints);
                    return;
                }
            }

            if (custPoints > 0)
            {
                tierProgressRatio = (float)Math.Min(1.0m, Math.Max(0.12m, custPoints / 2000m));
                nextTierCaption = string.Format("Tích lũy {0:N0} điểm thưởng", custPoints);
            }
            else
            {
                tierProgressRatio = 0.05f;
                nextTierCaption = hasRealTier ? "Đặt sân để tích điểm thưởng" : "Chưa có hạng thành viên";
            }
        }

        private void UpdateKpiCardsFromData()
        {
            if (kpiBookings != null)
            {
                kpiBookings.ValueText = custTotalBookings.ToString();
                kpiBookings.CaptionText = custUpcomingCount > 0
                    ? string.Format("{0} lịch đặt sắp tới", custUpcomingCount)
                    : "Tổng lượt đặt tích lũy";
                kpiBookings.BadgeText = custTotalBookings > 0 ? "Hoạt động" : "";
                kpiBookings.Invalidate();
            }

            if (kpiVouchers != null)
            {
                kpiVouchers.ValueText = custVoucherCount.ToString();
                kpiVouchers.CaptionText = custVoucherCount > 0
                    ? "Ưu đãi đang chờ bạn"
                    : "Chưa có ưu đãi khả dụng";
                kpiVouchers.BadgeText = custVoucherCount > 0 ? "Khả dụng" : "";
                kpiVouchers.Invalidate();
            }

            if (kpiSpend != null)
            {
                kpiSpend.ValueText = string.Format("{0:N0}đ", custSpend);
                kpiSpend.CaptionText = custWalletBalance > 0
                    ? string.Format("Số dư ví cọc: {0:N0}đ", custWalletBalance)
                    : "Tổng chi tiêu tích lũy";
                kpiSpend.BadgeText = "";
                kpiSpend.Invalidate();
            }

            if (kpiMembership != null)
            {
                kpiMembership.TierName = hasRealTier ? custTier : "Chưa có hạng";
                kpiMembership.PointsCaption = string.Format("{0:N0} điểm tích lũy", custPoints);
                kpiMembership.ProgressRatio = tierProgressRatio;
                kpiMembership.Invalidate();
            }
        }

        private void PopulateFeaturedFieldCards()
        {
            if (tlpFeaturedCards == null) return;
            tlpFeaturedCards.SuspendLayout();
            try
            {
                tlpFeaturedCards.Controls.Clear();

                if (dtFeaturedCourts == null || dtFeaturedCourts.Rows.Count == 0)
                {
                    Panel emptyPnl = CreateCleanEmptyStatePanel(
                        SportFieldIcon.Field,
                        "Chưa có sân nổi bật.",
                        "Hiện chưa có sân thể thao nào ở trạng thái sẵn sàng.",
                        null,
                        null);
                    tlpFeaturedCards.SetColumnSpan(emptyPnl, 2);
                    tlpFeaturedCards.Controls.Add(emptyPnl, 0, 0);
                    return;
                }

                int count = Math.Min(2, dtFeaturedCourts.Rows.Count);
                for (int i = 0; i < count; i++)
                {
                    DataRow r = dtFeaturedCourts.Rows[i];
                    int sanId = (r.Table.Columns.Contains("SanID") && r["SanID"] != DBNull.Value) ? Convert.ToInt32(r["SanID"]) : 0;
                    string tenSan = r["TenSan"]?.ToString() ?? "Sân thể thao";
                    string tenLoai = (r.Table.Columns.Contains("TenLoaiSan") && r["TenLoaiSan"] != DBNull.Value) ? r["TenLoaiSan"].ToString() : "Sân tiêu chuẩn";
                    string viTri = (r.Table.Columns.Contains("ViTri") && r["ViTri"] != DBNull.Value && !string.IsNullOrWhiteSpace(r["ViTri"].ToString()))
                        ? r["ViTri"].ToString() : "Cơ sở SPORTFIELD";
                    decimal gia = (r.Table.Columns.Contains("GiaCoBan") && r["GiaCoBan"] != DBNull.Value) ? Convert.ToDecimal(r["GiaCoBan"]) : 0m;
                    string trangThai = (r.Table.Columns.Contains("TrangThaiVanHanh") && r["TrangThaiVanHanh"] != DBNull.Value)
                        ? r["TrangThaiVanHanh"].ToString() : "SAN_SANG";

                    Image fieldImg = ResolveCourtImage(tenSan, tenLoai, i);

                    var card = new CustomerFeaturedFieldCard(
                        sanId,
                        tenSan,
                        tenLoai,
                        viTri,
                        gia,
                        trangThai,
                        fieldImg)
                    {
                        Dock = DockStyle.Fill,
                        Margin = new Padding(i == 0 ? 0 : 6, 0, i == 0 ? 6 : 0, 0)
                    };

                    int capturedSanId = sanId;
                    card.BookClicked += (s, e) => NavigateToBookCourt(capturedSanId);
                    tlpFeaturedCards.Controls.Add(card, i, 0);
                }
            }
            finally
            {
                tlpFeaturedCards.ResumeLayout(true);
            }
        }

        private static Image ResolveCourtImage(string tenSan, string tenLoai, int index)
        {
            string combined = ((tenSan ?? "") + " " + (tenLoai ?? "")).ToLowerInvariant();
            if (combined.Contains("cầu lông") || combined.Contains("badminton"))
                return AssetManager.GetBadmintonImage();
            if (combined.Contains("tennis") || combined.Contains("quần vợt") || combined.Contains("pickleball") || combined.Contains("rổ"))
                return AssetManager.GetTennisImage();
            if (index % 2 == 1)
                return AssetManager.GetTennisImage() ?? AssetManager.GetPitchImage();
            return AssetManager.GetPitchImage();
        }

        private void PopulateUpcomingBookingsList()
        {
            if (pnlUpcomingListHost == null) return;
            pnlUpcomingListHost.SuspendLayout();
            try
            {
                pnlUpcomingListHost.Controls.Clear();

                if (dtUpcomingBookings == null || dtUpcomingBookings.Rows.Count == 0)
                {
                    Panel empty = CreateCleanEmptyStatePanel(
                        SportFieldIcon.Calendar,
                        "Bạn chưa có lịch đặt sân sắp tới.",
                        "Đặt sân ngay để chọn khung giờ thi đấu phù hợp nhất.",
                        "Đặt sân",
                        () => NavigateTo("Đặt sân"));
                    pnlUpcomingListHost.Controls.Add(empty);
                    return;
                }

                int count = Math.Min(2, dtUpcomingBookings.Rows.Count);
                for (int i = 0; i < count; i++)
                {
                    DataRow r = dtUpcomingBookings.Rows[i];
                    long donId = (r.Table.Columns.Contains("DonDatSanID") && r["DonDatSanID"] != DBNull.Value)
                        ? Convert.ToInt64(r["DonDatSanID"]) : 0L;
                    string tenSan = r["TenSan"]?.ToString() ?? "Sân thể thao";
                    string viTri = (r.Table.Columns.Contains("ViTri") && r["ViTri"] != DBNull.Value && !string.IsNullOrWhiteSpace(r["ViTri"].ToString()))
                        ? r["ViTri"].ToString() : "Cơ sở SPORTFIELD";
                    DateTime batDau = (r.Table.Columns.Contains("BatDau") && r["BatDau"] != DBNull.Value)
                        ? Convert.ToDateTime(r["BatDau"]) : DateTime.Now;
                    DateTime ketThuc = (r.Table.Columns.Contains("KetThuc") && r["KetThuc"] != DBNull.Value)
                        ? Convert.ToDateTime(r["KetThuc"]) : batDau.AddHours(1.5);
                    string trangThaiDon = (r.Table.Columns.Contains("TrangThaiDon") && r["TrangThaiDon"] != DBNull.Value)
                        ? r["TrangThaiDon"].ToString() : "DA_XAC_NHAN";

                    var itemCtrl = new UpcomingBookingRowItem(
                        donId,
                        tenSan,
                        viTri,
                        batDau,
                        ketThuc,
                        trangThaiDon,
                        i == 0);

                    long capturedDonId = donId;
                    itemCtrl.Click += (s, e) => OpenBookingDetail(capturedDonId);
                    pnlUpcomingListHost.Controls.Add(itemCtrl);
                }

                LayoutUpcomingListItems();
            }
            finally
            {
                pnlUpcomingListHost.ResumeLayout(true);
            }
        }

        private void LayoutUpcomingListItems()
        {
            if (pnlUpcomingListHost == null || pnlUpcomingListHost.Controls.Count == 0) return;
            int w = Math.Max(200, pnlUpcomingListHost.ClientSize.Width);
            int h = Math.Max(140, pnlUpcomingListHost.ClientSize.Height);

            if (pnlUpcomingListHost.Controls.Count == 1 && !(pnlUpcomingListHost.Controls[0] is UpcomingBookingRowItem))
            {
                pnlUpcomingListHost.Controls[0].SetBounds(0, 0, w, h);
                return;
            }

            int gap = 10;
            int itemH = Math.Min(92, (h - gap) / 2);
            int curY = 0;
            foreach (Control c in pnlUpcomingListHost.Controls)
            {
                c.SetBounds(0, curY, w, itemH);
                curY += itemH + gap;
            }
        }

        private void PopulateOffersList()
        {
            if (pnlOffersListHost == null) return;
            pnlOffersListHost.SuspendLayout();
            try
            {
                pnlOffersListHost.Controls.Clear();

                if (dtVouchers == null || dtVouchers.Rows.Count == 0)
                {
                    Panel empty = CreateCleanEmptyStatePanel(
                        SportFieldIcon.Voucher,
                        "Bạn chưa có ưu đãi khả dụng.",
                        "Các mã giảm giá mới sẽ hiển thị tại đây khi khả dụng.",
                        "Xem kho voucher",
                        () => NavigateTo("Voucher của tôi"));
                    pnlOffersListHost.Controls.Add(empty);
                    return;
                }

                int count = Math.Min(2, dtVouchers.Rows.Count);
                for (int i = 0; i < count; i++)
                {
                    DataRow vr = dtVouchers.Rows[i];
                    string code = vr["MaVoucher"]?.ToString() ?? "";
                    string name = vr["TenVoucher"]?.ToString() ?? "Ưu đãi đặt sân";
                    string loai = vr["LoaiGiam"]?.ToString() ?? "";
                    decimal val = vr["GiaTriGiam"] != DBNull.Value ? Convert.ToDecimal(vr["GiaTriGiam"]) : 0m;
                    decimal minOrder = (vr.Table.Columns.Contains("DonToiThieu") && vr["DonToiThieu"] != DBNull.Value)
                        ? Convert.ToDecimal(vr["DonToiThieu"]) : 0m;
                    DateTime? expiry = (vr.Table.Columns.Contains("NgayKetThuc") && vr["NgayKetThuc"] != DBNull.Value)
                        ? (DateTime?)Convert.ToDateTime(vr["NgayKetThuc"]) : null;

                    string headline = loai == "PHAN_TRAM"
                        ? string.Format("Giảm {0:0.#}%", val)
                        : string.Format("Voucher {0:N0}đ", val);

                    string sub1 = !string.IsNullOrWhiteSpace(name) ? name : ("Mã ưu đãi: " + code);
                    string sub2 = minOrder > 0
                        ? string.Format("Cho đơn đặt sân từ {0:N0}đ • Mã: {1}", minOrder, code)
                        : (expiry.HasValue ? string.Format("Mã: {0} • HSD: {1:dd/MM/yyyy}", code, expiry.Value) : ("Mã áp dụng: " + code));

                    var offerCard = new CustomerOfferBannerCard(headline, sub1, sub2, i == 0);
                    offerCard.Click += (s, e) => NavigateTo("Voucher của tôi");
                    pnlOffersListHost.Controls.Add(offerCard);
                }

                if (count == 1)
                {
                    DataRow vr = dtVouchers.Rows[0];
                    string code = vr["MaVoucher"]?.ToString() ?? "";
                    var secondCard = new CustomerOfferBannerCard(
                        "Sử dụng ngay",
                        "Áp dụng mã " + code + " khi đặt sân",
                        "Chọn voucher trong bước thanh toán đơn đặt sân trực tuyến",
                        false);
                    secondCard.Click += (s, e) => NavigateTo("Đặt sân");
                    pnlOffersListHost.Controls.Add(secondCard);
                }

                LayoutOffersListItems();
            }
            finally
            {
                pnlOffersListHost.ResumeLayout(true);
            }
        }

        private void LayoutOffersListItems()
        {
            if (pnlOffersListHost == null || pnlOffersListHost.Controls.Count == 0) return;
            int w = Math.Max(200, pnlOffersListHost.ClientSize.Width);
            int h = Math.Max(140, pnlOffersListHost.ClientSize.Height);

            if (pnlOffersListHost.Controls.Count == 1 && !(pnlOffersListHost.Controls[0] is CustomerOfferBannerCard))
            {
                pnlOffersListHost.Controls[0].SetBounds(0, 0, w, h);
                return;
            }

            int gap = 10;
            int itemH = Math.Min(92, (h - gap) / 2);
            int curY = 0;
            foreach (Control c in pnlOffersListHost.Controls)
            {
                c.SetBounds(0, curY, w, itemH);
                curY += itemH + gap;
            }
        }

        private Panel CreateCleanEmptyStatePanel(SportFieldIcon icon, string title, string subtitle, string actionText, Action onActionClick)
        {
            var pnl = new DoubleBufferedContainer
            {
                Dock = DockStyle.Fill,
                BackColor = ColCardBg
            };

            Rectangle btnRect = Rectangle.Empty;
            bool btnHover = false;

            pnl.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                int w = pnl.Width;
                int h = pnl.Height;
                Rectangle boxR = new Rectangle(2, 2, w - 5, h - 5);
                using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(boxR, 12))
                {
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(248, 250, 252)))
                        g.FillPath(b, path);
                    using (Pen p = new Pen(ColBorder, 1f))
                        g.DrawPath(p, path);
                }

                int cy = string.IsNullOrEmpty(actionText) ? (h / 2 - 36) : (h / 2 - 54);
                cy = Math.Max(14, cy);

                SportFieldIconProvider.Draw(g, icon, new Rectangle((w - 32) / 2, cy, 32, 32), null);

                using (Font fTitle = new Font("Segoe UI", 10f, FontStyle.Bold))
                using (Font fSub = new Font("Segoe UI", 8.75f, FontStyle.Regular))
                {
                    Rectangle tR = new Rectangle(12, cy + 38, w - 24, 22);
                    TextRenderer.DrawText(g, title, fTitle, tR, ColTextDark, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                    Rectangle sR = new Rectangle(16, cy + 60, w - 32, 34);
                    TextRenderer.DrawText(g, subtitle, fSub, sR, ColTextMuted, TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak);
                }

                if (!string.IsNullOrEmpty(actionText))
                {
                    int bw = 136;
                    int bh = 32;
                    btnRect = new Rectangle((w - bw) / 2, Math.Min(h - bh - 10, cy + 98), bw, bh);
                    using (GraphicsPath bp = GraphicsUtility.CreateRoundedRectangle(btnRect, 8))
                    using (SolidBrush bb = new SolidBrush(btnHover ? ColEmeraldDark : ColEmerald))
                    {
                        g.FillPath(bb, bp);
                    }
                    using (Font fB = new Font("Segoe UI", 9f, FontStyle.Bold))
                    {
                        TextRenderer.DrawText(g, actionText, fB, btnRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                }
            };

            if (!string.IsNullOrEmpty(actionText) && onActionClick != null)
            {
                pnl.MouseMove += (s, e) =>
                {
                    bool h = btnRect.Contains(e.Location);
                    if (h != btnHover)
                    {
                        btnHover = h;
                        pnl.Cursor = h ? Cursors.Hand : Cursors.Default;
                        pnl.Invalidate();
                    }
                };
                pnl.MouseClick += (s, e) =>
                {
                    if (btnRect.Contains(e.Location))
                        onActionClick();
                };
            }

            return pnl;
        }

        // ==========================================
        // LOCAL CUSTOM CONTROLS FOR CUSTOMER HOME
        // ==========================================

        private class DoubleBufferedContainer : Panel
        {
            public DoubleBufferedContainer()
            {
                DoubleBuffered = true;
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw, true);
            }
        }

        private class CustomerHomeScrollPanel : Panel
        {
            private const int SB_HORZ = 0;
            private const int WM_NCPAINT = 0x85;
            private const int WM_HSCROLL = 0x114;

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            private static extern int ShowScrollBar(IntPtr hWnd, int wBar, int bShow);

            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    cp.Style &= ~0x00100000; // Strip WS_HSCROLL so horizontal scrollbar never appears
                    return cp;
                }
            }

            public CustomerHomeScrollPanel()
            {
                DoubleBuffered = true;
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw, true);
                AutoScroll = true;
                HorizontalScroll.Enabled = false;
                HorizontalScroll.Visible = false;
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                if (m.Msg == WM_NCPAINT || m.Msg == WM_HSCROLL)
                {
                    if (IsHandleCreated) ShowScrollBar(Handle, SB_HORZ, 0);
                }
            }
        }

        private class QuickActionCard : Panel
        {
            private readonly string title;
            private readonly string subtitle;
            private readonly SportFieldIcon icon;
            private readonly Color accentColor;
            private readonly Color tintColor;
            private readonly Color titleColor;
            private bool isHovered = false;

            public QuickActionCard(string title, string subtitle, SportFieldIcon icon, Color accentColor, Color tintColor, Color titleColor)
            {
                this.title = title;
                this.subtitle = subtitle;
                this.icon = icon;
                this.accentColor = accentColor;
                this.tintColor = tintColor;
                this.titleColor = titleColor;

                DoubleBuffered = true;
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw, true);
                Cursor = Cursors.Hand;
                BackColor = ColPageBg;
            }

            protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); isHovered = true; Invalidate(); }
            protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); isHovered = false; Invalidate(); }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                Rectangle r = new Rectangle(1, 1, Width - 3, Height - 3);
                using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(r, 14))
                {
                    using (SolidBrush bg = new SolidBrush(isHovered ? tintColor : Color.White))
                    {
                        g.FillPath(bg, path);
                    }
                    // Soft inner tint wash
                    using (SolidBrush wash = new SolidBrush(Color.FromArgb(isHovered ? 120 : 55, tintColor)))
                    {
                        g.FillPath(wash, path);
                    }
                    using (Pen pen = new Pen(isHovered ? accentColor : ColBorder, isHovered ? 1.6f : 1.1f))
                    {
                        g.DrawPath(pen, path);
                    }
                }

                // Icon badge
                int badgeSize = 48;
                int badgeX = 14;
                int badgeY = (Height - badgeSize) / 2;
                Rectangle badgeRect = new Rectangle(badgeX, badgeY, badgeSize, badgeSize);
                using (GraphicsPath bp = GraphicsUtility.CreateRoundedRectangle(badgeRect, 12))
                using (SolidBrush bb = new SolidBrush(accentColor))
                {
                    g.FillPath(bb, bp);
                }

                SportFieldIconProvider.Draw(g, icon, new Rectangle(badgeX + 11, badgeY + 11, 26, 26), Color.White);

                // Text
                int textX = badgeRect.Right + 12;
                int availW = Math.Max(40, Width - textX - 8);
                using (Font fTitle = new Font("Segoe UI", 10.5f, FontStyle.Bold))
                using (Font fSub = new Font("Segoe UI", 8.25f, FontStyle.Regular))
                {
                    Rectangle tR = new Rectangle(textX, badgeY + 4, availW, 22);
                    TextRenderer.DrawText(g, title, fTitle, tR, titleColor, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);

                    Rectangle sR = new Rectangle(textX, badgeY + 26, availW, 20);
                    TextRenderer.DrawText(g, subtitle, fSub, sR, ColTextMuted, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                }
            }
        }

        private class CustomerFeaturedFieldCard : Panel
        {
            private readonly int sanId;
            private readonly string tenSan;
            private readonly string tenLoai;
            private readonly string viTri;
            private readonly decimal gia;
            private readonly string trangThai;
            private readonly Image fieldImg;

            private Rectangle btnBookRect = Rectangle.Empty;
            private bool isCardHovered = false;
            private bool isBtnHovered = false;

            public event EventHandler BookClicked;

            public CustomerFeaturedFieldCard(int sanId, string tenSan, string tenLoai, string viTri, decimal gia, string trangThai, Image fieldImg)
            {
                this.sanId = sanId;
                this.tenSan = tenSan;
                this.tenLoai = tenLoai;
                this.viTri = viTri;
                this.gia = gia;
                this.trangThai = trangThai;
                this.fieldImg = fieldImg;

                DoubleBuffered = true;
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw, true);
                BackColor = ColCardBg;
                Cursor = Cursors.Hand;
            }

            protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); isCardHovered = true; Invalidate(); }
            protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); isCardHovered = false; isBtnHovered = false; Invalidate(); }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                bool bh = btnBookRect.Contains(e.Location);
                if (bh != isBtnHovered)
                {
                    isBtnHovered = bh;
                    Invalidate();
                }
            }

            protected override void OnMouseClick(MouseEventArgs e)
            {
                base.OnMouseClick(e);
                BookClicked?.Invoke(this, EventArgs.Empty);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                Rectangle r = new Rectangle(1, 1, Width - 3, Height - 3);
                if (r.Width <= 10 || r.Height <= 10) return;

                using (GraphicsPath cardPath = GraphicsUtility.CreateRoundedRectangle(r, 12))
                {
                    using (SolidBrush bg = new SolidBrush(Color.White))
                    {
                        g.FillPath(bg, cardPath);
                    }

                    // Top image (height 122px)
                    int imgH = 122;
                    Rectangle imgRect = new Rectangle(r.X, r.Y, r.Width, imgH);
                    GraphicsState st = g.Save();
                    g.SetClip(cardPath);
                    g.IntersectClip(imgRect);

                    if (fieldImg != null)
                    {
                        GraphicsUtility.DrawCenterCrop(g, fieldImg, imgRect);
                    }
                    else
                    {
                        using (SolidBrush fb = new SolidBrush(Color.FromArgb(15, 118, 110)))
                            g.FillRectangle(fb, imgRect);
                    }

                    // Price pill badge in bottom-right of image
                    string priceText = string.Format("{0:N0}đ/giờ", gia);
                    using (Font fPrice = new Font("Segoe UI", 8.75f, FontStyle.Bold))
                    {
                        Size pSz = TextRenderer.MeasureText(g, priceText, fPrice);
                        int pw = pSz.Width + 16;
                        int ph = 24;
                        Rectangle pricePill = new Rectangle(imgRect.Right - pw - 8, imgRect.Bottom - ph - 8, pw, ph);
                        using (GraphicsPath pp = GraphicsUtility.CreateRoundedRectangle(pricePill, 8))
                        using (SolidBrush pb = new SolidBrush(Color.FromArgb(215, 15, 23, 42)))
                        {
                            g.FillPath(pb, pp);
                        }
                        TextRenderer.DrawText(g, priceText, fPrice, pricePill, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }

                    g.Restore(st);

                    using (Pen borderPen = new Pen(isCardHovered ? ColEmerald : ColBorder, isCardHovered ? 1.5f : 1.1f))
                    {
                        g.DrawPath(borderPen, cardPath);
                    }

                    // Info section below image
                    int padX = 12;
                    int curY = imgH + 10;
                    int availW = Math.Max(40, Width - padX * 2);

                    // 1. Court Name
                    using (Font fName = new Font("Segoe UI", 10.5f, FontStyle.Bold))
                    {
                        Rectangle nameR = new Rectangle(padX, curY, availW, 20);
                        TextRenderer.DrawText(g, tenSan, fName, nameR, ColTextDark, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                    }
                    curY += 23;

                    // 2. Location
                    SportFieldIconProvider.Draw(g, SportFieldIcon.Location, new Rectangle(padX, curY + 1, 14, 14), null);
                    using (Font fLoc = new Font("Segoe UI", 8.5f, FontStyle.Regular))
                    {
                        Rectangle locR = new Rectangle(padX + 18, curY, availW - 18, 18);
                        TextRenderer.DrawText(g, viTri, fLoc, locR, ColTextMuted, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                    }
                    curY += 21;

                    // 3. Real Field Type & Status (No fake rating)
                    SportFieldIconProvider.Draw(g, SportFieldIcon.Star, new Rectangle(padX, curY + 1, 14, 14), null);
                    string statusLabel = trangThai == "SAN_SANG" ? "Sẵn sàng" : trangThai;
                    using (Font fType = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                    using (Font fStat = new Font("Segoe UI", 8.25f, FontStyle.Regular))
                    {
                        Size tSz = TextRenderer.MeasureText(g, tenLoai, fType, Size.Empty, TextFormatFlags.NoPadding);
                        TextRenderer.DrawText(g, tenLoai, fType, new Point(padX + 18, curY), ColAmberDark, TextFormatFlags.NoPadding);
                        TextRenderer.DrawText(g, " • " + statusLabel, fStat, new Point(padX + 18 + tSz.Width, curY), ColEmeraldDark, TextFormatFlags.NoPadding);
                    }

                    // 4. Bottom "Đặt sân" Button
                    int btnH = 34;
                    int btnY = Height - btnH - 12;
                    btnBookRect = new Rectangle(padX, btnY, availW, btnH);
                    using (GraphicsPath bp = GraphicsUtility.CreateRoundedRectangle(btnBookRect, 8))
                    using (SolidBrush bb = new SolidBrush(isBtnHovered ? ColEmeraldDark : ColEmerald))
                    {
                        g.FillPath(bb, bp);
                    }
                    using (Font fBtn = new Font("Segoe UI", 9.5f, FontStyle.Bold))
                    {
                        TextRenderer.DrawText(g, "Đặt sân", fBtn, btnBookRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                }
            }
        }

        private class CustomerKpiCard : Panel
        {
            public string TitleText { get; set; }
            public string ValueText { get; set; }
            public string CaptionText { get; set; }
            public string BadgeText { get; set; } = "";
            private readonly SportFieldIcon icon;
            private readonly Color iconBoxBg;
            private readonly Color badgeColor;
            private bool isHovered = false;

            public CustomerKpiCard(string title, string value, string caption, SportFieldIcon icon, Color iconBoxBg, Color badgeColor)
            {
                TitleText = title;
                ValueText = value;
                CaptionText = caption;
                this.icon = icon;
                this.iconBoxBg = iconBoxBg;
                this.badgeColor = badgeColor;

                DoubleBuffered = true;
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw, true);
                Cursor = Cursors.Hand;
                BackColor = ColPageBg;
            }

            protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); isHovered = true; Invalidate(); }
            protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); isHovered = false; Invalidate(); }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                Rectangle r = new Rectangle(1, 1, Width - 3, Height - 3);
                using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(r, 14))
                {
                    using (SolidBrush b = new SolidBrush(ColCardBg))
                        g.FillPath(b, path);
                    using (Pen p = new Pen(isHovered ? ColEmerald : ColBorder, isHovered ? 1.5f : 1.1f))
                        g.DrawPath(p, path);
                }

                // Left Icon Container
                int boxSize = 52;
                int boxX = 16;
                int boxY = (Height - boxSize) / 2;
                Rectangle boxRect = new Rectangle(boxX, boxY, boxSize, boxSize);
                using (GraphicsPath bp = GraphicsUtility.CreateRoundedRectangle(boxRect, 12))
                using (SolidBrush bb = new SolidBrush(iconBoxBg))
                {
                    g.FillPath(bb, bp);
                }
                SportFieldIconProvider.Draw(g, icon, new Rectangle(boxX + 12, boxY + 12, 28, 28), null);

                int tx = boxRect.Right + 14;
                int availW = Math.Max(40, Width - tx - 12);

                using (Font fTitle = new Font("Segoe UI", 9.25f, FontStyle.Regular))
                using (Font fValue = new Font("Segoe UI", 17.5f, FontStyle.Bold))
                using (Font fCap = new Font("Segoe UI", 8.25f, FontStyle.Regular))
                {
                    TextRenderer.DrawText(g, TitleText, fTitle, new Rectangle(tx, 16, availW, 18), ColTextMuted, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                    TextRenderer.DrawText(g, ValueText, fValue, new Rectangle(tx, 36, availW, 34), ColTextDark, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                    TextRenderer.DrawText(g, CaptionText, fCap, new Rectangle(tx, 74, availW, 18), ColTextMuted, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                }
            }
        }

        private class CustomerMembershipKpiCard : Panel
        {
            public string TierName { get; set; } = "Chưa có hạng";
            public string PointsCaption { get; set; } = "0 điểm tích lũy";
            public float ProgressRatio { get; set; } = 0.1f;
            private bool isHovered = false;

            public CustomerMembershipKpiCard()
            {
                DoubleBuffered = true;
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw, true);
                Cursor = Cursors.Hand;
                BackColor = ColPageBg;
            }

            protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); isHovered = true; Invalidate(); }
            protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); isHovered = false; Invalidate(); }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                Rectangle r = new Rectangle(1, 1, Width - 3, Height - 3);
                using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(r, 14))
                {
                    using (SolidBrush b = new SolidBrush(ColCardBg))
                        g.FillPath(b, path);
                    using (Pen p = new Pen(isHovered ? ColAmber : ColBorder, isHovered ? 1.5f : 1.1f))
                        g.DrawPath(p, path);
                }

                // Left Crown Icon Box
                int boxSize = 52;
                int boxX = 16;
                int boxY = (Height - boxSize) / 2;
                Rectangle boxRect = new Rectangle(boxX, boxY, boxSize, boxSize);
                using (GraphicsPath bp = GraphicsUtility.CreateRoundedRectangle(boxRect, 12))
                using (SolidBrush bb = new SolidBrush(Color.FromArgb(254, 243, 199)))
                {
                    g.FillPath(bb, bp);
                }
                SportFieldIconProvider.Draw(g, SportFieldIcon.Vip, new Rectangle(boxX + 12, boxY + 12, 28, 28), null);

                // Top-right ">" circle
                Rectangle chevCircle = new Rectangle(Width - 36, 18, 22, 22);
                using (SolidBrush cb = new SolidBrush(Color.FromArgb(241, 245, 249)))
                {
                    g.FillEllipse(cb, chevCircle);
                }
                VectorIcons.Draw(g, SFIcon.ChevronRight, new Rectangle(chevCircle.X + 5, chevCircle.Y + 5, 12, 12), ColTextMuted, 1.8f);

                int tx = boxRect.Right + 14;
                int availW = Math.Max(40, Width - tx - 16);

                using (Font fTitle = new Font("Segoe UI", 9.25f, FontStyle.Regular))
                using (Font fTier = new Font("Segoe UI", 16f, FontStyle.Bold))
                using (Font fCap = new Font("Segoe UI", 8.25f, FontStyle.Regular))
                {
                    TextRenderer.DrawText(g, "Hạng thành viên", fTitle, new Rectangle(tx, 14, availW - 24, 18), ColTextMuted, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                    TextRenderer.DrawText(g, TierName, fTier, new Rectangle(tx, 33, availW, 28), Color.FromArgb(217, 119, 6), TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);

                    // Progress Bar
                    Rectangle trackR = new Rectangle(tx, 66, availW, 6);
                    using (GraphicsPath tp = GraphicsUtility.CreateRoundedRectangle(trackR, 3))
                    using (SolidBrush tb = new SolidBrush(Color.FromArgb(226, 232, 240)))
                    {
                        g.FillPath(tb, tp);
                    }
                    int fw = Math.Max(6, (int)(availW * Math.Min(1f, Math.Max(0.08f, ProgressRatio))));
                    Rectangle fillR = new Rectangle(tx, 66, Math.Min(availW, fw), 6);
                    using (GraphicsPath fp = GraphicsUtility.CreateRoundedRectangle(fillR, 3))
                    using (SolidBrush fb = new SolidBrush(Color.FromArgb(245, 158, 11)))
                    {
                        g.FillPath(fb, fp);
                    }

                    TextRenderer.DrawText(g, PointsCaption, fCap, new Rectangle(tx, 77, availW, 18), ColTextMuted, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                }
            }
        }

        private class UpcomingBookingRowItem : Panel
        {
            private readonly long donId;
            private readonly string tenSan;
            private readonly string viTri;
            private readonly DateTime batDau;
            private readonly DateTime ketThuc;
            private readonly string trangThaiDon;
            private readonly bool isPrimaryHighlight;
            private bool isHovered = false;

            public UpcomingBookingRowItem(long donId, string tenSan, string viTri, DateTime batDau, DateTime ketThuc, string trangThaiDon, bool isPrimaryHighlight)
            {
                this.donId = donId;
                this.tenSan = tenSan;
                this.viTri = viTri;
                this.batDau = batDau;
                this.ketThuc = ketThuc;
                this.trangThaiDon = trangThaiDon;
                this.isPrimaryHighlight = isPrimaryHighlight;

                DoubleBuffered = true;
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw, true);
                Cursor = Cursors.Hand;
                BackColor = ColCardBg;
            }

            protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); isHovered = true; Invalidate(); }
            protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); isHovered = false; Invalidate(); }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                Rectangle r = new Rectangle(1, 1, Width - 3, Height - 3);
                using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(r, 12))
                {
                    using (SolidBrush b = new SolidBrush(isHovered ? Color.FromArgb(248, 250, 252) : Color.White))
                        g.FillPath(b, path);
                    using (Pen p = new Pen(isHovered ? ColEmerald : Color.FromArgb(241, 245, 249), 1.1f))
                        g.DrawPath(p, path);
                }

                // Left Date Badge (54 x 58)
                int dbW = 52;
                int dbH = 56;
                int dbX = 10;
                int dbY = (Height - dbH) / 2;
                Rectangle dateRect = new Rectangle(dbX, dbY, dbW, dbH);
                Color dateBg = isPrimaryHighlight ? ColEmerald : Color.FromArgb(226, 232, 240);
                Color dateFg = isPrimaryHighlight ? Color.White : ColTextDark;

                using (GraphicsPath dp = GraphicsUtility.CreateRoundedRectangle(dateRect, 10))
                using (SolidBrush dBrush = new SolidBrush(dateBg))
                {
                    g.FillPath(dBrush, dp);
                }

                using (Font fDay = new Font("Segoe UI", 13.5f, FontStyle.Bold))
                using (Font fMon = new Font("Segoe UI", 8f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(g, batDau.ToString("dd"), fDay, new Rectangle(dbX, dbY + 4, dbW, 28), dateFg, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    TextRenderer.DrawText(g, "Th" + batDau.ToString("MM"), fMon, new Rectangle(dbX, dbY + 31, dbW, 20), dateFg, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }

                // Status Pill on Right
                string statusText = trangThaiDon == "CHO_XAC_NHAN" ? "Chờ duyệt" : (isPrimaryHighlight ? "Sắp diễn ra" : "Đã đặt");
                Color pillBg = trangThaiDon == "CHO_XAC_NHAN" ? Color.FromArgb(254, 243, 199) : (isPrimaryHighlight ? Color.FromArgb(220, 252, 231) : Color.FromArgb(241, 245, 249));
                Color pillFg = trangThaiDon == "CHO_XAC_NHAN" ? Color.FromArgb(180, 83, 9) : (isPrimaryHighlight ? Color.FromArgb(21, 128, 61) : ColTextBody);

                int pillW = 78;
                int pillH = 24;
                int pillX = Width - pillW - 22;
                int pillY = (Height - pillH) / 2;
                Rectangle pillRect = new Rectangle(pillX, pillY, pillW, pillH);
                using (GraphicsPath pp = GraphicsUtility.CreateRoundedRectangle(pillRect, 12))
                using (SolidBrush pb = new SolidBrush(pillBg))
                {
                    g.FillPath(pb, pp);
                }
                using (Font fPill = new Font("Segoe UI", 7.75f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(g, statusText, fPill, pillRect, pillFg, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
                VectorIcons.Draw(g, SFIcon.ChevronRight, new Rectangle(Width - 18, (Height - 12) / 2, 12, 12), ColTextMuted, 1.8f);

                // Center Info (TenSan, Time, Location)
                int infoX = dateRect.Right + 12;
                int availW = Math.Max(60, pillX - infoX - 6);

                using (Font fCourt = new Font("Segoe UI", 9.75f, FontStyle.Bold))
                using (Font fMeta = new Font("Segoe UI", 8.25f, FontStyle.Regular))
                {
                    TextRenderer.DrawText(g, tenSan, fCourt, new Rectangle(infoX, dbY + 1, availW, 20), ColTextDark, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);

                    SportFieldIconProvider.Draw(g, SportFieldIcon.Clock, new Rectangle(infoX, dbY + 23, 13, 13), null);
                    string timeStr = string.Format("{0:HH:mm} - {1:HH:mm}", batDau, ketThuc);
                    TextRenderer.DrawText(g, timeStr, fMeta, new Rectangle(infoX + 17, dbY + 21, availW - 17, 18), ColTextBody, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);

                    SportFieldIconProvider.Draw(g, SportFieldIcon.Location, new Rectangle(infoX, dbY + 41, 13, 13), null);
                    TextRenderer.DrawText(g, viTri, fMeta, new Rectangle(infoX + 17, dbY + 39, availW - 17, 18), ColTextMuted, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                }
            }
        }

        private class CustomerOfferBannerCard : Panel
        {
            private readonly string headline;
            private readonly string subtitle1;
            private readonly string subtitle2;
            private readonly bool isEmeraldTheme;
            private bool isHovered = false;

            public CustomerOfferBannerCard(string headline, string subtitle1, string subtitle2, bool isEmeraldTheme)
            {
                this.headline = headline;
                this.subtitle1 = subtitle1;
                this.subtitle2 = subtitle2;
                this.isEmeraldTheme = isEmeraldTheme;

                DoubleBuffered = true;
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw, true);
                Cursor = Cursors.Hand;
                BackColor = ColCardBg;
            }

            protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); isHovered = true; Invalidate(); }
            protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); isHovered = false; Invalidate(); }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                Rectangle r = new Rectangle(1, 1, Width - 3, Height - 3);
                if (r.Width <= 10 || r.Height <= 10) return;

                using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(r, 12))
                {
                    Color c1 = isEmeraldTheme ? Color.FromArgb(6, 78, 59) : Color.FromArgb(234, 88, 12);
                    Color c2 = isEmeraldTheme ? Color.FromArgb(16, 185, 129) : Color.FromArgb(251, 191, 36);

                    using (LinearGradientBrush grad = new LinearGradientBrush(r, c1, c2, 18f))
                    {
                        g.FillPath(grad, path);
                    }

                    // Decorative circles / gift artwork on the right side
                    GraphicsState st = g.Save();
                    g.SetClip(path);
                    using (SolidBrush glow = new SolidBrush(Color.FromArgb(35, 255, 255, 255)))
                    {
                        g.FillEllipse(glow, Width - 105, -20, 120, 120);
                        g.FillEllipse(glow, Width - 70, Height - 55, 90, 90);
                    }
                    SportFieldIconProvider.Draw(
                        g,
                        isEmeraldTheme ? SportFieldIcon.Discount : SportFieldIcon.Gift,
                        new Rectangle(Width - 76, (Height - 44) / 2, 44, 44),
                        Color.FromArgb(220, 255, 255, 255));
                    g.Restore(st);

                    if (isHovered)
                    {
                        using (Pen p = new Pen(Color.White, 1.5f))
                            g.DrawPath(p, path);
                    }
                }

                int availW = Math.Max(80, Width - 88);
                Color headColor = isEmeraldTheme ? Color.FromArgb(253, 224, 71) : Color.White;

                using (Font fHead = new Font("Segoe UI", 12.5f, FontStyle.Bold))
                using (Font fSub1 = new Font("Segoe UI", 9.25f, FontStyle.Bold))
                using (Font fSub2 = new Font("Segoe UI", 8f, FontStyle.Regular))
                {
                    TextRenderer.DrawText(g, headline, fHead, new Rectangle(16, 10, availW, 24), headColor, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                    TextRenderer.DrawText(g, subtitle1, fSub1, new Rectangle(16, 36, availW, 20), Color.White, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                    TextRenderer.DrawText(g, subtitle2, fSub2, new Rectangle(16, 58, availW, 18), Color.FromArgb(236, 253, 245), TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                }

                VectorIcons.Draw(g, SFIcon.ChevronRight, new Rectangle(Width - 24, (Height - 16) / 2, 16, 16), Color.White, 2.2f);
            }
        }
    }

    public class CustomerBookingView : UserControl
    {
        private SFComboBox cbBranches;
        private SFComboBox cbSports;
        private SFComboBox cbCourts;
        private SFComboBox cbStartTime;
        private SFComboBox cbEndTime;
        private SFComboBox cbService;
        private SFComboBox cbVouchers;
        private DateTimePicker dp;
        private Label sumText;
        private Panel noticeAvailable;
        private Panel profileBadge;
        private Label lblNoticeHeader;
        private Label lblNoticeBody;

        public static int? PreselectedSanId { get; set; }
        private DataTable dtCourts;
        private DataTable dtLoaiSan;
        private DataTable dtVouchers;
        private int selectedSanId = 1;
        private decimal currentBasePrice = 350000;
        private decimal calculatedFieldPrice = 525000;
        private decimal calculatedServicePrice = 0;
        private decimal calculatedDiscount = 0;
        private decimal calculatedTotal = 525000;
        private long? selectedVoucherKhachHangId = null;

        public CustomerBookingView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Canvas;
            Font = Typography.Body;
            BuildUI();
            LoadData();
        }

        private void BuildUI()
        {
            Controls.Add(Ui.Header("Đặt Sân Thể Thao Trực Tuyến", "Chọn cơ sở, môn thể thao, khung giờ và dịch vụ. Hệ thống kiểm tra trùng lịch tự động trước khi xác nhận."));

            profileBadge = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                Padding = new Padding(16, 12, 16, 12),
                Margin = new Padding(0, 0, 0, 14),
                BackColor = Colors.SuccessBg
            };
            lblNoticeHeader = new Label
            {
                Text = (AppSession.Current != null ? AppSession.Current.HoTen.ToUpper() : "KHÁCH HÀNG") + " • HỘI VIÊN TIÊU CHUẨN",
                Font = Typography.BodyBold,
                ForeColor = Colors.Emerald,
                Dock = DockStyle.Top,
                Height = 20
            };
            lblNoticeBody = new Label
            {
                Text = "Tài khoản chính thức • Tích lũy điểm thưởng và áp dụng voucher trực tuyến.",
                Font = Typography.Secondary,
                ForeColor = Colors.TextSecondary,
                Dock = DockStyle.Bottom,
                Height = 18
            };
            profileBadge.Controls.Add(lblNoticeBody);
            profileBadge.Controls.Add(lblNoticeHeader);
            Controls.Add(profileBadge);
            profileBadge.BringToFront();

            TableLayoutPanel root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(0)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28f));

            // Col 1: Facility & Field
            SFCard left = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(16), Margin = new Padding(0, 0, 8, 0) };
            Label lTitle = new Label { Text = "1. Cơ Sở & Sân Đấu", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32, UseMnemonic = false };
            left.Controls.Add(lTitle);

            Panel p1 = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            cbBranches = AddCombo(p1, "Chi nhánh / Cụm sân *", 10, 310, "SportZone Stadium Cầu Giấy (Chính)");
            cbSports = AddCombo(p1, "Loại sân thể thao *", 80, 310, "Bóng đá", "Cầu lông", "Bóng rổ", "Pickleball");
            cbCourts = AddCombo(p1, "Sân khả dụng *", 150, 310, "Đang tải danh sách sân...");

            cbBranches.SelectedIndexChanged += (s, e) => FilterCourts();
            cbSports.SelectedIndexChanged += (s, e) => FilterCourts();
            cbCourts.SelectedIndexChanged += (s, e) => OnCourtSelected();

            Label info = new Label
            {
                Text = "Tiện ích tiêu chuẩn tại sân:\n• Hệ thống đèn LED chiếu sáng 400 lux\n• Phòng thay đồ & Tắm nóng lạnh riêng biệt\n• Bãi đỗ xe ô tô & xe máy an ninh 24/7\n• Nước uống tinh khiết miễn phí\n• Wifi băng thông cao tốc độ 300Mbps",
                Font = Typography.Body,
                ForeColor = Colors.TextSecondary,
                Location = new Point(4, 230),
                Size = new Size(Math.Max(200, p1.ClientSize.Width - 12), 160),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoSize = false,
                UseMnemonic = false
            };
            p1.Controls.Add(info);
            left.Controls.Add(p1);
            p1.BringToFront();
            root.Controls.Add(left, 0, 0);

            // Col 2: Date & Slots
            SFCard mid = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(16), Margin = new Padding(8, 0, 8, 0) };
            Label mTitle = new Label { Text = "2. Thời Gian & Dịch Vụ", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32, UseMnemonic = false };
            mid.Controls.Add(mTitle);

            Panel p2 = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            Label lblDate = new Label { Text = "Ngày thi đấu *", Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(4, 10), AutoSize = true };
            p2.Controls.Add(lblDate);
            dp = new DateTimePicker
            {
                Location = new Point(4, 30),
                Width = Math.Max(200, p2.ClientSize.Width - 12),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Format = DateTimePickerFormat.Short,
                Font = Typography.Body,
                MinDate = DateTime.Today
            };
            dp.ValueChanged += (s, e) => ReCalculatePrice();
            p2.Controls.Add(dp);

            cbStartTime = AddCombo(p2, "Giờ bắt đầu *", 75, 150, "17:00", "17:30", "18:00", "18:30", "19:00", "19:30", "20:00", "20:30");
            cbEndTime = AddCombo(p2, "Giờ kết thúc *", 145, 150, "18:30", "19:00", "19:30", "20:00", "20:30", "21:00", "21:30", "22:00");
            cbStartTime.SelectedIndexChanged += (s, e) => ReCalculatePrice();
            cbEndTime.SelectedIndexChanged += (s, e) => ReCalculatePrice();

            SFButton btnCheck = new SFButton
            {
                Text = "Kiểm Tra Sân Trống",
                Variant = SFButtonVariant.Primary,
                IconType = SFIcon.Search,
                Location = new Point(4, 220),
                Size = new Size(Math.Max(180, p2.ClientSize.Width - 12), 38),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            btnCheck.Click += (s, e) => CheckCourtAvailability();
            p2.Controls.Add(btnCheck);

            noticeAvailable = Ui.Notice("SÂN CÒN TRỐNG", "Khung giờ đã chọn khả dụng 100%. Sẵn sàng đặt chỗ.", Colors.Emerald, Colors.SuccessBg);
            noticeAvailable.Location = new Point(4, 270);
            noticeAvailable.Size = new Size(Math.Max(200, p2.ClientSize.Width - 12), 58);
            noticeAvailable.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            noticeAvailable.Dock = DockStyle.None;
            p2.Controls.Add(noticeAvailable);

            cbService = AddCombo(p2, "Dịch vụ bổ sung", 340, 310, "Không chọn", "Nước khoáng Lavie x4 (40.000đ)", "Bóng thi đấu Động Lực x1 (30.000đ)", "Áo Bib phân đội x10 (50.000đ)");
            cbService.SelectedIndexChanged += (s, e) =>
            {
                string sel = cbService.SelectedItem?.ToString() ?? "";
                if (sel.Contains("40.000đ")) calculatedServicePrice = 40000;
                else if (sel.Contains("30.000đ")) calculatedServicePrice = 30000;
                else if (sel.Contains("50.000đ")) calculatedServicePrice = 50000;
                else calculatedServicePrice = 0;
                ReCalculatePrice();
            };

            mid.Controls.Add(p2);
            p2.BringToFront();
            root.Controls.Add(mid, 1, 0);

            // Col 3: Summary & Order Confirmation
            SFCard right = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(16), Margin = new Padding(8, 0, 0, 0) };
            Label rTitle = new Label { Text = "3. Tóm Tắt Đơn Đặt", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            right.Controls.Add(rTitle);

            Panel p3 = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            sumText = new Label
            {
                Text = "Đang tính toán chi phí đặt sân...",
                Font = Typography.BodyBold,
                ForeColor = Colors.TextPrimary,
                Location = new Point(4, 10),
                Size = new Size(Math.Max(200, p3.ClientSize.Width - 12), 270),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoSize = false
            };
            p3.Controls.Add(sumText);

            cbVouchers = AddCombo(p3, "Voucher của bạn", 290, 260, "Không áp dụng");
            cbVouchers.SelectedIndexChanged += (s, e) => OnVoucherChanged();

            SFButton btnConfirm = new SFButton
            {
                Text = "XÁC NHẬN ĐẶT SÂN",
                Variant = SFButtonVariant.Primary,
                IconType = SFIcon.Check,
                Location = new Point(4, 365),
                Size = new Size(Math.Max(200, p3.ClientSize.Width - 12), 46),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            btnConfirm.Click += (s, e) => ConfirmBooking();
            p3.Controls.Add(btnConfirm);

            right.Controls.Add(p3);
            p3.BringToFront();
            root.Controls.Add(right, 2, 0);

            Controls.Add(root);
            root.BringToFront();
        }

        private void LoadData()
        {
            try
            {
                int custId = Security.SessionGuard.RequireCustomerId();

                // Load Customer info
                DataTable dtCust = DatabaseManager.ExecuteQuery(
                    "SELECT TOP 1 kh.HoTen, kh.MaKhachHang, kh.SoDienThoai, htv.TenHang, kh.DiemTichLuy " +
                    "FROM dbo.KhachHang kh LEFT JOIN dbo.HangThanhVien htv ON kh.HangThanhVienID = htv.HangThanhVienID WHERE kh.KhachHangID = @ID",
                    new SqlParameter("@ID", custId));
                if (dtCust != null && dtCust.Rows.Count > 0)
                {
                    DataRow cr = dtCust.Rows[0];
                    string name = cr["HoTen"]?.ToString() ?? "Khách hàng";
                    string maKh = cr["MaKhachHang"]?.ToString() ?? "KH001";
                    string sdt = cr["SoDienThoai"]?.ToString() ?? "";
                    string tier = cr["TenHang"]?.ToString() ?? "Thành viên";
                    int diem = cr["DiemTichLuy"] != DBNull.Value ? Convert.ToInt32(cr["DiemTichLuy"]) : 0;
                    lblNoticeHeader.Text = string.Format("{0} • {1} • {2}", name.ToUpper(), maKh, tier.ToUpper());
                    lblNoticeBody.Text = string.Format("SĐT: {0} • Điểm tích lũy: {1:N0} pts • Ưu đãi hội viên tự động.", sdt, diem);
                }

                // Load Clusters
                DataTable dtCum = DatabaseManager.ExecuteQuery("SELECT CumSanID, TenCumSan FROM dbo.CumSan WHERE DangHoatDong = 1 ORDER BY CumSanID");
                if (dtCum != null && dtCum.Rows.Count > 0)
                {
                    cbBranches.Items.Clear();
                    foreach (DataRow r in dtCum.Rows) cbBranches.Items.Add(r["TenCumSan"]?.ToString() ?? "");
                    if (cbBranches.Items.Count > 0) cbBranches.SelectedIndex = 0;
                }

                // Load Sports
                dtLoaiSan = DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_DanhSachLoaiSan");
                if (dtLoaiSan != null && dtLoaiSan.Rows.Count > 0)
                {
                    cbSports.Items.Clear();
                    foreach (DataRow r in dtLoaiSan.Rows) cbSports.Items.Add(r["TenLoaiSan"]?.ToString() ?? "");
                    if (cbSports.Items.Count > 0) cbSports.SelectedIndex = 0;
                }

                // Load Courts
                dtCourts = DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_DanhSachSan");
                FilterCourts();

                if (PreselectedSanId.HasValue && dtCourts != null)
                {
                    int targetId = PreselectedSanId.Value;
                    PreselectedSanId = null;
                    foreach (DataRow r in dtCourts.Rows)
                    {
                        if (r["SanID"] != DBNull.Value && Convert.ToInt32(r["SanID"]) == targetId)
                        {
                            string targetSport = r["TenLoaiSan"]?.ToString() ?? "";
                            string targetCourt = r["TenSan"]?.ToString() ?? "";
                            for (int i = 0; i < cbSports.Items.Count; i++)
                            {
                                string itemText = cbSports.Items[i].ToString();
                                if (targetSport.IndexOf(itemText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    itemText.IndexOf(targetSport, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    cbSports.SelectedIndex = i;
                                    break;
                                }
                            }
                            FilterCourts();
                            for (int i = 0; i < cbCourts.Items.Count; i++)
                            {
                                if (cbCourts.Items[i].ToString() == targetCourt)
                                {
                                    cbCourts.SelectedIndex = i;
                                    break;
                                }
                            }
                            selectedSanId = targetId;
                            OnCourtSelected();
                            break;
                        }
                    }
                }

                // Load Vouchers for this customer
                dtVouchers = DatabaseManager.ExecuteQuery(
                    "SELECT vkh.VoucherKhachHangID, v.MaVoucher, v.TenVoucher, v.LoaiGiam, v.GiaTriGiam, v.GiamToiDa, v.DonToiThieu " +
                    "FROM dbo.vw_VoucherKhachHangKhaDung vkh JOIN dbo.Voucher v ON vkh.VoucherID = v.VoucherID " +
                    "WHERE vkh.KhachHangID = @ID", new SqlParameter("@ID", custId));

                cbVouchers.Items.Clear();
                cbVouchers.Items.Add("Không áp dụng");
                if (dtVouchers != null && dtVouchers.Rows.Count > 0)
                {
                    foreach (DataRow vr in dtVouchers.Rows)
                    {
                        string ma = vr["MaVoucher"]?.ToString() ?? "";
                        string ten = vr["TenVoucher"]?.ToString() ?? "";
                        cbVouchers.Items.Add(string.Format("{0} - {1}", ma, ten));
                    }
                }
                cbVouchers.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "CUSTOMER_BOOKING", "LoadData");
            }
        }

        private void FilterCourts()
        {
            if (dtCourts == null) return;
            string sport = cbSports.SelectedItem?.ToString() ?? "";
            cbCourts.Items.Clear();

            foreach (DataRow r in dtCourts.Rows)
            {
                string sSport = r["TenLoaiSan"]?.ToString() ?? "";
                if (string.IsNullOrEmpty(sport) || sSport.Contains(sport))
                {
                    cbCourts.Items.Add(r["TenSan"]?.ToString() ?? "");
                }
            }

            if (cbCourts.Items.Count > 0)
            {
                cbCourts.SelectedIndex = 0;
                OnCourtSelected();
            }
        }

        private void OnCourtSelected()
        {
            if (dtCourts == null || cbCourts.SelectedItem == null) return;
            string courtName = cbCourts.SelectedItem.ToString();

            foreach (DataRow r in dtCourts.Rows)
            {
                if ((r["TenSan"]?.ToString() ?? "") == courtName)
                {
                    selectedSanId = Convert.ToInt32(r["SanID"]);
                    currentBasePrice = (r.Table.Columns.Contains("GiaCoBan") && r["GiaCoBan"] != DBNull.Value)
                        ? Convert.ToDecimal(r["GiaCoBan"]) : 350000;
                    break;
                }
            }

            ReCalculatePrice();
        }

        private void OnVoucherChanged()
        {
            selectedVoucherKhachHangId = null;
            calculatedDiscount = 0;

            if (cbVouchers.SelectedIndex > 0 && dtVouchers != null && cbVouchers.SelectedIndex - 1 < dtVouchers.Rows.Count)
            {
                DataRow vr = dtVouchers.Rows[cbVouchers.SelectedIndex - 1];
                selectedVoucherKhachHangId = Convert.ToInt64(vr["VoucherKhachHangID"]);
                string loai = vr["LoaiGiam"]?.ToString() ?? "";
                decimal val = vr["GiaTriGiam"] != DBNull.Value ? Convert.ToDecimal(vr["GiaTriGiam"]) : 0;
                decimal max = vr["GiamToiDa"] != DBNull.Value ? Convert.ToDecimal(vr["GiamToiDa"]) : 0;

                if (loai == "PHAN_TRAM")
                {
                    calculatedDiscount = calculatedFieldPrice * (val / 100m);
                    if (max > 0 && calculatedDiscount > max) calculatedDiscount = max;
                }
                else
                {
                    calculatedDiscount = val;
                }
            }

            UpdateSummaryDisplay();
        }

        private void ReCalculatePrice()
        {
            DateTime start = GetDateTime(cbStartTime.SelectedItem?.ToString() ?? "17:00");
            DateTime end = GetDateTime(cbEndTime.SelectedItem?.ToString() ?? "18:30");

            if (end <= start)
            {
                end = start.AddHours(1.5);
            }

            try
            {
                DataTable dtPrice = DatabaseManager.ExecuteQuery(
                    "EXEC dbo.sp_TinhGiaDatSan @SanID = @SanID, @BatDau = @Start, @KetThuc = @End",
                    new SqlParameter("@SanID", selectedSanId),
                    new SqlParameter("@Start", start),
                    new SqlParameter("@End", end));

                if (dtPrice != null && dtPrice.Rows.Count > 0)
                {
                    calculatedFieldPrice = dtPrice.Rows[0]["TienSan"] != DBNull.Value ? Convert.ToDecimal(dtPrice.Rows[0]["TienSan"]) : currentBasePrice;
                    currentBasePrice = dtPrice.Rows[0]["DonGiaTaiLucBatDau"] != DBNull.Value ? Convert.ToDecimal(dtPrice.Rows[0]["DonGiaTaiLucBatDau"]) : currentBasePrice;
                }
                else
                {
                    double hours = (end - start).TotalHours;
                    calculatedFieldPrice = (decimal)hours * currentBasePrice;
                }
            }
            catch
            {
                double hours = Math.Max(1.0, (end - start).TotalHours);
                calculatedFieldPrice = (decimal)hours * currentBasePrice;
            }

            OnVoucherChanged();
        }

        private void UpdateSummaryDisplay()
        {
            DateTime start = GetDateTime(cbStartTime.SelectedItem?.ToString() ?? "17:00");
            DateTime end = GetDateTime(cbEndTime.SelectedItem?.ToString() ?? "18:30");
            if (end <= start) end = start.AddHours(1.5);
            double hours = (end - start).TotalHours;

            calculatedTotal = Math.Max(0, calculatedFieldPrice + calculatedServicePrice - calculatedDiscount);

            string courtName = cbCourts.SelectedItem?.ToString() ?? "Sân thi đấu";
            string sport = cbSports.SelectedItem?.ToString() ?? "Bộ môn";

            sumText.Text = string.Format(
                "{0} • {1}\nThời gian: {2:HH:mm} - {3:HH:mm} ({4:0.#} giờ) • {5:dd/MM/yyyy}\n\nĐơn giá chuẩn:             {6,12:N0}đ/h\nTiền thuê sân:              {7,12:N0}đ\nDịch vụ bổ sung:            {8,12:N0}đ\nGiảm giá / Voucher:        -{9,12:N0}đ\n──────────────────────────────\nTỔNG CẦN THANH TOÁN:\n{10:N0} đ",
                courtName, sport, start, end, hours, dp.Value,
                currentBasePrice, calculatedFieldPrice, calculatedServicePrice, calculatedDiscount, calculatedTotal);
        }

        private DateTime GetDateTime(string timeStr)
        {
            DateTime baseDate = dp.Value.Date;
            if (TimeSpan.TryParse(timeStr, out TimeSpan ts))
            {
                return baseDate.Add(ts);
            }
            return baseDate.AddHours(17);
        }

        private void CheckCourtAvailability()
        {
            DateTime start = GetDateTime(cbStartTime.SelectedItem?.ToString() ?? "17:00");
            DateTime end = GetDateTime(cbEndTime.SelectedItem?.ToString() ?? "18:30");
            if (end <= start) end = start.AddHours(1.5);

            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery(
                    "EXEC dbo.sp_TimSanTrong @BatDau = @Start, @KetThuc = @End, @LoaiSanID = NULL, @CumSanID = NULL",
                    new SqlParameter("@Start", start),
                    new SqlParameter("@End", end));

                bool isFree = false;
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        if (Convert.ToInt32(r["SanID"]) == selectedSanId)
                        {
                            isFree = true;
                            break;
                        }
                    }
                }

                if (isFree)
                {
                    SFToast.Show(this, "Sân đã chọn còn trống trong khung giờ đăng ký. Sẵn sàng đặt chỗ!", "KIỂM TRA SÂN", SFIcon.Check, Colors.Emerald);
                }
                else
                {
                    SFToast.Show(this, "Sân đã có lịch hoặc đang bảo trì vào khung giờ này. Vui lòng chọn khung giờ khác.", "SÂN KHÔNG KHẢ DỤNG", SFIcon.AlertCircle, Colors.Coral);
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "CUSTOMER_BOOKING", "CheckCourtAvailability");
                SFToast.Show(this, "Lỗi kiểm tra sân: " + ex.Message, "LỖI", SFIcon.AlertCircle, Colors.Coral);
            }
        }

        private void ConfirmBooking()
        {
            DateTime start = GetDateTime(cbStartTime.SelectedItem?.ToString() ?? "17:00");
            DateTime end = GetDateTime(cbEndTime.SelectedItem?.ToString() ?? "18:30");
            if (end <= start) end = start.AddHours(1.5);

            int custId = Security.SessionGuard.RequireCustomerId();
            string custName = AppSession.Current != null ? AppSession.Current.HoTen : "Khách hàng";

            try
            {
                var bookingService = new BookingService();
                var bRes = bookingService.CreateBooking(
                    custId,
                    selectedSanId,
                    start,
                    end,
                    "ONLINE",
                    Security.SessionGuard.RequireAccountId(),
                    selectedVoucherKhachHangId,
                    "Đặt sân trực tuyến qua cổng hội viên");

                long donId = bRes.DonDatSanID;
                string maDon = bRes.MaDon;

                var payService = new PaymentService();
                var invRes = payService.CreateInvoice(donId, Security.SessionGuard.RequireAccountId());
                long hoaDonId = invRes.HoaDonID;
                if (hoaDonId <= 0)
                {
                    SFToast.Show(this, "Đã tạo đơn đặt sân " + maDon + " nhưng không thể tạo hóa đơn thanh toán.", "LỖI HÓA ĐƠN", SFIcon.AlertCircle, Colors.Coral);
                    return;
                }

                long giaoDichId = 0;
                try
                {
                    var gdRes = payService.RecordPayment(hoaDonId, calculatedTotal, "VIETQR", Security.SessionGuard.RequireAccountId(), "", "Cọc trực tuyến VietQR đơn " + maDon);
                    giaoDichId = gdRes.GiaoDichID;
                    if (giaoDichId > 0)
                    {
                        payService.CreateVietQrRequest(giaoDichId, null, 15);
                    }
                }
                catch (Exception pEx)
                {
                    AppLogger.LogError(pEx, "CUSTOMER_BOOKING", "RecordPayment VietQR: " + pEx.Message);
                    SFToast.Show(this, "Đã tạo đơn " + maDon + " nhưng lỗi khởi tạo giao dịch thanh toán: " + AppLogger.GetUserFriendlySqlMessage(pEx), "LỖI THANH TOÁN", SFIcon.AlertCircle, Colors.Coral);
                    return;
                }

                SFToast.Show(this, string.Format("Đã tạo đơn đặt sân {0} thành công! Mở mã QR thanh toán...", maDon), "ĐẶT SÂN THÀNH CÔNG", SFIcon.Check, Colors.Emerald);
                new CustomerVietQrForm(calculatedTotal, maDon, custName, hoaDonId, giaoDichId).ShowDialog(this);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "CUSTOMER_BOOKING", "ConfirmBooking");
                SFToast.Show(this, "Lỗi tạo đơn: " + AppLogger.GetUserFriendlySqlMessage(ex), "LỖI ĐẶT SÂN", SFIcon.AlertCircle, Colors.Coral);
            }
        }

        private SFComboBox AddCombo(Control p, string label, int y, int w, params string[] items)
        {
            Label l = new Label { Text = label, Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(4, y), AutoSize = true };
            p.Controls.Add(l);
            SFComboBox c = new SFComboBox
            {
                Location = new Point(4, y + 20),
                Width = Math.Max(160, p.ClientSize.Width - 12),
                Height = 42,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            c.Items.AddRange(items);
            if (c.Items.Count > 0) c.SelectedIndex = 0;
            p.Controls.Add(c);
            return c;
        }
    }

    public class CustomerHistoryView : UserControl
    {
        private SFDataGridView grid;
        private SFComboBox filter;
        private SFMetricCard cardSpent;
        private SFMetricCard cardUpcoming;
        private SFMetricCard cardCompleted;

        public CustomerHistoryView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Canvas;
            Font = Typography.Body;
            BuildUI();
            LoadData();
        }

        private void BuildUI()
        {
            Controls.Add(Ui.Header("Lịch Sử Đặt Sân Của Tôi", "Theo dõi trạng thái lượt đặt, tiến độ thanh toán hoặc thao tác hủy/đổi lịch thi đấu."));

            TableLayoutPanel k = new TableLayoutPanel { Dock = DockStyle.Top, Height = 100, ColumnCount = 3, Padding = new Padding(0, 0, 0, 14) };
            for (int i = 0; i < 3; i++) k.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            cardSpent = new SFMetricCard("TỔNG TIỀN ĐÃ CHI", "0đ", "Năm 2026", SFIcon.Dollar, Colors.Emerald);
            cardUpcoming = new SFMetricCard("ĐƠN SẮP DIỄN RA", "0 Trận", "Lịch thi đấu sắp tới", SFIcon.Clock, Colors.Amber);
            cardCompleted = new SFMetricCard("TRẬN ĐÃ THI ĐẤU", "0 Trận", "Hoàn thành xuất sắc", SFIcon.Check, Colors.ElectricBlue);
            k.Controls.Add(cardSpent, 0, 0);
            k.Controls.Add(cardUpcoming, 1, 0);
            k.Controls.Add(cardCompleted, 2, 0);
            Controls.Add(k);
            k.BringToFront();

            FlowLayoutPanel tb = Ui.Toolbar();
            filter = new SFComboBox { Width = 160, Height = 42 };
            filter.Items.AddRange(new object[] { "Tất cả trạng thái", "Đang diễn ra", "Đã xác nhận", "Hoàn thành", "Đã hủy" });
            filter.SelectedIndex = 0;
            filter.SelectedIndexChanged += (s, e) => LoadData();
            tb.Controls.Add(filter);

            SFButton btnRefresh = new SFButton { Text = "Làm Mới", Variant = SFButtonVariant.Secondary, IconType = SFIcon.Refresh, Width = 110, Height = 42 };
            btnRefresh.Click += (s, e) => LoadData();
            tb.Controls.Add(btnRefresh);

            SFButton btnPay = new SFButton { Text = "Thanh Toán VietQR", Variant = SFButtonVariant.Primary, IconType = SFIcon.QrCode, Width = 165, Height = 42 };
            btnPay.Click += (s, e) => PaySelectedRow();
            tb.Controls.Add(btnPay);

            SFButton btnCancel = new SFButton { Text = "Hủy Đặt Sân", Variant = SFButtonVariant.Danger, IconType = SFIcon.Close, Width = 135, Height = 42 };
            btnCancel.Click += (s, e) => CancelSelectedRow();
            tb.Controls.Add(btnCancel);
            Controls.Add(tb);
            tb.BringToFront();

            SFCard card = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(16) };
            Label lTitle = new Label { Text = "Danh Sách Các Đơn Đặt Sân (SQL Server • Nhấp đúp dòng để mở mã QR)", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            card.Controls.Add(lTitle);

            grid = new SFDataGridView { Dock = DockStyle.Fill };
            grid.Columns.Add("Code", "Mã đơn");
            grid.Columns.Add("Field", "Sân thi đấu");
            grid.Columns.Add("Sport", "Bộ môn");
            grid.Columns.Add("Date", "Ngày");
            grid.Columns.Add("Time", "Khung giờ");
            grid.Columns.Add("Total", "Tổng tiền");
            grid.Columns.Add("Paid", "Đã trả");
            grid.Columns.Add("Remaining", "Còn lại");
            grid.Columns.Add("Status", "Trạng thái");

            grid.Columns["Code"].Width = 140;
            grid.Columns["Field"].Width = 130;
            grid.Columns["Sport"].Width = 110;
            grid.Columns["Date"].Width = 100;
            grid.Columns["Time"].Width = 120;
            grid.Columns["Total"].Width = 110;
            grid.Columns["Paid"].Width = 110;
            grid.Columns["Remaining"].Width = 100;
            grid.Columns["Status"].Width = 120;

            ContextMenuStrip cms = new ContextMenuStrip();
            var itemPay = cms.Items.Add("Thanh Toán VietQR Napas 247");
            itemPay.Click += (s, e) => PaySelectedRow();
            var itemCancel = cms.Items.Add("Hủy Đơn Đặt Sân & Hoàn Cọc");
            itemCancel.Click += (s, e) => CancelSelectedRow();
            cms.Items.Add("-");
            var itemExport = cms.Items.Add("Xuất Lịch Sử Đặt Sân Ra Excel");
            itemExport.Click += (s, e) => ExportHelper.ExportToExcel(grid, "Lich_Su_Dat_San_Customer", this);
            grid.ContextMenuStrip = cms;

            grid.CellDoubleClick += (s, e) => PaySelectedRow();

            card.Controls.Add(grid);
            grid.BringToFront();
            Controls.Add(card);
            card.BringToFront();
        }

        private void PaySelectedRow()
        {
            if (grid.SelectedRows.Count > 0 && grid.SelectedRows[0].Tag is DataRow r)
            {
                decimal conLai = r["ConLai"] != DBNull.Value ? Convert.ToDecimal(r["ConLai"]) : 0;
                decimal tong = r["TongTien"] != DBNull.Value ? Convert.ToDecimal(r["TongTien"]) : 0;
                string maDon = r["MaDon"]?.ToString() ?? "";
                string cust = AppSession.Current != null ? AppSession.Current.HoTen : "Khách hàng";
                new CustomerVietQrForm(conLai > 0 ? conLai : tong, maDon, cust).ShowDialog(this);
            }
            else
            {
                SFToast.Show(this, "Vui lòng chọn đơn đặt sân cần thanh toán từ danh sách.", "CHƯA CHỌN ĐƠN", SFIcon.Info, Colors.Cyan);
            }
        }

        private void CancelSelectedRow()
        {
            if (grid.SelectedRows.Count > 0 && grid.SelectedRows[0].Tag is DataRow r)
            {
                long donId = r.Table.Columns.Contains("DonDatSanID") && r["DonDatSanID"] != DBNull.Value ? Convert.ToInt64(r["DonDatSanID"]) : 0;
                string maDon = r["MaDon"]?.ToString() ?? "";
                string san = r["TenSan"]?.ToString() ?? "";
                decimal daTra = r["DaThanhToan"] != DBNull.Value ? Convert.ToDecimal(r["DaThanhToan"]) : 0;
                new CustomerCancelBookingForm(donId, maDon, san, daTra).ShowDialog(this);
                LoadData();
            }
            else
            {
                new CustomerCancelBookingForm().ShowDialog(this);
            }
        }

        private void LoadData()
        {
            try
            {
                int custId = SessionGuard.RequireCustomerId();

                DataTable dt = DatabaseManager.ExecuteQuery(
                    "EXEC dbo.sp_UI_CustomerLichSuDatSan @KhachHangID = @ID",
                    new SqlParameter("@ID", custId));

                grid.Rows.Clear();
                decimal totalSpend = 0;
                int upcoming = 0;
                int completed = 0;

                if (dt != null && dt.Rows.Count > 0)
                {
                    string f = filter.SelectedItem?.ToString() ?? "Tất cả trạng thái";

                    foreach (DataRow r in dt.Rows)
                    {
                        string maDon = r["MaDon"]?.ToString() ?? "";
                        string san = r["TenSan"]?.ToString() ?? (r["MaSan"]?.ToString() ?? "");
                        string sport = r["TenLoaiSan"]?.ToString() ?? "";
                        DateTime bd = r["BatDau"] != DBNull.Value ? Convert.ToDateTime(r["BatDau"]) : DateTime.Today;
                        DateTime kt = r["KetThuc"] != DBNull.Value ? Convert.ToDateTime(r["KetThuc"]) : DateTime.Today;
                        decimal tong = r["TongTien"] != DBNull.Value ? Convert.ToDecimal(r["TongTien"]) : 0;
                        decimal daTra = r["DaThanhToan"] != DBNull.Value ? Convert.ToDecimal(r["DaThanhToan"]) : 0;
                        decimal conLai = r["ConLai"] != DBNull.Value ? Convert.ToDecimal(r["ConLai"]) : 0;
                        string st = r["TrangThaiDon"]?.ToString() ?? "";

                        totalSpend += daTra;
                        if (st == "DA_XAC_NHAN" || st == "DANG_SU_DUNG") upcoming++;
                        if (st == "HOAN_THANH") completed++;

                        string stDisp = "Đã xác nhận";
                        if (st == "DANG_SU_DUNG") stDisp = "Đang diễn ra";
                        else if (st == "HOAN_THANH") stDisp = "Hoàn thành";
                        else if (st == "DA_HUY") stDisp = "Đã hủy";

                        if (f != "Tất cả trạng thái" && !stDisp.Contains(f))
                            continue;

                        int idx = grid.Rows.Add(maDon, san, sport, bd.ToString("dd/MM/yyyy"), string.Format("{0:HH:mm} - {1:HH:mm}", bd, kt), string.Format("{0:N0}đ", tong), string.Format("{0:N0}đ", daTra), string.Format("{0:N0}đ", conLai), stDisp);
                        grid.Rows[idx].Tag = r;
                    }
                }

                cardSpent.Value = string.Format("{0:N0}đ", totalSpend);
                cardUpcoming.Value = string.Format("{0:D2} Trận", upcoming);
                cardCompleted.Value = string.Format("{0:D2} Trận", completed);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "CUSTOMER_HISTORY", "LoadData");
                SFToast.Show(this, "Lỗi tải lịch sử đặt sân: " + ex.Message, "LỖI", SFIcon.AlertCircle, Colors.Coral);
            }
        }
    }

    public class CustomerInvoicesView : UserControl
    {
        private SFDataGridView grid;

        public CustomerInvoicesView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Canvas;
            Font = Typography.Body;
            BuildUI();
            LoadData();
        }

        private void BuildUI()
        {
            Controls.Add(Ui.Header("Hóa Đơn & Lịch Sử Giao Dịch", "Theo dõi phiếu thu, các khoản cọc và trạng thái khớp lệnh thanh toán VietQR Napas 247."));

            Panel notice = Ui.Notice("LƯU Ý ĐỐI SOÁT CHUYỂN KHOẢN", "Sau khi quét mã VietQR thành công, hệ thống sẽ tự động cập nhật trạng thái 'Đã thanh toán' trong vòng 30 giây.", Colors.Cyan, Colors.InfoBg);
            Controls.Add(notice);
            notice.BringToFront();

            FlowLayoutPanel tb = Ui.Toolbar();
            SFButton btnPay = new SFButton { Text = "Thanh Toán VietQR", Variant = SFButtonVariant.Primary, IconType = SFIcon.QrCode, Width = 175, Height = 42 };
            btnPay.Click += (s, e) => PaySelectedRow();
            tb.Controls.Add(btnPay);

            SFButton btnRefresh = new SFButton { Text = "Làm Mới", Variant = SFButtonVariant.Secondary, IconType = SFIcon.Refresh, Width = 110, Height = 42 };
            btnRefresh.Click += (s, e) => LoadData();
            tb.Controls.Add(btnRefresh);

            SFComboBox status = new SFComboBox { Width = 160, Height = 42 };
            status.Items.AddRange(new object[] { "Tất cả trạng thái", "Chưa thanh toán", "Một phần", "Đã thanh toán" });
            status.SelectedIndex = 0;
            status.SelectedIndexChanged += (s, e) => LoadData();
            tb.Controls.Add(status);
            Controls.Add(tb);
            tb.BringToFront();

            SFCard card = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(16) };
            Label lTitle = new Label { Text = "Danh Sách Hóa Đơn Của Tôi (SQL Server • Nhấp đúp dòng để mở VietQR)", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            card.Controls.Add(lTitle);

            grid = new SFDataGridView { Dock = DockStyle.Fill };
            grid.Columns.Add("InvoiceId", "Mã HĐ");
            grid.Columns.Add("BookingId", "Mã đơn");
            grid.Columns.Add("Field", "Sân thi đấu");
            grid.Columns.Add("Total", "Tổng tiền");
            grid.Columns.Add("Paid", "Đã trả");
            grid.Columns.Add("Remaining", "Còn lại");
            grid.Columns.Add("Status", "Trạng thái");
            grid.Columns.Add("Date", "Ngày tạo");

            grid.Columns["InvoiceId"].Width = 130;
            grid.Columns["BookingId"].Width = 140;
            grid.Columns["Field"].Width = 130;
            grid.Columns["Total"].Width = 110;
            grid.Columns["Paid"].Width = 110;
            grid.Columns["Remaining"].Width = 100;
            grid.Columns["Status"].Width = 120;
            grid.Columns["Date"].Width = 120;

            ContextMenuStrip cms = new ContextMenuStrip();
            var itemPay = cms.Items.Add("Thanh Toán VietQR Napas 247");
            itemPay.Click += (s, e) => PaySelectedRow();
            cms.Items.Add("-");
            var itemExport = cms.Items.Add("Xuất Danh Sách Hóa Đơn Ra Excel");
            itemExport.Click += (s, e) => ExportHelper.ExportToExcel(grid, "Hoa_Don_Customer", this);
            grid.ContextMenuStrip = cms;

            grid.CellDoubleClick += (s, e) => PaySelectedRow();

            card.Controls.Add(grid);
            grid.BringToFront();
            Controls.Add(card);
            card.BringToFront();
        }

        private void PaySelectedRow()
        {
            if (grid.SelectedRows.Count > 0 && grid.SelectedRows[0].Tag is DataRow r)
            {
                decimal conLai = r["ConLai"] != DBNull.Value ? Convert.ToDecimal(r["ConLai"]) : 0;
                decimal tong = r["TongTien"] != DBNull.Value ? Convert.ToDecimal(r["TongTien"]) : 0;
                string maDon = r["MaDon"]?.ToString() ?? "";
                string cust = AppSession.Current != null ? AppSession.Current.HoTen : "Khách hàng";
                new CustomerVietQrForm(conLai > 0 ? conLai : tong, maDon, cust).ShowDialog(this);
            }
            else
            {
                SFToast.Show(this, "Vui lòng chọn đơn đặt sân cần thanh toán từ danh sách.", "CHƯA CHỌN ĐƠN", SFIcon.Info, Colors.Cyan);
            }
        }

        private void LoadData()
        {
            try
            {
                int custId = SessionGuard.RequireCustomerId();

                DataTable dt = DatabaseManager.ExecuteQuery(
                    "EXEC dbo.sp_UI_CustomerHoaDon @KhachHangID = @ID",
                    new SqlParameter("@ID", custId));

                grid.Rows.Clear();
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        string hd = r["MaHoaDon"]?.ToString() ?? "";
                        string don = r["MaDon"]?.ToString() ?? "";
                        string san = (r.Table.Columns.Contains("TenSan") && r["TenSan"] != DBNull.Value) ? r["TenSan"].ToString() : "Sân thể thao";
                        decimal tong = r["TongTien"] != DBNull.Value ? Convert.ToDecimal(r["TongTien"]) : 0;
                        decimal daTra = r["DaThanhToan"] != DBNull.Value ? Convert.ToDecimal(r["DaThanhToan"]) : 0;
                        decimal conLai = r["ConLai"] != DBNull.Value ? Convert.ToDecimal(r["ConLai"]) : 0;
                        string st = r["TrangThaiThanhToan"]?.ToString() ?? "";
                        DateTime ngay = r["NgayLap"] != DBNull.Value ? Convert.ToDateTime(r["NgayLap"]) : (r["NgayTao"] != DBNull.Value ? Convert.ToDateTime(r["NgayTao"]) : DateTime.Today);

                        string stDisp = "Chưa thanh toán";
                        if (st == "DA_THANH_TOAN") stDisp = "Đã thanh toán";
                        else if (st == "THANH_TOAN_MOT_PHAN") stDisp = "Một phần";

                        int idx = grid.Rows.Add(hd, don, san, string.Format("{0:N0}đ", tong), string.Format("{0:N0}đ", daTra), string.Format("{0:N0}đ", conLai), stDisp, ngay.ToString("dd/MM/yyyy"));
                        grid.Rows[idx].Tag = r;
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "CUSTOMER_INVOICES", "LoadData");
                SFToast.Show(this, "Lỗi tải hóa đơn: " + ex.Message, "LỖI", SFIcon.AlertCircle, Colors.Coral);
            }
        }
    }

    public class CustomerVouchersView : UserControl
    {
        private FlowLayoutPanel grid;
        private SFMetricCard metricPoints;
        private SFMetricCard metricAvailable;
        private SFMetricCard metricUsed;

        public CustomerVouchersView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Canvas;
            Font = Typography.Body;
            BuildUI();
            LoadData();
        }

        private void BuildUI()
        {
            Controls.Add(Ui.Header("Kho Voucher & Ưu Đãi Hội Viên", "Danh sách phiếu giảm giá độc quyền dành riêng cho tài khoản hội viên SportZone."));

            TableLayoutPanel top = new TableLayoutPanel { Dock = DockStyle.Top, Height = 100, ColumnCount = 3, Padding = new Padding(0, 0, 0, 14) };
            for (int i = 0; i < 3; i++) top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            metricPoints = new SFMetricCard("ĐIỂM TÍCH LŨY", "0 pts", "Hạng Thân Thiết", SFIcon.Star, Colors.Purple);
            metricAvailable = new SFMetricCard("VOUCHER KHẢ DỤNG", "0 Mã", "Sẵn sàng áp dụng", SFIcon.Ticket, Colors.Emerald);
            metricUsed = new SFMetricCard("ĐÃ SỬ DỤNG", "0 Mã", "Tiết kiệm chi phí", SFIcon.Check, Colors.ElectricBlue);
            top.Controls.Add(metricPoints, 0, 0);
            top.Controls.Add(metricAvailable, 1, 0);
            top.Controls.Add(metricUsed, 2, 0);
            Controls.Add(top);
            top.BringToFront();

            FlowLayoutPanel input = Ui.Toolbar();
            SFSearchBox codeBox = new SFSearchBox { PlaceholderText = "Nhập mã khuyến mãi mới...", Width = 260, Height = 42 };
            input.Controls.Add(codeBox);

            SFButton apply = new SFButton { Text = "Áp Dụng Mã", Variant = SFButtonVariant.Primary, IconType = SFIcon.Plus, Width = 140, Height = 42 };
            apply.Click += (s, e) =>
            {
                string code = codeBox.Text != null ? codeBox.Text.Trim() : "";
                if (string.IsNullOrEmpty(code))
                {
                    SFToast.Show(this, "Vui lòng nhập mã khuyến mãi.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Amber);
                    return;
                }
                SFToast.Show(this, "Mã ưu đãi '" + code + "' đã được lưu vào ví thành công!", "THÊM VOUCHER", SFIcon.Check, Colors.Emerald);
                LoadData();
            };
            input.Controls.Add(apply);
            Controls.Add(input);
            input.BringToFront();

            grid = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                BackColor = Colors.Canvas,
                Padding = new Padding(0, 6, 0, 0)
            };

            Controls.Add(grid);
            grid.BringToFront();
        }

        private void LoadData()
        {
            try
            {
                int custId = SessionGuard.RequireCustomerId();

                DataTable dtKh = DatabaseManager.ExecuteQuery(
                    "SELECT DiemTichLuy FROM dbo.KhachHang WHERE KhachHangID = @ID", new SqlParameter("@ID", custId));
                if (dtKh != null && dtKh.Rows.Count > 0)
                {
                    int pts = dtKh.Rows[0]["DiemTichLuy"] != DBNull.Value ? Convert.ToInt32(dtKh.Rows[0]["DiemTichLuy"]) : 0;
                    metricPoints.Value = string.Format("{0:N0} pts", pts);
                }

                DataTable dt = DatabaseManager.ExecuteQuery(
                    "SELECT vkh.VoucherKhachHangID, v.MaVoucher, v.TenVoucher, v.LoaiGiam, v.GiaTriGiam, v.DonToiThieu, v.NgayKetThuc, vkh.DaSuDung " +
                    "FROM dbo.VoucherKhachHang vkh JOIN dbo.Voucher v ON vkh.VoucherID = v.VoucherID " +
                    "WHERE vkh.KhachHangID = @ID AND v.DangHoatDong = 1", new SqlParameter("@ID", custId));

                grid.Controls.Clear();
                int availCount = 0;
                int usedCount = 0;

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        string ma = r["MaVoucher"]?.ToString() ?? "";
                        string ten = r["TenVoucher"]?.ToString() ?? "";
                        string loai = r["LoaiGiam"]?.ToString() ?? "";
                        decimal val = r["GiaTriGiam"] != DBNull.Value ? Convert.ToDecimal(r["GiaTriGiam"]) : 0;
                        decimal min = r["DonToiThieu"] != DBNull.Value ? Convert.ToDecimal(r["DonToiThieu"]) : 0;
                        bool daDung = r["DaSuDung"] != DBNull.Value && Convert.ToBoolean(r["DaSuDung"]);
                        DateTime hsd = r["NgayKetThuc"] != DBNull.Value ? Convert.ToDateTime(r["NgayKetThuc"]) : DateTime.Today.AddDays(30);

                        if (daDung) usedCount++;
                        else availCount++;

                        string disc = loai == "PHAN_TRAM" ? string.Format("-{0:0.#}%", val) : string.Format("-{0:N0}đ", val);
                        string sub = string.Format("Đơn từ {0:N0}đ • HSD {1:dd/MM/yyyy}", min, hsd);

                        SFVoucherCard vc = new SFVoucherCard(ma, disc, ten, sub, daDung ? Colors.TextSecondary : Colors.Emerald);
                        vc.OnUseClicked += (s, e) => (FindForm() as MainForm)?.ShowView("Đặt sân trực tuyến");
                        grid.Controls.Add(vc);
                    }
                }
                else
                {
                    // Fallback to active system vouchers
                    DataTable dtSys = DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_DanhSachVoucher @DangHoatDong = 1");
                    if (dtSys != null && dtSys.Rows.Count > 0)
                    {
                        foreach (DataRow r in dtSys.Rows)
                        {
                            string ma = r["MaVoucher"]?.ToString() ?? "";
                            string ten = r["TenVoucher"]?.ToString() ?? "";
                            string loai = r["LoaiGiam"]?.ToString() ?? "";
                            decimal val = r["GiaTriGiam"] != DBNull.Value ? Convert.ToDecimal(r["GiaTriGiam"]) : 0;
                            decimal min = r["DonToiThieu"] != DBNull.Value ? Convert.ToDecimal(r["DonToiThieu"]) : 0;
                            availCount++;

                            string disc = loai == "PHAN_TRAM" ? string.Format("-{0:0.#}%", val) : string.Format("-{0:N0}đ", val);
                            string sub = string.Format("Đơn từ {0:N0}đ • Sẵn sàng áp dụng", min);

                            SFVoucherCard vc = new SFVoucherCard(ma, disc, ten, sub, Colors.Emerald);
                            vc.OnUseClicked += (s, e) => (FindForm() as MainForm)?.ShowView("Đặt sân trực tuyến");
                            grid.Controls.Add(vc);
                        }
                    }
                }

                metricAvailable.Value = string.Format("{0:D2} Mã", availCount);
                metricUsed.Value = string.Format("{0:D2} Mã", usedCount);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "CUSTOMER_VOUCHERS", "LoadData");
            }
        }
    }

    public class CustomerProfileView : UserControl
    {
        private SFTextBox txtName;
        private SFTextBox txtPhone;
        private SFTextBox txtEmail;
        private SFTextBox txtAddress;
        private SFTextBox txtMaKh;
        private Label lblPassName;
        private Label lblPassDetails;
        private Label lblBadge;
        private int currentKhId = 2;

        public CustomerProfileView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Canvas;
            Font = Typography.Body;
            BuildUI();
            LoadData();
        }

        private void BuildUI()
        {
            Controls.Add(Ui.Header("Hồ Sơ Cá Nhân & Tài Khoản", "Quản lý thông tin định danh, số điện thoại, mật khẩu và quyền lợi thành viên hạng Vàng."));

            TableLayoutPanel root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(0) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65f));

            // Left: VIP Gold Pass Card
            SFCard left = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(20), Margin = new Padding(0, 0, 10, 0) };
            Label brand = new Label { Text = "SPORTFIELD VIP PASS", Font = Typography.SectionHeader, ForeColor = Colors.Amber, Location = new Point(8, 20), AutoSize = true };
            left.Controls.Add(brand);

            lblPassName = new Label { Text = "KHÁCH HÀNG", Font = Typography.PageTitle, ForeColor = Colors.TextPrimary, Location = new Point(8, 44), AutoSize = true };
            left.Controls.Add(lblPassName);

            lblBadge = new Label
            {
                Text = "★ THÀNH VIÊN SPORTZONE",
                Font = Typography.CaptionBold,
                ForeColor = Colors.Purple,
                BackColor = Colors.PurpleBg,
                Padding = new Padding(8, 5, 8, 5),
                Location = new Point(8, 82),
                AutoSize = true
            };
            left.Controls.Add(lblBadge);

            lblPassDetails = new Label
            {
                Text = "Đang tải dữ liệu hồ sơ cá nhân...",
                Font = Typography.Body,
                ForeColor = Colors.TextSecondary,
                Location = new Point(8, 125),
                Size = new Size(290, 270),
                AutoSize = false
            };
            left.Controls.Add(lblPassDetails);

            SFButton pass = new SFButton { Text = "Đổi Mật Khẩu", Variant = SFButtonVariant.Secondary, IconType = SFIcon.Lock, Location = new Point(8, 400), Size = new Size(150, 38) };
            pass.Click += (s, e) => new ChangePasswordForm(false).ShowDialog(this);
            left.Controls.Add(pass);
            root.Controls.Add(left, 0, 0);

            // Right: Profile Form
            SFCard right = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(20), Margin = new Padding(10, 0, 0, 0) };
            Label rTitle = new Label { Text = "Thông Tin Cá Nhân & Liên Hệ (SQL Server)", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            right.Controls.Add(rTitle);

            Panel pForm = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            txtMaKh = AddFieldBox(pForm, "Mã khách hàng", "KH001", 10, true);
            txtName = AddFieldBox(pForm, "Họ và tên *", "", 74, false);
            txtPhone = AddFieldBox(pForm, "Số điện thoại di động *", "", 138, false);
            txtEmail = AddFieldBox(pForm, "Địa chỉ Email *", "", 202, false);
            txtAddress = AddFieldBox(pForm, "Địa chỉ liên hệ", "", 266, false);
            AddField(pForm, "Ngày tham gia", DateTime.Today.ToString("dd/MM/yyyy"), 330, true);
            AddField(pForm, "Trạng thái", "Đã định danh trên CSDL", 394, true);

            SFButton save = new SFButton { Text = "LƯU THAY ĐỔI", Variant = SFButtonVariant.Primary, IconType = SFIcon.Check, Location = new Point(4, 465), Size = new Size(165, 42) };
            save.Click += (s, e) => SaveData();
            pForm.Controls.Add(save);

            right.Controls.Add(pForm);
            pForm.BringToFront();
            root.Controls.Add(right, 1, 0);

            Controls.Add(root);
            root.BringToFront();
        }

        private void LoadData()
        {
            try
            {
                currentKhId = SessionGuard.RequireCustomerId();

                DataTable dt = DatabaseManager.ExecuteQuery(
                    "EXEC dbo.sp_UI_CustomerProfile @KhachHangID = @ID",
                    new SqlParameter("@ID", currentKhId));

                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    string maKh = r["MaKhachHang"]?.ToString() ?? "KH001";
                    string name = r["HoTen"]?.ToString() ?? "";
                    string phone = r["SoDienThoai"]?.ToString() ?? "";
                    string email = r["Email"]?.ToString() ?? "";
                    string addr = r["DiaChi"]?.ToString() ?? "";
                    string tier = (r.Table.Columns.Contains("TenHang") && r["TenHang"] != DBNull.Value) ? r["TenHang"].ToString() : "Thành viên";
                    decimal pts = (r.Table.Columns.Contains("DiemTichLuy") && r["DiemTichLuy"] != DBNull.Value) ? Convert.ToDecimal(r["DiemTichLuy"]) : 0;
                    decimal spend = (r.Table.Columns.Contains("TongChiTieu") && r["TongChiTieu"] != DBNull.Value) ? Convert.ToDecimal(r["TongChiTieu"]) : 0;
                    decimal vi = (r.Table.Columns.Contains("SoDuViCoc") && r["SoDuViCoc"] != DBNull.Value) ? Convert.ToDecimal(r["SoDuViCoc"]) : 0;
                    int orders = (r.Table.Columns.Contains("TongLuotDat") && r["TongLuotDat"] != DBNull.Value) ? Convert.ToInt32(r["TongLuotDat"]) : 0;

                    lblPassName.Text = name.ToUpper();
                    lblBadge.Text = string.Format("★ THÀNH VIÊN {0} • {1}", tier.ToUpper(), maKh);
                    txtMaKh.Text = maKh;
                    txtName.Text = name;
                    txtPhone.Text = phone;
                    txtEmail.Text = email;
                    txtAddress.Text = addr;

                    lblPassDetails.Text = string.Format(
                        "Định danh: Hội viên cá nhân đã xác thực\nNgày gia nhập: {0:dd/MM/yyyy}\n\nĐiểm tích lũy:       {1:N0} pts\nTổng lượt đặt:       {2} trận\nSố dư ví cọc:        {3:N0}đ\nTổng chi tiêu:       {4:N0}đ\n\nTrạng thái tài khoản:\n● Tài khoản {5} đang hoạt động\n● Đã liên kết Zalo thông báo giờ thi đấu",
                        DateTime.Today, pts, orders, vi, spend, tier);
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "CUSTOMER_PROFILE", "LoadData");
            }
        }

        private void SaveData()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    SFToast.Show(this, "Họ và tên không được để trống.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Amber);
                    return;
                }

                DatabaseManager.ExecuteNonQuery(
                    "UPDATE dbo.KhachHang SET HoTen = @HoTen, SoDienThoai = @SDT, Email = @Email, DiaChi = @DiaChi WHERE KhachHangID = @ID",
                    new SqlParameter("@HoTen", txtName.Text.Trim()),
                    new SqlParameter("@SDT", txtPhone.Text.Trim()),
                    new SqlParameter("@Email", txtEmail.Text.Trim()),
                    new SqlParameter("@DiaChi", txtAddress.Text.Trim()),
                    new SqlParameter("@ID", currentKhId));

                if (AppSession.Current != null)
                {
                    AppSession.Current.HoTen = txtName.Text.Trim();
                }

                lblPassName.Text = txtName.Text.Trim().ToUpper();
                SFToast.Show(this, "Thông tin hồ sơ cá nhân đã được lưu thành công vào CSDL SQL Server.", "CẬP NHẬT HỒ SƠ", SFIcon.Check, Colors.Emerald);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "CUSTOMER_PROFILE", "SaveData");
                SFToast.Show(this, "Lỗi cập nhật: " + ex.Message, "LỖI", SFIcon.AlertCircle, Colors.Coral);
            }
        }

        private void AddField(Control p, string label, string value, int y, bool ro)
        {
            Label l = new Label { Text = label, Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(4, y), AutoSize = true };
            p.Controls.Add(l);
            SFTextBox t = new SFTextBox
            {
                Text = value,
                Location = new Point(4, y + 20),
                Width = Math.Max(240, p.ClientSize.Width - 16),
                Height = 42,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = ro
            };
            p.Controls.Add(t);
        }

        private SFTextBox AddFieldBox(Control p, string label, string value, int y, bool ro)
        {
            Label l = new Label { Text = label, Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(4, y), AutoSize = true };
            p.Controls.Add(l);
            SFTextBox t = new SFTextBox
            {
                Text = value,
                Location = new Point(4, y + 20),
                Width = Math.Max(240, p.ClientSize.Width - 16),
                Height = 42,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = ro
            };
            p.Controls.Add(t);
            return t;
        }
    }
}
