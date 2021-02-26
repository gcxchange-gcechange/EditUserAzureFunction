using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Text;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Azure.WebJobs.Host;

namespace UpdateUserHttp.Tests
{
    [TestClass]
    public class UnitTest1 : TestHelpers.FunctionTest
    {
        [TestMethod]
        public async Task Request_With_Query()
        {
            // get local.settings and add to app.config 
            await LocalSettings.SetupEnvironment();

            // Create HttpRequestMessage
            var data = "{\"user\": { \"userID\": \"679b3ae7-2a36-4bd3-8c50-672ab22f88ca\" } }";
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/");
            request.Content = new StringContent(data, Encoding.UTF8, "application/json");
            var httpConfig = new HttpConfiguration();
            request.SetConfiguration(httpConfig);

            var result = await UpdateUser.Run(req: request, log: log);
            Assert.AreEqual("\"Finished\"", result.Content.ReadAsStringAsync().Result);
        }

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
        public async Task Request_Query_With_Invalid_ID()
        {
            // get local.settings and add to app.config 
            await LocalSettings.SetupEnvironment();

            // Create HttpRequestMessage
            var data = "{\"user\": { \"userID\": \"0000-0000-000\" } }";
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/");
            request.Content = new StringContent(data, Encoding.UTF8, "application/json");
            var httpConfig = new HttpConfiguration();
            request.SetConfiguration(httpConfig);

            var result = await UpdateUser.Run(req: request, log: log);
            Assert.AreEqual("\"E1BadRequest\"", result.Content.ReadAsStringAsync().Result);
        }
    }
}
