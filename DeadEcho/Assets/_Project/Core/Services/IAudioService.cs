namespace Project.Core.Services
{
    public interface IAudioService
    {
        void Play(string eventId);
        void Stop(string eventId);
    }
}
