# LIICON(LI_Script_SmoothChanger.png) LI SmoothChanger

此组件主要用于控制BlendShape等无级调节的物品。

## 功能描述

由Float类型控制。注册的帧在构建时会被转换为AnimationClip和AnimatorController的BlendTree。动画会根据参数值进行混合，从而播放注册帧的动画。

构建时具体会执行以下处理：

- 创建AnimationClip，其中包含每个帧的设置值和预制件（prefab）的初始值。
- 为了防止同步错误，将对象的开关状态与组件设置保持一致。
- 使用预制件的初始值填充每个帧的未设置值。
- 在AnimatorController和ExpressionParameters中添加名为“菜单·参数名”的Float参数。
- 将“保存有效状态”和“仅限本地”的设置复制到ExpressionParameters中。
- 在AnimatorController中添加一个图层，并注册State、BlendTree和AnimationClip。
- 生成一个使用RadialPuppet来控制Float值的菜单。

## 设置项

### 菜单设置

#include "docs/zh/docs/components/_menu_settings_table.md"

### 动画设置

|名称|说明|
|-|-|
|Puppet初始值(%)|可以指定所创建菜单使用的参数初始值。|

#### 帧（可多选）

|名称|说明|
|-|-|
|Puppet设置值(%)|指定分配给帧的参数值。|

#include "docs/zh/docs/components/_additional_settings_table.md"