using EHRIS.Core.Entities;
using EHRIS.Core.Models.Event;
using System;
using System.Collections.Generic;
using System.Text;

namespace EHRIS.Core.Repositories.Event
{
    public interface IOperatesRepository
    {
        //批次處理
        //void BatchStart();
        //Task AddOperateAsync(int peoUid, string depName, string proName, string peoName,
        //                     int sfuNo, string sfuName, En_OperatorMode function, string actionid, string memo);

        //Task<int> BatchSaveAsync();
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
        Task<int> ExecuteOperateAsync(int peoUid, string depName, string proName, string peoName,
                                      int sfuNo, string sfuName, En_OperatorMode function, string actionid, string memo);
    }
}
