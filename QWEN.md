# FantasyTown_EveryDay — QWEN.md

## 项目概述

**FantasyTown_EveryDay** 是一款基于 Unity 6 (6000.3.2f1) 开发的 2D RPG/农场模拟游戏，是《Rune Factory》(符文工厂) 的 Fan-made 项目。项目采用 **URP (Universal Render Pipeline)** 渲染管线，支持多平台发布。

- **公司/开发者**: shanflyer
- **Unity 版本**: 6000.3.2f1 (Unity 6)
- **产品名**: FantasyTown_EveryDay
- **目标平台**: Android / iOS / iPad / Windows
- **渲染管线**: URP (Universal Render Pipeline)

---
# Project Rules

你是这个项目的代码智能体。回答和修改代码前，必须先阅读项目结构，不允许凭空猜测。

## 基本原则

- 先分析，再结论，再建议修改。
- 没有明确要求时，不要直接改代码。
- 大改动前必须先列出会修改哪些文件。
- 优先使用 rg 搜索代码。
- 不要扫描 Library、Temp、Logs、obj、bin、Build、Builds、UserSettings。
- 不要随便改 .meta 文件。
- 不要自动升级 Unity Package。
- 不要把核心逻辑塞进 MonoBehaviour。
- 核心逻辑与表现实体解耦。

## Unity 性能分析要求

涉及性能问题时，必须区分：

- CPU 主线程
- Job / Burst
- GC Alloc
- DrawCall
- SetPass
- SRP Batcher
- GPU Instancing
- MaterialPropertyBlock
- Overdraw
- RenderTexture
- Shader Variant
- IO / 资源加载

## 渲染问题要求

涉及渲染时，必须说明：

- 是否影响 SRP Batcher
- 是否影响 Instancing
- 是否因为 MPB 打断合批
- 是否引入额外 Pass
- 是否增加透明 Overdraw
- 是否增加 RenderTexture 带宽
- 是否改变 RenderQueue / ZTest / ZWrite

## 输出格式

每次回答优先使用：

1. 结论
2. 证据
3. 风险
4. 具体做法
5. 是否需要修改代码

不要输出空泛建议。

## 目录结构

| 目录 | 说明 |
|------|------|
| `Assets/` | 游戏资源主目录（脚本、场景、预制体、特效、UI、音效等） |
| `Assets/Scripts/` | 所有 C# 源代码 |
| `Assets/Scenes/` | 游戏场景 |
| `Assets/Resources/` | Resources 加载资源 |
| `Assets/Editor/` | Unity Editor 工具脚本 |
| `Assets/Effect/` | 特效资源（Epic Toon FX, Cartoon FX, Hovl Studio, ProFlares 等） |
| `Assets/Plugins/` | 第三方插件（VoxelBusters, TextMesh Pro Effect） |
| `Assets/OtherPackage/` | 其他第三方包（Anima2D, TextMesh Pro, SerializableDictionary 等） |
| `Assets/MyLight/` | 自定义 2D 光照/阴影系统 |
| `Assets/ControlMaps/` | Input System 输入控制映射 |
| `Assets/AddressableAssetsData/` | Addressable Assets 配置 |
| `Datas/` | Excel 数据表（~50+ 个 .xlsx 文件） |
| `mapLink.json` | 地图场景链接配置 |
| `OutText/` | 文本输出文件 |
| `ProjectSettings/` | Unity 项目设置 |
| `Packages/` | Unity Package Manager 包清单 |
| `GameApp/` | (空) 游戏应用构建输出目录 |
| `ipad/` / `iphone/` / `andriod/` | 各平台构建输出 |
| `英语/` | 英文本地化资源 |
| `Editor_Image/` | 编辑器用图片资源 |
| `OutTexture/` | 导出纹理 |

---

## 构建场景 (Build Scenes)

EditorBuildSettings 中注册的 4 个场景：

| 场景路径 | 启用 | 用途 |
|---------|------|------|
| `Assets/Scenes/002.unity` | ✅ | 主游戏场景 |
| `Assets/Scenes/Fight.unity` | ✅ | 战斗场景 |
| `Assets/Scenes/World.unity` | ✅ | 世界地图场景 |
| `Assets/Scenes/render.unity` | ❌ | 渲染/测试场景 |

