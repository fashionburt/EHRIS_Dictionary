extern alias CryptoNew;

using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using EHRIS.Tools.Crypto;
using GemBox.Spreadsheet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Reflection;
using static CryptoNew::Org.BouncyCastle.Math.EC.ECCurve;
using SaveOptions = GemBox.Spreadsheet.SaveOptions;

namespace EHRIS.Tools.Office.Package
{
    public class GemBoxSheetHelper
    {
        //   private readonly static string LicenseKey = "FREE-LIMITED-KEY"; 

        private static bool _licenseInitialized = false;

        private static void EnsureLicense()
        {
            if (_licenseInitialized) return;

            string encrypted = Environment.GetEnvironmentVariable("GemBoxEncryptedKey");
            string masterKey = Environment.GetEnvironmentVariable("EHRISKey");
            string aad = "EHRIS";

            if (string.IsNullOrWhiteSpace(encrypted) || string.IsNullOrWhiteSpace(masterKey))
                throw new InvalidOperationException("GemBoxEncryptedKey 或 EHRISKey 未設定");

            string licenseKey = SecureEncryptor.Decrypt(encrypted, masterKey, aad);
            SpreadsheetInfo.SetLicense(licenseKey);
            _licenseInitialized = true;
        }

        public enum SpreadsheetFormat
        {
            Xlsx,
            Ods,
            Pdf,
            Csv
        }
         
        #region 客製化方法
        private ExcelFile _workBook;
        private ExcelWorksheet _currentSheet;
        public GemBoxSheetHelper(string sheetName)
        {
            //SpreadsheetInfo.SetLicense(LicenseKey); // 或填入商業授權碼
            if (_licenseInitialized) return;

            string encrypted = Environment.GetEnvironmentVariable("GemBoxEncryptedKey");
            string masterKey = Environment.GetEnvironmentVariable("EHRISKey");
            string aad = "EHRIS";

            if (string.IsNullOrWhiteSpace(encrypted) || string.IsNullOrWhiteSpace(masterKey))
                throw new InvalidOperationException("GemBoxEncryptedKey 或 EHRISKey 未設定");

            string licenseKey = SecureEncryptor.Decrypt(encrypted, masterKey, aad);
            SpreadsheetInfo.SetLicense(licenseKey);

            _workBook = new ExcelFile();
            _currentSheet = _workBook.Worksheets.Add(sheetName);
        }

        //插入大標題列
        public void SetTitle(
            string TitleText, 
            int rowStartIdx = 0,
            int colStartIdx = 0,
            int rowEndIdx = 0,
            int colEndIdx = 0,
            int TitleFontSize = 14,
            HorizontalAlignmentStyle horizontalAlignmentStyle = HorizontalAlignmentStyle.Center,
            VerticalAlignmentStyle verticalAlignmentStyle = VerticalAlignmentStyle.Center,
            string TitleColorHex = "#000000")
        {
            if (_workBook == null)
                return;
            if (_currentSheet == null)
                return;

            if (rowEndIdx < rowStartIdx || colEndIdx < colStartIdx)
                throw new ArgumentException("結束位置不能小於起始位置");

            if (!string.IsNullOrEmpty(TitleText))
            {
                var titleCell = _currentSheet.Cells[rowStartIdx, colStartIdx];
                titleCell.Value = TitleText;
                _currentSheet.Cells.GetSubrangeAbsolute(rowStartIdx, colStartIdx, rowEndIdx, colEndIdx).Merged = true;

                var titleColor = SpreadsheetColor.FromArgb(ColorTranslator.FromHtml(TitleColorHex).R,
                                                           ColorTranslator.FromHtml(TitleColorHex).G,
                                                           ColorTranslator.FromHtml(TitleColorHex).B);

                var style = titleCell.Style;
                style.Font.Size = TitleFontSize * 20;
                style.Font.Weight = ExcelFont.BoldWeight;
                style.Font.Color = titleColor;// SpreadsheetColor.FromName(ColorName.White);
                //style.FillPattern.SetSolid(titleColor);
                style.HorizontalAlignment = horizontalAlignmentStyle;
                style.VerticalAlignment = verticalAlignmentStyle;
            }
        }

