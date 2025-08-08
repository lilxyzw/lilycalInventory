# LIICON(LI_Script_Preset.png) LI Preset

此组件旨在批量操作lilycalInventory的菜单系统组件，实现同时切换多个对象。

## 功能描述

设置内容在构建时会被转换为State和ParameterDriver。所有用于State转换的参数都是非同步的（Non-Synced），因此不会增加参数内存。此外，无论存在多少个LI Preset，AnimatorController中添加的图层数量都为1（如果不存在LI Preset则为0）。

构建时具体会执行以下处理：

- 在AnimatorController和ExpressionParameters中添加名为“菜单·参数名”的Bool参数（非同步）。
- 在AnimatorController中添加一个图层，并添加DefaultState和空的AnimationClip。
- 为每个LI Preset添加Transition、State和空的AnimationClip。
- 在生成的State中添加VRC Avatar Parameter Driver，并根据LI Preset的设置来设置参数名和值。
- 生成一个使用Toggle来设置Bool值的菜单。

## 设置项

### 菜单设置

#include "docs/zh/docs/components/_menu_folder_settings_table.md"

### 操作项

可以指定要操作的组件和要设置的值。对于AutoDresser，无需输入值，它将切换到所设置的衣装。