# LIICON(LI_Script_AutoFixMeshSettings.png) LI AutoFixMeshSettings

此组件旨在统一Avatar中所有网格（Renderer）的设置。

## 功能描述

在构建时，它会扫描Avatar中的所有Renderer（Mesh Renderer、Skinned Mesh Renderer、Particle System Renderer），并使其设置与此组件保持一致。`Update When Offscreen`、`Skinned Motion Vectors`、`Root Bone`和`Bounds`只对Skinned Mesh Renderer进行修改。

## 设置项

|名称|说明|
|-|-|
|排除的渲染器（可多选）|可以指定不包含在统一设置目标中的渲染器。|
|网格设置（高级）|可以自定义统一的设置内容。通常无需修改此处。此项目的内容与Unity的标准Renderer保持一致。|