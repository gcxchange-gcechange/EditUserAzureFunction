using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Configuration;
using System;
using System.Text;
using System.Web;
using System.IO;
using System.Web.Script.Serialization;
using Microsoft.Graph;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace UpdateUserHttp
{
    public static class UpdateUser
    {
        [FunctionName("UpdateUser")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            // parse query parameter
            string userID = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "userid", true) == 0)
                .Value;

            string jobTitle = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "jobtitle", true) == 0)
                .Value;

            string firstName = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "firstname", true) == 0)
                .Value;

            string lastName = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "lastname", true) == 0)
                .Value;
            string businessPhones = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "businessPhones", true) == 0)
                .Value;
            string streetAddress = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "streetAddress", true) == 0)
                .Value;
            string department = req.GetQueryNameValuePairs()
               .FirstOrDefault(q => string.Compare(q.Key, "department", true) == 0)
               .Value;
            string city = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "city", true) == 0)
                .Value;
            string province = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "province", true) == 0)
                .Value;
            string postalcode = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "postalcode", true) == 0)
                .Value;
            string mobilePhone = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "mobilephone", true) == 0)
                .Value;
            string country = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "country", true) == 0)
                .Value;

            if (userID == null)
            {
                // Get request body
                dynamic data = await req.Content.ReadAsAsync<object>();
                userID = userID ?? data?.userID;
                jobTitle = jobTitle ?? data?.jobTitle;
                firstName = firstName ?? data?.firstName;
                lastName = lastName ?? data?.lastName;
                businessPhones = businessPhones ?? data?.businessPhones;
                streetAddress = streetAddress ?? data?.streetAddress;
                department = department ?? data?.department;
                city = city ?? data?.city;
                province = province ?? data?.province;
                postalcode = postalcode ?? data?.postalcode;
                mobilePhone = mobilePhone ?? data?.mobilePhone;
                country = country ?? data?.country;
            }

            var authResult = GetOneAccessToken();
            var graphClient = GetGraphClient(authResult);
            ChangeUserInfo(graphClient, log, userID, jobTitle, firstName, lastName, businessPhones, streetAddress, department, city, province, postalcode, mobilePhone, country);

      return req.CreateResponse(HttpStatusCode.OK, "Finished. ");
        }

    public static string GetOneAccessToken()
    {
      string token = "";
      string CLIENT_ID = ConfigurationManager.AppSettings["CLIENT_ID"];
      string CLIENT_SECERET = ConfigurationManager.AppSettings["CLIENT_SECRET"];
      string TENANT_ID = ""; //ADD
      string TOKEN_ENDPOINT = "";
      string MS_GRAPH_SCOPE = "";
      string GRANT_TYPE = "";

      try
      {

        TOKEN_ENDPOINT = "https://login.microsoftonline.com/" + TENANT_ID + "/oauth2/v2.0/token";
        MS_GRAPH_SCOPE = "https://graph.microsoft.com/.default";
        GRANT_TYPE = "client_credentials";

      }
      catch (Exception e)
      {
        Console.WriteLine("A error happened while search config file");
      }
      try
      {
        HttpWebRequest request = WebRequest.Create(TOKEN_ENDPOINT) as HttpWebRequest;
        request.Method = "POST";
        request.ContentType = "application/x-www-form-urlencoded";
        StringBuilder data = new StringBuilder();
        data.Append("client_id=" + HttpUtility.UrlEncode(CLIENT_ID));
        data.Append("&scope=" + HttpUtility.UrlEncode(MS_GRAPH_SCOPE));
        data.Append("&client_secret=" + HttpUtility.UrlEncode(CLIENT_SECERET));
        data.Append("&GRANT_TYPE=" + HttpUtility.UrlEncode(GRANT_TYPE));
        byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());
        request.ContentLength = byteData.Length;
        using (Stream postStream = request.GetRequestStream())
        {
          postStream.Write(byteData, 0, byteData.Length);
        }

        // Get response

        using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
        {

          using (var reader = new StreamReader(response.GetResponseStream()))
          {
            JavaScriptSerializer js = new JavaScriptSerializer();
            var objText = reader.ReadToEnd();
            LgObject myojb = (LgObject)js.Deserialize(objText, typeof(LgObject));
            token = myojb.access_token;
          }

        }
        return token;
      }
      catch (Exception e)
      {
        Console.WriteLine("A error happened while connect to server please check config file");
        return "error";
      }
    }

    public static GraphServiceClient GetGraphClient(string authResult)
    {
      GraphServiceClient graphClient = new GraphServiceClient(
           new DelegateAuthenticationProvider(
      async (requestMessage) =>
      {
        requestMessage.Headers.Authorization =
                  new AuthenticationHeaderValue("bearer",
                  authResult);
      }));
      return graphClient;
    }

    public static async void ChangeUserInfo(GraphServiceClient graphClient, TraceWriter Log, string userID, string jobTitle, string firstName, string lastName, string businessPhones, string streetAddress, string department, string city, string province, string postalcode,string mobilePhone, string country)
    {

      var guestUser = new User
      {
          JobTitle = jobTitle,
          Surname = lastName,
          GivenName = firstName,
          BusinessPhones = new List<String>()
            {
                businessPhones
            },
          MobilePhone = mobilePhone,
          StreetAddress = streetAddress,
          Department = department,
          City = city,
          PostalCode = postalcode,
          State = province,
          Country = country

      };

     await graphClient.Users[userID]  //USER_ID
     .Request()
     .UpdateAsync(guestUser);
    }
  }
}
