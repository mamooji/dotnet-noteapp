using System.Data;
using System.Data.Common;
using System.Reflection;
using Newtonsoft.Json;

namespace Domain.Utility;

public interface IMapperUtility
{
    List<T> MapToList<T>(DbDataReader reader) where T : class, new();
    T GetDataObject<T>(object data) where T : class;
    List<T> MapImmutableToList<T>(DbDataReader reader);
}

public class MapperUtility : IMapperUtility
{
    private readonly Dictionary<Type, SortedList<string, PropertyInfo>> _dict = new();

    public List<T> MapToList<T>(DbDataReader reader) where T : class, new()
    {
        var list = new List<T>();
        var columnNames = LoadColumnNames(reader);
        var propertyNames = LoadPropertyNames<T>();
        while (reader.Read())
        {
            var newObject = new T();
            foreach (var key in propertyNames.Keys)
                if (HasMatchingColumn(key, columnNames))
                {
                    var valToSet = reader[key];
                    var propertyInfo = propertyNames[key];
                    if (valToSet == DBNull.Value) valToSet = null;
                    if (valToSet != null && (propertyInfo.PropertyType == typeof(bool) ||
                                             propertyInfo.PropertyType == typeof(bool?)))
                    {
                        int i;
                        if (int.TryParse(valToSet.ToString(), out i) && (i == 1 || i == 0)) valToSet = i == 1;
                    }

                    propertyInfo.SetValue(newObject, valToSet);
                }

            list.Add(newObject);
        }

        return list;
    }


    public T GetDataObject<T>(object data) where T : class
    {
        if (data == null)
            return null;

        if (data is T ret)
            return ret;

        var json = data.ToString();
        if (json == null)
            return null;

        //if (!json.StartsWith("{"))
        //    return null;

        if (json.StartsWith("{{") && json.EndsWith("}}")) json = json.Substring(1, json.Length - 2);

        try
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public List<T> MapImmutableToList<T>(DbDataReader reader)
    {
        var list = new List<T>();

        while (reader.Read())
        {
            var obj = reader[0];
            if (obj is T) list.Add((T)obj);
        }

        return list;
    }

    #region Private Methods

    private SortedList<string, PropertyInfo> LoadPropertyNames<T>() where T : class
    {
        var t = typeof(T);
        if (!_dict.ContainsKey(t))
        {
            var props = typeof(T).GetProperties();
            SortedList<string, PropertyInfo> result = new();
            foreach (var p in props)
                // Only map system properties.
                if (p.PropertyType.FullName != null && p.PropertyType.FullName.StartsWith("System."))
                    result.Add(p.Name, p);
            _dict[t] = result;
        }

        return _dict[t];
    }

    private static List<string> LoadColumnNames(IDataRecord reader)
    {
        var columns = new List<string>();

        for (var i = 0; i < reader.FieldCount; i++) columns.Add(reader.GetName(i));
        return columns;
    }

    private bool HasMatchingColumn(string propName, IEnumerable<string> columnNames)
    {
        return columnNames.Any(x => x.Equals(propName, StringComparison.InvariantCultureIgnoreCase));
    }

    #endregion Private Methods
}