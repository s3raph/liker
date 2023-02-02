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

        [TestMethod]
        public async Task RetryStatusCodeZero_Basic()
        {
            Assert.AreEqual(1, await Process.RetryStatusCodeZero(() => Task.FromResult(1)));
        }

        [TestMethod]
        public async Task RetryStatusCodeZero_ThrowsOnFirstInvocation()
        {
            int invocationCount = 0;

            var result = await Process.RetryStatusCodeZero(() =>
            {
                invocationCount++;

                if (invocationCount == 1)
                {
                    throw new InstagramRESTException("Waffles", default);
                }

                return Task.FromResult(invocationCount);
            });

            Assert.AreEqual(2, invocationCount);
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public async Task RetryStatusCodeZero_PropagatesAfterFirstHandledException()
        {
            int invocationCount = 0;

            await Assert.ThrowsExceptionAsync<InstagramRESTException>(() =>
                Process.RetryStatusCodeZero<int>(() =>
                {
                    invocationCount++;

                    throw new InstagramRESTException("Waffles", default);
                }));

            Assert.AreEqual(2, invocationCount);
        }

        [TestMethod]
        public async Task RetryStatusCodeZero_PropagatesNonZeroStatusCodes()
        {
            int invocationCount = 0;

            await Assert.ThrowsExceptionAsync<InstagramRESTException>(() =>
                Process.RetryStatusCodeZero<int>(() =>
                {
                    invocationCount++;

                    throw new InstagramRESTException("Waffles", default);
                }));

            Assert.AreEqual(1, invocationCount);
        }

        [TestMethod]
        public async Task RetryStatusCodeZero_PropagatesNonRESTExceptions()
        {
            int invocationCount = 0;

            await Assert.ThrowsExceptionAsync<Exception>(() =>
                Process.RetryStatusCodeZero<int>(() =>
                {
                    invocationCount++;

                    throw new Exception("Waffles");
                }));

            Assert.AreEqual(1, invocationCount);
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
