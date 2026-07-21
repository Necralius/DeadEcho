using System;

namespace Project.Core.Services
{
    public enum SceneTransitionMode
    {
        ReplaceCurrent,
        Additive
    }

    public enum SceneTransitionStatus
    {
        Idle,
        OpeningLoadingScreen,
        Preparing,
        LoadingScene,
        WaitingForActivation,
        ActivatingScene,
        ResolvingSceneScope,
        InitializingScene,
        WarmingUpScene,
        SwitchingActiveScene,
        UnloadingPreviousScene,
        ClosingLoadingScreen,
        Completed,
        Failed,
        Cancelled
    }

    public sealed class SceneTransitionRequest
    {
        public SceneTransitionRequest(string targetSceneName)
        {
            TargetSceneName = targetSceneName;
        }

        public string TargetSceneName { get; }
        public SceneTransitionMode Mode { get; set; } = SceneTransitionMode.ReplaceCurrent;
        public bool SetAsActiveScene { get; set; } = true;
        public bool UnloadPreviousScene { get; set; } = true;
        public float MinimumLoadingScreenDuration { get; set; } = 0.5f;
        public float InitializationTimeoutSeconds { get; set; } = 30f;
        public ScenePayload Payload { get; set; }
    }

    public readonly struct SceneTransitionProgress
    {
        public SceneTransitionProgress(SceneTransitionStatus status, float normalizedProgress)
        {
            Status = status;
            NormalizedProgress = Math.Max(0f, Math.Min(1f, normalizedProgress));
        }

        public SceneTransitionStatus Status { get; }
        public float NormalizedProgress { get; }
    }

    public sealed class SceneTransitionError
    {
        public SceneTransitionError(string title, string message, Exception exception = null)
        {
            Title = string.IsNullOrWhiteSpace(title) ? "Loading Error" : title;
            Message = string.IsNullOrWhiteSpace(message) ? "The scene could not be loaded." : message;
            Exception = exception;
        }

        public string Title { get; }
        public string Message { get; }
        public Exception Exception { get; }
    }

    public sealed class SceneTransitionResult
    {
        private SceneTransitionResult(
            SceneTransitionStatus status,
            string targetSceneName,
            SceneTransitionError error)
        {
            Status = status;
            TargetSceneName = targetSceneName;
            Error = error;
        }

        public SceneTransitionStatus Status { get; }
        public string TargetSceneName { get; }
        public SceneTransitionError Error { get; }
        public bool Succeeded => Status == SceneTransitionStatus.Completed;

        public static SceneTransitionResult Completed(string targetSceneName)
        {
            return new SceneTransitionResult(SceneTransitionStatus.Completed, targetSceneName, null);
        }

        public static SceneTransitionResult Failed(string targetSceneName, SceneTransitionError error)
        {
            return new SceneTransitionResult(SceneTransitionStatus.Failed, targetSceneName, error);
        }

        public static SceneTransitionResult Cancelled(string targetSceneName)
        {
            return new SceneTransitionResult(SceneTransitionStatus.Cancelled, targetSceneName, null);
        }
    }
}
