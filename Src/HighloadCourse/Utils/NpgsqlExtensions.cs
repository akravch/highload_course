using Npgsql;
using NpgsqlTypes;

namespace HighloadCourse.Utils;

internal static class NpgsqlExtensions
{
    public static NpgsqlParameterCollection AddPositional<T>(
        this NpgsqlParameterCollection collection,
        T value,
        NpgsqlDbType type,
        int size)
    {
        collection.Add(new NpgsqlParameter<T>
        {
            TypedValue = value,
            NpgsqlDbType = type,
            Size = size
        });

        return collection;
    }

    public static NpgsqlParameterCollection AddPositional<T>(
        this NpgsqlParameterCollection collection,
        T value,
        NpgsqlDbType type)
    {
        collection.Add(new NpgsqlParameter<T>
        {
            TypedValue = value,
            NpgsqlDbType = type
        });

        return collection;
    }
}