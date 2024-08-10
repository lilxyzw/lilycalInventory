# インストールの方法

インストール方法にはいくつか種類がありますが、以下の4種類のうちどれか1つだけやればOKです。個人的にはVCCでのインストールがオススメです！

[[toc]]

## 【オススメ！】VCCでのインストール

一見すると複雑に見えますが、こちらの手順を1度でも済ませてしまえばVRCSDKと同じようにVCCから1クリックでアップデートできるようになります！

1. [こちらをクリック](vcc://vpm/addRepo?url=https://lilxyzw.github.io/vpm-repos/vpm.json)するとVCCが開かれるので、`I Understand, Add Repository`をクリックしてください。VCCにlilycalInventoryが追加されます。
2. インストールが完了したら`Projects`を押してプロジェクト選択画面に戻りましょう。

![VCCのパッケージインストール画面](/images/ja/tutorial/vcc_packages.png "VCCのパッケージインストール画面")

3. プロジェクト選択画面で`Manage Project`をクリックして管理画面を開きましょう。

![VCCのProjects画面](/images/ja/tutorial/vcc_projects.png "VCCのProjects画面")

4. 最後にlilycalInventoryの右端の`+`ボタンをクリックするとインストール完了です！

![VCCのManage画面](/images/ja/tutorial/vcc_manage.png "VCCのManage画面")

## Unitypackageでのインストール

[こちら](https://github.com/lilxyzw/lilycalInventory/releases)からunitypackageをダウンロードしてください。unitypackageをUnityのウィンドウにドラッグ&ドロップすることでインポートできます。

::: warning
こちらの方法ではアップデートのたびにダウンロードページからダウンロードする必要があります。
:::

![GitHubのダウンロードページ](/images/ja/tutorial/github_unitypackage.png "GitHubのダウンロードページ")

## VPMCLIでのインストール

VPMCLIを使う場合は以下のコマンドでインストールできます。

```
vpm add repo https://lilxyzw.github.io/vpm-repos/vpm.json

cd /path/to/your-unity-project
vpm add package jp.lilxyzw.lilycalinventory
```

## vrc-getでのインストール

vrc-getを使う場合は以下のコマンドでインストールできます。

```
vrc-get repo add https://lilxyzw.github.io/vpm-repos/vpm.json

cd /path/to/your-unity-project
vrc-get install jp.lilxyzw.lilycalinventory
```