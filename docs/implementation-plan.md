# Grasshopper 中文界面插件：预览版实施计划

目标：基于 MultilingualGH 的 MIT 翻译资源，制作 Windows Rhino 8 可安装和分享的界面汉化插件。

已确认：用户选择界面汉化，要求可分发。产品暂名 GrasshopperChinese，版本 0.1.0-preview。

## 架构与边界

- upstream/ 保存来源源码、译文、许可证及 commit；不打包旧插件和旧依赖。
- src/Core.cs：兼容上游 name/category/translation JSON；按类别优先匹配，未知条目回退；显示字段临时替换后恢复。
- src/Plugin.cs：沿用 GH_AssemblyPriority、CanvasCreated 和主菜单扩展入口。汉化已知主菜单和组件目录；对使用标准 GH_ComponentAttributes 的原生组件替换绘制类，在 Layout/Render 内使用 try/finally 恢复名称。特殊组件和第三方自定义绘制保留原文。
- Languages/zh-CN.json：上游繁体词库转简体，常用术语人工修订。未逐条审核的词库标为初稿。
- scripts/：构建、测试、打包。发行 ZIP 包含 GHA、内置词库、许可证、使用说明；无需覆盖 Rhino 文件。

## 执行与验证

1. 编写并运行词库优先级、未知回退、格式校验、临时名称恢复、用户昵称保留测试。
2. 实现核心，让同一组测试通过。
3. 使用本机 Rhino 8 SDK 编译插件，验证所有引用不随包分发。
4. 提供开启/关闭、双语模式、使用说明入口；切换时恢复菜单和目录原始内容。
5. 输出 ZIP、SHA256 和完整源码说明。
6. 人工宿主验收：启动 Rhino 8，开启 Grasshopper，检查菜单、搜索、拖放、文字组件布局；测试保存/重开、计算结果、撤销和卸载。没有实际执行的项目必须在交付说明中标为未验证。

## 发布范围

预览版交付可分享的本地 ZIP，不自动上传 GitHub 或 Yak。公共发布需使用用户确定的仓库、作者和包名。首版不承诺完整菜单、参数描述、动态报错或第三方插件汉化，也不宣称所有 Rhino 8 小版本及运行时已验证。
