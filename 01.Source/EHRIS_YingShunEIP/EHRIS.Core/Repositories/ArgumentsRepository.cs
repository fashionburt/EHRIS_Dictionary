using EHRIS.Core.DbContext;
using EHRIS.Core.Models.Common;
using EHRIS.Tools.DataBase;
using Microsoft.Data.SqlClient;

namespace EHRIS.Core.Repositories
{
    public class ArgumentsRepository : BaseRepository, IArgumentsRepository
    {
        public ArgumentsRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<string> GetArgumentAsync(string argVariable, int depNo = 0)
        {
            var result = await GetArgumentInternalAsync(argVariable, depNo);
            return result.Value;
        }

        public async Task<List<string>> GetArgumentListAsync(string argVariable, int depNo = 0)
        {
            var result = await GetArgumentInternalAsync(argVariable, depNo);

            // 非 SPLITTEXT 來源，直接回傳單一值
            if (!string.Equals(result.Source, "SPLITTEXT", StringComparison.OrdinalIgnoreCase))
                return new List<string> { result.Value };

            var value = result.Value;
            if (string.IsNullOrEmpty(value))
                return new List<string>();

            // 空白分隔字元 = 一個字元拆一個
            if (string.IsNullOrEmpty(result.SplitChar))
                return value.Select(c => c.ToString()).ToList();

            return value.Split(result.SplitChar, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private async Task<ArgumentResult> GetArgumentInternalAsync(string argVariable, int depNo)
        {
            SqlQueryObject sqlObj = new SqlQueryObject();

            var sql = @"
                SELECT
                    a.arg_value,
                    a.arg_defaultvalue,
                    a.arg_source,
                    a.arg_splitchar,
                    ad.agd_value,
                    sch.ags_value
                FROM arguments a
                LEFT JOIN arguments_dept ad
                    ON a.arg_variable = ad.arg_variable
                    AND ad.dep_no = @dep_no
                    AND ad.agd_status = 1
                LEFT JOIN arguments_schedule sch
                    ON a.arg_variable = sch.arg_variable
                    AND sch.dep_no = @dep_no
                    AND sch.ags_status = 1
                    AND GETDATE() BETWEEN sch.ags_starttime AND sch.ags_endtime
                WHERE a.arg_variable = @arg_variable
            ";

            sqlObj.Sql = sql;
            sqlObj.AddParameter(
                new SqlParameter("@dep_no", depNo),
                new SqlParameter("@arg_variable", argVariable)
            );

            var list = await SQLQueryAsync<ArgumentResult>(
                sqlObj,
                reader =>
                {
                    var agsValue = reader["ags_value"]?.ToString() ?? "";
                    var agdValue = reader["agd_value"]?.ToString() ?? "";
                    var argValue = reader["arg_value"]?.ToString() ?? "";
                    var argDefault = reader["arg_defaultvalue"]?.ToString() ?? "";

                    // 優先序：排程 → 部門 → 全域 → 預設值
                    var finalValue = !string.IsNullOrEmpty(agsValue) ? agsValue
                                   : !string.IsNullOrEmpty(agdValue) ? agdValue
                                   : !string.IsNullOrEmpty(argValue) ? argValue
                                   : argDefault;

                    return new ArgumentResult
                    {
                        Value = finalValue,
                        Source = reader["arg_source"]?.ToString() ?? "",
                        SplitChar = reader["arg_splitchar"]?.ToString() ?? ""
                    };
                }
            );

            return list.Count > 0 ? list[0] : new ArgumentResult();
        }

        private class ArgumentResult
        {
            public string Value { get; set; } = "";
            public string Source { get; set; } = "";
            public string SplitChar { get; set; } = "";
        }
    }
}
