using System.Collections.Generic;

namespace QueryProvider.Models
{
    public class ODataResponse<T>
    {
        public IEnumerable<T> value { get; set; }
    }
}
