# LIICON(LI_Script_ItemToggler.png) LI ItemToggler

小物を切り替える際に使用することを想定したコンポーネントです。

## 仕様

Bool型で制御しています。設定内容はビルド時にAnimationClipとAnimatorControllerのStateに変換されます。パラメーターの値に応じてStateが遷移し、アニメーションが再生される仕組みになっています。

通常、このコンポーネントがアタッチされたオブジェクト自身の切り替えは行いません、コンポーネントのアタッチされたオブジェクト自身の切り替えを行う場合、[LI Prop](prop)を用いるのが便利です。

ビルド時には具体的に以下の処理が行われます。

- コンポーネントの設定値とprefab初期値を取得したAnimationClipをそれぞれ作成
- 同期事故防止のためにオブジェクトのオンオフ状況をコンポーネントの設定に合わせる
- AnimatorControllerとExpressionParametersに`メニュー・パラメーター名`に設定した名前のBoolパラメーターを追加
- ExpressionParametersに`有効状態を保存`と`ローカルのみにする`設定がコピーされる
- AnimatorControllerにレイヤーを追加し、State・AnimationClip・Transitionを登録
- ToggleでBool値を設定するメニューを生成

## 設定項目

### メニュー設定

#include "docs/ja/docs/components/_menu_settings_table.md"

### アニメーション設定

#include "docs/ja/docs/components/_additional_settings_table.md"

### 詳細設定

|名前|説明|
|-|-|
|デフォルト状態のパラメーターの値|作成されるメニューで使用するパラメーターの初期値(Bool値)を指定することができます。|
