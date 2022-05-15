using System.Collections;
using UnityEngine;

namespace neeksdk.Scripts.Infrastructure.SceneController
{
    public interface ICoroutineRunner
    {
        Coroutine StartCoroutine(IEnumerator coroutine);
    }
}