        //插入欄位
        public void Cell(
           object cellText, 
           int rowStartIdx = 0,
           int colStartIdx = 0,
           int rowEndIdx = 0,
           int colEndIdx = 0,
           int cellFontSize = 12,
           HorizontalAlignmentStyle horizontalAlignmentStyle = HorizontalAlignmentStyle.Left,
           VerticalAlignmentStyle verticalAlignmentStyle = VerticalAlignmentStyle.Center,
           string cellColorHex = "#000000")
        {
            if (_workBook == null || _currentSheet == null)
                return;

            if (rowEndIdx < rowStartIdx || colEndIdx < colStartIdx)
                throw new ArgumentException("結束位置不能小於起始位置");

            var titleCell = _currentSheet.Cells[rowStartIdx, colStartIdx];

            titleCell.Value = cellText;
             
            // 合併儲存格（如有範圍）
            if (rowEndIdx > rowStartIdx || colEndIdx > colStartIdx)
            {
                var range = _currentSheet.Cells.GetSubrangeAbsolute(rowStartIdx, colStartIdx, rowEndIdx, colEndIdx);
                if (range.Merged) range.Merged = false;
                range.Merged = true;
            }

            // 設定樣式
            var titleColor = SpreadsheetColor.FromArgb(ColorTranslator.FromHtml(cellColorHex).R,
                                                       ColorTranslator.FromHtml(cellColorHex).G,
                                                       ColorTranslator.FromHtml(cellColorHex).B);

            var style = titleCell.Style;
            style.Font.Size = cellFontSize * 20;
            style.Font.Color = titleColor;
            style.HorizontalAlignment = horizontalAlignmentStyle;
            style.VerticalAlignment = verticalAlignmentStyle;
            style.WrapText = true;
            if (cellText is DateTime)
            {
                style.NumberFormat = "yyyy-MM-dd HH:mm";
            }

        }

        public void InsertList<T>(List<T> data,
          int rowStartIdx = 0,
          int colStartIdx = 0,
          bool printHeader=false,
          int cellFontSize = 12,
          HorizontalAlignmentStyle horizontalAlignmentStyle = HorizontalAlignmentStyle.Left,
          VerticalAlignmentStyle verticalAlignmentStyle = VerticalAlignmentStyle.Center,
          string cellColorHex = "#000000")
        {
            bool isObjectArray = typeof(T) == typeof(object[]);
            int headerOffset = printHeader ? 1 : 0;
             
            if (isObjectArray) 
            {
                // 標題列
                if (printHeader)
                {
                    var firstRow = data.FirstOrDefault() as object[];
                    for (int i = 0; i < firstRow?.Length; i++)
                    {
                        var cell = _currentSheet.Cells[rowStartIdx, colStartIdx + i];
                        cell.Value = $"欄位{i + 1}"; // 可改成外部傳入欄位名稱
                        var style = cell.Style;
                        style.Font.Size = cellFontSize*20;
                        style.HorizontalAlignment = horizontalAlignmentStyle;
                        style.VerticalAlignment = verticalAlignmentStyle;
                    }

                }

                // 資料列
                for (int row = 0; row < data.Count; row++)
                {
                    var item = data[row];
                    var values = item as object[];
                    for (int col = 0; col < values.Length; col++)
                    {
                        //var cell = _currentSheet.Cells[rowStartIdx + row + headerOffset, colStartIdx + col];
                        //cell.Value = values[col].ToString();
                        //var style = cell.Style;
                        //style.Font.Size = cellFontSize;
                        //style.HorizontalAlignment = horizontalAlignmentStyle;
                        //style.VerticalAlignment = verticalAlignmentStyle;


                        _currentSheet.Cells[rowStartIdx + row + headerOffset, colStartIdx + col].Value = values[col];

                        var style = _currentSheet.Cells[rowStartIdx + row + headerOffset, colStartIdx + col].Style;
                        style.Font.Size = cellFontSize * 20;
                        style.HorizontalAlignment = horizontalAlignmentStyle;
                        style.VerticalAlignment = verticalAlignmentStyle;
                    } 
                }

            }
            else
            {
                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                // 標題列
                if (printHeader)
                {
                    for (int i = 0; i < properties.Length; i++)
                    {
                        var prop = properties[i];
                        var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
                        var header = displayAttr?.Name ?? prop.Name;

                        var cell = _currentSheet.Cells[rowStartIdx + 0, colStartIdx + i];
                        cell.Value = header;
                        var style = cell.Style;
                        style.Font.Size = cellFontSize * 20;
                        style.HorizontalAlignment = horizontalAlignmentStyle;
                        style.VerticalAlignment = verticalAlignmentStyle;
                    }
                }

                // 資料列
                for (int row = 0; row < data.Count; row++)
                {
                    var item = data[row];
                    for (int col = 0; col < properties.Length; col++)
                    {
                        var value = properties[col].GetValue(item);
                        if (printHeader)
                        {
                            _currentSheet.Cells[rowStartIdx + row + 1, colStartIdx + col].Value = value;

                            var style = _currentSheet.Cells[rowStartIdx + row + 1, colStartIdx + col].Style;
                            style.Font.Size = cellFontSize * 20;
                            style.HorizontalAlignment = horizontalAlignmentStyle;
                            style.VerticalAlignment = verticalAlignmentStyle;
                        }
                        else
                        {
                            _currentSheet.Cells[rowStartIdx + row, colStartIdx + col].Value = value;

                            var style = _currentSheet.Cells[rowStartIdx + row, colStartIdx + col].Style;
                            style.Font.Size = cellFontSize * 20;
                            style.HorizontalAlignment = horizontalAlignmentStyle;
                            style.VerticalAlignment = verticalAlignmentStyle;
                        }
                    }
                }
            }
               
        }

