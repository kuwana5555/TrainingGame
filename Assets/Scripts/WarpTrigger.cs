using System.Collections;
using UnityEngine;

/// <summary>
/// 指定した2Dコライダーに触れたら暗転→ワープ→暗転解除を行うスクリプト
/// </summary>
public class WarpTrigger : MonoBehaviour
{
    [Header("ワープ設定")]
    [Tooltip("ワープ先の位置")] public Vector3 warpPosition = Vector3.zero;
    [Tooltip("ワープ先の位置をTransformで指定する場合")] public Transform warpTarget;
    [Tooltip("ワープ先の位置をTransformで指定するか")] public bool useWarpTarget = false;
    
    [Header("リスポーン地点更新設定")]
    [Tooltip("ワープ時にリスポーン地点を更新するか")] public bool updateRespawnPoint = false;
    [Tooltip("リスポーン地点の位置")] public Vector3 respawnPosition = Vector3.zero;
    [Tooltip("リスポーン地点をTransformで指定する場合")] public Transform respawnTarget;
    [Tooltip("リスポーン地点をTransformで指定するか")] public bool useRespawnTarget = false;
    
    [Header("暗転設定")]
    [Tooltip("暗転に使用するScreenFader")] public ScreenFader screenFader;
    [Tooltip("暗転時間（秒）")] public float fadeTime = 1.0f;
    [Tooltip("暗転維持時間（秒）")] public float fadeHoldTime = 0.5f;
    [Tooltip("暗転解除時間（秒）")] public float fadeInTime = 1.0f;
    
    [Header("トリガー設定")]
    [Tooltip("対象のタグ")] public string targetTag = "Player";
    [Tooltip("1回のみ機能するか")] public bool oneTimeOnly = true;
    [Tooltip("音声を再生するか")] public bool playSound = true;
    [Tooltip("ワープ時の音声")] public AudioClip warpSound;
    
    [Header("デバッグ")]
    [Tooltip("デバッグ情報を表示するか")] public bool showDebugInfo = false;
    
    private bool hasTriggered = false;
    private bool isWarping = false;
    private AudioSource audioSource;
    
    // ワープ対象の制御用変数
    private Rigidbody2D targetRigidbody;
    private Vector2 originalVelocity;
    private float originalGravityScale;
    private bool originalKinematic;
    private MonoBehaviour[] targetScripts;
    private bool[] originalScriptStates;
    private bool isMovementDisabled = false;
    
