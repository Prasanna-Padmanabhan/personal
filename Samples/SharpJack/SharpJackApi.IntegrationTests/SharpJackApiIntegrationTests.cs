using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace SharpJackApi.IntegrationTests
{
    [TestClass]
    public class SharpJackApiIntegrationTests
    {
        [TestMethod]
        public async Task SimpleTest()
        {
            using (var client = new SharpJackApiClient("https://sharpjackapi.azurewebsites.net"))
            {
                var player1 = await client.AddPlayerAsync("humptydumpty");
                var player2 = await client.GetPlayerAsync(player1.Id);
                Assert.AreEqual(player1.Name, player2.Name);
            }
        }
    }
}
