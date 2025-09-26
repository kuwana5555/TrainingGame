using UnityEngine;
using System.Collections;

public class CameraProjection : MonoBehaviour
{
    [Header("カメラ設定")]
    [SerializeField] private string cameraName = ""; // 使用するカメラ名（空の場合はデフォルトカメラ）
    [SerializeField] private int cameraWidth = 1280; // カメラ解像度の幅
    [SerializeField] private int cameraHeight = 720; // カメラ解像度の高さ
    [SerializeField] private int cameraFPS = 30; // カメラのフレームレート
    
    [Header("制御設定")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Y; // 切り替えキー
    [SerializeField] private bool startWithCamera = false; // 開始時にカメラを起動するか
    
    [Header("RawImage設定")]
    [SerializeField] private UnityEngine.UI.RawImage targetRawImage; // 投影先のRawImage
    
    [Header("デバッグ設定")]
    [SerializeField] private bool enableDebugLogs = true; // デバッグログを有効にする
    
    [Header("監視カメラ効果")]
    [SerializeField] private bool enableSecurityEffect = true; // 監視カメラ効果を有効にする
    [SerializeField] private float noiseIntensity = 0.1f; // ノイズの強度（0.0-1.0）
    [SerializeField] private float scanlineSpeed = 2.0f; // スキャンラインの速度
    [SerializeField] private bool enableScanlines = true; // スキャンラインを有効にする
    [SerializeField] private bool enableStaticNoise = true; // 静的ノイズを有効にする
    
    private WebCamTexture webCamTexture;
    private bool isCameraActive = false;
    private bool isWebCamTextureReady = false; // WebCamTextureが準備完了かどうか
    private Texture2D processedTexture; // 処理済みテクスチャ
    private float scanlineOffset = 0f; // スキャンラインのオフセット
    
    void Start()
    {
        // 開始時にカメラを起動する場合
        if (startWithCamera)
        {
            StartCamera();
        }
    }
    
    void Update()
    {
        // Yキーが押されたらカメラのオン/オフを切り替え
        if (Input.GetKeyDown(toggleKey))
        {
            if (isCameraActive)
            {
                StopCamera();
            }
            else
            {
                StartCamera();
            }
        }
        
        // 監視カメラ効果の更新
        if (isCameraActive && enableSecurityEffect && targetRawImage != null)
        {
            UpdateSecurityEffect();
        }
    }
    
    /// <summary>
    /// カメラを開始する
    /// </summary>
    public void StartCamera()
    {
        if (isCameraActive)
        {
            Debug.LogWarning("カメラは既に起動しています。");
            return;
        }
        
        // 利用可能なカメラデバイスを取得
        WebCamDevice[] devices = WebCamTexture.devices;
        
        if (devices.Length == 0)
        {
            Debug.LogError("カメラデバイスが見つかりません。");
            return;
        }
        
        // カメラデバイスを選択
        string selectedCamera = "";
        if (string.IsNullOrEmpty(cameraName))
        {
            // デフォルトカメラを使用
            selectedCamera = devices[0].name;
        }
        else
        {
            // 指定された名前のカメラを検索
            foreach (WebCamDevice device in devices)
            {
                if (device.name.Contains(cameraName))
                {
                    selectedCamera = device.name;
                    break;
                }
            }
            
            if (string.IsNullOrEmpty(selectedCamera))
            {
                Debug.LogWarning($"指定されたカメラ '{cameraName}' が見つかりません。デフォルトカメラを使用します。");
                selectedCamera = devices[0].name;
            }
        }
        
        // WebCamTextureを作成
        webCamTexture = new WebCamTexture(selectedCamera, cameraWidth, cameraHeight, cameraFPS);
        
        // カメラを開始
        webCamTexture.Play();
        
        // WebCamTextureの準備を待つ
        StartCoroutine(WaitForWebCamTextureAndApply());
        
        isCameraActive = true;
        if (enableDebugLogs)
        {
            Debug.Log($"カメラを開始しました: {selectedCamera}");
            Debug.Log($"WebCamTexture解像度: {webCamTexture.width}x{webCamTexture.height}");
        }
    }
    
    /// <summary>
    /// カメラを停止する
    /// </summary>
    public void StopCamera()
    {
        if (!isCameraActive)
        {
            Debug.LogWarning("カメラは既に停止しています。");
            return;
        }
        
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
            webCamTexture = null;
        }
        
        // 処理済みテクスチャをクリーンアップ
        if (processedTexture != null)
        {
            DestroyImmediate(processedTexture);
            processedTexture = null;
        }
        
        // RawImageからテクスチャを削除
        if (targetRawImage != null)
        {
            targetRawImage.texture = null;
            
            if (enableDebugLogs)
            {
                Debug.Log("RawImageからテクスチャを削除しました。");
            }
        }
        
        isCameraActive = false;
        isWebCamTextureReady = false;
        
        if (enableDebugLogs)
        {
            Debug.Log("カメラを停止しました。");
        }
    }
    
