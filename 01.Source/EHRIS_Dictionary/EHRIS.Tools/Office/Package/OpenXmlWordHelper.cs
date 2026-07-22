using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace EHRIS.Tools.Office.Package
{
    public class OpenXmlWordHelper
    {
        public enum WordHorizontalAlignment
        {
            Left,
            Center,
            Right
        }

        public class WordTableColumn
        {
            public string Header { get; set; } = string.Empty;
            public int WidthDxa { get; set; } = 1500;
        }

        public class WordTableCellSpec
        {
            public string Text { get; set; } = string.Empty;
            public int ColSpan { get; set; } = 1;
            public bool Bold { get; set; } = false;
            public WordHorizontalAlignment Alignment { get; set; } = WordHorizontalAlignment.Center;
        }

        private readonly MemoryStream _stream;
        private readonly WordprocessingDocument _document;
        private readonly Body _body;

        public OpenXmlWordHelper()
        {
            _stream = new MemoryStream();
            _document = WordprocessingDocument.Create(_stream, WordprocessingDocumentType.Document);
            var mainPart = _document.AddMainDocumentPart();
            mainPart.Document = new Document();
            _body = mainPart.Document.AppendChild(new Body());
        }

        public void AddHeading(string text, int level = 1)
        {
            var paragraph = new Paragraph();
            var paragraphProperties = new ParagraphProperties(
                new ParagraphStyleId { Val = $"Heading{level}" }
            );
            paragraph.Append(paragraphProperties);

            var run = new Run(new Text(text));
            run.PrependChild(new RunProperties(new Bold(), new FontSize { Val = (24 - (level * 2)).ToString() }));
            paragraph.Append(run);

            _body.Append(paragraph);
        }

        public void AddParagraph(string text, bool bold = false, int fontSize = 10, WordHorizontalAlignment alignment = WordHorizontalAlignment.Left)
        {
            var paragraph = new Paragraph();
            var paragraphProperties = new ParagraphProperties(
                new Justification { Val = ToJustificationValue(alignment) }
            );
            paragraph.Append(paragraphProperties);

            var run = new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
            var runProps = new RunProperties(new FontSize { Val = (fontSize * 2).ToString() });
            if (bold) runProps.Append(new Bold());
            run.PrependChild(runProps);
            paragraph.Append(run);

            _body.Append(paragraph);
        }

        public void AddTable(List<WordTableColumn> columns, List<List<WordTableCellSpec>> rows)
        {
            var table = new Table();

            var tableProperties = new TableProperties(
                new TableBorders(
                    new TopBorder { Val = BorderValues.Single, Size = 4 },
                    new BottomBorder { Val = BorderValues.Single, Size = 4 },
                    new LeftBorder { Val = BorderValues.Single, Size = 4 },
                    new RightBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 }
                ),
                new TableWidth { Type = TableWidthUnitValues.Auto }
            );
            table.AppendChild(tableProperties);

            var grid = new TableGrid();
            foreach (var col in columns)
            {
                grid.Append(new GridColumn { Width = col.WidthDxa.ToString() });
            }
            table.Append(grid);

            foreach (var rowCells in rows)
            {
                var tableRow = new TableRow();
                foreach (var cellSpec in rowCells)
                {
                    var tableCell = new TableCell();
                    var cellProperties = new TableCellProperties();
                    if (cellSpec.ColSpan > 1)
                    {
                        cellProperties.Append(new GridSpan { Val = cellSpec.ColSpan });
                    }
                    tableCell.Append(cellProperties);

                    var paragraph = new Paragraph(
                        new ParagraphProperties(new Justification { Val = ToJustificationValue(cellSpec.Alignment) })
                    );
                    var run = new Run(new Text(cellSpec.Text ?? "") { Space = SpaceProcessingModeValues.Preserve });
                    var runProps = new RunProperties();
                    if (cellSpec.Bold) runProps.Append(new Bold());
                    run.PrependChild(runProps);
                    paragraph.Append(run);
                    tableCell.Append(paragraph);

                    tableRow.Append(tableCell);
                }
                table.Append(tableRow);
            }

            _body.Append(table);
        }

        public void AddPageBreak()
        {
            var paragraph = new Paragraph(new Run(new Break { Type = BreakValues.Page }));
            _body.Append(paragraph);
        }

        private static JustificationValues ToJustificationValue(WordHorizontalAlignment alignment)
        {
            return alignment switch
            {
                WordHorizontalAlignment.Center => JustificationValues.Center,
                WordHorizontalAlignment.Right => JustificationValues.Right,
                _ => JustificationValues.Left
            };
        }

        public MemoryStream Export()
        {
            _document.MainDocumentPart!.Document.Save();
            _document.Dispose();
            _stream.Position = 0;
            return _stream;
        }
    }
}