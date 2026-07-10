using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EHRIS.Core.Repositories
{
    public class FileDataRepository : BaseRepository, IFileDataRepository
    {
        public FileDataRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task AddFileData(FileData fileData)
        {
            _context.FileData.Add(fileData);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateFileData(FileData fileData)
        {
            _context.FileData.Update(fileData);
            await _context.SaveChangesAsync();
        }

        public async Task<FileData?> GetByStoredName(string storedName)
        {
            return await _context.FileData
                .FirstOrDefaultAsync(f => f.FilFileName == storedName && f.FilStatus == 1);
        }

        public async Task<List<FileData>> GetFileListByUid(int filUid)
        {
            return await _context.FileData
                .AsNoTracking()
                .Where(f => f.FilUid == filUid && f.FilStatus == 1)
                .OrderBy(f => f.FilCreateTime)
                .ToListAsync();
        }

        public async Task<List<FileData>> GetFileListByIds(List<Guid> filIds)
        {
            return await _context.FileData
                .AsNoTracking()
                .Where(f => filIds.Contains(f.FilId) && f.FilStatus == 1)
                .OrderBy(f => f.FilCreateTime)
                .ToListAsync();
        }
    }
}
