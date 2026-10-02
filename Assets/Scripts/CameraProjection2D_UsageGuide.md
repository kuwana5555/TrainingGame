# CameraProjection 2D使用ガイド

## 🎮 2Dプロジェクトでの使用方法

### **基本設定**

1. **スクリプトをGameObjectにアタッチ**
2. **2D用設定を確認**:
   - `Is 2D Project`: ✅ チェック
   - `Auto Detect 2D`: ✅ チェック（自動検出）
   - `Flip Texture Vertically`: 通常は❌（必要に応じて）

### **2Dでの使用パターン**

#### **パターン1: SpriteRendererに直接適用**
```csharp
// スクリプト内で
public SpriteRenderer targetSpriteRenderer;

void Start()
{
    // カメラ開始後
    StartCamera();
    ApplyToSpriteRenderer(targetSpriteRenderer);
}
```

#### **パターン2: UI Imageに適用**
```csharp
// スクリプト内で
public UnityEngine.UI.Image targetUIImage;

void Start()
{
    // カメラ開始後
    StartCamera();
    ApplyToUIImage(targetUIImage);
}
```

#### **パターン3: マテリアル経由（従来通り）**
```csharp
// InspectorでTarget Materialを設定
// Yキーでカメラ切り替え
```

### **2D用の推奨設定**

```csharp
// スクリプト内で
void Start()
{
    Apply2DRecommendedSettings();
}
```

### **2D特有の注意点**

1. **カメラ解像度**: 2Dでは通常720pで十分
2. **フレームレート**: 30fpsで軽量化
3. **テクスチャ反転**: 通常は不要（必要に応じて設定）
4. **ピクセル単位**: Sprite作成時は100ピクセル単位を推奨

### **トラブルシューティング**

#### **問題**: カメラ映像が表示されない
- **解決**: `CheckMaterialSetup()` でマテリアル設定を確認
- **解決**: `CheckWebCamTextureStatus()` でカメラ状態を確認

#### **問題**: 2Dで映像が反転している
- **解決**: `Flip Texture Vertically` をチェック

#### **問題**: パフォーマンスが重い
- **解決**: 解像度を下げる（640x480など）
- **解決**: フレームレートを下げる（15fpsなど）

### **2D用の最適化設定例**

```csharp
// 軽量設定
cameraWidth = 640;
cameraHeight = 480;
cameraFPS = 15;

// 高品質設定
cameraWidth = 1280;
cameraHeight = 720;
cameraFPS = 30;
```

## 🎯 2Dでの実用例

### **セキュリティゲームでの使用**
- 監視カメラ画面の表示
- リアルタイム映像の投影
- プレイヤーの顔認識

### **教育ゲームでの使用**
- 鏡のような効果
- 顔認識ゲーム
- インタラクティブな映像表示

このスクリプトは2Dプロジェクトに完全対応しており、自動検出機能により最適な設定が適用されます！





