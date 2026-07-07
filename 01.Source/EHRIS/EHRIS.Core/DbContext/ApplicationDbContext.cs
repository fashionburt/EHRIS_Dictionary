using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Repositories.Event;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;


namespace EHRIS.Core.DbContext;

public class ApplicationDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    #region  變數
    public readonly FieldsMappingService _fieldMappingService=new FieldsMappingService();
    public readonly AuditDbContext _auditDbContext;
    #endregion

    #region 後台管理
    public DbSet<AdminUsers> AdminUsers { get; set; }
    public DbSet<AdminUserRoles> AdminUserRoles { get; set; }
    public DbSet<AdminSys> AdminSys { get; set; }
    public DbSet<AdminFunctions> AdminFunctions { get; set; }
    public DbSet<AdminRoles> AdminRoles { get; set; }
    public DbSet<AdminRoleFunctions> AdminRoleFunctions { get; set; }
    #endregion

    #region Table Entities
    //Table:Roles
    public DbSet<Role> Roles { get; set; }
    
    // Table: sysfunction
    public DbSet<SysFuction> SysFuction { get; set; }
    //Table: account
    public DbSet<Account> Accounts {  get; set; }
    //Table: ChangeLog
    public DbSet<ChangeLog> ChangeLogs { get; set; }
    //Table: people
    public DbSet<People> Peoples { get; set; }
    //Table: rauthority
    public DbSet<RAuthority> RAuthoritys { get; set; }
    //Table: roleaccount
    public DbSet<RoleAccount> RoleAccounts { get; set; }
    //Table: pauthority
    public DbSet<AAuthority> AAuthoritys { get; set; }
    public DbSet<Announcements> Announcements { get; set; }
    public DbSet<Arguments> Arguments { get; set; }
    public DbSet<ArgumentsGroup> ArgumentsGroup { get; set; }
    public DbSet<ArgumentsDept> ArgumentsDept { get; set; }

    public DbSet<ArgumentsSchedule> ArgumentsSchedule { get; set; }
    
    //Table: systeminfo
    public DbSet<SystemInfo> SystemInfo { get; set; }

    //Table: mailmessage
    public DbSet<Entities.MailMessage> MailMessage { get; set; }
    //Table: mailType
    public DbSet<MailType> mailType { get; set; }

    public DbSet<ScheduleConfig> scheduleConfig { get; set; }

    //public DbSet<Personalevent> Personalevents { get; set; }
    //public DbSet<Personaleventsdetail> Personaleventsdetails { get; set; }

    //Table: baseperson
    public DbSet<BasePerson> Basepersons { get; set; }
    public DbSet<BasePersonAddress> BasePersonAddress {  get; set; }
    public DbSet<BasePersonBank> BasePersonBank { get; set; }
    public DbSet<BasePersonEmail> BasePersonEmail { get; set; }
    public DbSet<BasePersonPhone> BasePersonPhone { get; set; }
    public DbSet<ForgetPassWord> ForgetPassWords { get; set; }

    public DbSet<PasswdChange> PasswdChanges { get; set; }

    public DbSet<Departments> Department { get; set; }
    public DbSet<Classes> Classes { get; set; }
    public DbSet<PType> PType { get; set; }
    public DbSet<PtypePersonType> PtypePersonType { get; set; }
    public DbSet<Profess> Profess { get; set; }
    public DbSet<ProfessPersonType> ProfessPersonTypes { get; set; }
    public DbSet<DepartmentCategory> DepartmentCategory { get; set; }
    public DbSet<Supervise>   Supervise { get; set; }
    public DbSet<Rolemanager> Rolemanager { get; set; }
    public DbSet<MType> Mtype { get; set; }
    public DbSet<Unit> Unit { get; set; }
    public DbSet<UnitDepart> UnitDepart { get; set; }
    public DbSet<Sys> sys { get; set; }
    public DbSet<SysNotice> SysNotices { get; set; }
    
    public DbSet<ZbufferRef> ZbufferRef { get; set; }
    
    public DbSet<HRHoliday> HRHoliday { get; set; }
    public DbSet<HROriginal> HROriginal { get; set; }
    public DbSet<HRPLevel> HRPlevel { get; set; }    
    public DbSet<HROvertime> HROvertime { get; set; }
    public DbSet<HRVacations> HRVacations { get; set; }
    public DbSet<HRWage> HRWage { get; set; }
    public DbSet<HRWkRecord> HRWkRecord { get; set; }
    public DbSet<HRCfg01> HRCfg01 { get; set; }
    public DbSet<SysVariable> SysVariable { get; set; }
    public DbSet<StaParams> StaParams { get; set; }

    public DbSet<StaHr005> StaHr005 { get; set; }
    public DbSet<StaHr005Details> StaHr005Details { get; set; }
    public DbSet<StaHr005DLeave> StaHr005DLeave { get; set; }
    public DbSet<StaHr006> StaHr006 { get; set; }
    public DbSet<StaHr006Details> StaHr006Details { get; set; }
    #endregion

    #region WebAPI
    public DbSet<AcpLeave> AcpLeaves { get; set; }
    public DbSet<AcpOvertime> AcpOvertimes { get; set; }
    public DbSet<StatisticLeave> StatisticLeaves { get; set; }
    public DbSet<StatisticOvertime> StatisticOvertimes { get; set; }
    public DbSet<ToBeCompared> ToBeCompareds { get; set; }
    public DbSet<ComparedError> ComparedErrors { get; set; }
    //Table: filedata
    public DbSet<FileData> FileData { get; set; }
    #endregion

    #region 非Table類的Model
    //Table : Sys
    public DbSet<MenuSysViewModel> MenuSys { get; set; }
    //非Table 
    public DbSet<DepartmentTree> DepartmentTrees { get; set; }
   public DbSet<UnitTree> unitTrees { get; set; }

    #endregion

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
        AuditDbContext auditDbContext)
    : base(options)
    {
        _auditDbContext = auditDbContext;
    }
   
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuSysViewModel>().HasNoKey(); //  宣告 `MenuSys` 沒有主鍵
        modelBuilder.Entity<SysFuction>().HasNoKey(); 

        // 建立 People 與 BasePerson 的一對一關聯
        modelBuilder.Entity<People>()
            .HasOne(p => p.BasePerson)
            .WithOne()
            .HasForeignKey<People>(p => p.BasId);

        // 建立 People 與 Department 的一對多關聯
        modelBuilder.Entity<People>()
            .HasOne(p => p.Department)
            .WithMany()
            .HasForeignKey(p => p.DepNo);

        // 建立 People 與 Account 的一對一關聯
        modelBuilder.Entity<People>()
            .HasOne(p => p.Account)
            .WithOne()
            .HasForeignKey<Account>(a => a.PeoUid);
    }

    //public void SetEventOperator(EventOperatorDto dto)
    //{
    //    _eventOperatorDto = dto;
    //}

    #region Override SaveChange
    //不記錄Log的排除名單
    //private static readonly HashSet<string> _excludedEntities = new HashSet<string>
    //{
    //    "Personalevent",
    //    "Personaleventsdetail", 

    //};

    //public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    //{
    //    throw new NotSupportedException("非法儲存：請使用 Repository 方法。");


    //    // 先記錄要 log 的資料
    //    var logs = GenerateChangeLogs();
    //    var exePeople = await PersonDepartmentDtos
    //                .FromSqlInterpolated($@"
    //                        SELECT baseperson.bas_name, departments.dep_name, isnull(profess.pro_name,'') as pro_name 
    //                        FROM people
    //                        INNER JOIN baseperson on baseperson.bas_id = people.bas_id
    //                        INNER JOIN departments ON people.dep_no = departments.dep_no
    //                        LEFT JOIN profess on profess.pro_no = people.pro_no and profess.pro_status='1'
    //                        WHERE people.peo_uid = {_eventOperatorDto?.ExecUid ?? 0}").AsNoTracking()
    //                .FirstOrDefaultAsync();
    //    if (exePeople == null)
    //    {
    //        _eventOperatorDto.PevDepName =  "";
    //        _eventOperatorDto.PevPeoName =  "";
    //        _eventOperatorDto.PevProName =  "";
    //    }
    //    else
    //    {
    //        _eventOperatorDto.PevDepName = exePeople.Dep_Name ?? "";
    //        _eventOperatorDto.PevPeoName = exePeople.bas_name ?? "";
    //        _eventOperatorDto.PevProName = exePeople.Pro_Name ?? "";
    //    }
    //    foreach (var log in logs)
    //    {
    //        log.pev_execuid = _eventOperatorDto?.ExecUid ?? 0;
    //        log.pev_execipaddress = _eventOperatorDto?.FromIP ?? "Unknown";
    //        log.pev_execprocname = _eventOperatorDto?.ExecProcName ?? "Unknown";
    //        log.pev_exectype = ((int?)_eventOperatorDto?.Who)?.ToString() ?? ""; 
    //        log.pev_execsfuno = _eventOperatorDto?.ExecSfuNo ?? 0; 
    //        log.pev_uid = _eventOperatorDto?.PevUid ?? 0;
    //        log.pev_depname = _eventOperatorDto?.PevDepName ?? "";
    //        log.pev_peoname = _eventOperatorDto?.PevPeoName ?? "";
    //        log.pev_proname = _eventOperatorDto?.PevProName ?? "";
    //        if (log.pev_eventtype =="1")
    //        {
    //            var entries = ChangeTracker.Entries()
    //                .Where(e => e.State == EntityState.Unchanged &&
    //                            e.Entity.GetType().Name == log.pev_table);

    //            log.pev_pk = string.Join(",", entries.Select(e => GetPrimaryKeyValue(e)));
    //        }

    //    }


    //    // 先儲存主要變更
    //    var result = await base.SaveChangesAsync(cancellationToken);
    //    //更新 新增的PK
    //    foreach (var log in  logs.Where(l => l.pev_eventtype == "1"))
    //    {


    //            var entries = ChangeTracker.Entries()
    //                .Where(e => e.State == EntityState.Unchanged &&
    //                            e.Entity.GetType().Name == log.pev_table);

    //            log.pev_pk = string.Join(",", entries.Select(e => GetPrimaryKeyValue(e)));


    //    }
    //    // 再另行寫入 log（不影響主交易）
    //    if (logs.Any())
    //    {
    //        //Personalevents.AddRange(logs);
    //        //await base.SaveChangesAsync(cancellationToken);
    //        await _auditDbContext.Personalevents.AddRangeAsync(logs,cancellationToken);
    //        await _auditDbContext.SaveChangesAsync(cancellationToken);

    //    }

    //    return result;
    //}

    //private List<PersonalEvent> GenerateChangeLogs()
    //{
    //    var logs = new List<PersonalEvent>();

    //    // 找出有異動的實體（排除不需要記錄的）
    //    var entries = ChangeTracker.Entries()
    //        .Where(e => e.State == EntityState.Modified ||
    //                    e.State == EntityState.Deleted ||
    //                    e.State == EntityState.Added)
    //        .Where(e => !_excludedEntities.Contains(e.Entity.GetType().Name))
    //        .ToList();

    //    // 依 Table +State分群
    //    //var grouped = entries.GroupBy(e => e.Entity.GetType().Name, e.State);
    //    var grouped = entries.GroupBy(e => new { TableName = e.Entity.GetType().Name, e.State });
    //    foreach (var group in grouped)
    //    {
    //        string tableName = group.Key.TableName;
    //        EntityState state = group.Key.State;
    //        string eventType = state switch
    //        {
    //            EntityState.Added => "1",
    //            EntityState.Modified => "2",
    //            EntityState.Deleted => "3",
    //            _ => "0"
    //        };
    //        // 建立單一筆 Personalevent
    //        var logEvent = new PersonalEvent
    //        {
    //            pev_execuid = 0,
    //            pev_exectime = DateTime.Now,
    //            pev_execsfuno = 0,
    //            pev_execprocname = "ExecProcName",
    //            pev_exectype = "10",
    //            pev_execipaddress =  "127.0.0.1",
    //            pev_uid = 1,
    //            pev_depname = "",
    //            pev_peoname = "",
    //            pev_proname = "",
    //            pev_eventtype = eventType,
    //            pev_table = tableName,
    //            pev_pk = string.Join(",", group.Select(e => GetPrimaryKeyValue(e))), // 多筆 PK 用逗號連接
    //            Details = new List<PersonalEventsDetail>()
    //        };

    //        // 累積這個 table 內所有異動欄位
    //        foreach (var entry in group)
    //        {
    //            if (entry.State == EntityState.Modified)
    //            {
    //                foreach (var prop in entry.OriginalValues.Properties)
    //                {
    //                    var realTable = StoreObjectIdentifier.Table(
    //                                    entry.Metadata.GetTableName(),
    //                                    entry.Metadata.GetSchema());
    //                    var realColName = entry.Property(prop.Name).Metadata.GetColumnName(realTable);
    //                    var original = entry.OriginalValues[prop]?.ToString();
    //                    var current = entry.CurrentValues[prop]?.ToString();

    //                    if (original != current)
    //                    {
    //                        logEvent.Details.Add(new PersonalEventsDetail
    //                        {
    //                            pvt_coldesc = _fieldMappingService.GetDisplayName(realTable.Name, realColName),
    //                            pvt_column = prop.Name,
    //                            pvt_orivalue = original??"",    
    //                            pvt_newvalue = current ?? ""
    //                        });
    //                    }
    //                }
    //            }
    //            else if (entry.State == EntityState.Added)
    //            {
    //                foreach (var prop in entry.CurrentValues.Properties)
    //                {
    //                    var realTable = StoreObjectIdentifier.Table(
    //                                    entry.Metadata.GetTableName(),
    //                                    entry.Metadata.GetSchema());
    //                    var realColName = entry.Property(prop.Name).Metadata.GetColumnName(realTable);
    //                    var current = entry.CurrentValues[prop]?.ToString();
    //                    logEvent.Details.Add(new PersonalEventsDetail
    //                    {
    //                        pvt_coldesc = _fieldMappingService.GetDisplayName(realTable.Name, realColName),
    //                        pvt_column = prop.Name,
    //                        pvt_orivalue = "",
    //                        pvt_newvalue = current??""
    //                    });
    //                }
    //            }
    //            else if (entry.State == EntityState.Deleted)
    //            {
    //                foreach (var prop in entry.OriginalValues.Properties)
    //                {
    //                    var realTable = StoreObjectIdentifier.Table(
    //                                   entry.Metadata.GetTableName(),
    //                                   entry.Metadata.GetSchema());
    //                    var realColName = entry.Property(prop.Name).Metadata.GetColumnName(realTable);
    //                    var original = entry.OriginalValues[prop]?.ToString();
    //                    logEvent.Details.Add(new PersonalEventsDetail
    //                    {
    //                        pvt_coldesc = _fieldMappingService.GetDisplayName(realTable.Name, realColName),
    //                        pvt_column = prop.Name,
    //                        pvt_orivalue = original ?? "",
    //                        pvt_newvalue = ""
    //                    });
    //                }
    //            }
    //        }

    //        // 轉成精簡 JSON
    //        var simplifiedDetails = logEvent.Details.Select(d => new
    //        {
    //            d.pvt_coldesc,
    //            d.pvt_column,
    //            d.pvt_orivalue,
    //            d.pvt_newvalue
    //        }).ToList();

    //        logEvent.pev_content = JsonConvert.SerializeObject(simplifiedDetails);

    //        logs.Add(logEvent);
    //    }

    //    return logs;
    //}



    //private string GetPrimaryKeyValue(EntityEntry entry)
    //{
    //    var pk = entry.Metadata.FindPrimaryKey();
    //    if (pk == null) return "";
    //    var values = pk.Properties
    //        .Select(p => entry.Property(p.Name).CurrentValue?.ToString())
    //        .ToArray();

    //    return string.Join(",", values);
    //}
    #endregion


}
