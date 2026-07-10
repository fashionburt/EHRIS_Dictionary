using System;
using System.Collections.Generic;
using System.Text;

namespace EHRIS.Tools.Web
{
    public static class PathHelper
    {
        public static string ResolvePath(string relativePath, string? baseDir = null)
        {
            // 如果呼叫者有指定 baseDir (Web 專案傳 env.ContentRootPath)
            if (!string.IsNullOrEmpty(baseDir))
            {
                return Path.Combine(baseDir, CleanPath(relativePath));
            }

            // 非 Web 環境 → 使用執行檔所在目錄
            return Path.Combine(AppContext.BaseDirectory, CleanPath(relativePath));
        }

        private static string CleanPath(string relativePath)
        {
            return relativePath.TrimStart('~', '/')
                               .Replace('/', Path.DirectorySeparatorChar);
        }
    }
}
