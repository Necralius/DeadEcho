using Project.Core.Services;
using UnityEngine.Audio;

namespace Project.Infrastructure.Settings
{
    public sealed class UnityAudioSettingsService : IAudioSettingsService
    {
        private const string MasterVolumeParameter = "MasterVolume";
        private const string MusicVolumeParameter = "MusicVolume";
        private const string SfxVolumeParameter = "SfxVolume";
        private readonly AudioMixer _audioMixer;
        private readonly ISettingsStorage _storage;
        private GameSettingsData _data;

        public UnityAudioSettingsService(ISettingsStorage storage)
            : this(null, storage)
        {
        }

        private UnityAudioSettingsService(AudioMixer audioMixer, ISettingsStorage storage)
        {
            _audioMixer = audioMixer;
            _storage = storage;
            _data = storage != null && storage.TryLoad(out GameSettingsData loaded) ? loaded : new GameSettingsData();
            ApplyAll();
        }

        public float MasterVolume
        {
            get => _data.MasterVolume;
            set => SetVolume(ref _data.MasterVolume, value, MasterVolumeParameter);
        }

        public float MusicVolume
        {
            get => _data.MusicVolume;
            set => SetVolume(ref _data.MusicVolume, value, MusicVolumeParameter);
        }

        public float SfxVolume
        {
            get => _data.SfxVolume;
            set => SetVolume(ref _data.SfxVolume, value, SfxVolumeParameter);
        }

        public bool MuteAll
        {
            get => _data.MuteAll;
            set
            {
                _data.MuteAll = value;
                ApplyAll();
                Save();
            }
        }

        public void RestoreDefaults()
        {
            _data.MasterVolume = 1f;
            _data.MusicVolume = 1f;
            _data.SfxVolume = 1f;
            _data.MuteAll = false;
            ApplyAll();
            Save();
        }

        public static float LinearToDecibels(float value)
        {
            return value <= 0.0001f ? -80f : UnityEngine.Mathf.Log10(UnityEngine.Mathf.Clamp01(value)) * 20f;
        }

        private void SetVolume(ref float target, float value, string mixerParameter)
        {
            target = UnityEngine.Mathf.Clamp01(value);
            SetMixerVolume(mixerParameter, target);
            Save();
        }

        private void ApplyAll()
        {
            SetMixerVolume(MasterVolumeParameter, _data.MuteAll ? 0f : _data.MasterVolume);
            SetMixerVolume(MusicVolumeParameter, _data.MuteAll ? 0f : _data.MusicVolume);
            SetMixerVolume(SfxVolumeParameter, _data.MuteAll ? 0f : _data.SfxVolume);
        }

        private void SetMixerVolume(string parameter, float value)
        {
            _audioMixer?.SetFloat(parameter, LinearToDecibels(value));
        }

        private void Save()
        {
            _storage?.Save(_data);
        }
    }
}
