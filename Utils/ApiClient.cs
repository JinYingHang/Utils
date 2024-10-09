using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Utils
{


    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiClient(string baseUrl) {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(3);
        }

        public async Task<string> SendRequestAsync(HttpMethod method, string path, Dictionary<string, string> queryParams, Dictionary<string, string> headers = null) {
            try {
                var requestUri = BuildRequestUri(path, queryParams);
                using (var request = new HttpRequestMessage(method, requestUri)) {

                    if (headers != null) {
                        foreach (var header in headers) {
                            request.Headers.Add(header.Key, header.Value);
                        }
                    }

                    var response = await _httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode) {
                        return await response.Content.ReadAsStringAsync();
                    }
                    else {
                        throw new Exception($"API request failed with status code: {response.StatusCode} ");
                    }
                }
            }
            catch (TaskCanceledException ex) {
                // 处理超时或取消的任务
                throw new Exception("Request was canceled or timed out", ex);
            }
            catch (Exception) {
                throw;
            }
        }

        private Uri BuildRequestUri(string path, Dictionary<string, string> queryParams) {
            var queryParamsString = BuildQueryString(queryParams);
            var requestUri = new UriBuilder(_baseUrl) {
                Path = path,
                Query = queryParamsString
            };
            return requestUri.Uri;
        }

        private string BuildQueryString(Dictionary<string, string> queryParams) {
            var queryParamsList = new List<string>();
            foreach (var param in queryParams) {
                queryParamsList.Add($"{param.Key}={HttpUtility.UrlEncode(param.Value)}");
            }
            return string.Join("&", queryParamsList);
        }
    }
}
