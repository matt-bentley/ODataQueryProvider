using System.Collections.Generic;

namespace QueryProvider.Interfaces
{
    public interface IODataClient<T>
    {
        IEnumerable<T> GetData(string queryString);
    }
}