    /// <summary>
    /// カメラの状態を取得
    /// </summary>
    public bool IsCameraActive()
    {
        return isCameraActive;
    }
    
    /// <summary>
    /// 利用可能なカメラデバイス一覧を取得
    /// </summary>
    public string[] GetAvailableCameras()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        string[] cameraNames = new string[devices.Length];
        
        for (int i = 0; i < devices.Length; i++)
        {
            cameraNames[i] = devices[i].name;
        }
        
        return cameraNames;
    }
    
    /// <summary>
    /// カメラ映像の解像度を変更
    /// </summary>
    public void SetCameraResolution(int width, int height)
    {
        if (isCameraActive)
        {
            Debug.LogWarning("カメラが起動中です。解像度を変更するには一度カメラを停止してください。");
            return;
        }
        
        cameraWidth = width;
        cameraHeight = height;
        Debug.Log($"カメラ解像度を設定しました: {width}x{height}");
    }
    
    /// <summary>
    /// ターゲットRawImageを変更
    /// </summary>
    public void SetTargetRawImage(UnityEngine.UI.RawImage rawImage)
    {
        targetRawImage = rawImage;
        
        // カメラが起動中の場合は新しいRawImageにテクスチャを設定
        if (isCameraActive && webCamTexture != null && targetRawImage != null)
        {
            targetRawImage.texture = webCamTexture;
        }
    }
    
    void OnDestroy()
    {
        // オブジェクトが破棄される際にカメラを停止
        if (isCameraActive)
        {
            StopCamera();
        }
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        // アプリケーションが一時停止された際にカメラを停止
        if (pauseStatus && isCameraActive)
        {
            StopCamera();
        }
    }
    
    /// <summary>
    /// RawImageに直接適用（参考サイトの方法）
    /// </summary>
    private void ApplyToRawImage()
    {
        if (targetRawImage == null)
        {
            Debug.LogError("Target RawImageが設定されていません。");
            return;
        }
        
        if (webCamTexture == null || !isCameraActive || !isWebCamTextureReady)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning("カメラが起動していないか、WebCamTextureが準備できていません。");
            }
            return;
        }
        
        // 監視カメラ効果を適用
        if (enableSecurityEffect)
        {
            ApplySecurityEffect();
        }
        else
        {
            // 参考サイトの方法：直接WebCamTextureを設定
            targetRawImage.texture = webCamTexture;
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"RawImage '{targetRawImage.name}' にカメラ映像を適用しました。");
            Debug.Log($"RawImage有効: {targetRawImage.enabled}");
            Debug.Log($"RawImage表示: {targetRawImage.gameObject.activeInHierarchy}");
            Debug.Log($"RawImageサイズ: {targetRawImage.rectTransform.sizeDelta}");
        }
    }
    
    /// <summary>
    /// RawImageの状態をデバッグ表示
    /// </summary>
    public void DebugRawImageStatus()
    {
        if (targetRawImage == null)
        {
            Debug.LogError("Target RawImageが設定されていません。");
            return;
        }
        
        Debug.Log($"=== RawImage状態デバッグ ===");
        Debug.Log($"名前: {targetRawImage.name}");
        Debug.Log($"有効: {targetRawImage.enabled}");
        Debug.Log($"表示: {targetRawImage.gameObject.activeInHierarchy}");
        Debug.Log($"位置: {targetRawImage.rectTransform.position}");
        Debug.Log($"サイズ: {targetRawImage.rectTransform.sizeDelta}");
        Debug.Log($"色: {targetRawImage.color}");
        Debug.Log($"テクスチャ: {targetRawImage.texture?.name}");
        Debug.Log($"テクスチャ有効: {targetRawImage.texture != null}");
        Debug.Log($"================================");
    }
    
    /// <summary>
    /// WebCamTextureの準備を待ってから適用
    /// </summary>
    private IEnumerator WaitForWebCamTextureAndApply()
    {
        // WebCamTextureが準備できるまで待機
        float timeout = 5f; // 5秒でタイムアウト
        float elapsed = 0f;
        
        while (webCamTexture != null && !webCamTexture.isPlaying && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            // さらに少し待ってからピクセルデータが利用可能になるまで待機
            yield return new WaitForSeconds(0.5f);
            
            isWebCamTextureReady = true;
            
            if (enableDebugLogs)
            {
                Debug.Log("WebCamTextureが準備完了しました。");
            }
            
            // RawImageに直接適用（参考サイトの方法）
            ApplyToRawImage();
        }
        else
        {
            Debug.LogError("WebCamTextureの準備に失敗しました。");
        }
    }
    
    /// <summary>
    /// 現在のWebCamTextureの状態を確認
    /// </summary>
    public void CheckWebCamTextureStatus()
    {
        if (webCamTexture == null)
        {
            Debug.Log("WebCamTextureはnullです。");
            return;
        }
        
        Debug.Log($"WebCamTexture状態:");
        Debug.Log($"  デバイス名: {webCamTexture.deviceName}");
        Debug.Log($"  解像度: {webCamTexture.width}x{webCamTexture.height}");
        Debug.Log($"  フレームレート: {webCamTexture.requestedFPS}");
        Debug.Log($"  再生中: {webCamTexture.isPlaying}");
        Debug.Log($"  ピクセル数: {webCamTexture.GetPixels().Length}");
    }
    
    /// <summary>
    /// 監視カメラ効果の更新
    /// </summary>
    private void UpdateSecurityEffect()
    {
        if (webCamTexture == null || !webCamTexture.isPlaying) return;
        
        // スキャンラインのオフセットを更新
        scanlineOffset += scanlineSpeed * Time.deltaTime;
        if (scanlineOffset > 1f) scanlineOffset = 0f;
        
        // 監視カメラ効果を適用
        ApplySecurityEffect();
    }
    
    /// <summary>
    /// 監視カメラ効果を適用
    /// </summary>
    private void ApplySecurityEffect()
    {
        if (webCamTexture == null || targetRawImage == null) return;
        
        // 処理済みテクスチャを作成または更新
        if (processedTexture == null || 
            processedTexture.width != webCamTexture.width || 
            processedTexture.height != webCamTexture.height)
        {
            if (processedTexture != null)
            {
                DestroyImmediate(processedTexture);
            }
            processedTexture = new Texture2D(webCamTexture.width, webCamTexture.height);
        }
        
        // WebCamTextureからピクセルデータを取得
        Color[] pixels = webCamTexture.GetPixels();
        
        // 監視カメラ効果を適用
        Color[] processedPixels = ApplySecurityEffects(pixels, webCamTexture.width, webCamTexture.height);
        
        // 処理済みテクスチャに設定
        processedTexture.SetPixels(processedPixels);
        processedTexture.Apply();
        
        // RawImageに適用
        targetRawImage.texture = processedTexture;
    }
    
    /// <summary>
    /// 監視カメラ効果をピクセルに適用
    /// </summary>
    private Color[] ApplySecurityEffects(Color[] originalPixels, int width, int height)
    {
        Color[] processedPixels = new Color[originalPixels.Length];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                Color originalColor = originalPixels[index];
                Color processedColor = originalColor;
                
                // 静的ノイズを適用
                if (enableStaticNoise)
                {
                    processedColor = ApplyStaticNoise(processedColor, x, y);
                }
                
                // スキャンラインを適用
                if (enableScanlines)
                {
                    processedColor = ApplyScanlines(processedColor, y);
                }
                
                processedPixels[index] = processedColor;
            }
        }
        
        return processedPixels;
    }
    
    /// <summary>
    /// 静的ノイズを適用
    /// </summary>
    private Color ApplyStaticNoise(Color color, int x, int y)
    {
        // ランダムノイズを生成
        float noise = Mathf.PerlinNoise(x * 0.1f + Time.time, y * 0.1f + Time.time);
        noise = (noise - 0.5f) * 2f; // -1 to 1 の範囲に正規化
        
        // ノイズ強度を適用
        float noiseAmount = noise * noiseIntensity;
        
        // 色にノイズを適用
        color.r = Mathf.Clamp01(color.r + noiseAmount);
        color.g = Mathf.Clamp01(color.g + noiseAmount);
        color.b = Mathf.Clamp01(color.b + noiseAmount);
        
        return color;
    }
    
    /// <summary>
    /// スキャンラインを適用
    /// </summary>
    private Color ApplyScanlines(Color color, int y)
    {
        // スキャンラインの位置を計算
        float scanlinePosition = (y / (float)webCamTexture.height + scanlineOffset) % 1f;
        
        // スキャンラインの強度を計算
        float scanlineIntensity = Mathf.Sin(scanlinePosition * Mathf.PI * 2f) * 0.1f + 0.9f;
        
        // 色にスキャンライン効果を適用
        color.r *= scanlineIntensity;
        color.g *= scanlineIntensity;
        color.b *= scanlineIntensity;
        
        return color;
    }
    
    /// <summary>
    /// 監視カメラ効果の設定を変更
    /// </summary>
    public void SetSecurityEffect(bool enabled, float noise = 0.1f, float scanSpeed = 2.0f)
    {
        enableSecurityEffect = enabled;
        noiseIntensity = Mathf.Clamp01(noise);
        scanlineSpeed = scanSpeed;
        
        if (enableDebugLogs)
        {
            Debug.Log($"監視カメラ効果: {(enabled ? "有効" : "無効")} (ノイズ: {noiseIntensity}, スキャン速度: {scanlineSpeed})");
        }
    }
}