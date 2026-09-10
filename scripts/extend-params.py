"""Build exact-string translations for SDK Params metadata, preserving original English evidence."""
import json
from pathlib import Path

root = Path(__file__).resolve().parents[1]
tools = json.loads((root / 'docs/native-catalog.json').read_text(encoding='utf-8-sig'))['tools']
names = '''Angular Dimension|角度标注
Centermark|中心标记
Hatch|填充
Block Instance|块实例
Leader|引线
Light|灯光
Linear Dimension|线性标注
Ordinate Dimension|坐标标注
Radial Dimension|半径标注
Annotation Dot|注释点
Text Entity|文字对象
Mesher Settings|网格划分设置
Domain²|二维区间
Vector|向量
Circular Arc|圆弧
Boolean|布尔值
Box|长方体
Brep|边界表示体
Circle|圆
Colour|颜色
Complex|复数
Culture|区域文化设置
Curve|曲线
Extrusion|挤出体
Field|向量场
File Path|文件路径
Geometry|几何
Group|组
Guid|全局唯一标识符
Integer|整数
Domain|区间
Location|地理位置
Line|线段
Matrix|矩阵
Mesh|网格
Mesh Face|网格面
Number|数值
Data|数据
Shader|显示材质
Plane|平面
Point|点
Point Cloud|点云
Predicate|判断条件
Rectangle|矩形
Data Path|数据路径
SubD|细分曲面
Surface|曲面
Text|文本
Time|时间
Transform|变换
Receiver|数据接收器
Boolean Toggle|布尔开关
Button|按钮
Cluster Input|群集输入
Cluster Output|群集输出
Cluster|群集
Colour Picker|颜色选择器
Colour Swatch|色样
Colour Wheel|色轮
Constant|常量
Control Knob|控制旋钮
Data Recorder|数据记录器
Digit Scroller|数字滚动器
Geometry Pipeline|几何管线
Geometry Cache|几何缓存
Gradient|渐变
Graph Mapper|图形映射器
Image Sampler|图像采样器
Cherry Picker|单项选取器
Jump|跳转
Sketch|草图
MD Slider|多维滑块
Number Slider|数值滑块
Param Viewer|参数查看器
Relay|接线中继
Suirify|Suire 数据树简化
Text Balloon|文字气泡
Panel|面板
Trigger|触发器
Value List|值列表
Timeline|时间轴
Read File|读取文件
Data Dam|数据闸门
Match Text|文本匹配
Model Attribute Key|模型属性键
Model Content|模型内容
Font|字体
Model Mesher Settings|模型网格划分设置
Unit System|单位制
Arrow|箭头
Arrow Settings|箭头设置
Dimension Settings|标注设置
Leader Settings|引线设置
Text Settings|文字设置
Tolerance Settings|公差设置
Units Settings|单位设置
Date Time Format|日期时间格式
Model Annotation Style|模型标注样式
Earth Anchor Point|地球锚点
Model Block Definition|模型块定义
Model Layer|模型图层
Model Object|模型对象
Display Mode|显示模式
Layout Viewport|布局视口
Model Viewport|模型视口
View|视图
Object Display|对象显示
Object Visibility|对象可见性
Color Gradient|颜色渐变
Color Stop|渐变色标
Hatch Line|填充线
Model Hatch Pattern|模型填充图案
Model Linetype|模型线型
Model Print Width|模型打印线宽
Object Drafting|对象制图属性
Model Environment|模型环境
Model Material|模型材质
Model Texture|模型纹理
Object Render|对象渲染属性'''
names = dict(line.split('|', 1) for line in names.splitlines())
descriptions = '''Represents a list of Meshing settings.|表示一组网格划分设置。
A predicate defines the criteria and determines whether a specified object meets those criteria|定义判断条件，并确定指定对象是否满足这些条件。
A data receiver object.|用于接收数据的对象。
Boolean (true/false) toggle|在真（True）和假（False）之间切换的布尔开关。
Button object with two values|具有两种状态值的按钮。
Represents a cluster input parameter|表示群集的输入参数。
Represents a cluster output parameter|表示群集的输出参数。
Contains a cluster of Grasshopper components|包含一组封装为群集的 Grasshopper 组件。
Provides a colour picker object|提供颜色选择器。
Colour (palette) swatch|用于设置颜色的色样面板。
Creates a palette of related colours|创建一组相互关联的配色。
Define a document-wide constant for use in Expressions|定义可在整个文档的表达式中使用的常量。
A radial dial knob for settings numbers|通过旋转拨盘设置数值。
Records data over time|随时间记录输入的数据。
Numeric scroller for single numbers|通过滚动调整单个数值。
Defines a geometry pipeline from Rhino to Grasshopper|建立从 Rhino 向 Grasshopper 传入几何对象的管线。
Push or Pull geometry to and from the Rhino document|将几何对象写入 Rhino 文档，或从 Rhino 文档读取几何对象。
Represents a multiple colour gradient|表示包含多个颜色的渐变。
Represents a numeric mapping function|通过图形定义数值映射函数。
A group of Grasshopper objects|由多个 Grasshopper 对象组成的组。
Provides image (bitmap) sampling routines.|提供图像（位图）采样功能。
Pick a single item from a data tree|从数据树中选取单个数据项。
Jump between different locations|在不同的画布位置之间跳转。
A series of doodles|一组手绘草图。
A multidimensional slider|用于输入多个维度数值的滑块。
Numeric slider for single values|通过滑块设置单个数值。
A viewer for data structures.|查看数据结构。
A wire relay object|用于整理连线的数据中继对象。
Suire-style simplification of data trees.|使用 Suire 方式简化数据树。
A thought balloon for annotation|用于注释的文字气泡。
A panel for custom notes and text values|用于输入自定义注释、文本值或查看数据的面板。
Manually or cyclically trigger updates of certain components.|手动或周期性触发指定组件更新。
Provides a list of preset values to choose from|提供可供选择的预设值列表。
A timeline of values|沿时间轴组织数值。
Read the contents of a file|读取文件内容。
Delay data on its way through the document|暂缓数据在文档中的传递。
Specialized parameter to compare string input against different comparison engines|使用不同的比较方式匹配输入字符串的专用参数。
Arrow type|箭头类型。
Contains an Earth anchor point which associates a model point and orientation with a latitude, longitude, and elevation value.|包含地球锚点，将模型中的点及方向与纬度、经度和高程关联。'''
descriptions = dict(line.split('|', 1) for line in descriptions.splitlines())
rows = json.loads((root / 'Languages/zh-CN.json').read_text(encoding='utf-8'))
index = {(r.get('category', ''), r['name']): r for r in rows}
phrase_rows = {}
covered = 0
for tool in tools:
    if tool['category'] != 'Params':
        continue
    english = tool['name']
    chinese = names[english]
    index[('Params', english)] = {'name': english, 'category': 'Params', 'translation': chinese}
    description = tool['description']
    if description.startswith('Contains a collection of '):
        translated = '存储一组' + chinese + '数据。'
        if 'This parameter is obsolete' in description:
            translated += '此参数已弃用，已由支持持久化数据的新版参数替代。'
    else:
        translated = descriptions[description]
    # Exact original sentence is the lookup key; no blind substring replacement of unknown descriptions.
    phrase_rows.setdefault(description, {'name': description, 'category': 'description', 'translation': translated})
    covered += 1
(root / 'Languages/zh-CN.json').write_text(json.dumps(list(index.values()), ensure_ascii=False, indent=2), encoding='utf-8')
(root / 'Languages/tool-text.zh-CN.json').write_text(json.dumps(list(phrase_rows.values()), ensure_ascii=False, indent=2), encoding='utf-8')
print('Params metadata rows:', covered, 'Exact description entries:', len(phrase_rows))
