# コンポーネント一覧

## メニュー生成コンポーネント

|名前|説明|
|-|-|
|LIICON(LI_Script_MenuFolder.png) LI MenuFolder|メニューのフォルダとなるコンポーネントです。各コンポーネントを整理したい場合に使います。パラメーターは生成されません。|
|LIICON(LI_Script_Prop.png) LI Prop|付けたオブジェクトをオンオフできるシンプルなコンポーネントです。オブジェクトが初期状態でオフの場合はオンになるアニメーションが、初期状態でオンの場合はオフになるアニメーションが生成されます。|
|LIICON(LI_Script_ItemToggler.png) LI ItemToggler|オブジェクトのオンオフ、BlendShapeの切り替え、マテリアルの置き換え・プロパティ操作を行うコンポーネントです。Boolパラメーターが生成されます。|
|LIICON(LI_Script_CostumeChanger.png) LI CostumeChanger|複数衣装の着替えを想定したコンポーネントで、排他的に動作します。各衣装でItemTogglerと同じような操作ができます。Intパラメーターが生成されます。|
|LIICON(LI_Script_SmoothChanger.png) LI SmoothChanger|アバターの明るさ調整や体型調整を想定したコンポーネントです。無段階でオブジェクトの操作ができます。Floatパラメーターが生成されます。|
|LIICON(LI_Script_AutoDresser.png) LI AutoDresser|複数衣装の着替えを想定したコンポーネントです。`LI CostumeChanger`同様に動作しますが、こちらは各衣装のルートにつけるだけで動作します。Intパラメーターが生成されます。|
|LIICON(LI_Script_AutoDresserSettings.png) LI AutoDresserSettings|AutoDresserに関する設定を行うコンポーネントです。現在は生成されるメニューの位置と名前の変更だけ行うことができます。1アバターにつき0または1つだけ設定してください。|

## その他コンポーネント

|名前|説明|
|-|-|
|LIICON(LI_Script_Material.png) LI MaterialModifier|アバターのライティングの統一などを想定した、マテリアルの設定を指定したマテリアルに統一するコンポーネントです。|
|LIICON(LI_Script_MaterialOptimizer.png) LI MaterialOptimizer|マテリアルから自動的に不要なプロパティを削除するコンポーネントです。|
|LIICON(LI_Script_Comment.png) LI Comment|GameObjectにコメントを表示するだけのコンポーネントでこれ自体に機能はありません。prefabの説明を残す用途を想定しています。|
