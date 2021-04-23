using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Text;
using System;
using System.Net.Http;
using System.Web.Http;
using System.Collections.Generic;

namespace fncappUpdateUserdev01.Tests
{
    [TestClass]
    public class UnitTest1 : TestHelpers.FunctionTest
    {
        [TestMethod]
        public async Task ChangeUserInfo_With_Valid_ID()
        {
            // get local.settings and add to app.config 
            await LocalSettings.SetupEnvironment();

            IGraphClientWrapper graphClientWrapper = new GraphClientMock(null);
            var result = UpdateUser.ChangeUserInfo(graphClient: graphClientWrapper, Log: log, userID: "679b3ae7-2a36-4bd3-8c50-672ab22f88ca", jobTitle: null, firstName: null, lastName: null, displayName: null, businessPhones: null, streetAddress: null, department: null, city: null, province: null, postalcode: null, mobilePhone: null, country: null);
            Assert.AreEqual(null, result.Result);
        }

        // Run() tests - expect to stop before they require api client
        [TestMethod]
        public async Task Request_Query_Without_UserID()
        {
            // Create HttpRequestMessage
            var data = "{\"user\": {  } }";
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/");
            request.Content = new StringContent(data, Encoding.UTF8, "application/json");
            var httpConfig = new HttpConfiguration();
            request.SetConfiguration(httpConfig);

            var result = await UpdateUser.Run(req: request, log: log);
            Assert.AreEqual("\"E0NoUserID\"", result.Content.ReadAsStringAsync().Result);
        }
        [TestMethod]
        public async Task Request_Query_With_empty_UserID()
        {
            // Create HttpRequestMessage
            var data = "{\"user\": { \"userID\": \"\" } }";
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/");
            request.Content = new StringContent(data, Encoding.UTF8, "application/json");
            var httpConfig = new HttpConfiguration();
            request.SetConfiguration(httpConfig);

            var result = await UpdateUser.Run(req: request, log: log);
            Assert.AreEqual("\"E0NoUserID\"", result.Content.ReadAsStringAsync().Result);
        }

        [TestMethod]
        public async Task ExtractHttpData_Query_Without_UserID()
        {
            // Create HttpRequestMessage
            var data = "{\"user\": {  } }";
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/");
            request.Content = new StringContent(data, Encoding.UTF8, "application/json");
            var httpConfig = new HttpConfiguration();
            request.SetConfiguration(httpConfig);

            string errorMessage = "";
            try{
                var result = await UpdateUser.ExtractHttpData(req: request, log: log);
            }
            catch(HttpRequestException e)
            {
                errorMessage = e.Message;
            }
            Assert.AreEqual("E0NoUserID", errorMessage);
        }
        [TestMethod]
        public async Task ExtractHttpData_Query_With_empty_UserID()
        {
            // Create HttpRequestMessage
            var data = "{\"user\": { \"userID\": \"\" } }";
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/");
            request.Content = new StringContent(data, Encoding.UTF8, "application/json");
            var httpConfig = new HttpConfiguration();
            request.SetConfiguration(httpConfig);

            string errorMessage = "";
            try{
                var result = await UpdateUser.ExtractHttpData(req: request, log: log);
            }
            catch(HttpRequestException e)
            {
                errorMessage = e.Message;
            }
            Assert.AreEqual("E0NoUserID", errorMessage);
        }
        [TestMethod]
        public async Task ExtractHttpData_Query_With_All_Parameters()
        {
            Dictionary<string,string> expectedData = new Dictionary<string,string>();
            expectedData.Add("userID", "679b3ae7-2a36-4bd3-8c50-672ab22f88ca");
            expectedData.Add("jobTitle", "Unit Test");
            expectedData.Add("firstName", "Foo");
            expectedData.Add("lastName", "Bar");
            expectedData.Add("displayName", "Foo Bar");
            expectedData.Add("businessPhones", "123-456-7890");
            expectedData.Add("streetAddress", "0 North Pole");
            expectedData.Add("department", "Testing");
            expectedData.Add("city", "Santa's Workshop");
            expectedData.Add("province", "Santa's");
            expectedData.Add("postalcode", "HOH OHO");
            expectedData.Add("mobilePhone", "012-345-6789");
            expectedData.Add("country", "North Pole");
            
            // Create HttpRequestMessage
            var data = $@"{{""user"": {{""userID"": ""{expectedData["userID"]}"",
                        ""jobTitle"": ""{expectedData["jobTitle"]}"",
                        ""firstName"": ""{expectedData["firstName"]}"",
                        ""lastName"": ""{expectedData["lastName"]}"",
                        ""displayName"": ""{expectedData["displayName"]}"",
                        ""businessPhones"": ""{expectedData["businessPhones"]}"",
                        ""streetAddress"": ""{expectedData["streetAddress"]}"",
                        ""department"": ""{expectedData["department"]}"",
                        ""city"": ""{expectedData["city"]}"",
                        ""province"": ""{expectedData["province"]}"",
                        ""postalcode"": ""{expectedData["postalcode"]}"",
                        ""mobilePhone"": ""{expectedData["mobilePhone"]}"",
                        ""country"": ""{expectedData["country"]}"",
                        }} }}";

            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/");
            request.Content = new StringContent(data, Encoding.UTF8, "application/json");
            var httpConfig = new HttpConfiguration();
            request.SetConfiguration(httpConfig);

            string errorMessage = "";
            Dictionary<string,string> result = new Dictionary<string,string>();
            try{
                result = await UpdateUser.ExtractHttpData(req: request, log: log);
            }
            catch(HttpRequestException e)
            {
                errorMessage = e.Message;
            }
            Assert.AreEqual("", errorMessage);
            Assert.AreEqual(expectedData["userID"], result["userID"]);
            Assert.AreEqual(expectedData["jobTitle"], result["jobTitle"]);
            Assert.AreEqual(expectedData["firstName"], result["firstName"]);
            Assert.AreEqual(expectedData["lastName"], result["lastName"]);
            Assert.AreEqual(expectedData["displayName"], result["displayName"]);
            Assert.AreEqual(expectedData["businessPhones"], result["businessPhones"]);
            Assert.AreEqual(expectedData["streetAddress"], result["streetAddress"]);
            Assert.AreEqual(expectedData["department"], result["department"]);
            Assert.AreEqual(expectedData["city"], result["city"]);
            Assert.AreEqual(expectedData["province"], result["province"]);
            Assert.AreEqual(expectedData["postalcode"], result["postalcode"]);
            Assert.AreEqual(expectedData["mobilePhone"], result["mobilePhone"]);
            Assert.AreEqual(expectedData["country"], result["country"]);
        }

        [TestMethod]
        public async Task ChangeUserInfo_Query_With_Invalid_ID()
        {
            // get local.settings and add to app.config 
            await LocalSettings.SetupEnvironment();

            string errorMessage = "";
            IGraphClientWrapper graphClientWrapper = new GraphClientMock("Invalid ID");
            
            try{
                var result = UpdateUser.ChangeUserInfo(graphClient: graphClientWrapper, Log: log, userID: "0000-0000-000", jobTitle: null, firstName: null, lastName: null, displayName: null, businessPhones: null, streetAddress: null, department: null, city: null, province: null, postalcode: null, mobilePhone: null, country: null);
            }
            catch(Exception e)
            {
                errorMessage = e.Message;
            }
            
            Assert.AreEqual("Message: Invalid ID", errorMessage.Trim());
        }
    }
}
