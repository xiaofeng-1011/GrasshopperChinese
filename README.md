# GrasshopperChinese · Rhino 8 Grasshopper 中文界面

面向 **Windows Rhino 8 / Grasshopper 1** 的中文界面插件，支持原生工具名称、分类展开列表、画布电池标题和部分工具说明汉化，提供中文、中英对照及恢复英文功能。

**当前版本：0.3.0-preview。** 尚未覆盖全部原生工具，也尚未完成 Rhino 窗口内的完整验收。第三方插件不在当前汉化范围内。

## 下载与安装

- [Windows 安装包](downloads/GrasshopperChinese-0.3.0-preview-win.zip)
- [源码压缩包](downloads/GrasshopperChinese-0.3.0-preview-source.zip)
- [安装包 SHA-256](downloads/GrasshopperChinese-0.3.0-preview-win.zip.sha256)

1. 完整解压安装包。
2. 关闭所有 Rhino 窗口，双击 `Install.cmd`。
3. 启动 Rhino 8，运行 `Grasshopper`。
4. 在新增的“中文界面”菜单中切换显示模式。

安装无需管理员权限，也无需 Python、Visual Studio 或 .NET SDK。安装目录为 `%APPDATA%\Grasshopper\Libraries\GrasshopperChinese`，不会覆盖 Rhino 程序文件。插件仅在 Rhino 8 中加载。

手动安装：将安装包内的 `GrasshopperChinese` 文件夹复制到 `File > Special Folders > Components Folder`。如 Windows 阻止加载，请在下载文件属性中解除阻止后重新解压。

## 功能与覆盖

| 位置 | 当前支持 |
| --- | --- |
| 主菜单 | 已知主菜单与部分子菜单汉化 |
| 顶部工具栏 | 原生分类及已知分组标题汉化 |
| 分类展开列表 | 按组件 GUID 更新已匹配工具名称 |
| 画布电池 | 标准组件与独立参数标题汉化，为读取对象名称的绘制扩展提供临时中文名称 |
| 工具说明 | 部分工具用途说明，按英文原文精确匹配 |
| 设置 | 中文、中英对照、关闭汉化及恢复英文 |
| 覆盖检查 | 导出已加载原生工具与缺译清单 |

已为提取到的 **123 个 Params 工具记录**补充名称和说明，其中包含旧版重复组件，共 120 个不同说明文本。词库还包含来自 MultilingualGH 的其他工具名称；词条数量不代表完整覆盖率。

自定义昵称予以保留。图标上方由其他插件绘制的名称需要单独验证；纯中文模式下英文检索受 Grasshopper 搜索缓存影响。

## 更新与卸载

- 更新：关闭 Rhino，运行新版安装包的 `Install.cmd` 覆盖安装。
- 暂时关闭：取消勾选“中文界面 → 启用界面汉化”。
- 卸载：关闭 Rhino，运行 `Uninstall.cmd`。仅移除本插件已知文件，保留用户新增文件和偏好设置。

## 兼容性与限制

已使用 Rhino SDK `8.31.26126.13431` 编译，通过核心逻辑、SDK 对象适配及隔离安装/卸载测试。Rhino GUI 中的显示、搜索、拖放、保存重开、求解回归和不同 .NET 运行时仍需验收，详见[人工验收清单](docs/manual-acceptance.md)。

- 不支持 macOS 或 Rhino 7。
- 部分工具说明、输入输出参数、右键菜单、动态报错及特殊控件内部文字仍为英文。
- 功能区分页适配使用 Rhino 8 内部字段，版本变化可能需要更新适配。
- 同时启用多个汉化或名称标注插件时，文字可能互相覆盖。
- 画布名称在绘制期间临时替换，结束后恢复；文件往返不变性仍需在 Rhino 中验证。

## 开发与构建

```text
src/          C# 插件与界面适配
Languages/    中文名称与说明词库
installer/    Windows 安装和卸载脚本
scripts/      构建、测试、打包与覆盖检查
 tests/       核心逻辑与 SDK 适配测试
 docs/        开发资料与人工验收清单
upstream/     上游源码参考、许可与来源版本
 downloads/   当前预览版安装包与源码包
```

在已安装 Rhino 8 的 Windows PowerShell 中执行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/package.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tests/InstallTests.ps1
```

打包脚本执行测试和编译，输出 ZIP 及 SHA-256 到 `dist/`。项目使用 Windows 自带的 .NET Framework C# 编译器，无需 NuGet。单独编译可指定安装路径：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build.ps1 -RhinoRoot 'C:\Program Files\Rhino 8'
```

完整打包中的测试脚本使用默认 Rhino 8 路径，非默认安装时需同步调整测试脚本。

## 反馈

欢迎通过仓库 Issues 提供 Rhino 完整版本、工具英文名称、分类、预期译法和截图。插件菜单中的“导出原生工具及缺译清单”可用于核对实际工具覆盖。

## 致谢与许可

基于 [MultilingualGH](https://github.com/lin-ycv/MultilingualGH) 的 MIT 翻译资源、词库结构与部分加载入口开发。感谢 Victor Lin 及原项目贡献者。

上游版权声明保留在 [upstream/LICENSE](upstream/LICENSE)，来源版本见 [upstream/SOURCE.json](upstream/SOURCE.json)，复用范围见 [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md)。安装包保留 `LICENSE-MultilingualGH.txt`。

新增代码暂未指定统一开源许可证；公开源码不等同于授予新增代码的任意使用许可。上游 MIT 许可按其原有适用范围生效。
