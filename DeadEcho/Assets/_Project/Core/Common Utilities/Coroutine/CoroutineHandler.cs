using System.Collections;
using UnityEngine;

namespace Project.Core.Utils
{
    public sealed class CoroutineHandler : Singleton<CoroutineHandler>, ICoroutineRunner
    {
        private void Start() => gameObject.name = "[CoroutineHandler]";

        /// <summary>
        /// Runs a Coroutine via static access.
        /// </summary>
        public Coroutine Run(IEnumerator routine)
        {
            if (routine == null)
                return null;

            return Instance.StartCoroutine(routine);
        }

        /// <summary>
        /// Stops a especific Coroutine via static access.
        /// </summary>
        public void Stop(Coroutine coroutine) => Instance.StopCoroutine(coroutine);

        /// <summary>
        /// Stops all Coroutines running on this handler.
        /// </summary>
        public void StopAll() => Instance.StopAllCoroutines();
    }
}