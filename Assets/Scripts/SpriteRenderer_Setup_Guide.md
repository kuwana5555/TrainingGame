# SpriteRenderer設定ガイド

## 🎮 SpriteRendererの推奨設定

### **必須設定項目**

#### **1. Color（色）**
- **設定値**: `White (1, 1, 1, 1)`
- **理由**: カメラ映像の色を正確に表示するため
- **注意**: 他の色にすると映像が色付きで表示される

#### **2. Sorting Order（描画順序）**
- **設定値**: `0` または適切な値
- **理由**: 他のオブジェクトとの描画順序を制御
- **推奨**: カメラ映像は通常前面に表示

#### **3. Sorting Layer（ソートレイヤー）**
- **設定値**: `Default` または専用レイヤー
- **理由**: レイヤー別の描画順序制御
- **推奨**: カメラ映像用の専用レイヤーを作成

### **推奨設定項目**

#### **4. Material（マテリアル）**
- **設定値**: `Sprites-Default` または2D用マテリアル
- **理由**: 2Dプロジェクトに最適化されたマテリアル
- **注意**: 3D用マテリアルは使用しない

#### **5. Transform（変形）**
- **Scale**: `(1, 1, 1)` - 等倍
- **Position**: 適切な位置
- **Rotation**: `(0, 0, 0)` - 回転なし

## ⚙️ 自動設定機能

### **自動設定の有効化**
```csharp
// Inspectorで設定
Auto Configure Sprite Renderer: ✅ チェック
```

### **手動設定**
```csharp
// スクリプト内で設定
ConfigureSpriteRendererManually(
    spriteRenderer, 
    Color.white,     // 色
    0,              // 描画順序
    "Default"       // ソートレイヤー
);
```

## 🎯 2Dプロジェクト用の最適設定

### **基本設定**
- **Color**: White (1, 1, 1, 1)
- **Sorting Order**: 0
- **Sorting Layer**: Default
- **Material**: Sprites-Default
- **Scale**: (1, 1, 1)

### **カメラ映像専用設定**
- **Color**: White (映像の色を保持)
- **Sorting Order**: 10 (前面表示)
- **Sorting Layer**: Camera (専用レイヤー)
- **Material**: 2D用マテリアル

## 🚨 よくある問題と解決方法

### **問題1: 映像が暗く表示される**
- **原因**: ColorがWhite以外に設定されている
- **解決**: ColorをWhite (1, 1, 1, 1)に設定

### **問題2: 他のオブジェクトの後ろに隠れる**
- **原因**: Sorting Orderが低い
- **解決**: Sorting Orderを上げる（例: 10）

### **問題3: 映像が歪んで表示される**
- **原因**: Scaleが等倍でない
- **解決**: Scaleを(1, 1, 1)に設定

### **問題4: パフォーマンスが重い**
- **原因**: 高解像度のSpriteを使用
- **解決**: Max Sprite Sizeを下げる（例: 256x256）

## 📋 設定チェックリスト

### **必須チェック項目**
- [ ] Color = White (1, 1, 1, 1)
- [ ] Sorting Order = 適切な値
- [ ] Sorting Layer = 設定済み
- [ ] Scale = (1, 1, 1)
- [ ] Material = 2D用

### **推奨チェック項目**
- [ ] Auto Configure Sprite Renderer = 有効
- [ ] Max Sprite Size = 適切な値
- [ ] Preserve Aspect Ratio = 有効
- [ ] Sprite Pixels Per Unit = 100

この設定により、2Dプロジェクトでカメラ映像が最適に表示されます！





