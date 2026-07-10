using EHRIS.Core.Models.Common;

namespace EHRIS.Security.Permission;

public interface IPermissionService
{
    /// <summary>
    /// 檢查該功能是否有權限
    /// </summary>
    /// <param name="sfuNo"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    bool HasPermission(int sfuNo, string action);

    /// <summary>
    /// 取得該帳號的功能權限清單
    /// </summary>
    /// <param name="acc_no"></param>
    /// <returns></returns>
    Task<List<UserFuncPermissionViewModel>> GetPermissionFunction(int acc_no);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="adu_no"></param>
    /// <returns></returns>
    Task<List<UserFuncPermissionViewModel>> GetAdminPermissionFunction(int adu_no);

    /// <summary>
    /// 取得該帳號的人員類別權限清單
    /// </summary>
    /// <param name="acc_no"></param>
    /// <returns></returns>
    Task<List<UserPtypePermissionViewModel>> GetPtypePermission(int acc_no);

    /// <summary>
    /// 依人員種類篩選出該帳號的人員類別權限清單 
    /// </summary>
    /// <param name="acc_no"></param>
    /// <param name="personTypes"></param>
    /// <returns></returns>
    Task<List<UserPtypePermissionViewModel>> GetPtypePermissionByPersonType(int acc_no, List<string> personTypes);

    /// <summary>
    /// 是否為全單位管理者
    /// </summary>
    /// <param name="acc_no"></param>
    /// <returns></returns>
    Task<bool> isSuperManByDep(int acc_no);
    /// <summary>
    /// 取得該帳號的單位權限清單
    /// </summary>
    /// <param name="acc_no"></param>
    /// <returns></returns>
    Task<List<int>> GetDepNoListByAuth(int acc_no);

    //下面未完成
    Task<bool> isSuperMan(int acc_no);
 
    Task<bool> isSuperManByPty(int acc_no);

    Task<List<int>> GetPtyNoListByAuth(int acc_no);

   
}
