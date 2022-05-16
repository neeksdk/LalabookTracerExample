using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace neeksdk.Scripts.Infrastructure.SceneController
{
    public class SceneLoader
    {
        private readonly ICoroutineRunner _coroutineRunner;
        private bool _isLoading;

        public SceneLoader(ICoroutineRunner coroutineRunner) => _coroutineRunner = coroutineRunner;

        public void Load(string name, Action onLoaded = null)
        {
            if (_isLoading)
            {
                return;
            }
            
            _coroutineRunner.StartCoroutine((LoadScene(name, onLoaded)));
        }

        private IEnumerator LoadScene(string nextScene, Action onLoaded = null)
        {
            if (SceneManager.GetActiveScene().name == nextScene)
            {
                onLoaded?.Invoke();
                yield break;
            }

            _isLoading = true;
            AsyncOperation waitNextScene = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Single);

            while (!waitNextScene.isDone)
                yield return null;

            onLoaded?.Invoke();
            _isLoading = false;
        }
    }
}