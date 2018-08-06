using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using UserManagement.Models;

namespace UserManagement.Services
{
    public class AADUserManager
    {
        //
        // The Client ID is used by the application to uniquely identify itself to Azure AD.
        // The App Key is a credential used by the application to authenticate to Azure AD.
        // The Tenant is the name of the Azure AD tenant in which this application is registered.
        // The AAD Instance is the instance of Azure, for example public Azure or Azure China.
        // The Authority is the sign-in URL of the tenant.
        //
        private static string aadInstance = "https://login.microsoftonline.com/{0}";

         //*******************************
         //******* company account *******
         //*******************************
        private static string tenant = "gstcloud.onmicrosoft.com";
        private static string clientId = "eb9c1d8f-16de-4e44-923b-23cd9bc2d4eb";
        private static string appKey = "ZRWgzBALfRkmYAgQxZGRFSwZrKgdWQLp63Nz48nwZP4=";
        private static string authAppID = "964d79aa-069a-4aa7-b5f4-262764ef38de";
        private static string authUrl = "https://login.microsoftonline.com/f92e674f-c2f4-43cf-b2a0-cebecab3b84e/oauth2/token";

        //*******Resource List*******
        //microsoft graph api
        private static string graphResourceId = "https://graph.microsoft.com/";
        //for authrozie ohter service
        private static string clientResourceId = "f9a8eeb7-39fc-43c0-9ca9-fabf9ab8fe21";

        //*******GraphAPILinks********
        private static string graphUser = "https://graph.microsoft.com/v1.0/users";
        private static string graphGroupAddNoRole = "https://graph.microsoft.com/v1.0/groups/2d027316-941e-458d-bf56-3504716ceff6/members/$ref";
        private static string graphGroupCommiEng = "https://graph.microsoft.com/v1.0/groups/0280b7da-3679-437f-96d8-1bc0fd05a9bd/members";

        //*******Group Id List*******
        private static Dictionary<string, string> RoleDic = new Dictionary<string, string> {
            { "e8c0d050-5c40-4c78-932e-b3d5fd5e3ede","system_supervisor"},
            { "964811b5-e052-4db0-9c10-c5b731c58801","engineering_supervisor"},
            { "74fe6f20-5480-41f2-8036-1858de5774a4","technical_supervisor"},
            { "2376b6f4-71dc-4014-88b6-cebd3c5c95de","service_engineer"},
            { "0280b7da-3679-437f-96d8-1bc0fd05a9bd","commission_engineer"},
            { "2d027316-941e-458d-bf56-3504716ceff6","NoRole"}
        };
        

        static string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
        private static HttpClient httpClient = new HttpClient();
        private static AuthenticationContext authContext = null;
        private static ClientCredential clientCredential = null;

        private static string graph_accesstoken = null;
        private static DateTimeOffset graph_onExpire;

        private static Timer timer;
        private static bool hasCounter = false;

        public  AADUserManager()
        {
        }

        /**************************************************
        ****************Renew AccessToken Auto*************
        **************************************************/

        public static void AutoRenewAccessToken()
        {
            if (!hasCounter)
            {
                hasCounter = true;
                //Renew graphToken every 55min
                timer = new Timer(TimeCouterFeedBack, null, 0, 3300000);
            }
        }

        private static void TimeCouterFeedBack(object o)
        {
            getAccessToken();
        }

        /**************************************************
        ****************Get Graph Access Token*************
        **************************************************/

        public static string getAccessToken()
        {
            authContext = new AuthenticationContext(authority);
            clientCredential = new ClientCredential(clientId, appKey);
            graphToken().Wait();
            return graph_accesstoken;
        }

