using System.Data;
using Application.Common.Interfaces;
using Application.ServicesAndUtilities;
using Domain.Utility;
using Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services.Dao;

public class DaoService : IDaoService
{
    private readonly string _connectionString;
    private readonly IMapperUtility _mapper;
    private readonly IServiceScopeFactory _scopeFactory;


    public DaoService(IServiceScopeFactory scopeFactory, IApplicationDbContext context, IMapperUtility mapper)
    {
        _scopeFactory = scopeFactory;
        _mapper = mapper;
        _connectionString = context.ConnectionString;
    }

    public IApplicationDbContext IApplicationDbContext
    {
        get
        {
            var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
            return context;
        }
    }

    public async Task<DataTable> FillDataTableUsingInlineSql(string sql, CancellationToken cancellationToken,
        params IDataParameter[] parameters)
    {
        return await FillDataTable(sql, CommandType.Text, cancellationToken, parameters);
    }

    public async Task<DataTable> FillDataTableUsingProcedure(string procedure, CancellationToken cancellationToken,
        params IDataParameter[] parameters)
    {
        return await FillDataTable(procedure, CommandType.StoredProcedure, cancellationToken, parameters);
    }

    public async Task<List<T>> FillListUsingInlineSql<T>(string sql, CancellationToken cancellationToken,
        params IDataParameter[] parameters) where T : class, new()
    {
        return await FillList<T>(sql, CommandType.Text, cancellationToken, parameters);
    }

    public async Task<List<T>> FillListFromProcedure<T>(string procedure, CancellationToken cancellationToken,
        params IDataParameter[] parameters) where T : class, new()
    {
        return await FillList<T>(procedure, CommandType.StoredProcedure, cancellationToken, parameters);
    }

    public IDataParameter CreateParameter(string parameterName, object parameterValue)
    {
        return new SqlParameter { ParameterName = parameterName, Value = parameterValue };
    }

    public async Task<int> ExecuteNonQueryUsingInlineSql(string sql, CancellationToken cancellationToken,
        params IDataParameter[] parameters)
    {
        return await ExecuteNonQuery(sql, CommandType.Text, cancellationToken, parameters);
    }

    public async Task<int> ExecuteNonQueryUsingProcedure(string procedure, CancellationToken cancellationToken,
        params IDataParameter[] parameters)
    {
        return await ExecuteNonQuery(procedure, CommandType.StoredProcedure, cancellationToken, parameters);
    }

    public async Task<object> ExecuteScalarUsingInlineSql(string sql, CancellationToken cancellationToken,
        params IDataParameter[] parameters)
    {
        return await ExecuteScalar(sql, false, cancellationToken, parameters);
    }

    public async Task<object> ExecuteScalarUsingStoredProcedure(string procedureName,
        CancellationToken cancellationToken, params IDataParameter[] parameters)
    {
        return await ExecuteScalar(procedureName, true, cancellationToken, parameters);
    }

    public async Task<DataTable> GetTableTypeSchema(string tableType, CancellationToken cancellationToken)
    {
        var sql = $"DECLARE @T {tableType}; SELECT TOP 1 * FROM @T";
        return await FillDataTable(sql, CommandType.Text, cancellationToken);
    }

