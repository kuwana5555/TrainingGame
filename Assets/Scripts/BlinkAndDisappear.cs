using System.Collections;
using UnityEngine;

/// <summary>
/// 指定したTagが触れたら点滅して消え、指定した秒数後に復活する2Dゲーム用スクリプト
/// </summary>
public class BlinkAndDisappear : MonoBehaviour
{
    [Header("トリガー設定")]
    [Tooltip("対象のタグ")] public string targetTag = "Player";
    [Tooltip("1回のみ機能するか")] public bool oneTimeOnly = false;
    
    [Header("点滅設定")]
    [Tooltip("点滅の間隔（秒）")] public float blinkInterval = 0.1f;
    [Tooltip("点滅する回数")] public int blinkCount = 10;
    [Tooltip("点滅の色（オプション）")] public Color blinkColor = Color.white;
    [Tooltip("点滅時に色を変更するか")] public bool useBlinkColor = false;
    
    [Header("消失・復活設定")]
    [Tooltip("消失するまでの時間（秒）")] public float disappearTime = 2.0f;
    [Tooltip("復活するまでの時間（秒）")] public float respawnTime = 5.0f;
    [Tooltip("復活時にフェードインするか")] public bool useFadeIn = true;
    [Tooltip("フェードイン時間（秒）")] public float fadeInTime = 1.0f;
    
    [Header("音声設定")]
    [Tooltip("音声を再生するか")] public bool playSound = true;
    [Tooltip("点滅開始時の音声")] public AudioClip blinkStartSound;
    [Tooltip("消失時の音声")] public AudioClip disappearSound;
    [Tooltip("復活時の音声")] public AudioClip respawnSound;
    
    [Header("デバッグ")]
    [Tooltip("デバッグ情報を表示するか")] public bool showDebugInfo = false;
    
    // 内部状態
    private bool hasTriggered = false;
    private bool isBlinking = false;
    private bool isDisappeared = false;
    private bool isRespawning = false;
    private AudioSource audioSource;
    
    // 元の状態を保存
    private Color originalColor;
    private bool originalActive;
    private Renderer[] renderers;
    private Collider2D[] colliders;
    private MonoBehaviour[] scripts;
    private bool[] originalScriptStates;
    
    void Start()
    {
        // 初期状態を保存
        SaveOriginalState();
        
        // AudioSourceを設定
        SetupAudioSource();
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // 対象のタグかチェック
        if (!other.CompareTag(targetTag))
        {
            return;
        }
        
        // 1回のみの設定で、既に発動済みの場合は何もしない
        if (oneTimeOnly && hasTriggered)
        {
            if (showDebugInfo)
            {
                Debug.Log("BlinkAndDisappear: 既に発動済みのため処理をスキップします");
            }
            return;
        }
        
        // 既に処理中の場合は何もしない
        if (isBlinking || isDisappeared || isRespawning)
        {
            if (showDebugInfo)
            {
                Debug.Log("BlinkAndDisappear: 既に処理中のためスキップします");
            }
            return;
        }
        
        // 点滅処理を開始
        StartCoroutine(BlinkAndDisappearCoroutine());
    }
    
    /// <summary>
    /// 点滅→消失→復活のメイン処理
    /// </summary>
    IEnumerator BlinkAndDisappearCoroutine()
    {
        hasTriggered = true;
        isBlinking = true;
        
        if (showDebugInfo)
        {
            Debug.Log("BlinkAndDisappear: 点滅処理開始");
        }
        
        // 点滅開始音声
        if (playSound && audioSource != null && blinkStartSound != null)
        {
            audioSource.PlayOneShot(blinkStartSound);
        }
        
        // 点滅処理
        yield return StartCoroutine(BlinkCoroutine());
        
        // 消失処理
        yield return StartCoroutine(DisappearCoroutine());
        
        // 復活待機
        yield return StartCoroutine(RespawnWaitCoroutine());
        
        // 復活処理
        yield return StartCoroutine(RespawnCoroutine());
        
        if (showDebugInfo)
        {
            Debug.Log("BlinkAndDisappear: 全処理完了");
        }
    }
    
