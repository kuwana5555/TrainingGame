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
    
    [Header("自動進行設定")]
    [SerializeField] private bool autoFillOnEnable = true; // オブジェクトアクティブ時に自動進行
    [SerializeField] private float autoFillSpeed = 50f; // 自動進行の速度（% per second）
    [SerializeField] private bool loopAutoFill = false; // 自動進行をループするか
    
    [Header("完了時実行機能")]
    [SerializeField] private bool enableCompletionActions = true; // 完了時実行機能を有効にする
    [SerializeField] private GameObject[] objectsToDeactivate; // 非アクティブにするオブジェクト
    [SerializeField] private GameObject[] objectsToActivate; // アクティブにするオブジェクト
    [SerializeField] private DelayedActivation[] delayedActivations; // 遅延実行するオブジェクト
    
    [Header("SE設定")]
    [SerializeField] private AudioSource audioSource; // オーディオソース
    [SerializeField] private AudioClip activationSE; // アクティブ時のSE
    [SerializeField] private AudioClip delayedActivationSE; // 遅延アクティブ時のSE
    [SerializeField] private bool playSEOnActivation = true; // アクティブ時にSEを再生するか
    [SerializeField] private bool playSEOnDelayedActivation = true; // 遅延アクティブ時にSEを再生するか
    
    private float currentProgress = 0f; // 現在の進捗（0-1）
    private float targetProgress = 0f; // 目標進捗（0-1）
    private float smoothVelocity = 0f; // スムーズアニメーション用
    private bool isAnimating = false; // アニメーション中かどうか
    private bool isCompleted = false; // 完了したかどうか
    private Coroutine fillCoroutine; // フィルアニメーションのコルーチン
    private Coroutine autoFillCoroutine; // 自動進行のコルーチン
    private Vector3 originalFillScale; // 元のフィルスケール
    private Vector3 originalFillPosition; // 元のフィル位置
    private bool isAutoFilling = false; // 自動進行中かどうか
    
    [System.Serializable]
    public class DelayedActivation
    {
        public GameObject targetObject; // 対象オブジェクト
        public float delayTime = 1f; // 遅延時間（秒）
        public bool isActive = true; // 有効かどうか
        public AudioClip customSE; // カスタムSE（オプション）
        public bool playCustomSE = false; // カスタムSEを再生するか
    }
    
    void Start()
    {
        InitializeLoadingBar();
    }
    
    void OnEnable()
    {
        // オブジェクトがアクティブになった時に自動進行を開始
        if (autoFillOnEnable)
        {
            StartAutoFill();
        }
    }
    
    void OnDisable()
    {
        // オブジェクトが非アクティブになった時に自動進行を停止
        StopAutoFill();
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
        
        // 完了時実行機能を実行
        if (enableCompletionActions)
        {
            ExecuteCompletionActions();
        }
        
        // 完了イベントを呼び出し
        OnLoadingCompleteEvent?.Invoke();
    }
    
    /// <summary>
    /// 完了時実行機能
    /// </summary>
    private void ExecuteCompletionActions()
    {
        // 指定したオブジェクトを非アクティブにする
        if (objectsToDeactivate != null)
        {
            foreach (GameObject obj in objectsToDeactivate)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
        
        // 指定したオブジェクトをアクティブにする
        if (objectsToActivate != null)
        {
            foreach (GameObject obj in objectsToActivate)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                    
                    // SEを再生
                    if (playSEOnActivation && audioSource != null && activationSE != null)
                    {
                        audioSource.PlayOneShot(activationSE);
                    }
                }
            }
        }
        
        // 遅延実行するオブジェクトを開始
        if (delayedActivations != null)
        {
            foreach (DelayedActivation delayedActivation in delayedActivations)
            {
                if (delayedActivation.isActive && delayedActivation.targetObject != null)
                {
                    StartCoroutine(DelayedActivationCoroutine(delayedActivation));
                }
            }
        }
    }
    
    /// <summary>
    /// 遅延実行コルーチン
    /// </summary>
    private IEnumerator DelayedActivationCoroutine(DelayedActivation delayedActivation)
    {
        yield return new WaitForSeconds(delayedActivation.delayTime);
        
        if (delayedActivation.targetObject != null)
        {
            delayedActivation.targetObject.SetActive(true);
            
            // SEを再生
            if (playSEOnDelayedActivation && audioSource != null)
            {
                // カスタムSEが設定されている場合はそれを再生
                if (delayedActivation.playCustomSE && delayedActivation.customSE != null)
                {
                    audioSource.PlayOneShot(delayedActivation.customSE);
                }
                // カスタムSEが設定されていない場合はデフォルトSEを再生
                else if (delayedActivationSE != null)
                {
                    audioSource.PlayOneShot(delayedActivationSE);
                }
            }
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
    
    /// <summary>
    /// 自動進行を開始
    /// </summary>
    public void StartAutoFill()
    {
        if (autoFillCoroutine != null)
        {
            StopCoroutine(autoFillCoroutine);
        }
        
        isAutoFilling = true;
        autoFillCoroutine = StartCoroutine(AutoFillCoroutine());
    }
    
    /// <summary>
    /// 自動進行を停止
    /// </summary>
    public void StopAutoFill()
    {
        isAutoFilling = false;
        
        if (autoFillCoroutine != null)
        {
            StopCoroutine(autoFillCoroutine);
        }
    }
    
    /// <summary>
    /// 自動進行コルーチン
    /// </summary>
    private IEnumerator AutoFillCoroutine()
    {
        while (isAutoFilling)
        {
            // 現在の進捗を取得
            float currentPercent = GetProgressPercent();
            
            // 進捗を追加
            float addAmount = autoFillSpeed * Time.deltaTime;
            AddProgressPercent(addAmount);
            
            // 100%に達した場合
            if (currentPercent >= 100f)
            {
                if (loopAutoFill)
                {
                    // ループする場合はリセット
                    ResetLoading();
                }
                else
                {
                    // ループしない場合は停止
                    StopAutoFill();
                }
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// 自動進行の設定
    /// </summary>
    public void SetAutoFillSettings(bool autoFillOnEnable, float speed, bool loop)
    {
        this.autoFillOnEnable = autoFillOnEnable;
        this.autoFillSpeed = speed;
        this.loopAutoFill = loop;
    }
    
    /// <summary>
    /// 自動進行中かどうかを取得
    /// </summary>
    public bool IsAutoFilling()
    {
        return isAutoFilling;
    }
    
    /// <summary>
    /// 完了時実行機能の設定
    /// </summary>
    public void SetCompletionActions(bool enable, GameObject[] deactivateObjects, GameObject[] activateObjects, DelayedActivation[] delayedObjects)
    {
        enableCompletionActions = enable;
        objectsToDeactivate = deactivateObjects;
        objectsToActivate = activateObjects;
        delayedActivations = delayedObjects;
    }
    
    /// <summary>
    /// 完了時実行機能を有効/無効にする
    /// </summary>
    public void SetCompletionActionsEnabled(bool enabled)
    {
        enableCompletionActions = enabled;
    }
    
    /// <summary>
    /// 非アクティブにするオブジェクトを設定
    /// </summary>
    public void SetObjectsToDeactivate(GameObject[] objects)
    {
        objectsToDeactivate = objects;
    }
    
    /// <summary>
    /// アクティブにするオブジェクトを設定
    /// </summary>
    public void SetObjectsToActivate(GameObject[] objects)
    {
        objectsToActivate = objects;
    }
    
    /// <summary>
    /// 遅延実行するオブジェクトを設定
    /// </summary>
    public void SetDelayedActivations(DelayedActivation[] delayedObjects)
    {
        delayedActivations = delayedObjects;
    }
    
    /// <summary>
    /// SE設定を変更
    /// </summary>
    public void SetSESettings(AudioSource audioSource, AudioClip activationSE, AudioClip delayedActivationSE, bool playOnActivation, bool playOnDelayedActivation)
    {
        this.audioSource = audioSource;
        this.activationSE = activationSE;
        this.delayedActivationSE = delayedActivationSE;
        this.playSEOnActivation = playOnActivation;
        this.playSEOnDelayedActivation = playOnDelayedActivation;
    }
    
    /// <summary>
    /// アクティブ時のSEを再生
    /// </summary>
    public void PlayActivationSE()
    {
        if (audioSource != null && activationSE != null)
        {
            audioSource.PlayOneShot(activationSE);
        }
    }
    
    /// <summary>
    /// 遅延アクティブ時のSEを再生
    /// </summary>
    public void PlayDelayedActivationSE()
    {
        if (audioSource != null && delayedActivationSE != null)
        {
            audioSource.PlayOneShot(delayedActivationSE);
        }
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
        
        if (autoFillCoroutine != null)
        {
            StopCoroutine(autoFillCoroutine);
        }
    }
}
