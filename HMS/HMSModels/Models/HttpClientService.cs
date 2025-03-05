using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using HMSModels.Services;
using HMSModels.Models;
using Azure;
using System.Text.Json;

namespace HMSModels
{
    public class HttpClientService<T>
    {
        /// <summary>
        /// Označuje názov controlera, ktorý sa bude používať pri volaní API, defaultne je to názov triedy T. Možno zmeniť ak názov triedy nezodpovedá controleri.
        /// </summary>
        public string NavigationControler { get; set; } = typeof(T).Name;

        protected readonly HttpClient _httpClient;
        protected readonly UserServiceClient _userServiceClient;

        public HttpClientService(IHttpClientFactory httpClientFactory, UserServiceClient userServiceClient)
        {
            _httpClient = httpClientFactory.CreateClient("DefaultClient");  //pouziva default clienta uvedeneho v mauiprogram
            _userServiceClient = userServiceClient;

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _userServiceClient.JwtToken);
        }

        #region Authentification
        public async Task<LoginResponse?> LoginAsync(string username, string password)
        {
            var loginRequest = new LoginRequest { Name = username, Password = password };


            // you don't need a token for this call.
            var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}api/Auth/login", loginRequest);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result == null || string.IsNullOrEmpty(result?.Token))
            {
                throw new Exception("Login failed: no token received or null response");
            }
            return result;
        }

        public async Task<bool> LogOutAsync()
        {
            var response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}api/Auth/logout", null);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region GetAll
        /// <summary>
        /// Vráti zoznam všetkých entít typu T, nutnost použivat typy, ktore sa zhoduju s názvor controlera napr Objednavka -> ObjednavkaControler
        /// </summary>
        /// <returns></returns>
        public async Task<List<T>> GetAllAsync(bool includeAll = false)
        {
            var list = await _httpClient
                .GetFromJsonAsync<List<T>>(
                $"{_httpClient.BaseAddress}api/{NavigationControler}" + (includeAll ? "/includeall" : "")
                );
            return list ?? new List<T>();
        }
        #endregion

        #region FirstOrDefault
        /// <summary>
        /// Vráti prvu entitu typu T, ktora sa zhoduje s id, nutnost použivat typy, ktore sa zhoduju s názvor controlera napr Objednavka -> ObjednavkaControler
        /// </summary>
        /// <param name="id"></param>
        /// <param name="includeAll"></param>
        /// <returns></returns>
        public async Task<T?> FirstOrDefaultAsync(object id, bool includeAll = false)
        {
            string url = $"{_httpClient.BaseAddress}api/{NavigationControler}" + (includeAll ? "/includeall" : "") + $"/GetByIdOrDefault";

            var res = await _httpClient
                .PostAsJsonAsync(
                url, id ?? ""
                );
            var res2 = await res.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(res2))
            {
                return default;
            }
            return await res.Content.ReadFromJsonAsync<T?>();
        }

        /// <summary>
        /// Pouzije filter na ziskanie prvej entity. filter funguje presne len na typ T, nie na jeho props ako napr T.U.Name
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="includeAll"></param>
        /// <returns></returns>
        public async Task<T?> FirstOrDefaultAsync(FilterHtml<T> filter, bool includeAll = false)
        {
            string url = $"{_httpClient.BaseAddress}api/{NavigationControler}" + (includeAll ? "/includeall" : "") + $"/filter/GetByIdOrDefault";
            var res = await _httpClient
                .PostAsJsonAsync(
                url, filter.Conditions
                );
            var res2 = await res.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(res2))
            {
                return default;
            }
            return await res.Content.ReadFromJsonAsync<T?>(); ;
        }
        #endregion FirstOrDefault

        #region Delete
        public void Delete(object id)
        {
            string url = $"{_httpClient.BaseAddress}api/{NavigationControler}";
            var json = JsonSerializer.Serialize(id);
            var request = new HttpRequestMessage(HttpMethod.Delete, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            var response = _httpClient.Send(request);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(object id)
        {
            string url = $"{_httpClient.BaseAddress}api/{NavigationControler}";
            var json = JsonSerializer.Serialize(id);
            var request = new HttpRequestMessage(HttpMethod.Delete, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
        #endregion

        #region Create
        public async Task<T?> CreateAsync(T entity)
        {
            string url = $"{_httpClient.BaseAddress}api/{NavigationControler}/" + "Create";

            var res = await _httpClient
                .PostAsJsonAsync(
                url, entity
                );
            var res2 = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode || string.IsNullOrEmpty(res2))
            {
                return default;
            }
            return await res.Content.ReadFromJsonAsync<T?>();
        }
        #endregion

        #region Update
        public async Task<bool> UpdateAsync(object id, T entity)
        {
            var updateData = new UpdateRequest<T>
            {
                Id = id,
                Entity = entity
            };

            string url = $"{_httpClient.BaseAddress}api/{NavigationControler}/" + "Update";
            var res = await _httpClient
                .PostAsJsonAsync(
                url, updateData
                );
            return res.IsSuccessStatusCode;
        }
        #endregion

        #region Misc

        public async Task<object?> GetNextID()
        {
            string url = $"{_httpClient.BaseAddress}api/{NavigationControler}/" + "GetNextID";
            var res = await _httpClient
                .GetAsync(
                url
                );
            if (res.IsSuccessStatusCode)
            {
                var result = await res.Content.ReadFromJsonAsync<ValueResponse>();
                return FilterCondition.GetValueFromJsonElement((JsonElement?)result?.Value, null);
            }
            return null;
        }
        #endregion

    }

}
