using EHRIS.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EHRIS.Services.Services
{
    public interface IAnnouncementService
    {
        Task<List<Announcements>> GetAnnouncements(int page = 1, int pageSize = 10);

    }
}
