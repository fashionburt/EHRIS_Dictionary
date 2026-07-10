using System;
using System.Collections.Generic;
using System.Text;

namespace EHRIS.Core.Models.Event
{
   
    public interface IDataLogger
    {
        /// <summary>
        /// 操作人UID 
        /// </summary>
        int ExecUID { get; set; }

        /// <summary>
        /// 操作人功能編號
        /// 排程 設為 0
        /// </summary>
        int ExecSfuNo { get; set; }

        /// <summary>
        /// 操作程式/功能名稱
        /// 排程 設為 排程執行檔名稱
        /// </summary>
        string ExecProName { get; set; }

        /// <summary>
        /// 操作者類型
        /// 人為操作 = 10,
        /// 排程 = 20,
        /// WebAPI/Service = 30, 
        /// </summary>
        string ExecType { get;  }

        /// <summary>
        /// 操作人來源IP
        /// </summary>
        string ExecFromIP { get; set; }

        /// <summary>
        /// 被影響資料的關係人UID
        /// 若沒有填0
        /// </summary>
        int ToPeoUID { get; set; }

        /// <summary>
        /// 異動資料類型
        /// AddEvent 1 ModEvent 2 DelEvent 3
        /// </summary>
        En_DataEventMode EventType { get; set; }
    }
}
