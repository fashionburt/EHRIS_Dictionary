using EHRIS.Core.Entities;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Repositories;
using EHRIS.Tools.DataBase;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;


namespace EHRIS.Core.DbContext;

public class AuditDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    // 對應到 Log 資料庫中的資料表
    public AuditDbContext(DbContextOptions<AuditDbContext> options)
        : base(options)
    {
    }

    #region Table Entities
    //public DbSet<AccessLog> AccessLogs { get; set; }
    public DbSet<Operates> Operates { get; set; }

    public DbSet<PersonalEvent> Personalevents { get; set; }
    public DbSet<PersonalEventsDetail> Personaleventsdetails { get; set; }
    #endregion

    // 定義資料表的細節，例如欄位長度等
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 建立 Personalevent 與 Personaleventsdetail 一對多關聯
        modelBuilder.Entity<PersonalEvent>()
        .HasMany(p => p.Details)
        .WithOne(d => d.Personalevent)
        .HasForeignKey(d => d.pev_no)
        .OnDelete(DeleteBehavior.Cascade);

        //建立 operates 
        modelBuilder.Entity<Operates>(entity =>
        {
            entity.ToTable("operates");
            entity.HasKey(e => e.ope_no);

            entity.Property(e => e.ope_no).HasColumnName("ope_no");
            entity.Property(e => e.ope_date).HasColumnName("ope_date");
            entity.Property(e => e.ope_peouid).HasColumnName("ope_peouid");
            entity.Property(e => e.ope_depname).HasColumnName("ope_depname");
            entity.Property(e => e.ope_proname).HasColumnName("ope_proname");
            entity.Property(e => e.ope_peoname).HasColumnName("ope_peoname");
            entity.Property(e => e.ope_sfuno).HasColumnName("ope_sfuno");
            entity.Property(e => e.ope_sfuname).HasColumnName("ope_sfuname");
            entity.Property(e => e.ope_logintime).HasColumnName("ope_logintime");
            entity.Property(e => e.ope_function).HasColumnName("ope_function");
            entity.Property(e => e.ope_method).HasColumnName("ope_method");
            entity.Property(e => e.ope_memo).HasColumnName("ope_memo");
            entity.Property(e => e.ope_ipaddresss).HasColumnName("ope_ipaddresss");
        });

    }

    /// <summary>
    /// 單筆呼叫 SP
    /// </summary>
    /// <param name="peoUID">操作人</param>
    /// <param name="depName">操作人單位</param>
    /// <param name="proName">操作人職稱</param>
    /// <param name="peoName">操作人姓名</param>
    /// <param name="sfuNo">功能編號</param>
    /// <param name="sfuName">功能名稱</param>
    /// <param name="function">操作行為</param>
    /// <param name="actionmethod">操作網站方法</param>
    /// <param name="memo">操作訊息</param>
    /// <param name="ip">來源IP</param>
    /// <returns></returns>
    public async Task<int> ExecuteOperatesSP(int peoUID, string depName, string proName, string peoName,
                                             int sfuNo, string sfuName, int function, string actionmethod, string memo, string ip)
    {
        var sql = "EXEC usp_OperatesIns @PEOUID, @DEPNAME, @PRONAME, @PEONAME, @SFUNO, @SFUNAME, @FUNCTION, @ACTIONMETHOD, @MEMO, @IPADDRESS";

        var parameters = new[]
        {
            new SqlParameter("@PEOUID", peoUID),
            new SqlParameter("@DEPNAME", depName ?? string.Empty),
            new SqlParameter("@PRONAME", proName ?? string.Empty),
            new SqlParameter("@PEONAME", peoName ?? string.Empty),
            new SqlParameter("@SFUNO", sfuNo),
            new SqlParameter("@SFUNAME", sfuName ?? string.Empty),
            new SqlParameter("@FUNCTION", function),
            new SqlParameter("@ACTIONMETHOD", actionmethod ?? string.Empty),
            new SqlParameter("@MEMO", memo ?? string.Empty),
            new SqlParameter("@IPADDRESS", ip ?? string.Empty)
        };

        return await Database.ExecuteSqlRawAsync(sql, parameters);
    }

    // 批次呼叫 SP (TVP)
    //public async Task<int> ExecuteBatchOperatesSP(List<OperateDto> operates)
    //{
    //    using (var conn = Database.GetDbConnection())
    //    {
    //        await conn.OpenAsync();
    //        using (var cmd = conn.CreateCommand())
    //        {
    //            cmd.CommandText = "usp_OperatesInsBatch";
    //            cmd.CommandType = CommandType.StoredProcedure;

    //            var param = new SqlParameter("@Operates", SqlDbType.Structured)
    //            {
    //                TypeName = "dbo.OperatesTableType",
    //                Value = CreateOperatesDataTable(operates)
    //            };

    //            cmd.Parameters.Add(param);

    //            return await cmd.ExecuteNonQueryAsync();
    //        }
    //    }
    //}

    //// 建立 DataTable 對應 TVP
    //private DataTable CreateOperatesDataTable(List<OperateDto> operates)
    //{
    //    var table = new DataTable();
    //    table.Columns.Add("PEOUID", typeof(int));
    //    table.Columns.Add("DEPNAME", typeof(string));
    //    table.Columns.Add("PRONAME", typeof(string));
    //    table.Columns.Add("PEOName", typeof(string));
    //    table.Columns.Add("SFUNO", typeof(int));
    //    table.Columns.Add("SFUNAME", typeof(string));
    //    table.Columns.Add("FUNCTION", typeof(int));
    //    table.Columns.Add("ACTIONMETHOD", typeof(string));
    //    table.Columns.Add("MEMO", typeof(string));
    //    table.Columns.Add("IPADDRESS", typeof(string));

    //    foreach (var op in operates)
    //    {
    //        table.Rows.Add(op.PeoUid, op.DepName, op.ProName, op.PeoName,
    //                       op.SfuNo, op.SfuName, op.Function, op.ActionMethod, op.Memo, op.IpAddress);
    //    }

    //    return table;
    //}


}
