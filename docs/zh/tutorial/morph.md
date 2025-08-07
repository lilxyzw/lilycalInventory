# アバターの体型調整

ここではアバターに体型調整メニューを追加する方法を紹介しています。体型以外にも他のBlendShapeの操作などいろいろ応用可能です。

## やり方

1. Hierarchyで右クリックし`Create Empty`でアバター内に新しいオブジェクトを作成
2. そのオブジェクトに`LI SmoothChanger`コンポーネントを追加
3. フレームの+ボタンを押し、フレーム値を0に設定し`BlendShapeの切り替え`に操作するメッシュとBlendShapeを指定（メッシュが未指定であれば全メッシュを操作）
4. フレームの+ボタンを押し、フレーム値を1に設定し3の手順同様に設定

<video controls="controls" src="/images/ja/tutorial/morph.webm" />