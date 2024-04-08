# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.3.7] - 2023-04-09

### 追加
- UI全般の向上
- パラメーター数の確認機能を追加
- SmoothChangerのプレビューでフレーム間の補間を追加
- AutoDresserのエディタ上に警告を追加
- Altキーを押しながらFoldoutをクリックしたときに一括で開閉できるようにした
- バージョン確認システムを追加
- 変更履歴確認ウィンドウを追加
- AutoDresserSettingsにプロパティの保存とローカルオンリーの設定を追加
- コードにコメントを追加
- AnimationClipの挿入に対応

### 変更
- フレーム値をパーセント表記にするようにした
- ビルド時にアクティブでないオブジェクトについたコンポーネントを除外するように（AutoDresserとPropを除く）
- DirectBlendTreeによる最適化をデフォルトでオンにした

### 修正
- LI Commentの表示バグを修正
- Foldoutの表示バグを修正
- ビルド時に警告が表示されていたのを修正
- プレビュー時にエラーが出ていたのを修正
- BlendShapeのサジェストが正しくないのを修正
- 全てのAutoDresserがオフのときのエラーを修正

## [0.3.6] - 2023-03-09

### 追加
- 一部プロパティで複数オブジェクトをドラッグ＆ドロップで一括追加できるように
- MenuFolder編集時にフォルダの内容を確認・編集できるように

### 変更
- アセットのクローン時にNDMFのObjectRegistryに登録するように
- マテリアル変更処理がTexTransToolの後に動くように
- 過剰にFoldoutがあった部分を改善

### 修正
- MaterialModifierの除外機能が正しく動作していなかったのを修正
- NDMFのバージョンが古い場合にエラーになっていたのを修正

## [0.3.5] - 2023-03-07

### 修正
- 言語変更時にLI Commentの一部プロパティが見えなくなってしまう問題を修正
- マテリアルのベクトル操作の無効化が正しく機能していない問題を修正
- 同一の名前のオブジェクトが同一オブジェクトとして扱われてしまう問題を修正
- ItemTogglerのプロパティが特定条件でAnimatorControllerに登録されない場合がある問題を修正
- パラメーター重複時のエラーレポートに該当オブジェクトが表示されないのを修正
- マテリアルが空のRendererでエラーが出る問題を修正
- SmoothChanger・CostumeChangerのプレビュー時にマテリアルのプロパティ操作のRendererにメッシュを指定している場合に該当メッシュ内のマテリアルの値で設定値が上書きされてしまう問題を修正

### 変更
- MenuFolderコンポーネントと他コンポーネントを同時に付けられるように
- アクティブでないGameObjectのコンポーネントも処理対象にするように戻した

## [0.3.4] - 2023-03-06

### 追加
- コンポーネントがアクティブでない場合にビルド時に無視される旨のヘルプボックスを表示するように
- Prefab・GameObjectにコメントを残せるLI Commentコンポーネントを追加（言語切替・マークダウン対応）

### 変更
- ビルド時にアクティブでないオブジェクトについたコンポーネントを除外するように（AutoDresserとPropを除く）

## [0.3.3] - 2023-03-02

### 追加
- Modular AvatarのMenu Group内にメニューを生成できるように

### 変更
- ビルド時にEditorOnlyのオブジェクトについたコンポーネントを除外するように

## [0.3.2] - 2023-03-02

### 追加
- Direct Blend Treeによる最適化機能を追加

### 変更
- ビルド時の処理を高速化

## [0.3.0] - 2023-02-29

## 追加
- コンポーネントをつけるだけで着替えができるAutoDresserコンポーネントを追加

### 変更
- ツール名をlilAvatarMofifierからlilycalInventoryに変更
- コンポーネントのアイコンを設定
- 複数のコンポーネントからオブジェクトをトグルできるように