        public void InsertList<T>(List<T> data,
         int rowStartIdx = 0,
         int colStartIdx = 0,
         int printColNum = 0,
         bool printHeader = false,
         int cellFontSize = 12,
         HorizontalAlignmentStyle horizontalAlignmentStyle = HorizontalAlignmentStyle.Left,
         VerticalAlignmentStyle verticalAlignmentStyle = VerticalAlignmentStyle.Center,
         string cellColorHex = "#000000")
        {
            bool isObjectArray = typeof(T) == typeof(object[]);
            int headerOffset = printHeader ? 1 : 0;

            if (isObjectArray)
            {
                // 標題列
                if (printHeader)
                {
                    var firstRow = data.FirstOrDefault() as object[];
                    int columnNum = firstRow?.Length ?? 0;
                    if (printColNum > 0)
                    {
                        columnNum = printColNum;
                    }
                    if (columnNum > firstRow.Length)
                    {
                        columnNum = firstRow.Length;
                    }
                    for (int i = 0; i < columnNum; i++)
                    {
                        var cell = _currentSheet.Cells[rowStartIdx, colStartIdx + i];
                        cell.Value = $"欄位{i + 1}"; // 可改成外部傳入欄位名稱
                        var style = cell.Style;
                        style.Font.Size = cellFontSize * 20;
                        style.HorizontalAlignment = horizontalAlignmentStyle;
                        style.VerticalAlignment = verticalAlignmentStyle;
                    }

                }

                // 資料列
                for (int row = 0; row < data.Count; row++)
                {
                    var item = data[row];
                    var values = item as object[];
                    int columnNum = values?.Length ?? 0;
                    if (printColNum > 0)
                    {
                        columnNum = printColNum;
                    }
                    if (columnNum > values.Length)
                    {
                        columnNum = values.Length;
                    }
                    for (int col = 0; col < columnNum; col++)
                    {
                        //var cell = _currentSheet.Cells[rowStartIdx + row + headerOffset, colStartIdx + col];
                        //cell.Value = values[col].ToString();
                        //var style = cell.Style;
                        //style.Font.Size = cellFontSize;
                        //style.HorizontalAlignment = horizontalAlignmentStyle;
                        //style.VerticalAlignment = verticalAlignmentStyle;


                        _currentSheet.Cells[rowStartIdx + row + headerOffset, colStartIdx + col].Value = values[col];

                        var style = _currentSheet.Cells[rowStartIdx + row + headerOffset, colStartIdx + col].Style;
                        style.Font.Size = cellFontSize * 20;
                        style.HorizontalAlignment = horizontalAlignmentStyle;
                        style.VerticalAlignment = verticalAlignmentStyle;
                    }
                }

            }
            else
            {
                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                int columnNum = properties?.Length ?? 0;
                if (printColNum > 0)
                {
                    columnNum = printColNum;
                }
                if (columnNum > properties.Length)
                {
                    columnNum = properties.Length;
                }
                // 標題列
                if (printHeader)
                { 
                    for (int i = 0; i < columnNum; i++)
                    {
                        var prop = properties[i];
                        var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
                        var header = displayAttr?.Name ?? prop.Name;

                        var cell = _currentSheet.Cells[rowStartIdx + 0, colStartIdx + i];
                        cell.Value = header;
                        var style = cell.Style;
                        style.Font.Size = cellFontSize * 20;
                        style.HorizontalAlignment = horizontalAlignmentStyle;
                        style.VerticalAlignment = verticalAlignmentStyle;
                    }
                }

                // 資料列
                for (int row = 0; row < data.Count; row++)
                {
                    var item = data[row];
                    for (int col = 0; col < columnNum; col++)
                    {
                        var value = properties[col].GetValue(item);
                        if (printHeader)
                        {
                            _currentSheet.Cells[rowStartIdx + row + 1, colStartIdx + col].Value = value;

                            var style = _currentSheet.Cells[rowStartIdx + row + 1, colStartIdx + col].Style;
                            style.Font.Size = cellFontSize * 20;
                            style.HorizontalAlignment = horizontalAlignmentStyle;
                            style.VerticalAlignment = verticalAlignmentStyle;
                        }
                        else
                        {
                            _currentSheet.Cells[rowStartIdx + row, colStartIdx + col].Value = value;

                            var style = _currentSheet.Cells[rowStartIdx + row, colStartIdx + col].Style;
                            style.Font.Size = cellFontSize * 20;
                            style.HorizontalAlignment = horizontalAlignmentStyle;
                            style.VerticalAlignment = verticalAlignmentStyle;
                        }
                    }
                }
            }

        }

