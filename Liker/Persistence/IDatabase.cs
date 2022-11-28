using Liker.Instagram;

namespace Liker.Persistence
{
    public interface IDatabase
    {
        Task<IReadOnlyCollection<AccountFollower>> GetFollowersAsync(params string[] followerPks);
        Task InsertFollowerAsync(AccountFollower toInsert);
        Task<Account> GetAccountAsync(string accountUserName);
        Task SetAccountNextMaxIdAsync(string accountUserName, string nextMaxId);
        Task DeleteAccountAsync(string accountUserName);
    }

    public static class DatabaseExtensions
    {
        public static async Task<IReadOnlyDictionary<string, AccountFollower>> GetFollowersDictionaryAsync(this IDatabase instance, params string[] followerPks) => (await instance.GetFollowersAsync(followerPks)).ToDictionary(f => f.UserID, f => f);
    }
}