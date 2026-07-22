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

        private const string EastAsiaFont = "微軟正黑體";
        private const string LatinFont = "Calibri";

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

            var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
            stylesPart.Styles = new Styles(
                new DocDefaults(
                    new RunPropertiesDefault(
                        new RunProperties(
                            new RunFonts { Ascii = LatinFont, HighAnsi = LatinFont, EastAsia = EastAsiaFont },
                            new FontSize { Val = "20" }
                        )
                    )
                )
            );
            stylesPart.Styles.Save();

            _body.Append(new SectionProperties(
                new PageMargin { Top = 720, Bottom = 720, Left = 720, Right = 720 }
            ));
        }

        public void AddHeading(string text, int level = 1)
        {
            var paragraph = new Paragraph();
            var paragraphProperties = new ParagraphProperties(
                new SpacingBetweenLines { Before = "240", After = "120" },
                new KeepNext()
            );
            paragraph.Append(paragraphProperties);

            var run = new Run(new Text(text));
            var runProps = new RunProperties(
                new RunFonts { Ascii = LatinFont, HighAnsi = LatinFont, EastAsia = EastAsiaFont },
                new Bold(),
                new FontSize { Val = (32 - (level * 4)).ToString() }
            );
            run.PrependChild(runProps);
            paragraph.Append(run);

            _body.Append(paragraph);
        }

        public void AddParagraph(string text, bool bold = false, int fontSize = 10, WordHorizontalAlignment alignment = WordHorizontalAlignment.Left)
        {
            var paragraph = new Paragraph();
            var paragraphProperties = new ParagraphProperties(
                new Justification { Val = ToJustificationValue(alignment) },
                new SpacingBetweenLines { Before = "0", After = "120" }
            );
            paragraph.Append(paragraphProperties);

            var run = new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
            var runProps = new RunProperties(
                new RunFonts { Ascii = LatinFont, HighAnsi = LatinFont, EastAsia = EastAsiaFont },
                new FontSize { Val = (fontSize * 2).ToString() }
            );
            if (bold) runProps.Append(new Bold());
            run.PrependChild(runProps);
            paragraph.Append(run);

            _body.Append(paragraph);
        }

        public void AddTable(List<WordTableColumn> columns, List<List<WordTableCellSpec>> rows, int headerRowCount = 2)
        {
            var table = new Table();
            int tableWidthTotal = columns.Sum(c => c.WidthDxa);

            var tableProperties = new TableProperties(
                new TableBorders(
                    new TopBorder { Val = BorderValues.Single, Size = 4 },
                    new BottomBorder { Val = BorderValues.Single, Size = 4 },
                    new LeftBorder { Val = BorderValues.Single, Size = 4 },
                    new RightBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 }
                ),
                new TableWidth { Type = TableWidthUnitValues.Dxa, Width = tableWidthTotal.ToString() },
                new TableLayout { Type = TableLayoutValues.Fixed },
                new TableCellMarginDefault(
                    new TopMargin { Type = TableWidthUnitValues.Dxa, Width = "40" },
                    new BottomMargin { Type = TableWidthUnitValues.Dxa, Width = "40" },
                    new TableCellLeftMargin { Type = TableWidthValues.Dxa, Width = 80 },
                    new TableCellRightMargin { Type = TableWidthValues.Dxa, Width = 80 }
                )
            );
            table.AppendChild(tableProperties);

            var grid = new TableGrid();
            foreach (var col in columns)
            {
                grid.Append(new GridColumn { Width = col.WidthDxa.ToString() });
            }
            table.Append(grid);

            for (int r = 0; r < rows.Count; r++)
            {
                var rowCells = rows[r];
                var tableRow = new TableRow();

                if (r < headerRowCount)
                {
                    tableRow.Append(new TableRowProperties(new TableHeader()));
                }

                int colIdx = 0;
                foreach (var cellSpec in rowCells)
                {
                    int span = cellSpec.ColSpan;
                    int cellWidth = 0;
                    for (int i = 0; i < span && (colIdx + i) < columns.Count; i++)
                    {
                        cellWidth += columns[colIdx + i].WidthDxa;
                    }
                    colIdx += span;

                    var tableCell = new TableCell();
                    var cellProperties = new TableCellProperties(
                        new TableCellWidth { Type = TableWidthUnitValues.Dxa, Width = cellWidth.ToString() },
                        new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center }
                    );
                    if (cellSpec.ColSpan > 1)
                    {
                        cellProperties.Append(new GridSpan { Val = cellSpec.ColSpan });
                    }
                    tableCell.Append(cellProperties);

                    var paragraph = new Paragraph(
                        new ParagraphProperties(new Justification { Val = ToJustificationValue(cellSpec.Alignment) })
                    );
                    var run = new Run(new Text(cellSpec.Text ?? "") { Space = SpaceProcessingModeValues.Preserve });
                    var runProps = new RunProperties(
                        new RunFonts { Ascii = LatinFont, HighAnsi = LatinFont, EastAsia = EastAsiaFont },
                        new FontSize { Val = "20" }
                    );
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