---

## 核心架构

### 设计模式

- **单例模式**: 几乎所有 Manager 都继承 `Singleton<T>` 基类（位于 `Assets/Scripts/Main/Singleton.cs`），支持自动初始化、Update/LateUpdate/FixedUpdate 生命周期管理。
- **事件驱动**: 使用 `GameActionManager` 进行模块间解耦通信。
- **ScriptableObject 数据驱动**: 游戏配置数据以 ScriptableObject 形式组织。
- **Addressable Assets**: 使用 Unity Addressables 进行资源异步加载。

### 核心 Manager（`Assets/Scripts/Main/`）

| Manager | 职责 |
|---------|------|
| `GameManager` | 游戏主入口，协调各子系统 |
| `GameController` | 游戏流程控制 |
| `GameDataManager` | 数据加载与缓存 |
| `GameDataSaveManager` | 存档（支持加密） |
| `SceneManager` | 场景管理 |
| `SceneInfoManager` | 场景信息管理 |
| `CameraManager` | 相机控制 |
| `AudioController` | 音频管理 |
| `InputManager` | 输入管理 |
| `GameTimeManager` | 游戏内时间系统 |
| `GameTimerController` | 计时器 |
| `GameSourceManager` | 资源管理 |
| `GameRuntimeObjManager` | 运行时对象管理 |
| `FriendManager` | 好感度系统 |
| `ShopManager` | 商店系统 |
| `TalkManager` | 对话系统 |
| `PlayerOperateManager` | 玩家操作管理 |
| `LanguageManage` | 多语言管理 |
| `PayManager` | 支付/内购 |
| `CloudDataManager` | 云存档 |
| `FestivalManager` | 节日系统 |
| `HomeEquipManager` | 家园装备 |
| `GameNotificationManager` | 游戏通知/提示 UI |
| `AchievementManager` | 成就系统 |
| `EmoteManager` | 表情系统 |
| `AppStoreManager` | 应用商店相关 |
| `InformationController` | 信息提示控制器 |
| `GameVolumeManager` | 音量管理 |
| `GamePlayerRecordManager` | 玩家行为记录 |

### 数据系统（`Assets/Scripts/Data/`）

- 约 50+ 个数据类，均继承/实现 `IGameData` 接口
- 数据通过 Excel 表（`Datas/*.xlsx`） → Unity Editor 工具（`ExcelDataEditor`） → ScriptableObject 的流程导入
- 关键数据文件：`ItemData`, `CharacterData`, `MonsterData`, `NPCData`, `TalkData`, `ShopDataList`, `BuffData`, `SkillData`, `FormulaData`, `PlantData`, `FishData`, `FriendShipData` 等

### ECS 系统（`Assets/Scripts/ECS/`）

自研轻量级 ECS 框架，包含：
- `Components/` — ECS 组件定义
- `ISystem/` — ECS 系统接口
- `Runtime/` — ECS 运行时
- `FlatConcurrentMap` / `NativeConcurrentMap` — 高性能并发容器

### UI 系统（`Assets/Scripts/UI/`）

Panel 式 UI 管理，`UIManager` 统一管理所有面板生命周期：
- 使用 `ShowGamePanel<T, TData>()` 泛型方法异步打开面板
- 分模块的子目录：FightPanel, FishPanel, FormulaPanel, Shop, Talk, WorldPanel, SetPanel 等 20+ 个 UI 面板模块

### 地图系统（`Assets/Scripts/Map/`）

- `WorldMapController` / `WorldMapManager` — 世界地图管理
- `MapCell` / `MapCellController` — 地图单元格管理
- `MapItemData` / `MapRoomData` — 地图物品/房间数据
- `SpecialMapLink` — 地图间链接（配合 `mapLink.json`）
- 世界地图对象管理（`WorldMapObjManager`）

### 角色系统（`Assets/Scripts/Character/`）

- `CharacterManager` — 角色总管理
- `Character` / `CharacterData` / `CharacterRuntimeObj` — 角色基础
- `PlayerData` / `Team` — 玩家和队伍
- `CharacterBehaviorManager` — 行为树驱动 (Behavior Designer)
- `NPC/` 子目录 — NPC 专有逻辑
- `TempCharacterManager` — 临时角色管理

