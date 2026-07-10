using GemBox.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Infrastructure.Exporting
{
    public static class ReportStyleHelper
    {
        public static void ApplyHeaderStyle(ExcelCell cell)
        {
            var style = cell.Style;
            style.Font.Weight = ExcelFont.BoldWeight;
            style.HorizontalAlignment = HorizontalAlignmentStyle.Center;
            style.VerticalAlignment = VerticalAlignmentStyle.Center;
            style.Borders.SetBorders(MultipleBorders.All, SpreadsheetColor.FromName(ColorName.Black), LineStyle.Thick);
        }

        public static void ApplyCellStyle(ExcelCell cell, ReportColumn column)
        {
            var style = cell.Style;
            style.HorizontalAlignment = HorizontalAlignmentStyle.Left;
            style.VerticalAlignment = VerticalAlignmentStyle.Center;
            if (!string.IsNullOrEmpty(column.Format))
                style.NumberFormat = column.Format;
        }

        public static void SetBorderRange(ExcelWorksheet sheet, int rowStart, int colStart, int rowEnd, int colEnd,
            MultipleBorders innerBorders = MultipleBorders.All,
            LineStyle innerStyle = LineStyle.Thin,
            SpreadsheetColor? innerColor = null,
            LineStyle outerStyle = LineStyle.Thick,
            SpreadsheetColor? outerColor = null)
        {
            var range = sheet.Cells.GetSubrangeAbsolute(rowStart, colStart, rowEnd, colEnd);
            var inner = innerColor ?? SpreadsheetColor.FromName(ColorName.Black);
            var outer = outerColor ?? SpreadsheetColor.FromName(ColorName.Black);

            foreach (var cell in range)
            {
                var style = cell.Style;
                style.Borders.SetBorders(innerBorders, inner, innerStyle);

                if (cell.Row.Index == rowStart)
                    style.Borders.SetBorders(MultipleBorders.Top, outer, outerStyle);
                if (cell.Row.Index == rowEnd)
                    style.Borders.SetBorders(MultipleBorders.Bottom, outer, outerStyle);
                if (cell.Column.Index == colStart)
                    style.Borders.SetBorders(MultipleBorders.Left, outer, outerStyle);
                if (cell.Column.Index == colEnd)
                    style.Borders.SetBorders(MultipleBorders.Right, outer, outerStyle);
            }
        }
    }
}
