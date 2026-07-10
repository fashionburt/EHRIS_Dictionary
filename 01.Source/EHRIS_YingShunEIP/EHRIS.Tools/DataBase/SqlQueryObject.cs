using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Tools.DataBase
{
    /// <summary>
    /// var sqlObj = new SqlQueryObject
    /// {
    ///     Sql = "usp_UpdatePeopleStatus", // Stored Procedure 名稱
    ///     CommandType = CommandType.StoredProcedure,
    ///     CommandTimeout = 60
    /// };

    ///     sqlObj.AddParameter(
    ///     new SqlParameter("@EmployeeId", 123),
    ///     new SqlParameter("@Status", "Active")
    /// );

    ///     using var conn = new SqlConnection(connectionString);
    ///     await conn.OpenAsync();

    ///     using var tran = conn.BeginTransaction();

    /// try
    /// {
    ///     var cmd = sqlObj.ToSqlCommand(conn, tran);
    ///     await cmd.ExecuteNonQueryAsync();

    ///     tran.Commit(); // 成功就提交
    /// }
    /// catch
    /// {
    ///     tran.Rollback(); // 發生錯誤就回滾
    ///     throw;
    /// }
    /// </summary>
    public class SqlQueryObject
    {
        public string Sql { get; set; } = string.Empty;
        public List<SqlParameter> Para { get; set; } = new();
        public int CommandTimeout { get; set; } = 30;
        public CommandType CommandType { get; set; } = CommandType.Text; // 可切換為 StoredProcedure

        /// <summary>
        /// 新增參數
        /// </summary>
        /// <param name="parameters"></param>
        public void AddParameter(params SqlParameter[] parameters)
        {
            var existing = new HashSet<string>(Para.Select(p => p.ParameterName));
            foreach (var p in parameters)
            {
                if (!existing.Contains(p.ParameterName))
                {
                    Para.Add(p);
                    existing.Add(p.ParameterName);
                }
            }
        }

        /// <summary>
        /// 轉成SQL Command
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public SqlCommand ToSqlCommand(SqlConnection connection, SqlTransaction transaction = null)
        {
            var cmd = new SqlCommand(Sql, connection, transaction)
            {
                CommandType = CommandType,
                CommandTimeout = CommandTimeout
            };

            if (Para.Count > 0)
                cmd.Parameters.AddRange(Para.ToArray());

            return cmd;
        }

        //清除所有SQL的指令
        public void Clear()
        {
            Sql = string.Empty;
            Para.Clear();
            CommandType = CommandType.Text;
        }
    }

}
