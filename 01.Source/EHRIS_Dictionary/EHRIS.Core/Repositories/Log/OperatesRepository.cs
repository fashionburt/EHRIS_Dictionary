using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.Event;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace EHRIS.Core.Repositories.Event
{
    public class OperatesRepository : IOperatesRepository
    {
        private readonly AuditDbContext _auditContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private bool _isBatch = false;
        //private readonly List<OperateDto> _batchList = new();

        public OperatesRepository(AuditDbContext auditContext, IHttpContextAccessor httpContextAccessor)
        {
            _auditContext = auditContext;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetUserIp()
        {
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            return string.IsNullOrEmpty(ip) ? "排程或其它程式的操作" : ip;
        }

        //public void BatchStart()
        //{
        //    _isBatch = true;
        //    _batchList.Clear();
        //}

        //public async Task AddOperateAsync(int peoUid, string depName, string proName, string peoName,
        //                                  int sfuNo, string sfuName, En_OperatorMode function, string actionMethod, string memo)
        //{
        //    if (!_isBatch)
        //        throw new InvalidOperationException("尚未啟用批次處理");

        //    var dto = new OperateDto
        //    {
        //        PeoUid = peoUid,
        //        DepName = depName,
        //        ProName = proName,
        //        PeoName = peoName,
        //        SfuNo = sfuNo,
        //        SfuName = sfuName,
        //        Function = (int)function,
        //        ActionMethod = actionMethod,
        //        Memo = memo,
        //        IpAddress = GetUserIp()
        //    };

        //    _batchList.Add(dto);
        //}

        //public async Task<int> BatchSaveAsync()
        //{
        //    if (!_isBatch)
        //        throw new InvalidOperationException("尚未啟用批次處理");

        //    if (_batchList.Count == 0)
        //        return 0;

        //    int result = await _context.ExecuteBatchOperatesSP(_batchList);

        //    _isBatch = false;
        //    _batchList.Clear();

        //    return result;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="peoUid">操作人編號</param>
        /// <param name="depName">操作人單位</param>
        /// <param name="proName">操作人職稱</param>
        /// <param name="peoName">操作人姓名</param>
        /// <param name="sfuNo">功能編號</param>
        /// <param name="sfuName">功能名稱</param>
        /// <param name="function">操作行為</param>
        /// <param name="actionMethod">網站方法</param>
        /// <param name="memo">操作訊息</param>
        /// <returns></returns>
        public async Task<int> ExecuteOperateAsync(int peoUid, string depName, string proName, string peoName,
                                                   int sfuNo, string sfuName, En_OperatorMode function, string actionMethod, string memo)
        {
            return await _auditContext.ExecuteOperatesSP(peoUid, depName, proName, peoName,
                                                    sfuNo, sfuName, (int)function, actionMethod, memo, GetUserIp());
        }
    }

}