    void Start()
    {
        // ScreenFaderを自動検索
        if (screenFader == null)
        {
            screenFader = FindObjectOfType<ScreenFader>();
            if (screenFader == null)
            {
                Debug.LogWarning("WarpTrigger: ScreenFaderが見つかりません。暗転効果は無効になります。");
            }
        }
        
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
                Debug.Log("WarpTrigger: 既に発動済みのため処理をスキップします");
            }
            return;
        }
        
        // ワープ処理中は何もしない
        if (isWarping)
        {
            if (showDebugInfo)
            {
                Debug.Log("WarpTrigger: ワープ処理中のため処理をスキップします");
            }
            return;
        }
        
        // 既に移動制御中の場合は何もしない
        if (isMovementDisabled)
        {
            if (showDebugInfo)
            {
                Debug.Log("WarpTrigger: 既に移動制御中のため処理をスキップします");
            }
            return;
        }
        
        // ワープ処理を開始
        StartCoroutine(WarpCoroutine(other.gameObject));
    }
    
    /// <summary>
    /// ワープ処理のコルーチン
    /// </summary>
    IEnumerator WarpCoroutine(GameObject targetObject)
    {
        isWarping = true;
        hasTriggered = true;
        
        if (showDebugInfo)
        {
            Debug.Log($"WarpTrigger: ワープ処理開始 - {targetObject.name}");
        }
        
        // ワープ対象の移動を停止
        try
        {
            DisableTargetMovement(targetObject);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"WarpTrigger: 移動停止中にエラーが発生しました - {e.Message}");
        }
        
        // 音声再生
        try
        {
            if (playSound && audioSource != null && warpSound != null)
            {
                audioSource.PlayOneShot(warpSound);
                if (showDebugInfo)
                {
                    Debug.Log("WarpTrigger: ワープ音声を再生しました");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"WarpTrigger: 音声再生中にエラーが発生しました - {e.Message}");
        }
        
        // 暗転開始
        if (screenFader != null)
        {
            if (showDebugInfo)
            {
                Debug.Log("WarpTrigger: 暗転開始");
            }
            yield return screenFader.FadeOut(fadeTime);
        }
        else
        {
            // ScreenFaderがない場合は単純に待機
            yield return new WaitForSeconds(fadeTime);
        }
        
        // 暗転維持時間
        if (fadeHoldTime > 0f)
        {
            yield return new WaitForSeconds(fadeHoldTime);
        }
        
        // ワープ先の位置を決定
        Vector3 finalWarpPosition = useWarpTarget && warpTarget != null ? 
            warpTarget.position : warpPosition;
        
        if (showDebugInfo)
        {
            Debug.Log($"WarpTrigger: ワープ実行 - {targetObject.name} を {finalWarpPosition} に移動");
        }
        
        // オブジェクトをワープ先に移動
        try
        {
            targetObject.transform.position = finalWarpPosition;
            
            // リスポーン地点を更新
            if (updateRespawnPoint)
            {
                UpdateRespawnPoint(targetObject);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"WarpTrigger: ワープ移動中にエラーが発生しました - {e.Message}");
        }
        
        // 暗転解除
        if (screenFader != null)
        {
            if (showDebugInfo)
            {
                Debug.Log("WarpTrigger: 暗転解除開始");
            }
            yield return screenFader.FadeIn(fadeInTime);
        }
        else
        {
            // ScreenFaderがない場合は単純に待機
            yield return new WaitForSeconds(fadeInTime);
        }
        
        // ワープ対象の移動を再有効化
        try
        {
            EnableTargetMovement(targetObject);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"WarpTrigger: 移動再有効化中にエラーが発生しました - {e.Message}");
        }
        
        if (showDebugInfo)
        {
            Debug.Log("WarpTrigger: ワープ処理完了");
        }
        
        // 状態をリセット
        isWarping = false;
        isMovementDisabled = false;
    }
    
    /// <summary>
    /// ワープ対象の移動を停止
    /// </summary>
    void DisableTargetMovement(GameObject targetObject)
    {
        if (isMovementDisabled)
        {
            if (showDebugInfo)
            {
                Debug.Log("WarpTrigger: 既に移動制御中のためスキップします");
            }
            return;
        }
        
        isMovementDisabled = true;
        
        // Rigidbody2Dの制御
        targetRigidbody = targetObject.GetComponent<Rigidbody2D>();
        if (targetRigidbody != null)
        {
            originalVelocity = targetRigidbody.velocity;
            originalGravityScale = targetRigidbody.gravityScale;
            originalKinematic = targetRigidbody.isKinematic;
            
            // 速度を0にして、キネマティックに設定
            targetRigidbody.velocity = Vector2.zero;
            targetRigidbody.gravityScale = 0f;
            targetRigidbody.isKinematic = true;
            
            if (showDebugInfo)
            {
                Debug.Log($"WarpTrigger: Rigidbody2Dを停止しました (元の重力: {originalGravityScale}, 元のキネマティック: {originalKinematic})");
            }
        }
        
        // プレイヤーコントローラーなどのスクリプトを無効化
        MonoBehaviour[] allScripts = targetObject.GetComponents<MonoBehaviour>();
        targetScripts = new MonoBehaviour[allScripts.Length];
        originalScriptStates = new bool[allScripts.Length];
        
        int validScriptCount = 0;
        for (int i = 0; i < allScripts.Length; i++)
        {
            MonoBehaviour script = allScripts[i];
            if (script != null && script != this && script.enabled)
            {
                // 特定のスクリプトは除外（例：AudioSource、ParticleSystem等）
                if (ShouldDisableScript(script))
                {
                    targetScripts[validScriptCount] = script;
                    originalScriptStates[validScriptCount] = script.enabled;
                    script.enabled = false;
                    validScriptCount++;
                }
            }
        }
        
        // 配列を有効な要素のみにリサイズ
        System.Array.Resize(ref targetScripts, validScriptCount);
        System.Array.Resize(ref originalScriptStates, validScriptCount);
        
        if (showDebugInfo)
        {
            Debug.Log($"WarpTrigger: {validScriptCount}個のスクリプトを無効化しました");
        }
    }
    
    /// <summary>
    /// ワープ対象の移動を再有効化
    /// </summary>
    void EnableTargetMovement(GameObject targetObject)
    {
        if (!isMovementDisabled)
        {
            if (showDebugInfo)
            {
                Debug.Log("WarpTrigger: 移動制御されていないためスキップします");
            }
            return;
        }
        
        // Rigidbody2Dの復元
        if (targetRigidbody != null)
        {
            targetRigidbody.isKinematic = originalKinematic;
            targetRigidbody.gravityScale = originalGravityScale;
            // 速度は復元しない（ワープ先で静止状態から開始）
            
            if (showDebugInfo)
            {
                Debug.Log($"WarpTrigger: Rigidbody2Dを復元しました (重力: {originalGravityScale}, キネマティック: {originalKinematic})");
            }
        }
        
        // スクリプトの再有効化
        if (targetScripts != null)
        {
            for (int i = 0; i < targetScripts.Length; i++)
            {
                if (targetScripts[i] != null)
                {
                    targetScripts[i].enabled = originalScriptStates[i];
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"WarpTrigger: {targetScripts.Length}個のスクリプトを再有効化しました");
            }
        }
        
        isMovementDisabled = false;
    }
    
    /// <summary>
    /// スクリプトを無効化すべきかどうかを判定
    /// </summary>
    bool ShouldDisableScript(MonoBehaviour script)
    {
        // 無効化しないスクリプトのタイプ
        if (script is AudioSource || 
            script is ParticleSystem ||
            script is TrailRenderer ||
            script is LineRenderer)
        {
            return false;
        }
        
        // その他のスクリプトは無効化
        return true;
    }
    
    /// <summary>
    /// AudioSourceの設定
    /// </summary>
    void SetupAudioSource()
    {
        if (playSound && warpSound != null)
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
    /// ワープ先の位置を設定（外部から呼び出し可能）
    /// </summary>
    public void SetWarpPosition(Vector3 position)
    {
        warpPosition = position;
        if (showDebugInfo)
        {
            Debug.Log($"WarpTrigger: ワープ先位置を設定しました - {position}");
        }
    }
    
    /// <summary>
    /// ワープ先のTransformを設定（外部から呼び出し可能）
    /// </summary>
    public void SetWarpTarget(Transform target)
    {
        warpTarget = target;
        useWarpTarget = target != null;
        if (showDebugInfo)
        {
            Debug.Log($"WarpTrigger: ワープ先Transformを設定しました - {(target != null ? target.name : "null")}");
        }
    }
    
    /// <summary>
    /// リスポーン地点を更新
    /// </summary>
    void UpdateRespawnPoint(GameObject targetObject)
    {
        // リスポーン地点の位置を決定
        Vector3 finalRespawnPosition = useRespawnTarget && respawnTarget != null ? 
            respawnTarget.position : respawnPosition;
        
        // PlayerControllerを取得
        PlayerController playerController = targetObject.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // リスポーン地点を設定
            playerController.SetRespawnPosition(finalRespawnPosition);
            
            if (showDebugInfo)
            {
                Debug.Log($"WarpTrigger: リスポーン地点を更新しました - {finalRespawnPosition}");
            }
        }
        else
        {
            // PlayerControllerがない場合は静的変数を直接更新
            PlayerController.CheckPoint = finalRespawnPosition;
            
            if (showDebugInfo)
            {
                Debug.Log($"WarpTrigger: 静的CheckPointを更新しました - {finalRespawnPosition}");
            }
        }
    }
    
    /// <summary>
    /// リスポーン地点を設定（外部から呼び出し可能）
    /// </summary>
    public void SetRespawnPosition(Vector3 position)
    {
        respawnPosition = position;
        if (showDebugInfo)
        {
            Debug.Log($"WarpTrigger: リスポーン地点を設定しました - {position}");
        }
    }
    
    /// <summary>
    /// リスポーン地点のTransformを設定（外部から呼び出し可能）
    /// </summary>
    public void SetRespawnTarget(Transform target)
    {
        respawnTarget = target;
        useRespawnTarget = target != null;
        if (showDebugInfo)
        {
            Debug.Log($"WarpTrigger: リスポーン地点Transformを設定しました - {(target != null ? target.name : "null")}");
        }
    }
    
    /// <summary>
    /// トリガーをリセット（外部から呼び出し可能）
    /// </summary>
    [ContextMenu("Reset Trigger")]
    public void ResetTrigger()
    {
        hasTriggered = false;
        isWarping = false;
        isMovementDisabled = false;
        
        // 強制的に復元を試行
        if (targetRigidbody != null)
        {
            try
            {
                targetRigidbody.isKinematic = originalKinematic;
                targetRigidbody.gravityScale = originalGravityScale;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"WarpTrigger: 強制復元中にエラーが発生しました - {e.Message}");
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log("WarpTrigger: トリガーをリセットしました");
        }
    }
    
    /// <summary>
    /// 手動でワープを実行（デバッグ用）
    /// </summary>
    [ContextMenu("Manual Warp")]
    public void ManualWarp()
    {
        GameObject player = GameObject.FindGameObjectWithTag(targetTag);
        if (player != null)
        {
            StartCoroutine(WarpCoroutine(player));
        }
        else
        {
            Debug.LogWarning("WarpTrigger: 対象のオブジェクトが見つかりません");
        }
    }
    
    /// <summary>
    /// ワープ処理を強制停止（デバッグ用）
    /// </summary>
    [ContextMenu("Force Stop Warp")]
    public void ForceStopWarp()
    {
        StopAllCoroutines();
        isWarping = false;
        isMovementDisabled = false;
        
        // 強制的に復元を試行
        if (targetRigidbody != null)
        {
            try
            {
                targetRigidbody.isKinematic = originalKinematic;
                targetRigidbody.gravityScale = originalGravityScale;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"WarpTrigger: 強制停止時の復元中にエラーが発生しました - {e.Message}");
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log("WarpTrigger: ワープ処理を強制停止しました");
        }
    }
    
    /// <summary>
    /// 現在のワープ状態を取得
    /// </summary>
    public bool IsWarping => isWarping;
    
    /// <summary>
    /// トリガーが発動済みかどうかを取得
    /// </summary>
    public bool HasTriggered => hasTriggered;
    
    /// <summary>
    /// 移動制御中かどうかを取得
    /// </summary>
    public bool IsMovementDisabled => isMovementDisabled;
    
    void OnDrawGizmosSelected()
    {
        // ワープ先の位置を表示
        Vector3 finalWarpPosition = useWarpTarget && warpTarget != null ? 
            warpTarget.position : warpPosition;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(finalWarpPosition, 0.5f);
        
        // 現在位置からワープ先への線を表示
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, finalWarpPosition);
        
        // ワープ先のラベル表示
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(finalWarpPosition + Vector3.up, "Warp Target");
        #endif
    }
}
