# LIICON(LI_Script_CostumeChanger.png) LI CostumeChanger

此组件主要用于切换衣装。

## 功能描述

由Int类型控制。注册的衣装在构建时会被转换为AnimationClip和AnimatorController的State。State会根据参数值进行转换，从而播放注册衣装的动画。

构建时具体会执行以下处理：

- 创建AnimationClip，其中包含每个衣装的设置值和预制件（prefab）的初始值。
- 为了防止同步错误，将对象的开关状态与组件设置保持一致。
- 使用预制件的初始值填充每个衣装的未设置值。
- 在AnimatorController和ExpressionParameters中添加名为“菜单·参数名”的Int参数。
- 将“保存有效状态”和“仅限本地”的设置复制到ExpressionParameters中。
- 在AnimatorController中添加一个图层，并注册State、AnimationClip和Transition。
- 生成一个使用Toggle来设置Int值的菜单。

## 设置项

### 菜单设置

#include "docs/zh/docs/components/_menu_settings_table.md"

### 衣装（可多选）

#include "docs/zh/docs/components/_menu_folder_settings_table.md"

#include "docs/zh/docs/components/_additional_settings_table.md"

### 详细设置

|名称|说明|
|-|-|
|默认状态的参数值|可以指定所创建菜单使用的参数初始值（Int值）。|