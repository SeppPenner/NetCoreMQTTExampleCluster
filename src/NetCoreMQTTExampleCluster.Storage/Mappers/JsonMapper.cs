// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonMapper.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A database mapper class to map classes to JSON.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Mappers;

/// <summary>
/// A database mapper class to map classes to JSON.
/// </summary>
/// <typeparam name="T">The generic type parameter.</typeparam>
public class JsonMapper<T> : SqlMapper.TypeHandler<T> where T : class, new()
{
    /// <summary>
    /// The json serializer settings.
    /// </summary>
    private readonly JsonSerializerSettings jsonSerializerSettingsCustom = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        Formatting = Formatting.Indented,
        ObjectCreationHandling = ObjectCreationHandling.Replace,
        Converters = new List<JsonConverter>
        {
            new IsoDateTimeConverter()
        }
    };

    /// <summary>
    /// Parses a database value back to a typed value.
    /// </summary>
    /// <param name="value">The value from the database.</param>
    /// <returns>The typed value.</returns>
    public override T? Parse(object value)
    {
        var config = new T();

        if (value is not null)
        {
            config = JsonConvert.DeserializeObject<T?>(value?.ToString() ?? string.Empty, this.jsonSerializerSettingsCustom);
        }

        return config;
    }

    /// <summary>
    /// Assigns the value of a parameter before a command executes.
    /// </summary>
    /// <param name="parameter">The parameter to configure.</param>
    /// <param name="value">The parameter value.</param>
    public override void SetValue(IDbDataParameter parameter, T? value)
    {
        parameter.Value = JsonConvert.SerializeObject(value, this.jsonSerializerSettingsCustom);
    }
}
