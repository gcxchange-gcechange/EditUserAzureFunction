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

  public interface IGraphClientWrapper
  {
    Task<object> updateUser( String userID, User guestUser );
  }
  
  public class GraphClientMock : IGraphClientWrapper
  {
      private readonly String _result;

      public GraphClientMock( String result )
      {
          _result = result;
      }

      public async Task<object> updateUser( String userID, User guestUser )
      {
          var mockResult = Task<object>.Run( () => {return _result;} );
          return await mockResult;
      }
  }

    public static class UpdateUser
    {
      public static IGraphClientWrapper _graphClientWrapper;

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
            string displayName = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "displayName", true) == 0)
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

                // Get request body
                dynamic data = await req.Content.ReadAsAsync<object>();
                userID = userID ?? data?.user.userID;
                jobTitle = jobTitle ?? data?.user.jobTitle;
                firstName = firstName ?? data?.user.firstName;
                lastName = lastName ?? data?.user.lastName;
                displayName = displayName ?? data?.user.displayName;
                businessPhones = businessPhones ?? data?.user.businessPhones;
                streetAddress = streetAddress ?? data?.user.streetAddress;
                department = department ?? data?.user.department;
                city = city ?? data?.user.city;
                province = province ?? data?.user.province;
                postalcode = postalcode ?? data?.user.postalcode;
                mobilePhone = mobilePhone ?? data?.user.mobilePhone;
                country = country ?? data?.user.country;

            // Check if userID is passed
            // return BadRequest if not present
            if (String.IsNullOrEmpty(userID))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "E0NoUserID");
            }

            if (_graphClientWrapper == null)
            {
              var authResult = GetOneAccessToken();
              _graphClientWrapper = new GraphClientWrapper(GetGraphClient(authResult));
            }

            try
            {
              ChangeUserInfo(_graphClientWrapper, log, userID, jobTitle, firstName, lastName, displayName, businessPhones, streetAddress, department, city, province, postalcode, mobilePhone, country);
            }
            catch( ServiceException e )
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "E1BadRequest");
            }

            return req.CreateResponse(HttpStatusCode.OK, "Finished");
        }

    private class GraphClientWrapper : IGraphClientWrapper
    {
      private readonly GraphServiceClient _graphClient;

      public GraphClientWrapper( GraphServiceClient graphClient )
      {
        _graphClient = graphClient;
      }

      public async Task<object> updateUser( String userID, User guestUser )
      {
        return await _graphClient.Users[userID]
                  .Request()
                  .UpdateAsync(guestUser);
      }
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

    public static Task<object> ChangeUserInfo(IGraphClientWrapper graphClient, TraceWriter Log, string userID, string jobTitle, string firstName, string lastName, string displayName, string businessPhones, string streetAddress, string department, string city, string province, string postalcode,string mobilePhone, string country)
    {
      var BusinessPhones = new List<String>();
      if (!String.IsNullOrEmpty(businessPhones))
      {
        BusinessPhones = new List<String>()
          {
              businessPhones
          };
      }
      else
      {
        BusinessPhones = new List<String>()
          {};
      }
        
      var guestUser = new User
      {
          JobTitle = jobTitle,
          Surname = lastName,
          GivenName = firstName,
          DisplayName = displayName,
          BusinessPhones = BusinessPhones,
          MobilePhone = mobilePhone,
          StreetAddress = streetAddress,
          Department = department,
          City = city,
          PostalCode = postalcode,
          State = province,
          Country = country

      };

      var result = graphClient.updateUser( userID, guestUser );

      if( result.Result != null )
      {
        Error err = new Error();
        err.Message = (String)result.Result;
        throw new ServiceException(err);
      }
 
      return result;
    }
  }
}
