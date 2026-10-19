using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SportZone.WinForms.DesignTokens;

namespace SportZone.WinForms.Controls
{
    public class SFDataGridView : DataGridView
    {
        private int hoveredRowIndex = -1;

        public SFDataGridView()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            Dock = DockStyle.Fill;
            BackgroundColor = Colors.Surface;
            BorderStyle = BorderStyle.None;
            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            AllowUserToResizeRows = false;
            ReadOnly = true;
            RowHeadersVisible = false;
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            MultiSelect = false;
            Font = Typography.Body;
            GridColor = ColorTranslator.FromHtml("#EEF2F6");
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            EnableHeadersVisualStyles = false;

            // Headers styling
            ColumnHeadersHeight = Metrics.HeaderRowHeight;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#F8FAFC");
            ColumnHeadersDefaultCellStyle.ForeColor = ColorTranslator.FromHtml("#475467");
            ColumnHeadersDefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#F8FAFC");
            ColumnHeadersDefaultCellStyle.SelectionForeColor = ColorTranslator.FromHtml("#475467");
            ColumnHeadersDefaultCellStyle.Font = Typography.SecondaryBold;
            ColumnHeadersDefaultCellStyle.Padding = new Padding(12, 0, 12, 0);
            ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // Row styling
            RowTemplate.Height = Metrics.RowHeight;
            DefaultCellStyle.BackColor = Colors.Surface;
            DefaultCellStyle.ForeColor = Colors.TextPrimary;
            DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#F0FDF4");
            DefaultCellStyle.SelectionForeColor = Colors.TextPrimary;
            DefaultCellStyle.Padding = new Padding(12, 0, 12, 0);
            DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            AlternatingRowsDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#FAFBFD");
            AlternatingRowsDefaultCellStyle.ForeColor = Colors.TextPrimary;
            AlternatingRowsDefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#F0FDF4");
            AlternatingRowsDefaultCellStyle.SelectionForeColor = Colors.TextPrimary;

