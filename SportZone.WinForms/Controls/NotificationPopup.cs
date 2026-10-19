using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SportZone.WinForms.DesignTokens;
using SportZone.WinForms.Models;
using SportZone.WinForms.Services;

namespace SportZone.WinForms.Controls
{
    /// <summary>
    /// Popup danh sách thông báo Dropdown hiện đại chuẩn WinForms
    /// Hỗ trợ click ngoài tự đóng, ESC tự đóng, Empty/Loading/Error state và Navigation
    /// </summary>
    public class NotificationPopup : ToolStripDropDown
    {
        private readonly NotificationPanel _panel;
        private readonly ToolStripControlHost _host;

        public event Action<NotificationItem> NotificationSelected
        {
            add => _panel.NotificationSelected += value;
            remove => _panel.NotificationSelected -= value;
        }

        public NotificationPopup()
        {
            DoubleBuffered = true;
            AutoClose = true;
            DropShadowEnabled = true;
            Padding = Padding.Empty;
            Margin = Padding.Empty;

            _panel = new NotificationPanel(this);
            _host = new ToolStripControlHost(_panel)
            {
                Padding = Padding.Empty,
                Margin = Padding.Empty,
                AutoSize = false,
                Size = _panel.Size
            };

            Items.Add(_host);
        }

        public void ShowBelow(Control anchorControl)
        {
            if (anchorControl == null) return;

            Point screenPoint = anchorControl.PointToScreen(new Point(anchorControl.Width - _panel.Width, anchorControl.Height + 4));
            Rectangle workingArea = Screen.FromControl(anchorControl).WorkingArea;
            if (screenPoint.X + _panel.Width > workingArea.Right)
            {
                screenPoint.X = workingArea.Right - _panel.Width - 10;
            }
            if (screenPoint.X < workingArea.Left)
            {
                screenPoint.X = workingArea.Left + 10;
            }

            Show(screenPoint);
            _panel.LoadData();
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }
    }

    /// <summary>
    /// Panel giao diện nội dung của Notification Center
    /// </summary>
    public class NotificationPanel : UserControl
    {
        private readonly ToolStripDropDown _parentDropDown;
        private Panel _headerPanel;
        private Panel _bodyPanel;
        private Panel _footerPanel;
        private Label _lblTitle;
        private Label _lblBadge;
        private Label _btnMarkAll;
        private FlowLayoutPanel _listFlow;
        private Panel _statePanel;

        public event Action<NotificationItem> NotificationSelected;

        public NotificationPanel(ToolStripDropDown parentDropDown)
        {
            _parentDropDown = parentDropDown;
            DoubleBuffered = true;
            Size = new Size(390, 470);
            BackColor = Colors.Surface;
            Font = Typography.Body;

            BuildUI();
        }

