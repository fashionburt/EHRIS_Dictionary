using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models;
using EHRIS.Core.Repositories;
using EHRIS.Core.Repositories.Schedule;
using EHRIS.Services.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Services.Services.Schedule
{
    public class MailQueService : IMailQueService
    {
        //private readonly ApplicationDbContext _db;

        //public MailQueService(ApplicationDbContext db)
        //{
        //    _db = db;
        //}
        private readonly IUnitOfWork _unit; //共同交易模式元件
        private readonly IMailQueRepository _mailQueRepository;
        private readonly ICommonService _commonService;

        public MailQueService(IUnitOfWork unit, IMailQueRepository mailQueRepository, ICommonService commonService)
        {
            _unit = unit; 
            _mailQueRepository = mailQueRepository;
            _commonService = commonService;
        }

        public async Task<List<MailQueSendViewModel>> GetMailSendQueList()
        {
            return await _mailQueRepository.GetMailSendQueListAsync("0", 3); //未來會用參數
        }

        public async Task<(bool success, string message)> UpdateMailMessageAsync(int mai_no, string mai_status, string who)
        {

            try
            {
                //啟動交易模式
                await _unit.ExecuteTransactionAsync(async () =>
                {
                    var mailData = await _mailQueRepository.GetMailMessageAsync(mai_no);
                    if (mailData == null)
                        throw new InvalidOperationException("查無此寄件信箱");

                    mailData.MaiSendTime = DateTime.Now;
                    mailData.MaiStatus = mai_status;
                    mailData.MaiErrorTimes = 0;
                    mailData.MaiModifyName = who;
                    mailData.MaiModifyTime = DateTime.Now;

                    var result = await _mailQueRepository.UpdateMailMessageAsync(mailData);
                    if (!result)
                        throw new InvalidOperationException("更新失敗");

                    // 可選：寫入 Log
                    // await _commonService.WriteLogAsync(...);
                });

                return (true, "更新成功");
            }
            catch (InvalidOperationException ex)
            {
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                return (false, $"交易失敗：{ex.Message}");
            }

        }
    }
}
