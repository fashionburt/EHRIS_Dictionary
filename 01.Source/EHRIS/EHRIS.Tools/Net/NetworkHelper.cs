using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EHRIS.Tools.Net
{
    public static class NetworkHelper
    {
        /// <summary>
        /// 取得本機 IPv4 位址
        /// </summary>
        /// <returns>本機 IP (若找不到則回傳 127.0.0.1)</returns>
        public static string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1"; // fallback
        }
    }

}
