using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingBar : MonoBehaviour
{
    [Header("ローディングバー設定")]
    [SerializeField] private Image fillImage; // バーのフィル部分
    [SerializeField] private Image backgroundImage; // バーの背景
    [SerializeField] private Text progressText; // 進捗テキスト（オプション）
    [SerializeField] private Text statusText; // ステータステキスト（オプション）
    
    [Header("アニメーション設定")]
    [SerializeField] private float fillSpeed = 1.0f; // バーが満タンになる速度
    [SerializeField] private AnimationCurve fillCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // アニメーションカーブ
    [SerializeField] private bool useSmoothFill = true; // スムーズなアニメーション
    [SerializeField] private float smoothTime = 0.3f; // スムーズアニメーションの時間
    
    [Header("視覚効果")]
    [SerializeField] private bool enablePulseEffect = true; // パルス効果を有効にする
    [SerializeField] private float pulseSpeed = 2.0f; // パルスの速度
    [SerializeField] private float pulseIntensity = 0.1f; // パルスの強度
    [SerializeField] private bool enableGlowEffect = true; // グロウ効果を有効にする
    [SerializeField] private float glowIntensity = 1.5f; // グロウの強度
    
    [Header("色設定")]
    [SerializeField] private Color fillColor = Color.green; // フィル色
    [SerializeField] private Color backgroundColor = Color.gray; // 背景色
    [SerializeField] private Color textColor = Color.white; // テキスト色
    [SerializeField] private bool useGradient = false; // グラデーションを使用するか
    [SerializeField] private Gradient fillGradient; // フィルグラデーション
    
    [Header("音響効果")]
    [SerializeField] private AudioSource audioSource; // オーディオソース
    [SerializeField] private AudioClip fillSound; // フィル音
    [SerializeField] private AudioClip completeSound; // 完了音
    [SerializeField] private bool playSoundOnComplete = true; // 完了時に音を再生
    
    [Header("デバッグ設定")]
    [SerializeField] private bool enableDebugLogs = true; // デバッグログを有効にする
    
    private float currentProgress = 0f; // 現在の進捗（0-1）
    private float targetProgress = 0f; // 目標進捗（0-1）
    private float smoothVelocity = 0f; // スムーズアニメーション用
    private bool isAnimating = false; // アニメーション中かどうか
    private bool isCompleted = false; // 完了したかどうか
    private Coroutine fillCoroutine; // フィルアニメーションのコルーチン
    
    void Start()
    {
        InitializeLoadingBar();
    }
    
    void Update()
    {
        // スムーズアニメーションの更新
        if (useSmoothFill && isAnimating)
        {
            UpdateSmoothFill();
        }
        
        // 視覚効果の更新
        UpdateVisualEffects();
    }
    
    /// <summary>
    /// ローディングバーを初期化
    /// </summary>
    private void InitializeLoadingBar()
    {
        // 初期状態を設定
        currentProgress = 0f;
        targetProgress = 0f;
        isAnimating = false;
        isCompleted = false;
        
        // バーの初期状態を設定
        if (fillImage != null)
        {
            fillImage.fillAmount = 0f;
            fillImage.color = fillColor;
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }
        
        // テキストの初期状態を設定
        UpdateProgressText();
        
        if (enableDebugLogs)
        {
            Debug.Log("ローディングバーを初期化しました。");
        }
    }
    
    /// <summary>
    /// スムーズフィルの更新
    /// </summary>
    private void UpdateSmoothFill()
    {
        if (Mathf.Abs(currentProgress - targetProgress) < 0.001f)
        {
            currentProgress = targetProgress;
            isAnimating = false;
            
            // 完了チェック
            if (currentProgress >= 1f && !isCompleted)
            {
                OnLoadingComplete();
            }
        }
        else
        {
            currentProgress = Mathf.SmoothDamp(currentProgress, targetProgress, ref smoothVelocity, smoothTime);
        }
        
        UpdateFillAmount();
    }
    
    /// <summary>
    /// 視覚効果の更新
    /// </summary>
    private void UpdateVisualEffects()
    {
        if (fillImage == null) return;
        
        // パルス効果
        if (enablePulseEffect && isAnimating)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
            Color currentColor = fillImage.color;
            currentColor.a = 1f + pulse;
            fillImage.color = currentColor;
        }
        
        // グロウ効果
        if (enableGlowEffect && isAnimating)
        {
            // グロウ効果の実装（必要に応じて）
            // ここでは簡単な色の変化で表現
            Color glowColor = fillColor * glowIntensity;
            fillImage.color = Color.Lerp(fillColor, glowColor, Mathf.Sin(Time.time * 2f) * 0.5f + 0.5f);
        }
    }
    
    /// <summary>
    /// フィル量を更新
    /// </summary>
    private void UpdateFillAmount()
    {
        if (fillImage != null)
        {
            float fillAmount = fillCurve.Evaluate(currentProgress);
            fillImage.fillAmount = fillAmount;
            
            // グラデーション適用
            if (useGradient && fillGradient != null)
            {
                fillImage.color = fillGradient.Evaluate(currentProgress);
            }
        }
        
        UpdateProgressText();
    }
    
    /// <summary>
    /// 進捗テキストを更新
    /// </summary>
    private void UpdateProgressText()
    {
        if (progressText != null)
        {
            progressText.text = $"{Mathf.RoundToInt(currentProgress * 100)}%";
            progressText.color = textColor;
        }
    }
    
    /// <summary>
    /// ローディングを開始
    /// </summary>
    public void StartLoading()
    {
        if (enableDebugLogs)
        {
            Debug.Log("ローディングを開始しました。");
        }
        
        isCompleted = false;
        isAnimating = true;
        
        // フィルアニメーションを開始
        if (fillCoroutine != null)
        {
            StopCoroutine(fillCoroutine);
        }
        fillCoroutine = StartCoroutine(FillAnimation());
    }
    
    /// <summary>
    /// フィルアニメーション
    /// </summary>
    private IEnumerator FillAnimation()
    {
        float startProgress = currentProgress;
        float duration = 1f / fillSpeed;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // アニメーションカーブを適用
            float curveValue = fillCurve.Evaluate(progress);
            targetProgress = Mathf.Lerp(startProgress, 1f, curveValue);
            
            // スムーズフィルが無効の場合は直接更新
            if (!useSmoothFill)
            {
                currentProgress = targetProgress;
                UpdateFillAmount();
            }
            
            yield return null;
        }
        
        // 完了
        targetProgress = 1f;
        if (!useSmoothFill)
        {
            currentProgress = 1f;
            UpdateFillAmount();
            OnLoadingComplete();
        }
    }
    
    /// <summary>
    /// 進捗を設定
    /// </summary>
    public void SetProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);
        targetProgress = progress;
        
        if (!useSmoothFill)
        {
            currentProgress = progress;
            UpdateFillAmount();
            
            if (progress >= 1f && !isCompleted)
            {
                OnLoadingComplete();
            }
        }
        else
        {
            isAnimating = true;
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"進捗を設定: {progress * 100}%");
        }
    }
    
    /// <summary>
    /// 進捗を追加
    /// </summary>
    public void AddProgress(float amount)
    {
        SetProgress(currentProgress + amount);
    }
    
    /// <summary>
    /// ローディング完了時の処理
    /// </summary>
    private void OnLoadingComplete()
    {
        isCompleted = true;
        isAnimating = false;
        
        // 完了音を再生
        if (playSoundOnComplete && audioSource != null && completeSound != null)
        {
            audioSource.PlayOneShot(completeSound);
        }
        
        // ステータステキストを更新
        if (statusText != null)
        {
            statusText.text = "完了！";
            statusText.color = Color.green;
        }
        
        if (enableDebugLogs)
        {
            Debug.Log("ローディングが完了しました。");
        }
        
        // 完了イベントを呼び出し
        OnLoadingCompleteEvent?.Invoke();
    }
    
    /// <summary>
    /// ローディングをリセット
    /// </summary>
    public void ResetLoading()
    {
        currentProgress = 0f;
        targetProgress = 0f;
        isAnimating = false;
        isCompleted = false;
        
        if (fillImage != null)
        {
            fillImage.fillAmount = 0f;
            fillImage.color = fillColor;
        }
        
        if (statusText != null)
        {
            statusText.text = "準備中...";
            statusText.color = textColor;
        }
        
        UpdateProgressText();
        
        if (enableDebugLogs)
        {
            Debug.Log("ローディングバーをリセットしました。");
        }
    }
    
    /// <summary>
    /// ステータステキストを設定
    /// </summary>
    public void SetStatusText(string text)
    {
        if (statusText != null)
        {
            statusText.text = text;
            statusText.color = textColor;
        }
    }
    
    /// <summary>
    /// 色を設定
    /// </summary>
    public void SetColors(Color fill, Color background, Color text)
    {
        fillColor = fill;
        backgroundColor = background;
        textColor = text;
        
        if (fillImage != null)
        {
            fillImage.color = fillColor;
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }
        
        if (progressText != null)
        {
            progressText.color = textColor;
        }
        
        if (statusText != null)
        {
            statusText.color = textColor;
        }
    }
    
    /// <summary>
    /// アニメーション設定を変更
    /// </summary>
    public void SetAnimationSettings(float speed, bool smooth, float smoothTime)
    {
        fillSpeed = speed;
        useSmoothFill = smooth;
        this.smoothTime = smoothTime;
    }
    
    /// <summary>
    /// 視覚効果を設定
    /// </summary>
    public void SetVisualEffects(bool pulse, bool glow, float pulseSpeed, float glowIntensity)
    {
        enablePulseEffect = pulse;
        enableGlowEffect = glow;
        this.pulseSpeed = pulseSpeed;
        this.glowIntensity = glowIntensity;
    }
    
    /// <summary>
    /// 現在の進捗を取得
    /// </summary>
    public float GetProgress()
    {
        return currentProgress;
    }
    
    /// <summary>
    /// 完了状態を取得
    /// </summary>
    public bool IsCompleted()
    {
        return isCompleted;
    }
    
    /// <summary>
    /// アニメーション中かどうかを取得
    /// </summary>
    public bool IsAnimating()
    {
        return isAnimating;
    }
    
    // イベント
    public System.Action OnLoadingCompleteEvent;
    
    void OnDestroy()
    {
        // コルーチンを停止
        if (fillCoroutine != null)
        {
            StopCoroutine(fillCoroutine);
        }
    }
}
