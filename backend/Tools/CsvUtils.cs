using CsvHelper;

public static class CsvUtils
{
    public static IEnumerable<T> ReadManually<T>(this CsvReader reader, Func<CsvReader, T> mapper)
    {
        while (reader.Read())
        {
            yield return mapper(reader);
        }
    }
}