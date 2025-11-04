using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartCare.Application.Commons;
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
                var connection = (SqlConnection)_context.Database.GetDbConnection();
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_getapplock", connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@Resource", resource);
                command.Parameters.AddWithValue("@LockMode", mode);
                command.Parameters.AddWithValue("@LockOwner", "Session");
                command.Parameters.AddWithValue("@LockTimeout", timeoutMs);

                await command.ExecuteNonQueryAsync();

                return new AsyncDisposableAction(async () =>
                {
                    using var releaseCommand = new SqlCommand("sp_releaseapplock", connection)
                    {
                        CommandType = System.Data.CommandType.StoredProcedure
                    };
                    releaseCommand.Parameters.AddWithValue("@Resource", resource);
                    releaseCommand.Parameters.AddWithValue("@LockOwner", "Session");
                    await releaseCommand.ExecuteNonQueryAsync();

                    await connection.CloseAsync();
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