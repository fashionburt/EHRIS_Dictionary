using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Event;
using EHRIS.Tools.DataBase;
using EHRIS.Tools.Formatter;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Logging;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Core.Repositories
{
    public class BaseRepository : IBaseRepository
    {
        protected readonly ApplicationDbContext _context;
        protected readonly IDataLogger _operateContext;

        protected readonly IUnitOfWork _unit;
        private readonly bool _isExternalUnit;


        private SqlConnection _connection;

        private readonly string _connectionString; 
        private readonly int _commandTimeout;

        private List<PersonalEvent> auditEntries = new List<PersonalEvent>();
        private List<(EntityEntry entry, PersonalEvent auditLog)> auditPairs = new List<(EntityEntry entry, PersonalEvent auditLog)>();
        private int effectNum = 0;
         
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public BaseRepository( ApplicationDbContext context) : this( context, new UnitOfWork(context))  // 單一交易模式
        {
            _isExternalUnit = false;
             
            _connectionString = _context.Database.GetDbConnection().ConnectionString;
            _commandTimeout = _context.Database.GetDbConnection().ConnectionTimeout;

        }

        /// <summary>
        /// - 外部注入的 UnitOfWork
        /// </summary>
        /// <param name="context"></param>
        /// <param name="unit"></param>
        public BaseRepository(ApplicationDbContext context , IUnitOfWork unit) // 共同交易模式 
        { 
            _context = context;  

            _unit = unit;
            _isExternalUnit = true;
             
            _commandTimeout = _context.Database.GetDbConnection().ConnectionTimeout; 
        }

        private SqlConnection CreateConnection()
        {
            //var connStr = _context.Database.GetDbConnection().ConnectionString;
            return new SqlConnection(_connectionString); 
        }

        /// <summary>
        /// 單獨回傳一個物件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlObj"></param>
        /// <param name="mapFunc"></param>
        /// <returns></returns>
        public async Task<T> QuerySingleAsync<T>(SqlQueryObject sqlObj, Func<DataRow, T> mapFunc)
        {
            DataTable dt = await SQLQueryDTAsync(sqlObj);
            if (dt.Rows.Count > 0)
            {
                return mapFunc(dt.Rows[0]);
            }
            return default;
        }


        /// <summary>
        /// 適合報表、動態欄位、未知結構查詢
        /// foreach (var row in result)
        /// {
        ///   var PeoName = row[0].ToString();
        ///   var Age = Convert.ToInt32(row[1]);
        /// }
        /// </summary>
        /// <param name="sqlObj"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<List<object[]>> SQLQueryRawAsync(SqlQueryObject sqlObj)
        {
            var result = new List<object[]>();

            await using var connection = CreateConnection();
            await connection.OpenAsync();

            await using var command = sqlObj.ToSqlCommand(connection);
            command.CommandTimeout = _commandTimeout;

            try
            {
                using var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
                while (await reader.ReadAsync())
                {
                    var row = new object[reader.FieldCount];
                    reader.GetValues(row);
                    result.Add(row);
                }
            }
            catch (Exception ex)
            {
                // 可記錄 log 或拋出
                throw new InvalidOperationException("查詢失敗", ex);
            }
            finally
            {
                // 未來可加上 log 或清理其他資源
            }

            return result;
        }

        public async Task<DataTable> SQLQueryDTAsync(SqlQueryObject sqlObj)
        {
            var dataTable = new DataTable();

            await using var connection = CreateConnection();
            await connection.OpenAsync();

            await using var command = sqlObj.ToSqlCommand(connection);
            command.CommandTimeout = _commandTimeout;

            try
            {
                using var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
                dataTable.Load(reader);   // 直接把 reader 填入 DataTable
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("查詢失敗: " + ex.Message, ex);
            }

            return dataTable;
        }

        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlObj"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<List<T>> SQLQueryAsync<T>(SqlQueryObject sqlObj, Func<SqlDataReader, T> mapper)
        {
            var result = new List<T>();

            await using var connection = CreateConnection();
            await connection.OpenAsync();

            await using var command = sqlObj.ToSqlCommand(connection);
            command.CommandTimeout = _commandTimeout;

            try
            {
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(mapper(reader));
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("查詢失敗", ex);
            }
            finally
            {
                // 未來可加上 log 或清理其他資源
            }

            return result;
        }


        public async Task<int> ExecuteNonQueryAsync(SqlQueryObject sqlObj)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();

            await using var command = sqlObj.ToSqlCommand(connection);
            command.CommandTimeout = _commandTimeout;

            return await command.ExecuteNonQueryAsync();
        }


        /// <summary>
        /// LINQ 更新儲存
        /// 呼叫動態存入資料庫並記得異動欄位到personalEvents 
        /// </summary>
        /// <param name="opeatorUID">操作人</param>
        /// <param name="effectUID">和誰有關的人</param>
        /// <returns></returns>
        public async Task<int> SaveChangesAsync(IDataLogger operateContext)
        { 
            // Step 1: 收集異動
            var auditEntries = new List<PersonalEvent>();
            var auditPairs = new List<(EntityEntry entry, PersonalEvent auditLog)>();

            foreach (var entry in _context.ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added 
                    || entry.State == EntityState.Modified 
                    || entry.State == EntityState.Deleted)
                {
                    var auditLog = await BuildChangeEventLog(entry, operateContext);
                    if (auditLog != null)
                    {
                        auditEntries.Add(auditLog);
                        auditPairs.Add((entry, auditLog));
                    }
                }
            }

            int effectNum = 0;
            bool success = false;

            // Step 2: 存主資料
            try
            {
                effectNum = await _unit.CommitAsync();
                success = true;
            }
            catch (Exception ex)
            {
                //LogHelper.Error($"資料存檔失敗: {ex.Message}\n{ex.StackTrace}");
                throw;
            }

            // Step 3: 回填主鍵與新值
            if (success)
            {
                foreach (var pair in auditPairs)
                {
                    var entry = pair.entry;
                    var auditLog = pair.auditLog;

                    // 回填主鍵
                    string realKey = TryGetEntityKey(_context, entry);
                    if (!string.IsNullOrEmpty(realKey) && auditLog.pev_pk.StartsWith("TEMP-"))
                    {
                        auditLog.pev_pk = realKey;
                    }
                    // 呼叫重構方法
                    FillAuditDetails(entry, auditLog);

                }
            }

            // Step 4: 存 AuditLog
            if (success && auditEntries.Any())
            {
                try
                {
                    _context._auditDbContext.Personalevents.AddRange(auditEntries);
                    await _context._auditDbContext.SaveChangesAsync();
                    auditEntries.Clear();
                }
                catch (Exception ex)
                {
                    //LogHelper.Error($"PersonEventLog 寫入失敗: {ex.Message}\n{ex.StackTrace}");
                }
            }

            return effectNum;
        }

        private static void FillAuditDetails(EntityEntry entry, PersonalEvent auditLog)
        {
            foreach (var detail in auditLog.Details)
            {
                // EF Core 建議直接用 entry.Property(name)
                var property = entry.Property(detail.pvt_column);
                if (property != null)
                {
                    detail.pvt_newvalue = FormatValue(property.CurrentValue) ?? "";
                }
            }

            // 更新 JSON 內容
            var detailsForJson = auditLog.Details
                .Select(d => new ChangeColumn
                {
                    ColumnDesc = d.pvt_coldesc,
                    Column = d.pvt_column,
                    OriValue = d.pvt_orivalue,
                    NewValue = d.pvt_newvalue
                }).ToList();

            auditLog.pev_content = Newtonsoft.Json.JsonConvert.SerializeObject(detailsForJson);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="affectedUID"></param>
        /// <param name="operatorID"></param>
        /// <param name="sfuNo"></param>
        /// <param name="sfuName"></param>
        /// <returns></returns>
        private async Task<PersonalEvent> BuildChangeEventLog(EntityEntry entry, IDataLogger operateContext)
        {
            try
            {
                En_OperatorMode operMode = En_OperatorMode.新增;
                if (entry.State == EntityState.Modified) 
                    operMode = En_OperatorMode.更新;
                else if (entry.State == EntityState.Deleted) 
                    operMode = En_OperatorMode.刪除;

                string tableName = GetTableName(_context, entry.Entity);
                string entityKey = TryGetEntityKey(_context, entry);

                if (string.IsNullOrEmpty(entityKey) && entry.State == EntityState.Added)
                {
                    entityKey = $"TEMP-{Guid.NewGuid()}";
                }

                // 取得被影響人資訊
                //string depName = "", peoName = "", proName = "";
                LoginUserInfo userInfo = new LoginUserInfo();
                userInfo.dep_name = "";
                userInfo.peo_name = "";
                userInfo.pro_name = "";
                if (operateContext.ToPeoUID != 0)
                {
                    //SqlQueryObject sqlObj = new SqlQueryObject();
                    //sqlObj.Sql = $@"
                    //        SELECT baseperson.bas_name, departments.dep_name, profess.pro_name
                    //        FROM people
                    //        INNER JOIN baseperson ON baseperson.bas_id = people.bas_id
                    //        INNER JOIN departments ON people.dep_no = departments.dep_no
                    //        INNER JOIN profess ON people.pro_no = profess.pro_no
                    //        WHERE people.peo_uid = @uid";   // 建議用參數化避免 SQL Injection
                    //sqlObj.AddParameter(new SqlParameter("@uid", operateContext.ToPeoUID));
                    //// 呼叫非同步方法
                    //DataTable dt = await SQLQueryDTAsync(sqlObj);
                    //if (dt.Rows.Count > 0)
                    //{
                    //    depName = dt.Rows[0]["dep_name"].ToString();
                    //    peoName = dt.Rows[0]["bas_name"].ToString();
                    //    proName = dt.Rows[0]["pro_name"].ToString();
                    //} 
                    userInfo  = await GetUserInfo(operateContext.ToPeoUID);
                }
                  
                var eventLog = new PersonalEvent
                {
                    pev_execuid = operateContext.ExecUID,
                    pev_execdate = DateTime.Now.Date,
                    pev_exectime = DateTime.Now,
                    pev_execsfuno = operateContext.ExecSfuNo,
                    pev_execprocname = operateContext.ExecProName,
                    //pev_execsfuversion = GetCallerVersion(),
                    pev_exectype = 10,
                    pev_execipaddress = operateContext.ExecFromIP,
                    pev_uid = operateContext.ToPeoUID,
                    pev_eventtype = ((int)operMode).ToString(),
                    pev_table = tableName,
                    pev_pk = entityKey,
                    pev_depname = userInfo.dep_name,
                    pev_peoname = userInfo.peo_name,
                    pev_proname = userInfo.pro_name,
                    Details = new List<PersonalEventsDetail>()
                };

                IEnumerable<string> propertyNames = entry.State == EntityState.Added
                    ? entry.CurrentValues.Properties.Select(p => p.Name)
                    : entry.OriginalValues.Properties.Select(p => p.Name);

                foreach (var name in propertyNames)
                {
                    object original = "";
                    object current = "";

                    if (entry.State == EntityState.Added)
                    {
                        current = entry.CurrentValues[name] ?? "";
                    }
                    else if (entry.State == EntityState.Deleted)
                    {
                        original = entry.OriginalValues[name] ?? "";
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        original = entry.OriginalValues[name] ?? "";
                        current = entry.CurrentValues[name] ?? "";
                    }

                    string oriValue = FormatValue(original);
                    string curValue = FormatValue(current);

                    if (original is decimal od && current is decimal cd)
                    {
                        if (od == cd) continue;
                    }
                    else
                    {
                        if (oriValue == curValue) continue;
                    }

                    var detail = new  PersonalEventsDetail
                    {
                        pvt_execdate = DateTime.Now.Date,
                        pvt_column = name,
                        pvt_coldesc = _context._fieldMappingService.GetDisplayName(tableName, name),
                        pvt_orivalue = oriValue ?? "",
                        pvt_newvalue = curValue ?? ""
                    };

                    eventLog.Details.Add(detail);
                }

                eventLog.pev_content = Newtonsoft.Json.JsonConvert.SerializeObject(
                    eventLog.Details.Select(d => new ChangeColumn
                    {
                        ColumnDesc = d.pvt_coldesc,
                        Column = d.pvt_column,
                        OriValue = d.pvt_orivalue,
                        NewValue = d.pvt_newvalue
                    }).ToList()
                );

                return eventLog;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 取得資料表資訊
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static string GetTableName(ApplicationDbContext context, object entity)
        {
            // 去掉 Proxy，取得原始型別
            var entityType = entity.GetType();
            if (entityType.Namespace != null && entityType.Namespace.Contains("Castle.Proxies"))
            {
                entityType = entityType.BaseType;
            }

            // 從 Model 找對應的 EntityType
            var efEntityType = context.Model.FindEntityType(entityType);
            if (efEntityType == null)
            {
                return entityType.Name; // 找不到就退回類別名稱
            }

            // EF Core 提供 TableName 與 Schema
            var tableName = efEntityType.GetTableName();
            var schema = efEntityType.GetSchema();

            return string.IsNullOrEmpty(schema) ? tableName : $"{schema}.{tableName}";
        }

        /// <summary>
        /// 取得主鍵
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        private static string TryGetEntityKey(ApplicationDbContext ctx, EntityEntry entry)
        {
            try
            {
                var entityType = entry.Metadata; // IEntityType
                var primaryKey = entityType.FindPrimaryKey();

                if (primaryKey == null || primaryKey.Properties.Count == 0)
                    return null;

                var keyPairs = primaryKey.Properties
                    .Select(p =>
                    {
                        var value = entry.Property(p.Name).CurrentValue;
                        return $"{p.Name}:{value ?? "null"}";
                    });

                return string.Join(",", keyPairs);
            }
            catch
            {
                // 後備策略：用 [Key] 屬性反射
                var keyProp = entry.Entity.GetType()
                    .GetProperties()
                    .FirstOrDefault(p => Attribute.IsDefined(p, typeof(KeyAttribute)));

                return keyProp != null
                    ? $"{keyProp.Name}:{keyProp.GetValue(entry.Entity)}"
                    : null;
            }
        }

        private static string FormatValue(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            switch (value)
            {
                // DateTime (包含 Nullable<DateTime>)
                case DateTime dt:
                    return dt.ToString("yyyy-MM-dd HH:mm:ss");

                // Decimal → 保留原始 scale
                case decimal dec:
                    var bits = decimal.GetBits(dec);
                    byte scale = (byte)((bits[3] >> 16) & 0x7F); // 取出小數位數
                    return dec.ToString($"F{scale}", System.Globalization.CultureInfo.InvariantCulture);

                // Double/float → 預設顯示 (InvariantCulture)
                case double dbl:
                    return dbl.ToString(System.Globalization.CultureInfo.InvariantCulture);

                case float flt:
                    return flt.ToString(System.Globalization.CultureInfo.InvariantCulture);

                // Boolean → 顯示 True/False
                case bool b:
                    return b ? "True" : "False";

                // Guid → 標準格式
                case Guid guid:
                    return guid.ToString();

                // 其他型別 → 預設 ToString()
                default:
                    return value.ToString();
            }
        }

        // 
        public Task BeginTransactionAsync() => _unit.BeginTransactionAsync();
        public Task CommitTransactionAsync() => _unit.CommitTransactionAsync();
        public Task RollbackTransactionAsync() => _unit.RollbackTransactionAsync();

        /// <summary>
        /// 取得使用者基本資料
        /// </summary>
        /// <param name="peo_uid"></param>
        /// <returns></returns>
        public async Task<LoginUserInfo> GetUserInfo(int peo_uid)
        {
            SqlQueryObject sqlObj = new SqlQueryObject();
            sqlObj.Sql = @"
                        SELECT people.peo_uid, accounts.acc_login, baseperson.bas_name, departments.dep_name, profess.pro_name
                        FROM people
                        INNER JOIN baseperson ON baseperson.bas_id = people.bas_id
                        INNER JOIN departments ON people.dep_no = departments.dep_no
                        INNER JOIN profess ON people.pro_no = profess.pro_no
                        INNER JOIN accounts ON people.peo_uid = accounts.peo_uid
                        WHERE people.peo_jobtype='1' AND accounts.acc_status ='1'
                         AND people.peo_uid = @uid ";  
            sqlObj.AddParameter(new SqlParameter("@uid", peo_uid));
             
            LoginUserInfo userInfo = await QuerySingleAsync(sqlObj, row => new LoginUserInfo
            {
                acc_login = row["acc_login"].ToString(),
                peo_uid = row["peo_uid"].ToString().ToInt(),
                peo_name = row["bas_name"].ToString(),
                dep_name = row["dep_name"].ToString(),
                pro_name = row["pro_name"].ToString(),
            });

            return userInfo;
        }

        public async Task<LoginUserInfo> GetUserInfoByAccount(int acc_no)
        {
            SqlQueryObject sqlObj = new SqlQueryObject();
            sqlObj.Sql = @"
                        SELECT people.peo_uid, accounts.acc_login, baseperson.bas_name, departments.dep_name, profess.pro_name
                        FROM people
                        INNER JOIN baseperson ON baseperson.bas_id = people.bas_id
                        INNER JOIN departments ON people.dep_no = departments.dep_no
                        INNER JOIN profess ON people.pro_no = profess.pro_no
                        INNER JOIN accounts ON people.peo_uid = accounts.peo_uid
                        WHERE people.peo_jobtype='1' AND accounts.acc_status ='1'
                         AND accounts.acc_no = @accno ";
            sqlObj.AddParameter(new SqlParameter("@accno", acc_no));

            LoginUserInfo userInfo = await QuerySingleAsync(sqlObj, row => new LoginUserInfo
            {
                acc_login = row["acc_login"].ToString(),
                peo_uid = row["peo_uid"].ToString().ToInt(),
                peo_name = row["bas_name"].ToString(),
                dep_name = row["dep_name"].ToString(),
                pro_name = row["pro_name"].ToString(),
            });

            return userInfo;
        }
    }
}
