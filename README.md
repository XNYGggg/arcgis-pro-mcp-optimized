[![English](https://img.shields.io/badge/Language-English-1f6feb?style=flat-square)](README.md) [![简体中文](https://img.shields.io/badge/Language-%E7%AE%80%E4%BD%93%E4%B8%AD%E6%96%87-d1242f?style=flat-square)](README.zh-CN.md) [![日本語](https://img.shields.io/badge/Language-%E6%97%A5%E6%9C%AC%E8%AA%9E-bc002d?style=flat-square)](README.ja.md) [![한국어](https://img.shields.io/badge/Language-%ED%95%9C%EA%B5%AD%EC%96%B4-0047a0?style=flat-square)](README.ko.md) [![License: AGPL v3+](https://img.shields.io/badge/License-AGPL--3.0--or--later-blue?style=flat-square)](LICENSE) [![ArcGIS Pro](https://img.shields.io/badge/ArcGIS%20Pro-3.7%2B-00a1e0?style=flat-square)](https://www.esri.com/en-us/arcgis/products/arcgis-pro/overview) [![Platform](https://img.shields.io/badge/Platform-Windows%2011-lightgrey?style=flat-square)]()

# 🗺️ ArcGIS Pro MCP · AgentKit ⚡

> 🚧 **正在编写中，不开放下载。**
>
> 🚧 **Under development — not open for download.**

本分支是 ArcGIS Pro MCP 2.x 的开发版本（2.0.2 开发线）。功能与文档仍在编写，**暂不提供任何下载**：无 Release 资产、无安装包、无 NuGet 包。

This branch holds the in-progress ArcGIS Pro MCP 2.x line (2.0.2). The feature set and documentation are still being written, and **nothing is available for download** here — no release assets, no installers, no packages.

- 开发进度见 [CHANGELOG.md](CHANGELOG.md) 与提交历史；完成后会另行发布。
- Progress is tracked in [CHANGELOG.md](CHANGELOG.md) and the commit history. A separate announcement will be made once this version is finished.

---

## 🧩 Three modules at a glance / 三模块速查

> 2.x settles on a core MCP plus on-demand Guard / Distill. This is the quick version; full docs ship with the release. / 2.x 的正式形态是「核心 MCP + 按需 Guard / Distill」，先给速查版，完整文档随正式发布补齐。

| Module / 模块 | What it does / 作用 | How to use / 怎么用 | Example / 提示词示例 |
|---|---|---|---|
| 🧩 MCP | 125 native tools plus RunCode and the Python bridge / 125 个原生工具 + RunCode / Python 桥 | Install the add-in; connect your client to `127.0.0.1:6520/mcp` / add-in 装好，客户端连 `127.0.0.1:6520/mcp` | "Graduate `counties` by `AREA` into 5 classes, export an A3 PDF to `D:\out\`" |
| 🛡️ Guard | Clicks only the third-party trust dialog / 只点"第三方代码库"信任框 | Summon with `guard_summon`, verify with `guard_status` / 助手 `guard_summon` 召唤、`guard_status` 确认收队 | "Summon the guard before RunCode; confirm it disbanded after" |
| 🧪 Distill | Sample figure → reusable layout template / 样图 → 可复用版式模板 | The 10 `template_*` tools: import → measure → patch → validate → preview → review → approve → apply | "Distill `fig3.png` into a template; approve after I confirm the preview" |

**Four prompt rules / 提示词四条规则**: name the module, specify the objects, pin the output, build in a checkpoint. / 点名模块、对象写具体、输出定死、留确认点。

---

Upstream: [Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP) by Pisut Nakmuenwai, licensed AGPL-3.0-or-later. Attribution and license: [NOTICE](NOTICE), [LICENSE](LICENSE).

---

<!-- LDU-THANKS -->

<h2 align="center">🙏 Acknowledgements</h2>

<p align="center">
  This project is supported by <strong>Ludong University</strong>. We sincerely thank the university for its support.
</p>

<p align="center">
  <a href="https://www.ldu.edu.cn/"><img src="assets/ldu-emblem.png" width="104" alt="Ludong University emblem"></a>
</p>

<h3 align="center">Ludong University</h3>

> Founded in 1930 and located in Yantai, Shandong, Ludong University is a provincial comprehensive university that focuses on applied talent cultivation and pursues coordinated development across engineering, agriculture, science and the humanities. The university has 26 colleges, and six disciplines — including Engineering, Chemistry and Materials Science — rank among the global top 1% in ESI. Its motto is “Great Virtue, Broad Learning, Daily Renewal, Earnest Practice” (厚德、博学、日新、笃行).

<p align="center">
  📍 No. 186 Hongqi Middle Road, Zhifu District, Yantai, Shandong 264025, China<br>
  🌐 <a href="https://www.ldu.edu.cn/">Official Website</a> ·
  <a href="https://jwc.ldu.edu.cn/">Academic Affairs Office</a> ·
  <a href="https://lib.ldu.edu.cn/">Library</a> ·
  <a href="https://ehall.ldu.edu.cn/">Student Portal</a>
</p>
