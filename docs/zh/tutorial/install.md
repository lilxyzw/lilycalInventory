# 安装方法

有几种安装方法，但只需选择其中一种即可。个人推荐使用VCC进行安装！

[[toc]]

## 【推荐！】通过VCC安装

这看起来可能很复杂，但只要完成一次这些步骤，以后就可以像VRCSDK一样通过VCC一键更新了！

1. [点击此处](vcc://vpm/addRepo?url=https://lilxyzw.github.io/vpm-repos/vpm.json)，VCC将会打开，然后点击`I Understand, Add Repository`。lilycalInventory将被添加到VCC中。
2. 安装完成后，点击`Projects`返回项目选择界面。

![VCC的软件包安装界面](/images/zh/tutorial/vcc_packages.png "VCC的软件包安装界面")

3. 在项目选择界面，点击`Manage Project`打开管理界面。

![VCC的项目界面](/images/zh/tutorial/vcc_projects.png "VCC的项目界面")

4. 最后，点击lilycalInventory右侧的`+`按钮即可完成安装！

![VCC的管理界面](/images/zh/tutorial/vcc_manage.png "VCC的管理界面")

## 通过Unitypackage安装

请从[此处](https://github.com/lilxyzw/lilycalInventory/releases)下载unitypackage。将其拖放到Unity窗口中即可导入。

::: warning
使用此方法，每次更新都需要从下载页面重新下载。
:::

![GitHub的下载页面](/images/zh/tutorial/github_unitypackage.png "GitHub的下载页面")

## 通过VPMCLI安装

如果使用VPMCLI，可以使用以下命令进行安装：

```
vpm add repo https://lilxyzw.github.io/vpm-repos/vpm.json

cd /path/to/your-unity-project
vpm add package jp.lilxyzw.lilycalinventory
```

## 通过vrc-get安装

如果使用vrc-get，可以使用以下命令进行安装：

```
vrc-get repo add https://lilxyzw.github.io/vpm-repos/vpm.json

cd /path/to/your-unity-project
vrc-get install jp.lilxyzw.lilycalinventory
```