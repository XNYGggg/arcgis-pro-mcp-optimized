# Architecture

三个模块共享一个仓库与一套约定，但互不强依赖：

- **mcp** —— ArcGIS Pro add-in（C#）+ Python stdio/CLI（上游 fork）。监听 `127.0.0.1:6520/mcp`。
- **guard** —— 召唤式弹窗守卫。文件令牌握手（`ready.json` / `settle` / `result.json`），事件优先检测，
  仅在操作期间存在；安全边界只允许点击“第三方代码库执行”信任框。
- **distill** —— 一次性模板蒸馏作业。`import → measure → patch → validate → preview → review → ready → apply`，
  以输入 sha256 幂等，无 LLM 的确定性工具 + 助手语义判读（技能驱动）。

共享契约：状态目录 `%LOCALAPPDATA%\ArcGIS-AgentKit`（可用 `ARCGIS_AGENTKIT_HOME` 覆盖）、
JSON 输出信封、退出码 0/2/3/4/5、每模块一个 `doctor`。

完整设计（含状态机、协议、配置、验收标准）见 [design-2026-11-16.md](design-2026-11-16.md)。
