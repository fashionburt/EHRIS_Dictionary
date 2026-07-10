extern alias CryptoNew;

using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using EHRIS.Tools.Office.Package;
using GemBox.Spreadsheet;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static CryptoNew::Org.BouncyCastle.Math.EC.ECCurve;
using static EHRIS.Tools.Office.Package.GemBoxSheetHelper;

namespace EHRIS.Tools.Office.Excel
{ 
    public class ExcelGenerater
    { 
        private const string PackageMode = "GemBox";
        public enum ExportFormat
        {
            Xlsx,
            Ods,
            Pdf,
            Csv
        }

        public enum HorizontalAlignment
        {
            Center = 2,
            //
            // 摘要:
            //     The horizontal alignment is right-aligned, meaning that cell contents are aligned
            //     at the right edge of the cell.
            Right = 3,
            //
            // 摘要:
            //     Indicates that the value of the cell should be filled across the entire width
            //     of the cell.
            //
            //     This option is currently not supported in PDF, XPS, and image formats and defaults
            //     to GemBox.Spreadsheet.HorizontalAlignmentStyle.General.
            Fill = 4,
            //
            // 摘要:
            //     The horizontal alignment is justified (flush left and right).
            //
            //     For each line of text, aligns each line of the wrapped text in a cell to the
            //     right and left (except the last line).
            //
            //     If no single line of text wraps in the cell, then the text is not justified.
            Justify = 5,
            //
            // 摘要:
            //     The horizontal alignment is centered across multiple cells.
            //
            //     This option is currently not supported in PDF, XPS, and image formats and defaults
            //     to GemBox.Spreadsheet.HorizontalAlignmentStyle.Center.
            CenterAcross = 6,
            //
            // 摘要:
            //     Indicates that each 'word' in each line of text inside the cell is evenly distributed
            //     across the width of the cell, with flush right and left margins.
            //
            //     When there is also an GemBox.Spreadsheet.CellStyle.Indent value to apply, both
            //     the left and right side of the cell are padded by the indent value.
            //
            //     A 'word' is a set of characters with no space character in them.
            //
            //     Two lines inside a cell are separated by a carriage return.
            Distributed = 7
        }

        public enum VerticalAlignment
        {
            //
            // 摘要:
            //     The vertical alignment is aligned-to-top.
            Top = 0,
            //
            // 摘要:
            //     The vertical alignment is centered across the height of the cell.
            Center = 1,
            //
            // 摘要:
            //     The vertical alignment is aligned-to-bottom.
            Bottom = 2,
            //
            // 摘要:
            //     When text direction is horizontal: the vertical alignment of lines of text is
            //     distributed vertically, where each line of text inside the cell is evenly distributed
            //     across the height of the cell, with flush top and bottom margins.
            //
            //     When text direction is vertical: similar behavior as horizontal justification.
            //     The alignment is justified (flush top and bottom in this case). For each line
            //     of text, each line of the wrapped text in a cell is aligned to the top and bottom
            //     (except the last line). If no single line of text wraps in the cell, then the
            //     text is not justified.
            //
            //     This option is currently not supported in PDF, XPS, and image formats and defaults
            //     to GemBox.Spreadsheet.VerticalAlignmentStyle.Top.
            Justify = 3,
            //
            // 摘要:
            //     When text direction is horizontal: the vertical alignment of lines of text is
            //     distributed vertically, where each line of text inside the cell is evenly distributed
            //     across the height of the cell, with flush top and bottom margins.
            //
            //     When text direction is vertical: behaves exactly as distributed horizontal alignment.
            //     The first words in a line of text (appearing at the top of the cell) are flush
            //     with the top edge of the cell, and the last words of a line of text are flush
            //     with the bottom edge of the cell, and the line of text is distributed evenly
            //     from top to bottom.
            //
            //     This option is currently not supported in PDF, XPS, and image formats and defaults
            //     to GemBox.Spreadsheet.VerticalAlignmentStyle.Center.
            Distributed = 4
        }

        public enum CellFormatStyle
        {
            General,            // 預設格式
            Text,               // 純文字 "@"
            Integer,            // 整數千位 "#,##0"
            Decimal2,           // 小數兩位 "#,##0.00"
            DecimalFlexible,    // 小數最多兩位 "#,##0.##"
            Currency,           // 貨幣 "$#,##0.00"
            Percent,            // 百分比 "0%"
            Date,               // 日期 "yyyy/mm/dd"
            Time,               // 時間 "hh:mm:ss"
            DateTime            // 日期時間 "yyyy/mm/dd hh:mm:ss"
        }

