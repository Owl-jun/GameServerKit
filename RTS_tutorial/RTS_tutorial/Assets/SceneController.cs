using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }
    private Dictionary<SceneType, string> sceneMap = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitSceneMap();
    }

    private void InitSceneMap()
    {
        sceneMap[SceneType.Login] = "LoginScene";
        sceneMap[SceneType.Sample] = "SampleScene";
        // 필요한 씬 추가
    }

    public void LoadScene(SceneType type)
    {
        if (!sceneMap.ContainsKey(type))
        {
            Debug.LogError($"[SceneController] 등록되지 않은 씬: {type}");
            return;
        }

        string sceneName = sceneMap[type];
        SceneManager.LoadScene(sceneName);
    }

    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public enum SceneType
    {
        Login,
        Sample
    }
}