        public void SetAlignmentRange(
                int rowStartIdx,
                int colStartIdx,
                int rowEndIdx,
                int colEndIdx,
                HorizontalAlignmentStyle horizontalAlignmentStyle,
                VerticalAlignmentStyle verticalAlignmentStyle)
        {
            if (_workBook == null)
                return;
            if (_currentSheet == null)
                return;

            if (rowEndIdx < rowStartIdx || colEndIdx < colStartIdx)
                throw new ArgumentException("結束位置不能小於起始位置");

            var range = _currentSheet.Cells.GetSubrangeAbsolute(rowStartIdx, colStartIdx, rowEndIdx, colEndIdx);
            foreach (var cell in range)
            {
                var style = cell.Style;
                style.HorizontalAlignment = horizontalAlignmentStyle;
                style.VerticalAlignment = verticalAlignmentStyle;
            }
        }

        public void ApplyCellFormatStyle(int rowStartIdx = 0,
           int colStartIdx = 0,
           int rowEndIdx = 0,
           int colEndIdx = 0,
           string formatString = "@")
        {
            var range = _currentSheet.Cells.GetSubrangeAbsolute(rowStartIdx, colStartIdx, rowEndIdx, colEndIdx);
            foreach (var cell in range)
            {
                var style = cell.Style;

                style.NumberFormat = formatString;// "#,##0.##";
            } 
            
        }


        //public void InsertObjectArrayList(
        //    List<object[]> data,
        //    List<string> headers = null,
        //    int rowStartIdx = 0,
        //    int colStartIdx = 0,
        //    int cellFontSize = 12,
        //    HorizontalAlignmentStyle horizontalAlignmentStyle = HorizontalAlignmentStyle.Left,
        //    VerticalAlignmentStyle verticalAlignmentStyle = VerticalAlignmentStyle.Center,
        //    string cellColorHex = "#000000")
        //{
        //    // 標題列（如果有提供）
        //    if (headers != null)
        //    {
        //        for (int i = 0; i < headers.Count; i++)
        //        {
        //            var cell = _currentSheet.Cells[rowStartIdx, colStartIdx + i];
        //            cell.Value = headers[i];
        //            var style = cell.Style;
        //            style.Font.Size = cellFontSize * 20;
        //            style.HorizontalAlignment = horizontalAlignmentStyle;
        //            style.VerticalAlignment = verticalAlignmentStyle;
        //        }
        //        rowStartIdx++; // 資料列往下移一列
        //    }

        //    // 資料列
        //    for (int row = 0; row < data.Count; row++)
        //    {
        //        var rowData = data[row];
        //        for (int col = 0; col < rowData.Length; col++)
        //        {
        //            var cell = _currentSheet.Cells[rowStartIdx + row, colStartIdx + col];
        //            cell.Value = rowData[col];
        //            var style = cell.Style;
        //            style.Font.Size = cellFontSize * 20;
        //            style.HorizontalAlignment = horizontalAlignmentStyle;
        //            style.VerticalAlignment = verticalAlignmentStyle;
        //        }
        //    }
        //}