        static async Task graphToken()
        {
            //
            // Get an access token from Azure AD using client credentials.
            // If the attempt to get a token fails because the server is unavailable, retry twice.
            //
            AuthenticationResult result = null;
            int retryCount = 0;
            bool retry = false;

            do
            {
                retry = false;
                try
                {
                    // ADAL includes an in memory cache, so this call will only send a message to the server if the cached token is expired.
                    result = await authContext.AcquireTokenAsync(graphResourceId, clientCredential);
                }
                catch (AdalException ex)
                {
                    if (ex.ErrorCode == "temporarily_unavailable")
                    {
                        retry = true;
                        retryCount++;
                    }
                }

            } while ((retry == true) && (retryCount < 3));
            graph_accesstoken = result.AccessToken;
            graph_onExpire = result.ExpiresOn;
        }

        /**************************************************
        ***********Read all Commission Engineers***********
        **************************************************/
        public static async Task<string> ReadAllComEngAsync()
        {
            var client = new HttpClient();
            var getUserUri = new Uri(graphGroupCommiEng);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", graph_accesstoken);
            var response = await client.GetAsync(getUserUri);
            if (response.IsSuccessStatusCode){
                return await response.Content.ReadAsStringAsync();
            }
            else { 
                return "error" + await response.Content.ReadAsStringAsync();
            }
        }

        /**************************************************
        ******************Create a new user****************
        **************************************************/
        public static async Task<string> createNewUserRequestAsync(CreateUserRequestModel user)
        {
            var client = new HttpClient();
            // get the access token
            var postUserUri = new Uri(graphUser);
            // create a new user in aad
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", graph_accesstoken);
            var content = new StringContent(JsonConvert.SerializeObject(new CreateUserByGraphRequestModel(user, tenant)), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(postUserUri, content);
            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                var newUserRes = await response.Content.ReadAsStringAsync();
                JObject newUserReturn = JObject.Parse(newUserRes);
                var userId = newUserReturn["id"].ToString();
                // add new user to "noRole" Group
                var postGroupUri = new Uri(graphGroupAddNoRole);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", graph_accesstoken);
                var responseGroup = await client.PostAsync(postGroupUri, new StringContent(JsonConvert.SerializeObject(new OdataIdModel("https://graph.microsoft.com/v1.0/directoryObjects/"+userId)), Encoding.UTF8, "application/json"));
                if (responseGroup.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    return "Created";
                }
                else
                {
                    return await responseGroup.Content.ReadAsStringAsync();
                }
            }
            else
            {
                return await response.Content.ReadAsStringAsync();
            }
        }


        /**************************************************
        ******************Test has this user***************
        **************************************************/
        public static async Task<string> hasThisUserRequestAsync(string username)
        {
            var client = new HttpClient();
            var getUserUri = new Uri(graphUser + '/' + username + "@" +tenant);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", graph_accesstoken);
            var response = await client.GetAsync(getUserUri);
            // if this username exists
            if (response.IsSuccessStatusCode){
                return "true";
            }
            // if this username doesn't exist
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return "false";
            }
            else { 
                return response.ReasonPhrase;
            }
        }

        /**************************************************
        ***********Update user personal info***************
        **************************************************/
        public static async Task<string> UpdateUserRequestAsync(AuthResponseUserModel user)
        {
            var client = new HttpClient();
            var Uri = new Uri(graphUser + '/' + user.username + "@" +tenant);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", graph_accesstoken);
            var requestBodyList = new Dictionary<string, string>();
            requestBodyList.Add("givenName", user.givenName);
            requestBodyList.Add("surname", user.surname);
            requestBodyList.Add("jobTitle", user.email);
            requestBodyList.Add("mobilePhone", user.mobilePhone);
            requestBodyList.Add("officeLocation", user.officeLocation);
            var content = new StringContent(JsonConvert.SerializeObject(requestBodyList), Encoding.UTF8, "application/json");
            var response = await client.PatchAsync(Uri, content);
            if (response.IsSuccessStatusCode){
                return "success";
            }
            else { 
                return await response.Content.ReadAsStringAsync();
            }
        }

        /**************************************************
        ************authentication by password*************
        **************************************************/

