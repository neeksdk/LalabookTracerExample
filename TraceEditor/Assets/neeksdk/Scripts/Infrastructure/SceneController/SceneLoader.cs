using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace neeksdk.Scripts.Infrastructure.SceneController
{
    public class SceneLoader
    {
        private readonly ICoroutineRunner _coroutineRunner;

        public SceneLoader(ICoroutineRunner coroutineRunner) => _coroutineRunner = coroutineRunner;

        public void Load(string name, Action onLoaded = null) => _coroutineRunner.StartCoroutine((LoadScene(name, onLoaded)));

        private IEnumerator LoadScene(string nextScene, Action onLoaded = null)
        {
            if (SceneManager.GetActiveScene().name == nextScene)
            {
                onLoaded?.Invoke();
                yield break;
            }
      
            AsyncOperation waitNextScene = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Single);

            while (!waitNextScene.isDone)
                yield return null;

            UnloadUnusedScenes();
            onLoaded?.Invoke();
        }

        private void UnloadUnusedScenes()
        {
            string activeSceneName = SceneManager.GetActiveScene().name;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name != activeSceneName)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }
        }
    }
}