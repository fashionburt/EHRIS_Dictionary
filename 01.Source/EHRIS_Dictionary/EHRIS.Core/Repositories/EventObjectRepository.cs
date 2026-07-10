using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EHRIS.Core.Repositories
{
    public class EventObjectRepository: BaseRepository,  IEventObjectRepository
    {
        public EventObjectDto _eventObject;
        private readonly AuditDbContext _auditDbContext;

        public EventObjectRepository(ApplicationDbContext context  , AuditDbContext auditDbContext) : base(context)
        {
            _eventObject = new EventObjectDto();
            _auditDbContext = auditDbContext;
        }
       
        public ChangeColumn AddChange(string columndesc, string column, string orivalue, string newvalue)
        {
            return new ChangeColumn
            {
                ColumnDesc = columndesc,
                Column = column,
                OriValue = orivalue,
                NewValue = newvalue
            };
        }

        public async Task<EventObjectDto> SetEventObject(int uid, string tablename, string tablePK)
        {
            _eventObject.PeoUID = uid;
            _eventObject.TableName = tablename;
            _eventObject.TablePK = tablePK;

            var dt = await _context.PersonDepartmentDtos
                        .FromSqlInterpolated($@"
                            SELECT baseperson.bas_name, departments.dep_name, 'profess' as pro_name 
                            FROM people
                            INNER JOIN baseperson on baseperson.bas_id = people.bas_id
                            INNER JOIN departments ON people.dep_no = departments.dep_no
                            WHERE people.peo_uid = {uid}").AsNoTracking()
                        .FirstOrDefaultAsync();

            if (dt != null)
            {
                _eventObject.DepName = dt.Dep_Name;
                _eventObject.PeoName = dt.bas_name;
                _eventObject.ProName = dt.Pro_Name;
            }

            return _eventObject;
        }
        public async Task SaveAsync(List<EventObjectDto> _eventList, EventOperatorDto _eventOperatorModel)
        {
            //_dbContext.EventLogs.Add(log);
            //await _dbContext.SaveChangesAsync();

            string ip = "排程或其它程式的操作";
            try { ip = "127.0.0.1"; } catch { }
            try
            {
                string remote_addr = "192.168.28.180";
                if (!string.IsNullOrWhiteSpace(remote_addr))
                    ip = ip + $" [{remote_addr}]";
            }
            catch { }
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {

                foreach (var evtObj in _eventList)
                {
                    string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(evtObj.content);

                    var mainEvent = new PersonalEvent
                    {
                        pev_execuid = _eventOperatorModel.ExecUid,
                        pev_exectime = DateTime.Now,
                        pev_execsfuno = _eventOperatorModel.ExecSfuNo,
                        pev_execprocname = _eventOperatorModel.ExecProcName,
                        pev_exectype = ((int)_eventOperatorModel.Who).ToString(),
                        pev_execipaddress = _eventOperatorModel.FromIP,
                        pev_uid = evtObj.PeoUID,
                        pev_depname = evtObj.DepName ?? "",
                        pev_peoname = evtObj.PeoName ?? "",
                        pev_proname = evtObj.ProName ?? "",
                        pev_eventtype = ((int)_eventOperatorModel.OperMode).ToString(),
                        pev_table = evtObj.TableName,
                        pev_pk = evtObj.TablePK,
                        pev_content = jsonContent
                    };

                    foreach (var col in evtObj.content)
                    {
                        mainEvent.Details.Add(new PersonalEventsDetail
                        {
                            pvt_coldesc = col.ColumnDesc,
                            pvt_column = col.Column,
                            pvt_orivalue = col.OriValue,
                            pvt_newvalue = col.NewValue
                        });
                    }

                        _auditDbContext.Personalevents.Add(mainEvent);
                }

                await _auditDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
            });
        }
    }
}
