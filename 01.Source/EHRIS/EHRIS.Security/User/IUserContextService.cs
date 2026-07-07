using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Security.User
{
    public interface IUserContextService
    {
        /// <summary>
        /// Account.AccNo
        /// </summary>
        int AccNO { get; }

        /// <summary>
        /// 登入者帳號
        /// </summary>
        string UserAccount { get; }
         
        /// <summary>
        /// 登入者職員編號
        /// </summary>
        int PeoUID { get; }

        /// <summary>
        /// 人員姓名
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// 是否登入
        /// </summary>
        bool IsAuthenticated { get; }

        #region 差勤必要資訊
        /// <summary>
        /// 最剛開始登錄的人(切換帳號用)
        /// </summary>
        int? TopChangeLoginUID { get; }
        /// <summary>
        /// 登入者第一層單位編號
        /// </summary>
        int UserDepartmentLv1NO { get; }
        /// <summary>
        /// 登入者所在單位編號
        /// </summary>
        int UserDepartmentNO { get; }
        /// <summary>
        /// 登入者所在單位名稱
        /// </summary>
        string UserDepartmentName { get; }
        /// <summary>
        /// 登入者人員類別編號
        /// </summary>
        int UserPtyNO { get; }
        /// <summary>
        /// 登入者人員類別
        /// </summary>
        string UserPtyName { get; }
        /// <summary>
        /// 登入者職稱編號
        /// </summary>
        int UserProfessNO { get; }
        /// <summary>
        /// 登入者職稱
        /// </summary>
        string UserProfessName { get; }
        #endregion

        /// <summary>
        /// 登入者所在單位機關
        /// </summary>
        string UserUnitName { get; }

        /// <summary>
        /// 來源IP
        /// </summary>
        string SourceIP { get; }

        #region 後台系統
        /// <summary>
        /// 後台管理者編號
        /// </summary>
        int AduNo { get; }
        /// <summary>
        /// 後台管理者位階編號 (來自加密映射)
        /// </summary>
        int AdlNo { get; }
        /// <summary>
        /// 後台管理者等級
        /// </summary>
        int AdlRank { get; }
        #endregion
    }
}
