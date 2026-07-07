using DocumentFormat.OpenXml.Presentation;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Repositories;
using EHRIS.Core.Repositories.Event;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace EHRIS.Services.Common;

public class CommonService : ICommonService
{
    public string MasterKey => Environment.GetEnvironmentVariable("EHRISKey");
    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly IDeptRepository _deptRepository;
    private readonly IPTypeRepository _pTypeRepository;
    private readonly ISysVariableRepository _sysVarRepository;
    private readonly IEmailTemplateRepository _emailTemplateRepository;
    private readonly IMenuRepository _menuRepository;
    private readonly IFileExportService _fileExportService;
    private readonly IOperatesRepository _operatesRepository;
    private readonly IArgumentsRepository _argumentsRepository;


    public CommonService(
        IHttpContextAccessor httpContextAccessor
        , IPTypeRepository pTypeRepository
        , IDeptRepository deptRepository
        , IEmailTemplateRepository emailTemplateRepository
        , ISysVariableRepository sysVarRepository
        , IMenuRepository menuRepository
        , IFileExportService fileExportService
        , IOperatesRepository operatesRepository
        , IArgumentsRepository argumentsRepository
        )
    {
        _httpContextAccessor = httpContextAccessor;

        _pTypeRepository = pTypeRepository;
        _deptRepository = deptRepository;
        
        _emailTemplateRepository = emailTemplateRepository;
        _sysVarRepository = sysVarRepository;
        _menuRepository = menuRepository;

        _fileExportService = fileExportService;
        _operatesRepository = operatesRepository;

        _argumentsRepository = argumentsRepository;

    }

    #region 密碼
    /// <summary>
    /// GenerateMD5
    /// MD5加密
    /// </summary>
    /// <param name="input">要加密的字串</param>
    /// <returns></returns>
    public string GenerateMD5(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
    /// <summary>
    /// GenerateSHA256
    /// 使用SHA256加密
    /// </summary>
    /// <param name="input">要加密的字串</param>
    /// <returns></returns>
    public string GenerateSHA256(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = sha256.ComputeHash(inputBytes);
            return Convert.ToHexString(hashBytes); // SHA-256 加密
        }
    }
    #endregion

    #region 共同基礎資料查詢
    /// <summary>
    /// GetAllDeptList for 差勤
    /// </summary>
    /// <returns></returns>
    public async Task<List<DepartmentTree>> GetAllDeptList()
    {
        return await _deptRepository.GetAllDepartment();
    }
    /// <summary>
    /// GetAllUnitDeptList for 統計部門
    /// </summary>
    /// <returns></returns>
    public async Task<List<DepartmentTree>> GetAllUnitDeptList(int AccNo)
    {
        return await _deptRepository.GetAllUnitDepartment(AccNo);
    }
    /// <summary>
    /// GetAllUnitList for 統計機關
    /// </summary>
    /// <returns></returns>
    public async Task<List<UnitTree>> GetAllUnitList(int AccNo)
    {
        return await _deptRepository.GetAllUnit(AccNo);
    }

    /// <summary>
    /// 人員類別清單
    /// </summary>
    /// <param name="ptyNoList"></param>
    /// <returns></returns>
    public async Task<List<string>> GetPtypeNameListAsync(List<int> ptyNoList)
    {
        return await _pTypeRepository.GetPtypeNameListAsync(ptyNoList);
    }

    /// <summary>
    /// 部門單位清單
    /// </summary>
    /// <param name="depNoList"></param>
    /// <returns></returns>
    public async Task<List<string>> GetDepNameListAsync(List<int> depNoList)
    {
        return await _deptRepository.GetDepartmentNameListAsync(depNoList);
    }

    public async Task<List<string>> GetDepNameByUniIdListAsync(List<string> uniIDList)
    {
        return await _deptRepository.GetDepartmentNameByUniIdListAsync(uniIDList);
    }

