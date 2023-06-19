using HouseManagement.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HouseManagement.OtherClasses
{
    public class ApiRequests
    {
        public static async Task<ResultsRequest> LoginToWebApiAsync(string email, string password)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(Urles.AuthenticateUrl),
                Method = HttpMethod.Post
            };
            request.Content = new StringContent(
                $"{{\"Username\": \"{email}\", \"Password\": \"{password}\"}}",
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage response = await client.SendAsync(request);
            

            if(response.StatusCode == HttpStatusCode.OK)
            {
                var json = await response.Content.ReadAsStringAsync();
                JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;
                result.StatusCode = (int)response.StatusCode;
                result.Item = root.GetProperty("token").ToString();
            }
            else
            {
                result.StatusCode = (int)response.StatusCode;
            }
            return result;
        }
        public static async Task<ResultsRequest> PutToApiAsync(Object objectToSend, string url, string token)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Put
            };

            request.Headers.Add("Authorization", "Bearer " + token);

            request.Content = new StringContent(
                JsonConvert.SerializeObject(objectToSend),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage response = await client.SendAsync(request);

            result.StatusCode = (int)response.StatusCode;

            var json = await response.Content.ReadAsStringAsync(); 
            try
            {
                JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;
                result.Message = root.GetProperty("message").ToString();
            }
            catch { }

            return result;
        }
        public static async Task<ResultsRequest> PostToApiAsync(Object objectToSend, string url, string token)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Post
            };

            request.Headers.Add("Authorization", "Bearer " + token);

            request.Content = new StringContent(
                JsonConvert.SerializeObject(objectToSend),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage response = await client.SendAsync(request);

            result.StatusCode = (int)response.StatusCode;

            var json = await response.Content.ReadAsStringAsync();
            try
            {
                JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;
                result.Message = root.GetProperty("message").ToString();
            }
            catch { }

            return result;
        }
        public static async Task<ResultsRequest> DeleteToApiAsync(string url, string token)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Delete
            };

            request.Headers.Add("Authorization", "Bearer " + token);

            HttpResponseMessage response = await client.SendAsync(request);

            result.StatusCode = (int)response.StatusCode;

            var json = await response.Content.ReadAsStringAsync();
            try
            {
                JsonDocument message = JsonDocument.Parse(json);
                result.Message = message.RootElement.GetProperty("message").GetString();
            }
            catch { }

            return result;
        }
        public static async Task<ResultsRequest> PostComplaintPhotoToApiAsync(IFormFile photo, string url, string token)
        {
            var result = new ResultsRequest();
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Put,
            };

            request.Headers.Add("Authorization", "Bearer " + token);

            var photoContent = new StreamContent(photo.OpenReadStream());
            photoContent.Headers.ContentType = MediaTypeHeaderValue.Parse(photo.ContentType);

            var formData = new MultipartFormDataContent();
            formData.Add(photoContent, "photo", photo.FileName);

            request.Content = formData;

            HttpResponseMessage response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            result.StatusCode = (int)response.StatusCode;

            return result;
        }
    }
}
