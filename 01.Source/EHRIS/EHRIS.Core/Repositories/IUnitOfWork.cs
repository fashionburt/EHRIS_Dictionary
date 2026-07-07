using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Core.Repositories
{
    public interface IUnitOfWork
    {
        /// <summary>
        /// 啟動交易模式
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        Task ExecuteTransactionAsync(Func<Task> operation);
        Task<int> CommitAsync();

        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }

}
