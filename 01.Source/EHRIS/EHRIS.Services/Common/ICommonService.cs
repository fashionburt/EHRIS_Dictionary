using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Event;
using Microsoft.AspNetCore.Mvc;

namespace EHRIS.Services.Common;

public interface ICommonService
{
    string MasterKey { get; } 
    string GenerateMD5(string input); // MD5 加密
    string GenerateSHA256(string input); // SHA256 加密
    //string FormatDate(DateTime date, string format); // 格式化日期
    Task<List<DepartmentTree>> GetAllDeptList();
    Task<List<DepartmentTree>> GetAllUnitDeptList(int AccNo);
    Task<List<UnitTree>> GetAllUnitList(int AccNo);
    string GenerateToken();
    string GenerateVerificationCode(int length = 6);

    Task<EmailTemplateModel?> GetEmailTemplateAsync(int templateId);
    string RenderTemplate(string body, Dictionary<string, string> parameters);
    Task<List<SysVariableModel>> GetSysVariableBySarVodeCodeAsync(string sarCode);

    Task<List<StaParamsViewModel>> GetStaParamsAsync(string stp_code);

  

    /// <summary>
    /// 取得系統參數值（優先序：排程→部門→全域→預設值）
    /// </summary>
    Task<string> GetArgumentAsync(string argVariable, int depNo = 0);

    /// <summary>
    /// 取得系統參數值（拆成陣列，依 arg_source=SPLITTEXT 時的 arg_splitchar 拆分）
    /// </summary>
    Task<List<string>> GetArgumentListAsync(string argVariable, int depNo = 0);
    

    /// <summary>
    /// 取得人員類別名稱
    /// </summary>
    /// <param name="ptyNoList"></param>
    /// <returns></returns>
    Task<List<string>> GetPtypeNameListAsync(List<int> ptyNoList);
    /// <summary>
    /// 取得單位名稱
    /// </summary>
    /// <param name="depNoList"></param>
    /// <returns></returns>
    Task<List<string>> GetDepNameListAsync(List<int> depNoList);

    Task<List<string>> GetDepNameByUniIdListAsync(List<string> uniIDList); 

    /// <summary>
    /// 取得功能Breadcrumb 路徑
    /// </summary>
    /// <param name="sfuNo"></param>
    /// <returns></returns>
    Task<(string SysNAME, string SfuMainName, string SfuName)> GetFunctionNamesAsync(int sfuNo);

    IActionResult ExportFile(byte[] fileBytes, string fileExtension, string baseFileName = "統計表");

    /// <summary>
    /// 行為操作紀錄
    /// </summary>
    /// <param name="peoUid">操作人編號</param>
    /// <param name="depName">操作人單位</param>
    /// <param name="proName">操作人職稱</param>
    /// <param name="peoName">操作人姓名</param>
    /// <param name="sfuNo">功能編號</param>
    /// <param name="sfuName">功能名稱</param>
    /// <param name="function">操作行為</param>
    /// <param name="actionMethod">網站方法</param>
    /// <param name="memo">操作訊息</param>
    /// <returns></returns>
    Task LogOperateAsync(int peoUid, string depName, string proName, string peoName,
                                     int sfuNo, string sfuName, En_OperatorMode function, string memo);
}
