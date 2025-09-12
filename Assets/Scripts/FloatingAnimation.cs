using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingAnimation : MonoBehaviour
{
    [Header("浮遊アニメーション設定")]
    [SerializeField] private float amplitude = 1.0f;        // 上下の振幅
    [SerializeField] private float frequency = 1.0f;        // 浮遊の周波数（1秒間の周期数）
    [SerializeField] private float phase = 0.0f;            // 位相オフセット（ランダム化用）
    [SerializeField] private bool randomizePhase = true;    // 位相をランダム化するか
    [SerializeField] private bool useSineWave = true;       // sin波を使用するか（falseで三角波）
    
    [Header("動作設定")]
    [SerializeField] private bool playOnStart = true;       // 開始時に自動再生するか
    [SerializeField] private bool isPlaying = true;         // 現在再生中かどうか
    [SerializeField] private bool pauseOnStart = false;     // 開始時に一時停止するか
    
    [Header("制限設定")]
    [SerializeField] private bool useLimits = false;        // 上下限界を使用するか
    [SerializeField] private float upperLimit = 5.0f;       // 上限界
    [SerializeField] private float lowerLimit = -5.0f;      // 下限界
    
    private Vector3 startPosition;                          // 開始位置
    private float timeOffset;                               // 時間オフセット
    
    // Start is called before the first frame update
    void Start()
    {
        // 開始位置を保存
        startPosition = transform.position;
        
        // 位相をランダム化
        if (randomizePhase)
        {
            phase = Random.Range(0f, 2f * Mathf.PI);
        }
        
        // 時間オフセットを設定
        timeOffset = phase / frequency;
        
        // 一時停止設定
        if (pauseOnStart)
        {
            isPlaying = false;
        }
        
        // 自動再生設定
        if (!playOnStart)
        {
            isPlaying = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlaying)
        {
            UpdateFloatingAnimation();
        }
    }
    
    // 浮遊アニメーションの更新
    private void UpdateFloatingAnimation()
    {
        float time = Time.time + timeOffset;
        float yOffset = 0f;
        
        if (useSineWave)
        {
            // sin波を使用した滑らかな浮遊
            yOffset = Mathf.Sin(time * frequency * 2f * Mathf.PI) * amplitude;
        }
        else
        {
            // 三角波を使用した直線的な浮遊
            float triangle = Mathf.PingPong(time * frequency, 1f);
            yOffset = (triangle - 0.5f) * 2f * amplitude;
        }
        
        // 新しい位置を計算
        Vector3 newPosition = startPosition + new Vector3(0, yOffset, 0);
        
        // 制限を適用
        if (useLimits)
        {
            newPosition.y = Mathf.Clamp(newPosition.y, lowerLimit, upperLimit);
        }
        
        // 位置を更新
        transform.position = newPosition;
    }
    
    // アニメーションを開始
    public void StartAnimation()
    {
        isPlaying = true;
        Debug.Log($"浮遊アニメーション開始: {gameObject.name}");
    }
    
    // アニメーションを停止
    public void StopAnimation()
    {
        isPlaying = false;
        Debug.Log($"浮遊アニメーション停止: {gameObject.name}");
    }
    
    // アニメーションを一時停止
    public void PauseAnimation()
    {
        isPlaying = false;
        Debug.Log($"浮遊アニメーション一時停止: {gameObject.name}");
    }
    
    // アニメーションを再開
    public void ResumeAnimation()
    {
        isPlaying = true;
        Debug.Log($"浮遊アニメーション再開: {gameObject.name}");
    }
    
    // アニメーションをリセット（開始位置に戻す）
    public void ResetAnimation()
    {
        transform.position = startPosition;
        timeOffset = phase / frequency;
        Debug.Log($"浮遊アニメーションリセット: {gameObject.name}");
    }
    
    // 振幅を設定
    public void SetAmplitude(float newAmplitude)
    {
        amplitude = newAmplitude;
        Debug.Log($"振幅を設定: {amplitude}");
    }
    
    // 周波数を設定
    public void SetFrequency(float newFrequency)
    {
        frequency = newFrequency;
        timeOffset = phase / frequency;
        Debug.Log($"周波数を設定: {frequency}");
    }
    
    // 開始位置を更新（現在位置を新しい開始位置にする）
    public void UpdateStartPosition()
    {
        startPosition = transform.position;
        Debug.Log($"開始位置を更新: {startPosition}");
    }
    
    // デバッグ用：Inspectorでボタンから手動で開始
    [ContextMenu("Start Animation")]
    public void ManualStart()
    {
        StartAnimation();
    }
    
    // デバッグ用：Inspectorでボタンから手動で停止
    [ContextMenu("Stop Animation")]
    public void ManualStop()
    {
        StopAnimation();
    }
    
    // デバッグ用：Inspectorでボタンから手動でリセット
    [ContextMenu("Reset Animation")]
    public void ManualReset()
    {
        ResetAnimation();
    }
    
    // デバッグ用：Inspectorでボタンから開始位置更新
    [ContextMenu("Update Start Position")]
    public void ManualUpdateStartPosition()
    {
        UpdateStartPosition();
    }
    
    // 現在の状態を取得（外部から参照用）
    public bool IsPlaying()
    {
        return isPlaying;
    }
    
    // 現在の振幅を取得（外部から参照用）
    public float GetAmplitude()
    {
        return amplitude;
    }
    
    // 現在の周波数を取得（外部から参照用）
    public float GetFrequency()
    {
        return frequency;
    }
}