            CellMouseEnter += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex != hoveredRowIndex)
                {
                    int old = hoveredRowIndex;
                    hoveredRowIndex = e.RowIndex;
                    if (old >= 0 && old < RowCount) InvalidateRow(old);
                    if (hoveredRowIndex >= 0 && hoveredRowIndex < RowCount) InvalidateRow(hoveredRowIndex);
                }
            };

            MouseLeave += (s, e) =>
            {
                if (hoveredRowIndex >= 0)
                {
                    int old = hoveredRowIndex;
                    hoveredRowIndex = -1;
                    if (old >= 0 && old < RowCount) InvalidateRow(old);
                }
            };
        }

        protected override void OnColumnAdded(DataGridViewColumnEventArgs e)
        {
            base.OnColumnAdded(e);
            if (e.Column != null)
            {
                int measured = TextRenderer.MeasureText(e.Column.HeaderText ?? "", ColumnHeadersDefaultCellStyle.Font ?? Typography.SecondaryBold).Width + 28;
                e.Column.MinimumWidth = Math.Max(measured, 65);

                // Tiền/số lượng → căn phải toàn cột (header + cell)
                string h = (e.Column.HeaderText ?? "").ToLower();
                if (h.Contains("giá") || h.Contains("tổng") || h.Contains("tiền") || h.Contains("doanh thu") || h.Contains("share") || h.Contains("lượt") || h.Contains("giờ") || h.Contains("số dư") || h.Contains("amount") || h.Contains("total") || h.Contains("revenue") || h.Contains("orders") || h.Contains("hours") || h.Contains("price"))
                {
                    e.Column.DefaultCellStyle = new DataGridViewCellStyle(e.Column.DefaultCellStyle)
                    {
                        Alignment = DataGridViewContentAlignment.MiddleRight
                    };
                    e.Column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (IsHandleCreated)
            {
                BeginInvoke(new Action(ClearSelection));
            }
        }

        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // Check if this column is a Status column or has status-like values
                string colHeader = Columns[e.ColumnIndex].HeaderText.ToLower();
                object valObj = e.Value;
                string val = valObj != null ? valObj.ToString().Trim() : "";

                bool isStatus = colHeader.Contains("trạng thái") ||
                                colHeader.Contains("tình trạng") ||
                                colHeader.Contains("hoạt động") ||
                                colHeader.Contains("hiệu lực");

                if (isStatus && !string.IsNullOrEmpty(val))
                {
                    // Paint background first
                    e.PaintBackground(e.CellBounds, (e.State & DataGridViewElementStates.Selected) != 0);

                    // Determine badge colors
                    Color badgeBg, badgeFg;
                    GetBadgeColors(val, out badgeBg, out badgeFg);

                    Graphics g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    Size textSize = TextRenderer.MeasureText(val, Typography.MicroBold);
                    int badgeW = textSize.Width + 18;
                    int badgeH = 24;
                    int badgeX = e.CellBounds.X + 12;
                    int badgeY = e.CellBounds.Y + (e.CellBounds.Height - badgeH) / 2;

                    Rectangle badgeRect = new Rectangle(badgeX, badgeY, badgeW, badgeH);
                    using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(badgeRect, 12))
                    using (SolidBrush b = new SolidBrush(badgeBg))
                    {
                        g.FillPath(b, path);
                    }

                    TextRenderer.DrawText(g, val, Typography.MicroBold, badgeRect, badgeFg,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                    e.Handled = true;
                    return;
                }

                // If Price column, color bold emerald, honor per-column alignment
                if (colHeader.Contains("giá") || colHeader.Contains("tổng") || colHeader.Contains("thu") || colHeader.Contains("chi tiêu"))
                {
                    e.PaintBackground(e.CellBounds, (e.State & DataGridViewElementStates.Selected) != 0);
                    TextFormatFlags tf = (e.ColumnIndex >= 0 && Columns[e.ColumnIndex].DefaultCellStyle != null && Columns[e.ColumnIndex].DefaultCellStyle.Alignment == DataGridViewContentAlignment.MiddleRight)
                        ? TextFormatFlags.Right : TextFormatFlags.Left;
                    TextRenderer.DrawText(e.Graphics, val, Typography.BodyBold, e.CellBounds,
                        Colors.DeepEmerald, tf | TextFormatFlags.VerticalCenter);
                    e.Handled = true;
                    return;
                }
            }

            base.OnCellPainting(e);
        }

        private void GetBadgeColors(string status, out Color bg, out Color fg)
        {
            string s = status.ToLower();
            if (s.Contains("sẵn sàng") || s.Contains("hoàn tất") || s.Contains("hoàn thành") || s.Contains("hoạt động") || s.Contains("đã thanh toán") || s.Contains("áp dụng") || s.Contains("còn hàng") || s.Contains("đã khớp"))
            {
                bg = Colors.SuccessBg;
                fg = Colors.DeepEmerald;
            }
            else if (s.Contains("đang") || s.Contains("thi đấu") || s.Contains("sử dụng") || s.Contains("đang làm") || s.Contains("đang chạy"))
            {
                bg = Colors.InfoBg;
                fg = Colors.ElectricBlue;
            }
            else if (s.Contains("chờ") || s.Contains("cọc") || s.Contains("một phần") || s.Contains("sắp"))
            {
                bg = Colors.WarningBg;
                fg = ColorTranslator.FromHtml("#B45309");
            }
            else if (s.Contains("bảo trì") || s.Contains("tạm"))
            {
                bg = Colors.OrangeBg;
                fg = Colors.Orange;
            }
            else if (s.Contains("hủy") || s.Contains("chưa") || s.Contains("khóa"))
            {
                bg = Colors.DangerBg;
                fg = Colors.Danger;
            }
            else
            {
                bg = Colors.SlateBg;
                fg = Colors.Slate;
            }
        }

        /// <summary>
        /// Tự động định hình kích thước các cột: Cột nội dung dài dãn Fill, cột mã/giá/trạng thái/thao tác cố định
        /// </summary>
        public void ApplySmartColumnSizing()
        {
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            for (int i = 0; i < Columns.Count; i++)
            {
                var col = Columns[i];
                string h = col.HeaderText.ToLower();
                if (h.Contains("mã") || h.Contains("stt") || h.Contains("id"))
                {
                    col.Width = 85;
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }
                else if (h.Contains("trạng thái") || h.Contains("tình trạng") || h.Contains("vai trò") || h.Contains("hiệu lực"))
                {
                    col.Width = 135;
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }
                else if (h.Contains("thời gian") || h.Contains("ngày") || h.Contains("giờ") || h.Contains("hạn dùng"))
                {
                    col.Width = 150;
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }
                else if (h.Contains("giá") || h.Contains("tiền") || h.Contains("doanh thu") || h.Contains("cọc"))
                {
                    col.Width = 130;
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }
                else if (h.Contains("thao tác") || h.Contains("hành động") || h.Contains("chi tiết"))
                {
                    col.Width = 120;
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }
                else
                {
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
            }
        }
    }
}
