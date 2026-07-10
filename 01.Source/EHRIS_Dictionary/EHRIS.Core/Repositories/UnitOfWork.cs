using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Core.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        //public void SetEventOperator(EventOperatorDto dto)
        //{
        //    _context.SetEventOperator(dto);
        //}
         
        public async Task ExecuteTransactionAsync(Func<Task> operation)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await BeginTransactionAsync();
                try
                {
                    await operation();
                    await CommitTransactionAsync();
                }
                catch
                {
                    await RollbackTransactionAsync();
                    throw;
                }
            });

        }

        /// <summary>
        /// 暫留
        /// </summary>
        /// <returns></returns>
        public Task<int> CommitAsync() => _context.SaveChangesAsync();

        /// <summary>
        /// 新加操作紀錄
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }


        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

    }
}
