using HouseManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;

namespace HouseManagement.OtherClasses
{
    public class ApiGetRequests
    {
        public static async Task<ResultsRequest> LoadUserFromAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);


            HttpResponseMessage response = await client.SendAsync(request);

            string responseBody = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                UserModel? item = new UserModel();
                item = JsonConvert.DeserializeObject<UserModel>(responseBody);
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadAdvertisementFromAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);


            HttpResponseMessage response = await client.SendAsync(request);

            string responseBody = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                Advertisement? item = new Advertisement();
                item = JsonConvert.DeserializeObject<Advertisement>(responseBody);
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadComplaintFromAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);


            HttpResponseMessage response = await client.SendAsync(request);

            string responseBody = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                Complaint? item = new Complaint();
                item = JsonConvert.DeserializeObject<Complaint>(responseBody);
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadFlatFromAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);


            HttpResponseMessage response = await client.SendAsync(request);

            string responseBody = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                Flat? item = new Flat();
                item = JsonConvert.DeserializeObject<Flat>(responseBody);
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadStringAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);


            HttpResponseMessage response = await client.SendAsync(request);

            string responseBody = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                string? item = null;
                item = JsonConvert.DeserializeObject<string>(responseBody);
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadStringWithoutDeserializeAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);


            HttpResponseMessage response = await client.SendAsync(request);

            string responseBody = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                string? item = null;
                item = responseBody;
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadStringPhotoAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);


            HttpResponseMessage response = await client.SendAsync(request);

            string responseBody = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                string? item = responseBody;
                result.StatusCode = 200;
                if (item == "") result.Item = null;
                else result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadListStringPhotosAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);


            HttpResponseMessage response = await client.SendAsync(request);

            string responseBody = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                List<string?> item = JsonConvert.DeserializeObject<List<string?>>(responseBody);
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadUseByIdFromAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);


            HttpResponseMessage response = await client.SendAsync(request);

            string responseBody = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                UserModel? item = new UserModel();
                item = JsonConvert.DeserializeObject<UserModel>(responseBody);
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadFlatOwnerFromAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);


            HttpResponseMessage response = await client.SendAsync(request);

            string responseBody = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                FlatOwner? item = new FlatOwner();
                item = JsonConvert.DeserializeObject<FlatOwner>(responseBody);
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadHouseFromAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);


            HttpResponseMessage response = await client.SendAsync(request);

            string responseBody = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                House? item = new House();
                item = JsonConvert.DeserializeObject<House>(responseBody);
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadCounterFromAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);


            HttpResponseMessage response = await client.SendAsync(request);

            string responseBody = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                Counter? item = new Counter();
                item = JsonConvert.DeserializeObject<Counter>(responseBody);
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadServiceFromAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);


            HttpResponseMessage response = await client.SendAsync(request);

            string responseBody = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                Service? item = new Service();
                item = JsonConvert.DeserializeObject<Service>(responseBody);
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadPaymentFromAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);


            HttpResponseMessage response = await client.SendAsync(request);

            string responseBody = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                Payment? item = new Payment();
                item = JsonConvert.DeserializeObject<Payment>(responseBody);
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadSettingsServiceFromAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);


            HttpResponseMessage response = await client.SendAsync(request);

            string responseBody = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                SettingsService? item = new SettingsService();
                item = JsonConvert.DeserializeObject<SettingsService>(responseBody);
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadIndicationFromAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);


            HttpResponseMessage response = await client.SendAsync(request);

            string responseBody = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                Indication? item = new Indication();
                item = JsonConvert.DeserializeObject<Indication>(responseBody);
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadListAdvertisementsFromAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                List<Advertisement>? item = new List<Advertisement>();
                if (response.IsSuccessStatusCode)
                {
                    item = JsonConvert.DeserializeObject<List<Advertisement>>(responseBody);
                }
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadListHousesFromAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                List<House>? item = new List<House>();
                if (response.IsSuccessStatusCode)
                {
                    item = JsonConvert.DeserializeObject<List<House>>(responseBody);
                }
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadListFlatsFromAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                List<Flat>? item = new List<Flat>();
                if (response.IsSuccessStatusCode)
                {
                    item = JsonConvert.DeserializeObject<List<Flat>>(responseBody);
                }
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadListComplaintsFromAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                List<Complaint>? item = new List<Complaint>();
                if (response.IsSuccessStatusCode)
                {
                    item = JsonConvert.DeserializeObject<List<Complaint>>(responseBody);
                }
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadListServicesFromAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                List<Service>? item = new List<Service>();
                if (response.IsSuccessStatusCode)
                {
                    item = JsonConvert.DeserializeObject<List<Service>>(responseBody);
                }
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadListUsersFromAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                List<UserModel>? item = new List<UserModel>();
                if (response.IsSuccessStatusCode)
                {
                    item = JsonConvert.DeserializeObject<List<UserModel>>(responseBody);
                }
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadListSummaryDataFromAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                List<SummaryData>? item = new List<SummaryData>();
                if (response.IsSuccessStatusCode)
                {
                    item = JsonConvert.DeserializeObject<List<SummaryData>>(responseBody);
                }
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadListSettingsServicesFromAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                List<SettingsService>? item = new List<SettingsService>();
                if (response.IsSuccessStatusCode)
                {
                    item = JsonConvert.DeserializeObject<List<SettingsService>>(responseBody);
                }
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadListCountersFromAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                List<Counter>? item = new List<Counter>();
                if (response.IsSuccessStatusCode)
                {
                    item = JsonConvert.DeserializeObject<List<Counter>>(responseBody);
                }
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadListPaymentsFromAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                List<Payment>? item = new List<Payment>();
                if (response.IsSuccessStatusCode)
                {
                    item = JsonConvert.DeserializeObject<List<Payment>>(responseBody);
                }
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadListIndicationsFromAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                List<Indication>? item = new List<Indication>();
                if (response.IsSuccessStatusCode)
                {
                    item = JsonConvert.DeserializeObject<List<Indication>>(responseBody);
                }
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
        public static async Task<ResultsRequest> LoadListFlatOwnersFromAPIAsync(string token, string url)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", "Bearer " + token);

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                List<FlatOwner>? item = new List<FlatOwner>();
                if (response.IsSuccessStatusCode)
                {
                    item = JsonConvert.DeserializeObject<List<FlatOwner>>(responseBody);
                }
                result.StatusCode = 200;
                result.Item = item;
                return result;
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
    }
}