        private string ToFormatStyleString(CellFormatStyle format)
        {
            return format switch
            {
                CellFormatStyle.General => "General",
                CellFormatStyle.Text => "@",
                CellFormatStyle.Integer => "#,##0",
                CellFormatStyle.Decimal2 => "#,##0.00",
                CellFormatStyle.DecimalFlexible => "#,##0.##",
                CellFormatStyle.Currency => "$#,##0.00",
                CellFormatStyle.Percent => "0%",
                CellFormatStyle.Date => "yyyy/mm/dd",
                CellFormatStyle.Time => "hh:mm:ss",
                CellFormatStyle.DateTime => "yyyy/mm/dd hh:mm:ss",
                _ => "General"
            };
        }

        GemBoxSheetHelper _workSheet;
        public ExcelGenerater(string sheetName)
        { 
            _workSheet = new GemBoxSheetHelper(sheetName);
        }

        public void SetTitle(
            string TitleText, 
            int rowStartIdx = 0,
            int colStartIdx = 0,
            int rowEndIdx = 0,
            int colEndIdx = 0,
             int TitleFontSize = 14,
            HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment verticalAlignment = VerticalAlignment.Center,
            string TitleColorHex = "#000000")
        {
            HorizontalAlignmentStyle horizontalAlignmentStyle = Enum.Parse<HorizontalAlignmentStyle>(horizontalAlignment.ToString());
            VerticalAlignmentStyle verticalAlignmentStyle = Enum.Parse<VerticalAlignmentStyle>(verticalAlignment.ToString());

            _workSheet.SetTitle(TitleText, rowStartIdx, colStartIdx, rowEndIdx, colEndIdx, TitleFontSize, horizontalAlignmentStyle, verticalAlignmentStyle, TitleColorHex);
        }

        public void Cell(
          object cellText, 
           int rowStartIdx = 0,
           int colStartIdx = 0,
           int rowEndIdx = 0,
           int colEndIdx = 0,
           int cellFontSize = 12,
           HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center,
           VerticalAlignment verticalAlignment = VerticalAlignment.Center,
           string TitleColorHex = "#000000")
        {
            HorizontalAlignmentStyle horizontalAlignmentStyle = Enum.Parse<HorizontalAlignmentStyle>(horizontalAlignment.ToString());
            VerticalAlignmentStyle verticalAlignmentStyle = Enum.Parse<VerticalAlignmentStyle>(verticalAlignment.ToString());

            _workSheet.Cell(cellText, rowStartIdx, colStartIdx, rowEndIdx, colEndIdx, cellFontSize, horizontalAlignmentStyle, verticalAlignmentStyle, TitleColorHex);
        }

        public void InsertList<T>(List<T> data,
         int rowStartIdx = 0,
         int colStartIdx = 0,
         bool printHeader = false,
         int cellFontSize = 12,
         HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center,
           VerticalAlignment verticalAlignment = VerticalAlignment.Center,
         string cellColorHex = "#000000")
        {
            HorizontalAlignmentStyle horizontalAlignmentStyle = Enum.Parse<HorizontalAlignmentStyle>(horizontalAlignment.ToString());
            VerticalAlignmentStyle verticalAlignmentStyle = Enum.Parse<VerticalAlignmentStyle>(verticalAlignment.ToString());

            _workSheet.InsertList(data, rowStartIdx, colStartIdx, printHeader, cellFontSize, horizontalAlignmentStyle, verticalAlignmentStyle, cellColorHex);

        }

        public void InsertList<T>(List<T> data,
         int rowStartIdx = 0,
         int colStartIdx = 0,
         int printColumNum=0, 
         bool printHeader = false,
         int cellFontSize = 12,
         HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center,
           VerticalAlignment verticalAlignment = VerticalAlignment.Center,
         string cellColorHex = "#000000")
        {
            HorizontalAlignmentStyle horizontalAlignmentStyle = Enum.Parse<HorizontalAlignmentStyle>(horizontalAlignment.ToString());
            VerticalAlignmentStyle verticalAlignmentStyle = Enum.Parse<VerticalAlignmentStyle>(verticalAlignment.ToString());

            _workSheet.InsertList(data, rowStartIdx, colStartIdx, printColumNum, printHeader, cellFontSize, horizontalAlignmentStyle, verticalAlignmentStyle, cellColorHex);

        }

        public void SetHorizontalAlignment(
                    int rowStartIdx,
                    int colStartIdx,
                    int rowEndIdx,
                    int colEndIdx,
                    HorizontalAlignment horizontalAlignment,
                    VerticalAlignment verticalAlignment)
        {
            HorizontalAlignmentStyle horizontalAlignmentStyle = Enum.Parse<HorizontalAlignmentStyle>(horizontalAlignment.ToString());
            VerticalAlignmentStyle verticalAlignmentStyle = Enum.Parse<VerticalAlignmentStyle>(verticalAlignment.ToString());

            _workSheet.SetAlignmentRange(rowStartIdx, colStartIdx, rowEndIdx, colEndIdx, horizontalAlignmentStyle, verticalAlignmentStyle);
        }



