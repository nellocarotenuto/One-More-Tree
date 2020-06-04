using Mobile.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Mobile.Services
{
    public class TreeService
    {
        private static readonly string _synchronizationUrl = "https://onemoretree.azurewebsites.net/api/trees/sync?fromVersion={0}";
        private static readonly string _postTreeUrl = "https://onemoretree.azurewebsites.net/api/trees";

        public async Task Synchronize()
        {
            HttpClient httpClient = new HttpClient();
            
            // Call API to syncronize with the server
            HttpResponseMessage response = 
                await httpClient.GetAsync(string.Format(_synchronizationUrl, Preferences.Get("db_version", 0)));

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException();
            }

            dynamic json = JObject.Parse(await response.Content.ReadAsStringAsync());
            
            if (json.Metadata.Sync.Version > Preferences.Get("db_version", 0))
            {
                foreach (dynamic jsonTree in json.Data)
                {                    
                    if (jsonTree["$operation"] == "D")
                    {
                        await App.Database.DeleteTreeAsync((long) jsonTree.Id);
                    }
                    else
                    {
                        Tree tree = new Tree()
                        {
                            Id = jsonTree.Id,
                            Photo = jsonTree.Photo,
                            Description = jsonTree.Description,
                            Latitude = jsonTree.Latitude,
                            Longitude = jsonTree.Longitude,
                            City = jsonTree.City,
                            State = jsonTree.State,
                            Date = jsonTree.Date,
                            UserId = jsonTree.User.Id,
                            User = new User()
                            {
                                Id = jsonTree.User.Id,
                                Name = jsonTree.User.Name,
                                Picture = jsonTree.User.Picture
                            }
                        };
                        
                        await App.Database.SaveTreeAsync(tree);
                    }                    
                }
                Preferences.Set("db_version", (int) json.Metadata.Sync.Version);
            }
        }

        public async Task PostTree(string filePath, double latitude, double longitude, string description)
        {
            using (FileStream file = File.OpenRead(filePath))
            {
                StreamContent photo = new StreamContent(file);
                photo.Headers.ContentType = new MediaTypeHeaderValue("image/png");

                HttpClient httpClient = new HttpClient();

                // Set the access token
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", await App.AuthService.GetAccessToken());

                MultipartFormDataContent form = new MultipartFormDataContent();

                if (description != null && description.Trim() != string.Empty)
                {
                    form.Add(new StringContent(description), "Description");
                }

                form.Add(new StringContent($"{latitude.ToString("G", CultureInfo.InvariantCulture)}, {longitude.ToString("G", CultureInfo.InvariantCulture)}"), "Coordinates");
                form.Add(photo, "Photo", filePath.Substring(filePath.LastIndexOf("/") + 1));
                
                HttpResponseMessage response = await httpClient.PostAsync(_postTreeUrl, form);

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException("Please verify that the photo doesn't contain adult content," +
                        "the descritpion isn't offensive and that the location is valid.");
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException();
                }
            }       
        }
    }
}
