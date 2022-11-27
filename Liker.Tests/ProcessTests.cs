using Liker.Instagram;
using Liker.Logic;
using Moq;

namespace Liker.Tests
{
    [TestClass]
    public class ProcessTests
    {
        [TestMethod]
        public void DoesTextContainAnyHashTags_NullInput()
        {
            Process instance = MakeProcessInstance();

            Assert.IsFalse(instance.DoesTextContainAnyHashTags(null));
        }

        private static Process MakeProcessInstance()
        {
            var instaService = new Mock<IInstagramService>();
            var db           = new Mock<Persistence.IDatabase>();
            var options      = new Mock<IProcessOptions>();
            var instance     = new Logic.Process(instaService.Object, db.Object, options.Object);
            return instance;
        }
    }
}
