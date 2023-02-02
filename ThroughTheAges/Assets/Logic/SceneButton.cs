using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneButton : MonoBehaviour
{
    public int sceneIndex = -1;
    private void Start() {
        GetComponent<Button>().onClick.AddListener(() => {
            SceneLoading.Instance.LoadScene(sceneIndex);
        });
    }
}
