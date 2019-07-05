using InformBillingExampleAPIClient.APIClient;
using InformBillingExampleAPIClient.Models;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace InformBillingExampleAPIClient
{
    class Program
    {
        private static IAPIClient sampleAPIClient;

        private static string baseUrl = "";
        private static string tokenAccessEndPoint = "";
        private static string clientId = "";
        private static string apiKey = "";
        private static string customerAPIKey = "";
        private static string getCustomerEndPoint = "";

        static void Main(string[] args)
        {

            // The following values which needed to be populated in the App.Config file will need to be provided by Inform Billing
            baseUrl = ConfigurationManager.AppSettings["InformBillingAPIBaseUrl"];
            tokenAccessEndPoint = ConfigurationManager.AppSettings["TokenAccessEndPoint"];
            clientId = ConfigurationManager.AppSettings["ClientId"];
            apiKey = ConfigurationManager.AppSettings["APIKey"];
            customerAPIKey = ConfigurationManager.AppSettings["CustomerAPIKey"];
            getCustomerEndPoint = ConfigurationManager.AppSettings["GetCustomerByIdEndPoint"];

            // Best to use Dependency Injection here but keeping it simple for this example.
            sampleAPIClient = new SampleAPIClient(baseUrl, tokenAccessEndPoint, clientId, apiKey, customerAPIKey, new TimeSpan(0, 5, 0));

            // Call EndPoint
            CallGetEndPoint();

            Console.ReadLine();

        }

        private static async void CallGetEndPoint()
        {
            Console.WriteLine($"Calling endpoint: {baseUrl}{getCustomerEndPoint}");

            var result = await GetCustomerAsync(1);
            Console.WriteLine("Results:");
            Console.WriteLine($"CustomerId: {result.id}");
            Console.WriteLine($"AccountNumber: {result.accountNumber}");
            Console.WriteLine($"Company Name: {result.company}");
        }

        public static async Task<CustomerModel> GetCustomerAsync(int customerId)
        {
            var model = await sampleAPIClient.GetAsync<CustomerModel>($"{getCustomerEndPoint}{customerId}");
            return model;
        }

    }
}
