using EHRIS.Tools.Office.Package;

namespace EHRIS.Tools.Office.Word
{
    public class WordGenerater
    {
        private readonly OpenXmlWordHelper _document;

        public WordGenerater()
        {
            _document = new OpenXmlWordHelper();
        }

        public void AddHeading(string text, int level = 1)
        {
            _document.AddHeading(text, level);
        }

        public void AddParagraph(string text, bool bold = false, int fontSize = 10,
            OpenXmlWordHelper.WordHorizontalAlignment alignment = OpenXmlWordHelper.WordHorizontalAlignment.Left)
        {
            _document.AddParagraph(text, bold, fontSize, alignment);
        }

        public void AddTable(List<OpenXmlWordHelper.WordTableColumn> columns, List<List<OpenXmlWordHelper.WordTableCellSpec>> rows)
        {
            _document.AddTable(columns, rows);
        }

        public void AddPageBreak()
        {
            _document.AddPageBreak();
        }

        public (MemoryStream stream, string contentType, string fileExtension) Export()
        {
            var stream = _document.Export();
            return (stream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "docx");
        }
    }
}