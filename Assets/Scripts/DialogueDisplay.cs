using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueDisplay : MonoBehaviour
{
    [Header("テキスト設定")]
    [SerializeField] private TextMeshProUGUI dialogueText;  // 表示するテキストコンポーネント
    [SerializeField] private string fullText = "";          // 表示する完全なテキスト
    [SerializeField] private float displayInterval = 0.05f; // 文字表示間隔（秒）
    [SerializeField] private bool autoStart = false;        // 自動開始するか
    
    [Header("表示設定")]
    [SerializeField] private bool useTypewriterEffect = true;  // タイプライター効果を使用するか
    [SerializeField] private bool showCursor = true;           // カーソルを表示するか
    [SerializeField] private string cursorText = "|";          // カーソルの文字
    [SerializeField] private float cursorBlinkSpeed = 0.5f;    // カーソル点滅速度
    
    [Header("色・フォント設定")]
    [SerializeField] private Color normalColor = Color.white;     // 通常の文字色
    [SerializeField] private Color highlightColor = Color.yellow; // ハイライト色
    [SerializeField] private bool useGradient = false;           // グラデーションを使用するか
    [SerializeField] private Color startColor = Color.white;     // グラデーション開始色
    [SerializeField] private Color endColor = Color.blue;        // グラデーション終了色
    
    [Header("音声設定")]
    [SerializeField] private AudioSource audioSource;            // 音声を再生するAudioSource
    [SerializeField] private AudioClip typeSound;                // タイプ音
    [SerializeField] private AudioClip completeSound;            // 完了音
    [SerializeField] private float volume = 0.5f;                // 音量
    
    [Header("制御設定")]
    [SerializeField] private bool canSkip = true;                // スキップ可能か
    [SerializeField] private bool canPause = true;               // 一時停止可能か
    [SerializeField] private KeyCode skipKey = KeyCode.Space;    // スキップキー
    [SerializeField] private KeyCode pauseKey = KeyCode.P;       // 一時停止キー
    
    [Header("イベント設定")]
    [SerializeField] private bool triggerOnStart = true;         // Start時に自動実行するか
    [SerializeField] private bool destroyOnComplete = false;     // 完了時にオブジェクトを削除するか
    
    // 内部変数
    private string currentText = "";         // 現在表示中のテキスト
    private int currentIndex = 0;            // 現在の文字インデックス
    private bool isDisplaying = false;       // 表示中かどうか
    private bool isPaused = false;           // 一時停止中かどうか
    private bool isCompleted = false;        // 完了したかどうか
    private Coroutine displayCoroutine;      // 表示コルーチンの参照
    private Coroutine cursorCoroutine;       // カーソルコルーチンの参照
    private Coroutine typeSoundCoroutine;    // タイプ音コルーチンの参照
    
    // イベント
    public System.Action OnDialogueStart;    // セリフ開始時
    public System.Action OnDialogueComplete; // セリフ完了時
    public System.Action OnDialoguePause;    // セリフ一時停止時
    public System.Action OnDialogueResume;   // セリフ再開時
    public System.Action OnDialogueSkip;     // セリフスキップ時

    void Start()
    {
        // AudioSourceの設定
        SetupAudioSource();
        
        // 初期テキスト設定
        if (dialogueText != null)
        {
            dialogueText.text = "";
            dialogueText.color = normalColor;
            // TextMeshProの設定を確実にする
            dialogueText.ForceMeshUpdate();
        }
        
        // 自動開始
        if (triggerOnStart && !string.IsNullOrEmpty(fullText))
        {
            StartDialogue();
        }
    }

    void Update()
    {
        // キー入力処理
        HandleInput();
    }

    // セリフ表示を開始
    public void StartDialogue()
    {
        if (isDisplaying || isCompleted)
        {
            return;
        }
        
        // テキストが空の場合は警告
        if (string.IsNullOrEmpty(fullText))
        {
            Debug.LogWarning("DialogueDisplay: 表示するテキストが設定されていません");
            return;
        }
        
        // TextMeshProコンポーネントが設定されていない場合は警告
        if (dialogueText == null)
        {
            Debug.LogWarning("DialogueDisplay: TextMeshProUGUIコンポーネントが設定されていません");
            return;
        }
        
        isDisplaying = true;
        isPaused = false;
        isCompleted = false;
        currentIndex = 0;
        currentText = "";
        
        if (dialogueText != null)
        {
            dialogueText.text = "";
            dialogueText.color = normalColor;
            dialogueText.ForceMeshUpdate();
        }
        
        // 表示コルーチンを開始
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }
        displayCoroutine = StartCoroutine(DisplayText());
        
        // タイプ音を開始
        if (typeSound != null && audioSource != null)
        {
            if (typeSoundCoroutine != null)
            {
                StopCoroutine(typeSoundCoroutine);
            }
            typeSoundCoroutine = StartCoroutine(PlayTypeSound());
        }
        
        // カーソル表示
        if (showCursor && useTypewriterEffect)
        {
            if (cursorCoroutine != null)
            {
                StopCoroutine(cursorCoroutine);
            }
            cursorCoroutine = StartCoroutine(BlinkCursor());
        }
        
        OnDialogueStart?.Invoke();
        Debug.Log($"セリフ表示開始: '{fullText}'");
    }

    // テキスト表示コルーチン
    private IEnumerator DisplayText()
    {
        Debug.Log($"テキスト表示開始: 文字数={fullText.Length}, 間隔={displayInterval}秒");
        
        while (currentIndex < fullText.Length && isDisplaying)
        {
            // 一時停止中は待機
            while (isPaused)
            {
                yield return null;
            }
            
            // 文字を追加
            currentText += fullText[currentIndex];
            currentIndex++;
            
            // テキストを更新
            UpdateDisplayText();
            
            // デバッグログ（最初の数文字のみ）
            if (currentIndex <= 5)
            {
                Debug.Log($"文字表示: '{currentText}' (進捗: {currentIndex}/{fullText.Length})");
            }
            
            
            // 間隔を待機
            if (useTypewriterEffect)
            {
                yield return new WaitForSeconds(displayInterval);
            }
        }
        
        // 表示完了
        CompleteDialogue();
    }

    // 表示テキストを更新
    private void UpdateDisplayText()
    {
        if (dialogueText == null) return;
        
        string displayText = currentText;
        
        // カーソルを追加
        if (showCursor && useTypewriterEffect && !isCompleted)
        {
            displayText += cursorText;
        }
        
        // 色を適用
        if (useGradient && currentText.Length > 0)
        {
            displayText = ApplyGradient(displayText);
        }
        
        dialogueText.text = displayText;
        // TextMeshProの更新を強制実行
        dialogueText.ForceMeshUpdate();
    }

    // グラデーションを適用
    private string ApplyGradient(string text)
    {
        string result = "";
        for (int i = 0; i < text.Length; i++)
        {
            float t = (float)i / (text.Length - 1);
            Color color = Color.Lerp(startColor, endColor, t);
            result += $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text[i]}</color>";
        }
        return result;
    }

    // カーソル点滅コルーチン
    private IEnumerator BlinkCursor()
    {
        while (isDisplaying && !isCompleted)
        {
            yield return new WaitForSeconds(cursorBlinkSpeed);
        }
    }

    // セリフ完了処理
    private void CompleteDialogue()
    {
        isDisplaying = false;
        isCompleted = true;
        
        // タイプ音を停止
        if (typeSoundCoroutine != null)
        {
            StopCoroutine(typeSoundCoroutine);
            typeSoundCoroutine = null;
        }
        
        // タイプ音の再生を強制的に停止
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        // カーソルを非表示
        if (showCursor && dialogueText != null)
        {
            dialogueText.text = currentText;
        }
        
        // 完了音を再生
        if (completeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(completeSound, volume);
        }
        
        OnDialogueComplete?.Invoke();
        Debug.Log("セリフ表示完了");
        
        // オブジェクトを削除
        if (destroyOnComplete)
        {
            Destroy(gameObject);
        }
    }

    // 入力処理
    private void HandleInput()
    {
        if (!isDisplaying && !isCompleted) return;
        
        // スキップ
        if (canSkip && Input.GetKeyDown(skipKey))
        {
            SkipDialogue();
        }
        
        // 一時停止/再開
        if (canPause && Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
            {
                ResumeDialogue();
            }
            else
            {
                PauseDialogue();
            }
        }
    }

    // セリフをスキップ
    public void SkipDialogue()
    {
        if (!canSkip || isCompleted) return;
        
        // 表示を即座に完了
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }
        
        currentText = fullText;
        currentIndex = fullText.Length;
        UpdateDisplayText();
        
        OnDialogueSkip?.Invoke();
        CompleteDialogue();
        Debug.Log("セリフをスキップしました");
    }

    // セリフを一時停止
    public void PauseDialogue()
    {
        if (!canPause || isPaused || isCompleted) return;
        
        isPaused = true;
        OnDialoguePause?.Invoke();
        Debug.Log("セリフを一時停止しました");
    }

    // セリフを再開
    public void ResumeDialogue()
    {
        if (!canPause || !isPaused || isCompleted) return;
        
        isPaused = false;
        OnDialogueResume?.Invoke();
        Debug.Log("セリフを再開しました");
    }

    // セリフをリセット
    public void ResetDialogue()
    {
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }
        if (cursorCoroutine != null)
        {
            StopCoroutine(cursorCoroutine);
        }
        if (typeSoundCoroutine != null)
        {
            StopCoroutine(typeSoundCoroutine);
        }
        
        // タイプ音の再生を停止
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        isDisplaying = false;
        isPaused = false;
        isCompleted = false;
        currentIndex = 0;
        currentText = "";
        
        if (dialogueText != null)
        {
            dialogueText.text = "";
        }
        
        Debug.Log("セリフをリセットしました");
    }

    // 新しいテキストを設定して開始
    public void SetTextAndStart(string newText)
    {
        fullText = newText;
        ResetDialogue();
        StartDialogue();
    }
    
    // 日本語対応のテスト用メソッド
    public void TestJapaneseText()
    {
        SetTextAndStart("こんにちは、世界！これは日本語のテストです。");
    }

    // AudioSourceの設定
    private void SetupAudioSource()
    {
        if (audioSource == null && (typeSound != null || completeSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    // 外部からアクセス可能なプロパティ
    public bool IsDisplaying => isDisplaying;
    public bool IsPaused => isPaused;
    public bool IsCompleted => isCompleted;
    public float Progress => fullText.Length > 0 ? (float)currentIndex / fullText.Length : 0f;
    public string CurrentText => currentText;
    public string FullText => fullText;

    // デバッグ用：Inspectorでボタンから手動で開始
    [ContextMenu("Start Dialogue")]
    public void ManualStartDialogue()
    {
        StartDialogue();
    }
    
    // デバッグ用：Inspectorでボタンから手動でスキップ
    [ContextMenu("Skip Dialogue")]
    public void ManualSkipDialogue()
    {
        SkipDialogue();
    }
    
    // デバッグ用：Inspectorでボタンから手動でリセット
    [ContextMenu("Reset Dialogue")]
    public void ManualResetDialogue()
    {
        ResetDialogue();
    }
    
    // デバッグ用：Inspectorでボタンから日本語テスト
    [ContextMenu("Test Japanese Text")]
    public void ManualTestJapaneseText()
    {
        TestJapaneseText();
    }
    
    // タイプ音を連続再生するコルーチン
    private IEnumerator PlayTypeSound()
    {
        while (isDisplaying && !isCompleted)
        {
            // タイプ音を再生
            if (typeSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(typeSound, volume);
                
                // 音の長さ分待機
                yield return new WaitForSeconds(typeSound.length);
                
                // 表示がまだ続いている場合は再度再生
                if (isDisplaying && !isCompleted)
                {
                    // 音が終わったが表示が続いている場合、再度再生
                    continue;
                }
            }
            else
            {
                // タイプ音が設定されていない場合は待機
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
