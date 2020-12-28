using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SharpJackApi.Tests
{
    /// <summary>
    /// Helper methods to send HTTP requests, trace responses and serialize/deserialize objects.
    /// </summary>
    /// <remarks>
    /// Most HTTP client usage is of the following pattern:
    ///     1. Construct an object.
    ///     2. Serialize it to JSON (or another format).
    ///     3. Send the request.
    ///     4. Log the response.
    ///     5. Deserialize the response body into an object.
    /// 
    /// These helper methods codify this pattern, specifically steps 2-5.
    /// 
    /// The sync methods are simple wrappers over async methods and are
    /// a placeholder until we convert everything to async all up.
    /// 
    /// The sync methods explicitly queue the work to the thread pool to
    /// avoid deadlocks.
    /// </remarks>
    public static class HttpClientExtensions
    {
        #region Async Methods

        public static async Task<TResult> GetAsJsonAsync<TResult>(this HttpClient client, string relativeUri,
            CancellationToken token, [CallerMemberName] string callerName = "")
        {
            var response = await client.GetAsync(relativeUri, token);

            var responseString = await TraceAndReturnResponseAsString(response, callerName);

            return JsonConvert.DeserializeObject<TResult>(responseString);
        }

        public static async Task<TResult> GetAsJsonAsync<TContent, TResult>(this HttpClient client, string relativeUri,
            TContent content, CancellationToken token, [CallerMemberName] string callerName = "")
        {
            var requestContent = SerializeAsJson(content);

            var request = new HttpRequestMessage(HttpMethod.Get, relativeUri) { Content = requestContent };

            var response = await client.SendAsync(request, token);
            var responseString = await TraceAndReturnResponseAsString(response, callerName);

            return JsonConvert.DeserializeObject<TResult>(responseString);
        }

        public static async Task<string> PostAsJsonAsync<TContent>(this HttpClient client, string relativeUri,
            TContent content, CancellationToken token, [CallerMemberName] string callerName = "")
        {
            var httpContent = SerializeAsJson(content);

            var response = await client.PostAsync(relativeUri, httpContent, token);

            return await TraceAndReturnResponseAsString(response, callerName);
        }

        public static async Task<TResult> PostAsJsonAsync<TContent, TResult>(this HttpClient client, string relativeUri,
            TContent content, CancellationToken token, [CallerMemberName] string callerName = "")
        {
            var httpContent = SerializeAsJson(content);

            var response = await client.PostAsync(relativeUri, httpContent, token);

            var responseString = await TraceAndReturnResponseAsString(response, callerName);

            return JsonConvert.DeserializeObject<TResult>(responseString);
        }

        public static async Task<TResult> PostAsFormUrlEncodedAsync<TResult>(this HttpClient client, string relativeUri,
            string content, CancellationToken token, [CallerMemberName] string callerName = "")
        {
            HttpContent requestContent = new StringContent(content);
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var response = await client.PostAsync(relativeUri, requestContent, token);

            var responseString = await TraceAndReturnResponseAsString(response, callerName);

            return JsonConvert.DeserializeObject<TResult>(responseString);
        }

        public static async Task<string> DeleteAsync(this HttpClient client, string relativeUri,
            CancellationToken token, [CallerMemberName] string callerName = "")
        {
            var response = await client.DeleteAsync(relativeUri, token);

            return await TraceAndReturnResponseAsString(response, callerName);
        }

        public static async Task<TResult> PutAsJsonAsync<TContent, TResult>(this HttpClient client, string relativeUri,
            TContent content, CancellationToken token, [CallerMemberName] string callerName = "")
        {
            var httpContent = SerializeAsJson(content);

            var response = await client.PutAsync(relativeUri, httpContent, token);

            var responseString = await TraceAndReturnResponseAsString(response, callerName);

            return JsonConvert.DeserializeObject<TResult>(responseString);
        }

        public static async Task<string> PutAsJsonAsync<TContent>(this HttpClient client, string relativeUri,
            TContent content, CancellationToken token, [CallerMemberName] string callerName = "")
        {
            var httpContent = SerializeAsJson(content);

            var response = await client.PutAsync(relativeUri, httpContent, token);

            return await TraceAndReturnResponseAsString(response, callerName);
        }

        public static async Task<string> PatchAsJsonAsync<TContent>(this HttpClient client, string relativeUri,
            TContent content, CancellationToken token, [CallerMemberName] string callerName = "")
        {
            var requestContent = SerializeAsJson(content);

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), relativeUri) { Content = requestContent };

            return await SendAsync(client, request, token, callerName);
        }

        public static async Task<string> SendAsync(this HttpClient client, HttpRequestMessage request,
            CancellationToken token, [CallerMemberName] string callerName = "")
        {
            var response = await client.SendAsync(request, token);

            return await TraceAndReturnResponseAsString(response, callerName);
        }

        #endregion

        #region Sync Wrappers

        public static TResult GetAsJson<TResult>(this HttpClient client, string relativeUri,
            CancellationToken token, [CallerMemberName] string callerName = "")
        {
            return Task.Run(() => GetAsJsonAsync<TResult>(client, relativeUri, token, callerName)).Result;
        }

        public static string PostAsJson<TContent>(this HttpClient client, string relativeUri,
            TContent content, CancellationToken token, [CallerMemberName] string callerName = "")
        {
            return Task.Run(() => PostAsJsonAsync<TContent>(client, relativeUri, content, token, callerName)).Result;
        }

        public static TResult PostAsJson<TContent, TResult>(this HttpClient client, string relativeUri,
            TContent content, CancellationToken token, [CallerMemberName] string callerName = "")
        {
            return Task.Run(() => PostAsJsonAsync<TContent, TResult>(client, relativeUri, content, token, callerName)).Result;
        }

        public static TResult PostAsFormUrlEncoded<TResult>(this HttpClient client, string relativeUri,
            string content, CancellationToken token, [CallerMemberName] string callerName = "")
        {
            return Task.Run(() => PostAsFormUrlEncodedAsync<TResult>(client, relativeUri, content, token, callerName)).Result;
        }

        public static string Delete(this HttpClient client, string relativeUri,
            CancellationToken token, [CallerMemberName] string callerName = "")
        {
            return Task.Run(() => DeleteAsync(client, relativeUri, token, callerName)).Result;
        }

        public static string PatchAsJson<TContent>(this HttpClient client, string relativeUri, TContent content,
            CancellationToken token, [CallerMemberName] string callerName = "")
        {
            return Task.Run(() => PatchAsJsonAsync(client, relativeUri, content, token, callerName)).Result;
        }

        public static string Send(this HttpClient client, HttpRequestMessage message,
            CancellationToken token, [CallerMemberName] string callerName = "")
        {
            return Task.Run(() => SendAsync(client, message, token, callerName)).Result;
        }

        #endregion

        private static HttpContent SerializeAsJson<TContent>(TContent content)
        {
            string json = JsonConvert.SerializeObject(content);
            HttpContent requestContent = new StringContent(json);
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return requestContent;
        }

        private static Task<string> TraceAndReturnResponseAsString(HttpResponseMessage response, string callerName)
        {
            response.EnsureSuccessStatusCode();

            // TODO: insert logging

            return response.Content.ReadAsStringAsync();
        }
    }
}