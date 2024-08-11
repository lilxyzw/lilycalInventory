# LI MenuFolder

lilycalInventoryの各コンポーネントで生成されるメニューを整理する際に使用することを想定したコンポーネントです。

## 仕様

`LI MenuFolder`の子のオブジェクトから生成されるメニューは全てこのコンポーネントによって生成されるSubMenuの配下になります。子のオブジェクトのコンポーネントで`メニューの親フォルダ`が指定されている場合はそちらが優先されます。

## 設定項目

### メニュー設定

#include "docs/ja/docs/components/_menu_folder_settings_table.md"