using EHRIS.Core.DbContext;
using EHRIS.Core.Models.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Core.Repositories
{
    public class PermissionsRepository : BaseRepository, IPermissionsRepository
    {
        public PermissionsRepository(ApplicationDbContext dbContext) : base(dbContext)
        {

        }

        /// <summary>
        /// 取得該帳號所有可以新增、修改、刪除、查詢的功能權限
        /// </summary>
        /// <param name="acc_no"></param>
        /// <returns></returns>
        public async Task<List<UserFuncPermissionViewModel>> GetFunctionPermissionByAccount(int acc_no)
        {
            //之後要改成SQL
            var rolNoList = _context.RoleAccounts
                            .AsNoTracking()
                            .Where(rac => rac.AccNo == acc_no)
                            .Select(rac => rac.RolNo);

            //角色權限
            var rolePermissions = _context.RAuthoritys
                                  .AsNoTracking()
                                 .Where(rau => rolNoList.Contains(rau.RolNo))
                                 .Select(rau => new
                                 {
                                     rau.SfuNo,
                                     CanCreate = rau.RauIns > 0,
                                     CanUpdate = rau.RauEdi > 0,
                                     CanDelete = rau.RauDel > 0
                                 });
            //個人特殊權限
            var accountPermissions = _context.AAuthoritys
                                    .AsNoTracking()
                                    .Where(aau => aau.AccNo == acc_no)
                                    .Select(aau => new
                                    {
                                        aau.SfuNo,
                                        CanCreate = aau.AauIns > 0,
                                        CanUpdate = aau.AauEdi > 0,
                                        CanDelete = aau.AauDel > 0
                                    });

            // 合併權限並彙整
            var mergedPermissions = rolePermissions
                                    .Concat(accountPermissions)
                                    .GroupBy(p => p.SfuNo)
                                    .Select(g => new UserFuncPermissionViewModel
                                    {
                                        SfuNo = g.Key,
                                        CanQuery = true,
                                        CanCreate = g.Any(x => x.CanCreate),
                                        CanUpdate = g.Any(x => x.CanUpdate),
                                        CanDelete = g.Any(x => x.CanDelete)
                                    })
                                    .ToList();

            return mergedPermissions;
        }

        public async Task<List<UserFuncPermissionViewModel>> GetFunctionAdminPermissionByAccount(int adu_no)
        {
            //之後要改成SQL
            var rolNoList = _context.AdminUserRoles
                            .AsNoTracking()
                            .Where(rac => rac.AduNo == adu_no)
                            .Select(rac => rac.AdrNo);

            var rolePermissions = _context.AdminRoleFunctions
                                  .AsNoTracking()
                                 .Where(rau => rolNoList.Contains(rau.AdrNo))
                                 .Select(rau => new
                                 {
                                     rau.AdfNo,
                                     CanCreate = rau.ArfCanCreate,
                                     CanUpdate = rau.ArfCanEdit,
                                     CanDelete = rau.ArfCanDelete
                                 });

            // 合併權限並彙整
            var mergedPermissions = rolePermissions 
                                    .GroupBy(p => p.AdfNo)
                                    .Select(g => new UserFuncPermissionViewModel
                                    {
                                        SfuNo = g.Key,
                                        CanQuery = true,
                                        CanCreate = g.Any(x => x.CanCreate),
                                        CanUpdate = g.Any(x => x.CanUpdate),
                                        CanDelete = g.Any(x => x.CanDelete)
                                    })
                                    .ToList();

            return mergedPermissions;
        }

        /// <summary>
        /// 取得該帳號所有可以查詢的人員類別權限
        /// </summary>
        /// <param name="acc_no"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<List<UserPtypePermissionViewModel>> GetPtypePermissionByAccount(int acc_no)
        {
            var ptynoList =  _context.Mtype
                            .AsNoTracking()
                            .Where(m => m.AccNo == acc_no)
                            .Join(
                                _context.PType.AsNoTracking(),
                                m => m.PtyNo,
                                p => p.PtyNo,
                                (m, p) => new { m, p }
                            )
                            .Where(x => x.p.PtyStatus == 1)
                            .OrderBy(x => x.p.PtyOrder)
                            .Select(x => new UserPtypePermissionViewModel
                            {
                                PtyNo = x.m.PtyNo,
                                PtyCode = x.p.PtyCode,
                                PtyName = x.p.PtyName,
                            })

                            .ToList();

            return  ptynoList;
        }

        public async Task<List<UserPtypePermissionViewModel>> GetPtypePermissionByPersonType(int acc_no, List<string> personTypes)
        {
            var plist = (from p in _context.PtypePersonType.AsNoTracking()
                         where personTypes.Contains(p.PttPersonType)
                         select p.PttPtyNo).Distinct();

            var ptynoList = (from mtp in _context.Mtype.AsNoTracking()
                             join pty in _context.PType.AsNoTracking() on mtp.PtyNo equals pty.PtyNo
                             where mtp.AccNo == acc_no && pty.PtyStatus.Equals("1") && plist.Contains(pty.PtyNo)
                             orderby pty.PtyOrder
                             select new UserPtypePermissionViewModel
                             {
                                 PtyNo = mtp.PtyNo,
                                 PtyCode = pty.PtyCode,
                                 PtyName = pty.PtyName,
                             }).ToList();



            return ptynoList;
        }

        public async Task<bool> isSuperManByDep(int acc_no)
        {
            bool result = true;
            var supData = (from sup in _context.Supervise.AsNoTracking()
                           where sup.AccNo == acc_no
                           select sup).FirstOrDefault();
            if (supData != null)
            {
                result = result && (supData.SupType == "1");
            }
            else
                result = false;

            return result;
        }

        public async Task<List<int>> GetDepNoPermissionByAccount(int acc_no)
        {
            var supData = (from sup in _context.Supervise.AsNoTracking()
                           where sup.AccNo == acc_no
                           select sup).FirstOrDefault();
            int[] dataList = null;
            if (supData != null)
            {
                if (supData.SupType == "2")
                {
                    dataList = (from d in _context.DepartmentCategory.AsNoTracking()
                                where d.SupNo == supData.SupNo
                                select d.DepNo).ToArray();
                }
            }

            if (dataList != null)
            {
                if (dataList.Count() == 0)
                    return null;
                else
                    return dataList.ToList();
            }
            else
                return null;
        }


    }
}
