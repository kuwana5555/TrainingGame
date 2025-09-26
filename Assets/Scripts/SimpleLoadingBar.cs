using UnityEngine;
using System.Collections;

public class SimpleLoadingBar : MonoBehaviour
{
    [Header("スプライト設定")]
    [SerializeField] private SpriteRenderer backgroundSprite; // 背景スプライト
    [SerializeField] private SpriteRenderer fillSprite; // フィルスプライト
    [SerializeField] private TextMesh progressText; // 進捗テキスト（オプション）
    
    [Header("バー設定")]
    [SerializeField] private float barWidth = 2f; // バーの幅
    [SerializeField] private float barHeight = 0.3f; // バーの高さ
    [SerializeField] private bool fillFromLeft = true; // 左から右にフィルするか
    
    [Header("アニメーション設定")]
    [SerializeField] private float fillSpeed = 1.0f; // バーが満タンになる速度
    [SerializeField] private bool useSmoothFill = true; // スムーズなアニメーション
    [SerializeField] private float smoothTime = 0.3f; // スムーズアニメーションの時間
    
    [Header("色設定")]
    [SerializeField] private Color fillColor = Color.green; // フィル色
    [SerializeField] private Color backgroundColor = Color.gray; // 背景色
    [SerializeField] private Color textColor = Color.white; // テキスト色
    
    [Header("Inspector制御")]
    [SerializeField] private float barValue = 0f; // Inspectorで制御するバーの値（0-100）
    [SerializeField] private bool enableInspectorControl = true; // Inspector制御を有効にする
    
    private float currentProgress = 0f; // 現在の進捗（0-1）
    private float targetProgress = 0f; // 目標進捗（0-1）
    private float smoothVelocity = 0f; // スムーズアニメーション用
    private bool isAnimating = false; // アニメーション中かどうか
    private bool isCompleted = false; // 完了したかどうか
    private Coroutine fillCoroutine; // フィルアニメーションのコルーチン
    private Vector3 originalFillScale; // 元のフィルスケール
    private Vector3 originalFillPosition; // 元のフィル位置
    
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
        
        // Inspector制御の更新
        if (enableInspectorControl)
        {
            UpdateInspectorControl();
        }
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
        
        // スプライトの初期設定
        SetupSprites();
        
