using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;       //シーンの切り替えに必要
using TMPro;                             //TextMeshProに必要

public class ChangeScene : MonoBehaviour
{
    [Header("シーン設定")]
    public string sceneName;        //読み込むシーン名
    
    [Header("点滅設定")]
    public TextMeshProUGUI blinkingText;       //点滅させるテキスト（TextMeshPro）
    public float blinkSpeed = 1.0f; //点滅速度（秒）
    public float waitTime = 3.0f;   //シーン切り替えまでの待機時間（秒）
    public float fadeOutTime = 1.0f; //フェードアウト時間（秒）
    
    private bool isBlinking = false; //点滅中かどうか
    private bool isFadingOut = false; //フェードアウト中かどうか
    private Coroutine blinkingCoroutine; //点滅コルーチンの参照

    // Start is called before the first frame update
    void Start()
    {
        // 常時点滅を開始
        if (blinkingText != null)
        {
            blinkingCoroutine = StartCoroutine(ContinuousBlinking());
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
        
        // シーンを切り替え
        SceneManager.LoadScene(sceneName);
    }
}
