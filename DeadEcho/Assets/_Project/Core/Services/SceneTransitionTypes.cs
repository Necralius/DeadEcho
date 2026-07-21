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

    public enum SceneTransitionErrorPolicy
    {
        Recoverable,
        Retryable,
        Fatal
    }

    public enum SceneTransitionErrorCode
    {
        Unknown,
        SceneNotFound,
        LoadingStartFailed,
        InitializationFailed,
        InvalidSave,
        CorruptedSave,
        AddressablesFailed,
        Timeout,
        LifetimeScopeMissing,
        CriticalInitializerMissing,
        Cancelled,
        UnloadFailed,
        SetActiveSceneFailed
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
        public int StabilizationFrameCount { get; set; } = 2;
        public bool RequireSceneLifetimeScope { get; set; } = true;
        public string MainMenuSceneName { get; set; } = "Menu";
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
        public SceneTransitionError(
            string title,
            string message,
            Exception exception = null,
            SceneTransitionErrorCode code = SceneTransitionErrorCode.Unknown,
            SceneTransitionErrorPolicy policy = SceneTransitionErrorPolicy.Recoverable,
            bool canRetry = false,
            bool canReturnToMainMenu = true)
        {
            Title = string.IsNullOrWhiteSpace(title) ? "Loading Error" : title;
            Message = string.IsNullOrWhiteSpace(message) ? "The scene could not be loaded." : message;
            Exception = exception;
            Code = code;
            Policy = policy;
            CanRetry = canRetry;
            CanReturnToMainMenu = canReturnToMainMenu;
        }

        public string Title { get; }
        public string Message { get; }
        public Exception Exception { get; }
        public SceneTransitionErrorCode Code { get; }
        public SceneTransitionErrorPolicy Policy { get; }
        public bool CanRetry { get; }
        public bool CanReturnToMainMenu { get; }
    }

    public sealed class SceneTransitionDiagnostics
    {
        public string TransitionId { get; set; }
        public string PreviousSceneName { get; set; }
        public string TargetSceneName { get; set; }
        public SceneTransitionStatus FinalStatus { get; set; }
        public float LoadingScreenOpenDuration { get; set; }
        public float SceneLoadDuration { get; set; }
        public float ActivationDuration { get; set; }
        public float InitializationDuration { get; set; }
        public float WarmupDuration { get; set; }
        public float UnloadDuration { get; set; }
        public float TotalTransitionDuration { get; set; }
        public string CurrentInitializer { get; set; }
        public string CurrentWarmupStep { get; set; }
    }

    public sealed class SceneTransitionResult
    {
        private SceneTransitionResult(
            SceneTransitionStatus status,
            string targetSceneName,
            SceneTransitionError error,
            SceneTransitionDiagnostics diagnostics)
        {
            Status = status;
            TargetSceneName = targetSceneName;
            Error = error;
            Diagnostics = diagnostics;
        }

        public SceneTransitionStatus Status { get; }
        public string TargetSceneName { get; }
        public SceneTransitionError Error { get; }
        public SceneTransitionDiagnostics Diagnostics { get; }
        public bool Succeeded => Status == SceneTransitionStatus.Completed;

        public static SceneTransitionResult Completed(string targetSceneName, SceneTransitionDiagnostics diagnostics = null)
        {
            return new SceneTransitionResult(SceneTransitionStatus.Completed, targetSceneName, null, diagnostics);
        }

        public static SceneTransitionResult Failed(string targetSceneName, SceneTransitionError error, SceneTransitionDiagnostics diagnostics = null)
        {
            return new SceneTransitionResult(SceneTransitionStatus.Failed, targetSceneName, error, diagnostics);
        }

        public static SceneTransitionResult Cancelled(string targetSceneName, SceneTransitionDiagnostics diagnostics = null)
        {
            var error = new SceneTransitionError(
                "Loading Cancelled",
                "O carregamento foi cancelado.",
                code: SceneTransitionErrorCode.Cancelled,
                policy: SceneTransitionErrorPolicy.Recoverable,
                canRetry: true,
                canReturnToMainMenu: true);
            return new SceneTransitionResult(SceneTransitionStatus.Cancelled, targetSceneName, error, diagnostics);
        }
    }
}
