using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml;
using EHRIS.Infrastructure.Exporting.Report;
using EHRIS.Tools.Office.Excel;
using GemBox.Spreadsheet;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static EHRIS.Tools.Office.Excel.ExcelGenerater;

namespace EHRIS.Infrastructure.Exporting
{
    public class ReportExporter
    {
        private Type _templateType;
        private IReportTemplateLayout _templateLayout;

        private readonly ExcelGenerater excelGen;
        private int dataRowStartIdx = 1;
        private string _titleName = string.Empty;
        private int _printColumnNum = 0;

        public ReportExporter(string titleName, string sheetName)
        {
            _titleName = titleName;
            excelGen = new ExcelGenerater($"{sheetName}");
        }

        public ReportExporter(string titleName, string sheetName, int columnNum)
        {
            _titleName = titleName;
            _printColumnNum = columnNum;
            excelGen = new ExcelGenerater($"{sheetName}");
        }

        public void ApplyTemplateLayout(IReportTemplateLayout layout)
        {
            _templateLayout = layout;
            _templateType = layout.GetType();

            dataRowStartIdx = layout.ApplyLayout(excelGen, _titleName);
        }

        public void ApplyTemplateLayout(IReportTemplateLayout layout, ReportColumnMap columnMap)
        {
            _templateLayout = layout;
            _templateType = layout.GetType();

            dataRowStartIdx = layout.ApplyLayout(excelGen, columnMap, _titleName);
        }

        public void FillData<T>(List<T> data)
        {
            int rowIdx = dataRowStartIdx;

            //*******資料列開始******* 
            int colIdx = 0;
            if (_printColumnNum > 0)
            {
                excelGen.InsertList(data, rowIdx, colIdx, _printColumnNum, false, 12, HorizontalAlignment.Center, VerticalAlignment.Center);
            }
            else
            {
                excelGen.InsertList(data, rowIdx, colIdx, false, 12, HorizontalAlignment.Center, VerticalAlignment.Center);
            }

            if (_templateLayout?.ShouldPrintFooterSummary == true)
            {
                int summaryRow = rowIdx + data.Count;
                if (_printColumnNum > 0)
                    _templateLayout.PrintColumnNum = _printColumnNum;
                _templateLayout.RenderFooterSummary(excelGen, data, summaryRow);
            }

            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            if (_printColumnNum > 0)
                _templateLayout.SetBorderRange(excelGen, dataRowStartIdx, 0, dataRowStartIdx + data.Count - 1, _printColumnNum - 1);
            else
                _templateLayout.SetBorderRange(excelGen, dataRowStartIdx, 0, dataRowStartIdx + data.Count - 1, props.Length - 1);
        }

        public void FillData<T>(List<T> data, bool isNumericFormat, CellFormatStyle cellFormatStyle)
        {
            int rowIdx = dataRowStartIdx;

            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            int colNum = props.Length;
            if (_printColumnNum > 0)
                colNum = _printColumnNum;

            //*******資料列開始******* 
            int colIdx = 0;
            if (_printColumnNum > 0)
            {
                excelGen.InsertList(data, rowIdx, colIdx, colNum, false, 12, HorizontalAlignment.Center, VerticalAlignment.Center);
            }
            else
            {
                excelGen.InsertList(data, rowIdx, colIdx, false, 12, HorizontalAlignment.Center, VerticalAlignment.Center);
            }

            if (_templateLayout?.ShouldPrintFooterSummary == true)
            {
                if (_printColumnNum > 0)
                    _templateLayout.PrintColumnNum = _printColumnNum;

                int summaryRow = rowIdx + data.Count;
                _templateLayout.RenderFooterSummary(excelGen, data, summaryRow);
            }

             _templateLayout.SetBorderRange(excelGen, dataRowStartIdx, 0, dataRowStartIdx + data.Count - 1, colNum - 1);

            //找出資料欄位
            if (isNumericFormat && data.Count > 0)
            { 
                for (int i = 0; i < colNum; i++)
                {
                    var prop = props[i];
                    if (IsNumericType(prop.PropertyType))
                    {
                        excelGen.ApplyCellFormatStyle(rowIdx, i, (rowIdx + data.Count - 1), i, cellFormatStyle);
                    }
                }
            }

        }

        public void FillData<T>(List<T> data, int rowStartIdx, int colStartIdx)
        {
            int rowIdx = rowStartIdx;
            int colIdx = colStartIdx;

            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            int colNum = props.Length;
            if (_printColumnNum > 0)
                colNum = _printColumnNum;

            //*******資料列開始*******  
            if (_printColumnNum > 0)
            {
                excelGen.InsertList(data, rowIdx, colIdx, colNum, false, 12, HorizontalAlignment.Center, VerticalAlignment.Center);
            }
            else
            {
                excelGen.InsertList(data, rowIdx, colIdx, false, 12, HorizontalAlignment.Center, VerticalAlignment.Center);
            }

            if (_templateLayout?.ShouldPrintFooterSummary == true)
            {
                if (_printColumnNum > 0)
                    _templateLayout.PrintColumnNum = _printColumnNum;

                int summaryRow = rowIdx + data.Count;
                _templateLayout.RenderFooterSummary(excelGen, data, summaryRow);
            }
             
            _templateLayout.SetBorderRange(excelGen, rowIdx, colIdx, rowIdx + data.Count, colIdx + colNum - 1);
        }

