# LIICON(LI_Script_ItemToggler.png) LI ItemToggler

此组件主要用于切换小件物品。

## 功能描述

由Bool类型控制。设置内容在构建时会被转换为AnimationClip和AnimatorController的State。State会根据参数值进行转换，从而播放动画。

通常，此组件不会切换其所附加的对象本身。如果需要切换附加了此组件的对象本身，使用[LI Prop](prop)会更方便。

构建时具体会执行以下处理：

- 分别创建包含组件设置值和预制件（prefab）初始值的AnimationClip。
- 为了防止同步错误，将对象的开关状态与组件设置保持一致。
- 在AnimatorController和ExpressionParameters中添加名为“菜单·参数名”的Bool参数。
- 将“保存有效状态”和“仅限本地”的设置复制到ExpressionParameters中。
- 在AnimatorController中添加一个图层，并注册State、AnimationClip和Transition。
- 生成一个使用Toggle来设置Bool值的菜单。

## 设置项

### 菜单设置

#include "docs/zh/docs/components/_menu_settings_table.md"

### 动画设置

#include "docs/zh/docs/components/_additional_settings_table.md"

### 详细设置

|名称|说明|
|-|-|
|默认状态的参数值|可以指定所创建菜单使用的参数初始值（Bool值）。|