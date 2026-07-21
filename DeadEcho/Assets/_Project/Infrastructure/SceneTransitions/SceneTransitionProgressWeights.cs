namespace Project.Infrastructure.SceneTransitions
{
    public sealed class SceneTransitionProgressWeights
    {
        public float OpeningEnd { get; set; } = 0.05f;
        public float LoadingEnd { get; set; } = 0.70f;
        public float ActivationEnd { get; set; } = 0.78f;
        public float InitializationEnd { get; set; } = 0.92f;
        public float WarmupEnd { get; set; } = 0.98f;
    }
}
