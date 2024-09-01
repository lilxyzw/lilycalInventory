# LIICON(LI_Script_AutoFixMeshSettings.png) LI AutoFixMeshSettings

アバター内の全メッシュ（Renderer）の設定を統一することを想定したコンポーネントです。

## 仕様

ビルド時にアバター内のRenderer（Mesh Renderer、Skinned Mesh Renderer、Particle System Renderer）をスキャンし、設定内容をこのコンポーネントに合わせます。`Update When Offscreen`、`Skinned Motion Vectors`、`Root Bone`、`Bounds`についてはSkinned Mesh Rendererのみ変更されます。

## 設定項目

|名前|説明|
|-|-|
|除外するレンダラー (複数指定可)|設定統一の対象に含めないレンダラーを指定することができます。|
|メッシュの設定（上級者向け）|設定統一内容をカスタマイズできます。通常はここを変更する必要はありません。この項目の内容はUnity標準のRendererに準拠しています。|
