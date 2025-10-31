using UnityEngine;
using UnityEngine.SceneManagement;

public class ReloadSceneOnKey : MonoBehaviour
{
    [SerializeField]
    private KeyCode reloadKey = KeyCode.P;

    private void Update()
    {
        if (Input.GetKeyDown(reloadKey))
        {
            var activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.name);
        }
    }
}


