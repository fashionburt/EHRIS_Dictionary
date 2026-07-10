using System.ComponentModel;

namespace EHRIS.Core.Models.Common;

/// <summary>
/// 權限方式（根範圍策略）。
/// 目前：Automa = 最大權限（不卡權限，＝元件現行行為）；All 亦為不過濾。
/// Self / Parallel / Auth 已定義、實作保留；Limit / AgentForMe2 留位置，暫不實作。
/// </summary>
public enum NodeType : int
{
    [Description("全部")]
    All,
    [Description("自己部門")]
    Self,
    [Description("平行部門")]
    Parallel,
    [Description("查詢權限表")]
    Auth,
    [Description("自動判斷")]
    Automa,
    [Description("依職務代理限制")]
    Limit,
    [Description("我是別人職務代理人")]
    AgentForMe2
}

/// <summary>
/// 權限類型（已定義，權責邏輯待接）。
/// </summary>
public enum AuthType : int
{
    [Description("全部")]
    All = 0,
    [Description("單位")]
    Department = 1,
    [Description("人員類別")]
    PersonalType = 2
}

/// <summary>
/// 人員狀態，依 people.peo_jobtype（1=在職、2=離職）。
/// </summary>
public enum PeopleStatus : int
{
    All = 0,      // 全部（peo_jobtype in (1,2)）
    OnJob = 1,    // 在職（peo_jobtype = 1）
    StopJob = 2   // 非在職 / 離職（peo_jobtype = 2）
}

/// <summary>
/// 是否顯示自己（False 時排除登入者）。
/// </summary>
public enum PeopleShowSelf : int
{
    False = 0,
    True = 1
}
