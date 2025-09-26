# 组件列表

## 菜单生成组件

|名称|说明|
|-|-|
|LIICON(LI_Script_MenuFolder.png) LI MenuFolder|这是一个用于创建菜单文件夹的组件。用于整理各个组件。不生成参数。|
|LIICON(LI_Script_Prop.png) LI Prop|一个可以开关附加对象的简单组件。如果对象初始为关闭，则会生成一个开启的动画；如果初始为开启，则会生成一个关闭的动画。|
|LIICON(LI_Script_ItemToggler.png) LI ItemToggler|一个可以开关对象、切换BlendShape、替换或操作材质球属性的组件。会生成Bool参数。|
|LIICON(LI_Script_CostumeChanger.png) LI CostumeChanger|此组件用于切换多套衣装，并以排他方式运行。每套衣装都可以进行与ItemToggler相同的操作。会生成Int参数。|
|LIICON(LI_Script_SmoothChanger.png) LI SmoothChanger|此组件用于无级调节Avatar的亮度或体型等。可以无级操作对象。会生成Float参数。|
|LIICON(LI_Script_AutoDresser.png) LI AutoDresser|此组件用于切换多套衣装。与`LI CostumeChanger`功能类似，但只需将其附加到每个衣装的根部即可工作。会生成Int参数。|
|LIICON(LI_Script_AutoDresserSettings.png) LI AutoDresserSettings|此组件用于设置AutoDresser。目前仅支持更改生成的菜单位置和名称。每个Avatar只设置0个或1个此组件。|
|LIICON(LI_Script_Preset.png) LI Preset|此组件旨在批量操作lilycalInventory的菜单系统组件，实现同时切换多个对象。|

## 其他组件

|名称|说明|
|-|-|
|LIICON(LI_Script_Material.png) LI MaterialModifier|此组件用于统一材质球设置，例如统一Avatar的光照接收方式，将材质球设置与指定材质球保持一致。|
|LIICON(LI_Script_MaterialOptimizer.png) LI MaterialOptimizer|此组件会自动移除材质球中不必要的属性，从而进行优化。|
|LIICON(LI_Script_Comment.png) LI Comment|这只是一个在GameObject上显示注释的组件，本身没有功能。主要用于为预制件（prefab）留下说明。|
|LIICON(LI_Script_AutoFixMeshSettings.png) LI AutoFixMeshSettings|此组件旨在统一Avatar中所有网格（Renderer）的设置。|