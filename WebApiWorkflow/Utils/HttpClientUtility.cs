using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WebApiWorkflow.Utilities
{
    public class HttpClientUtility
    {
        private readonly HttpClient _httpClient;

        public HttpClientUtility(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<object> SendHttpRequestAsync(string url, string method, object payload = null, Dictionary<string, string> headers = null, Dictionary<string, string> queryParams = null)
        {
            try
            {
                Console.WriteLine($"\n\n\n\n\n\n\n\nStarting HTTP request {url}");
                // Construct URL with query parameters
                var requestUrl = BuildUrlWithQueryParams(url, queryParams);

                // Create HttpRequestMessage
                var request = new HttpRequestMessage(new HttpMethod(method), requestUrl);

                // Add headers
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }

                // Add payload for POST or PUT
                if ((method == "POST" || method == "PUT") && payload != null)
                {
                    var payloadJson = JsonConvert.SerializeObject(payload);
                    request.Content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
                }

                // Send HTTP request
                var response = await _httpClient.SendAsync(request);

                // Return response
                var responseContent = await response.Content.ReadAsStringAsync();
                return new
                {
                    StatusCode = response.StatusCode,
                    Content = responseContent
                };
            }
            catch (Exception ex)
            {
                // Rethrow exceptions to be handled by the calling context
                throw new Exception($"HTTP request failed: {ex.Message}", ex);
            }
        }

        private string BuildUrlWithQueryParams(string url, Dictionary<string, string> queryParams)
        {
            if (queryParams != null && queryParams.Count > 0)
            {
                var query = string.Join("&", queryParams.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
                return $"{url}?{query}";
            }
            return url;
        }
    }
}