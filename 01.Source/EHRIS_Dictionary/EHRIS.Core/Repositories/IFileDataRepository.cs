using EHRIS.Core.Entities;

namespace EHRIS.Core.Repositories
{
    public interface IFileDataRepository
    {
        Task AddFileData(FileData fileData);
        Task UpdateFileData(FileData fileData);
        Task<FileData?> GetByStoredName(string storedName);
        Task<List<FileData>> GetFileListByUid(int filUid);
        Task<List<FileData>> GetFileListByIds(List<Guid> filIds);
    }
}
