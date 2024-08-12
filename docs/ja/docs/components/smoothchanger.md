# LIICON(LI_Script_SmoothChanger.png) LI SmoothChanger

BlendShapeなど無段階で制御するものに使用することを想定したコンポーネントです。

## 仕様

Float型で制御しています。登録したフレームはビルド時にAnimationClipとAnimatorControllerのBlendTreeに変換されます。パラメーターの値に応じて登録したフレームのアニメーションがブレンドされる仕組みになっています。

ビルド時には具体的に以下の処理が行われます。

- 各フレームの設定値とprefab初期値を取得したAnimationClipを作成
- 同期事故防止のためにオブジェクトのオンオフ状況をコンポーネントの設定に合わせる
- 各フレームの未設定値をprefab初期値で埋める
- AnimatorControllerとExpressionParametersに`メニュー・パラメーター名`に設定した名前のFloatパラメーターを追加
- ExpressionParametersに`有効状態を保存`と`ローカルのみにする`設定がコピーされる
- AnimatorControllerにレイヤーを追加し、State・BlendTree・AnimationClipを登録
- RadialPuppetでFloat値を制御するメニューを生成

## 設定項目

### メニュー設定

#include "docs/ja/docs/components/_menu_settings_table.md"

### アニメーション設定

|名前|説明|
|-|-|
|パペット初期値(%)|作成されるメニューで使用するパラメーターの初期値を指定することができます。|

#### フレーム (複数指定可)

|名前|説明|
|-|-|
|パペット設定値(%)|フレームに割り当てるパラメーターの値を指定します。|

#include "docs/ja/docs/components/_additional_settings_table.md"
