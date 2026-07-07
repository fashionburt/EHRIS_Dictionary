using EHRIS.Core.Entities;

namespace EHRIS.Core.Repositories
{
    public interface IArgumentsRepository
    {
        /// <summary>
        /// 取得系統參數值（優先序：排程→部門→全域→預設值）
        /// </summary>
        Task<string> GetArgumentAsync(string argVariable, int depNo = 0);

        /// <summary>
        /// 取得系統參數值（拆成陣列，依 arg_source=SPLITTEXT 時的 arg_splitchar 拆分）
        /// </summary>
        Task<List<string>> GetArgumentListAsync(string argVariable, int depNo = 0);
    }
}
