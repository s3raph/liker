using Liker.Instagram;

namespace Liker.Persistence
{
    public interface IDatabase
    {
        Task<IReadOnlyCollection<AccountFollower>> GetFollowers(params string[] followerPks);
        Task InsertFollower(AccountFollower toInsert);
    }

    public static class DatabaseExtensions
    {
        public static async Task<IReadOnlyDictionary<string, AccountFollower>> GetFollowersDictionary(this IDatabase instance, params string[] followerPks) => (await instance.GetFollowers(followerPks)).ToDictionary(f => f.UserID, f => f);
    }
}