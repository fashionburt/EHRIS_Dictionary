using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace EHRIS.Services.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IAnnouncementRepository _repository;

        public AnnouncementService(IAnnouncementRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Announcements>> GetAnnouncements(int page = 1, int pageSize = 10)
        {
            return await _repository.GetAnnouncements(page, pageSize);
        }

    }
}
