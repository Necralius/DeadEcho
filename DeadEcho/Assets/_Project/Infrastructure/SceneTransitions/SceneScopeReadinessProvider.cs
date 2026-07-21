using System;
using System.Collections.Generic;
using Project.Core.Services;
using Project.Infrastructure.DependencyInjection;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace Project.Infrastructure.SceneTransitions
{
    public sealed class SceneScopeReadinessProvider : ISceneScopeReadinessProvider
    {
        private static readonly IReadOnlyList<ISceneInitializer> EmptyInitializers = Array.Empty<ISceneInitializer>();
        private static readonly IReadOnlyList<ISceneWarmupStep> EmptyWarmupSteps = Array.Empty<ISceneWarmupStep>();

        public bool HasSceneScope(Scene scene)
        {
            return FindScope(scene) != null;
        }

        public IReadOnlyList<ISceneInitializer> GetInitializers(Scene scene)
        {
            SceneLifetimeScope scope = FindScope(scene);
            if (scope == null || scope.Container == null)
                return EmptyInitializers;

            return ResolveCollection<ISceneInitializer>(scope.Container);
        }

        public IReadOnlyList<ISceneWarmupStep> GetWarmupSteps(Scene scene)
        {
            SceneLifetimeScope scope = FindScope(scene);
            if (scope == null || scope.Container == null)
                return EmptyWarmupSteps;

            return ResolveCollection<ISceneWarmupStep>(scope.Container);
        }

        private static IReadOnlyList<T> ResolveCollection<T>(IObjectResolver resolver)
        {
            try
            {
                return resolver.Resolve<IReadOnlyList<T>>() ?? Array.Empty<T>();
            }
            catch (VContainerException)
            {
                return Array.Empty<T>();
            }
        }

        private static SceneLifetimeScope FindScope(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
                return null;

            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].TryGetComponent(out SceneLifetimeScope directScope))
                    return directScope;

                SceneLifetimeScope childScope = roots[i].GetComponentInChildren<SceneLifetimeScope>(true);
                if (childScope != null)
                    return childScope;
            }

            return null;
        }
    }
}
