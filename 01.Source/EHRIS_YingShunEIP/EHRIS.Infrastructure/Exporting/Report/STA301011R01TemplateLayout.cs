using DocumentFormat.OpenXml.Vml;
using EHRIS.Tools.Extensions;
using EHRIS.Tools.Office.Excel;
using GemBox.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static EHRIS.Tools.Office.Excel.ExcelGenerater;

namespace EHRIS.Infrastructure.Exporting.Report
{
    public class STA301011R01TemplateLayout : ReportTemplateLayoutBase
    {
        public override bool ShouldPrintFooterSummary { get; set; } = true;
        public string _subTitle = "單位";
        public string _sMonthText = string.Empty;
        public string _eMonthText = string.Empty;

        public STA301011R01TemplateLayout(string subTitle, string sMonthText, string eMonthText)
        {
            _subTitle = subTitle;
            _sMonthText = sMonthText;
            _eMonthText = eMonthText;
        }
         
        public override int ApplyLayout(ExcelGenerater excelGen, string TitleName)
        {
            int titleRow = 0;
            int headerRow = 1;

            int ColumnNum = 11;

            int RowIdx = 0;
            int ColIdx = 0;

            //大標題
            excelGen.SetTitle($"{TitleName}", RowIdx, ColIdx, RowIdx, (ColIdx + ColumnNum - 1), 14);

            RowIdx++;
            string SubTitleContent = $"統計區間：{_sMonthText}至{_eMonthText}";
            excelGen.SetTitle(SubTitleContent, RowIdx, ColIdx, RowIdx, (ColIdx + ColumnNum - 1), 14, HorizontalAlignment.Center);
            excelGen.SetRowHeight(RowIdx, 20.25);

            //第二層標題
            RowIdx++;
            if (_subTitle == "單位")
            {
                excelGen.Cell("單位別", RowIdx, ColIdx, RowIdx + 1, ColIdx, 12, HorizontalAlignment.Center);
            }
            else
                excelGen.Cell(_subTitle, RowIdx, ColIdx, RowIdx + 2, ColIdx, 12, HorizontalAlignment.Center); ColIdx++;

            excelGen.Cell("總計", RowIdx, ColIdx, RowIdx + 1, ColIdx + 1, 12, HorizontalAlignment.Center); ColIdx += 2;
            excelGen.Cell("公務人員", RowIdx, ColIdx, RowIdx, (ColIdx + 6) - 1, 12, HorizontalAlignment.Center);
            excelGen.Cell("主管人員", RowIdx + 1, ColIdx, RowIdx + 1, (ColIdx + 2) - 1, 12, HorizontalAlignment.Center); ColIdx += 2;
            excelGen.Cell("一般人員", RowIdx + 1, ColIdx, RowIdx + 1, (ColIdx + 2) - 1, 12, HorizontalAlignment.Center); ColIdx += 2;
            excelGen.Cell("合計", RowIdx + 1, ColIdx, RowIdx + 1, (ColIdx + 2) - 1, 12, HorizontalAlignment.Center); ColIdx += 2;
            excelGen.Cell("約聘僱人員", RowIdx, ColIdx, RowIdx + 1, (ColIdx + 2) - 1, 12, HorizontalAlignment.Center);


            //第三層標題
            RowIdx += 2;
            ColIdx = 0;
            if (_subTitle == "單位")
            {
                excelGen.Cell(_subTitle, RowIdx, ColIdx, RowIdx, ColIdx, 12, HorizontalAlignment.Center); ColIdx++;
            }
            else
                ColIdx++;

            for (int i = 0; i < 5; i++)
            {
                excelGen.Cell("男性", RowIdx, ColIdx, RowIdx, ColIdx, 12, HorizontalAlignment.Center); ColIdx++;
                excelGen.Cell("女性", RowIdx, ColIdx, RowIdx, ColIdx, 12, HorizontalAlignment.Center); ColIdx++;
            }

            //單位別
            excelGen.SetColumnWidth(0, 15.14);
            excelGen.SetBorderRange(2, 0, 4, ColumnNum - 1);
            return 5;
        }
         
        public override void RenderFooterSummary<T>(ExcelGenerater excelGen, List<T> data, int summaryRow)
        {
            if (data == null || !data.Any()) return;

            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            int rowStartIdx = summaryRow;
            int rowEndIdx = summaryRow + 1;
            int colStartIdx = 0;

            // 第一欄：顯示「合計」文字，合併兩列
            excelGen.Cell("合計", rowStartIdx, colStartIdx, rowEndIdx, colStartIdx, 12, HorizontalAlignment.Center);
            colStartIdx++;

            // 從第二欄開始處理每個欄位
            for (int colIdx = 1; colIdx < props.Length; colIdx++)
            {
                var prop = props[colIdx];
                if (IsNumericType(prop.PropertyType))
                {
                    double total = data.Sum(item => Convert.ToDouble(prop.GetValue(item) ?? 0));
                    // 合併下方列顯示總和
                    excelGen.Cell(total, rowStartIdx, colIdx, rowStartIdx, colIdx, 12, HorizontalAlignment.Center);
                    excelGen.ApplyCellFormatStyle(rowStartIdx, colIdx, rowStartIdx, colIdx, CellFormatStyle.Integer);
                }
            }
            rowStartIdx++;
            for (int colIdx = 1; colIdx < props.Length; colIdx += 2)
            {
                var prop1 = props[colIdx];
                var prop2 = props[colIdx + 1];

                double total1 = 0;
                double total2 = 0;
                if (IsNumericType(prop1.PropertyType))
                {
                    total1 = data.Sum(item => Convert.ToDouble(prop1.GetValue(item) ?? 0));
                }
                if (IsNumericType(prop2.PropertyType))
                {
                    total2 = data.Sum(item => Convert.ToDouble(prop2.GetValue(item) ?? 0));
                }

                // 合併兩欄
                excelGen.Cell(total1 + total2, rowStartIdx, colIdx, rowStartIdx, colIdx + 1, 12, HorizontalAlignment.Center);
                excelGen.ApplyCellFormatStyle(rowStartIdx, colIdx, rowStartIdx+1, colIdx + 1, CellFormatStyle.Integer);
            }

            excelGen.SetBorderRange(summaryRow, 0, summaryRow + 1, props.Length - 1);
        }
         
        private bool IsNumericType(Type type)
        {
            return type == typeof(int) || type == typeof(double) || type == typeof(decimal) || type == typeof(float) || type == typeof(long);
        }
         
    }
}