        public void SetRowHeight(int rowIndex, double height)
        {
            _currentSheet.Rows[rowIndex].Height = (int)(height * 20);
        }

        public void SetRowHeights(int startRow, int endRow, double height)
        {
            for (int i = startRow; i <= endRow; i++)
                _currentSheet.Rows[i].Height = (int)(height * 20);
        }


        public void SetColumnWidth(int columnIndex, double width)
        {
            _currentSheet.Columns[columnIndex].Width = (int)(width * 256); 
        }

        public void SetColumnWidths(int startCol, int endCol, double width)
        {
            for (int i = startCol; i <= endCol; i++)
                _currentSheet.Columns[i].Width = (int)(width * 256);
        }

        public (MemoryStream stream, string contentType, string fileExtension) Export(SpreadsheetFormat exportFormat)
        {
            var stream = new MemoryStream();
            GemBox.Spreadsheet.SaveOptions saveOptions;
            string contentType;
            string extension;

            switch (exportFormat)
            {
                case SpreadsheetFormat.Ods:
                    saveOptions = GemBox.Spreadsheet.SaveOptions.OdsDefault;
                    contentType = "application/vnd.oasis.opendocument.spreadsheet";
                    extension = "ods";
                    break;
                case SpreadsheetFormat.Pdf:
                    saveOptions = GemBox.Spreadsheet.SaveOptions.PdfDefault;
                    contentType = "application/pdf";
                    extension = "pdf";
                    break;
                case SpreadsheetFormat.Csv:
                    saveOptions = GemBox.Spreadsheet.SaveOptions.CsvDefault;
                    contentType = "text/csv";
                    extension = "csv";
                    break;
                default:
                    saveOptions = GemBox.Spreadsheet.SaveOptions.XlsxDefault;
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    extension = "xlsx";
                    break;
            }

            _workBook.Save(stream, saveOptions);
            stream.Position = 0;
            return (stream, contentType, extension);
        }
        #endregion

        #region 直接輸出
        public static MemoryStream Export<T>(List<T> data, SpreadsheetFormat format, string sheetName = "Sheet1")
        {
            //SpreadsheetInfo.SetLicense(LicenseKey); // 或填入商業授權碼
            EnsureLicense();

            var workbook = new ExcelFile();
            var sheet = workbook.Worksheets.Add(sheetName);

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 標題列
            for (int i = 0; i < properties.Length; i++)
            {
                var prop = properties[i];
                var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
                var header = displayAttr?.Name ?? prop.Name;

                var cell = sheet.Cells[0, i];
                cell.Value = header;

                var htmlColor = "#4F81BD";
                var systemColor = ColorTranslator.FromHtml(htmlColor);
                var gemboxColor = SpreadsheetColor.FromArgb(systemColor.R, systemColor.G, systemColor.B);

                cell.Style.FillPattern.SetSolid(gemboxColor);
            }

            // 資料列
            for (int row = 0; row < data.Count; row++)
            {
                var item = data[row];
                for (int col = 0; col < properties.Length; col++)
                {
                    var value = properties[col].GetValue(item);
                    sheet.Cells[row + 1, col].Value = value;
                }
            }

            var stream = new MemoryStream();

            SaveOptions saveOption = format switch
            {
                SpreadsheetFormat.Xlsx => SaveOptions.XlsxDefault,
                SpreadsheetFormat.Ods => SaveOptions.OdsDefault,
                SpreadsheetFormat.Csv => SaveOptions.CsvDefault,
                SpreadsheetFormat.Pdf => SaveOptions.PdfDefault,
                _ => SaveOptions.XlsxDefault
            };

            workbook.Save(stream, saveOption);
            stream.Position = 0;

            return stream;
        }

        //public void SetBorderRange(
        //     int rowStartIdx,
        //     int colStartIdx,
        //     int rowEndIdx,
        //     int colEndIdx,
        //     MultipleBorders borders = MultipleBorders.All,
        //     LineStyle lineStyle = LineStyle.Thin,
        //     SpreadsheetColor? borderColor = null)
        //{
        //    var range = _currentSheet.Cells.GetSubrangeAbsolute(rowStartIdx, colStartIdx, rowEndIdx, colEndIdx);

