using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EHRIS.Core.Repositories.SysBasic
{
    public class SYS201001Repository : BaseRepository, ISYS201001Repository
    {
        public SYS201001Repository(ApplicationDbContext context, AuditDbContext aduitContext) : base(context)
        {

        }

        public async Task<List<AdminListViewModel>> GetAllAdminsAsync(AdminDataTableRequest request)
        {
            var searchValue = request.extraSearch?.searchValue;
            var searchParam = new SqlParameter("@searchValue",
                string.IsNullOrEmpty(searchValue) ? (object)DBNull.Value : $"%{searchValue}%");

            var whereClause = BuildAdminWhereClause(request.extraSearch);

            string sql = $@"
                SELECT
                    a.acc_no,
                    d.dep_name AS unit,
                    b.bas_name AS name,
                    a.acc_login AS loginAccount,
                    CASE WHEN a.acc_status = '1' THEN N'啟用' ELSE N'停用' END AS accountStatus,
                    a.acc_modifyname AS modifyName,
                    (
                        SELECT MAX(LatestTime) 
                        FROM (
                            VALUES (a.acc_pwchange), 
                                   ((SELECT MAX(pas_datetime) FROM passwdChange pc WHERE pc.pas_accno = a.acc_no))
                        ) AS AllTimes(LatestTime)
                    ) AS modifyTime
                FROM accounts a
                INNER JOIN people p ON a.peo_uid = p.peo_uid
                INNER JOIN baseperson b ON p.bas_id = b.bas_id
                INNER JOIN departments d ON p.dep_no = d.dep_no
                {whereClause}
                ";

            return await _context.Database.SqlQueryRaw<AdminListViewModel>(sql, searchParam).ToListAsync();
        }

        public async Task<List<(string Hash, string Salt)>> GetRecentPasswordHistoryAsync(int accNo, int count)
        {
            int historyCountToFetch = 3;

            string sql = @"
                SELECT Hash, Salt 
                FROM 
                (
                    SELECT TOP (@pCount) 
                           pc.pas_passwdHash AS Hash, 
                           pc.pas_passwdSalt AS Salt
                    FROM passwdChange pc
                    WHERE pc.pas_accno = @pAccNo
                    ORDER BY pc.pas_datetime DESC
                ) AS History

                UNION
                        SELECT
                               a.acc_passwdHash AS Hash,
                               a.acc_passwdSalt AS Salt
                                FROM accounts a
                        WHERE a.acc_no = @pAccNo";

            var parameters = new[]
            {
                new SqlParameter("@pCount", historyCountToFetch),
                new SqlParameter("@pAccNo", accNo)
            };

            var dtoResults = await _context.Database
                .SqlQueryRaw<PasswordHistoryDto>(sql, parameters)
                .ToListAsync();

            var tupleResults = dtoResults.Select(dto => (dto.Hash, dto.Salt)).ToList();

            return tupleResults;
        }

        public async Task<int> GetTotalAdminCountAsync()
        {
            string sql = "SELECT COUNT(*) FROM accounts WHERE acc_status = '1'";
            var result = await _context.Database.SqlQueryRaw<int>(sql).ToListAsync();
            return result.SingleOrDefault();
        }

        public async Task<int> GetFilteredAdminCountAsync(AdminDataTableRequest request)
        {
            var searchValue = request.extraSearch?.searchValue;
            var searchParam = new SqlParameter("@searchValue",
                string.IsNullOrEmpty(searchValue) ? (object)DBNull.Value : $"%{searchValue}%");

            var whereClause = BuildAdminWhereClause(request.extraSearch);

            string sql = $@"
                SELECT COUNT(*)
                FROM accounts a
                INNER JOIN people p ON a.peo_uid = p.peo_uid
                INNER JOIN baseperson b ON p.bas_id = b.bas_id
                INNER JOIN departments d ON p.dep_no = d.dep_no
                {whereClause}";

            var result = await _context.Database.SqlQueryRaw<int>(sql, searchParam).ToListAsync();
            return result.SingleOrDefault();
        }

        private string BuildAdminWhereClause(ExtraSearch extraSearch)
        {
            var whereClause = new StringBuilder("WHERE a.acc_status = '1' ");

            if (extraSearch == null ||
                string.IsNullOrEmpty(extraSearch.searchValue) ||
                extraSearch.columnIndexes == null ||
                !extraSearch.columnIndexes.Any())
            {
                return whereClause.ToString();
            }

            var searchConditions = new List<string>();

            if (extraSearch.columnIndexes.Contains(0))
            {
                searchConditions.Add("d.dep_name LIKE @searchValue");
            }
            if (extraSearch.columnIndexes.Contains(1))
            {
                searchConditions.Add("b.bas_name LIKE @searchValue");
            }
            if (extraSearch.columnIndexes.Contains(2))
            {
                searchConditions.Add("a.acc_login LIKE @searchValue");
            }

            if (searchConditions.Any())
            {
                whereClause.Append($" AND ({string.Join(" OR ", searchConditions)}) ");
            }

            return whereClause.ToString();
        }

        public async Task<AdminDetailsViewModel> GetAdminDetailsAsync(int accNo)
        {
            string mainSql = @"
                SELECT
                    a.acc_no,
                    p.dep_no AS unitId,
                    b.bas_name AS name,
                    p.pro_no AS proNo,
                    b.bas_idcard AS idNumber,
                    b.bas_birthday AS birthDate,
                    a.acc_login AS loginAccount,
                    b.bas_sex AS sex,
                    b.bas_id AS BasId
                FROM
                    accounts a
                INNER JOIN
                    people p ON a.peo_uid = p.peo_uid
                INNER JOIN
                    baseperson b ON p.bas_id = b.bas_id
                WHERE
                    a.acc_no = {0}";

            var admin = await _context.Database.SqlQueryRaw<AdminDetailsViewModel>(mainSql, accNo).FirstOrDefaultAsync();

            if (admin == null) return null;

            string roleSql = "SELECT T1.rol_no FROM roleaccount T1 WHERE T1.acc_no = {0}";
            admin.selectedRoleIds = await _context.Database.SqlQueryRaw<int>(roleSql, accNo).ToListAsync();

            string ptypeSql = "SELECT T1.pty_no FROM mtype T1 WHERE T1.acc_no = {0}";
            admin.SelectedPtypeNos = await _context.Database.SqlQueryRaw<int>(ptypeSql, accNo).ToListAsync();

            string superviseSql = "SELECT T1.sup_no, T1.acc_no, T1.sup_type FROM supervise T1 WHERE T1.acc_no = {0}";
            var supervise = await _context.Database.SqlQueryRaw<Supervise>(superviseSql, accNo).FirstOrDefaultAsync();
            admin.managesAllDepts = supervise?.SupType == "1";

            if (supervise != null && !admin.managesAllDepts)
            {
                string deptSql = "SELECT T1.dep_no FROM department_category T1 WHERE T1.sup_no = {0}";
                admin.selectedDeptIds = await _context.Database.SqlQueryRaw<int>(deptSql, supervise.SupNo).ToListAsync();
            }
            string emailSql = "SELECT bse_no AS BseNo, bse_emailtype AS BseEmailType, bse_email AS BseEmail, bse_order AS BseOrder FROM baseperson_email WHERE bas_id = @basId ORDER BY bse_order";
            admin.Emails = await _context.Database.SqlQueryRaw<AdminEmailViewModel>(emailSql, new SqlParameter("@basId", admin.BasId)).ToListAsync();

            return admin;
        }

        public async Task<Account> GetAccountByIdAsync(int id) => await _context.Accounts.FindAsync(id);

        public async Task<bool> DoesAccountExistAsync(string loginAccount, int excludeAccNo = 0) => await _context.Accounts.AnyAsync(a => a.AccLogin == loginAccount && a.AccNo != excludeAccNo);

        public async Task<int> AddAdminAsync(AdminUpdateModel model, string hashedPassword, string salt, string modifierName, DateTime processTime, IDataLogger dataLogger)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var newBasePerson = new BasePerson
                    {
                        BasId = Guid.NewGuid(),
                        BasName = model.name,
                        BasIdCard = model.idNumber,
                        BasBirthday = model.birthDate,
                        BasSex = model.sex ?? false,
                        BasCreateTime = processTime,
                        BasCreateName = modifierName,
                        BasModifyTime = processTime,
                        BasModifyName = modifierName,
                        BasIsNational = true,
                        BasPhoto = "",
                        BasPhotoWatermark = "",
                        BasRemark = ""
                    };
                    _context.Basepersons.Add(newBasePerson);
                    await SaveChangesAsync(dataLogger);

                    var newPerson = new People
                    {
                        BasId = newBasePerson.BasId,
                        DepNo = model.unitId,
                        PeoAccount = model.loginAccount,
                        PeoCreateTime = processTime,
                        PeoCreateName = modifierName,
                        PeoModifyTime = processTime,
                        PeoModifyName = modifierName,
                        ProNo = model.proNo,
                        PtyNo = 1,
                        PeoJobType = 1
                    };
                    _context.Peoples.Add(newPerson);
                    await SaveChangesAsync(dataLogger);

                    var newAccount = new Account
                    {
                        PeoUid = newPerson.PeoUid,
                        AccLogin = model.loginAccount,
                        AccPainText = "",
                        AccPasswdHash = hashedPassword,
                        AccPasswdSalt = salt,
                        AccStatus = 1,
                        AccPwChange = processTime,
                        AccCreateTime = processTime,
                        AccCreateName = modifierName,
                        AccModifyTime = processTime,
                        AccModifyName = modifierName
                    };
                    _context.Accounts.Add(newAccount);
                    await SaveChangesAsync(dataLogger);

                    var newPasswordHistory = new PasswdChange
                    {
                        PasAccNo = newAccount.AccNo,
                        PasPainText = "",
                        PasPasswdHash = hashedPassword,
                        PasPasswdSalt = salt,
                        PasDatetime = processTime,
                        PasChangeUID = dataLogger.ExecUID
                    };
                    _context.PasswdChanges.Add(newPasswordHistory);

                    if (model.SelectedPtypeNos?.Any() == true)
                        _context.Mtype.AddRange(model.SelectedPtypeNos.Select(p => new MType { AccNo = newAccount.AccNo, PtyNo = p }));

                    if (model.selectedRoleIds?.Any() == true)
                    {
                        _context.RoleAccounts.AddRange(model.selectedRoleIds.Select(r => new RoleAccount
                        {
                            AccNo = newAccount.AccNo,
                            RolNo = r,
                            RacCreateName = modifierName,
                            RacCreateTime = processTime,
                            RacModifyName = modifierName,
                            RacModifyTime = processTime
                        }));
                    }

                    var newSupervise = new Supervise { AccNo = newAccount.AccNo, SupType = model.managesAllDepts ? "1" : "2" };
                    _context.Supervise.Add(newSupervise);
                    await SaveChangesAsync(dataLogger);

                    if (!model.managesAllDepts && model.selectedDeptIds?.Any() == true)
                        _context.DepartmentCategory.AddRange(model.selectedDeptIds.Select(d => new DepartmentCategory { SupNo = newSupervise.SupNo, DepNo = d }));

                    if (model.Emails?.Any() == true)
                    {
                        int emailOrder = 1;
                        foreach (var emailVm in model.Emails)
                        {
                            var newEmail = new BasePersonEmail
                            {
                                BasId = newBasePerson.BasId,
                                BseEmailType = emailVm.BseEmailType,
                                BseEmail = emailVm.BseEmail,
                                BseOrder = emailOrder,
                                BseCreateName = modifierName,
                                BseCreateTime = processTime,
                                BseModifyName = modifierName,
                                BseModifyTime = processTime
                            };
                            _context.BasePersonEmail.Add(newEmail);
                        }
                    }

                    var result = await SaveChangesAsync(dataLogger);
                    await transaction.CommitAsync();
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<int> UpdateAdminAsync(AdminUpdateModel model, string hashedPassword, string salt, string modifierName, DateTime processTime, IDataLogger dataLogger)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var account = await _context.Accounts.FindAsync(model.acc_no);
                    var person = await _context.Peoples.FirstOrDefaultAsync(p => p.PeoUid == account.PeoUid);
                    var basePerson = await _context.Basepersons.FirstOrDefaultAsync(b => b.BasId == person.BasId);

                    basePerson.BasName = model.name;
                    basePerson.BasBirthday = model.birthDate;
                    basePerson.BasSex = model.sex ?? false;
                    basePerson.BasModifyTime = processTime;
                    basePerson.BasModifyName = modifierName;

                    person.DepNo = model.unitId;
                    person.ProNo = model.proNo;
                    person.PeoModifyTime = processTime;
                    person.PeoModifyName = modifierName;

                    account.AccModifyTime = processTime;
                    account.AccModifyName = modifierName;

                    if (!string.IsNullOrEmpty(model.password))
                    {
                        account.AccPainText = "";
                        account.AccPwChange = processTime;
                        account.AccPasswdHash = hashedPassword;
                        account.AccPasswdSalt = salt;

                        var newPasswordHistory = new PasswdChange
                        {
                            PasAccNo = account.AccNo,
                            PasPainText = "",
                            PasPasswdHash = hashedPassword,
                            PasPasswdSalt = salt,
                            PasDatetime = processTime,
                            PasChangeUID = dataLogger.ExecUID
                        };
                        _context.PasswdChanges.Add(newPasswordHistory);
                    }

                    _context.Mtype.RemoveRange(_context.Mtype.Where(m => m.AccNo == model.acc_no));
                    _context.RoleAccounts.RemoveRange(_context.RoleAccounts.Where(ra => ra.AccNo == model.acc_no));

                    var existingSupervises = await _context.Supervise.Where(s => s.AccNo == model.acc_no).ToListAsync();
                    if (existingSupervises.Any())
                    {
                        var supNos = existingSupervises.Select(s => s.SupNo).ToList();
                        _context.DepartmentCategory.RemoveRange(_context.DepartmentCategory.Where(d => supNos.Contains(d.SupNo)));
                        _context.Supervise.RemoveRange(existingSupervises);
                    }

                    var oldEmails = await _context.BasePersonEmail.Where(e => e.BasId == basePerson.BasId).ToListAsync();
                    _context.BasePersonEmail.RemoveRange(oldEmails);

                    await SaveChangesAsync(dataLogger);

                    if (model.SelectedPtypeNos?.Any() == true)
                        _context.Mtype.AddRange(model.SelectedPtypeNos.Select(p => new MType { AccNo = model.acc_no, PtyNo = p }));

                    if (model.selectedRoleIds?.Any() == true)
                    {
                        _context.RoleAccounts.AddRange(model.selectedRoleIds.Select(r => new RoleAccount
                        {
                            AccNo = model.acc_no,
                            RolNo = r,
                            RacCreateName = modifierName,
                            RacCreateTime = processTime,
                            RacModifyName = modifierName,
                            RacModifyTime = processTime
                        }));
                    }

                    var newSupervise = new Supervise { AccNo = model.acc_no, SupType = model.managesAllDepts ? "1" : "2" };
                    _context.Supervise.Add(newSupervise);
                    await SaveChangesAsync(dataLogger);

                    if (!model.managesAllDepts && model.selectedDeptIds?.Any() == true)
                        _context.DepartmentCategory.AddRange(model.selectedDeptIds.Select(d => new DepartmentCategory { SupNo = newSupervise.SupNo, DepNo = d }));

                    if (model.Emails?.Any() == true)
                    {
                        int emailOrder = 1;
                        foreach (var emailVm in model.Emails)
                        {
                            var newEmail = new BasePersonEmail
                            {
                                BasId = basePerson.BasId,
                                BseEmailType = emailVm.BseEmailType,
                                BseEmail = emailVm.BseEmail,
                                BseOrder = emailOrder,
                                BseCreateName = modifierName,
                                BseCreateTime = processTime,
                                BseModifyName = modifierName,
                                BseModifyTime = processTime
                            };
                            _context.BasePersonEmail.Add(newEmail);
                        }
                    }

                    var result = await SaveChangesAsync(dataLogger);
                    await transaction.CommitAsync();
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<int> SoftDeleteAdminAsync(int accNo, string modifierName, DateTime processTime, IDataLogger dataLogger)
        {
            var account = await _context.Accounts.FindAsync(accNo);
            if (account == null) return 0;

            account.AccStatus = 2;
            account.AccModifyTime = processTime;
            account.AccModifyName = modifierName;

            return await SaveChangesAsync(dataLogger);
        }

        public async Task<List<Profess>> GetProfessListAsync()
        {
            string sql = @"
        SELECT T1.[pro_no], T1.[pro_code], T1.[pro_name], T1.[pro_english], T1.[pro_order], T1.[pro_isManager], T1.[pro_status], T1.[pro_createtime], T1.[pro_createname], T1.[pro_modifytime], T1.[pro_modifyname] 
        FROM profess T1 
        WHERE T1.pro_status = @pStatus
        ORDER BY T1.pro_order";

            var param = new SqlParameter("@pStatus", System.Data.SqlDbType.TinyInt) { Value = (byte)1 };

            return await _context.Database.SqlQueryRaw<Profess>(sql, param).ToListAsync();
        }

        public async Task<List<Departments>> GetUnitListAsync()
        {
            string sql = @"
                SELECT T1.[dep_no], T1.[dep_depid], T1.[dep_parentid], T1.[dep_name], T1.[dep_level], T1.[dep_code], T1.[dep_order], T1.[dep_status], T1.[dep_introduce], T1.[dep_createtime], T1.[dep_createname], T1.[dep_modifytime], T1.[dep_modifyname], T1.[uni_id], T1.[ude_no] 
                FROM departments T1 
                WHERE T1.dep_level = 1 AND T1.dep_status = '1'";

            return await _context.Database.SqlQueryRaw<Departments>(sql).ToListAsync();
        }

        public async Task<List<Role>> GetRoleListAsync()
        {
            string sql = @"
                SELECT T1.[rol_no], T1.[rol_name], T1.[rol_memo], T1.[rol_open], T1.[rol_createtime], T1.[rol_createname], T1.[rol_modifytime], T1.[rol_modifyname] 
                FROM role T1 
                WHERE T1.rol_open = 1";

            return await _context.Database.SqlQueryRaw<Role>(sql).ToListAsync();
        }

        public async Task<List<PType>> GetActivePersonnelTypesAsync()
        {
            string sql = @"
                SELECT T1.[pty_no], T1.[pty_code], T1.[pty_name], T1.[pty_order], T1.[pty_status], T1.[pty_createtime], T1.[pty_createname], T1.[pty_modifytime], T1.[pty_modifyname] 
                FROM ptype T1 
                WHERE T1.pty_status = '1'
                ORDER BY T1.pty_no";

            return await _context.Database.SqlQueryRaw<PType>(sql).ToListAsync();
        }

        public async Task<HashSet<int>> GetStaticLockPeoUidsAsync()
        {
            string sql = "SELECT refKey FROM zbufferRef WHERE kind = @Kind";

            var param = new SqlParameter("@Kind", "staticLock");

            var result = await _context.Database
                .SqlQueryRaw<string>(sql, param)
                .ToListAsync();

            var ids = new HashSet<int>();
            foreach (var item in result)
            {
                if (int.TryParse(item, out int id))
                {
                    ids.Add(id);
                }
            }

            return ids;
        }
    }
}