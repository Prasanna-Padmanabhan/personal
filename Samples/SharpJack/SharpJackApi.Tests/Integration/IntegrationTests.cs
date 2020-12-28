using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharpJackApi.Tests
{
    [TestClass]
    public class IntegrationTests
    {
        [TestMethod]
        public void TwoPlayersOneRound()
        {
            Tests<SharpJackApiClient>.TwoPlayersOneRound();
        }
    }
}
