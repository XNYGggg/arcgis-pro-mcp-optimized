[![English](https://img.shields.io/badge/Language-English-1f6feb?style=flat-square)](README.md) [![简体中文](https://img.shields.io/badge/Language-%E7%AE%80%E4%BD%93%E4%B8%AD%E6%96%87-d1242f?style=flat-square)](README.zh-CN.md) [![日本語](https://img.shields.io/badge/Language-%E6%97%A5%E6%9C%AC%E8%AA%9E-bc002d?style=flat-square)](README.ja.md) [![한국어](https://img.shields.io/badge/Language-%ED%95%9C%EA%B5%AD%EC%96%B4-0047a0?style=flat-square)](README.ko.md) [![License: AGPL v3+](https://img.shields.io/badge/License-AGPL--3.0--or--later-blue?style=flat-square)](LICENSE) [![ArcGIS Pro](https://img.shields.io/badge/ArcGIS%20Pro-3.7%2B-00a1e0?style=flat-square)](https://www.esri.com/en-us/arcgis/products/arcgis-pro/overview) [![Platform](https://img.shields.io/badge/Platform-Windows%2011-lightgrey?style=flat-square)]()

# 🗺️ ArcGIS Pro MCP · AgentKit ⚡

> 🚧 **開発中につき、ダウンロードは未公開です。**
>
> 🚧 **Under development — not open for download.**

このブランチは ArcGIS Pro MCP 2.x の開発版（2.0.2 開発ライン）です。機能とドキュメントはまだ作成中で、**現時点では一切ダウンロードできません**：Release アセット、インストーラー、NuGet パッケージはいずれもありません。

This branch holds the in-progress ArcGIS Pro MCP 2.x line (2.0.2). The feature set and documentation are still being written, and **nothing is available for download** here — no release assets, no installers, no packages.

- 開発の進捗は [CHANGELOG.md](CHANGELOG.md) とコミット履歴を参照してください。完成後に別途アナウンスします。
- Progress is tracked in [CHANGELOG.md](CHANGELOG.md) and the commit history. A separate announcement will be made once this version is finished.

---

## 🧩 3 モジュール早見表 / Three modules at a glance

> 2.x の正式構成は「中核 MCP + オンデマンドの Guard / Distill」です。まず早見版を掲載し、完全なドキュメントは正式リリース時に追加します。 / 2.x settles on a core MCP plus on-demand Guard / Distill; full docs ship with the release.

| モジュール / Module | 役割 / What it does | 使い方 / How to use | 例 / Example |
|---|---|---|---|
| 🧩 MCP | 125 個のネイティブツール + RunCode / Python ブリッジ / 125 native tools plus RunCode and the Python bridge | add-in を導入し、クライアントを `127.0.0.1:6520/mcp` に接続 / install the add-in, connect to `127.0.0.1:6520/mcp` | 「`counties` を `AREA` で 5 クラスに分け、A3 PDF を `D:\out\` に出力」 |
| 🛡️ Guard | サードパーティ信頼ダイアログのみクリック / clicks only the third-party trust dialog | `guard_summon` で召喚、`guard_status` で撤収確認 / summon with `guard_summon`, verify with `guard_status` | 「RunCode の前にガードを召喚し、終了後に撤収を確認」 |
| 🧪 Distill | サンプル図 → 再利用可能なレイアウトテンプレート / sample figure → reusable layout template | 10 個の `template_*`：import → measure → patch → validate → preview → review → approve → apply | 「`fig3.png` をテンプレートに蒸留し、プレビュー確認後に approve」 |

**プロンプト 4 原則 / Four prompt rules**：モジュールを指名・対象を具体化・出力を確定・確認ポイントを設ける。 / Name the module, specify the objects, pin the output, and build in a checkpoint.

---

上流プロジェクト：[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)（作者 Pisut Nakmuenwai、AGPL-3.0-or-later）。帰属とライセンス：[NOTICE](NOTICE)、[LICENSE](LICENSE)。

---

<!-- LDU-THANKS -->

<h2 align="center">🙏 謝辞</h2>

<p align="center">
  本プロジェクトは <strong>魯東大学</strong> の支援を受けて開発されています。ここに心より感謝申し上げます。
</p>

<p align="center">
  <a href="https://www.ldu.edu.cn/"><img src="assets/ldu-emblem.png" width="104" alt="魯東大学 校章"></a>
</p>

<h3 align="center">魯東大学 · Ludong University</h3>

> 魯東大学は1930年に創立され、山東省煙台市に位置する山東省所属の総合大学です。応用型人材の育成を主導とし、工学・農学・理学・文学が調和して発展しています。26の学院を擁し、工学・化学・材料科学など6つの学問分野がESI世界ランキングの上位1%にランクインしています。校訓は「厚徳・博学・日新・篤行」です。

<p align="center">
  📍 中国山東省煙台市芝罘区紅旗中路186号（郵便番号 264025）<br>
  🌐 <a href="https://www.ldu.edu.cn/">公式サイト</a> ·
  <a href="https://jwc.ldu.edu.cn/">教務処</a> ·
  <a href="https://lib.ldu.edu.cn/">図書館</a> ·
  <a href="https://ehall.ldu.edu.cn/">学生ポータル</a>
</p>
