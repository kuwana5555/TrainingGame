using UnityEngine;

/// <summary>
/// 移動床の上にいるキャラクターの処理
/// 方法2: 床の移動量をキャラクターに加算する方式
/// </summary>
public class CharacterOnMovingPlatform : MonoBehaviour
{
    [Header("移動床設定")]
    [SerializeField] private string platformTag = "Ground"; // 移動床のタグ（Groundタグを使用）
    [SerializeField] private bool showDebugInfo = false; // デバッグ情報を表示するか
    [SerializeField] private bool checkForMovingPlatform = true; // MovingPlatformスクリプトの存在をチェックするか
    
    private Transform platformTransform; // 現在乗っているプラットフォーム
    private Vector3 lastPlatformPosition; // 前フレームのプラットフォーム位置
    private bool onPlatform = false; // プラットフォームの上にいるかどうか
    
    // イベント
    public System.Action OnPlatformEnter; // プラットフォームに乗った時
    public System.Action OnPlatformExit;  // プラットフォームから降りた時

    void Start()
    {
        // 初期化
        onPlatform = false;
        platformTransform = null;
        lastPlatformPosition = Vector3.zero;
    }

    void Update()
    {
        // プラットフォームの上にいる場合の処理
        if (onPlatform && platformTransform != null)
        {
            // プラットフォームの移動量を計算
            Vector3 platformMovement = platformTransform.position - lastPlatformPosition;
            
            // プラットフォームが移動している場合のみ処理
            if (platformMovement.magnitude > 0.001f)
            {
                // 床の移動量をキャラクターの位置に直接加算
                transform.position += platformMovement;
                
                if (showDebugInfo)
                {
                    Debug.Log($"CharacterOnMovingPlatform: {gameObject.name} を移動量 {platformMovement} で移動");
                }
            }
            
            // 現在のプラットフォーム位置を記録
            lastPlatformPosition = platformTransform.position;
        }
    }

    // 2Dコリジョンに入った時の処理
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(platformTag))
        {
            // MovingPlatformスクリプトの存在をチェック
            bool isMovingPlatform = true;
            if (checkForMovingPlatform)
            {
                MovingPlatform movingPlatform = collision.gameObject.GetComponent<MovingPlatform>();
                isMovingPlatform = movingPlatform != null;
            }
            
            if (isMovingPlatform)
            {
                // プラットフォームの上にいる状態に設定
                platformTransform = collision.transform;
                lastPlatformPosition = platformTransform.position;
                onPlatform = true;
                
                OnPlatformEnter?.Invoke();
                
                if (showDebugInfo)
                {
                    Debug.Log($"CharacterOnMovingPlatform: {gameObject.name} が移動床 {collision.gameObject.name} に乗りました");
                }
            }
        }
    }

    // 2Dコリジョンから出た時の処理
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(platformTag))
        {
            // MovingPlatformスクリプトの存在をチェック
            bool isMovingPlatform = true;
            if (checkForMovingPlatform)
            {
                MovingPlatform movingPlatform = collision.gameObject.GetComponent<MovingPlatform>();
                isMovingPlatform = movingPlatform != null;
            }
            
            if (isMovingPlatform)
            {
                // プラットフォームから降りた状態に設定
                onPlatform = false;
                platformTransform = null;
                lastPlatformPosition = Vector3.zero;
                
                OnPlatformExit?.Invoke();
                
                if (showDebugInfo)
                {
                    Debug.Log($"CharacterOnMovingPlatform: {gameObject.name} が移動床 {collision.gameObject.name} から降りました");
                }
            }
        }
    }

    // 3Dコリジョンに入った時の処理
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(platformTag))
        {
            // MovingPlatformスクリプトの存在をチェック
            bool isMovingPlatform = true;
            if (checkForMovingPlatform)
            {
                MovingPlatform movingPlatform = collision.gameObject.GetComponent<MovingPlatform>();
                isMovingPlatform = movingPlatform != null;
            }
            
            if (isMovingPlatform)
            {
                // プラットフォームの上にいる状態に設定
                platformTransform = collision.transform;
                lastPlatformPosition = platformTransform.position;
                onPlatform = true;
                
                OnPlatformEnter?.Invoke();
                
                if (showDebugInfo)
                {
                    Debug.Log($"CharacterOnMovingPlatform: {gameObject.name} が移動床 {collision.gameObject.name} に乗りました");
                }
            }
        }
    }

    // 3Dコリジョンから出た時の処理
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag(platformTag))
        {
            // MovingPlatformスクリプトの存在をチェック
            bool isMovingPlatform = true;
            if (checkForMovingPlatform)
            {
                MovingPlatform movingPlatform = collision.gameObject.GetComponent<MovingPlatform>();
                isMovingPlatform = movingPlatform != null;
            }
            
            if (isMovingPlatform)
            {
                // プラットフォームから降りた状態に設定
                onPlatform = false;
                platformTransform = null;
                lastPlatformPosition = Vector3.zero;
                
                OnPlatformExit?.Invoke();
                
                if (showDebugInfo)
                {
                    Debug.Log($"CharacterOnMovingPlatform: {gameObject.name} が移動床 {collision.gameObject.name} から降りました");
                }
            }
        }
    }

    // 現在プラットフォームの上にいるかどうかを取得
    public bool IsOnPlatform()
    {
        return onPlatform;
    }

    // 現在乗っているプラットフォームを取得
    public Transform GetCurrentPlatform()
    {
        return platformTransform;
    }

    // 手動でプラットフォームとの連携を開始（デバッグ用）
    [ContextMenu("Start Platform Connection")]
    public void StartPlatformConnection(Transform platform)
    {
        if (platform != null)
        {
            platformTransform = platform;
            lastPlatformPosition = platform.position;
            onPlatform = true;
            
            if (showDebugInfo)
            {
                Debug.Log($"CharacterOnMovingPlatform: 手動でプラットフォーム連携を開始しました: {platform.name}");
            }
        }
    }

    // 手動でプラットフォームとの連携を停止（デバッグ用）
    [ContextMenu("Stop Platform Connection")]
    public void StopPlatformConnection()
    {
        onPlatform = false;
        platformTransform = null;
        lastPlatformPosition = Vector3.zero;
        
        if (showDebugInfo)
        {
            Debug.Log("CharacterOnMovingPlatform: 手動でプラットフォーム連携を停止しました");
        }
    }
}
