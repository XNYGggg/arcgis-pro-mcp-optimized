# Architecture

三个模块共享一个仓库与一套约定，但互不强依赖（版本 2.0.1）：

- **mcp** —— ArcGIS Pro add-in（C#）+ Python stdio/CLI（上游 fork）。监听
  `127.0.0.1:6520/mcp`，共 **125 个工具**：113 个上游原生工具 + `guard_summon` /
  `guard_status` + 10 个 `template_*`。
- **guard** —— 召唤式弹窗守卫。文件令牌握手（`ready.json` / `settle` /
  `result.json`），UIA 事件优先 + 兜底轮询，仅在操作期间存在；安全边界只允许点击
  “第三方代码库执行”信任框，覆盖/删除类确认框只警报、绝不点击。add-in 在
  `run_python_toolbox_tool` 与 `.pyt/.tbx/.atbx` 的 GP 调用前后自动 arm/settle。
- **distill** —— 一次性模板蒸馏作业。`import → measure → patch → validate →
  preview → review → approve → apply`，以输入 sha256 幂等；measure/validate/preview
  无 LLM，语义字段由助手按 `skills/template-distill/SKILL.md` 通过 `patch` 提交，
  经校验合并后写入库；只有 review + approve 后才是 `ready`。

共享契约：状态目录 `%LOCALAPPDATA%\ArcGIS-AgentKit`（可用 `ARCGIS_AGENTKIT_HOME`
覆盖）、JSON 输出信封、退出码 0/2/3/4/5、每模块一个 `doctor`。模板库默认在仓库
`library/`，客户版在 `%LOCALAPPDATA%\ArcGIS Pro MCP\library`，可用
`ARCGIS_DISTILL_LIBRARY` 覆盖。

完整设计（含状态机、协议、配置、验收标准）见
[design-2026-11-16.md](design-2026-11-16.md)；守卫与蒸馏的操作说明见
[guard.md](guard.md) 与 [distill.md](distill.md)。