    public async Task<List<T>> GetImmutableData<T>(string sp, CommandType commandType,
        CancellationToken cancellationToken, params IDataParameter[] parameters)
    {
        var list = new List<T>();
        await using (var conn = new SqlConnection(_connectionString))
        {
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = commandType;
                cmd.CommandText = sp;
                cmd.CommandTimeout = 300; //5 minutes
                cmd.Parameters.AddRange(parameters);
                await conn.OpenAsync(cancellationToken);
                await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (reader.HasRows) list.AddRange(_mapper.MapImmutableToList<T>(reader));
                }

                conn.Close();
            }
        }

        return list;
    }


    public async Task<List<T>> GetImmutableData<T>(string sql, CancellationToken cancellationToken)
    {
        var list = new List<T>();
        await using (var conn = new SqlConnection(_connectionString))
        {
            await using (var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text })
            {
                await conn.OpenAsync(cancellationToken);
                await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (reader.HasRows) list.AddRange(MapImmutableToList<T>(reader));
                }

                conn.Close();
            }
        }

        return list;
    }

    #region PRIVATE_METHODS

    public List<T> MapImmutableToList<T>(SqlDataReader reader)
    {
        var list = new List<T>();

        while (reader.Read())
        {
            var obj = reader[0];
            if (obj is T) list.Add((T)obj);
        }

        return list;
    }

    private async Task<object> ExecuteScalar(string sql, bool isStoredProcedure, CancellationToken cancellationToken,
        params IDataParameter[] parameters)
    {
        object ret;
        await using (var conn = new SqlConnection(_connectionString))
        {
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = isStoredProcedure ? CommandType.StoredProcedure : CommandType.Text;
                cmd.CommandTimeout = 300; //5 minutes
                cmd.CommandText = sql;
                foreach (var parm in parameters) cmd.Parameters.Add(parm);
                try
                {
                    await conn.OpenAsync(cancellationToken);
                    ret = await cmd.ExecuteScalarAsync(cancellationToken);
                    conn.Close();
                }
                catch (SqlException ex)
                {
                    var cmdType = isStoredProcedure ? "procedure" : "SQL";
                    var msg = $"Error executing {cmdType}: '{sql}'";
                    throw new DataException(msg, ex);
                }
            }
        }

        return ret;
    }

    private async Task<int> ExecuteNonQuery(string sql, CommandType commandType, CancellationToken cancellationToken,
        params IDataParameter[] parameters)
    {
        int ret;
        await using (var conn = new SqlConnection(_connectionString))
        {
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = commandType;
                cmd.CommandTimeout = 300; //5 minutes
                cmd.CommandText = sql;
                cmd.Parameters.AddRange(parameters);
                try
                {
                    await conn.OpenAsync(cancellationToken);
                    ret = await cmd.ExecuteNonQueryAsync(cancellationToken);
                    conn.Close();
                }
                catch (SqlException ex)
                {
                    var msg = $"Error executing {commandType}: '{sql}'";
                    throw new DataException(msg, ex);
                }
            }
        }

        return ret;
    }

    private async Task<DataTable> FillDataTable(string sql, CommandType commandType,
        CancellationToken cancellationToken, params IDataParameter[] parameters)
    {
        var tbl = new DataTable();
        await using (var conn = new SqlConnection(_connectionString))
        {
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = commandType;
                cmd.Parameters.AddRange(parameters);
                try
                {
                    await conn.OpenAsync(cancellationToken);
                    await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        tbl.Load(reader);
                    }

                    await conn.CloseAsync();
                }
                catch (Exception ex)
                {
                    var msg = $"Error retrieving data using {commandType}: '{sql}'";
                    throw new SystemException(msg, ex);
                }
            }
        }

        return tbl;
    }

    private async Task<List<T>> FillList<T>(string sql, CommandType commandType, CancellationToken cancellationToken,
        params IDataParameter[] parameters) where T : class, new()
    {
        var ret = new List<T>();

        await using (var conn = new SqlConnection(_connectionString))
        {
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = commandType;
                cmd.CommandText = sql;
                cmd.CommandTimeout = 300; //5 minutes
                cmd.Parameters.AddRange(parameters);
                try
                {
                    await conn.OpenAsync(cancellationToken);
                    var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                    ret.AddRange(_mapper.MapToList<T>(reader));
                    conn.Close();
                }
                catch (SqlException ex)
                {
                    var msg = $"Error retrieving data using this {commandType}: '{sql}'";
                    throw new DataException(msg, ex);
                }
            }
        }

        return ret;
    }

    #endregion PRIVATE_METHODS
}