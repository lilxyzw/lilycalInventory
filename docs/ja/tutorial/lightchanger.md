# アバターの明るさ調整

ここでは明るさ調整メニューを追加する方法を紹介しています。

## やり方

lilToonの場合はPrefabsフォルダ内のLightChangerをアバターにドラッグ&ドロップするだけで完了です。

1. Packagesを開く
2. lilycalInventoryフォルダを開く

![Packagesディレクトリ内のlilycalInventoryを選択](/images/ja/tutorial/lightchanger_1.png "Packagesディレクトリ内のlilycalInventoryを選択")

3. Prefabsフォルダを開く

![Prefabsフォルダを開く](/images/ja/tutorial/lightchanger_2.png "Prefabsフォルダを開く")

4. LightChangerをアバターにドラッグ&ドロップ

![LightChangerをアバターにドラッグ&ドロップ](/images/ja/tutorial/lightchanger_3.png "LightChangerをアバターにドラッグ&ドロップ")

## 手動で設定する場合

他シェーダーの場合は手動でプロパティを設定する必要があります。

1. Hierarchyで右クリックし`Create Empty`でアバター内に新しいオブジェクトを作成
2. そのオブジェクトに`LI SmoothChanger`コンポーネントを追加
3. フレームの+ボタンを押し、フレーム値を0に設定し`マテリアルのプロパティ操作`に操作するメッシュとプロパティを指定（メッシュが未指定であれば全メッシュを操作）
4. フレームの+ボタンを押し、フレーム値を1に設定し3の手順同様に設定