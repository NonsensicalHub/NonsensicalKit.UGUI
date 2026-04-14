# NonsensicalKit.UGUI

`com.nonsensicallab.nonsensicalkit.ugui` 提供基于 Unity UGUI 的高复用 UI 组件与工具集合，用于快速搭建列表、弹窗、页面联动、媒体控制与小地图等常见界面能力。

---

## 核心模块一览

### UIFactory（UI工厂）

- **模块定位**：统一管理弹窗与临时 UI 的创建、参数注入与复用回收。
- **核心入口**：`UIFactory`、`IFactoryUI`、`FactoryUIPrefabSetting`
- **使用方法**：
  1. 在 `Services/UIFactory` 预制体中配置 `m_prefabs`，为每种 UI 绑定 `Type/Alias` 与 Prefab。
  2. UI 预制体挂载实现 `IFactoryUI` 的脚本，在 `SetArg(object arg)` 里接收参数。
  3. 业务层通过 `OpenUI(type, arg)` 或发布 `"OpenUI"` 信号触发弹出。
  4. 关闭时将实例归还对象池，避免频繁实例化销毁。

### MiniMap(小地图)

- **模块定位**：提供小地图的拖拽、缩放、自动居中与图标映射能力。
- **核心入口**：`MapController`、`IconManagerBase`、`CoordinateTransformation`
- **使用方法**：
  1. 在小地图节点挂载 `MapController`，绑定前景遮罩与地图背景 `RectTransform`。
  2. 配置玩家图标或玩家坐标源，按需开启自动居中与居中缓动参数。
  3. 通过 Icon 管理器注册图标配置，把场景对象映射到小地图图标层。
  4. 结合示例中的交互脚本扩展测距、区域绘制与图标点击逻辑。

### Table（表格）

- **模块定位**：提供可复用的虚拟化列表、滚动表格、树形表格和流程表格组件。
- **核心入口**：`ScrollView`、`ScrollTable`、`ListTableManager`、`TreeNodeTableManagerBase`
- **使用方法**：
  1. 普通列表优先使用 `ScrollView`，设置 `ItemCountFunc` 与 `UpdateFunc` 后调用 `UpdateData()`。
  2. 二维数据展示使用 `ScrollTable`，通过 `SetTableData(...)` 配置表格内容与行列尺寸。
  3. 业务化列表继承 `ListTableManager<TElement, TData>` 与 `ListTableElement<TData>` 做数据绑定。
  4. 大数据场景保持对象池复用，避免一次性创建全量单元格造成卡顿。

### 音视频播放器

- **模块定位**：统一管理音频/视频播放状态、进度同步与 UI 控件联动。
- **核心入口**：`VideoManager`、`AudioManager`、`MediaProgress`
- **使用方法**：
  1. 视频播放挂载 `VideoManager`，配置 `RawImage`、播放区、控制区与全屏 Canvas。
  2. 音频播放挂载 `AudioManager`，通过 URL 或 `AudioClip` 触发播放。
  3. 使用 `Play/Pause/Switch/Replay` 与 `PlayTime` 控制播放流程。
  4. 订阅 `OnPlayStateChanged`、`OnPlayProgressChanged` 事件驱动按钮状态和进度条刷新。

### Effect（通用的简单UI动画效果）

- **模块定位**：为 UGUI 提供轻量化的通用动画与视觉增强效果。
- **核心入口**：`UGUIEffectBase`、`ZoomEffect`、`UIGradient`
- **使用方法**：
  1. 在目标 UI 挂载效果脚本，必要时显式指定 `m_target`。
  2. 运行时调用 `ShowEffect(command)` 触发动画（如 `ZoomEffect` 放大/缩小）。
  3. 静态渐变场景直接使用 `UIGradient` 配置颜色与角度。
  4. 对高频触发效果控制时长与并发，避免 UI 动画叠加造成跳变。

### Items(各种小组件)

