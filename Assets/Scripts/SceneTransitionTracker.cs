using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// シーン遷移時に「直前のシーン名」を記録しておく常駐トラッカー。
/// </summary>
public class SceneTransitionTracker : MonoBehaviour
{
    private static SceneTransitionTracker instance;
    public static SceneTransitionTracker Instance
    {
        get
        {
            if (instance == null)
            {
                // 自動生成（どのシーンにも存在しない場合）
                GameObject go = new GameObject("SceneTransitionTracker");
                instance = go.AddComponent<SceneTransitionTracker>();
            }
            return instance;
        }
    }

    /// <summary>
    /// 直前のシーン名（初回は空文字の場合あり）
    /// </summary>
    public string PreviousSceneName { get; private set; } = string.Empty;

    /// <summary>
    /// 現在のシーン名
    /// </summary>
    public string CurrentSceneName { get; private set; } = string.Empty;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false; // デバッグログを表示するか

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        CurrentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.activeSceneChanged += HandleActiveSceneChanged;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
        }
    }

    private void HandleActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        // Unityの挙動により oldScene.name が空になるケースへのフォールバック
        // 直前のキャッシュ（CurrentSceneName）が有効ならそれを優先して previous に採用
        string prevByCache = CurrentSceneName;
        string prevByParam = oldScene.name;
        string resolvedPrev = !string.IsNullOrEmpty(prevByCache) ? prevByCache : prevByParam;

        PreviousSceneName = resolvedPrev;
        CurrentSceneName = newScene.name;

        if (showDebugLogs)
        {
            Debug.Log($"[SceneTransitionTracker] ActiveSceneChanged: '{PreviousSceneName}' -> '{CurrentSceneName}' (paramPrev='{oldScene.name}')");
        }
    }
}


