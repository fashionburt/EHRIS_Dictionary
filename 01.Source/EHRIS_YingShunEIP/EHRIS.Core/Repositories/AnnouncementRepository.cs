using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;


namespace EHRIS.Core.Repositories
{
    public class AnnouncementRepository : BaseRepository, IAnnouncementRepository
    {
        public AnnouncementRepository(ApplicationDbContext context) : base(context)
        {

        }

        public async Task AddAnnouncements(Announcements announcement)
        {
            _context.Announcements.Add(announcement);
            _context.SaveChanges();

        }

        public async Task UpdateAnnouncements(Announcements announcement)
        {
            _context.Announcements.Update(announcement);
            _context.SaveChanges();

        }

        public async Task DeleteAnnouncements(int id)
        {
            var entity = _context.Announcements.Find(id);
            if (entity != null)
            {
                _context.Announcements.Remove(entity);
                _context.SaveChanges();
            }

        }

        public async Task<Announcements> GetAnnouncementsById(int id)
        {
            return _context.Announcements.Find(id);

        }

        public async Task<List<Announcements>> GetAnnouncements(int page = 1, int pageSize = 10)
        {
            return await _context.Announcements.AsNoTracking()
                .OrderByDescending(a => a.AnnDate)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync() ?? new List<Announcements>();


        }


    }
}
