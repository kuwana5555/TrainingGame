using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;       //シーンの切り替えに必要
using TMPro;                             //TextMeshProに必要

public class ChangeScene : MonoBehaviour
{
    [Header("シーン設定")]
    public string sceneName;        //読み込むシーン名
    
    [Header("自動シーン切り替え設定")]
    public bool useAutoSceneChange = false;  // 自動シーン切り替えを使用するか
    public float autoChangeDelay = 5.0f;     // 自動切り替えまでの遅延時間（秒）
    public string autoChangeSceneName = "";  // 自動切り替え先のシーン名（空の場合はsceneNameを使用）
    
    [Header("点滅設定")]
    public TextMeshProUGUI blinkingText;       //点滅させるテキスト（TextMeshPro）
    public float blinkSpeed = 1.0f; //点滅速度（秒）
    public float waitTime = 3.0f;   //シーン切り替えまでの待機時間（秒）
    public float fadeOutTime = 1.0f; //フェードアウト時間（秒）
    
    [Header("音声設定")]
    public AudioSource audioSource;            // 音声を再生するAudioSource
    public AudioClip blinkStartSound;          // 点滅開始時の音声クリップ
    public AudioClip fadeOutSound;             // フェードアウト開始時の音声クリップ
    public float volume = 1.0f;                // 音量（0.0～1.0）
    public bool playBlinkStartSound = true;    // 点滅開始音を再生するか
    public bool playFadeOutSound = true;       // フェードアウト音を再生するか
    
    private bool isBlinking = false; //点滅中かどうか
    private bool isFadingOut = false; //フェードアウト中かどうか
    private Coroutine blinkingCoroutine; //点滅コルーチンの参照
    
    // 自動シーン切り替え関連の変数
    private Coroutine autoChangeCoroutine; //自動切り替えコルーチンの参照
    private bool hasAutoChanged = false; //自動切り替えが実行されたかどうか