    /// <summary>
    /// 功能名稱
    /// </summary>
    /// <param name="sfuNo"></param>
    /// <returns></returns>
    public async Task<(string SysNAME, string SfuMainName, string SfuName)> GetFunctionNamesAsync(int sfuNo)
    {
        return await _menuRepository.GetFunctionNamesAsync(sfuNo);
    }
    #endregion

    #region 忘記密碼
    /// <summary>
    /// GenerateToken 忘記密碼的token
    /// </summary>
    /// <returns></returns>
    public string GenerateToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
    /// <summary>
    /// GenerateVerificationCode 忘記密碼的驗證碼
    /// </summary>
    /// <param name="length">驗證碼長度</param>
    /// <returns></returns>
    public string GenerateVerificationCode(int length = 6)
    {
        const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var rnd = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[rnd.Next(s.Length)]).ToArray());
    }
    #endregion

    #region Email樣版及寫入message
    /// <summary>
    /// 依樣版ID取得郵件模版
    /// </summary>
    /// <param name="templateId"></param>
    /// <returns></returns>
    public async Task<EmailTemplateModel?> GetEmailTemplateAsync(int templateId)
    {
        var mailTemplate = await _emailTemplateRepository.GetEmailTemplateAsync(templateId);
        return mailTemplate;
    }
    /// <summary>
    /// 依參數取代內容中的變數
    /// </summary>
    /// <param name="body"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public string RenderTemplate(string body, Dictionary<string, string> parameters)
    {
        string result = body;
        foreach (var kv in parameters)
        {
            result = result.Replace($"{{{kv.Key}}}", kv.Value);
        }
        return result;
    }
    #endregion

    #region 系統參數
    public async Task<List<SysVariableModel>> GetSysVariableBySarVodeCodeAsync( string sarCode)
    {
        var sysVarModel = await _sysVarRepository.GetSysVariableBySarVodeCodeAsync(sarCode);
                            

        return sysVarModel
               .Select(x => new SysVariableModel
               {
                   SvrCode = x.SvrCode,
                   SarCode = x.SarCode,
                   SvrName = x.SvrName,
                   SvrOrder = x.SvrOrder,
                   SvrStatus = x.SvrStatus
               })
            .ToList();

    }

    public async Task<List<StaParamsViewModel>> GetStaParamsAsync(string stp_code)
    {
       return await _sysVarRepository.GetStaParamsAsync(stp_code);
    }
    #endregion

    #region 匯出
    //共用匯出的方法
    public IActionResult ExportFile(byte[] fileBytes, string fileExtension, string baseFileName = "統計表")
    {
        return _fileExportService.CreateDownloadFile(fileBytes, fileExtension, baseFileName);
    }
    #endregion

    #region 操作紀錄
    /// <summary>
    /// Web行為操作紀錄
    /// </summary>
    /// <returns></returns>
    private string ResolveActionMethod()
    {
        var method = _httpContextAccessor.HttpContext?.Request?.Method;

        // 如果有 HTTP Method，就直接用原本的字串
        if (!string.IsNullOrEmpty(method))
        {
            return method;
        }

        // 沒有 HTTP Context → 視為排程或背景執行
        return "Schedule";
    }

    public async Task LogOperateAsync(
        int peoUid,
        string depName,
        string proName,
        string peoName,
        int sfuNo,
        string sfuName,
        En_OperatorMode function,
        string memo)
    {
        string actionMethod = ResolveActionMethod();

        await _operatesRepository.ExecuteOperateAsync(
            peoUid,
            depName,
            proName,
            peoName,
            sfuNo,
            sfuName,
            function,
            actionMethod,
            memo
        );
    }

   

    public async Task<string> GetArgumentAsync(string argVariable, int depNo = 0)
    {
        return await _argumentsRepository.GetArgumentAsync(argVariable, depNo);
    }

    public async Task<List<string>> GetArgumentListAsync(string argVariable, int depNo = 0)
    {
        return await _argumentsRepository.GetArgumentListAsync(argVariable, depNo);
    }
    #endregion


}