        public static async Task<AuthResponseUserModel> tokenByUserPassword(string username, string password)
        {
            authContext = new AuthenticationContext(authority);
            clientCredential = new ClientCredential(clientId, appKey);
            var authenResult = await AuthenticateAsync(username, password);
            // if username & password are correct
            if (authenResult != null)
            {
                AuthResponseUserModel user = new AuthResponseUserModel();
                user.username = username.Replace("@" + tenant, "");
                // read the group that this user belongs to
                user.roleName = await getRoleNameByUsername(username);
                // something error happened
                if(user.roleName == null)
                {
                    // if graph access token has expired
                    getAccessToken();
                    user.roleName = await getRoleNameByUsername(username);
                }
                user.OAuthToken = authenResult;

                // read more detail information about this user
                var client = new HttpClient();
                var getUserUri = new Uri(graphUser + '/' + username);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", graph_accesstoken);
                var response = await client.GetAsync(getUserUri);
                if (response.IsSuccessStatusCode)
                {
                    JObject userInfoDetailRes = JObject.Parse(await response.Content.ReadAsStringAsync());
                    // add user information in details
                    user.Id = userInfoDetailRes["id"].ToString();
                    user.givenName = userInfoDetailRes["givenName"].ToString() != string.Empty ? userInfoDetailRes["givenName"].ToString() : null;
                    user.surname = userInfoDetailRes["surname"].ToString() != string.Empty ? userInfoDetailRes["surname"].ToString() : null;
                    // we use "jobTitle" in AAD to storage email
                    user.email = userInfoDetailRes["jobTitle"].ToString() != string.Empty ? userInfoDetailRes["jobTitle"].ToString() : null;
                    user.mobilePhone = userInfoDetailRes["mobilePhone"].ToString() != string.Empty ? userInfoDetailRes["mobilePhone"].ToString() : null;
                    user.officeLocation = userInfoDetailRes["officeLocation"].ToString() != string.Empty ? userInfoDetailRes["officeLocation"].ToString() : null;
                }
                return user;
            }
            else
                return null;
        }

        private static async Task<string> getRoleNameByUsername(string username)
        {
            var client = new HttpClient();
            var getUserGroupUri = new Uri(graphUser + "/" + username + "/memberOf");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", graph_accesstoken);
            var response = await client.GetAsync(getUserGroupUri);
            if (response.IsSuccessStatusCode)
            {
                JObject userInfoDetailRes = JObject.Parse(await response.Content.ReadAsStringAsync());
                var result = userInfoDetailRes["value"][0]["id"].ToString();
                if (RoleDic.ContainsKey(result))
                    return RoleDic[result];
                else
                    return "not our user";
            }
            return null;
        }

        public static string getRoleName(string authenAccessToken)
        {
            //Assume the input is in a control called txtJwtIn,
            //string Role;
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtInput = authenAccessToken;
            bool canRead = jwtHandler.CanReadToken(jwtInput);

            if (canRead) {
                var token = jwtHandler.ReadJwtToken(jwtInput);
                //Extract the headers and claims of the JWT
                var headers = token.Header;
                var claims = token.Claims;

                foreach (Claim c in claims)
                {
                    if (c.Type.Equals("groups"))
                    {
                        if (RoleDic.ContainsKey(c.Value))
                            return RoleDic[c.Value];
                    }
                }
                return "not our user";
            }
            return "Can not read JWT!";
        }

        private static async Task<OAuthResult> AuthenticateAsync(string username, string password)
        {
            var oauthEndpoint = new Uri(authUrl);

            using (var client = new HttpClient())
            {
                var result = await client.PostAsync(oauthEndpoint, new FormUrlEncodedContent(new[]
                {
                    // paras of request
                    new KeyValuePair<string, string>("resource", clientResourceId),
                    new KeyValuePair<string, string>("client_id", authAppID),
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("scope", "openid"),
                }));
                if (result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<OAuthResult>(content);
                }
                else
                    return null;
            }
        }

    }
}