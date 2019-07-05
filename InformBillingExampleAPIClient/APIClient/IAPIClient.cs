using System.Net.Http;
using System.Threading.Tasks;

namespace InformBillingExampleAPIClient.APIClient
{
    public interface IAPIClient
    {
        Task<T> GetAsync<T>(string endPoint);
        Task<T> PostAsync<T>(object model, string endPoint);
        Task<HttpResponseMessage> PostAsyncResponse(object model, string endPoint);
        Task<bool> PutAsync(string endPoint, object model);
    }
}
