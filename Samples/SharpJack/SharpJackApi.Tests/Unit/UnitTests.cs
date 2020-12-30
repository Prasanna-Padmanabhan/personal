using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharpJackApi.Tests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void TwoPlayersOneRound()
        {
            Tests<SharpJackServiceClient>.TwoPlayersOneRound();
        }
        
        [TestMethod]
        public void ManyPlayersOneRound()
        {
            Tests<SharpJackServiceClient>.ManyPlayersOneRound();
        }

        [TestMethod]
        public void RecreateDatabase()
        {
            // place holder to trigger clean up of database
            //var client = new SharpJackServiceClient();
            //client.Context.Database.EnsureDeleted();
            //client.Context.Database.EnsureCreated();
        }
    }
}