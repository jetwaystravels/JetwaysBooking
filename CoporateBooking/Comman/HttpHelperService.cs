namespace CoporateBooking.Comman
{
    using Newtonsoft.Json;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public static class HttpHelperService
    {
        public static async Task<(bool Success, string Error)> SendDeleteAsync(HttpClient client, string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                return (false, $"DELETE failed at {url}: {error}");
            }

            return (true, null);
        }

        public static async Task<(bool Success, string Error)> SendPutAsync(HttpClient client, string url)
        {
            var body = new
            {
                notifyContacts = true,
                contactTypesToNotify = new[] { "P" }
            };

            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = content
            };

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                return (false, $"PUT failed at {url}: {error}");
            }

            return (true, null);
        }
    }

}
