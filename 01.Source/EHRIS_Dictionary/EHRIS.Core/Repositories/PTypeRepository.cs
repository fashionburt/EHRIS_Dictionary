using EHRIS.Core.DbContext;
using EHRIS.Tools.DataBase;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Core.Repositories
{
    public class PTypeRepository : BaseRepository, IPTypeRepository
    {
        public PTypeRepository(ApplicationDbContext context) : base(context)
        {

        }
        public async Task<List<string>> GetPtypeNameListAsync(List<int> ptyNoList)
        {
            SqlQueryObject sqlObj = new SqlQueryObject();

            var sqlDept = @"SELECT pty_name  
                        FROM ptype
                        WHERE {0}";

            List<string> PTYNOStrList = new List<string>();

            string ptyNoStr = "";
            int idx = 0;
            foreach (int depNo in ptyNoList)
            {
                sqlObj.AddParameter(new SqlParameter("@PTYNO" + idx, depNo));
                PTYNOStrList.Add("@PTYNO" + idx.ToString());
                idx++;
            }

            ptyNoStr = $" ptype.pty_no IN ({string.Join(",", PTYNOStrList)})";
            sqlObj.Sql = string.Format(sqlDept, ptyNoStr);

            var ptyNameList = await SQLQueryAsync<string>(sqlObj, reader => reader["pty_name"].ToString());

            return ptyNameList;
        }
    }
}
