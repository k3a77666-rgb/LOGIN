using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace LOGIN.Data
{
    public class UtcDateInterceptor : DbCommandInterceptor
    {
        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            // Reemplazar TODAS las fechas con UTC
            foreach (DbParameter parameter in command.Parameters)
            {
                if (parameter.Value is DateTime dateTime && dateTime.Kind != DateTimeKind.Utc)
                {
                    parameter.Value = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                }
            }
            return base.ReaderExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            foreach (DbParameter parameter in command.Parameters)
            {
                if (parameter.Value is DateTime dateTime && dateTime.Kind != DateTimeKind.Utc)
                {
                    parameter.Value = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                }
            }
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }
    }
}