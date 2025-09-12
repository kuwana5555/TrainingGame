using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerActivator : MonoBehaviour
{
    [Header("トリガー設定")]
    [SerializeField] private string targetTag = "Player";  // 対象のタグ
    [SerializeField] private GameObject[] targetObjects;   // アクティブにするオブジェクト配列
    [SerializeField] private bool activateOnEnter = true;  // 入った時にアクティブにするか
    [SerializeField] private bool deactivateOnExit = false; // 出た時に非アクティブにするか
    
    [Header("音声設定")]
    [SerializeField] private AudioSource audioSource;      // 音声を再生するAudioSource
    [SerializeField] private AudioClip audioClip;          // 再生する音声クリップ
    [SerializeField] private bool playOnEnter = true;      // 入った時に音声を再生するか
    [SerializeField] private bool playOnExit = false;      // 出た時に音声を再生するか
    [SerializeField] private float volume = 1.0f;          // 音量（0.0～1.0）
    [SerializeField] private bool loop = false;            // ループ再生するか
    
    [Header("動作設定")]
    [SerializeField] private bool oneTimeOnly = true;      // 1回のみ機能するか
    [SerializeField] private bool persistentInScene = true; // シーン内で継続するか
    
    private bool hasTriggered = false;  // トリガーが発動したかどうか
    private bool isPlayerInTrigger = false; // プレイヤーがトリガー内にいるか
    private bool hasPlayedAudio = false; // 音声が再生されたかどうか
    
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
        // プレイヤーがトリガー内にいる間は継続してアクティブ状態を維持
        if (isPlayerInTrigger && persistentInScene)
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
        if (other.CompareTag(targetTag))
        {
            isPlayerInTrigger = true;
            
            // 1回のみの設定で、既に発動済みの場合は何もしない
            if (oneTimeOnly && hasTriggered)
            {
                return;
            }
            
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
    }

    // トリガーから出た時の処理
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            isPlayerInTrigger = false;
            
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

    // オブジェクトのアクティブ状態を設定
    private void SetObjectsActive(bool active)
    {
        if (targetObjects != null)
        {
            foreach (GameObject obj in targetObjects)
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

    // デバッグ用：Inspectorでボタンから手動でリセット
    [ContextMenu("Reset Trigger")]
    public void ResetTrigger()
    {
        hasTriggered = false;
        isPlayerInTrigger = false;
        hasPlayedAudio = false;
        SetObjectsActive(false);
        
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
}
