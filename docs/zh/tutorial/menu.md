# 菜单整理

在这里，我们将介绍如何整理创建的菜单。

## 方法

1. 在Hierarchy中右键点击，选择`Create Empty`在Avatar中创建一个新对象。
2. 在该对象上添加`LI MenuFolder`组件。
3. 将创建的菜单放入此对象的子级下（如果无法移动，请在菜单覆写中指定此对象）。

<video controls="controls" src="/images/zh/tutorial/menu.webm" />

## 使用Modular Avatar管理

当将此工具的组件附加到对象并将其放入`MA Menu Group`中时，会自动生成相应的`MA Menu Item`。如果无法移动对象，请在`MA Menu Group`中创建一个新的`MA Menu Item`，然后在无法移动的对象的`菜单覆盖（Modular Avatar）`中设置新创建的`MA Menu Item`。