# LIICON(LI_Script_CostumeChanger.png) LI CostumeChanger

衣装を切り替える際に使用することを想定したコンポーネントです。

## 仕様

Int型で制御しています。登録したコスチュームはビルド時にAnimationClipとAnimatorControllerのStateに変換されます。パラメーターの値に応じてStateが遷移し、登録したコスチュームのアニメーションが再生される仕組みになっています。

ビルド時には具体的に以下の処理が行われます。

- 各衣装の設定値とprefab初期値を取得したAnimationClipを作成
- 同期事故防止のためにオブジェクトのオンオフ状況をコンポーネントの設定に合わせる
- 各衣装の未設定値をprefab初期値で埋める
- AnimatorControllerとExpressionParametersに`メニュー・パラメーター名`に設定した名前のIntパラメーターを追加
- ExpressionParametersに`有効状態を保存`と`ローカルのみにする`設定がコピーされる
- AnimatorControllerにレイヤーを追加し、State・AnimationClip・Transitionを登録
- ToggleでInt値を設定するメニューを生成

## 設定項目

### メニュー設定

#include "docs/ja/docs/components/_menu_settings_table.md"

### コスチューム (複数指定可)

#include "docs/ja/docs/components/_menu_folder_settings_table.md"

#include "docs/ja/docs/components/_additional_settings_table.md"

### 詳細設定

|名前|説明|
|-|-|
|デフォルト状態のパラメーターの値|作成されるメニューで使用するパラメーターの初期値(Int値)を指定することができます。|
