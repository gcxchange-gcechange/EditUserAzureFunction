using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Text;
using System;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Azure.WebJobs.Host;

namespace UpdateUserHttp.Tests
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
