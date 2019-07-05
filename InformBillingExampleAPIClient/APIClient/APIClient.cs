using IdentityModel.Client;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace InformBillingExampleAPIClient.APIClient
{
    public class SampleAPIClient : IAPIClient
    {
        private static HttpClient apiClient;
        private static TokenClient tokenClient;
        private static TokenResponse tokenResponse;
        private static DateTime tokenCreation;
        private readonly string scope = "NGServicesApi";
        private bool usingCredentials = false;
        private string userName;
        private string password;

        public SampleAPIClient(string endPointBaseAddress, string tokenAccessEndPoint, string clientId, string apiKey, string customerKey, TimeSpan timeoOut)
        {
            apiClient = new HttpClient { BaseAddress = new Uri(endPointBaseAddress) };
            apiClient.DefaultRequestHeaders.Accept.Clear();
            apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            apiClient.DefaultRequestHeaders.Add("customerKey", customerKey);
            apiClient.Timeout = timeoOut;
            tokenClient = new TokenClient(tokenAccessEndPoint, clientId, apiKey);
        }

        /// <summary>
        /// Gets a token
        /// </summary>
        /// <returns></returns>
        private async Task RequestToken()
        {
            tokenResponse = await tokenClient.RequestClientCredentialsAsync(scope).ConfigureAwait(false);
            tokenCreation = DateTime.UtcNow;
        }

        /// <summary>
        /// Checks the current token is valid
        /// </summary>
        /// <returns></returns>
        private bool IsTokenValid()
        {
            if (tokenCreation.Equals(DateTime.MinValue))
                tokenCreation = DateTime.UtcNow;

            return tokenResponse != null &&
                !tokenResponse.IsError &&
                !string.IsNullOrWhiteSpace(tokenResponse.AccessToken) &&
                (tokenCreation.AddSeconds(tokenResponse.ExpiresIn) > DateTime.UtcNow);
        }

        /// <summary>
        /// Sets the token checking if the current token is valid first
        /// </summary>
        /// <param name="forceRefresh"></param>
        /// <returns></returns>
        private async Task SetAccessToken(bool forceRefresh = false)
        {
            string token = "";

            if (!forceRefresh && IsTokenValid())
            {
                apiClient.SetBearerToken(tokenResponse.AccessToken);
                return;
            }

            if (string.IsNullOrEmpty(token))
            {
                await RequestToken().ConfigureAwait(false);

                if (!IsTokenValid())
                    throw new InvalidOperationException("An unexpected token validation error has occured during a token request.");
            }

            apiClient.SetBearerToken(tokenResponse.AccessToken);
        }

        /// <summary>
        /// Calls a GET endpoint and returns the response
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string endPoint)
        {
            await SetAccessToken().ConfigureAwait(false);
            var responseMessage = await apiClient.GetAsync(apiClient.BaseAddress + endPoint);

            // Sometimes the API returns unauthorised even though the token expiry says it shouldn't have - this guards against this occurrence
            if (responseMessage.StatusCode.Equals(HttpStatusCode.Unauthorized))
            {
                await SetAccessToken(true).ConfigureAwait(false);
                responseMessage = await apiClient.GetAsync(apiClient.BaseAddress + endPoint);
            }

            responseMessage.EnsureSuccessStatusCode();
            return await responseMessage.Content.ReadAsAsync<T>();
        }

        /// <summary>
        /// Calls a POST endpoint sending in a model and returning the expected model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public async Task<T> PostAsync<T>(object model, string endPoint)
        {
            await SetAccessToken().ConfigureAwait(false);
            var requestMessage = new HttpRequestMessage();
            var responseMessage = await apiClient.PostAsJsonAsync(apiClient.BaseAddress + endPoint, model);

            // Sometimes the API returns unauthorised even though the token expiry says it shouldn't have - this guards against this occurrence
            if (responseMessage.StatusCode.Equals(HttpStatusCode.Unauthorized))
            {
                await SetAccessToken(true).ConfigureAwait(false);
                responseMessage = await apiClient.PostAsJsonAsync(apiClient.BaseAddress + endPoint, model);
            }

            responseMessage.EnsureSuccessStatusCode();
            var result = await responseMessage.Content.ReadAsAsync<T>();
            return result;
        }

        /// <summary>
        /// Calls a POST endpoint sending in a model and returning the HttpResponse
        /// </summary>
        /// <param name="model"></param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PostAsyncResponse(object model, string endPoint)
        {
            await SetAccessToken().ConfigureAwait(false);
            var requestMessage = new HttpRequestMessage();
            var responseMessage = await apiClient.PostAsJsonAsync(apiClient.BaseAddress + endPoint, model);

            // Sometimes the API returns unauthorised even though the token expiry says it shouldn't have - this guards against this occurrence
            if (responseMessage.StatusCode.Equals(HttpStatusCode.Unauthorized))
            {
                await SetAccessToken(true).ConfigureAwait(false);
                responseMessage = await apiClient.PostAsJsonAsync(apiClient.BaseAddress + endPoint, model);
            }

            responseMessage.EnsureSuccessStatusCode();

            return responseMessage;
        }

        /// <summary>
        /// Calls a PUT endpoint sending in the model
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> PutAsync(string endPoint, object model)
        {
            await SetAccessToken().ConfigureAwait(false);
            var responseMessage = await apiClient.PutAsJsonAsync(apiClient.BaseAddress + endPoint, model);

            // Sometimes the API returns unauthorised even though the token expiry says it shouldn't have - this guards against this occurrence
            if (responseMessage.StatusCode.Equals(HttpStatusCode.Unauthorized))
            {
                await SetAccessToken(true).ConfigureAwait(false);
                responseMessage = await apiClient.PostAsJsonAsync(apiClient.BaseAddress + endPoint, model);
            }
            responseMessage = await apiClient.PutAsJsonAsync(apiClient.BaseAddress + endPoint, model);
            return responseMessage.IsSuccessStatusCode;
        }
    }

}

