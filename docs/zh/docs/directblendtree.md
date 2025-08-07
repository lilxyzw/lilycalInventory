# 使用Direct Blend Tree进行优化

当`Tools/lilycalInventory/Use Direct Blend Tree`开启时，会使用`Direct Blend Tree`将此工具生成的AnimatorController层合并为一个。此功能利用了[ExpressionParameters和AnimatorController中参数类型不匹配也能正常工作](https://creators.vrchat.com/avatars/animator-parameters/#mismatched-parameter-type-conversion)的特性。