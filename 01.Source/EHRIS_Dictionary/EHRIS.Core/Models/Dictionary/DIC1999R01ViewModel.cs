using EHRIS.Core.Models.Common;

namespace EHRIS.Core.Models.Dictionary
{
    public class DIC1999R01ViewModel
    {
        public string TableName { get; set; } = string.Empty;
        public string? OriginalTableName { get; set; }
        public int? MenuId { get; set; }
        public int? SheetId { get; set; }
        public string? SheetDesc { get; set; }
        public string? DbKey { get; set; }
        public string? EditAction { get; set; }
        public string? LogAction { get; set; }
        public string? DeleteAction { get; set; }
    }

    public class DIC1999R01Request : DataTableRequest
    {
        public string? DbKey { get; set; }
    }

    public class DIC1999R01CreateViewModel
    {
        public string DbKey { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string PkName { get; set; } = "Id";
        public string PkType { get; set; } = "int";
        public bool PkIdentity { get; set; } = true;
        public string? PkDescription { get; set; }
    }
}