        //    // 若未指定顏色，使用黑色
        //    var actualColor = borderColor ?? SpreadsheetColor.FromName(ColorName.Black);

        //    foreach (var cell in range)
        //    {
        //        var style = cell.Style;
        //        style.Borders.SetBorders(borders, actualColor, lineStyle);
        //    }
        //}

        public void SetBorderRange(
            int rowStartIdx,
            int colStartIdx,
            int rowEndIdx,
            int colEndIdx,
            MultipleBorders innerBorders = MultipleBorders.All,
            LineStyle innerLineStyle = LineStyle.Thin,
            SpreadsheetColor? innerColor = null,
            LineStyle outerLineStyle = LineStyle.Thick,
            SpreadsheetColor? outerColor = null)
        {
            var range = _currentSheet.Cells.GetSubrangeAbsolute(rowStartIdx, colStartIdx, rowEndIdx, colEndIdx);

            var actualInnerColor = innerColor ?? SpreadsheetColor.FromName(ColorName.Black);
            var actualOuterColor = outerColor ?? SpreadsheetColor.FromName(ColorName.Black);

            // 先設定整個範圍的細框
            foreach (var cell in range)
            {
                cell.Style.Borders.SetBorders(innerBorders, actualInnerColor, innerLineStyle);
            }

            foreach (var cell in range)
            {
                var style = cell.Style;
                 
                // 判斷是否在外框邊界，套用粗框
                if (cell.Row.Index == rowStartIdx)
                    style.Borders.SetBorders(MultipleBorders.Top, actualOuterColor, outerLineStyle);
                if (cell.Row.Index == rowEndIdx)
                    style.Borders.SetBorders(MultipleBorders.Bottom, actualOuterColor, outerLineStyle);
                if (cell.Column.Index == colStartIdx)
                    style.Borders.SetBorders(MultipleBorders.Left, actualOuterColor, outerLineStyle);
                if (cell.Column.Index == colEndIdx)
                    style.Borders.SetBorders(MultipleBorders.Right, actualOuterColor, outerLineStyle);
            }

        }


        public static (MemoryStream stream, string contentType, string fileName) Export<T>(List<T> data, string sheetName, SpreadsheetFormat exportFormat)
        {
            //SpreadsheetInfo.SetLicense(LicenseKey); // 或填入商業授權碼 
            EnsureLicense();

            var workbook = new ExcelFile();
            var sheet = workbook.Worksheets.Add(sheetName);

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 標題列
            for (int i = 0; i < properties.Length; i++)
            {
                var prop = properties[i];
                var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
                var header = displayAttr?.Name ?? prop.Name;

                var cell = sheet.Cells[0, i];
                cell.Value = header;

                var color = SpreadsheetColor.FromArgb(79, 129, 189); // #4F81BD
                cell.Style.Font.Weight = ExcelFont.BoldWeight;
                cell.Style.Font.Color = SpreadsheetColor.FromName(ColorName.White);
                cell.Style.FillPattern.SetSolid(color);
            }

            // 資料列
            for (int row = 0; row < data.Count; row++)
            {
                var item = data[row];
                for (int col = 0; col < properties.Length; col++)
                {
                    var value = properties[col].GetValue(item);
                    sheet.Cells[row + 1, col].Value = value;
                }
            }

            var stream = new MemoryStream();
            GemBox.Spreadsheet.SaveOptions saveOptions;
            string contentType;
            string extension;

            switch (exportFormat)
            {
                case SpreadsheetFormat.Ods:
                    saveOptions = GemBox.Spreadsheet.SaveOptions.OdsDefault;
                    contentType = "application/vnd.oasis.opendocument.spreadsheet";
                    extension = "ods";
                    break;
                case SpreadsheetFormat.Pdf:
                    saveOptions = GemBox.Spreadsheet.SaveOptions.PdfDefault;
                    contentType = "application/pdf";
                    extension = "pdf";
                    break;
                case SpreadsheetFormat.Csv:
                    saveOptions = GemBox.Spreadsheet.SaveOptions.CsvDefault;
                    contentType = "text/csv";
                    extension = "csv";
                    break;
                default:
                    saveOptions = GemBox.Spreadsheet.SaveOptions.XlsxDefault;
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    extension = "xlsx";
                    break;
            }

            workbook.Save(stream, saveOptions);
            stream.Position = 0;
            return (stream, contentType, $"{sheetName}.{extension}");
        }

