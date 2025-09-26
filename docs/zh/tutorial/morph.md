# Avatar体型调整

在这里，我们将介绍如何为Avatar添加体型调整菜单。除了体型，此方法还可以应用于其他BlendShape操作。

## 方法

1. 在Hierarchy中右键点击，选择`Create Empty`在Avatar中创建一个新对象。
2. 在该对象上添加`LI SmoothChanger`组件。
3. 点击帧的`+`按钮，将帧值设置为0，并在`BlendShape的切换`中指定要操作的网格和BlendShape（如果未指定网格，则操作所有网格）。
4. 点击帧的`+`按钮，将帧值设置为1，并按照步骤3进行设置。

<video controls="controls" src="/images/zh/tutorial/morph.webm" />