using UnityEngine;

public class SceneLoading : MonoBehaviour
{
    public static SceneLoading Instance { get; private set; }
    private void Awake() {
        if(Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void LoadScene(int sceneIndex) {
        if(sceneIndex < 0) {
            sceneIndex = PlayerPrefs.GetInt("LastScene", 2);
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
        if(sceneIndex > 2) {
            PlayerPrefs.SetInt("LastScene", sceneIndex);
        }
    }

    private void OnDestroy() {
        PlayerPrefs.Save();
    }
}
