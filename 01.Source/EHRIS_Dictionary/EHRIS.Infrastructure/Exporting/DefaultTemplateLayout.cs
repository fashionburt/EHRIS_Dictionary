using EHRIS.Tools.Office.Excel;
using GemBox.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Infrastructure.Exporting
{
    public class DefaultTemplateLayout : ReportTemplateLayoutBase
    {
        public override bool ShouldPrintFooterSummary { get; set; } = false;
         
        public override int ApplyLayout(ExcelGenerater excelGen, string TitleName, int ColumnNum)
        {
            int RowIdx = 0;
            int ColIdx = 0;

            //大標題
            excelGen.SetTitle($"{TitleName}", RowIdx, ColIdx, RowIdx, (ColIdx + ColumnNum - 1), 14);

            return 1;
        }

        public override int ApplyLayout(ExcelGenerater excelGen, ReportColumnMap columnMap, string TitleName)
        {
            throw new NotImplementedException();
        }

        public override int ApplyLayout(ExcelGenerater excelGen, string TitleName)
        {
            throw new NotImplementedException();
        }

        public override int ApplyLayout(ExcelGenerater excelGen, ReportColumnMap columnMap, string TitleName, int PrintColumnNum)
        {
            throw new NotImplementedException();
        }

        public override void RenderFooterSummary<T>(ExcelGenerater excelGen, List<T> data, int summaryRow)
        {
            throw new NotImplementedException();
        }
         
    }
}
