using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Tools.Logging
{
    public class DatabaseLogSink : ILogSink
    {
        private readonly string _connectionString;

        public DatabaseLogSink(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("LogConnection");
        }

        public async Task WriteAsync(LogEvent logEvent, LogLevel level, string message, Exception? ex = null)
        {
            //using var conn = new SqlConnection(_connectionString);
            //using var cmd = conn.CreateCommand();
            //cmd.CommandText = @"
            //INSERT INTO Logs (Timestamp, Level, Message, Module, Action, UserId, RequestId, Exception)
            //VALUES (@ts, @lvl, @msg, @mod, @act, @uid, @rid, @ex)";
            //cmd.Parameters.AddWithValue("@ts", DateTime.UtcNow);
            //cmd.Parameters.AddWithValue("@lvl", level.ToString());
            //cmd.Parameters.AddWithValue("@msg", message);
            //cmd.Parameters.AddWithValue("@mod", logEvent.Module);
            //cmd.Parameters.AddWithValue("@act", logEvent.Action);
            //cmd.Parameters.AddWithValue("@uid", logEvent.UserId);
            //cmd.Parameters.AddWithValue("@rid", logEvent.RequestId);
            //cmd.Parameters.AddWithValue("@ex", ex?.ToString() ?? "");

            //await conn.OpenAsync();
            //await cmd.ExecuteNonQueryAsync();
        }
    }
}
