using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Project.Core.Services
{
    public interface ISceneTransitionService
    {
        bool IsTransitioning { get; }
        SceneTransitionStatus CurrentStatus { get; }

        Task<SceneTransitionResult> TransitionAsync(
            SceneTransitionRequest request,
            CancellationToken cancellationToken = default);
    }

    public interface ISceneLoadingView
    {
        Task ShowAsync(CancellationToken cancellationToken = default);
        void SetProgress(float normalizedProgress);
        void SetStatus(string status);
        void SetTip(string tip);
        void ShowError(
            SceneTransitionError error,
            Func<Task> retryAsync = null,
            Func<Task> returnToMainMenuAsync = null);
        Task HideAsync(CancellationToken cancellationToken = default);
    }

    public interface ISceneReadinessService
    {
        Task<SceneReadinessResult> InitializeAsync(
            SceneInitializationContext context,
            IProgress<float> progress,
            CancellationToken cancellationToken = default);

        Task<SceneReadinessResult> WarmUpAsync(
            SceneWarmupContext context,
            IProgress<float> progress,
            CancellationToken cancellationToken = default);

        Task<SceneReadinessResult> ValidateAsync(
            SceneInitializationContext context,
            CancellationToken cancellationToken = default);
    }

    public interface ISceneInitializer
    {
        int Order { get; }

        Task InitializeAsync(
            SceneInitializationContext context,
            IProgress<float> progress,
            CancellationToken cancellationToken = default);
    }

    public interface ISceneWarmupStep
    {
        int Order { get; }
        float Weight { get; }

        Task WarmupAsync(
            SceneWarmupContext context,
            IProgress<float> progress,
            CancellationToken cancellationToken = default);
    }

    public interface ISceneGameplayGate
    {
        bool IsGameplayBlocked { get; }
        void Block();
        void Release();
    }

    public interface IScenePayloadStore
    {
        bool HasPayload { get; }
        ScenePayload Peek();
        ScenePayload Consume();
        void Store(ScenePayload payload);
        void Clear();
    }

    public interface ISceneScopeReadinessProvider
    {
        bool HasSceneScope(Scene scene);
        IReadOnlyList<ISceneInitializer> GetInitializers(Scene scene);
        IReadOnlyList<ISceneWarmupStep> GetWarmupSteps(Scene scene);
    }
}
