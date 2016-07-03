namespace Helpmebot.Repositories.Interfaces
{
    public interface IRemoteSendRepository
    {
        bool CanRemoteSend(string from, string to);
    }
}