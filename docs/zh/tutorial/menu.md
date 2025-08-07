# メニューの整理

ここでは作成したメニューを整理する方法を紹介しています。

## やり方

1. Hierarchyで右クリックし`Create Empty`でアバター内に新しいオブジェクトを作成
2. そのオブジェクトに`LI MenuFolder`コンポーネントを追加
3. 作成したメニューをこのオブジェクトの配下に入れる（移動できない場合はオーバーライドでこのオブジェクトを指定する）

<video controls="controls" src="/images/ja/tutorial/menu.webm" />

## Modular Avatarで管理する

このツールのコンポーネントを付けたオブジェクトは`MA Menu Group`の中に入れたときに自動的に対応する`MA Menu Item`が生成されます。もしオブジェクトを移動できない場合は`MA Menu Group`の中に新規の`MA Menu Item`を作成し、移動できないオブジェクトの`オーバーライド (Modular Avatar)`に作成した`MA Menu Item`を設定してください。