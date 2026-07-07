using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EHRIS.Core.Models.Event
{
    /// <summary>
    /// 資料異動狀態
    /// </summary>
    public enum En_DataEventMode
    {
        [DescriptionAttribute("AddEvent")]
        AddEvent = 10,
        [DescriptionAttribute("ModEvent")]
        ModEvent = 20,
        [DescriptionAttribute("DelEvent")]
        DelEvent = 30,

    }
    /// <summary>
    /// 行為操作狀態
    /// 二代差勤的狀態1:新增 2:查詢 3:更新 4:刪除 5:保留 6:操作訊息
    /// </summary>
    public enum En_OperatorMode
    {
        新增 = 10,
        申請 = 11,

        更新 = 20,
        密碼變更 = 21,

        刪除 = 30,

        查詢 = 40,
        
        操作訊息 = 60,

        登入 = 70,
        登出 = 71,        
    }
}
