# LIICON(LI_Script_Preset.png) LI Preset

lilycalInventoryのメニュー系コンポーネントを一括で操作し、複数オブジェクトを同時に切り替えることを想定したコンポーネントです。

## 仕様

設定内容はビルド時にStateとParameterDriverに変換されます。Stateの遷移に使用されるパラメーターはすべて非Syncedのため、パラメーターメモリの増加はありません。またAnimatorControllerに追加されるレイヤー数はLI Presetの数にかかわらず1つです（LI Presetが存在しない場合は0）。

ビルド時には具体的に以下の処理が行われます。

- AnimatorControllerとExpressionParametersに`メニュー・パラメーター名`に設定した名前のBoolパラメーター（非Synced）を追加
- AnimatorControllerにレイヤーを追加し、DefaultStateと空のAnimationClipを追加
- LI PresetごとにTransitionとState・空のAnimationClipを追加
- 生成したStateにVRC Avatar Parameter Driverを追加しLI Presetの設定に応じてパラメーター名と値をセット
- ToggleでBool値を設定するメニューを生成

## 設定項目

### メニュー設定

#include "docs/ja/docs/components/_menu_folder_settings_table.md"

### 操作項目

操作対象のコンポーネントとセットする値を指定することができます。AutoDresserの場合は値を入力する必要はなく、セットした衣装に着替えるようになります。
