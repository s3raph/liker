namespace Liker.Instagram
{
    public interface IInstagramOptions
    {
        string CSRFToken { get; set; }
        string SessionID { get; set; }
    }
}