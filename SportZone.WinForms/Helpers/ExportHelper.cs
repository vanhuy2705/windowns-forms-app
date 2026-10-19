using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ClosedXML.Excel;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using SportZone.WinForms.Controls;
using SportZone.WinForms.Database;
using SportZone.WinForms.DesignTokens;
using SportZone.WinForms.Session;

namespace SportZone.WinForms.Helpers
{
    public static class ExportHelper
    {
        public static bool ExportToExcel(DataGridView grid, string defaultFileName, Control owner)
        {
            if (grid == null || grid.Rows.Count == 0)
            {
                SFToast.Show(owner, "Không có dữ liệu để xuất.", "THÔNG BÁO", SFIcon.AlertCircle, Colors.Warning);
                return false;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Tệp Excel (*.xlsx)|*.xlsx|Tệp CSV (*.csv)|*.csv";
                sfd.FileName = defaultFileName;
                if (sfd.ShowDialog(owner) == DialogResult.OK)
                {
                    try
                    {
                        string ext = Path.GetExtension(sfd.FileName).ToLowerInvariant();
                        if (ext == ".csv")
                        {
                            ExportCsvInternal(grid, sfd.FileName);
                        }
                        else
                        {
                            ExportXlsxInternal(grid, sfd.FileName);
                        }

                        LogReport("EXCEL", sfd.FileName, true, null);
                        SFToast.Show(owner, $"Đã xuất thành công tệp: {Path.GetFileName(sfd.FileName)}", "XUẤT EXCEL THÀNH CÔNG", SFIcon.Check, Colors.Emerald);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        LogReport("EXCEL", sfd.FileName, false, ex.Message);
                        AppLogger.LogError(ex, "EXPORT_EXCEL");
                        SFToast.Show(owner, "Lỗi khi ghi tệp: " + ex.Message, "LỖI XUẤT TỆP", SFIcon.AlertCircle, Colors.Danger);
                        return false;
                    }
                }
            }
            return false;
        }

        public static void ExportXlsxToFile(DataGridView grid, string filePath)
        {
            ExportXlsxInternal(grid, filePath);
        }

        public static void ExportPdfToFile(DataGridView grid, string filePath, string reportTitle = "BÁO CÁO HỆ THỐNG SPORTFIELD")
        {
            ExportPurePdfInternal(reportTitle, grid, filePath);
        }

        private static void ExportXlsxInternal(DataGridView grid, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("BaoCao");

                // Headers
                int colIndex = 1;
                for (int i = 0; i < grid.Columns.Count; i++)
                {
                    if (!grid.Columns[i].Visible) continue;
                    var cell = worksheet.Cell(1, colIndex);
                    cell.Value = grid.Columns[i].HeaderText;
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromArgb(16, 185, 129); // Emerald
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    cell.Style.Border.OutsideBorderColor = XLColor.FromArgb(5, 150, 105);
                    colIndex++;
                }

                // Rows
                int rowIndex = 2;
                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (row.IsNewRow) continue;
                    colIndex = 1;
                    bool isEven = (rowIndex % 2 == 0);

                    for (int i = 0; i < grid.Columns.Count; i++)
                    {
                        if (!grid.Columns[i].Visible) continue;
                        object val = row.Cells[i].Value;
                        var cell = worksheet.Cell(rowIndex, colIndex);

                        if (val != null && val != DBNull.Value)
                        {
                            if (val is int || val is long || val is short || val is byte)
                            {
                                cell.SetValue(Convert.ToInt64(val));
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            }
                            else if (val is decimal || val is double || val is float)
                            {
                                cell.SetValue(Convert.ToDouble(val));
                                cell.Style.NumberFormat.Format = "#,##0";
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            }
                            else if (val is DateTime dt)
                            {
                                cell.SetValue(dt);
                                cell.Style.NumberFormat.Format = "dd/MM/yyyy HH:mm";
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            }
                            else if (val is bool b)
                            {
                                cell.SetValue(b ? "Có" : "Không");
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            }
                            else
                            {
                                cell.SetValue(val.ToString());
                            }
                        }
                        else
                        {
                            cell.SetValue("");
                        }

                        if (isEven)
                        {
                            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(248, 250, 252);
                        }
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        cell.Style.Border.OutsideBorderColor = XLColor.FromArgb(226, 232, 240);

                        colIndex++;
                    }
                    rowIndex++;
                }