        private void BuildUI()
        {
            BorderStyle = BorderStyle.None;

            // 1. Header Panel (Height: 46)
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 46,
                BackColor = Colors.Surface,
                Padding = new Padding(16, 12, 16, 12)
            };
            _headerPanel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Colors.Border, 1f))
                {
                    e.Graphics.DrawLine(pen, 0, _headerPanel.Height - 1, _headerPanel.Width, _headerPanel.Height - 1);
                }
            };

            _lblTitle = new Label
            {
                Text = "THÔNG BÁO",
                Font = Typography.BodyBold,
                ForeColor = Colors.TextPrimary,
                AutoSize = true,
                Location = new Point(14, 13)
            };
            _headerPanel.Controls.Add(_lblTitle);

            _lblBadge = new Label
            {
                Text = "0 mới",
                Font = Typography.MicroBold,
                ForeColor = Colors.Danger,
                BackColor = Colors.DangerBg,
                AutoSize = true,
                Padding = new Padding(4, 2, 4, 2),
                Location = new Point(106, 14),
                Visible = false
            };
            _headerPanel.Controls.Add(_lblBadge);

            _btnMarkAll = new Label
            {
                Text = "Đánh dấu tất cả đã đọc",
                Font = Typography.MicroBold,
                ForeColor = Colors.Primary,
                AutoSize = true,
                Cursor = Cursors.Hand,
                Location = new Point(236, 15)
            };
            _btnMarkAll.MouseEnter += (s, e) => _btnMarkAll.ForeColor = Colors.PrimaryHover;
            _btnMarkAll.MouseLeave += (s, e) => _btnMarkAll.ForeColor = Colors.Primary;
            _btnMarkAll.Click += (s, e) => MarkAllAsRead();
            _headerPanel.Controls.Add(_btnMarkAll);

            // 2. Footer Panel (Height: 38)
            _footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 38,
                BackColor = Colors.Canvas,
                Cursor = Cursors.Hand
            };
            _footerPanel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Colors.Border, 1f))
                {
                    e.Graphics.DrawLine(pen, 0, 0, _footerPanel.Width, 0);
                }
            };

            Label lblSeeAll = new Label
            {
                Text = "Đóng thông báo (ESC)",
                Font = Typography.MicroBold,
                ForeColor = Colors.TextMuted,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            lblSeeAll.Click += (s, e) => _parentDropDown.Close();
            _footerPanel.Controls.Add(lblSeeAll);

            // 3. Body Panel
            _bodyPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface
            };

            _listFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            _bodyPanel.Controls.Add(_listFlow);

            // State Panel (Loading / Empty / Error)
            _statePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface,
                Visible = false
            };
            _bodyPanel.Controls.Add(_statePanel);

            Controls.Add(_bodyPanel);
            Controls.Add(_headerPanel);
            Controls.Add(_footerPanel);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Vẽ viền bao quanh toàn bộ popup
            using (var pen = new Pen(Colors.Border, 1f))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        public void LoadData()
        {
            ShowLoading();

            try
            {
                var list = NotificationService.Instance.GetNotifications(onlyUnread: null, limit: 20);
                RenderItems(list);
            }
            catch (Exception ex)
            {
                ShowError("Không thể tải thông báo từ máy chủ.\n" + ex.Message);
            }
        }

        private void ShowLoading()
        {
            _statePanel.Controls.Clear();
            _statePanel.Visible = true;
            _listFlow.Visible = false;

            Label lbl = new Label
            {
                Text = "Đang tải thông báo...",
                Font = Typography.Secondary,
                ForeColor = Colors.TextMuted,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _statePanel.Controls.Add(lbl);
        }

        private void ShowError(string message)
        {
            _statePanel.Controls.Clear();
            _statePanel.Visible = true;
            _listFlow.Visible = false;

            TableLayoutPanel pnl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            pnl.RowStyles.Add(new RowStyle(SizeType.Percent, 70f));
            pnl.RowStyles.Add(new RowStyle(SizeType.Percent, 30f));

            Label lbl = new Label
            {
                Text = "⚠️ " + message,
                Font = Typography.Secondary,
                ForeColor = Colors.Danger,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnl.Controls.Add(lbl, 0, 0);

            SFButton btnRetry = new SFButton
            {
                Text = "Thử lại",
                Variant = SFButtonVariant.Secondary,
                Size = new Size(100, 32),
                Anchor = AnchorStyles.Top
            };
            btnRetry.Click += (s, e) => LoadData();
            pnl.Controls.Add(btnRetry, 0, 1);

            _statePanel.Controls.Add(pnl);
        }

        private void ShowEmpty()
        {
            _statePanel.Controls.Clear();
            _statePanel.Visible = true;
            _listFlow.Visible = false;

            Panel centerPnl = new Panel
            {
                Size = new Size(260, 160),
                Location = new Point((_bodyPanel.Width - 260) / 2, (_bodyPanel.Height - 160) / 2)
            };

            // Vẽ Icon Bell lớn
            PictureBox pbIcon = new PictureBox
            {
                Size = new Size(48, 48),
                Location = new Point((260 - 48) / 2, 10),
                BackColor = Color.Transparent
            };
            pbIcon.Paint += (s, e) =>
            {
                VectorIcons.Draw(e.Graphics, SFIcon.Bell, new Rectangle(6, 6, 36, 36), Colors.TextMuted, 1.8f);
            };
            centerPnl.Controls.Add(pbIcon);

            Label lbl1 = new Label
            {
                Text = "Chưa có thông báo nào",
                Font = Typography.BodyBold,
                ForeColor = Colors.TextPrimary,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(260, 24),
                Location = new Point(0, 70)
            };
            centerPnl.Controls.Add(lbl1);

            Label lbl2 = new Label
            {
                Text = "Các cập nhật về đơn đặt sân, thanh toán và khuyến mãi sẽ xuất hiện tại đây.",
                Font = Typography.Micro,
                ForeColor = Colors.TextMuted,
                TextAlign = ContentAlignment.TopCenter,
                Size = new Size(260, 45),
                Location = new Point(0, 96)
            };
            centerPnl.Controls.Add(lbl2);

            _statePanel.Controls.Add(centerPnl);
        }

        private void RenderItems(List<NotificationItem> items)
        {
            if (items == null || items.Count == 0)
            {
                _lblBadge.Visible = false;
                _btnMarkAll.Visible = false;
                ShowEmpty();
                return;
            }

            int unread = 0;
            foreach (var it in items) if (!it.DaDoc) unread++;

            if (unread > 0)
            {
                _lblBadge.Text = $"{unread} mới";
                _lblBadge.Visible = true;
                _btnMarkAll.Visible = true;
            }
            else
            {
                _lblBadge.Visible = false;
                _btnMarkAll.Visible = false;
            }

            _statePanel.Visible = false;
            _listFlow.SuspendLayout();
            _listFlow.Controls.Clear();
            _listFlow.Visible = true;

            int itemWidth = _bodyPanel.Width - (SystemInformation.VerticalScrollBarWidth + 2);

            foreach (var item in items)
            {
                var card = CreateItemCard(item, itemWidth);
                _listFlow.Controls.Add(card);
            }

            _listFlow.ResumeLayout(true);
        }

        private Control CreateItemCard(NotificationItem item, int width)
        {
            Panel card = new Panel
            {
                Width = width,
                Height = 74,
                BackColor = item.DaDoc ? Colors.Surface : Color.FromArgb(248, 250, 252),
                Cursor = Cursors.Hand,
                Margin = new Padding(0)
            };

            // Vẽ viền gạch chân đáy và chấm xanh unread
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(Colors.Border, 1f))
                {
                    e.Graphics.DrawLine(pen, 0, card.Height - 1, card.Width, card.Height - 1);
                }
                if (!item.DaDoc)
                {
                    using (var dotBrush = new SolidBrush(Colors.Primary))
                    {
                        e.Graphics.FillEllipse(dotBrush, 12, 16, 8, 8);
                    }
                }
            };

            // Icon loại thông báo
            PictureBox pbIcon = new PictureBox
            {
                Size = new Size(34, 34),
                Location = new Point(item.DaDoc ? 14 : 26, 20),
                BackColor = GetTypeBadgeBg(item.LoaiThongBao)
            };
            pbIcon.Paint += (s, e) =>
            {
                var (icon, color) = GetTypeIcon(item.LoaiThongBao);
                VectorIcons.Draw(e.Graphics, icon, new Rectangle(7, 7, 20, 20), color, 1.8f);
            };
            card.Controls.Add(pbIcon);

            // Tiêu đề & Giờ
            int contentLeft = pbIcon.Right + 12;
            int textWidth = card.Width - contentLeft - 14;

            Label lblHeader = new Label
            {
                Text = item.TieuDe,
                Font = item.DaDoc ? Typography.Secondary : Typography.SecondaryBold,
                ForeColor = item.DaDoc ? Colors.TextSecondary : Colors.TextPrimary,
                Location = new Point(contentLeft, 14),
                Size = new Size(textWidth - 75, 18),
                AutoEllipsis = true
            };
            card.Controls.Add(lblHeader);

            Label lblTime = new Label
            {
                Text = item.RelativeTime,
                Font = Typography.Micro,
                ForeColor = Colors.TextMuted,
                Location = new Point(card.Width - 85, 15),
                Size = new Size(75, 16),
                TextAlign = ContentAlignment.TopRight
            };
            card.Controls.Add(lblTime);

            Label lblBody = new Label
            {
                Text = item.NoiDung,
                Font = Typography.Micro,
                ForeColor = Colors.TextSecondary,
                Location = new Point(contentLeft, 34),
                Size = new Size(textWidth, 32),
                AutoEllipsis = true
            };
            card.Controls.Add(lblBody);

            // Sự kiện Click và Hover
            void OnCardEnter()
            {
                card.BackColor = Colors.SurfaceAlt;
            }
            void OnCardLeave()
            {
                card.BackColor = item.DaDoc ? Colors.Surface : Color.FromArgb(248, 250, 252);
            }
            void OnCardClick()
            {
                // Bước 1: Mark as read
                if (!item.DaDoc)
                {
                    NotificationService.Instance.MarkAsRead(item.ThongBaoID);
                    item.DaDoc = true;
                    card.BackColor = Colors.Surface;
                    lblHeader.Font = Typography.Secondary;
                    lblHeader.ForeColor = Colors.TextSecondary;
                    card.Invalidate();
                }

                // Bước 2: Đóng dropdown
                _parentDropDown.Close();

                // Bước 3: Điều hướng
                NotificationSelected?.Invoke(item);
            }

            card.MouseEnter += (s, e) => OnCardEnter();
            card.MouseLeave += (s, e) => OnCardLeave();
            card.Click += (s, e) => OnCardClick();

            lblHeader.MouseEnter += (s, e) => OnCardEnter();
            lblHeader.MouseLeave += (s, e) => OnCardLeave();
            lblHeader.Click += (s, e) => OnCardClick();

            lblBody.MouseEnter += (s, e) => OnCardEnter();
            lblBody.MouseLeave += (s, e) => OnCardLeave();
            lblBody.Click += (s, e) => OnCardClick();

            lblTime.MouseEnter += (s, e) => OnCardEnter();
            lblTime.MouseLeave += (s, e) => OnCardLeave();
            lblTime.Click += (s, e) => OnCardClick();

            pbIcon.MouseEnter += (s, e) => OnCardEnter();
            pbIcon.MouseLeave += (s, e) => OnCardLeave();
            pbIcon.Click += (s, e) => OnCardClick();

            return card;
        }

        private void MarkAllAsRead()
        {
            bool ok = NotificationService.Instance.MarkAllAsRead();
            if (ok)
            {
                LoadData();
            }
        }

        private static (SFIcon icon, Color color) GetTypeIcon(string type)
        {
            string t = (type ?? "").ToUpperInvariant();
            if (t.Contains("BOOKING") || t.Contains("DAT_SAN")) return (SFIcon.Calendar, Colors.Primary);
            if (t.Contains("PAYMENT") || t.Contains("THANH_TOAN") || t.Contains("TIEN")) return (SFIcon.Dollar, Colors.Emerald);
            if (t.Contains("VOUCHER") || t.Contains("PROMO")) return (SFIcon.Ticket, Colors.Purple);
            if (t.Contains("INVENTORY") || t.Contains("KHO") || t.Contains("ALERT")) return (SFIcon.AlertCircle, Colors.Warning);
            if (t.Contains("EMPLOYEE") || t.Contains("USER") || t.Contains("CA_TRUC")) return (SFIcon.User, Colors.Indigo);
            return (SFIcon.Bell, Colors.TextSecondary);
        }

        private static Color GetTypeBadgeBg(string type)
        {
            string t = (type ?? "").ToUpperInvariant();
            if (t.Contains("BOOKING") || t.Contains("DAT_SAN")) return Color.FromArgb(239, 246, 255);
            if (t.Contains("PAYMENT") || t.Contains("THANH_TOAN")) return Color.FromArgb(236, 253, 245);
            if (t.Contains("VOUCHER") || t.Contains("PROMO")) return Color.FromArgb(250, 245, 255);
            if (t.Contains("INVENTORY") || t.Contains("ALERT")) return Color.FromArgb(254, 243, 199);
            return Color.FromArgb(241, 245, 249);
        }
    }
}
