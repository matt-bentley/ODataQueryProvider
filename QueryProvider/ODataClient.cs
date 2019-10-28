using QueryProvider.Interfaces;
using QueryProvider.Models;
using System.Collections.Generic;
using System.Net.Http;

namespace QueryProvider
{
    public class ODataClient<T> : IODataClient<T>
    {
        private readonly HttpClient _client;
        private readonly string _subPath;

        public ODataClient(HttpClient client, string subPath)
        {
            _client = client;
            _subPath = subPath;
        }

        public IEnumerable<T> GetData(string queryString)
        {
            var response = _client.GetAsync($"{_subPath}{queryString}").Result;
            var data = response.Content.ReadAsAsync<ODataResponse<T>>().Result;
            return data.value;
        }
    }
}