                worksheet.Columns().AdjustToContents(10.0, 60.0);
                workbook.SaveAs(filePath);
            }
        }

        private static void ExportCsvInternal(DataGridView grid, string filePath)
        {
            StringBuilder sb = new StringBuilder();

            // Write headers
            int visibleColCount = 0;
            for (int i = 0; i < grid.Columns.Count; i++)
            {
                if (!grid.Columns[i].Visible) continue;
                if (visibleColCount > 0) sb.Append(",");
                sb.Append(EscapeCsv(grid.Columns[i].HeaderText));
                visibleColCount++;
            }
            sb.AppendLine();

            // Write rows
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.IsNewRow) continue;
                visibleColCount = 0;
                for (int i = 0; i < grid.Columns.Count; i++)
                {
                    if (!grid.Columns[i].Visible) continue;
                    if (visibleColCount > 0) sb.Append(",");
                    object val = row.Cells[i].Value;
                    sb.Append(EscapeCsv(val?.ToString() ?? ""));
                    visibleColCount++;
                }
                sb.AppendLine();
            }

            byte[] bom = new byte[] { 0xEF, 0xBB, 0xBF };
            byte[] data = Encoding.UTF8.GetBytes(sb.ToString());
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                fs.Write(bom, 0, bom.Length);
                fs.Write(data, 0, data.Length);
            }
        }

        public static bool ExportToPdf(string reportTitle, DataGridView grid, string defaultFileName, Control owner)
        {
            if (grid == null || grid.Rows.Count == 0)
            {
                SFToast.Show(owner, "Không có dữ liệu để xuất báo cáo.", "THÔNG BÁO", SFIcon.AlertCircle, Colors.Warning);
                return false;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Tệp PDF (*.pdf)|*.pdf|Tệp Báo Cáo HTML (*.html)|*.html";
                sfd.FileName = defaultFileName;
                if (sfd.ShowDialog(owner) == DialogResult.OK)
                {
                    try
                    {
                        string ext = Path.GetExtension(sfd.FileName).ToLowerInvariant();
                        if (ext == ".html")
                        {
                            ExportHtmlInternal(reportTitle, grid, sfd.FileName);
                        }
                        else
                        {
                            // Pure PDF 1.4 Binary Generator
                            ExportPurePdfInternal(reportTitle, grid, sfd.FileName);
                        }

                        LogReport("PDF", sfd.FileName, true, null);
                        SFToast.Show(owner, $"Đã tạo và xuất tài liệu báo cáo: {Path.GetFileName(sfd.FileName)}", "XUẤT BÁO CÁO THÀNH CÔNG", SFIcon.Check, Colors.Emerald);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        LogReport("PDF", sfd.FileName, false, ex.Message);
                        AppLogger.LogError(ex, "EXPORT_PDF");
                        SFToast.Show(owner, "Lỗi khi xuất tệp: " + ex.Message, "LỖI XUẤT BÁO CÁO", SFIcon.AlertCircle, Colors.Danger);
                        return false;
                    }
                }
            }
            return false;
        }

        private static void ExportHtmlInternal(string reportTitle, DataGridView grid, string filePath)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset='utf-8'>");
            sb.AppendLine($"<title>{reportTitle}</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; margin: 30px; color: #1e293b; }");
            sb.AppendLine("h1 { color: #0f172a; border-bottom: 2px solid #10b981; padding-bottom: 8px; font-size: 20px; }");
            sb.AppendLine(".meta { color: #64748b; font-size: 13px; margin-bottom: 20px; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 15px; font-size: 13px; }");
            sb.AppendLine("th { background-color: #10b981; color: #ffffff; font-weight: 600; text-align: left; padding: 10px; border: 1px solid #cbd5e1; }");
            sb.AppendLine("td { padding: 8px 10px; border: 1px solid #e2e8f0; }");
            sb.AppendLine("tr:nth-child(even) { background-color: #f8fafc; }");
            sb.AppendLine(".footer { margin-top: 30px; text-align: right; font-size: 12px; color: #94a3b8; }");
            sb.AppendLine("</style></head><body>");

            sb.AppendLine($"<h1>SPORTFIELD — {reportTitle.ToUpper()}</h1>");
            sb.AppendLine($"<div class='meta'>Thời gian xuất: {DateTime.Now:dd/MM/yyyy HH:mm:ss} | Người xuất: {AppSession.Current.HoTen ?? "Quản trị viên"}</div>");

            sb.AppendLine("<table><thead><tr>");
            for (int i = 0; i < grid.Columns.Count; i++)
            {
                if (!grid.Columns[i].Visible) continue;
                sb.AppendLine($"<th>{System.Net.WebUtility.HtmlEncode(grid.Columns[i].HeaderText)}</th>");
            }
            sb.AppendLine("</tr></thead><tbody>");

            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.IsNewRow) continue;
                sb.AppendLine("<tr>");
                for (int i = 0; i < grid.Columns.Count; i++)
                {
                    if (!grid.Columns[i].Visible) continue;
                    object val = row.Cells[i].Value;
                    sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(val?.ToString() ?? "")}</td>");
                }
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody></table>");
            sb.AppendLine($"<div class='footer'>Hệ thống quản lý đặt sân thể thao SPORTFIELD Aurora Platform</div>");
            sb.AppendLine("</body></html>");

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Bộ sinh tài liệu PDF sử dụng thư viện PdfSharp chuẩn Unicode tiếng Việt
        /// Hỗ trợ Header, Metadata, Khung bảng dữ liệu, Phân trang tự động và giữ nguyên dấu tiếng Việt
        /// </summary>
        private static void ExportPurePdfInternal(string reportTitle, DataGridView grid, string filePath)
        {
            var visibleCols = new List<DataGridViewColumn>();
            for (int i = 0; i < grid.Columns.Count; i++)
            {
                if (grid.Columns[i].Visible) visibleCols.Add(grid.Columns[i]);
            }

            if (visibleCols.Count == 0) return;

            bool isLandscape = visibleCols.Count >= 5;
            double margin = 36.0;

            // Collect rows
            var dataRows = new List<string[]>();
            foreach (DataGridViewRow r in grid.Rows)
            {
                if (r.IsNewRow) continue;
                string[] rowData = new string[visibleCols.Count];
                for (int c = 0; c < visibleCols.Count; c++)
                {
                    object v = r.Cells[visibleCols[c].Index].Value;
                    if (v != null && v != DBNull.Value)
                    {
                        if (v is DateTime dt)
                            rowData[c] = dt.ToString("dd/MM/yyyy HH:mm");
                        else if (v is decimal || v is double || v is float)
                            rowData[c] = string.Format("{0:N0}", v);
                        else if (v is bool b)
                            rowData[c] = b ? "Có" : "Không";
                        else
                            rowData[c] = v.ToString();
                    }
                    else
                    {
                        rowData[c] = "";
                    }
                }
                dataRows.Add(rowData);
            }

            using (PdfDocument document = new PdfDocument())
            {
                document.Info.Title = reportTitle ?? "BÁO CÁO HỆ THỐNG SPORTFIELD";
                document.Info.Author = AppSession.Current.HoTen ?? "Hệ thống SPORTFIELD";
                document.Info.Subject = "Báo cáo quản lý sân thể thao SPORTFIELD";

                // Setup Font với Unicode Encoding
                XPdfFontOptions fontOptions = new XPdfFontOptions(PdfFontEncoding.Unicode);
                XFont fontTitle = new XFont("Segoe UI", 14, XFontStyle.Bold, fontOptions);
                XFont fontSubtitle = new XFont("Segoe UI", 8.5, XFontStyle.Regular, fontOptions);
                XFont fontColHeader = new XFont("Segoe UI", 8.5, XFontStyle.Bold, fontOptions);
                XFont fontCell = new XFont("Segoe UI", 8, XFontStyle.Regular, fontOptions);
                XFont fontFooter = new XFont("Segoe UI", 7.5, XFontStyle.Regular, fontOptions);

                XPen penBorder = new XPen(XColor.FromArgb(226, 232, 240), 0.5);
                XPen penOuterBorder = new XPen(XColor.FromArgb(203, 213, 225), 1.0);
                XPen penAccent = new XPen(XColor.FromArgb(16, 185, 129), 2.0);

                XSolidBrush brushHeader = new XSolidBrush(XColor.FromArgb(16, 185, 129));
                XSolidBrush brushZebra = new XSolidBrush(XColor.FromArgb(248, 250, 252));
                XSolidBrush brushWhite = new XSolidBrush(XColors.White);
                XSolidBrush brushText = new XSolidBrush(XColor.FromArgb(30, 41, 59));
                XSolidBrush brushMuted = new XSolidBrush(XColor.FromArgb(100, 116, 139));
                XSolidBrush brushTitle = new XSolidBrush(XColor.FromArgb(15, 23, 42));

                // Page dimensions for A4
                double pageWidth = isLandscape ? 842.0 : 595.0;
                double pageHeight = isLandscape ? 595.0 : 842.0;
                double usableWidth = pageWidth - (margin * 2);

                // Compute column widths
                double[] colWidths = new double[visibleCols.Count];
                double totalWeight = 0;
                for (int i = 0; i < visibleCols.Count; i++)
                {
                    double w = Math.Max(50, visibleCols[i].Width);
                    colWidths[i] = w;
                    totalWeight += w;
                }
                for (int i = 0; i < visibleCols.Count; i++)
                {
                    colWidths[i] = (colWidths[i] / totalWeight) * usableWidth;
                }

                // Layout heights
                double headerBlockHeight = 60.0;
                double tableHeaderHeight = 24.0;
                double rowHeight = 20.0;
                double footerHeight = 30.0;

                double availableTableHeight = pageHeight - (margin * 2) - headerBlockHeight - tableHeaderHeight - footerHeight;
                int rowsPerPage = Math.Max(5, (int)(availableTableHeight / rowHeight));
                int totalPages = Math.Max(1, (int)Math.Ceiling((double)dataRows.Count / rowsPerPage));

                string titleText = "SPORTFIELD — " + (reportTitle ?? "BÁO CÁO HỆ THỐNG").ToUpper();
                string metaText = $"Thời gian xuất: {DateTime.Now:dd/MM/yyyy HH:mm:ss} | Người xuất: {AppSession.Current.HoTen ?? "Quản trị viên"}";

                for (int pIndex = 0; pIndex < totalPages; pIndex++)
                {
                    PdfPage page = document.AddPage();
                    page.Size = PageSize.A4;
                    page.Orientation = isLandscape ? PageOrientation.Landscape : PageOrientation.Portrait;

                    using (XGraphics gfx = XGraphics.FromPdfPage(page))
                    {
                        double currY = margin;

                        // 1. Title & Meta
                        gfx.DrawString(titleText, fontTitle, brushTitle, new XRect(margin, currY, usableWidth, 22), XStringFormats.CenterLeft);
                        currY += 22;

                        gfx.DrawString(metaText, fontSubtitle, brushMuted, new XRect(margin, currY, usableWidth, 16), XStringFormats.CenterLeft);
                        currY += 18;

                        gfx.DrawLine(penAccent, margin, currY, pageWidth - margin, currY);
                        currY += 8;

                        // 2. Table Header
                        double tableTopY = currY;
                        gfx.DrawRectangle(brushHeader, new XRect(margin, currY, usableWidth, tableHeaderHeight));

                        double currX = margin;
                        for (int c = 0; c < visibleCols.Count; c++)
                        {
                            XRect headerRect = new XRect(currX + 4, currY, colWidths[c] - 8, tableHeaderHeight);
                            gfx.DrawString(visibleCols[c].HeaderText ?? "", fontColHeader, brushWhite, headerRect, XStringFormats.CenterLeft);
                            currX += colWidths[c];
                        }
                        currY += tableHeaderHeight;

                        // 3. Table Rows
                        int startRow = pIndex * rowsPerPage;
                        int endRow = Math.Min(dataRows.Count, startRow + rowsPerPage);

                        for (int rIdx = startRow; rIdx < endRow; rIdx++)
                        {
                            string[] rData = dataRows[rIdx];
                            bool isEven = ((rIdx - startRow) % 2 == 0);

                            if (isEven)
                            {
                                gfx.DrawRectangle(brushZebra, new XRect(margin, currY, usableWidth, rowHeight));
                            }

                            // Bottom border
                            gfx.DrawLine(penBorder, margin, currY + rowHeight, pageWidth - margin, currY + rowHeight);

                            currX = margin;
                            for (int c = 0; c < visibleCols.Count; c++)
                            {
                                string cellText = rData[c] ?? "";
                                XStringFormat align = XStringFormats.CenterLeft;
                                if (visibleCols[c].DefaultCellStyle.Alignment == DataGridViewContentAlignment.MiddleRight)
                                {
                                    align = XStringFormats.CenterRight;
                                }
                                else if (visibleCols[c].DefaultCellStyle.Alignment == DataGridViewContentAlignment.MiddleCenter)
                                {
                                    align = XStringFormats.Center;
                                }

                                XRect cellRect = new XRect(currX + 4, currY, colWidths[c] - 8, rowHeight);
                                gfx.DrawString(cellText, fontCell, brushText, cellRect, align);
                                currX += colWidths[c];
                            }

                            currY += rowHeight;
                        }

                        // Outer table border
                        double tableTotalH = (tableHeaderHeight + (endRow - startRow) * rowHeight);
                        gfx.DrawRectangle(penOuterBorder, new XRect(margin, tableTopY, usableWidth, tableTotalH));

                        // 4. Footer
                        double footerY = pageHeight - margin - 16;
                        gfx.DrawLine(penBorder, margin, footerY - 4, pageWidth - margin, footerY - 4);
                        gfx.DrawString("SPORTFIELD Aurora Platform — Hệ thống quản lý sân thể thao thông minh", fontFooter, brushMuted, new XRect(margin, footerY, usableWidth * 0.7, 16), XStringFormats.CenterLeft);
                        gfx.DrawString($"Trang {pIndex + 1} / {totalPages}", fontFooter, brushMuted, new XRect(pageWidth - margin - 100, footerY, 100, 16), XStringFormats.CenterRight);
                    }
                }

                document.Save(filePath);
            }
        }

        private static string EscapeCsv(string field)
        {
            if (field == null) return "\"\"";
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }
            return field;
        }

        private static void LogReport(string format, string filePath, bool success, string error)
        {
            try
            {
                DatabaseManager.ExecuteNonQuery(
                    "EXEC dbo.sp_GhiNhatKyXuatBaoCao @LoaiBaoCao, @DinhDang, @TuNgay, @DenNgay, @DuongDanTep, @TaiKhoanID, @ThanhCong, @Loi",
                    new SqlParameter("@LoaiBaoCao", "BAO_CAO"),
                    new SqlParameter("@DinhDang", format),
                    new SqlParameter("@TuNgay", DateTime.Today),
                    new SqlParameter("@DenNgay", DateTime.Today),
                    new SqlParameter("@DuongDanTep", (object)filePath ?? DBNull.Value),
                    new SqlParameter("@TaiKhoanID", AppSession.Current.TaiKhoanID > 0 ? (object)AppSession.Current.TaiKhoanID : DBNull.Value),
                    new SqlParameter("@ThanhCong", success),
                    new SqlParameter("@Loi", (object)error ?? DBNull.Value));
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning("LogReport failed: " + ex.Message);
            }
        }
    }
}
