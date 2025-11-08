using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene"; // replace with your actual game scene name

    private void Start()
    {
        StartCoroutine(LoadGameScene());
    }

    IEnumerator LoadGameScene()
    {
        // Start async loading
        AsyncOperation operation = SceneManager.LoadSceneAsync(gameSceneName);
        operation.allowSceneActivation = false;

        // Optional: short delay to let "RangeLoadingScene" show up briefly
        yield return new WaitForSeconds(1f);

        // Now switch to game scene
        operation.allowSceneActivation = true;
    }
}