using EHRIS.Tools.Office.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Infrastructure.Exporting
{
    public abstract class ReportTemplateLayoutBase : IReportTemplateLayout
    {
        public virtual bool ShouldPrintFooterSummary { get; set; } = false;
        public int PrintColumnNum { get; set; } = 0;
         
        public virtual int ApplyLayout(ExcelGenerater excelGen, string TitleName)
        {
            return 0; 
        }
        public virtual int ApplyLayout(ExcelGenerater excelGen, string TitleName, int ColumnNum)
        {
            return 0;
        }
        public virtual int ApplyLayout(ExcelGenerater excelGen, ReportColumnMap columnMap, string TitleName)
        {
            return 0;
        }
        public virtual int ApplyLayout(ExcelGenerater excelGen, ReportColumnMap columnMap, string TitleName, int PrintColumnNum)
        {
            return 0;
        }

        public virtual void RenderFooterSummary<T>(ExcelGenerater excelGen, List<T> data, int summaryRow)
        {
            
        }

        /// <summary>
        /// 設定框線
        /// </summary>
        /// <param name="excelGen"></param>
        /// <param name="rowStartIdx"></param>
        /// <param name="colStartIdx"></param>
        /// <param name="rowEndIdx"></param>
        /// <param name="colEndIdx"></param>
        public virtual void SetBorderRange(ExcelGenerater excelGen,
                                           int rowStartIdx,
                                           int colStartIdx,
                                           int rowEndIdx,
                                           int colEndIdx)
        {
            // 預設框線邏輯
            excelGen.SetBorderRange(rowStartIdx, colStartIdx, rowEndIdx, colEndIdx);
        }
    }
}
