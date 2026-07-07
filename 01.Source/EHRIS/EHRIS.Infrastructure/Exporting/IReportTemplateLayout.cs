using EHRIS.Tools.Office.Excel;
using GemBox.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Infrastructure.Exporting
{
    public interface IReportTemplateLayout
    {
        /// <summary>
        /// 是否列印表尾合計
        /// </summary>
        bool ShouldPrintFooterSummary { get; set; }

        /// <summary>
        /// 列印行數0為自動
        /// </summary>
        int PrintColumnNum { get; set; }  

        /// <summary>
        /// 套用第一層格式-例如大標題
        /// </summary>
        /// <param name="excelGen"></param>
        /// <param name="TitleName">標題名稱</param>
        /// <returns>回傳下一筆列印的列數索引</returns>
        int ApplyLayout(ExcelGenerater excelGen, string TitleName);

        /// <summary>
        /// 套用第一層格式-例如大標題
        /// </summary>
        /// <param name="excelGen"></param>
        /// <param name="TitleName">標題名稱</param>
        /// <param name="ColumnNum">欄位數</param>
        /// <returns>回傳下一筆列印的列數索引</returns>
        int ApplyLayout(ExcelGenerater excelGen, string TitleName, int ColumnNum);

        /// <summary>
        /// 套用第一層格式-例如大標題
        /// </summary>
        /// <param name="excelGen"></param>
        /// <param name="columnMap">欄位數</param>
        /// <param name="TitleName">標題名稱</param>
        /// <returns>回傳下一筆列印的列數索引</returns>
        int ApplyLayout(ExcelGenerater excelGen, ReportColumnMap columnMap, string TitleName);

        /// <summary>
        /// 套用第三層格式-表尾合計樣式重置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="excelGen"></param>
        /// <param name="data"></param>
        /// <param name="summaryRow"></param>
        void RenderFooterSummary<T>(ExcelGenerater excelGen, List<T> data, int summaryRow);

        /// <summary>
        /// 設定框線
        /// </summary>
        /// <param name="excelGen"></param>
        /// <param name="rowStartIdx"></param>
        /// <param name="colStartIdx"></param>
        /// <param name="rowEndIdx"></param>
        /// <param name="colEndIdx"></param>
        void SetBorderRange(ExcelGenerater excelGen,
                    int rowStartIdx,
                    int colStartIdx,
                    int rowEndIdx,
                    int colEndIdx);
    }
}
