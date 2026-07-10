using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Tools.Office.Package
{
    // 自訂預設顏色清單
    public static class ExcelColor
    {
        public static readonly XLColor HeaderBackground = XLColor.FromHtml("#4F81BD");
        public static readonly XLColor HeaderFont = XLColor.White;
        public static readonly XLColor DefaultBackground = XLColor.NoColor;
        public static readonly XLColor DefaultFont = XLColor.Black;
        public static readonly XLColor Highlight = XLColor.LightYellow;
        public static readonly XLColor Warning = XLColor.Orange;
    }

    public class ClosedXMLHelper
    {
        protected XLWorkbook _workbook;
        protected IXLWorksheet _worksheet;

        #region 方法1：客製化的部份，彈性擴充方法
        //建立新的客製化物件
        public ClosedXMLHelper(string sheetName = "Sheet1")
        {
            _workbook = new XLWorkbook();
            _worksheet = _workbook.Worksheets.Add(sheetName);
        }

        private XLColor GetColorByName(string name)
        {
            return typeof(ExcelColor)
                .GetField(name, BindingFlags.Public | BindingFlags.Static)
                ?.GetValue(null) as XLColor;
        }

        /// <summary>
        /// 新增欄位值
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <param name="backgroundColor"></param>
        /// <param name="fontColor"></param>
        /// <param name="bold"></param>
        public void AddCell(int row, int column, object value, string backgroundColorName = null, string fontColorName = null, bool bold = false)
        {
            var cell = _worksheet.Cell(row, column);
            cell.Value = value?.ToString() ?? string.Empty;

            var bgColor = GetColorByName(backgroundColorName) ?? ExcelColor.DefaultBackground;
            var fontColor = GetColorByName(fontColorName) ?? ExcelColor.DefaultFont;

            cell.Style.Fill.BackgroundColor = bgColor;
            cell.Style.Font.FontColor = fontColor;
            if (bold)
                cell.Style.Font.Bold = true;
        }

        /// <summary>
        /// 合併欄位
        /// </summary>
        /// <param name="rangeAddress"></param>
        /// <param name="value"></param>
        /// <param name="backgroundColor"></param>
        /// <param name="fontColor"></param>
        /// <param name="bold"></param>
        public void MergeRange(string rangeAddress, object value = null, string backgroundColorName = null, string fontColorName = null, bool bold = false)
        {
            var range = _worksheet.Range(rangeAddress);
            range.Merge();

            var cell = range.FirstCell();
            cell.Value = value?.ToString() ?? string.Empty;

            var bgColor = GetColorByName(backgroundColorName) ?? ExcelColor.DefaultBackground;
            var fontColor = GetColorByName(fontColorName) ?? ExcelColor.DefaultFont;

            cell.Style.Fill.BackgroundColor = bgColor;
            cell.Style.Font.FontColor = fontColor;

            if (bold)
                cell.Style.Font.Bold = true;
        }

        /// <summary>
        /// 插入表格 
        /// </summary>
        /// <typeparam name="T">
        /// <param name="data">List<typeparamref name="T"/></typeparam></param>
        /// <param name="startRow">開始RowIDX</param>
        /// <param name="startColumn">開始ColumnIDX</param>
        /// <param name="customFormatters">
        /// var formatters = new Dictionary<string, Func<object, object>>
        /// {
        /// { "DepName", val => $"【{val}】" },
        /// { "TotalMale", val => ((int) val) > 0 ? $"{val} 人" : "無" },
        /// { "publicManagerFemale", val => null } // 測試 null formatter 回傳
        /// }; 
        /// </param>
        public void InsertListToSheet<T>(
            List<T> data,
            int startRow = 1,
            int startColumn = 1,
            Dictionary<string, Func<object, object>> customFormatters = null)
        {
            if (data == null || data.Count == 0)
                return;

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 標題列
            for (int i = 0; i < properties.Length; i++)
            {
                var prop = properties[i];
                var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
                var header = displayAttr?.Name ?? prop.Name;

                var cell = _worksheet.Cell(startRow, startColumn + i);
                cell.Value = header;
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4F81BD");
                cell.Style.Font.FontColor = XLColor.White;
            }

            // 資料列
            for (int row = 0; row < data.Count; row++)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    var prop = properties[col];
                    var value = prop.GetValue(data[row]);
                    var cell = _worksheet.Cell(startRow + 1 + row, startColumn + col);

                    if (value == null)
                    {
                        cell.Value = string.Empty;
                        continue;
                    }

                    if (customFormatters != null && customFormatters.TryGetValue(prop.Name, out var formatter))
                    {
                        var formatted = formatter(value);
                        cell.Value = formatted?.ToString() ?? string.Empty;
                        continue;
                    }

                    var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    if (type == typeof(int) || type == typeof(long) || type == typeof(short))
                    {
                        cell.Value = Convert.ToInt64(value);
                        cell.Style.NumberFormat.Format = "#,##0";
                    }
                    else if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
                    {
                        cell.Value = Convert.ToDouble(value);
                        cell.Style.NumberFormat.Format = "#,##0.00";
                    }
                    else if (type == typeof(DateTime))
                    {
                        cell.Value = (DateTime)value;
                        cell.Style.DateFormat.Format = "yyyy/mm/dd";
                    }
                    else if (type == typeof(bool))
                    {
                        cell.Value = (bool)value ? "是" : "否";
                    }
                    else
                    {
                        cell.Value = value.ToString();
                    }
                }
            }

            // 欄寬優化
            for (int i = 0; i < properties.Length; i++)
            {
                var prop = properties[i];
                var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
                var header = displayAttr?.Name ?? prop.Name;
                var column = _worksheet.Column(startColumn + i);
                var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                double minWidth = header.Length * 1.2;
                if (type == typeof(int) || type == typeof(long) || type == typeof(short)) minWidth = Math.Max(minWidth, 10);
                else if (type == typeof(decimal) || type == typeof(double) || type == typeof(float)) minWidth = Math.Max(minWidth, 12);
                else if (type == typeof(DateTime)) minWidth = Math.Max(minWidth, 14);
                else if (type == typeof(bool)) minWidth = Math.Max(minWidth, 6);
                else minWidth = Math.Max(minWidth, 15);

                if (column.Width < minWidth)
                    column.Width = minWidth;
            }

            _worksheet.SheetView.FreezeRows(startRow);
            _worksheet.SheetView.ZoomScale = 90;
        }

        /// <summary>
        /// 匯出
        /// </summary>
        /// <returns></returns>
        public MemoryStream ExportMemory()
        {
            if (_workbook == null)
                return null;

            var stream = new MemoryStream();
            _workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
        #endregion 

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
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 標題列
            for (int i = 0; i < properties.Length; i++)
            {
                var prop = properties[i];
                var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
                var header = displayAttr?.Name ?? prop.Name;

                worksheet.Cell(1, i + 1).Value = header;
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#4F81BD");
                worksheet.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
            }

            // 資料列
            for (int row = 0; row < data.Count; row++)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    var prop = properties[col];
                    var value = prop.GetValue(data[row]);
                    var cell = worksheet.Cell(row + 2, col + 1);

                    if (value == null)
                    {
                        cell.Value = string.Empty;
                        continue;
                    }

                    var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    if (type == typeof(int) || type == typeof(long) || type == typeof(short))
                    {
                        cell.Value = Convert.ToInt64(value);
                        cell.Style.NumberFormat.Format = "#,##0";
                    }
                    else if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
                    {
                        cell.Value = Convert.ToDouble(value);
                        cell.Style.NumberFormat.Format = "#,##0.00";
                    }
                    else if (type == typeof(DateTime))
                    {
                        cell.Value = (DateTime)value;
                        cell.Style.DateFormat.Format = "yyyy/mm/dd";
                    }
                    else if (type == typeof(bool))
                    {
                        cell.Value = (bool)value ? "是" : "否";
                    }
                    else
                    {
                        cell.Value = value.ToString();
                    }
                }
            }
            worksheet.Columns().AdjustToContents();

            for (int i = 0; i < properties.Length; i++)
            {
                var prop = properties[i];
                var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
                var header = displayAttr?.Name ?? prop.Name;

                var column = worksheet.Column(i + 1);
                var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                // 根據標題長度設定最小寬度
                double minWidth = header.Length * 1.2;

                // 根據型別設定建議寬度
                if (type == typeof(int) || type == typeof(long) || type == typeof(short))
                {
                    minWidth = Math.Max(minWidth, 10); // 整數欄位
                }
                else if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
                {
                    minWidth = Math.Max(minWidth, 12); // 小數欄位
                }
                else if (type == typeof(DateTime))
                {
                    minWidth = Math.Max(minWidth, 14); // 日期欄位
                }
                else if (type == typeof(bool))
                {
                    minWidth = Math.Max(minWidth, 6); // 是/否
                }
                else
                {
                    minWidth = Math.Max(minWidth, 15); // 一般文字欄位
                }

                // 若目前欄寬小於建議寬度，則補強
                if (column.Width < minWidth)
                    column.Width = minWidth;
            }


            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
        #endregion


    }
}
