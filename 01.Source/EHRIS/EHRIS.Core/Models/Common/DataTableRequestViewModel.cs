namespace EHRIS.Core.Models.Common;

public class DataTableRequest
{
    public int draw { get; set; }
    public int start { get; set; }
    public int length { get; set; }
    public string search { get; set; }
    public string ServerIp { get; set; } = string.Empty;

    public List<DataTableRequestColumn> columns { get; set; }
    public List<DataTableRequestOrder> orderby { get; set; }
    public ExtraSearch extraSearch { get; set; }
    
}
public class DataTableSearch
{ 
    public string value { get; set; } 
    public string regex { get; set; } 
}

public class DataTableRequestColumn
{
    public string data { get; set; }
    public string name { get; set; }
    public bool searchable { get; set; }
    public bool orderable { get; set; }
}

public class DataTableRequestOrder
{
    public int column { get; set; }  // 對應 columns 的索引
    public string dir { get; set; }  // "asc" 或 "desc"
}
public class DataTableResponse<T>
{
    public int draw { get; set; }
    public int recordsTotal { get; set; }
    public int recordsFiltered { get; set; }
    public List<T> data { get; set; }
}

public class ExtraSearch
{
    public string searchValue { get; set; }

    // 被勾選要搜尋的欄位 index (例如 [0, 1, 3])
    public List<int> columnIndexes { get; set; }
}
