# Avatar亮度调整

在这里，我们将介绍如何添加亮度调整菜单。

## 方法

对于lilToon，只需将Prefabs文件夹中的LightChanger拖放到Avatar中即可。

1. 打开Packages
2. 打开lilycalInventory文件夹

![在Packages目录中选择lilycalInventory](/images/zh/tutorial/lightchanger_1.png "在Packages目录中选择lilycalInventory")

3. 打开Prefabs文件夹

![打开Prefabs文件夹](/images/zh/tutorial/lightchanger_2.png "打开Prefabs文件夹")

4. 将LightChanger拖放到Avatar中

![将LightChanger拖放到Avatar中](/images/zh/tutorial/lightchanger_3.png "将LightChanger拖放到Avatar中")

## 手动设置

对于其他着色器，需要手动设置属性。

1. 在Hierarchy中右键点击，选择`Create Empty`在Avatar中创建一个新对象。
2. 在该对象上添加`LI SmoothChanger`组件。
3. 点击帧的`+`按钮，将帧值设置为0，并在`材质球属性操作`中指定要操作的网格和属性（如果未指定网格，则操作所有网格）。
4. 点击帧的`+`按钮，将帧值设置为1，并按照步骤3进行设置。