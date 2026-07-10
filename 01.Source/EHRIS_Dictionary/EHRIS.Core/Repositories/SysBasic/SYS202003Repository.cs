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
    public class SYS202003Repository : BaseRepository, ISYS202003Repository
    {
        public SYS202003Repository(ApplicationDbContext context) : base(context)
        {
        }

        private string BuildPTypeSearchHavingClause(ExtraSearch extraSearch)
        {
            if (extraSearch == null ||
                string.IsNullOrEmpty(extraSearch.searchValue) ||
                extraSearch.columnIndexes == null ||
                !extraSearch.columnIndexes.Any())
            {
                return "HAVING (p.pty_code LIKE @searchValue OR p.pty_name LIKE @searchValue OR ISNULL(STRING_AGG(sv.svr_name, ', '), '') LIKE @searchValue)";
            }

            var searchConditions = new List<string>();

            if (extraSearch.columnIndexes.Contains(0))
            {
                searchConditions.Add("p.pty_code LIKE @searchValue");
            }
            if (extraSearch.columnIndexes.Contains(1))
            {
                searchConditions.Add("p.pty_name LIKE @searchValue");
            }
            if (extraSearch.columnIndexes.Contains(4))
            {
                searchConditions.Add("p.pty_modifyname LIKE @searchValue");
            }
            if (!searchConditions.Any() || extraSearch.columnIndexes.Contains(2))
            {
                searchConditions.Add("ISNULL(STRING_AGG(sv.svr_name, ', '), '') LIKE @searchValue");
            }


            if (searchConditions.Any())
            {
                return $"HAVING ({string.Join(" OR ", searchConditions)})";
            }
            else
            {
                return "HAVING (p.pty_code LIKE @searchValue OR p.pty_name LIKE @searchValue OR ISNULL(STRING_AGG(sv.svr_name, ', '), '') LIKE @searchValue)";
            }
        }

        public async Task<DataTablesResponse<SYS202003ListViewModel>> GetPTypePagedListAsync(DataTablesRequest request)
        {
            var parameters = new List<SqlParameter>();
            var havingSql = new StringBuilder();

            var searchValue = request.extraSearch?.searchValue;
            parameters.Add(new SqlParameter("@searchValue",
                string.IsNullOrEmpty(searchValue) ? (object)DBNull.Value : $"%{searchValue}%"));

            if (!string.IsNullOrEmpty(searchValue))
            {
                havingSql.Append(BuildPTypeSearchHavingClause(request.extraSearch));
            }

            var baseSql = @"
                FROM ptype p
                LEFT JOIN ptype_persontype ppt ON p.pty_no = ppt.ptt_ptyno
                LEFT JOIN sysvariable sv ON ppt.ptt_persontype = sv.svr_code AND sv.sar_code = 'PERSON_TYPE'
                WHERE p.pty_status = '1'
                GROUP BY p.pty_no, p.pty_code, p.pty_name, p.pty_order, p.pty_modifyname, p.pty_modifytime";

            var orderBySql = "ORDER BY p.pty_order ASC, p.pty_code ASC";
            if (request.orderby != null && request.orderby.Any())
            {
                var order = request.orderby.First();
                var columnIndex = order.column;
                var sortColumn = request.Columns[columnIndex].data;
                var sortDirection = order.dir.ToUpper() == "ASC" ? "ASC" : "DESC";

                var validSortColumns = new Dictionary<string, string>
                 {
                     { "ptyCode", "p.pty_code" },
                     { "ptyName", "p.pty_name" },
                                          { "ptyOrder", "p.pty_order" },
                     { "ptyModifyName", "p.pty_modifyname" },
                     { "ptyModifyTime", "p.pty_modifytime" }
                 };

                if (validSortColumns.ContainsKey(sortColumn))
                {
                    orderBySql = $"ORDER BY {validSortColumns[sortColumn]} {sortDirection}";
                }
                else
                {
                    orderBySql = "ORDER BY p.pty_order ASC, p.pty_code ASC";
                }
            }


            var dataSql = $@"
                SELECT
                    p.pty_no AS PtyNo,
                    p.pty_code AS PtyCode,
                    p.pty_name AS PtyName,
                    p.pty_order AS PtyOrder,
                    p.pty_modifyname AS PtyModifyName,
                    p.pty_modifytime AS PtyModifyTime,
                    ISNULL(STRING_AGG(sv.svr_name, ', '), '') AS PersonTypes
                    {baseSql}
                    {havingSql}
                    {orderBySql}
                    OFFSET @start ROWS FETCH NEXT @length ROWS ONLY;";

            var dataParameters = new List<SqlParameter>(parameters); dataParameters.Add(new SqlParameter("@start", request.Start));
            dataParameters.Add(new SqlParameter("@length", request.Length));

            var data = await _context.Database.SqlQueryRaw<SYS202003ListViewModel>(dataSql, dataParameters.ToArray()).ToListAsync();

            var totalRecordsSql = "SELECT COUNT(*) AS Value FROM ptype WHERE pty_status = '1'";
            var totalRecords = await _context.Database.SqlQueryRaw<int>(totalRecordsSql).SingleAsync();

            int filteredRecords = totalRecords;
            if (!string.IsNullOrEmpty(searchValue))
            {
                var filteredRecordsSql = $"SELECT COUNT(*) AS Value FROM (SELECT p.pty_no {baseSql} {havingSql}) AS FilteredQuery";
                filteredRecords = await _context.Database.SqlQueryRaw<int>(filteredRecordsSql, parameters.ToArray()).SingleAsync();
            }


            return new DataTablesResponse<SYS202003ListViewModel>
            {
                draw = request.Draw,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data = data
            };
        }
        public async Task<PType> GetPTypeByIdAsync(int ptyNo)
        {
            var sql = @"
                SELECT [pty_no], [pty_code], [pty_name], [pty_order], [pty_status], [pty_createtime], [pty_createname], [pty_modifytime], [pty_modifyname] 
                FROM ptype 
                WHERE pty_no = @ptyNo";
            var param = new SqlParameter("@ptyNo", ptyNo);
            return await _context.PType.FromSqlRaw(sql, param).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<List<PersonTypeOption>> GetAllPersonTypeOptionsAsync()
        {
            var sql = @"
                 SELECT svr_code AS SvrCode, svr_name AS SvrName
                 FROM sysvariable
                 WHERE sar_code = 'PERSON_TYPE' AND svr_status = '1'
                 ORDER BY svr_order;";
            return await _context.Database.SqlQueryRaw<PersonTypeOption>(sql).ToListAsync();
        }

        public async Task<List<string>> GetSelectedPersonTypesAsync(int ptyNo)
        {
            var sql = @"
                 SELECT ptt_persontype 
                 FROM ptype_persontype 
                 WHERE ptt_ptyno = @ptyNo";
            var param = new SqlParameter("@ptyNo", ptyNo);
            return await _context.Database.SqlQueryRaw<string>(sql, param).ToListAsync();
        }

        public async Task<bool> CodeExistsAsync(string code, int currentId)
        {
            var sql = @"
                 SELECT 1 AS Value 
                 FROM ptype 
                 WHERE pty_code = @code AND pty_no != @currentId AND pty_status = '1'";
            var parameters = new[]
            {
                 new SqlParameter("@code", code),
                 new SqlParameter("@currentId", currentId)
             };
            var result = await _context.Database.SqlQueryRaw<int>(sql, parameters).FirstOrDefaultAsync();
            return result == 1;
        }

        public async Task CreatePTypeAndAssociationsAsync(PTypeUpdateViewModel model, string userName, IDataLogger dataLogger)
        {
            var ptype = new PType
            {
                PtyNo = int.Parse(model.PtyCode),
                PtyCode = model.PtyCode,
                PtyName = model.PtyName,
                PtyOrder = (int)model.PtyOrder,
                PtyStatus = 1,
                PtyCreateTime = DateTime.Now,
                PtyCreateName = userName,
                PtyModifyTime = DateTime.Now,
                PtyModifyName = userName
            };

            await _context.PType.AddAsync(ptype);

            if (model.SelectedPersonTypes != null && model.SelectedPersonTypes.Any())
            {
                foreach (var svrCode in model.SelectedPersonTypes)
                {
                    var assoc = new PtypePersonType
                    {
                        PttPtyNo = ptype.PtyNo,
                        PttPersonType = svrCode,
                        PtyCreateTime = DateTime.Now,
                        PtyCreateName = userName,
                        PtyModifyTime = DateTime.Now,
                        PtyModifyName = userName
                    };
                    await _context.PtypePersonType.AddAsync(assoc);
                }
            }

            await SaveChangesAsync(dataLogger);
        }

        public async Task UpdatePTypeAndAssociationsAsync(PTypeUpdateViewModel model, string userName, IDataLogger dataLogger)
        {
            var ptype = await _context.PType.FirstOrDefaultAsync(x => x.PtyNo == model.PtyNo);

            if (ptype != null)
            {
                ptype.PtyName = model.PtyName;
                ptype.PtyOrder = (int)model.PtyOrder;
                ptype.PtyModifyTime = DateTime.Now;
                ptype.PtyModifyName = userName;

                var oldAssocs = _context.PtypePersonType.Where(x => x.PttPtyNo == model.PtyNo);
                _context.PtypePersonType.RemoveRange(oldAssocs);

                if (model.SelectedPersonTypes != null && model.SelectedPersonTypes.Any())
                {
                    foreach (var svrCode in model.SelectedPersonTypes)
                    {
                        var newAssoc = new PtypePersonType
                        {
                            PttPtyNo = model.PtyNo,
                            PttPersonType = svrCode,
                            PtyCreateTime = DateTime.Now,
                            PtyCreateName = userName,
                            PtyModifyTime = DateTime.Now,
                            PtyModifyName = userName
                        };
                        await _context.PtypePersonType.AddAsync(newAssoc);
                    }
                }

                await SaveChangesAsync(dataLogger);
            }
        }

        public async Task SoftDeletePTypeAsync(int ptyNo, IDataLogger dataLogger)
        {
            var ptype = await _context.PType.FirstOrDefaultAsync(x => x.PtyNo == ptyNo);
            if (ptype != null)
            {
                ptype.PtyStatus = 2;
                await SaveChangesAsync(dataLogger);
            }
        }
    }
}