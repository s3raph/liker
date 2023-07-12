using Moq;

namespace Liker.Tests
{
    [TestClass]
    public class RetryInterceptorTests
    {
        [TestMethod]
        public void RetryInterceptor_InvokesMethod_WhenNoExceptionsThrown()
        {
            // Arrange
            var asyncTarget = async () => await Task.FromResult(42);
            var scheduler   = new TestableScheduler();
            var interceptor = new RetryInterceptor(scheduler);
            var invocation  = new Mock<Ninject.Extensions.Interception.IInvocation>();
            invocation.Setup(x => x.Proceed()).Callback(() => { invocation.Object.ReturnValue = asyncTarget(); });
            invocation.Setup(x => x.Clone()).Returns(invocation.Object);

            // Act
            interceptor.Intercept(invocation.Object);
            scheduler.RunAll();

            // Assert
            invocation.Verify(x => x.Proceed(), Times.Once);

            Assert.AreEqual(Task.CompletedTask, invocation.Object.ReturnValue);
        }
    }

    class TestableScheduler : TaskScheduler
    {
        private readonly Queue<Task> m_taskQueue = new();

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return m_taskQueue;
        }

        protected override void QueueTask(Task task)
        {
            m_taskQueue.Enqueue(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (task.Status == TaskStatus.WaitingToRun)
            {
                //task.RunSynchronously();
            }

            return true;
        }

        public void RunAll()
        {
            while (m_taskQueue.Count > 0)
            {
                var task = m_taskQueue.Dequeue();

                //if (task.Status == TaskStatus.WaitingToRun)
                //{
                //    task.RunSynchronously();
                //}

                //task.RunSynchronously();
            }
        }
    }
}
