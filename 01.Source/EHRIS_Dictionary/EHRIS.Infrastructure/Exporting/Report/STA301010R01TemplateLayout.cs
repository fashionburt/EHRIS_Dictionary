using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml; 
using EHRIS.Tools.Extensions;
using EHRIS.Tools.Office.Excel;
using GemBox.Spreadsheet;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static EHRIS.Tools.Office.Excel.ExcelGenerater;

namespace EHRIS.Infrastructure.Exporting.Report
{
    public class STA301010R01TemplateLayout : ReportTemplateLayoutBase
    {
        public override bool ShouldPrintFooterSummary { get; set; } = true;
        public string _subTitle = "單位";

        public STA301010R01TemplateLayout(string subTitle)
        {
            _subTitle = subTitle;
        }
         
        public override int ApplyLayout(ExcelGenerater excelGen, ReportColumnMap columnMap, string TitleName)
        {
            int titleRow = 0;
            int headerRow = 1;

            int ColumnNum = 7;

            int RowIdx = 0;
            int ColIdx = 0;

            //大標題
            excelGen.SetTitle($"{TitleName}", RowIdx, ColIdx, RowIdx, (ColIdx + ColumnNum - 1), 14);

            //第二層標題
            RowIdx++;
            excelGen.SetColumnWidth(ColIdx, 13.25);
            excelGen.Cell(_subTitle, RowIdx, ColIdx, RowIdx + 1, ColIdx, 12, HorizontalAlignment.Center); ColIdx++;
            excelGen.SetColumnWidth(ColIdx, 15);
            excelGen.Cell("總計", RowIdx, ColIdx, RowIdx + 1, ColIdx, 12, HorizontalAlignment.Center); ColIdx++;
            excelGen.Cell("日數", RowIdx, ColIdx, RowIdx, ColIdx + columnMap.Columns.Count - 1, 12, HorizontalAlignment.Center);

            RowIdx++;
            for (int i = 0; i < columnMap.Columns.Count; i++)
            {
                var column = columnMap.Columns[i];
                excelGen.SetColumnWidth(ColIdx + i, column.Width);
                excelGen.SetBorderRange(RowIdx, ColIdx + i, RowIdx, ColIdx + i);
                excelGen.Cell(column.Title, RowIdx, ColIdx + i, RowIdx, ColIdx + i, 12, HorizontalAlignment.Center);
            }

            //單位別
            excelGen.SetColumnWidth(0, 15.14);
            excelGen.SetBorderRange(1, 0, 1, ColumnNum - 1);
            return 3;
        }

        public override void RenderFooterSummary<T>(ExcelGenerater excelGen, List<T> data, int summaryRow)
        {
            if (data == null || !data.Any()) return;

            bool isObjectArray = typeof(T) == typeof(object[]);
            int rowStartIdx = summaryRow;
            int rowEndIdx = summaryRow ;
            int colStartIdx = 0;

            // 第一欄：顯示「合計」文字，合併兩列
            excelGen.Cell("合計", rowStartIdx, colStartIdx, rowEndIdx, colStartIdx, 12, HorizontalAlignment.Center);

            if (isObjectArray)
            {
                var firstRow = data[0] as object[];
                int columnNum = firstRow.Length;
                if (PrintColumnNum > 0)
                    columnNum = PrintColumnNum;
                 
                for (int colIdx = 1; colIdx < columnNum; colIdx++)
                {
                    double total = data.Sum(row =>
                    {
                        var value = (row as object[])?[colIdx];
                        return IsNumeric(value) ? Convert.ToDouble(value) : 0;
                    });

                    excelGen.Cell(total, rowStartIdx, colIdx, rowStartIdx, colIdx, 12, HorizontalAlignment.Center);
                    excelGen.ApplyCellFormatStyle(rowStartIdx, colIdx, rowStartIdx, colIdx, CellFormatStyle.Integer);
                }

                excelGen.SetBorderRange(rowStartIdx, 0, rowStartIdx, columnNum - 1);
            }
            else
            {
                var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                int columnNum = props.Length;
                if (PrintColumnNum > 0)
                    columnNum = PrintColumnNum;

                for (int colIdx = 1; colIdx < columnNum; colIdx++)
                {
                    var prop = props[colIdx];
                    if (IsNumericType(prop.PropertyType))
                    {
                        double total = data.Sum(item => Convert.ToDouble(prop.GetValue(item) ?? 0));
                        excelGen.Cell(total, rowStartIdx, colIdx, rowStartIdx, colIdx, 12, HorizontalAlignment.Center);
                        excelGen.ApplyCellFormatStyle(rowStartIdx, colIdx, rowStartIdx, colIdx, CellFormatStyle.Integer);
                    }
                }

                excelGen.SetBorderRange(rowStartIdx, 0, rowStartIdx, columnNum - 1);

                excelGen.SetBorderRange(summaryRow, 0, summaryRow + 1, columnNum - 1);
            }

          
        }
         
        private bool IsNumericType(Type type)
        {
            return type == typeof(int) || type == typeof(double) || type == typeof(decimal) || type == typeof(float) || type == typeof(long);
        }

        private bool IsNumeric(object value)
        {
            return value is int or double or decimal or float or long;
        }

        
    }
}

