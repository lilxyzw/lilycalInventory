# 複数衣装の切り替え

ここではアバターに入れた複数衣装を切り替える方法を紹介しています。

## やり方

<u>各衣装のルートに`LI AutoDresser`コンポーネントを追加し、衣装が1つだけオンの状態にするだけ</u>です！このときオンになっている衣装がデフォルト衣装になります。

::: info
アバター標準衣装などのようにオブジェクトがまとまっていない場合は、衣装の適当な部分に`LI AutoDresser`コンポーネントを追加し、`一緒に操作するパラメーター`の`オブジェクトのオンオフ`に衣装の他部分を追加してください。
:::

<video controls="controls" src="/images/ja/tutorial/costume.webm" />

## 衣装に直接コンポーネントを付けたくない場合

`LI CostumeChanger`で代用できます。こちらのコンポーネントではオンオフする対象を手動で設定してください。