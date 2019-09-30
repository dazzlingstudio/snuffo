using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Auth0.AuthenticationApi;
using Dazzling.Studio.Utils;
using System.Configuration;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Uvendia.Domain;
using RestSharp;
using Newtonsoft.Json;
using System.Security.Claims;
using Auth0.AuthenticationApi.Models;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Snuffo.Web.Models;

namespace Snuffo.Web
{
    public class Auth0Helper
    {
        public static string Auth0Domain = ConfigurationManager.AppSettings["auth0:Domain"];
        public static string Auth0ClientId = ConfigurationManager.AppSettings["auth0:ClientId"];
        public static string Auth0ClientSecret = ConfigurationManager.AppSettings["auth0:ClientSecret"];
        public static string Connection = "Username-Password-Authentication";

        private AuthenticationApiClient _client = null;
        private ManagementApiClient _managementClient = null;

        public AuthenticationApiClient CreateAuthenticationApiClientIfNotExists()
        {
            if (_client == null)
            {
                _client = new AuthenticationApiClient(new Uri($"https://{Auth0Domain}/"));
            }
            return _client;
        }

        public ManagementApiClient CreateManagementApiClientIfNotExists()
        {
            if (_managementClient == null)
            {
                var token = GetManagementToken();
                _managementClient = new ManagementApiClient(token, new Uri($"https://{Auth0Domain}/api/v2"));
            }
            return _managementClient;
        }

        private string GetManagementToken()
        {
            int attempts = 1;
            while (true)
            {
                try
                {
                    var client = new RestClient($"https://{Auth0Domain}/oauth/token");
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("content-type", "application/json");
                    request.AddParameter("application/json", $"{{\"grant_type\":\"client_credentials\",\"client_id\": \"{Auth0ClientId}\",\"client_secret\": \"{Auth0ClientSecret}\",\"audience\": \"https://{Auth0Domain}/api/v2/\"}}", ParameterType.RequestBody);
                    IRestResponse response = client.Execute(request);
                    if (response.IsSuccessful)
                    {
                        dynamic responseData = JsonConvert.DeserializeObject<dynamic>(response.Content);
                        return responseData.access_token;
                    }
                    else
                    {
                        throw new HttpException((int)response.StatusCode, response.ErrorMessage);
                    }
                }
                catch (HttpException httpEx)
                {
                    if (httpEx.GetHttpCode() != 401 || attempts > 3) // Unauthorized
                    {
                        throw;
                    }
                    ++attempts;
                }
            }
        }

        public User GetAuth0User(string user_id)
        {
            _managementClient = CreateManagementApiClientIfNotExists();
            var user = _managementClient.Users.GetAsync(user_id).GetAwaiter().GetResult();
            SetCustomerNames(user);
            return user;
        }

        public IEnumerable<User> SearchAuth0UserByEmail(string email)
        {
            _managementClient = CreateManagementApiClientIfNotExists();
            var response = _managementClient.Users.GetUsersByEmailAsync(email.ToLower()).GetAwaiter().GetResult();
            response.ForEach(u => SetCustomerNames(u));

            return response;
        }

        public User VerifyAuth0User(User user)
        {
            _managementClient = CreateManagementApiClientIfNotExists();
            var updateRequest = new UserUpdateRequest { EmailVerified = true };
            var response = _managementClient.Users.UpdateAsync(user.UserId, updateRequest).GetAwaiter().GetResult();
            
            return response;
        }

        public User ChangePassword(string userId, string password)
        {
            _managementClient = CreateManagementApiClientIfNotExists();
            var updateRequest = new UserUpdateRequest { Password = password, VerifyPassword = false };
            var response = _managementClient.Users.UpdateAsync(userId, updateRequest).GetAwaiter().GetResult();

            return response;
        }

        public User ChangeEmail(string userId, string email)
        {
            _managementClient = CreateManagementApiClientIfNotExists();
            var updateRequest = new UserUpdateRequest { Email = email, EmailVerified = true };
            var response = _managementClient.Users.UpdateAsync(userId, updateRequest).GetAwaiter().GetResult();

            return response;
        }

        public User UpdateAuth0User(AccountProfileModel customer)
        {
            _managementClient = CreateManagementApiClientIfNotExists();
            var request = new UserUpdateRequest();
            request.UserMetadata = new { customer.FirstName, customer.LastName, customer.Picture, customer.SubscribedToNewsletter, customer.Phone };
            
            var response = _managementClient.Users.UpdateAsync(customer.UserId, request).GetAwaiter().GetResult();
            return response;
        }

        public void DeleteAuth0User(string user_id)
        {
            _managementClient = CreateManagementApiClientIfNotExists();
            _managementClient.Users.DeleteAsync(user_id).Wait();
        }

        public void Authenticate(UserInfo user, bool rememberMe, IAuthenticationManager authenticationManager)
        {
            // Create claims principal
            var userDetails = this.GetAuth0User(user.UserId);
            //string name = (userDetails?.UserMetadata?.FirstName ?? user.FirstName) ?? user.NickName;
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId, "http://www.w3.org/2001/XMLSchema#string", $"https://{Auth0Helper.Auth0Domain}/"),
                new Claim(ClaimTypes.Name, userDetails.FirstName, "http://www.w3.org/2001/XMLSchema#string", $"https://{Auth0Helper.Auth0Domain}/")                
            }, CookieAuthenticationDefaults.AuthenticationType);

            if (!user.Email.IsNullOrEmpty())
            {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, user.Email, "http://www.w3.org/2001/XMLSchema#string", $"https://{Auth0Helper.Auth0Domain}/"));
            }
            if (!user.Picture.IsNullOrEmpty())
            {
                claimsIdentity.AddClaim(new Claim("picture", user.Picture, "http://www.w3.org/2001/XMLSchema#string", $"https://{Auth0Helper.Auth0Domain}/"));
            }
            claimsIdentity.AddClaim(new Claim("createdAt", userDetails.CreatedAt.ToString(), "http://www.w3.org/2001/XMLSchema#string", $"https://{Auth0Helper.Auth0Domain}/"));

            // Sign user into cookie middleware
            authenticationManager.SignIn(new AuthenticationProperties { IsPersistent = rememberMe }, claimsIdentity);
        }

        private void SetCustomerNames(User user)
        {
            if (user.FirstName.IsNullOrEmpty())
            {
                user.FirstName = (user.UserMetadata?.FirstName ?? user.NickName?.MakeFirstCharUpper()) ?? "Customer";
            }

            if (user.LastName.IsNullOrEmpty())
            {   
                user.LastName = (user.UserMetadata?.LastName ?? user.LastName) ?? "";
            }
        }
    }
}