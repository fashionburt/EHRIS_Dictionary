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
    public class SYS202004Repository : BaseRepository, ISYS202004Repository
    {
        public SYS202004Repository(ApplicationDbContext context) : base(context)
        {
        }

        private string BuildProfessSearchHavingClause(ExtraSearch extraSearch)
        {
            if (extraSearch == null || string.IsNullOrEmpty(extraSearch.searchValue))
            {
                return string.Empty;
            }

            var searchConditions = new List<string>();
            bool searchSpecificColumns = extraSearch.columnIndexes != null && extraSearch.columnIndexes.Any();

            bool performDefaultSearch = !searchSpecificColumns;


            if (searchSpecificColumns)
            {
                if (extraSearch.columnIndexes.Contains(0)) searchConditions.Add("p.pro_code LIKE @searchValue");
                if (extraSearch.columnIndexes.Contains(1)) searchConditions.Add("p.pro_name LIKE @searchValue");
                if (extraSearch.columnIndexes.Contains(2)) searchConditions.Add("p.pro_english LIKE @searchValue");
                if (extraSearch.columnIndexes.Contains(6)) searchConditions.Add("p.pro_modifyname LIKE @searchValue");
                if (extraSearch.columnIndexes.Contains(3)) searchConditions.Add("(CASE WHEN p.pro_isManager = 1 THEN N'是' ELSE N'否' END) LIKE @searchValue");
                if (extraSearch.columnIndexes.Contains(4)) searchConditions.Add("ISNULL(STRING_AGG(sv.svr_name, ', '), '') LIKE @searchValue");
            }

            if (!searchConditions.Any())
            {
                searchConditions.Add("p.pro_code LIKE @searchValue");
                searchConditions.Add("p.pro_name LIKE @searchValue");
                searchConditions.Add("p.pro_english LIKE @searchValue");
                searchConditions.Add("(CASE WHEN p.pro_isManager = 1 THEN N'是' ELSE N'否' END) LIKE @searchValue");
                searchConditions.Add("ISNULL(STRING_AGG(sv.svr_name, ', '), '') LIKE @searchValue");
                searchConditions.Add("p.pro_modifyname LIKE @searchValue");
            }

            if (searchConditions.Any())
            {
                return $"HAVING ({string.Join(" OR ", searchConditions)})";
            }

            return string.Empty;
        }

        public async Task<DataTablesResponse<SYS202004ListViewModel>> GetProfessPagedListAsync(DataTablesRequest request)
        {
            var parameters = new List<SqlParameter>();
            var havingSql = new StringBuilder();

            var searchValue = request.extraSearch?.searchValue;
            parameters.Add(new SqlParameter("@searchValue",
                string.IsNullOrEmpty(searchValue) ? (object)DBNull.Value : $"%{searchValue}%"));

            var baseSql = @"
                FROM profess p
                LEFT JOIN profess_persontype ppt ON p.pro_no = ppt.ptt_ptyno
                LEFT JOIN sysvariable sv ON ppt.ptt_persontype = sv.svr_code AND sv.sar_code = 'PERSON_TYPE'
                WHERE p.pro_status = 1
                GROUP BY p.pro_no, p.pro_code, p.pro_name, p.pro_english, p.pro_isManager, p.pro_order, p.pro_modifyname, p.pro_modifytime";

            if (!string.IsNullOrEmpty(searchValue))
            {
                havingSql.Append(BuildProfessSearchHavingClause(request.extraSearch));
            }


            var orderBySql = "ORDER BY p.pro_order ASC, p.pro_code ASC";
            if (request.orderby != null && request.orderby.Any())
            {
                var order = request.orderby.First();
                var columnIndex = order.column;
                var sortColumn = request.Columns[columnIndex].data;
                var sortDirection = order.dir.ToUpper() == "ASC" ? "ASC" : "DESC";

                var validSortColumns = new Dictionary<string, string>
                 {
                     { "proCode", "p.pro_code" },
                     { "proName", "p.pro_name" },
                     { "proEnglish", "p.pro_english" },
                     { "proIsManager", "p.pro_isManager" },
                     { "proOrder", "p.pro_order" },
                     { "proModifyName", "p.pro_modifyname" },
                     { "proModifyTime", "p.pro_modifytime" }
                 };

                if (validSortColumns.ContainsKey(sortColumn))
                {
                    orderBySql = $"ORDER BY {validSortColumns[sortColumn]} {sortDirection}";
                }
                else
                {
                    orderBySql = "ORDER BY p.pro_order ASC, p.pro_code ASC";
                }
            }


            var dataSql = $@"
                SELECT
                    p.pro_no AS ProNo,
                    p.pro_code AS ProCode,
                    p.pro_name AS ProName,
                    p.pro_english AS ProEnglish,
                CASE WHEN p.pro_isManager = 1 THEN N'是' ELSE N'否' END AS ProIsManager,
                    p.pro_order AS ProOrder,
                    p.pro_modifyname AS ProModifyName,
                    p.pro_modifytime AS ProModifyTime,
                ISNULL(STRING_AGG(sv.svr_name, ', '), '') AS PersonTypes
                    {baseSql}
                    {havingSql}
                    {orderBySql}
                OFFSET @start ROWS FETCH NEXT @length ROWS ONLY;";

            var dataParameters = new List<SqlParameter>(parameters); dataParameters.Add(new SqlParameter("@start", request.Start));
            dataParameters.Add(new SqlParameter("@length", request.Length));

            var data = await _context.Database.SqlQueryRaw<SYS202004ListViewModel>(dataSql, dataParameters.ToArray()).ToListAsync();

            var totalRecordsSql = "SELECT COUNT(*) AS Value FROM profess WHERE pro_status = 1";
            var totalRecords = await _context.Database.SqlQueryRaw<int>(totalRecordsSql).SingleAsync();

            int filteredRecords = totalRecords;
            if (!string.IsNullOrEmpty(searchValue))
            {
                var filteredRecordsSql = $@"
                     SELECT COUNT(*) AS Value
                     FROM (
                         SELECT p.pro_no 
                         {baseSql} 
                         {havingSql} 
                     ) AS FilteredQuery";
                filteredRecords = await _context.Database.SqlQueryRaw<int>(filteredRecordsSql, parameters.ToArray()).SingleAsync();
            }


            return new DataTablesResponse<SYS202004ListViewModel>
            {
                draw = request.Draw,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data = data
            };
        }

        public async Task<Profess> GetProfessByIdAsync(int proNo)
        {
            var sql = @"
                SELECT [pro_no], [pro_code], [pro_name], [pro_english], [pro_order], [pro_isManager], [pro_status], [pro_createtime], [pro_createname], [pro_modifytime], [pro_modifyname] 
                FROM profess 
                WHERE pro_no = @proNo";
            var param = new SqlParameter("@proNo", proNo);
            return await _context.Profess.FromSqlRaw(sql, param).AsNoTracking().FirstOrDefaultAsync();
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

        public async Task<List<string>> GetSelectedPersonTypesAsync(int proNo)
        {
            var sql = @"
                    SELECT ptt_persontype 
                    FROM profess_persontype 
                    WHERE ptt_ptyno = @proNo";
            var param = new SqlParameter("@proNo", proNo);
            return await _context.Database.SqlQueryRaw<string>(sql, param).ToListAsync();
        }

        public async Task<bool> CodeExistsAsync(string code, int currentId)
        {
            var sql = @"
                    SELECT 1 AS Value 
                    FROM profess 
                    WHERE pro_code = @code AND pro_no != @currentId AND pro_status = '1'";
            var parameters = new[]
            {
                new SqlParameter("@code", code),
                new SqlParameter("@currentId", currentId)
            };
            var result = await _context.Database.SqlQueryRaw<int>(sql, parameters).FirstOrDefaultAsync();
            return result == 1;
        }

        public async Task CreateProfessAndAssociationsAsync(SYS202004UpdateViewModel model, string userName, IDataLogger dataLogger)
        {
            var newProfess = new Profess
            {
                ProCode = model.ProCode,
                ProName = model.ProName,
                ProEnglish = model.ProEnglish,
                ProOrder = model.ProOrder,
                ProIsManager = byte.Parse(model.ProIsManager),
                ProStatus = 1,
                ProCreateTime = DateTime.Now,
                ProCreateName = userName,
                ProModifyTime = DateTime.Now,
                ProModifyName = userName
            };

            await _context.Profess.AddAsync(newProfess);

            await _context.SaveChangesAsync();

            if (model.SelectedPersonTypes != null && model.SelectedPersonTypes.Any())
            {
                foreach (var svrCode in model.SelectedPersonTypes)
                {
                    var assoc = new ProfessPersonType
                    {
                        PttPtyNo = newProfess.ProNo,
                        PttPersonType = svrCode,
                        PttCreateTime = DateTime.Now,
                        PttCreateName = userName,
                        PttModifyTime = DateTime.Now,
                        PttModifyName = userName
                    };
                    await _context.ProfessPersonTypes.AddAsync(assoc);
                }
            }

            await SaveChangesAsync(dataLogger);
        }

        public async Task UpdateProfessAndAssociationsAsync(SYS202004UpdateViewModel model, string userName, IDataLogger dataLogger)
        {
            var profess = await _context.Profess.FirstOrDefaultAsync(x => x.ProNo == model.ProNo);

            if (profess != null)
            {
                profess.ProName = model.ProName;
                profess.ProEnglish = model.ProEnglish;
                profess.ProOrder = model.ProOrder;
                profess.ProIsManager = byte.Parse(model.ProIsManager);
                profess.ProModifyTime = DateTime.Now;
                profess.ProModifyName = userName;

                var oldAssociations = _context.ProfessPersonTypes.Where(x => x.PttPtyNo == model.ProNo);
                _context.ProfessPersonTypes.RemoveRange(oldAssociations);

                if (model.SelectedPersonTypes != null && model.SelectedPersonTypes.Any())
                {
                    foreach (var svrCode in model.SelectedPersonTypes)
                    {
                        var newAssoc = new ProfessPersonType
                        {
                            PttPtyNo = model.ProNo,
                            PttPersonType = svrCode,
                            PttCreateTime = DateTime.Now,
                            PttCreateName = userName,
                            PttModifyTime = DateTime.Now,
                            PttModifyName = userName
                        };
                        await _context.ProfessPersonTypes.AddAsync(newAssoc);
                    }
                }

                await SaveChangesAsync(dataLogger);
            }
        }

        public async Task SoftDeleteProfessAsync(int proNo, IDataLogger dataLogger)
        {
            var profess = await _context.Profess.FirstOrDefaultAsync(x => x.ProNo == proNo);
            if (profess != null)
            {
                profess.ProStatus = 2;
                await SaveChangesAsync(dataLogger);
            }
        }
    }
}