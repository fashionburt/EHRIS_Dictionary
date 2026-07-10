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
    public class STA301014R02TemplateLayout : ReportTemplateLayoutBase
    {
        public override bool ShouldPrintFooterSummary { get; set; } = true;
        public string _sMonthText = string.Empty;
        
        public STA301014R02TemplateLayout(string sMonthText)
        {
            _sMonthText = sMonthText; 
        }
         
        public override int ApplyLayout(ExcelGenerater excelGen, string TitleName)
        {
            int titleRow = 0;
            int headerRow = 1;

            int ColumnNum = 7;

            int RowIdx = 0;
            int ColIdx = 0;

            //大標題
            excelGen.SetTitle($"{TitleName}", RowIdx, ColIdx, RowIdx, (ColIdx + ColumnNum - 1), 14);

            //RowIdx++;
            //string SubTitleContent = $"製表日期：{System.DateTime.Now.ToRocDate()}\r\n(單位：人)";
            //excelGen.SetTitle(SubTitleContent, RowIdx, ColIdx, RowIdx, (ColIdx + ColumnNum - 1), 12, HorizontalAlignment.Right);
            //excelGen.SetRowHeight(RowIdx, 41.25);

            //第二層標題
            RowIdx++;
            excelGen.Cell("單位別", RowIdx, ColIdx, RowIdx , ColIdx, 12, HorizontalAlignment.Center); ColIdx++;
            excelGen.Cell("0小時", RowIdx, ColIdx, RowIdx, ColIdx , 12, HorizontalAlignment.Center); ColIdx++;
            excelGen.Cell("20小時以下", RowIdx, ColIdx, RowIdx, (ColIdx ), 12, HorizontalAlignment.Center); ColIdx++;
            excelGen.Cell("超過20小時，40小時以下", RowIdx, ColIdx, RowIdx, (ColIdx) , 12, HorizontalAlignment.Center); ColIdx++;
            excelGen.Cell("超過40小時，60小時以下", RowIdx , ColIdx, RowIdx, (ColIdx) , 12, HorizontalAlignment.Center); ColIdx++;
            excelGen.Cell("超過60小時，80小時以下", RowIdx, ColIdx, RowIdx, (ColIdx) , 12, HorizontalAlignment.Center); ColIdx++;
            excelGen.Cell("超過80小時", RowIdx, ColIdx, RowIdx, (ColIdx) , 12, HorizontalAlignment.Center); ColIdx++;
           // excelGen.Cell("合計", RowIdx, ColIdx, RowIdx, (ColIdx ), 12, HorizontalAlignment.Center);

            //第三層標題
            //RowIdx +=2;
            //ColIdx = 0;
            //excelGen.Cell("單位", RowIdx, ColIdx, RowIdx, ColIdx, 12, HorizontalAlignment.Center); ColIdx++;
            //for (int i = 0; i < 5; i++)
            //{
            //    excelGen.Cell("男性", RowIdx, ColIdx, RowIdx, ColIdx, 12, HorizontalAlignment.Center); ColIdx++;
            //    excelGen.Cell("女性", RowIdx, ColIdx, RowIdx, ColIdx, 12, HorizontalAlignment.Center); ColIdx++;
            //}
             
            //單位別
            excelGen.SetColumnWidth(0, 15.14);
            excelGen.SetColumnWidth(1, 28);
            excelGen.SetColumnWidth(2, 28);
            excelGen.SetColumnWidth(3, 28);
            excelGen.SetColumnWidth(4, 28);
            excelGen.SetColumnWidth(5, 28);
            excelGen.SetColumnWidth(6, 28);
            excelGen.SetBorderRange(1, 0, 4, ColumnNum - 1);
            return 2;
        }
 
        public override void RenderFooterSummary<T>(ExcelGenerater excelGen, List<T> data, int summaryRow)
        {
            if (data == null || !data.Any()) return;

            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            int rowStartIdx = summaryRow;
            int rowEndIdx = summaryRow ;
            int colStartIdx = 0;

            // 第一欄：顯示「合計」文字 
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
                }
            }
            excelGen.SetHorizontalAlignment(2, 0, summaryRow, 0, HorizontalAlignment.Justify, VerticalAlignment.Center);
            excelGen.SetBorderRange(summaryRow, 0, summaryRow, props.Length-1);
        }
         
        private bool IsNumericType(Type type)
        {
            return type == typeof(int) || type == typeof(double) || type == typeof(decimal) || type == typeof(float) || type == typeof(long);
        }

        public override int ApplyLayout(ExcelGenerater excelGen, ReportColumnMap columnMap, string TitleName, int PrintColumnNum)
        {
            throw new NotImplementedException();
        }
    }
}