### 自定义渲染

- **MyLight**: 自定义 2D 光照/阴影系统（`MyLightBase`, `MyLightPolygon`, `MyLightSprite`, `MySpriteShadow`, `MyShadowPolygon`）
- **Hd2d**: 高分辨率 2D 渲染相关

---

## 技术栈

### Unity Packages

| 包名 | 用途 |
|------|------|
| `com.unity.render-pipelines.universal` | URP 渲染管线 |
| `com.unity.2d.animation` | 2D 骨骼动画 |
| `com.unity.2d.psdimporter` | PSD 导入 |
| `com.unity.2d.sprite` | 2D Sprite 系统 |
| `com.unity.2d.tilemap.extras` | Tilemap 扩展 |
| `com.unity.inputsystem` | 新输入系统 |
| `com.unity.addressables` | Addressable Assets |
| `com.unity.localization` | 本地化系统 |
| `com.unity.timeline` | 时间线动画 |
| `com.unity.mathematics` | 数学库（DOTS） |
| `com.unity.ugui` | uGUI 系统 |
| `com.singularitygroup.hotreload` | 热重载 |

### 第三方插件/资源

- **Behavior Designer** — 行为树 AI
- **Anima2D** — 2D 骨骼动画
- **VoxelBusters EssentialKit** — 原生功能封装（内购、社交、iCloud 等）
- **Cartoon FX Remaster** / **Epic Toon FX** / **Hovl Studio Auras** — 特效
- **ProFlares** — 镜头光晕
- **TextMesh Pro** — 高级文本渲染
- **ProBuilder** — 关卡原型搭建
- **Cinemachine** — 相机系统
- **YamlDotNet** — YAML 序列化

---

## 编码约定

根据现有代码推断的规范：

### 命名规则
- **类名**: PascalCase（如 `GameManager`, `MapCellController`）
- **公共方法/属性**: PascalCase
- **私有字段**: camelCase（下划线前缀 `_instance` 也有使用）
- **参数**: camelCase

### 模式与架构
- **单例**: 所有核心 Manager 使用 `Singleton<T>` 基类，通过 `instance` 静态属性访问
- **异步**: 广泛使用 `async/await` + `Task` 进行异步操作
- **事件**: 通过 `GameActionManager.instance.AddListener<T>()` / `RemoveListener<T>()` 发布/订阅
- **数据**: `ScriptableObject` 配合 `[CreateAssetMenu]` 属性菜单创建
- **UI**: 泛型 Panel 基类，`UIManager.instance.ShowGamePanel<T>()` 方式打开

### 注释
- 数据类字段使用中文 `[Header("说明")]` 属性标签
- 一般逻辑代码注释较少（英文）

---

## 数据流程

```
Excel (.xlsx) ──→ ExcelDataEditor (Editor工具) ──→ ScriptableObject (.asset)
                                                         │
                                                         ▼
                                              GameDataManager (运行时加载)
                                                         │
                                                         ▼
                                              各 Manager / UI 面板使用
```

配置数据存放在 `Assets/Resources/Data/` 目录下。

---

## 构建与运行（TODO）

> 由于是 Unity 项目，构建需要在 Unity Editor 中进行操作。

- **打开项目**: 使用 Unity Hub 打开项目根目录（需 Unity 6000.3.2f1）
- **构建**: `File → Build Settings → Build`
- **编辑器工具**: 位于 `Assets/Editor/` 下，通过 Unity 菜单栏访问

---

## 开发注意事项

1. **Unity 版本锁定**: 项目使用 Unity 6000.3.2f1，打开前请确认已安装对应版本
2. **Excel 数据编辑**: 修改 `Datas/*.xlsx` 后需通过 Editor 工具重新导入
3. **地图链接**: 修改地图间链接需同步更新 `mapLink.json`
4. **Addressable Assets**: 新增资源需配置到 Addressable Groups
5. **热重载**: `Hot Reload` 包已集成，支持代码热更新
6. **本地化**: 文本通过 `LanguageData.xlsx` + `I2Languages.asset` 管理，注意维护多语言
7. **`.gitignore`**: Library, Temp, Logs, obj, UserSettings 等 Unity 自动生成目录已排除
