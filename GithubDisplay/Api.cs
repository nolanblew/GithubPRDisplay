using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using GithubDisplay.RequestModels;
using Newtonsoft.Json;

namespace GithubDisplay
{
    public class Api
    {
        const string BASE_ENDPOINT = "https://api.unsplash.com";

        static JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            StringEscapeHandling = StringEscapeHandling.EscapeHtml,
            Formatting = Formatting.None,
        };

        public static async Task<T> GetResponseAsync<T>(string endpoint, Request request = null)
        {
            T rtn = default(T);

            var responseJson = await GetResponseJsonAsync(endpoint, request);
            rtn = JsonConvert.DeserializeObject<T>(responseJson);

            return rtn;
        }

        static async Task<string> GetResponseJsonAsync(string endpoint, Request request)
        {
            var ep = $"{BASE_ENDPOINT}/{endpoint}";

            if (request == null)
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Client-ID db99947a6ab97d5e2d7d414c621063eb71e12f8c1cfda0aeded48abc2c04e32d");

                    var response = await client.GetAsync(ep);
                    response.EnsureSuccessStatusCode();

                    return await response.Content.ReadAsStringAsync();
                }
            }

            
            var json = JsonConvert.SerializeObject(request, jsonSettings);
            using (var client = new HttpClient())
            using (var req = new HttpRequestMessage(HttpMethod.Post, ep))
            {
                var content = new StringContent(json);
                content.Headers.Add("Authorization", "Client-ID db99947a6ab97d5e2d7d414c621063eb71e12f8c1cfda0aeded48abc2c04e32d");

                foreach (var requestHeader in request.Headers)
                    content.Headers.Add(requestHeader.Item1, requestHeader.Item2);

                var response = await client.PostAsync(ep, content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