    /// <summary>
    /// 点滅処理
    /// </summary>
    IEnumerator BlinkCoroutine()
    {
        if (showDebugInfo)
        {
            Debug.Log($"BlinkAndDisappear: 点滅開始 - {blinkCount}回点滅");
        }
        
        for (int i = 0; i < blinkCount; i++)
        {
            // 表示/非表示を切り替え
            SetVisibility(!GetVisibility());
            
            // 色を変更（オプション）
            if (useBlinkColor)
            {
                SetColor(GetVisibility() ? blinkColor : originalColor);
            }
            
            yield return new WaitForSeconds(blinkInterval);
        }
        
        // 最後は表示状態にする
        SetVisibility(true);
        if (useBlinkColor)
        {
            SetColor(originalColor);
        }
        
        isBlinking = false;
        
        if (showDebugInfo)
        {
            Debug.Log("BlinkAndDisappear: 点滅完了");
        }
    }
    
    /// <summary>
    /// 消失処理
    /// </summary>
    IEnumerator DisappearCoroutine()
    {
        isDisappeared = true;
        
        if (showDebugInfo)
        {
            Debug.Log($"BlinkAndDisappear: 消失開始 - {disappearTime}秒後に消失");
        }
        
        // 消失音声
        if (playSound && audioSource != null && disappearSound != null)
        {
            audioSource.PlayOneShot(disappearSound);
        }
        
        // 消失までの待機
        yield return new WaitForSeconds(disappearTime);
        
        // オブジェクトを非表示にしてコライダーとスクリプトを無効化
        SetVisibility(false);
        SetCollidersEnabled(false);
        SetScriptsEnabled(false);
        
        if (showDebugInfo)
        {
            Debug.Log("BlinkAndDisappear: 消失完了");
        }
    }
    
    /// <summary>
    /// 復活待機処理
    /// </summary>
    IEnumerator RespawnWaitCoroutine()
    {
        if (showDebugInfo)
        {
            Debug.Log($"BlinkAndDisappear: 復活待機 - {respawnTime}秒後に復活");
        }
        
        yield return new WaitForSeconds(respawnTime);
    }
    
    /// <summary>
    /// 復活処理
    /// </summary>
    IEnumerator RespawnCoroutine()
    {
        isRespawning = true;
        
        if (showDebugInfo)
        {
            Debug.Log("BlinkAndDisappear: 復活開始");
        }
        
        // 復活音声
        if (playSound && audioSource != null && respawnSound != null)
        {
            audioSource.PlayOneShot(respawnSound);
        }
        
        // コライダーとスクリプトを再有効化
        SetCollidersEnabled(true);
        SetScriptsEnabled(true);
        
        // フェードイン処理
        if (useFadeIn)
        {
            yield return StartCoroutine(FadeInCoroutine());
        }
        else
        {
            // 即座に表示
            SetVisibility(true);
        }
        
        isDisappeared = false;
        isRespawning = false;
        
        if (showDebugInfo)
        {
            Debug.Log("BlinkAndDisappear: 復活完了");
        }
    }
    
    /// <summary>
    /// フェードイン処理
    /// </summary>
    IEnumerator FadeInCoroutine()
    {
        if (showDebugInfo)
        {
            Debug.Log($"BlinkAndDisappear: フェードイン開始 - {fadeInTime}秒");
        }
        
        // 透明状態で表示開始
        SetVisibility(true);
        SetAlpha(0f);
        
        float elapsedTime = 0f;
        while (elapsedTime < fadeInTime)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInTime);
            SetAlpha(alpha);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 完全に不透明にする
        SetAlpha(1f);
        
