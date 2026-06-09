# XianxiaSurvivor

XianxiaSurvivor 是一个 Unity 2D 单机修仙幸存者游戏项目。第一阶段目标是做出一个 15 分钟可玩的 MVP Demo，而不是一开始做成大型游戏。

## 项目目标

- 平台：PC
- 引擎：Unity 2D
- 语言：C#
- 类型：单机幸存者玩法
- 风格：修仙题材

## MVP 范围

第一阶段只包含以下内容：

1. 玩家移动
2. 怪物刷出并追踪玩家
3. 玩家自动释放功法攻击
4. 怪物死亡掉落灵气
5. 玩家吸收灵气升级
6. 升级时弹出三选一功法
7. 15 分钟出现 Boss
8. 玩家死亡失败，击败 Boss 通关
9. 局外有简单境界提升

暂时不做联机、开放世界、背包、复杂剧情、炼丹、炼器、宗门经营。

## 目录规则

主要代码放在 `Assets/Scripts` 下：

- `Core`：游戏状态、计时、事件等核心基础
- `Player`：玩家相关逻辑
- `Combat`：伤害、受击、战斗通用数据
- `Enemies`：怪物和 Boss 相关逻辑
- `Skills`：功法、技能、升级选择相关逻辑
- `Data`：配置数据、ScriptableObject 等
- `UI`：界面显示和交互
- `Utils`：对象池等通用工具

后续新增脚本时，优先放进对应模块目录，避免把所有逻辑写进一个大脚本。

## 当前骨架

- `GameManager`：只管理游戏状态，不写具体玩法。
- `RunTimer`：只记录一局经过时间，不直接刷 Boss。
- `EventBus`：提供简单事件分发，用于解耦 UI 和玩法逻辑。
- `ObjectPool`：基础 GameObject 对象池，后续给怪物、子弹、掉落物复用。
- `DamageInfo`：描述一次伤害的数据。
- `IDamageable`：表示对象可以受伤的接口。

## 手动创建场景

本次没有手写 `.unity` 场景文件。请在 Unity 编辑器中手动创建：

1. 打开 `Assets/Scenes`。
2. 创建场景 `MainMenu.unity`。
3. 创建场景 `Battle.unity`。
4. 打开 `File > Build Profiles` 或 `File > Build Settings`。
5. 将 `MainMenu` 和 `Battle` 添加到 Scenes In Build。
6. 建议 `MainMenu` 放在第 0 位，`Battle` 放在第 1 位。

如果暂时只测试脚本编译，也可以先保留 Unity 默认的 `SampleScene`。

## 运行方式

1. 用 Unity 打开项目根目录。
2. 等待 Unity 自动导入脚本。
3. 确认 Console 没有 C# 编译错误。
4. 在测试场景中新建空物体 `GameManager`，挂载 `GameManager.cs`。
5. 如需测试计时，新建空物体 `RunTimer`，挂载 `RunTimer.cs`。

## 开发原则

- 每次只完成指定模块。
- 不一次性重写整个项目。
- 不引入第三方插件。
- 怪物、子弹、掉落物后续优先使用对象池。
- 避免在 `Update` 里频繁查找对象。
- UI 和玩法逻辑尽量通过事件解耦。
