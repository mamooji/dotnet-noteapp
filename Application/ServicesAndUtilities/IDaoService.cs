using System.Data;
using Application.Common.Interfaces;

namespace Application.ServicesAndUtilities;

public interface IDaoService
{
    IApplicationDbContext IApplicationDbContext { get; }

    Task<DataTable> FillDataTableUsingInlineSql(string sql, CancellationToken cancellationToken,
        params IDataParameter[] parameters);

    Task<DataTable> FillDataTableUsingProcedure(string procedure, CancellationToken cancellationToken,
        params IDataParameter[] parameters);

    IDataParameter CreateParameter(string parameterName, object parameterValue);

    Task<List<T>> FillListUsingInlineSql<T>(string sql, CancellationToken cancellationToken,
        params IDataParameter[] parameters) where T : class, new();

    Task<List<T>> FillListFromProcedure<T>(string procedure, CancellationToken cancellationToken,
        params IDataParameter[] parameters) where T : class, new();

    Task<List<T>> GetImmutableData<T>(string sql, CancellationToken cancellationToken);

    Task<int> ExecuteNonQueryUsingInlineSql(string sql, CancellationToken cancellationToken,
        params IDataParameter[] parameters);

    Task<int> ExecuteNonQueryUsingProcedure(string procedure, CancellationToken cancellationToken,
        params IDataParameter[] parameters);

    Task<object> ExecuteScalarUsingInlineSql(string sql, CancellationToken cancellationToken,
        params IDataParameter[] parameters);

    Task<object> ExecuteScalarUsingStoredProcedure(string procedureName, CancellationToken cancellationToken,
        params IDataParameter[] parameters);

    Task<DataTable> GetTableTypeSchema(string tableType, CancellationToken cancellationToken);

    Task<List<T>> GetImmutableData<T>(string sp, CommandType commandType, CancellationToken cancellationToken,
        params IDataParameter[] parameters);
}