        public void FillData<T>(List<T> data, int rowStartIdx, int colStartIdx, bool isNumericFormat, CellFormatStyle cellFormatStyle)
        {
            int rowIdx = rowStartIdx;
            int colIdx = colStartIdx;

            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            int colNum = props.Length;
            if (_printColumnNum > 0)
                colNum = _printColumnNum;

            //*******資料列開始*******  
            if (_printColumnNum > 0)
            {
                excelGen.InsertList(data, rowIdx, colIdx, colNum, false, 12, HorizontalAlignment.Center, VerticalAlignment.Center);
            }
            else
            {
                excelGen.InsertList(data, rowIdx, colIdx, false, 12, HorizontalAlignment.Center, VerticalAlignment.Center);
            }

            if (_templateLayout?.ShouldPrintFooterSummary == true)
            {
                if (_printColumnNum > 0)
                    _templateLayout.PrintColumnNum = _printColumnNum;

                int summaryRow = rowIdx + data.Count;
                _templateLayout.RenderFooterSummary(excelGen, data, summaryRow);
            }
             
            _templateLayout.SetBorderRange(excelGen, rowIdx, colIdx, rowIdx + data.Count, colIdx + colNum - 1);

            //找出資料欄位
            if (isNumericFormat && data.Count > 0)
            {
                for (int i = 0; i < colNum; i++)
                {
                    var prop = props[i];
                    if (IsNumericType(prop.PropertyType))
                    {
                        excelGen.ApplyCellFormatStyle(rowIdx, i, (rowIdx + data.Count - 1), i, cellFormatStyle);
                    }
                }
            }
        }

        public void FillData<T>(List<T> data, ReportColumnMap columnMap, int startRow = 1)
        {
            int rowIdx = startRow;

            //*******資料列開始******* 
            int colIdx = 0;
            if (_printColumnNum > 0)
            {
                excelGen.InsertList(data, rowIdx, colIdx, _printColumnNum, false, 12, HorizontalAlignment.Center, VerticalAlignment.Center);
            }
            else
            {
                excelGen.InsertList(data, rowIdx, colIdx, false, 12, HorizontalAlignment.Center, VerticalAlignment.Center);
            }
        }

        public void FillData<T>(List<T> data, ReportColumnMap columnMap, int startRow, bool isNumericFormat, CellFormatStyle cellFormatStyle)
        {
            int rowIdx = startRow;

            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            int colNum = props.Length;
            if (_printColumnNum > 0)
                colNum = _printColumnNum;

            //*******資料列開始******* 
            int colIdx = 0;
            if (_printColumnNum > 0)
            {
                excelGen.InsertList(data, rowIdx, colIdx, colNum, false, 12, HorizontalAlignment.Center, VerticalAlignment.Center);
            }
            else
            {
                excelGen.InsertList(data, rowIdx, colIdx, false, 12, HorizontalAlignment.Center, VerticalAlignment.Center);
            }

            //找出資料欄位
            if (isNumericFormat && data.Count > 0)
            { 
                for (int i = 0; i < colNum; i++)
                {
                    var prop = props[i];
                    if (IsNumericType(prop.PropertyType))
                    {
                        excelGen.ApplyCellFormatStyle(rowIdx, i, (rowIdx + data.Count - 1), i, cellFormatStyle);
                    }
                }
            }
        }

        //設定資料格式
        public void ApplyCellFormatStyle(int rowStartIdx = 0,
           int colStartIdx = 0,
           int rowEndIdx = 0,
           int colEndIdx = 0,
           CellFormatStyle cellFormatStyle = CellFormatStyle.General)
        {
            excelGen.ApplyCellFormatStyle(rowStartIdx, colStartIdx, rowEndIdx, colEndIdx, cellFormatStyle);
        }

        public (MemoryStream stream, string contentType, string fileExtension) Export(ExportFormat exportFormat)
        {
            var (stream, contentType, fileExtension) = excelGen.Export(exportFormat);

            return (stream, contentType, fileExtension);
        }

        public (byte[] stream, string contentType, string fileExtension) ExportToBytes(ExportFormat exportFormat)
        {
            var (stream, contentType, fileExtension) = excelGen.Export(exportFormat);

            return (stream.ToArray(), contentType, fileExtension);
        }

        private bool IsNumericType(Type type)
        {
            return type == typeof(int) || type == typeof(double) || type == typeof(decimal) || type == typeof(float) || type == typeof(long);
        }
    }
}
