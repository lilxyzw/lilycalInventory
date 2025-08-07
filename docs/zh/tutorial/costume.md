# 切换多套衣装

在这里，我们将介绍如何切换放入Avatar中的多套衣装。

## 方法

**只需为每套衣装的根部添加`LI AutoDresser`组件，并确保只有一个衣装处于开启状态**！此时处于开启状态的衣装将成为默认衣装。

::: info
如果像Avatar标准衣装那样对象没有被整合在一起，请在衣装的某个适当部分添加`LI AutoDresser`组件，然后在`一起操作的参数`中的`对象的开关`里添加衣装的其他部分。
:::

<video controls="controls" src="/images/zh/tutorial/costume.webm" />

## 如果不想直接在衣装上添加组件

可以使用`LI CostumeChanger`作为替代。此组件需要手动设置要开关的目标。