    // Start is called before the first frame update
    void Start()
    {
        // AudioSourceの設定
        SetupAudioSource();
        
        // 常時点滅を開始
        if (blinkingText != null)
        {
            blinkingCoroutine = StartCoroutine(ContinuousBlinking());
        }
        
        // 自動シーン切り替えを開始
        if (useAutoSceneChange && !hasAutoChanged)
        {
            autoChangeCoroutine = StartCoroutine(AutoSceneChange());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //シーンを読み込む（フェードアウト付き）
    public void Load()
    {
        if (!isFadingOut)
        {
            StartCoroutine(FadeOutAndLoad());
        }
    }
    
    //常時点滅処理
    private IEnumerator ContinuousBlinking()
    {
        isBlinking = true;
        
        // 点滅開始音を再生
        if (playBlinkStartSound && blinkStartSound != null)
        {
            PlayBlinkStartSound();
        }
        
        while (isBlinking && !isFadingOut)
        {
            // アルファ値をsin波で変化させてゆっくり点滅
            float alpha = (Mathf.Sin(Time.time * Mathf.PI * 2 / blinkSpeed) + 1) / 2;
            Color textColor = blinkingText.color;
            textColor.a = alpha;
            blinkingText.color = textColor;
            
            yield return null;
        }
    }
    
    //フェードアウトしてからシーンを切り替える
    private IEnumerator FadeOutAndLoad()
    {
        isFadingOut = true;
        isBlinking = false; // 常時点滅を停止
        
        // フェードアウト開始音を再生
        if (playFadeOutSound && fadeOutSound != null)
        {
            PlayFadeOutSound();
        }
        
        if (blinkingText != null)
        {
            float elapsedTime = 0f;
            float fadeOutSpeed = blinkSpeed / 1.5f; // 1.5倍の頻度
            
            // フェードアウト処理
            while (elapsedTime < fadeOutTime)
            {
                // 1.5倍の頻度で点滅しながら徐々にフェードアウト
                float blinkAlpha = (Mathf.Sin(elapsedTime * Mathf.PI * 2 / fadeOutSpeed) + 1) / 2;
                float fadeAlpha = 1f - (elapsedTime / fadeOutTime); // 徐々に透明に
                float alpha = blinkAlpha * fadeAlpha; // 点滅とフェードアウトを組み合わせ
                
                Color textColor = blinkingText.color;
                textColor.a = alpha;
                blinkingText.color = textColor;
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 完全に透明にする
            Color finalColor = blinkingText.color;
            finalColor.a = 0f;
            blinkingText.color = finalColor;
        }
        else
        {
            // テキストが設定されていない場合は単純に待機
            yield return new WaitForSeconds(fadeOutTime);
        }
        
        // 遷移コンテキストを設定してからシーンを切り替え
        string current = SceneManager.GetActiveScene().name;
        SceneTransitionContext.Set(current, sceneName);
        SceneManager.LoadScene(sceneName);
    }
    
    // 自動シーン切り替え処理
    private IEnumerator AutoSceneChange()
    {
        Debug.Log($"自動シーン切り替え開始: {autoChangeDelay}秒後に切り替え");
        
        // 指定した秒数待機
        yield return new WaitForSeconds(autoChangeDelay);
        
        // 既に手動で切り替えが実行されていない場合のみ実行
        if (!hasAutoChanged && !isFadingOut)
        {
            hasAutoChanged = true;
            
            // 自動切り替え先のシーン名を決定
            string targetSceneName = string.IsNullOrEmpty(autoChangeSceneName) ? sceneName : autoChangeSceneName;
            
            Debug.Log($"自動シーン切り替え実行: {targetSceneName}に切り替え");
            
            // シーンを切り替え
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                string current = SceneManager.GetActiveScene().name;
                SceneTransitionContext.Set(current, targetSceneName);
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.LogWarning("自動シーン切り替え失敗: シーン名が設定されていません");
            }
        }
    }
    
    // 自動シーン切り替えを手動で停止
    public void StopAutoSceneChange()
    {
        if (autoChangeCoroutine != null)
        {
            StopCoroutine(autoChangeCoroutine);
            autoChangeCoroutine = null;
            Debug.Log("自動シーン切り替えを停止しました");
        }
    }
    
    // 自動シーン切り替えを手動で開始
    public void StartAutoSceneChange()
    {
        if (useAutoSceneChange && !hasAutoChanged && autoChangeCoroutine == null)
        {
            autoChangeCoroutine = StartCoroutine(AutoSceneChange());
            Debug.Log("自動シーン切り替えを開始しました");
        }
    }
    
    // 自動シーン切り替えの残り時間を取得（外部からアクセス可能）
    public float GetAutoChangeRemainingTime()
    {
        if (useAutoSceneChange && !hasAutoChanged && autoChangeCoroutine != null)
        {
            // 簡易的な実装：正確な残り時間の計算は複雑なため、基本的な情報のみ提供
            return autoChangeDelay;
        }
        return 0f;
    }
    
    // AudioSourceの設定
    private void SetupAudioSource()
    {
        if (audioSource == null)
        {
            // AudioSourceが設定されていない場合は、このオブジェクトに追加
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }
    
    // 点滅開始音を再生
    private void PlayBlinkStartSound()
    {
        if (audioSource != null && blinkStartSound != null)
        {
            audioSource.PlayOneShot(blinkStartSound, volume);
            Debug.Log($"点滅開始音を再生: {blinkStartSound.name}");
        }
        else
        {
            Debug.LogWarning($"点滅開始音の再生に失敗: AudioSourceまたはBlinkStartSoundが設定されていません");
        }
    }
    
    // フェードアウト音を再生
    private void PlayFadeOutSound()
    {
        if (audioSource != null && fadeOutSound != null)
        {
            audioSource.PlayOneShot(fadeOutSound, volume);
            Debug.Log($"フェードアウト音を再生: {fadeOutSound.name}");
        }
        else
        {
            Debug.LogWarning($"フェードアウト音の再生に失敗: AudioSourceまたはFadeOutSoundが設定されていません");
        }
    }
    
    // デバッグ用：Inspectorでボタンから手動で点滅開始音を再生
    [ContextMenu("Play Blink Start Sound")]
    public void ManualPlayBlinkStartSound()
    {
        PlayBlinkStartSound();
    }
    
    // デバッグ用：Inspectorでボタンから手動でフェードアウト音を再生
    [ContextMenu("Play Fade Out Sound")]
    public void ManualPlayFadeOutSound()
    {
        PlayFadeOutSound();
    }
}
