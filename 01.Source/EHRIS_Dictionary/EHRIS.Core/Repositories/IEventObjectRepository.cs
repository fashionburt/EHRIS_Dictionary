using EHRIS.Core.Entities;


namespace EHRIS.Core.Repositories
{
    public interface IEventObjectRepository
    {
       ChangeColumn AddChange(string columndesc, string column, string orivalue, string newvalue);
       Task SaveAsync(List<EventObjectDto> _eventList,EventOperatorDto _eventOperatorModel);
        Task<EventObjectDto> SetEventObject(int uid, string tablename, string tablePK);
    }
}
