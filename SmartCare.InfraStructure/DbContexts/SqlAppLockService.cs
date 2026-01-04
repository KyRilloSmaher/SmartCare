using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartCare.Application.commens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.InfraStructure.DbContexts
{
    public class SqlLockManager : ISqlLockManager
    {
        private readonly ApplicationDBContext _context;

        public SqlLockManager(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<IAsyncDisposable> AcquireLockAsync(string resource, string mode = "Exclusive", int timeoutMs = 10000)
        {
            var connection = _context.Database.GetDbConnection() as SqlConnection;

            if (connection == null)
            {
                throw new InvalidOperationException("Cannot get SQL connection from DbContext.");
            }

            // Ensure connection is open
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            using var command = new SqlCommand("sp_getapplock", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = (timeoutMs / 1000) + 30
            };

            command.Parameters.AddWithValue("@Resource", resource);
            command.Parameters.AddWithValue("@LockMode", mode);
            command.Parameters.AddWithValue("@LockOwner", "Session");
            command.Parameters.AddWithValue("@LockTimeout", timeoutMs);

            var returnValue = new SqlParameter
            {
                ParameterName = "@Result",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.ReturnValue
            };
            command.Parameters.Add(returnValue);

            await command.ExecuteNonQueryAsync();

            var result = (int)returnValue.Value;
            if (result < 0)
            {
                throw new Exception($"Failed to acquire lock for resource '{resource}'. SP result: {result}");
            }

            return new AsyncDisposableAction(async () =>
            {
                using var releaseCommand = new SqlCommand("sp_releaseapplock", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                releaseCommand.Parameters.AddWithValue("@Resource", resource);
                releaseCommand.Parameters.AddWithValue("@LockOwner", "Session");

                var releaseResult = new SqlParameter
                {
                    ParameterName = "@Result",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.ReturnValue
                };
                releaseCommand.Parameters.Add(releaseResult);

                await releaseCommand.ExecuteNonQueryAsync();

                // Optionally close connection if you want to manage it here
                // But typically EF Core manages it
                // await connection.CloseAsync();
            });
        }

        private class AsyncDisposableAction : IAsyncDisposable
        {
            private readonly Func<Task> _disposeAction;
            public AsyncDisposableAction(Func<Task> disposeAction) => _disposeAction = disposeAction;
            public async ValueTask DisposeAsync() => await _disposeAction();
        }
    }
}