        //public class ExportOptions
        //{
        //    public string TitleText { get; set; } = null;
        //    public int TitleMergeColumns { get; set; } = 0;
        //    public string TitleColorHex { get; set; } = "#4F81BD";
        //    public int TitleFontSize { get; set; } = 16;
        //    public HorizontalAlignmentStyle TitleAlignment { get; set; } = HorizontalAlignmentStyle.Center;

        //    public List<(int row, int colStart, int colEnd)> CellMerges { get; set; } = new();
        //    public bool AutoFitColumns { get; set; } = true;
        //}
        //public static MemoryStream Export<T>(List<T> data, SpreadsheetFormat format, string sheetName = "Sheet1", ExportOptions options = null)
        //{
        //    SpreadsheetInfo.SetLicense(LicenseKey);
        //    var workbook = new ExcelFile();
        //    var sheet = workbook.Worksheets.Add(sheetName);
        //    options ??= new ExportOptions();

        //    var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        //    int startRow = 0;

        //    // (1) 大標題列
        //    if (!string.IsNullOrEmpty(options.TitleText))
        //    {
        //        var titleCell = sheet.Cells[0, 0];
        //        titleCell.Value = options.TitleText;
        //        sheet.Cells.GetSubrangeAbsolute(0, 0, 0, options.TitleMergeColumns - 1).Merged = true;

        //        var titleColor = SpreadsheetColor.FromArgb(ColorTranslator.FromHtml(options.TitleColorHex).R,
        //                                                   ColorTranslator.FromHtml(options.TitleColorHex).G,
        //                                                   ColorTranslator.FromHtml(options.TitleColorHex).B);

        //        var style = titleCell.Style;
        //        style.Font.Size = options.TitleFontSize;
        //        style.Font.Weight = ExcelFont.BoldWeight;
        //        style.Font.Color = SpreadsheetColor.FromName(ColorName.White);
        //        style.FillPattern.SetSolid(titleColor);
        //        style.HorizontalAlignment = options.TitleAlignment;

        //        startRow = 1;
        //    }

        //    // (2) 標題列
        //    for (int i = 0; i < properties.Length; i++)
        //    {
        //        var prop = properties[i];
        //        var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
        //        var header = displayAttr?.Name ?? prop.Name;

        //        var cell = sheet.Cells[startRow, i];
        //        cell.Value = header;

        //        var headerColor = SpreadsheetColor.FromArgb(79, 129, 189); // #4F81BD
        //        cell.Style.Font.Weight = ExcelFont.BoldWeight;
        //        cell.Style.Font.Color = SpreadsheetColor.FromName(ColorName.White);
        //        cell.Style.FillPattern.SetSolid(headerColor);
        //        cell.Style.HorizontalAlignment = HorizontalAlignmentStyle.Center;
        //    }

        //    // (3) 資料列
        //    for (int row = 0; row < data.Count; row++)
        //    {
        //        var item = data[row];
        //        for (int col = 0; col < properties.Length; col++)
        //        {
        //            var value = properties[col].GetValue(item);
        //            sheet.Cells[startRow + 1 + row, col].Value = value;
        //        }
        //    }

        //    // (4) 欄位合併
        //    foreach (var merge in options.CellMerges)
        //    {
        //        sheet.Cells.GetSubrangeAbsolute(merge.row, merge.colStart, merge.row, merge.colEnd).Merged = true;
        //    }

        //    // (5) 自動欄寬
        //    if (options.AutoFitColumns)
        //        for (int i = 0; i < properties.Length; i++)
        //        {
        //            sheet.Columns[i].AutoFit();
        //            if (sheet.Columns[i].Width > 50) // 單位是字符寬度
        //                sheet.Columns[i].Width = 50;
        //        }

        //    // 匯出
        //    var stream = new MemoryStream();
        //    SaveOptions saveOption = format switch
        //    {
        //        SpreadsheetFormat.Xlsx => SaveOptions.XlsxDefault,
        //        SpreadsheetFormat.Ods => SaveOptions.OdsDefault,
        //        SpreadsheetFormat.Csv => SaveOptions.CsvDefault,
        //        SpreadsheetFormat.Pdf => SaveOptions.PdfDefault,
        //        _ => SaveOptions.XlsxDefault
        //    };

        //    workbook.Save(stream, saveOption);
        //    stream.Position = 0;
        //    return stream;
        //}
        #endregion
    }
}
