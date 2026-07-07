using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Core.Repositories;

public abstract class EventOperatorRepository
{
   
    private readonly IEventObjectRepository _eventObjectRepository;
    private readonly List<EventObjectDto> _eventList = new();

    // public bool IsSave { get; protected set; }


    public int _ExecUid { get; protected set; }
    public int _ExecSfuNo { get; protected set; }
    public string _ExecProcName { get; protected set; }
    public EventMode _OperMode { get; protected set; }
    public WhoExec _Who { get; protected set; }
    public string _FromIP { get; protected set; }

    protected EventOperatorRepository(IEventObjectRepository repository, int uid, int sfuNo, EventMode mode, string fromIP, WhoExec who)
    {
        _eventObjectRepository = repository;
        _ExecUid = uid;
        _ExecSfuNo = sfuNo;
        _OperMode = mode;
        _Who = who;
        _FromIP = fromIP;


    }
    public void AddChange(string columndesc, string column, string orivalue, string newvalue)
    {
        var dto = new EventObjectDto();
        //var obj = new EventObjectRepository(_dbContext, dto); // ← 直接 new
        var objContent = _eventObjectRepository.AddChange(columndesc, column, orivalue, newvalue);
        _eventList[0].content.Add(objContent);
    }
    public async Task AddEvent(int uid, string tablename, string pk)
    {

        var dto = new EventObjectDto();
        //var obj = new EventObjectRepository(_dbContext, dto);
        var filled = await _eventObjectRepository.SetEventObject(uid, tablename, pk);
        _eventList.Add(filled);
    }

    public  async Task SaveAsync()
    {
        var entity = new EventOperatorDto
        {
            ExecUid = _ExecUid,
            ExecSfuNo = _ExecSfuNo,
            OperMode = _OperMode,
            Who = _Who,
            FromIP = _FromIP
        };

        await _eventObjectRepository.SaveAsync( _eventList, entity);



        
    }
}