        // バーの初期状態を設定
        UpdateLoadingBar();
    }
    
    /// <summary>
    /// スプライトの設定
    /// </summary>
    private void SetupSprites()
    {
        // 背景スプライトの設定
        if (backgroundSprite != null)
        {
            backgroundSprite.color = backgroundColor;
            backgroundSprite.transform.localScale = new Vector3(barWidth, barHeight, 1f);
        }
        
        // フィルスプライトの設定
        if (fillSprite != null)
        {
            fillSprite.color = fillColor;
            originalFillScale = new Vector3(0f, barHeight, 1f);
            originalFillPosition = fillSprite.transform.localPosition;
            fillSprite.transform.localScale = originalFillScale;
        }
        
        // テキストの設定
        if (progressText != null)
        {
            progressText.color = textColor;
            progressText.text = "0%";
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
        
        UpdateLoadingBar();
    }
    
    /// <summary>
    /// ローディングバーを更新
    /// </summary>
    private void UpdateLoadingBar()
    {
        if (fillSprite == null) return;
        
        // フィルスプライトのスケールを更新
        Vector3 newScale = originalFillScale;
        newScale.x = barWidth * currentProgress;
        fillSprite.transform.localScale = newScale;
        
        // フィルスプライトの位置を調整
        Vector3 newPosition = originalFillPosition;
        if (fillFromLeft)
        {
            newPosition.x = originalFillPosition.x - (barWidth * (1f - currentProgress)) / 2f;
        }
        else
        {
            newPosition.x = originalFillPosition.x + (barWidth * (1f - currentProgress)) / 2f;
        }
        fillSprite.transform.localPosition = newPosition;
        
        // テキストを更新
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
    /// Inspector制御の更新
    /// </summary>
    private void UpdateInspectorControl()
    {
        // Inspectorの値が変更された場合に進捗を更新
        float inspectorProgress = barValue / 100f;
        if (Mathf.Abs(inspectorProgress - currentProgress) > 0.001f)
        {
            SetProgress(inspectorProgress);
        }
    }
    
    /// <summary>
    /// ローディングを開始
    /// </summary>
    public void StartLoading()
    {
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
            targetProgress = Mathf.Lerp(startProgress, 1f, progress);
            
            // スムーズフィルが無効の場合は直接更新
            if (!useSmoothFill)
            {
                currentProgress = targetProgress;
                UpdateLoadingBar();
            }
            
            yield return null;
        }
        
        // 完了
        targetProgress = 1f;
        if (!useSmoothFill)
        {
            currentProgress = 1f;
            UpdateLoadingBar();
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
            UpdateLoadingBar();
            
            if (progress >= 1f && !isCompleted)
            {
                OnLoadingComplete();
            }
        }
        else
        {
            isAnimating = true;
        }
    }
    
    /// <summary>
    /// 進捗をパーセントで設定
    /// </summary>
    public void SetProgressPercent(float percent)
    {
        float progress = percent / 100f;
        SetProgress(progress);
    }
    
    /// <summary>
    /// 進捗を追加
    /// </summary>
    public void AddProgress(float amount)
    {
        SetProgress(currentProgress + amount);
    }
    
    /// <summary>
    /// 進捗をパーセントで追加
    /// </summary>
    public void AddProgressPercent(float percent)
    {
        float amount = percent / 100f;
        AddProgress(amount);
    }
    
    /// <summary>
    /// ローディング完了時の処理
    /// </summary>
    private void OnLoadingComplete()
    {
        isCompleted = true;
        isAnimating = false;
        
        if (progressText != null)
        {
            progressText.text = "完了！";
            progressText.color = Color.green;
        }
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
        
        // フィルスプライトをリセット
        if (fillSprite != null)
        {
            fillSprite.transform.localScale = originalFillScale;
            fillSprite.transform.localPosition = originalFillPosition;
            fillSprite.color = fillColor;
        }
        
        if (progressText != null)
        {
            progressText.text = "0%";
            progressText.color = textColor;
        }
        
        UpdateLoadingBar();
    }
    
    /// <summary>
    /// 色を設定
    /// </summary>
    public void SetColors(Color fill, Color background, Color text)
    {
        fillColor = fill;
        backgroundColor = background;
        textColor = text;
        
        if (fillSprite != null)
        {
            fillSprite.color = fillColor;
        }
        
        if (backgroundSprite != null)
        {
            backgroundSprite.color = backgroundColor;
        }
        
        if (progressText != null)
        {
            progressText.color = textColor;
        }
    }
    
    /// <summary>
    /// バーサイズを設定
    /// </summary>
    public void SetBarSize(float width, float height)
    {
        barWidth = width;
        barHeight = height;
        
        // スプライトのサイズを更新
        SetupSprites();
        UpdateLoadingBar();
    }
    
    /// <summary>
    /// 現在の進捗を取得（0-1）
    /// </summary>
    public float GetProgress()
    {
        return currentProgress;
    }
    
    /// <summary>
    /// 現在の進捗をパーセントで取得
    /// </summary>
    public float GetProgressPercent()
    {
        return currentProgress * 100f;
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
    
    /// <summary>
    /// バーの値（0-100の範囲で設定・取得）
    /// シンプルな制御用
    /// </summary>
    public float BarValue
    {
        get
        {
            return GetProgressPercent();
        }
        set
        {
            SetProgressPercent(value);
        }
    }
    
    /// <summary>
    /// Inspectorでバーの値を設定
    /// </summary>
    public void SetBarValueFromInspector(float value)
    {
        barValue = Mathf.Clamp(value, 0f, 100f);
        SetProgressPercent(barValue);
    }
    
    /// <summary>
    /// Inspectorでバーの値を取得
    /// </summary>
    public float GetBarValueFromInspector()
    {
        return barValue;
    }
    
    void OnValidate()
    {
        // Inspectorで値が変更された時に呼ばれる
        if (Application.isPlaying && enableInspectorControl)
        {
            UpdateInspectorControl();
        }
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
