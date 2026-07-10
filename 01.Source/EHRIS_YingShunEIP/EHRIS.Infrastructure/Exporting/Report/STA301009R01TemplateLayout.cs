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
    public class STA301009R01TemplateLayout : ReportTemplateLayoutBase
    {
        public override bool ShouldPrintFooterSummary { get; set; } = false;

        public string _sMonthText = string.Empty;
        public string _eMonthText = string.Empty;

        public STA301009R01TemplateLayout( string sMonthText, string eMonthText)
        { 
            _sMonthText = sMonthText;
            _eMonthText = eMonthText;
        }
         
        public override int ApplyLayout(ExcelGenerater excelGen, string TitleName)
        {
            int titleRow = 0;
            int headerRow = 1;

            int ColumnNum = 6;

            int RowIdx = 0;
            int ColIdx = 0;

            //大標題
            excelGen.SetTitle($"{TitleName}", RowIdx, ColIdx, RowIdx, (ColIdx + ColumnNum - 1), 14);

            RowIdx++;
            string SubTitleContent = $"統計區間：{_sMonthText}至{_eMonthText}";
            excelGen.SetTitle(SubTitleContent, RowIdx, ColIdx, RowIdx, (ColIdx + ColumnNum - 1), 14, HorizontalAlignment.Center);
            excelGen.SetRowHeight(RowIdx, 19.5);

            RowIdx++;
            excelGen.SetTitle("(單位：百分比)", RowIdx, ColIdx, RowIdx, (ColIdx + ColumnNum - 1), 12, HorizontalAlignment.Right);
            excelGen.SetRowHeight(RowIdx, 17.25);

            //第二層標題
            RowIdx++;
            excelGen.Cell("出勤時間", RowIdx, ColIdx, RowIdx + 1, ColIdx, 12, HorizontalAlignment.Center); ColIdx++;
            excelGen.Cell("總計", RowIdx, ColIdx, RowIdx + 1, ColIdx, 12, HorizontalAlignment.Center); ColIdx++;
            excelGen.Cell("公務人員", RowIdx, ColIdx, RowIdx, (ColIdx + 3) - 1, 12, HorizontalAlignment.Center);
            excelGen.Cell("主管人員", RowIdx + 1, ColIdx, RowIdx + 1, ColIdx, 12, HorizontalAlignment.Center); ColIdx++;
            excelGen.Cell("一般人員", RowIdx + 1, ColIdx, RowIdx + 1, ColIdx, 12, HorizontalAlignment.Center); ColIdx++;
            excelGen.Cell("合計", RowIdx + 1, ColIdx, RowIdx + 1, ColIdx, 12, HorizontalAlignment.Center); ColIdx++;
            excelGen.Cell("約聘僱人員", RowIdx, ColIdx, RowIdx + 1, ColIdx, 12, HorizontalAlignment.Center);
            RowIdx++;
            //單位別
            excelGen.SetColumnWidth(0, 26);
            excelGen.SetBorderRange(3, 0, 4, ColumnNum - 1);
           
            return 5;
        }
         
    }
}

