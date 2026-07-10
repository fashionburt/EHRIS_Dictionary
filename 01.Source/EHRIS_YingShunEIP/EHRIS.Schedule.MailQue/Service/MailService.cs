using EHRIS.Core.DbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Schedule.MailQue.Service
{
    public class MailService : IMailService
    {
        private readonly ApplicationDbContext _db;

        public MailService(ApplicationDbContext db)
        {
            _db = db;
        }

        public void ExecuteTask()
        {
            // 這裡可以寫資料庫操作邏輯
            Console.WriteLine("執行資料庫任務");
        }
    }

}