        if (showDebugInfo)
        {
            Debug.Log("BlinkAndDisappear: フェードイン完了");
        }
    }
    
    /// <summary>
    /// 初期状態を保存
    /// </summary>
    void SaveOriginalState()
    {
        // レンダラーを取得
        renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            originalColor = renderers[0].material.color;
        }
        
        // コライダーを取得
        colliders = GetComponentsInChildren<Collider2D>();
        
        // スクリプトを取得（このスクリプト以外）
        MonoBehaviour[] allScripts = GetComponents<MonoBehaviour>();
        scripts = new MonoBehaviour[allScripts.Length - 1];
        originalScriptStates = new bool[allScripts.Length - 1];
        
        int scriptIndex = 0;
        for (int i = 0; i < allScripts.Length; i++)
        {
            if (allScripts[i] != this)
            {
                scripts[scriptIndex] = allScripts[i];
                originalScriptStates[scriptIndex] = allScripts[i].enabled;
                scriptIndex++;
            }
        }
        
        originalActive = gameObject.activeInHierarchy;
        
        if (showDebugInfo)
        {
            Debug.Log($"BlinkAndDisappear: 初期状態を保存 - レンダラー: {renderers.Length}個, コライダー: {colliders.Length}個, スクリプト: {scripts.Length}個");
        }
    }
    
    /// <summary>
    /// 表示状態を設定
    /// </summary>
    void SetVisibility(bool visible)
    {
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.enabled = visible;
            }
        }
    }
    
    /// <summary>
    /// 表示状態を取得
    /// </summary>
    bool GetVisibility()
    {
        if (renderers.Length > 0 && renderers[0] != null)
        {
            return renderers[0].enabled;
        }
        return false;
    }
    
    /// <summary>
    /// 色を設定
    /// </summary>
    void SetColor(Color color)
    {
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.material.color = color;
            }
        }
    }
    
    /// <summary>
    /// アルファ値を設定
    /// </summary>
    void SetAlpha(float alpha)
    {
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                Color color = renderer.material.color;
                color.a = alpha;
                renderer.material.color = color;
            }
        }
    }
    
    /// <summary>
    /// コライダーの有効/無効を設定
    /// </summary>
    void SetCollidersEnabled(bool enabled)
    {
        foreach (Collider2D collider in colliders)
        {
            if (collider != null)
            {
                collider.enabled = enabled;
            }
        }
    }
    
    /// <summary>
    /// スクリプトの有効/無効を設定
    /// </summary>
    void SetScriptsEnabled(bool enabled)
    {
        for (int i = 0; i < scripts.Length; i++)
        {
            if (scripts[i] != null)
            {
                scripts[i].enabled = enabled;
            }
        }
    }
    
    /// <summary>
    /// AudioSourceの設定
    /// </summary>
    void SetupAudioSource()
    {
        if (playSound)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
        }
    }
    
    /// <summary>
    /// 手動で点滅処理を開始（デバッグ用）
    /// </summary>
    [ContextMenu("Start Blink Process")]
    public void StartBlinkProcess()
    {
        if (!isBlinking && !isDisappeared && !isRespawning)
        {
            StartCoroutine(BlinkAndDisappearCoroutine());
        }
        else
        {
            Debug.LogWarning("BlinkAndDisappear: 既に処理中のため開始できません");
        }
    }
    
    /// <summary>
    /// 強制的に復活（デバッグ用）
    /// </summary>
    [ContextMenu("Force Respawn")]
    public void ForceRespawn()
    {
        StopAllCoroutines();
        
        // 状態をリセット
        isBlinking = false;
        isDisappeared = false;
        isRespawning = false;
        
        // 完全に復活
        SetVisibility(true);
        SetCollidersEnabled(true);
        SetScriptsEnabled(true);
        SetAlpha(1f);
        SetColor(originalColor);
        
        if (showDebugInfo)
        {
            Debug.Log("BlinkAndDisappear: 強制復活しました");
        }
    }
    
    /// <summary>
    /// トリガーをリセット（デバッグ用）
    /// </summary>
    [ContextMenu("Reset Trigger")]
    public void ResetTrigger()
    {
        hasTriggered = false;
        isBlinking = false;
        isDisappeared = false;
        isRespawning = false;
        
        // 初期状態に復元
        SetVisibility(true);
        SetCollidersEnabled(true);
        SetScriptsEnabled(true);
        SetAlpha(1f);
        SetColor(originalColor);
        
        if (showDebugInfo)
        {
            Debug.Log("BlinkAndDisappear: トリガーをリセットしました");
        }
    }
    
    /// <summary>
    /// 現在の状態を取得
    /// </summary>
    public string GetCurrentState()
    {
        if (isBlinking) return "点滅中";
        if (isDisappeared) return "消失中";
        if (isRespawning) return "復活中";
        return "待機中";
    }
}

