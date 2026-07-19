using System.Collections;
using UnityEngine;

namespace Project.Core.Utils
{
    public interface ICoroutineRunner
    {
        Coroutine Run(IEnumerator routine);
        void Stop(Coroutine routine);
        void StopAll();
    }
}