namespace EHRIS.Web.Shared.Models
{
    /// <summary>
    /// 單一欄位定義（對應 DataTables 的 &lt;th&gt;）
    /// </summary>
    public class TableColDef
    {
        /// <summary>欄位標題文字</summary>
        public string Label { get; set; }

        /// <summary>th 的 CSS class（例如 dt-col-mid、dt-col-action）</summary>
        public string CssClass { get; set; } = string.Empty;

        /// <summary>
        /// 選填，欄位寬度，例如 "15%"、"120px"。
        /// 對應舊寫法的 width="15%"，會轉為 style="width:15%"（符合 HTML5 標準）
        /// </summary>
        public string Width { get; set; } = string.Empty;

        public TableColDef(string label, string cssClass = "", string width = "")
        {
            Label = label;
            CssClass = cssClass;
            Width = width;
        }
    }

    /// <summary>
    /// 共用 DataTable 卡片的 Partial View Model
    /// </summary>
    public class CommonTableModel
    {
        /// <summary>table 的 HTML id，對應 JS 的 createEhrisTable(tableId, ...)</summary>
        public string TableId { get; set; }

        /// <summary>欄位標題定義列表</summary>
        public IEnumerable<TableColDef> Columns { get; set; } = [];

        /// <summary>
        /// 表格最小寬度（例如 "700px"、"900px"），避免手機版欄位被過度壓縮。
        /// 當表格寬度超過容器時，table-responsive 會自動產生水平捲軸。
        /// 預設 "1024px"，設為空字串可關閉此功能。
        /// </summary>
        public string MinWidth { get; set; } = "1024px";
    }
}
