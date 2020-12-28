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
    }
}