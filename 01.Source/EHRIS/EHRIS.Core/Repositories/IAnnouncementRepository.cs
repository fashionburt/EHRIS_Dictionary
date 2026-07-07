using EHRIS.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EHRIS.Core.Repositories
{
    public interface IAnnouncementRepository
    {
        Task<List<Announcements>> GetAnnouncements(int page = 1, int pageSize = 10);
        Task<Announcements> GetAnnouncementsById(int id);
        Task AddAnnouncements(Announcements announcement);
        Task UpdateAnnouncements(Announcements announcement);
        Task DeleteAnnouncements(int id);
    }

}
