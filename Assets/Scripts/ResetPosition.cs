using UnityEngine;

/// <summary>
/// オブジェクトの初期位置を記録し、Mキーを押したら元の位置に戻すスクリプト
/// </summary>
public class ResetPosition : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("リセットに使用するキー")]
    public KeyCode resetKey = KeyCode.M;
    
    [Tooltip("リセット時にデバッグログを出力するか")]
    public bool showDebugLog = true;
    
    // 初期位置を保存
    private Vector3 initialPosition;
    
    void Start()
    {
        // 初期位置を記録
        initialPosition = transform.position;
        
        if (showDebugLog)
        {
            Debug.Log($"[ResetPosition] 初期位置を記録しました: {initialPosition}", this);
        }
    }
    
    void Update()
    {
        // Mキー（または設定されたキー）が押されたら位置をリセット
        if (Input.GetKeyDown(resetKey))
        {
            ResetToInitialPosition();
        }
    }
    
    /// <summary>
    /// 初期位置に戻す
    /// </summary>
    public void ResetToInitialPosition()
    {
        transform.position = initialPosition;
        
        if (showDebugLog)
        {
            Debug.Log($"[ResetPosition] 位置をリセットしました: {initialPosition}", this);
        }
    }
    
    /// <summary>
    /// 初期位置を再記録する（外部から呼び出し可能）
    /// </summary>
    public void UpdateInitialPosition()
    {
        initialPosition = transform.position;
        
        if (showDebugLog)
        {
            Debug.Log($"[ResetPosition] 初期位置を更新しました: {initialPosition}", this);
        }
    }
}

