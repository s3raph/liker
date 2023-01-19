using Liker.Instagram;

namespace Liker.Tests
{
    [TestClass]
    public class UnitTest1
    {
        class TestOptions : IInstagramOptions
        {
            public string CSRFToken { get; set; }
            public string SessionID { get; set; }

            public int MaxAllowedUserProfileInfoCalls => throw new NotImplementedException();

            public string IGWWWClaim => throw new NotImplementedException();

            public string IGAjax => throw new NotImplementedException();
        }

        //[TestMethod]
        //public async Task TestMethod1()
        //{
        //    var service = new InstagramService(new TestOptions
        //    {
        //        CSRFToken = "cZPpvR2cfqISZ45eGsZ4lkbssMKHkL3H",
        //        SessionID = "49613149133%3AU2rbWOt8iu9LwW%3A7%3AAYdPQi6Id8ZD7htnJQS74Pboz7zFxfF8PCrti4xddw"
        //    });

        //    try
        //    {
        //        var followers = await service.GetUserFollowersAsync("infernalbrush", new PageOptions { PageSize = 12 });

        //        var posts = await service.GetUserFeedAsync("infernalbrush", new PageOptions { PageSize = 12 });
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}
    }
}