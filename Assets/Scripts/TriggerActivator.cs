using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerActivator : MonoBehaviour
{
    [Header("トリガー設定")]
    [SerializeField] private string targetTag = "Player";  // 対象のタグ
    [SerializeField] private GameObject[] targetObjects;   // 基本的なオブジェクト配列（アイテム運搬システム未使用時）
    [SerializeField] private bool activateOnEnter = true;  // 入った時にアクティブにするか
    [SerializeField] private bool deactivateOnExit = false; // 出た時に非アクティブにするか
    
    [Header("複数接触設定")]
    [SerializeField] private bool useMultiTrigger = false;  // 複数接触検知を使用するか
    [SerializeField] private int requiredCount = 3;         // 必要な接触数
    [SerializeField] private bool requireAllSameTag = true; // 全て同じタグが必要か
    [SerializeField] private string[] additionalTags = {};  // 追加で許可するタグ（requireAllSameTagがfalseの場合）
    
    [Header("レイヤー別設定")]
    [SerializeField] private bool useLayerCondition = false; // レイヤー条件を使用するか
    [SerializeField] private LayerMask targetLayer = -1;     // 対象レイヤー
    [SerializeField] private LayerMask excludeLayer = 0;     // 除外レイヤー
    [SerializeField] private GameObject[] layerTargetObjects; // レイヤー条件時の対象オブジェクト配列
    [SerializeField] private bool useLayerObjects = false;   // レイヤー条件時に別オブジェクトを使用するか
    
    [Header("アイテム運搬設定")]
    [SerializeField] private bool useItemCarrySystem = false; // アイテム運搬システムを使用するか
    [SerializeField] private string grabbableTag = "Grabbable"; // 持ち運び可能なアイテムのタグ
    [SerializeField] private LayerMask virusLayer = 0;        // ウイルスレイヤー
    [SerializeField] private bool excludeVirusFromCount = true; // ウイルスレイヤーをカウントから除外するか
    [SerializeField] private GameObject[] virusObjects;       // ウイルス検知時にアクティブにするオブジェクト
    [SerializeField] private GameObject[] normalObjects;      // 通常アイテム用オブジェクト（未設定時はtargetObjectsを使用）
    
    [Header("ウイルス検知タイマー設定")]
    [SerializeField] private bool useVirusTimer = false;      // ウイルス検知にタイマーを使用するか
    [SerializeField] private float virusDetectionTime = 2.0f; // ウイルス検知に必要な時間（秒）
    [SerializeField] private bool showVirusTimerProgress = true; // ウイルスタイマーの進行状況を表示するか
    
    [Header("音声設定")]
    [SerializeField] private AudioSource audioSource;      // 音声を再生するAudioSource
    [SerializeField] private AudioClip audioClip;          // 再生する音声クリップ
    [SerializeField] private bool playOnEnter = true;      // 入った時に音声を再生するか
    [SerializeField] private bool playOnExit = false;      // 出た時に音声を再生するか
    [SerializeField] private float volume = 1.0f;          // 音量（0.0～1.0）
    [SerializeField] private bool loop = false;            // ループ再生するか
    
    [Header("特殊音声設定")]
    [SerializeField] private AudioClip virusDetectionSound; // ウイルス検知時の音声クリップ
    [SerializeField] private AudioClip multiTriggerSound;   // 複数接触条件達成時の音声クリップ
    [SerializeField] private bool playVirusSound = true;    // ウイルス検知音を再生するか
    [SerializeField] private bool playMultiTriggerSound = true; // 複数接触音を再生するか
    [SerializeField] private float virusSoundVolume = 1.0f; // ウイルス音の音量
    [SerializeField] private float multiTriggerSoundVolume = 1.0f; // 複数接触音の音量
    
    [Header("時間差実行設定")]
    [SerializeField] private bool useDelayedActivation = false; // 時間差でアクティブ化するか
    [SerializeField] private float activationDelay = 1.0f;     // アクティブ化までの遅延時間（秒）
    [SerializeField] private bool showDelayProgress = true;    // 遅延の進行状況を表示するか
    
    [Header("動作設定")]
    [SerializeField] private bool oneTimeOnly = true;      // 1回のみ機能するか
    [SerializeField] private bool persistentInScene = true; // シーン内で継続するか
    
    [Header("タイマー設定")]
    [SerializeField] private bool useTimerTrigger = false;  // タイマートリガーを使用するか
    [SerializeField] private float triggerDuration = 2.0f;  // トリガーが発動するまでの時間（秒）
    [SerializeField] private bool showTimerProgress = true; // タイマーの進行状況を表示するか
    
    private bool hasTriggered = false;  // トリガーが発動したかどうか
    private bool isPlayerInTrigger = false; // プレイヤーがトリガー内にいるか
    private bool hasPlayedAudio = false; // 音声が再生されたかどうか
    
    // タイマー関連の変数
    private float timerProgress = 0f;    // タイマーの進行状況（0.0～1.0）
    private bool isTimerRunning = false; // タイマーが動作中かどうか
    private float currentTimer = 0f;     // 現在のタイマー値
    
    // 複数接触検知用の変数
    private List<Collider2D> activeColliders = new List<Collider2D>(); // 現在接触中のコライダー
    private List<Collider2D> validColliders = new List<Collider2D>();  // カウント対象のコライダー（ウイルス除外）
    private int currentCount = 0;        // 現在の接触数
    private bool isMultiTriggerActive = false; // 複数トリガーがアクティブかどうか
    
    // ウイルス検知タイマー用の変数
    private bool isVirusTimerRunning = false; // ウイルスタイマーが動作中かどうか
    private float virusTimerProgress = 0f;    // ウイルスタイマーの進行状況（0.0～1.0）
    private float currentVirusTimer = 0f;     // 現在のウイルスタイマー値
    private Collider2D currentVirusCollider = null; // 現在タイマー中のウイルスコライダー
    
    // 時間差実行用の変数
    private bool isDelayedActivationRunning = false; // 時間差アクティブ化が動作中かどうか
    private float delayedActivationProgress = 0f;    // 時間差アクティブ化の進行状況（0.0～1.0）
    private float currentDelayedTimer = 0f;          // 現在の時間差タイマー値
    private Coroutine delayedActivationCoroutine;    // 時間差アクティブ化コルーチンの参照
    private GameObject[] pendingActivationObjects;   // 待機中のアクティブ化対象オブジェクト
    
    // Start is called before the first frame update
    void Start()
    {
        // 初期状態でオブジェクトを非アクティブにする
        if (activateOnEnter)
        {
            SetObjectsActive(false);
        }
        
        // AudioSourceの設定
        SetupAudioSource();
    }

    // Update is called once per frame
    void Update()
    {
        // タイマートリガーが有効な場合の処理
        if (useTimerTrigger && isPlayerInTrigger && !hasTriggered)
        {
            UpdateTimer();
        }
        else if (useTimerTrigger && !isPlayerInTrigger)
        {
            // プレイヤーがトリガーから出た場合はタイマーをリセット
            ResetTimer();
        }
        
        // ウイルス検知タイマーが有効な場合の処理
        if (useVirusTimer && isVirusTimerRunning)
        {
            UpdateVirusTimer();
        }
        
        // 時間差アクティブ化が有効な場合の処理
        if (useDelayedActivation && isDelayedActivationRunning)
        {
            UpdateDelayedActivation();
        }
        
        // プレイヤーがトリガー内にいる間は継続してアクティブ状態を維持
        if (isPlayerInTrigger && persistentInScene && !useTimerTrigger)
        {
            if (activateOnEnter)
            {
                SetObjectsActive(true);
            }
        }
    }

    // トリガーに入った時の処理
    void OnTriggerEnter2D(Collider2D other)
    {
        // 複数接触検知を使用する場合
        if (useMultiTrigger)
        {
            HandleMultiTriggerEnter(other);
        }
        else
        {
            // 従来の単一接触処理
            HandleSingleTriggerEnter(other);
        }
    }
    
    // 単一接触処理
    private void HandleSingleTriggerEnter(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            isPlayerInTrigger = true;
            
            // 1回のみの設定で、既に発動済みの場合は何もしない
            if (oneTimeOnly && hasTriggered)
            {
                return;
            }
            
            // タイマートリガーが無効な場合の即座の処理
            if (!useTimerTrigger)
            {
                if (activateOnEnter)
                {
                    SetObjectsActive(true);
                    hasTriggered = true;
                    Debug.Log($"トリガー発動: {gameObject.name} - オブジェクトをアクティブにしました");
                }
                
                // 音声再生（入った時）
                if (playOnEnter && !hasPlayedAudio)
                {
                    PlayAudio();
                }
            }
            else
            {
                // タイマートリガーの場合はタイマーを開始
                StartTimer();
            }
        }
    }
    
    // 複数接触処理（入った時）
    private void HandleMultiTriggerEnter(Collider2D other)
    {
        // アイテム運搬システムを使用する場合
        if (useItemCarrySystem)
        {
            HandleItemCarryEnter(other);
            return;
        }
        
        // タグのチェック
        if (!IsValidTag(other.tag))
        {
            return;
        }
        
        // 既に接触中の場合はスキップ
        if (activeColliders.Contains(other))
        {
            return;
        }
        
        // 接触リストに追加
        activeColliders.Add(other);
        currentCount = activeColliders.Count;
        
        Debug.Log($"複数接触検知: {other.name} が入りました (現在の数: {currentCount}/{requiredCount})");
        
        // 必要な数に達したかチェック
        if (currentCount >= requiredCount)
        {
            if (!isMultiTriggerActive)
            {
                ActivateMultiTrigger();
            }
        }
    }
    
    // アイテム運搬処理（入った時）
    private void HandleItemCarryEnter(Collider2D other)
    {
        // Grabbableタグのチェック
        if (!other.CompareTag(grabbableTag))
        {
            return;
        }
        
        // 既に接触中の場合はスキップ
        if (activeColliders.Contains(other))
        {
            return;
        }
        
        // 接触リストに追加
        activeColliders.Add(other);
        
        // ウイルスレイヤーをカウントから除外するかチェック
        bool isVirusItem = excludeVirusFromCount && IsInLayer(other.gameObject, virusLayer);
        
        if (!isVirusItem)
        {
            // カウント対象のリストに追加
            validColliders.Add(other);
            currentCount = validColliders.Count;
            
            Debug.Log($"アイテム運搬検知: {other.name} が入りました (カウント対象: {currentCount}/{requiredCount})");
        }
        else
        {
            Debug.Log($"アイテム運搬検知: {other.name} が入りました (ウイルスアイテムのためカウント除外)");
        }
        
        // ウイルスアイテムが検知された場合の処理
        if (isVirusItem)
        {
            if (useVirusTimer)
            {
                Debug.Log("ウイルスアイテム検知: タイマーを開始します");
                StartVirusTimer(other);
            }
            else
            {
                Debug.Log("ウイルスアイテム検知: 即座にVirusオブジェクトをアクティブ化します");
                ActivateVirusObjects();
            }
        }
        // 通常のアイテムが必要な数に達したかチェック
        else
        {
            Debug.Log($"カウントチェック: {currentCount}/{requiredCount} (アクティブ: {isMultiTriggerActive})");
            if (currentCount >= requiredCount)
            {
                if (!isMultiTriggerActive)
                {
                    Debug.Log("条件達成: ActivateItemCarryTrigger()を呼び出します");
                    ActivateItemCarryTrigger();
                }
                else
                {
                    Debug.Log("条件達成済み: 既にアクティブ状態です");
                }
            }
            else
            {
                Debug.Log($"条件未達成: あと{requiredCount - currentCount}個必要です");
            }
        }
    }
    
    // 有効なタグかチェック
    private bool IsValidTag(string tag)
    {
        if (requireAllSameTag)
        {
            return tag == targetTag;
        }
        else
        {
            // 基本タグまたは追加タグのいずれか
            if (tag == targetTag) return true;
            foreach (string additionalTag in additionalTags)
            {
                if (tag == additionalTag) return true;
            }
            return false;
        }
    }
    
    // 複数トリガーをアクティブ化
    private void ActivateMultiTrigger()
    {
        isMultiTriggerActive = true;
        
        // 1回のみの設定で、既に発動済みの場合は何もしない
        if (oneTimeOnly && hasTriggered)
        {
            return;
        }
        
        // レイヤー条件をチェック
        GameObject[] objectsToActivate = GetObjectsToActivate();
        
        // 複数接触条件達成音を再生
        if (playMultiTriggerSound && multiTriggerSound != null)
        {
            PlayMultiTriggerSound();
        }
        
        // 通常の音声再生（入った時）
        if (playOnEnter && !hasPlayedAudio)
        {
            PlayAudio();
        }
        
        // 時間差アクティブ化を使用する場合
        if (useDelayedActivation && activateOnEnter)
        {
            StartDelayedActivation(objectsToActivate);
        }
        else if (activateOnEnter)
        {
            // 即座にアクティブ化
            SetObjectsActive(objectsToActivate, true);
            hasTriggered = true;
            Debug.Log($"複数トリガー発動: {gameObject.name} - {objectsToActivate.Length}個のオブジェクトをアクティブにしました");
        }
    }
    
    // ウイルスオブジェクトを即座にアクティブ化
    private void ActivateVirusObjects()
    {
        Debug.Log("=== ActivateVirusObjects() 開始 ===");
        
        // 1回のみの設定で、既に発動済みの場合は何もしない
        if (oneTimeOnly && hasTriggered)
        {
            Debug.Log("1回のみ設定: 既に発動済みのため処理をスキップします");
            return;
        }
        
        Debug.Log($"ウイルスオブジェクトをアクティブ化します (対象オブジェクト数: {virusObjects?.Length ?? 0})");
        
        if (activateOnEnter)
        {
            SetObjectsActive(virusObjects, true);
            hasTriggered = true;
            Debug.Log($"ウイルスオブジェクト発動: {gameObject.name} - {virusObjects?.Length ?? 0}個のオブジェクトをアクティブにしました");
        }
        else
        {
            Debug.Log($"activateOnEnter: false - ウイルスオブジェクトをアクティブ化しません");
        }
        
        // ウイルス検知音を再生
        if (playVirusSound && virusDetectionSound != null)
        {
            PlayVirusSound();
        }
        
        // 通常の音声再生（入った時）
        if (playOnEnter && !hasPlayedAudio)
        {
            Debug.Log("音声再生を実行します");
            PlayAudio();
        }
        else
        {
            Debug.Log($"音声再生スキップ: playOnEnter={playOnEnter}, hasPlayedAudio={hasPlayedAudio}");
        }
        
        Debug.Log("=== ActivateVirusObjects() 終了 ===");
    }
    
    // アイテム運搬トリガーをアクティブ化（通常アイテム用）
    private void ActivateItemCarryTrigger()
    {
        Debug.Log("=== ActivateItemCarryTrigger() 開始 ===");
        isMultiTriggerActive = true;
        
        // 1回のみの設定で、既に発動済みの場合は何もしない
        if (oneTimeOnly && hasTriggered)
        {
            Debug.Log("1回のみ設定: 既に発動済みのため処理をスキップします");
            return;
        }
        
        // アイテム運搬システム使用時はnormalObjectsを優先、なければtargetObjectsを使用
        GameObject[] objectsToActivate = (normalObjects != null && normalObjects.Length > 0) ? normalObjects : targetObjects;
        
        Debug.Log($"通常アイテム: 正常なオブジェクトをアクティブにします (対象オブジェクト数: {objectsToActivate?.Length ?? 0})");
        
        // 複数接触条件達成音を再生
        if (playMultiTriggerSound && multiTriggerSound != null)
        {
            PlayMultiTriggerSound();
        }
        
        // 通常の音声再生（入った時）
        if (playOnEnter && !hasPlayedAudio)
        {
            Debug.Log("音声再生を実行します");
            PlayAudio();
        }
        else
        {
            Debug.Log($"音声再生スキップ: playOnEnter={playOnEnter}, hasPlayedAudio={hasPlayedAudio}");
        }
        
        // 時間差アクティブ化を使用する場合
        if (useDelayedActivation && activateOnEnter)
        {
            StartDelayedActivation(objectsToActivate);
        }
        else if (activateOnEnter)
        {
            // 即座にアクティブ化
            SetObjectsActive(objectsToActivate, true);
            hasTriggered = true;
            Debug.Log($"アイテム運搬トリガー発動: {gameObject.name} - {objectsToActivate?.Length ?? 0}個のオブジェクトをアクティブにしました");
        }
        else
        {
            Debug.Log($"activateOnEnter: false - オブジェクトをアクティブ化しません");
        }
        
        Debug.Log("=== ActivateItemCarryTrigger() 終了 ===");
    }
    
    // ウイルスレイヤーのアイテムが含まれているかチェック
    private bool CheckForVirusItems()
    {
        Debug.Log($"ウイルスチェック開始: activeColliders数={activeColliders.Count}, virusLayer={virusLayer.value}");
        
        foreach (Collider2D collider in activeColliders)
        {
            if (collider != null)
            {
                Debug.Log($"チェック中: {collider.name} (レイヤー: {collider.gameObject.layer})");
                // レイヤーマスクでウイルスレイヤーかチェック
                if (IsInLayer(collider.gameObject, virusLayer))
                {
                    Debug.Log($"ウイルスアイテム検出: {collider.name} (レイヤー: {collider.gameObject.layer})");
                    return true;
                }
            }
        }
        Debug.Log("ウイルスアイテムは検出されませんでした");
        return false;
    }
    
    // 有効なアイテム（カウント対象）かチェック
    private bool IsValidItem(Collider2D collider)
    {
        if (collider == null) return false;
        
        // Grabbableタグを持っているかチェック
        if (!collider.CompareTag(grabbableTag)) return false;
        
        // ウイルスレイヤーをカウントから除外する場合
        if (excludeVirusFromCount && IsInLayer(collider.gameObject, virusLayer))
        {
            return false;
        }
        
        return true;
    }
    
    // オブジェクトが指定されたレイヤーにあるかチェック
    private bool IsInLayer(GameObject obj, LayerMask layerMask)
    {
        return (layerMask.value & (1 << obj.layer)) != 0;
    }
    
    // アクティブにするオブジェクトを決定
    private GameObject[] GetObjectsToActivate()
    {
        if (useLayerCondition && useLayerObjects)
        {
            // レイヤー条件時に別オブジェクトを使用
            return layerTargetObjects;
        }
        else
        {
            // 通常のオブジェクトを使用
            return targetObjects;
        }
    }

    // トリガーから出た時の処理
    void OnTriggerExit2D(Collider2D other)
    {
        // 複数接触検知を使用する場合
        if (useMultiTrigger)
        {
            HandleMultiTriggerExit(other);
        }
        else
        {
            // 従来の単一接触処理
            HandleSingleTriggerExit(other);
        }
    }
    
    // 単一接触処理（出た時）
    private void HandleSingleTriggerExit(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            isPlayerInTrigger = false;
            
            // タイマートリガーの場合はタイマーをリセット
            if (useTimerTrigger)
            {
                ResetTimer();
            }
            
            if (deactivateOnExit)
            {
                SetObjectsActive(false);
                Debug.Log($"トリガー終了: {gameObject.name} - オブジェクトを非アクティブにしました");
            }
            
            // 音声再生（出た時）
            if (playOnExit)
            {
                PlayAudio();
            }
        }
    }
    
    // 複数接触処理（出た時）
    private void HandleMultiTriggerExit(Collider2D other)
    {
        // アイテム運搬システムを使用する場合
        if (useItemCarrySystem)
        {
            HandleItemCarryExit(other);
            return;
        }
        
        // タグのチェック
        if (!IsValidTag(other.tag))
        {
            return;
        }
        
        // 接触リストから削除
        if (activeColliders.Contains(other))
        {
            activeColliders.Remove(other);
            currentCount = activeColliders.Count;
            
            Debug.Log($"複数接触検知: {other.name} が出ました (現在の数: {currentCount}/{requiredCount})");
            
            // 必要な数より少なくなった場合
            if (currentCount < requiredCount)
            {
                if (isMultiTriggerActive)
                {
                    DeactivateMultiTrigger();
                }
            }
        }
    }
    
    // アイテム運搬処理（出た時）
    private void HandleItemCarryExit(Collider2D other)
    {
        // Grabbableタグのチェック
        if (!other.CompareTag(grabbableTag))
        {
            return;
        }
        
        // 接触リストから削除
        if (activeColliders.Contains(other))
        {
            activeColliders.Remove(other);
            
            // ウイルスアイテムの場合はタイマーをリセット
            if (useVirusTimer && IsInLayer(other.gameObject, virusLayer) && other == currentVirusCollider)
            {
                Debug.Log("ウイルスアイテムが出ました: タイマーをリセットします");
                ResetVirusTimer();
            }
            
            // カウント対象のリストからも削除
            if (validColliders.Contains(other))
            {
                validColliders.Remove(other);
                currentCount = validColliders.Count;
                
                Debug.Log($"アイテム運搬検知: {other.name} が出ました (カウント対象: {currentCount}/{requiredCount})");
            }
            else
            {
                Debug.Log($"アイテム運搬検知: {other.name} が出ました (ウイルスアイテムのためカウント対象外)");
            }
            
            // 必要な数より少なくなった場合
            if (currentCount < requiredCount)
            {
                if (isMultiTriggerActive)
                {
                    DeactivateItemCarryTrigger();
                }
            }
        }
    }
    
    // 複数トリガーを非アクティブ化
    private void DeactivateMultiTrigger()
    {
        isMultiTriggerActive = false;
        
        if (deactivateOnExit)
        {
            GameObject[] objectsToDeactivate = GetObjectsToActivate();
            SetObjectsActive(objectsToDeactivate, false);
            Debug.Log($"複数トリガー終了: {gameObject.name} - オブジェクトを非アクティブにしました");
        }
        
        // 音声再生（出た時）
        if (playOnExit)
        {
            PlayAudio();
        }
    }
    
    // アイテム運搬トリガーを非アクティブ化
    private void DeactivateItemCarryTrigger()
    {
        isMultiTriggerActive = false;
        
        if (deactivateOnExit)
        {
            // ウイルスオブジェクトと通常オブジェクトの両方を非アクティブ化
            SetObjectsActive(virusObjects, false);
            SetObjectsActive(normalObjects, false);
            Debug.Log($"アイテム運搬トリガー終了: {gameObject.name} - オブジェクトを非アクティブにしました");
        }
        
        // 音声再生（出た時）
        if (playOnExit)
        {
            PlayAudio();
        }
    }

    // オブジェクトのアクティブ状態を設定
    private void SetObjectsActive(bool active)
    {
        SetObjectsActive(targetObjects, active);
    }
    
    // 指定されたオブジェクト配列のアクティブ状態を設定
    private void SetObjectsActive(GameObject[] objects, bool active)
    {
        if (objects != null)
        {
            foreach (GameObject obj in objects)
            {
                if (obj != null)
                {
                    obj.SetActive(active);
                }
            }
        }
    }

    // AudioSourceの設定
    private void SetupAudioSource()
    {
        if (audioSource == null && audioClip != null)
        {
            // AudioSourceが設定されていない場合は、このオブジェクトに追加
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        if (audioSource != null)
        {
            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.loop = loop;
        }
    }
    
    // 音声を再生
    private void PlayAudio()
    {
        if (audioSource != null && audioClip != null)
        {
            audioSource.Play();
            hasPlayedAudio = true;
            Debug.Log($"音声再生: {gameObject.name} - {audioClip.name}");
        }
        else
        {
            Debug.LogWarning($"音声再生失敗: {gameObject.name} - AudioSourceまたはAudioClipが設定されていません");
        }
    }

    // ウイルス検知音を再生
    private void PlayVirusSound()
    {
        if (audioSource != null && virusDetectionSound != null)
        {
            audioSource.PlayOneShot(virusDetectionSound, virusSoundVolume);
            Debug.Log($"ウイルス検知音再生: {gameObject.name} - {virusDetectionSound.name}");
        }
        else
        {
            Debug.LogWarning($"ウイルス検知音再生失敗: {gameObject.name} - AudioSourceまたはVirusDetectionSoundが設定されていません");
        }
    }

    // 複数接触条件達成音を再生
    private void PlayMultiTriggerSound()
    {
        if (audioSource != null && multiTriggerSound != null)
        {
            audioSource.PlayOneShot(multiTriggerSound, multiTriggerSoundVolume);
            Debug.Log($"複数接触音再生: {gameObject.name} - {multiTriggerSound.name}");
        }
        else
        {
            Debug.LogWarning($"複数接触音再生失敗: {gameObject.name} - AudioSourceまたはMultiTriggerSoundが設定されていません");
        }
    }

    // デバッグ用：Inspectorでボタンから手動でリセット
    [ContextMenu("Reset Trigger")]
    public void ResetTrigger()
    {
        hasTriggered = false;
        isPlayerInTrigger = false;
        hasPlayedAudio = false;
        SetObjectsActive(false);
        
        // 複数接触検知もリセット
        if (useMultiTrigger || useItemCarrySystem)
        {
            activeColliders.Clear();
            validColliders.Clear();
            currentCount = 0;
            isMultiTriggerActive = false;
        }
        
        // タイマーもリセット
        ResetTimer();
        ResetVirusTimer();
        ResetDelayedActivation();
        
        // 音声を停止
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        Debug.Log($"トリガーリセット: {gameObject.name}");
    }

    // デバッグ用：Inspectorでボタンから手動でアクティブ化
    [ContextMenu("Activate Objects")]
    public void ManualActivate()
    {
        SetObjectsActive(true);
        hasTriggered = true;
        Debug.Log($"手動アクティブ化: {gameObject.name}");
    }
    
    // デバッグ用：Inspectorでボタンから手動で音声再生
    [ContextMenu("Play Audio")]
    public void ManualPlayAudio()
    {
        PlayAudio();
    }

    // デバッグ用：Inspectorでボタンから手動でウイルス検知音を再生
    [ContextMenu("Play Virus Sound")]
    public void ManualPlayVirusSound()
    {
        PlayVirusSound();
    }

    // デバッグ用：Inspectorでボタンから手動で複数接触音を再生
    [ContextMenu("Play Multi Trigger Sound")]
    public void ManualPlayMultiTriggerSound()
    {
        PlayMultiTriggerSound();
    }

    // 時間差アクティブ化を開始
    private void StartDelayedActivation(GameObject[] objectsToActivate)
    {
        if (delayedActivationCoroutine != null)
        {
            StopCoroutine(delayedActivationCoroutine);
        }
        
        pendingActivationObjects = objectsToActivate;
        isDelayedActivationRunning = true;
        currentDelayedTimer = 0f;
        delayedActivationProgress = 0f;
        
        delayedActivationCoroutine = StartCoroutine(DelayedActivationCoroutine());
        
        Debug.Log($"時間差アクティブ化開始: {activationDelay}秒後に{objectsToActivate?.Length ?? 0}個のオブジェクトをアクティブ化します");
    }

    // 時間差アクティブ化コルーチン
    private IEnumerator DelayedActivationCoroutine()
    {
        while (currentDelayedTimer < activationDelay)
        {
            currentDelayedTimer += Time.deltaTime;
            delayedActivationProgress = currentDelayedTimer / activationDelay;
            
            if (showDelayProgress)
            {
                Debug.Log($"時間差アクティブ化進行中: {delayedActivationProgress:F2} ({currentDelayedTimer:F2}/{activationDelay:F2}秒)");
            }
            
            yield return null;
        }
        
        // 時間差アクティブ化完了
        CompleteDelayedActivation();
    }

    // 時間差アクティブ化を更新
    private void UpdateDelayedActivation()
    {
        if (isDelayedActivationRunning)
        {
            currentDelayedTimer += Time.deltaTime;
            delayedActivationProgress = currentDelayedTimer / activationDelay;
            
            if (currentDelayedTimer >= activationDelay)
            {
                CompleteDelayedActivation();
            }
        }
    }

    // 時間差アクティブ化を完了
    private void CompleteDelayedActivation()
    {
        isDelayedActivationRunning = false;
        
        if (pendingActivationObjects != null)
        {
            SetObjectsActive(pendingActivationObjects, true);
            hasTriggered = true;
            Debug.Log($"時間差アクティブ化完了: {gameObject.name} - {pendingActivationObjects.Length}個のオブジェクトをアクティブにしました");
        }
        
        pendingActivationObjects = null;
        delayedActivationCoroutine = null;
    }

    // 時間差アクティブ化をリセット
    private void ResetDelayedActivation()
    {
        if (delayedActivationCoroutine != null)
        {
            StopCoroutine(delayedActivationCoroutine);
            delayedActivationCoroutine = null;
        }
        
        isDelayedActivationRunning = false;
        currentDelayedTimer = 0f;
        delayedActivationProgress = 0f;
        pendingActivationObjects = null;
    }

    // 時間差アクティブ化の進行状況を取得（外部からアクセス可能）
    public float GetDelayedActivationProgress()
    {
        return delayedActivationProgress;
    }

    // 時間差アクティブ化が動作中かどうかを取得（外部からアクセス可能）
    public bool IsDelayedActivationRunning()
    {
        return isDelayedActivationRunning;
    }

    // 時間差アクティブ化の残り時間を取得（外部からアクセス可能）
    public float GetDelayedActivationRemainingTime()
    {
        if (isDelayedActivationRunning)
        {
            return Mathf.Max(0f, activationDelay - currentDelayedTimer);
        }
        return 0f;
    }
    
    // デバッグ用：Inspectorでボタンから複数接触状態を表示
    [ContextMenu("Show Multi Trigger Status")]
    public void ShowMultiTriggerStatus()
    {
        if (useMultiTrigger || useItemCarrySystem)
        {
            Debug.Log($"複数接触状態: {currentCount}/{requiredCount} (アクティブ: {isMultiTriggerActive})");
            Debug.Log($"接触中のオブジェクト: {activeColliders.Count}個 (カウント対象: {validColliders.Count}個)");
            
            Debug.Log("=== 全接触オブジェクト ===");
            foreach (var collider in activeColliders)
            {
                if (collider != null)
                {
                    bool isVirus = useItemCarrySystem && IsInLayer(collider.gameObject, virusLayer);
                    bool isCounted = validColliders.Contains(collider);
                    string status = isVirus ? " (ウイルス・カウント除外)" : (isCounted ? " (カウント対象)" : " (その他)");
                    Debug.Log($"- {collider.name} (タグ: {collider.tag}, レイヤー: {collider.gameObject.layer}){status}");
                }
            }
            
            if (useItemCarrySystem)
            {
                bool hasVirus = CheckForVirusItems();
                Debug.Log($"ウイルスアイテム検知: {hasVirus}");
                Debug.Log($"ウイルスカウント除外: {excludeVirusFromCount}");
            }
        }
        else
        {
            Debug.Log("複数接触検知は無効です");
        }
    }
    
    // 外部からアクセス可能なプロパティ
    public bool IsMultiTriggerActive => isMultiTriggerActive;
    public int CurrentCount => currentCount;
    public int RequiredCount => requiredCount;
    public int ActiveColliderCount => activeColliders.Count;
    
    // タイマーを開始
    private void StartTimer()
    {
        if (!isTimerRunning)
        {
            isTimerRunning = true;
            currentTimer = 0f;
            timerProgress = 0f;
            Debug.Log($"タイマー開始: {gameObject.name} - {triggerDuration}秒後に発動");
        }
    }
    
    // タイマーを更新
    private void UpdateTimer()
    {
        if (isTimerRunning)
        {
            currentTimer += Time.deltaTime;
            timerProgress = currentTimer / triggerDuration;
            
            // タイマーが完了した場合
            if (currentTimer >= triggerDuration)
            {
                CompleteTimer();
            }
        }
    }
    
    // タイマーをリセット
    private void ResetTimer()
    {
        if (isTimerRunning)
        {
            isTimerRunning = false;
            currentTimer = 0f;
            timerProgress = 0f;
            Debug.Log($"タイマーリセット: {gameObject.name}");
        }
    }
    
    // タイマー完了時の処理
    private void CompleteTimer()
    {
        isTimerRunning = false;
        hasTriggered = true;
        
        if (activateOnEnter)
        {
            SetObjectsActive(true);
            Debug.Log($"タイマートリガー発動: {gameObject.name} - {triggerDuration}秒後にオブジェクトをアクティブにしました");
        }
        
        // 音声再生（タイマー完了時）
        if (playOnEnter && !hasPlayedAudio)
        {
            PlayAudio();
        }
    }
    
    // タイマーの進行状況を取得（外部からアクセス可能）
    public float GetTimerProgress()
    {
        return timerProgress;
    }
    
    // タイマーが動作中かどうかを取得（外部からアクセス可能）
    public bool IsTimerRunning()
    {
        return isTimerRunning;
    }
    
    // タイマーの残り時間を取得（外部からアクセス可能）
    public float GetRemainingTime()
    {
        if (isTimerRunning)
        {
            return Mathf.Max(0f, triggerDuration - currentTimer);
        }
        return 0f;
    }
    
    // ウイルスタイマーを開始
    private void StartVirusTimer(Collider2D virusCollider)
    {
        if (!isVirusTimerRunning)
        {
            isVirusTimerRunning = true;
            currentVirusTimer = 0f;
            virusTimerProgress = 0f;
            currentVirusCollider = virusCollider;
            Debug.Log($"ウイルスタイマー開始: {virusCollider.name} - {virusDetectionTime}秒後に発動");
        }
    }
    
    // ウイルスタイマーを更新
    private void UpdateVirusTimer()
    {
        if (isVirusTimerRunning && currentVirusCollider != null)
        {
            currentVirusTimer += Time.deltaTime;
            virusTimerProgress = currentVirusTimer / virusDetectionTime;
            
            // タイマーが完了した場合
            if (currentVirusTimer >= virusDetectionTime)
            {
                CompleteVirusTimer();
            }
        }
    }
    
    // ウイルスタイマーをリセット
    private void ResetVirusTimer()
    {
        if (isVirusTimerRunning)
        {
            isVirusTimerRunning = false;
            currentVirusTimer = 0f;
            virusTimerProgress = 0f;
            currentVirusCollider = null;
            Debug.Log("ウイルスタイマーリセット");
        }
    }
    
    // ウイルスタイマー完了時の処理
    private void CompleteVirusTimer()
    {
        isVirusTimerRunning = false;
        Debug.Log($"ウイルスタイマー完了: {currentVirusCollider?.name} - {virusDetectionTime}秒後にVirusオブジェクトをアクティブ化します");
        
        ActivateVirusObjects();
        
        currentVirusCollider = null;
    }
    
    // ウイルスタイマーの進行状況を取得（外部からアクセス可能）
    public float GetVirusTimerProgress()
    {
        return virusTimerProgress;
    }
    
    // ウイルスタイマーが動作中かどうかを取得（外部からアクセス可能）
    public bool IsVirusTimerRunning()
    {
        return isVirusTimerRunning;
    }
    
    // ウイルスタイマーの残り時間を取得（外部からアクセス可能）
    public float GetVirusTimerRemainingTime()
    {
        if (isVirusTimerRunning)
        {
            return Mathf.Max(0f, virusDetectionTime - currentVirusTimer);
        }
        return 0f;
    }
}
