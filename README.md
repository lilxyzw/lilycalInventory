lilAvatarModifier
====

# This project is under development and may be subject to breaking changes
# このプロジェクトは開発中であり破壊的な変更がされる場合があります

## 説明

このツールは非破壊でVRChatのアバターを編集するツールです。オブジェクトのオンオフ、BlendShapeの切り替え、マテリアルの置き換え・プロパティ操作・プロパティ統一などができます。

## Install

現在はUPMからのみインストールできます。

```
https://github.com/lilxyzw/lilAvatarModifier.git
```

もしgitをインストールしていない場合は[zip](https://github.com/lilxyzw/lilAvatarModifier/archive/refs/heads/main.zip)をダウンロードし、プロジェクトのPackagesフォルダ配下に配置することでインストールできます。ディレクトリが以下のようになるように配置してください。

- Packages
  - lilAvatarModifier
    - Editor
    - Prefabs
    - ...

## メニュー生成コンポーネント

|名前|説明|
|-|-|
|lilAM MenuFolder|メニューのフォルダとなるコンポーネントです。各コンポーネントを整理したい場合に使います。パラメーターは生成されません。|
|lilAM Prop|付けたオブジェクトをオンオフできるシンプルなコンポーネントです。|
|lilAM ItemToggler|オブジェクトのオンオフ、BlendShapeの切り替え、マテリアルの置き換え・プロパティ操作を行うコンポーネントです。Boolパラメーターが生成されます。|
|lilAM CostumeChanger|複数衣装の着替えを想定したコンポーネントで、排他的に動作します。各衣装でItemTogglerと同じような操作ができます。Intパラメーターが生成されます。|
|lilAM SmoothChanger|アバターの明るさ調整や体型調整を想定したコンポーネントです。無段階でオブジェクトの操作ができます。Floatパラメーターが生成されます。|

## その他コンポーネント

|名前|説明|
|-|-|
|lilAM MaterialModifier|アバターのライティングの統一などを想定した、マテリアルの設定を指定したマテリアルに統一するコンポーネントです。|
|lilAM MaterialOptimizer|マテリアルから自動的に不要なプロパティを削除するコンポーネントです。|

## 使用方法

### オブジェクトのオンオフ

オンオフしたいオブジェクトに`lilAM Prop`コンポーネントを追加するだけです。

### 複数オブジェクトのオンオフ

1. Hierarchyで右クリックし`Create Empty`でアバター内に新しいオブジェクトを作成
2. そのオブジェクトに`lilAM ItemToggler`コンポーネントを追加
3. `オブジェクトのオンオフ`にオンオフするオブジェクトを指定

### 複数衣装の切り替え（排他）

1. Hierarchyで右クリックし`Create Empty`でアバター内に新しいオブジェクトを作成
2. そのオブジェクトに`lilAM CostumeChanger`コンポーネントを追加
3. コスチュームの+ボタンを押し、`オブジェクトのオンオフ`にオンオフするオブジェクトを指定
4. 衣装ごとに3の手順を繰り返す

### アバターの体型調整

1. Hierarchyで右クリックし`Create Empty`でアバター内に新しいオブジェクトを作成
2. そのオブジェクトに`lilAM SmoothChanger`コンポーネントを追加
3. フレームの+ボタンを押し、フレーム値を0に設定し`BlendShapeの切り替え`に操作するメッシュとBlendShapeを指定（メッシュが未指定であれば全メッシュを操作）
4. フレームの+ボタンを押し、フレーム値を1に設定し3の手順同様に設定

### アバターの明るさ調整

1. Hierarchyで右クリックし`Create Empty`でアバター内に新しいオブジェクトを作成
2. そのオブジェクトに`lilAM SmoothChanger`コンポーネントを追加
3. フレームの+ボタンを押し、フレーム値を0に設定し`マテリアルのプロパティ操作`に操作するメッシュとプロパティを指定（メッシュが未指定であれば全メッシュを操作）
4. フレームの+ボタンを押し、フレーム値を1に設定し3の手順同様に設定

例えばlilToonの場合は`_AsUnlit`、`_LightMinLimit`、`_LightMaxLimit`を指定します。

### 作成したメニューの整理

1. Hierarchyで右クリックし`Create Empty`でアバター内に新しいオブジェクトを作成
2. そのオブジェクトに`lilAM MenuFolder`コンポーネントを追加
3. 作成したメニューをこのオブジェクトの配下に入れる（移動できない場合はオーバーライドでこのオブジェクトを指定する）
