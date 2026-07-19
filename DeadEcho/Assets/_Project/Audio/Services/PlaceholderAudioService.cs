using Project.Core.Services;
using UnityEngine;

namespace Project.Audio.Services
{
    public sealed class PlaceholderAudioService : IAudioService
    {
        public void Play(string eventId)
        {
            Debug.Log($"[PlaceholderAudioService] Play requested for '{eventId}'.");
        }

        public void Stop(string eventId)
        {
            Debug.Log($"[PlaceholderAudioService] Stop requested for '{eventId}'.");
        }
    }
}
