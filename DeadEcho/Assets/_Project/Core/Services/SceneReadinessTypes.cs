using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Project.Core.Services
{
    public sealed class SceneInitializationContext
    {
        public SceneInitializationContext(
            Scene scene,
            ScenePayload payload,
            SceneTransitionRequest request)
        {
            Scene = scene;
            Payload = payload;
            Request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public Scene Scene { get; }
        public ScenePayload Payload { get; }
        public SceneTransitionRequest Request { get; }
    }

    public sealed class SceneWarmupContext
    {
        public SceneWarmupContext(
            Scene scene,
            ScenePayload payload,
            SceneTransitionRequest request)
        {
            Scene = scene;
            Payload = payload;
            Request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public Scene Scene { get; }
        public ScenePayload Payload { get; }
        public SceneTransitionRequest Request { get; }
    }

    public sealed class SceneInitializationError
    {
        public SceneInitializationError(string systemName, string message, Exception exception = null)
        {
            SystemName = string.IsNullOrWhiteSpace(systemName) ? "Unknown" : systemName;
            Message = string.IsNullOrWhiteSpace(message) ? "Scene initialization failed." : message;
            Exception = exception;
        }

        public string SystemName { get; }
        public string Message { get; }
        public Exception Exception { get; }
    }

    public sealed class SceneReadinessResult
    {
        public SceneReadinessResult(
            bool isReady,
            IReadOnlyList<string> pendingSystems = null,
            IReadOnlyList<SceneInitializationError> errors = null)
        {
            IsReady = isReady;
            PendingSystems = pendingSystems ?? Array.Empty<string>();
            Errors = errors ?? Array.Empty<SceneInitializationError>();
        }

        public bool IsReady { get; }
        public IReadOnlyList<string> PendingSystems { get; }
        public IReadOnlyList<SceneInitializationError> Errors { get; }

        public static SceneReadinessResult Ready()
        {
            return new SceneReadinessResult(true);
        }

        public static SceneReadinessResult Pending(params string[] pendingSystems)
        {
            return new SceneReadinessResult(false, pendingSystems ?? Array.Empty<string>());
        }

        public static SceneReadinessResult Failed(params SceneInitializationError[] errors)
        {
            return new SceneReadinessResult(false, Array.Empty<string>(), errors ?? Array.Empty<SceneInitializationError>());
        }
    }
}
