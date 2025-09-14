using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTrigger : MonoBehaviour
{
    [Header("トリガー設定")]
    [SerializeField] private string targetTag = "Player";  // 対象のタグ
    [SerializeField] private bool requireTrigger = true;   // トリガーコライダーが必要か
    
    [Header("キー設定")]
    [SerializeField] private KeyCode triggerKey1 = KeyCode.W;  // トリガーキー1
    [SerializeField] private KeyCode triggerKey2 = KeyCode.S;  // トリガーキー2
    [SerializeField] private bool useKey1 = true;             // キー1を使用するか
    [SerializeField] private bool useKey2 = true;             // キー2を使用するか
    [SerializeField] private bool requireHold = false;        // キーを押し続ける必要があるか
    
    [Header("シーン設定")]
    [SerializeField] private string targetSceneName = "";     // 移動先のシーン名
    [SerializeField] private float transitionDelay = 0.1f;    // シーン切り替えの遅延時間（秒）
    [SerializeField] private bool useFadeTransition = true;   // フェードトランジションを使用するか
    [SerializeField] private float fadeTime = 1.0f;           // フェード時間（秒）
    [SerializeField] private ScreenFader screenFader;         // フェード用のScreenFader（自動検索する場合は空のまま）
    
    [Header("UI設定")]
    [SerializeField] private GameObject promptUI;             // プロンプトUI（オプション）
    [SerializeField] private string promptText = "WキーまたはSキーを押してください"; // プロンプトテキスト
    [SerializeField] private bool showPrompt = true;          // プロンプトを表示するか
    
    [Header("音声設定")]
    [SerializeField] private AudioSource audioSource;         // 音声を再生するAudioSource
    [SerializeField] private AudioClip enterSound;            // エリアに入った時の音
    [SerializeField] private AudioClip exitSound;             // エリアから出た時の音
    [SerializeField] private AudioClip triggerSound;          // キーを押した時の音
    [SerializeField] private float volume = 0.5f;             // 音量
    
    [Header("制御設定")]
    [SerializeField] private bool oneTimeOnly = true;         // 1回のみ機能するか
    [SerializeField] private bool destroyAfterUse = false;    // 使用後にオブジェクトを削除するか
    
    // 内部変数
    private bool isPlayerInTrigger = false;    // プレイヤーがトリガー内にいるか
    private bool hasTriggered = false;         // トリガーが発動したかどうか
    private bool isTransitioning = false;      // シーン切り替え中かどうか
    private Coroutine transitionCoroutine;     // トランジションコルーチンの参照
    
    // イベント
    public System.Action OnPlayerEnter;        // プレイヤーが入った時
    public System.Action OnPlayerExit;         // プレイヤーが出た時
    public System.Action OnKeyPressed;         // キーが押された時
    public System.Action OnSceneTransition;    // シーン切り替え開始時

    void Start()
    {
        // AudioSourceの設定
        SetupAudioSource();
        
        // プロンプトUIの初期設定
        SetupPromptUI();
        
        // ScreenFaderの設定
        SetupScreenFader();
        
        // シーン名の確認
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning($"SceneTrigger ({gameObject.name}): シーン名が設定されていません");
        }
    }

    void Update()
    {
        // プレイヤーがトリガー内にいる場合のみキー入力をチェック
        if (isPlayerInTrigger && !hasTriggered && !isTransitioning)
        {
            CheckKeyInput();
        }
    }

    // キー入力をチェック
    private void CheckKeyInput()
    {
        bool keyPressed = false;
        
        // キー1のチェック
        if (useKey1 && Input.GetKeyDown(triggerKey1))
        {
            keyPressed = true;
        }
        
        // キー2のチェック
        if (useKey2 && Input.GetKeyDown(triggerKey2))
        {
            keyPressed = true;
        }
        
        // キーが押された場合
        if (keyPressed)
        {
            TriggerSceneTransition();
        }
    }

    // シーン切り替えをトリガー
    private void TriggerSceneTransition()
    {
        if (isTransitioning || hasTriggered) return;
        
        // キー押下音を再生
        if (triggerSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(triggerSound, volume);
        }
        
        OnKeyPressed?.Invoke();
        Debug.Log($"シーン切り替えトリガー: {gameObject.name} -> {targetSceneName}");
        
        // 1回のみの設定でフラグを設定
        if (oneTimeOnly)
        {
            hasTriggered = true;
        }
        
        // トランジションを開始
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        transitionCoroutine = StartCoroutine(TransitionToScene());
    }

    // シーンへのトランジション
    private IEnumerator TransitionToScene()
    {
        isTransitioning = true;
        OnSceneTransition?.Invoke();
        
        // 遅延時間を待機
        if (transitionDelay > 0f)
        {
            yield return new WaitForSeconds(transitionDelay);
        }
        
        // フェードトランジション
        if (useFadeTransition)
        {
            yield return StartCoroutine(FadeTransition());
        }
        
        // シーンを読み込み
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            // 遷移コンテキストを設定
            string currentScene = SceneManager.GetActiveScene().name;
            SceneTransitionContext.Set(currentScene, targetSceneName);
            
            // シーンを読み込み
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError($"SceneTrigger ({gameObject.name}): シーン名が設定されていません");
            isTransitioning = false;
        }
    }

    // フェードトランジション
    private IEnumerator FadeTransition()
    {
        if (screenFader == null)
        {
            Debug.LogWarning("ScreenFaderが設定されていません。フェードなしでシーンを切り替えます。");
            yield break;
        }
        
        Debug.Log("フェードアウト開始");
        
        // フェードアウトを実行
        Coroutine fadeCoroutine = screenFader.FadeOut(fadeTime);
        yield return fadeCoroutine;
        
        Debug.Log("フェードアウト完了");
    }

    // トリガーに入った時の処理
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            isPlayerInTrigger = true;
            
            // 入場音を再生
            if (enterSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(enterSound, volume);
            }
            
            // プロンプトUIを表示
            if (showPrompt && promptUI != null)
            {
                promptUI.SetActive(true);
            }
            
            OnPlayerEnter?.Invoke();
            Debug.Log($"プレイヤーがトリガーに入りました: {gameObject.name}");
        }
    }

    // トリガーから出た時の処理
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            isPlayerInTrigger = false;
            
            // 退場音を再生
            if (exitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(exitSound, volume);
            }
            
            // プロンプトUIを非表示
            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }
            
            OnPlayerExit?.Invoke();
            Debug.Log($"プレイヤーがトリガーから出ました: {gameObject.name}");
        }
    }

    // AudioSourceの設定
    private void SetupAudioSource()
    {
        if (audioSource == null && (enterSound != null || exitSound != null || triggerSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    // プロンプトUIの設定
    private void SetupPromptUI()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }
    
    // ScreenFaderの設定
    private void SetupScreenFader()
    {
        // ScreenFaderが設定されていない場合は自動検索
        if (screenFader == null)
        {
            screenFader = FindObjectOfType<ScreenFader>();
            if (screenFader == null)
            {
                Debug.LogWarning($"SceneTrigger ({gameObject.name}): ScreenFaderが見つかりません。フェード機能を使用するには、シーンにScreenFaderを配置するか、Inspectorで設定してください。");
            }
            else
            {
                Debug.Log($"SceneTrigger ({gameObject.name}): ScreenFaderを自動検索しました: {screenFader.gameObject.name}");
            }
        }
    }

    // 外部からアクセス可能なメソッド
    public void SetTargetScene(string sceneName)
    {
        targetSceneName = sceneName;
    }
    
    public void SetTriggerKeys(KeyCode key1, KeyCode key2)
    {
        triggerKey1 = key1;
        triggerKey2 = key2;
    }
    
    public void EnableKey1(bool enable)
    {
        useKey1 = enable;
    }
    
    public void EnableKey2(bool enable)
    {
        useKey2 = enable;
    }
    
    public void ForceTrigger()
    {
        if (isPlayerInTrigger && !hasTriggered && !isTransitioning)
        {
            TriggerSceneTransition();
        }
    }
    
    public void ResetTrigger()
    {
        hasTriggered = false;
        isTransitioning = false;
        isPlayerInTrigger = false;
        
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }
        
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
        
        Debug.Log($"トリガーをリセットしました: {gameObject.name}");
    }

    // 外部からアクセス可能なプロパティ
    public bool IsPlayerInTrigger => isPlayerInTrigger;
    public bool HasTriggered => hasTriggered;
    public bool IsTransitioning => isTransitioning;
    public string TargetSceneName => targetSceneName;

    // デバッグ用：Inspectorでボタンから手動でトリガー
    [ContextMenu("Force Trigger")]
    public void ManualForceTrigger()
    {
        ForceTrigger();
    }
    
    // デバッグ用：Inspectorでボタンから手動でリセット
    [ContextMenu("Reset Trigger")]
    public void ManualResetTrigger()
    {
        ResetTrigger();
    }
    
    // デバッグ用：Inspectorでボタンから設定をテスト
    [ContextMenu("Test Settings")]
    public void TestSettings()
    {
        Debug.Log($"SceneTrigger設定テスト:");
        Debug.Log($"- 対象タグ: {targetTag}");
        Debug.Log($"- キー1: {triggerKey1} (使用: {useKey1})");
        Debug.Log($"- キー2: {triggerKey2} (使用: {useKey2})");
        Debug.Log($"- 移動先シーン: {targetSceneName}");
        Debug.Log($"- 1回のみ: {oneTimeOnly}");
        Debug.Log($"- プレイヤーがトリガー内: {isPlayerInTrigger}");
        Debug.Log($"- フェード使用: {useFadeTransition}");
        Debug.Log($"- ScreenFader: {(screenFader != null ? screenFader.gameObject.name : "未設定")}");
    }
    
    // デバッグ用：Inspectorでボタンからフェードテスト
    [ContextMenu("Test Fade")]
    public void TestFade()
    {
        if (screenFader != null)
        {
            StartCoroutine(TestFadeCoroutine());
        }
        else
        {
            Debug.LogWarning("ScreenFaderが設定されていません");
        }
    }
    
    private IEnumerator TestFadeCoroutine()
    {
        Debug.Log("フェードテスト開始");
        yield return StartCoroutine(FadeTransition());
        Debug.Log("フェードテスト完了");
    }
}
