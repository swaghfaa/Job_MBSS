using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Job_MBSS.Data;
using Job_MBSS.Models;

namespace Job_MBSS.Services
{
    public class OAuthService
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;
        private readonly TokenRepository _repo;

        public OAuthService(string clientId, string clientSecret, string redirectUri, TokenRepository repo)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _redirectUri = redirectUri;
            _repo = repo;
        }

        public async Task<string> GetOrRefreshAccessToken()
        {
            var pair = _repo.GetTokens();

            if (!string.IsNullOrEmpty(pair.RefreshToken))
            {
                var access = await RefreshAccessToken(pair.RefreshToken);
                if (!string.IsNullOrEmpty(access)) return access;
            }

            Console.Write("Masukkan CODE Box: ");
            var code = Console.ReadLine();
            return await LoginGetTokens(code);
        }

        public async Task<string> LoginGetTokens(string code)
        {
            var http = new HttpClient();
            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("grant_type","authorization_code"),
                new KeyValuePair<string,string>("code", code),
                new KeyValuePair<string,string>("client_id", _clientId),
                new KeyValuePair<string,string>("client_secret", _clientSecret),
                new KeyValuePair<string,string>("redirect_uri", _redirectUri)
            });

            var res = await http.PostAsync("https://api.box.com/oauth2/token", form);
            var json = await res.Content.ReadAsStringAsync();

            var parsed = JObject.Parse(json);
            if (parsed["access_token"] == null) return null;

            var pair = new TokenPair
            {
                AccessToken = parsed["access_token"].ToString(),
                RefreshToken = parsed["refresh_token"].ToString()
            };
            _repo.SaveTokens(pair);

            return pair.AccessToken;
        }

        public async Task<string> RefreshAccessToken(string refreshToken)
        {
            var http = new HttpClient();
            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("grant_type","refresh_token"),
                new KeyValuePair<string,string>("refresh_token", refreshToken),
                new KeyValuePair<string,string>("client_id", _clientId),
                new KeyValuePair<string,string>("client_secret", _clientSecret)
            });

            var res = await http.PostAsync("https://api.box.com/oauth2/token", form);
            var json = await res.Content.ReadAsStringAsync();

            if (!json.Contains("access_token")) return null;

            var parsed = JObject.Parse(json);
            var pair = new TokenPair
            {
                AccessToken = parsed["access_token"].ToString(),
                RefreshToken = parsed["refresh_token"].ToString()
            };
            _repo.SaveTokens(pair);

            return pair.AccessToken;
        }
    }
}
