using Microsoft.VisualStudio.TestTools.UnitTesting;
using SemanticWebNPLSearchEngine.Classes;

namespace SemanticSearchEngine.Test.Classes
{
    [TestClass]
    public class utilitiesTest
    {
        [TestMethod]
        public void RemoveLast3Cahracters()
        {
            string test = "hello world^^sadfs";
            string expected = "hello world";

            var output = utilities.RemoveLast3Cahracters(test);

            Assert.AreEqual(test, expected);
        }
    }
}
