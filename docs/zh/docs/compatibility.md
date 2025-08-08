# 与NDMF及其他工具的兼容性

lilycalInventory可以独立运行，但如果项目中存在NDMF(Non-Destructive Modifier Framework)，它将作为NDMF插件运行。作为NDMF插件运行时，由于其运行顺序已预先设置，因此可以与其他已知的NDMF插件同时使用。

## Modular Avatar

通过为lilycalInventory的组件设置`菜单覆盖(Modular Avatar)`，指定Modular Avatar的菜单项，则菜单将使用Modular Avatar生成，而不是本工具。

## lilToon

支持使用附带的预制件（prefab）来统一距离淡出和光照设置，以及使用LightChanger来生成亮度调整菜单。