- **模块定位**：提供页面搭建高频使用的基础组件和行为封装。
- **核心入口**：`ToggleButton`、`DragSpace/DragSpacePlus`、`FollowGameObject`、`SwitchPageController`、`RestrictedInputFieldBase`
- **使用方法**：
  1. 按交互类型选择组件并挂载到 UI 节点（切换、拖拽、跟随、分页切换等）。
  2. 在 Inspector 中完成按钮状态、边界、目标节点、输入约束等参数配置。
  3. 对输入限制类组件先定义规则（浮点、区间、固定值）再接入业务校验。
  4. 保持组件职责单一，复杂业务组合多个小组件而非在单点脚本堆逻辑。

### SimpleSignalControl（简单信号控制）

- **模块定位**：基于 Aggregator 的轻量信号收发系统，实现低耦合 UI 联动控制。
- **核心入口**：`SignalPublisher`、`SendSignalButton/SendSignalToggle`、`SignalControlUI/SignalControlActive/SignalControlText`
- **使用方法**：
  1. 在发送端组件中配置信号名与数据类型，绑定按钮、Toggle 或触摸事件。
  2. 在接收端组件中配置对应信号，映射显示隐藏、激活切换、文本更新等行为。
  3. 统一约定信号命名，避免同名异义导致联动冲突。
  4. 跨模块状态同步优先走信号通道，减少 UI 间直接引用。

### VisualLogicFlowGraph(可视化逻辑节点流程图)

- **模块定位**：提供可视化节点编辑、连线和流程存读档能力。
- **核心入口**：`VisualLogicGraph`、`VisualLogicNodeBase`、`VisualLogicPointBase`、`IVisualSaveData`
- **使用方法**：
  1. 在图组件中配置节点预制体映射、连线预制体、黑板与对象池容器。
  2. 启动时调用 `Init(createNodeInfo, createPointInfo)` 注入节点/点位信息创建方法。
  3. 通过菜单或 `AddNewNode(type)` 创建节点，运行时支持 `DeleteNode(id)` 与状态更新。
  4. 使用 `Save<T...>()` 与 `Load<T...>(data)` 实现图数据持久化与恢复。

---

### 编辑器工具

| 工具 | 作用 | 菜单入口 |
| --- | --- | --- |
| CreateListTable | 一键生成 ListTable 预制体及配套 `Manager/Element` 脚本模板 | `Assets/Create/NonsensicalKit/UGUI/CreateListTable` |
| UGUIAutoAnchor | 将选中 UI 节点自动转换为自适应锚点，减少手动对齐成本 | `NonsensicalKit/UGUI/自动自适应锚点` |
| PrefabFontModifier | 批量扫描预制体并替换 TextMeshPro 字体资源 | `NonsensicalKit/UGUI/预制体字体修改` |
| AddScrollViewExtension | 快速创建带 Viewport/Scrollbar 的 `ScrollView` 标准结构 | `GameObject/Nonsensical/UI/ScrollView` |
| AddScrollTableExtension | 快速创建 `ScrollTable` 标准结构（含 Cell/Row/Column 容器） | `GameObject/Nonsensical/UI/ScrollTable` |
| AddScrollViewExExtension | 快速创建扩展版 `ScrollViewEx` 结构用于高级滚动场景 | `GameObject/Nonsensical/UI/ScrollViewEx` |
| AddScrollViewMK2Extension | 快速创建 `ScrollView_MK2` 结构用于 MK2 虚拟滚动方案 | `GameObject/Nonsensical/UI/ScrollView_MK2` |

## 示例

- `MultilevelMenu`：多级菜单展开/收起与层级导航示例。
- `RightClickMenu`：右键菜单弹出、定位和命令分发示例。
- `ScrollView`：基础滚动列表渲染与元素复用示例。
- `ScrollTable`：表格式数据展示、列定义与刷新示例。
- `SquarePuppy`：网格布局与卡片化交互示例。
- `StepGroup`：步骤流切换、状态同步和进度展示示例。
- `TreeNodeTable`：树形节点表格展示与层级操作示例。
- `MediaManager`：视频/音频控制面板与播放状态联动示例。
- `VisualLogicFlowGraph`：可视化流程图节点展示与连线示例。
- `MissionProcessWindow`：任务流程窗口与状态推进示例。
- `MiniMap`：小地图图标跟随、缩放与交互示例。
- `WindowSizeResetTool`：窗口尺寸拖拽调整与布局自适应示例。