        public void SetBorderRange(
                    int rowStartIdx,
                    int colStartIdx,
                    int rowEndIdx,
                    int colEndIdx )
        { 
            _workSheet.SetBorderRange(rowStartIdx, colStartIdx, rowEndIdx, colEndIdx, MultipleBorders.All, LineStyle.Thin, SpreadsheetColor.FromName(ColorName.Black), LineStyle.Thin, SpreadsheetColor.FromName(ColorName.Black));
        }

        public void SetRowHeight(int rowIndex, double height)
        {
            _workSheet.SetRowHeight(rowIndex, height);
        }

        public void SetRowHeights(int startRow, int endRow, double height)
        {
            _workSheet.SetRowHeights(startRow, endRow, height );
        }


        public void SetColumnWidth(int columnIndex, double width)
        {
            _workSheet.SetColumnWidth(columnIndex, width );
        }

        public void SetColumnWidths(int startCol, int endCol, double width)
        {
            _workSheet.SetColumnWidths(startCol, endCol, width );
        }

        /// <summary>
        /// 設定數字格式
        /// </summary>
        /// <param name="rowStartIdx"></param>
        /// <param name="colStartIdx"></param>
        /// <param name="rowEndIdx"></param>
        /// <param name="colEndIdx"></param>
        /// <param name="cellFormatStyle"></param>
        public void ApplyCellFormatStyle(int rowStartIdx = 0,
           int colStartIdx = 0,
           int rowEndIdx = 0,
           int colEndIdx = 0,
           CellFormatStyle cellFormatStyle = CellFormatStyle.General)
        {
            _workSheet.ApplyCellFormatStyle(rowStartIdx, colStartIdx, rowEndIdx, colEndIdx, ToFormatStyleString(cellFormatStyle));
        }

        public void ApplyCellFormatStyleCustom(int rowStartIdx = 0,
           int colStartIdx = 0,
           int rowEndIdx = 0,
           int colEndIdx = 0,
           string formatString = "@")
        {
            _workSheet.ApplyCellFormatStyle(rowStartIdx, colStartIdx, rowEndIdx, colEndIdx, formatString);
        }

        public (MemoryStream stream, string contentType, string fileExtension) Export(ExportFormat exportFormat)
        {
            SpreadsheetFormat spreadsheetFormat = Enum.Parse<SpreadsheetFormat>(exportFormat.ToString());

            (MemoryStream _stream, string _contentType, string _fileExtension) = _workSheet.Export(spreadsheetFormat);

            return (_stream, _contentType, _fileExtension);
        }

        #region 方法2：直接匯出
        /// <summary>
        /// 將任意 List<T> 匯出為 Excel 並回傳 MemoryStream
        /// </summary>
        /// <typeparam name="T">資料型別</typeparam>
        /// <param name="data">資料清單</param>
        /// <param name="sheetName">工作表名稱</param>
        /// <returns>Excel 檔案的 MemoryStream</returns>
        public static MemoryStream ExportToExcel<T>(List<T> data, string sheetName = "Sheet1")
        {
            MemoryStream memory = new MemoryStream();
            if (PackageMode == "ClosedXML")
            {
                memory = ClosedXMLHelper.ExportToExcel(data, sheetName);
            }
            else
            {
                memory = GemBoxSheetHelper.Export(data, GemBoxSheetHelper.SpreadsheetFormat.Xlsx, sheetName);
            }
            return memory;
        }

        public static MemoryStream ExportToCSV<T>(List<T> data, string sheetName = "Sheet1")
        {
            MemoryStream memory = new MemoryStream();
            if (PackageMode == "ClosedXML")
            {
                memory = ClosedXMLHelper.ExportToExcel(data, sheetName);
            }
            else
            {
                memory = GemBoxSheetHelper.Export(data, GemBoxSheetHelper.SpreadsheetFormat.Csv, sheetName);
            }
            return memory;
        }

        public static MemoryStream ExportToODS<T>(List<T> data, string sheetName = "Sheet1")
        {
            MemoryStream memory = new MemoryStream();
            if (PackageMode == "ClosedXML")
            {
                memory = ClosedXMLHelper.ExportToExcel(data, sheetName);
            }
            else
            {
                memory = GemBoxSheetHelper.Export(data, GemBoxSheetHelper.SpreadsheetFormat.Ods, sheetName);
            }
            return memory;
        }

        public static (MemoryStream stream, string contentType, string fileName) Export<T>(List<T> data, string sheetName, ExportFormat format)
        {
            SpreadsheetFormat spreadsheetFormat = Enum.Parse<SpreadsheetFormat>(format.ToString());

            (MemoryStream stream, string contentType, string fileName) = GemBoxSheetHelper.Export(data, sheetName, spreadsheetFormat);

            return (stream, contentType, fileName);
        }


        #endregion


    }
}
