[![English](https://img.shields.io/badge/Language-English-1f6feb?style=flat-square)](README.md) [![简体中文](https://img.shields.io/badge/Language-%E7%AE%80%E4%BD%93%E4%B8%AD%E6%96%87-d1242f?style=flat-square)](README.zh-CN.md) [![日本語](https://img.shields.io/badge/Language-%E6%97%A5%E6%9C%AC%E8%AA%9E-bc002d?style=flat-square)](README.ja.md) [![한국어](https://img.shields.io/badge/Language-%ED%95%9C%EA%B5%AD%EC%96%B4-0047a0?style=flat-square)](README.ko.md) [![License: AGPL v3+](https://img.shields.io/badge/License-AGPL--3.0--or--later-blue?style=flat-square)](LICENSE) [![ArcGIS Pro](https://img.shields.io/badge/ArcGIS%20Pro-3.7%2B-00a1e0?style=flat-square)](https://www.esri.com/en-us/arcgis/products/arcgis-pro/overview) [![Platform](https://img.shields.io/badge/Platform-Windows%2011%20x64-lightgrey?style=flat-square)]() [![Release](https://img.shields.io/github/v/release/XNYGggg/arcgis-pro-mcp-optimized?style=flat-square&label=release)](../../releases)

# 🗺️ ArcGIS Pro MCP · AgentKit ⚡

**1 つのリポジトリ、3 つのパーツ — AI コパイロットが、今開いている地図にようやく手を届けます (｀・ω・´)ゞ**

ファイルをあちこち受け渡す必要も、「エクスポート → アップロード → ダウンロード」の儀式も不要。アシスタントが目の前の ArcGIS Pro セッションと直接対話します：あなたのデータ、あなたの地図、あなたのレイアウト。🔒 すべて `127.0.0.1` の中で完結します。

> **📌 バージョン通知（2026-11-16）：** **1.0 エディション（[v1.1.4](../../releases/tag/v1.1.4)）は非常に安定しており、日常利用に推奨します。** **v2.0.0 は現在テスト段階**にあり、本番利用はまだ推奨しません — 安定版パッケージは [Release v1.1.4](../../releases/tag/v1.1.4) からダウンロードしてください。
>
> **📌 ブランチ通知：** `customer-v2.0.0` は復元された 2.0.0 カスタマーパッケージの履歴ブランチです。新規プロジェクトには **V2.01（[v2.0.1](../../releases/tag/v2.0.1)）** を使用してください。本 README のマッピング / ガード / 蒸留のプロンプトガイドは同様に適用できますが、修正済みの `template_preview --out` 処理には v2.0.1 を使用してください。

> ### 🙏 上流プロジェクトと原作者
> 本プロジェクトは **[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)** の上に立っています。
> 原作者は **Pisut Nakmuenwai**、ライセンスは **AGPL-3.0-or-later**。心より感謝します！🌟
> ローカルパッチを **v2.0.0** まで積み上げ、guard / distill モジュールを追加しました。上流の帰属表示は [NOTICE](NOTICE) とソーススナップショットにあります。

---

## ✨ これは何か

平易な言葉で言うだけ — *「郡を面積で色分けして、パステルレッドからグリーンに」* — 地図が変わるのを見てください。🎨

| | パーツ | 役割 | 状態 |
|---|---|---|---|
| 🧩 | **MCP** | ArcGIS Pro add-in：**ネイティブ 125 ツール**（インプロセス、約 9 ms ⚡）、すべてのジオプロセシングツール、任意の RunCode / Python ブリッジによる arcpy。`127.0.0.1:6520/mcp` で待ち受け。 | **v2.0.1 プレビュー · 実機検証済み** 🧪 · 安定版：v1.1.4 |
| 🛡️ | **Guard** | 「サードパーティ コード ライブラリ」信頼ダイアログ向けのオンデマンドガード。必要なときだけ召喚され、直後に消えます。🥷 | **準備完了** — [guard.md](docs/guard.md) |
| 🧪 | **Distill** | オンデマンドのテンプレート蒸留：サンプル図 → 再利用可能な版式 spec。ワンショット、デーモンなし、完全再現可能。🧬 | **準備完了** — [distill.md](docs/distill.md) |

### 🧠 なぜ「オンデマンド」なのか（ゾンビプロセスは誰も好きじゃないから 🧟）

- 🥷 **常駐ゼロ。** 危険な操作がなければ、ガードプロセスもポーリングもなし — タスクマネージャーは静かなものです。
- 🛡️ **ガードはちょうど 1 操作の間だけ生きる。** 信頼ダイアログが出るかもしれない直前に召喚され、操作が落ち着いた直後に解散。自動クリック成功＝あなたは気づかない ✨；失敗＝最前面アラート（30 秒以上、音あり）🚨。クリックするのはサードパーティコードの信頼ダイアログだけ — 上書き/削除の確認？絶対に触りません。🙅
- 🧪 **蒸留はワンショットのジョブ。** 受信箱の監視もサービスもなし。必要なときに実行するだけ。すべてのステップは冪等で再開可能。✅

```mermaid
flowchart LR
  AI[🤖 AI クライアント<br/>Claude · Codex · その他] -->|MCP over HTTP<br/>127.0.0.1:6520| M[🧩 ArcGIS Pro MCP add-in<br/>ArcGIS Pro 内]
  M -.->|必要なときだけ召喚 🛡️| G[ダイアログガード<br/>短命プロセス]
  M -->|任意の arcpy 🐍| R[RunCode ツールボックス /<br/>Pro 内 Python ブリッジ]
  F[🖼️ サンプル地図画像] -->|ワンショット作業 🧪| D[テンプレート蒸留]
  D -->|版式 spec 📐| M
```

---

## 🚀 クイックスタート — カスタマーエディション

v2.0.1 は ArcGIS Pro 3.7 での実機テスト（125 ツール、自動ガード、RunCode、template_*）に合格していますが、依然として**プレビュー / プレリリース**です。安定版の既定は v1.1.4 です：

- ✅ **安定版 · 推奨：** [**Release v1.1.4**](../../releases/tag/v1.1.4) → `ArcGIS-Pro-MCP-v1.1.4.zip` — add-in 1.1.4、stdio ブリッジ、RunCode、ダイアログガード、Python ブリッジ、インストーラー、ドキュメント、完全な AGPL ソーススナップショット（全ファイルが同梱の `SHA256SUMS.txt` で検証可能）
- 🧪 **プレビュー · 実機検証済み：** [**Release v2.0.1**](../../releases/tag/v2.0.1) → `ArcGIS-Pro-MCP-v2.0.1.zip` — add-in 2.0.1、オンデマンドガードエンジン、同梱の Distill ランタイム、インストーラーのテンプレートライブラリ修正
- 🗄️ **以前のプレビュー：** [**v2.0.0**](../../releases/tag/v2.0.0)（プレリリース、保存済み）
- 👀 ソースを見る： [**`customer-v1.1.4`**](../../tree/customer-v1.1.4)（安定） · [**`customer-v2.0.0`**](../../tree/customer-v2.0.0)（プレビュー）
- 🛺 コマンド 1 つで済ませたい？ 下の [NuGet インストーラー](#-install-via-nuget-github-packages) をどうぞ ⬇️

1. 📂 任意の場所に解凍 → `install/00-install.cmd` をダブルクリック（管理者不要；ArcGIS Pro **3.7 以降**が必要）。
2. 🖥️ ArcGIS Pro を開く → 未署名 add-in の信頼を求められたら「はい」😄 → **MCP** タブが **running**（ポート `127.0.0.1:6520`）であることを確認。
3. 🔌 AI クライアントを接続（下の設定セクション参照）。
4. ✅ `install/01-check.cmd` を実行 — すべて `[OK]` なら完璧です (๑•̀ㅂ•́)و✧

Python 🐍 不要、pip 📦 不要、インターネット 🌐 不要：add-in は ArcGIS Pro 3.7 以降に同梱の .NET ランタイムで動作し、ブリッジ / ガードは Windows 標準の PowerShell を使用します。`SHA256SUMS.txt` がすべてのファイルをカバーします。🔐

### 🛺 NuGet 経由でインストール（GitHub Packages）

コマンド 1 つがよければ、[`ArcGISProMCP.Install`](../../pkgs/nuget/ArcGISProMCP.Install) dotnet ツールがリリース zip をダウンロードし、`SHA256SUMS.txt` で全ファイルを検証してから `install/00-install.cmd` を起動します。.NET SDK と `read:packages` 権限付きの [PAT](https://github.com/settings/tokens) が必要です：

```powershell
# 初回のみ：GitHub Packages の NuGet ソースを登録
dotnet nuget add source "https://nuget.pkg.github.com/XNYGggg/index.json" --name github --username XNYGggg --password <YOUR_PAT> --store-password-in-clear-text

dotnet tool install --global ArcGISProMCP.Install --version 1.1.4   # 安定版。2.0.1 はプレビュー
arcgis-pro-mcp-install              # --dry-run を付けるとダウンロードと検証のみ
```

---

## 🎛️ 設定 — AI クライアントの接続

**前提：** ArcGIS Pro が開いていて、**MCP** タブが **running** を表示していること。すべてのポートはループバック限定です：

| ポート | 用途 |
|---|---|
| `6520` | MCP over HTTP — すべての AI クライアントはここに接続 |
| `6510` | レガシー TCP ブリッジ（現在のクライアントは使用しない） |
| `6511` | 任意の Pro 内 Python ブリッジ（`execute_arcpy_code`） |

### 🟣 Claude Code

```powershell
claude mcp add --transport http arcgis http://127.0.0.1:6520/mcp
claude mcp list
```

`~/.claude.json` での手動の等価構成：

```json
{ "mcpServers": { "arcgis": { "type": "http", "url": "http://127.0.0.1:6520/mcp" } } }
```

削除は `claude mcp remove arcgis`、再追加は同じコマンドで。

### 🖥️ Claude Desktop（同梱の stdio ブリッジ）

Claude Desktop は HTTP サーバーへ発信できず、ローカルプロセスを起動します。同梱の PowerShell ブリッジ（Python 不要）を使ってください：

1. **Claude Desktop を完全に終了**（トレイアイコン含む）。
2. `install/02-configure-claude.cmd` を実行するか、設定を手動編集（ファイルは自動でバックアップされます）。
3. 設定ファイルは次のうち最初に存在するフォルダにあります：
   `%APPDATA%\Claude`、`%APPDATA%\Claude-3p`、`%LOCALAPPDATA%\Claude-3p`、`%LOCALAPPDATA%\Claude` — ファイル名 `claude_desktop_config.json`。

```json
{
  "mcpServers": {
    "arcgis": {
      "command": "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe",
      "args": ["-NoProfile", "-ExecutionPolicy", "Bypass", "-File",
               "C:\\Users\\<you>\\AppData\\Local\\ArcGIS Pro MCP\\stdio-bridge.ps1"]
    }
  }
}
```

4. Claude Desktop を再度開くと、ツール一覧に `arcgis` が表示されます。

### 🟢 Codex

`~/.codex/config.toml` に追加（古い `arcgis` セクションがあれば置き換え）：

```toml
[mcp_servers.arcgis]
url = "http://127.0.0.1:6520/mcp"
```

Codex を再起動します。

### 🔧 その他の MCP クライアント

`http://127.0.0.1:6520/mcp` への Streamable HTTP、または上記の汎用 JSON を使用します。（Antigravity は `serverUrl` を要求し、`type` は不要です。）

### ✅ 接続の確認

- `install/01-check.cmd` — Pro のバージョン、add-in ハッシュ、ランタイム、ポート、クライアント設定をチェック。
- `netstat -ano | findstr 6520` — ポートが待ち受けていることを確認。
- ログ：`%LOCALAPPDATA%\ArcGIS-MCP\addin.log`。

---

## 🎯 使い方

**ループ：** データのあるプロジェクトを開く → やりたいことを言う → アシスタントが適切なツールを選ぶ → 結果を確認（画像はインラインで返ります）。🎨

| こう言う | 起きること |
|---|---|
| 「郡を面積で色分けして、パステルレッドからグリーンに」 | `set_layer_renderer`（段彩） |
| 「人口 50 万以上の郡を選択してエクスポートして」 | `select_features` → `export_features` |
| 「道路を 500 m バッファして」 | `run_geoprocessing_tool`（Buffer） |
| 「各乡镇の耕地はどれくらい？」 | `summarize_features` / `zonal_statistics` |
| 「現在の地図ビューを見せて」 | `export_map_view`（PNG を返す） |
| 「タイトル・凡例・縮尺バー・北矢印付きの A3 縦 PDF を作って」 | `create_layout` → `add_map_frame` → `add_layout_*` → `export_layout` |
| 「12 レイヤをすべて同じシンボルでエクスポートして」 | `run_batch` または `execute_arcpy_code` |

**ツールレベルの使い方。** 下のカタログにあるすべてのツールは、クライアントから名前で呼び出せます。`run_batch` は多数のコマンドを 1 往復で連鎖させ、`execute_arcpy_code` は専用ツールがカバーしない作業のための万能の逃げ道です。

**編集ルール。** 編集は `save_edits` まで保留されます — 保留中の編集はデータをロックするため、保存するまでデータセットの削除や上書きは失敗します。`discard_edits` で破棄できます。`delete_features` / `delete_dataset` は元に戻せません：先にアシスタントへ影響範囲を述べさせてください。

**オプションコンポーネントのオンデマンド利用。**
- 🐍 RunCode ツールボックス：`%LOCALAPPDATA%\ArcGIS Pro MCP\runcode\RunCode.pyt` — Pro セッションごとの初回実行で信頼ダイアログが出ます。先にガードを起動してください。
- 🔌 Python ブリッジ：Pro の Python ウィンドウに貼り付け（`sys.path.insert(0, r"...\ArcGIS Pro MCP\plugin")`、続いて `mcp_bridge.start_server(port=6511)`）；`CURRENT` には `mcp_bridge.start_pump()` を追加。
- 🛡️ ダイアログガード：`install\03-dialog-guard.cmd`（600 秒）、`-AlertOnly`、`-NotifyTest`。

---

## 🎨 マッピングと地図作成 — プロンプトガイド

このセクションは *アシスタントへの話しかけ方* についてです。ツール名を挙げる必要はありません：下の 4 要素を述べれば、アシスタントが MCP 呼び出しに変換します。

### この 4 つを言えば 1 パスで完了

| 要素 | 述べること | 例 |
|---|---|---|
| データ | レイヤ名、データセットパス、現在の選択 | 「現在の地図の `counties` レイヤ」 |
| 操作 | 分類フィールド/方法、バッファ距離、統計 | 「`AREA` で 5 クラス、自然分類」 |
| スタイル | 配色、フォントサイズ、凡例/縮尺バー/北矢印の配置、ベースマップ | 「パステルレッドからグリーン、凡例は右下」 |
| 出力 | 画像/PDF/新フィーチャクラス、解像度、保存先 | 「A3 横 PDF、300 dpi、`D:\out\` に保存」 |

**良いプロンプトと悪いプロンプト**
- ❌ 「地図をもっときれいにして。」 — 目標がなく、アシスタントは推測するしかありません。
- ✅ 「`landuse` を `DLMC` の値別にシンボル化し、配色はテンプレート `tpl-xxx` を参照、凡例は右下、その後 A3 横 PDF を 300 dpi で `D:\out\landuse.pdf` にエクスポートして。」
- ❌ 「地図を作って。」 — 範囲、レイアウト、出力がありません。
- ✅ 「`study_area` レイヤの範囲で 1:10,000、A4 縦 PNG、タイトル『対象区域の土地利用』、凡例・縮尺バー・北矢印付き。まずプレビューを送って。」

### ツールグループ別プロンプト早見表

| モジュール（ツールグループ） | こんなふうに言う | アシスタントが使うもの |
|---|---|---|
| セッションとプロジェクト | 「まず `diagnose` を実行して、それからプロジェクトを `D:\proj\result.aprx` として保存して」 | diagnose / save_project |
| レイヤ | 「`roads` を `basemap` グループに移動、透過 40%、1:50,000 以細でのみ表示；`POI` はオフ」 | move_layer / set_layer_transparency / set_layer_scale_range / set_layer_visibility |
| データと属性 | 「`parcels` の最初の 20 行を面積順で読んで；`DLMC` のクラス別面積を集計して」 | get_features / summarize_features |
| 選択 | 「`POP` が 50 万超の郡を選択して `D:\out\counties_big.shp` にエクスポート；次に `study_area` 内の地番を選択して」 | select_features / select_by_location / export_features |
| スキーマとデータセット | 「`parcels` に DOUBLE フィールド `AREA_HA` を追加して `!Shape_Area!/10000` で計算；EPSG:4547 でポリゴンフィーチャクラス `result_2026` を作って」 | add_field / calculate_field / create_feature_class |
| ジオプロセシング | 「道路を 500 m バッファしてディゾルブ；`study_area` をマスクに土地利用ラスタをクリップ」 | run_geoprocessing_tool / run_batch |
| シンボロジ | 「面積で段彩、5 つの自然分類、パステルレッドからグリーン；または `TYPE` の値別」 | set_layer_renderer |
| 地図ビュー | 「`study_area` に 1:10,000 でズームして 1600×900 の PNG を送って」 | set_map_view / zoom_to_layer / export_map_view |
| レイアウトとエクスポート | 「A3 横レイアウトを作成、タイトル…、凡例は 3 レイヤのみ、縮尺バーと北矢印、300 dpi PDF でエクスポート」 | create_layout / add_layout_* / export_layout |
| ラスタ | 「DEM のバンド、セルサイズ、統計を読んで；NDVI を計算して」 | get_raster_info / raster_calculator / zonal_statistics |
| カタログとデータ探索 | 「プロジェクトとフォルダ接続から、名前に `2025` を含むシェープファイルを探して」 | search_data / describe_dataset / list_folder |
| ユーティリティ | 「一気にやって：ベースマップを隠す → 郡へズーム → PNG エクスポート；arcpy で全レイヤ名に接頭辞を付けて」 | run_batch / execute_arcpy_code |

### 標準の地図作成プロンプト（コピーして記入）

```text
現在のプロジェクトの <地図名> から地図を作成してください：
- 範囲：<対象区域レイヤ/範囲>、縮尺 1:<...>
- データ：<レイヤ1>、<レイヤ2>（必要に応じてフィールドとフィルタを追加）
- シンボル：<段彩/値別/単一シンボル>、配色 <ランプまたは hex>
- レイアウト：<A3 横 / A4 縦>、タイトル「<タイトル>」、凡例 <位置>、縮尺バー <位置>、北矢印 <位置>
- 出力：<PDF / PNG>、<300> dpi、<絶対パス> に保存
- 進め方：レイアウトをセルフチェックし、プレビューを送ってください。私が確認してから最終ファイルをエクスポートしてください。
```

### バッチエクスポートのプロンプト

```text
`maps` のすべての地図について、同じ A3 横レイアウトで PDF をエクスポートしてください：
タイトル = 地図名、凡例 = 表示中のレイヤのみ、縮尺バーと北矢印は右下、
`D:\out\maps\` に 300 dpi で出力。まず 1 枚をエクスポートして確認させてから、残りをバッチ処理してください。
```

### 何かおかしいときの一言修正

- 「凡例が地図を覆っている」→ `legend` ボックスかレイアウトを調整。
- 「ページ比率がおかしい / 余白が多すぎる」→ 地図フレームとページ比率を調整。
- 「色が派手すぎる / 論文スタイルでない」→ 配色ランプ、hex コード、テンプレート ID を指定。
- 「まだエクスポートしないで — プレビューを見せて」→ まず `export_map_view` かレイアウトプレビュー。

**効果：** データ、操作、スタイル、出力を最初に述べれば、通常 1 パスでほぼ最終版の地図が得られます。その後の調整は差分だけ（「凡例を右に 1 cm 動かして」）で済み、ブリーフ全体を繰り返す必要はありません。

---

## 🛡️ ガードモジュール — プロンプトガイド

**解決する問題：** ArcGIS Pro がサードパーティ製 `.pyt` / Python ツール（RunCode.pyt など）を初めて実行するとき、「サードパーティ コード ライブラリ」の信頼ダイアログが表示されます。無人作業はそこで止まります。ガードは、それを出す可能性のある操作の前にだけ召喚され、「是(Y)」をクリックし、操作が落ち着くとすぐに終了します。

**2 つのトリガーモード**
1. **自動（2.x の既定）：** `run_python_toolbox_tool` / `execute_arcpy_code` の前に add-in がガードを arm し、終了後に settle します — 通常はこちらからの操作は不要です。
2. **手動 `guard_summon`：** Pro で手動実行する `.pyt` ツール、ENVI→Pro インポート、その他アシスタント外の操作に。

**プロンプト例**

| こう言う | アシスタントの動作 |
|---|---|
| 「これから Pro で手動で RunCode.pyt を実行する。ガードを 10 分間召喚して。サードパーティ信頼ダイアログが出たら『是』をクリックし、できないときは最前面で音付き警告して」 | `guard_summon`（timeout_s≈600） |
| 「今回だけ警告のみ — クリックしないで」 | `guard_summon`（alert_only） |
| 「直前のガードセッションが正常に終了したか確認して」 | `guard_status` |
| 「RunCode 実行前にガードを arm して」 | 自動 arm + settle |

**境界とパラメータ**
- クリックするのは「サードパーティ コード ライブラリ」の信頼ダイアログのみ。上書き、削除、その他の確認は一切触れません。
- ボタンが見つからない場合、最前面アラートが少なくとも 30 秒表示され、10 秒ごとに鳴ります。
- `timeout_s` はガードの生存時間を制御します；`guard_status` はエンジンのパス/バージョンと直近 20 セッションを報告します。
- 信頼は Pro プロセスごとに記憶されます。Pro を再起動するとダイアログが再び出ることがあります。
- 1.1.4 には `guard_summon` がありません；同梱の `install\03-dialog-guard.cmd`（既定 600 秒）で同じガードを利用できます。

**効果：** 無人 RunCode / ENVI 連携ワークフローがダイアログで止まることがなくなります。自動クリック失敗時も最前面アラートと音があるため、静かに失敗することはありません。

---

## 🧪 蒸留モジュール — プロンプトガイド

ここでいう「蒸留」は**モデル学習ではありません**。**サンプル図**（論文の図、教科書の図、模範的な地図）の版式知識 — フレームのジオメトリ、配色、フォントサイズ、凡例/縮尺バー/北矢印の配置、既知の落とし穴 — を、後続の地図作成ジョブが従える再利用可能な JSON テンプレート spec として抽出するものです。

**得られるもの：** `template_id` と、`canvas`（サイズ/DPI）、`map_frame`、`title / legend / scale_bar / north_arrow`、`palette`、`typography`、`regions[]`、`pitfalls[]`、`measured`（測定証跡）、`review`（人間の承認）を含む spec。ライブラリの既定の場所は `%LOCALAPPDATA%\ArcGIS Pro MCP\library` です。

**効果**
- 同じプロジェクトや論文の多数の図で版式が一貫します。
- 凡例の位置、タイトルのサイズ、配色について毎回口頭で説明する必要がありません。
- 監査可能：各テンプレートには測定の出所、patch 内容、レビュアーと日付が残ります。
- 回避可能なミスが減る：`pitfalls[]` が「凡例が地図を覆う」のような問題を実行可能なチェックに変えます。

**ワークフローとプロンプト**

| ステップ | こう言う | アシスタントの呼び出し | あなたが確認するもの |
|---|---|---|---|
| 1 インポート | 「`D:\samples\fig3.png` を『対象区域概況』という名前のテンプレートに蒸留して。タグ：journal-style」 | `template_import` | 返ってきた `template_id` |
| 2 測定 | 「測定して」 | `template_measure` | `h_lines/v_lines`、`palette_top`、DPI |
| 3 セマンティクス | アシスタントが図を読み、patch（フレーム、凡例、フォント、regions、pitfalls）を書く | `template_patch` | patch 内の座標とフォントサイズ |
| 4 検証 | 「検証して」 | `template_validate` | エラー 0；warning は許容範囲か |
| 5 プレビュー | 「プレビューを見せて」 | `template_preview` | サンプルとの並列ワイヤーフレーム |
| 6 レビュー / 承認 | 「確認した。レビュアー Liu Haowen、メモ：版式チェック済み」→「承認して」 | `template_review` → `template_approve` | ステータスが `ready` に |
| 7 適用 | 「テンプレート `tpl-xxx` でこのプロジェクトの地図を作って」 | `template_apply` → マッピングツール | 出力がテンプレート版式に一致 |

**コピー＆ペースト用プロンプト**

1. **単一図の完全蒸留**

```text
`D:\samples\fig3.png` を版式テンプレートに蒸留してください：
- 名前：対象区域概況；タグ：journal-style、A4 横
- フルフローを実行：import → measure → patch → validate → preview
- セマンティックフィールド（フレーム、凡例、縮尺バー、北矢印、フォントサイズ、配色、regions、pitfalls）は図を読んで判断してください
- 各ステップの要点を報告し、プレビューを見せて私の OK を待ってから review + approve
- 測定値を手で編集しないこと
```

2. **バッチ取り込み（最初は測定のみ）**

```text
`D:\samples\` 配下のすべての PNG をスキャンし、1 枚ずつ import + measure してください。名前はファイル名、
すべてに `candidate` タグを付ける。セマンティック patch はまだ書かないこと；測定サマリー表を
出して、私が精修する対象を選べるようにしてください。
```

3. **テンプレートで地図を作る**

```text
テンプレート `tpl-20261116-172854-57ec` で現在のプロジェクトに地図を作ってください：
まず template_apply を呼び、map_frame/palette/typography/pitfalls に従って
タイトル「対象区域の土地利用」の A4 横レイアウトを作り、300 dpi PDF を `D:\out\landuse.pdf` にエクスポート。
セルフチェック結果とプレビューを送って、受入確認をさせてください。
```

4. **ready テンプレートを編集する**

```text
テンプレート `tpl-xxx` の凡例を右下（x=0.78, y=0.78, w=0.16, h=0.16）に移し、
タイトルのサイズを 22 に。validate と preview を再実行して結果を見せてください；measured には触れないこと。
私が確認したら、再度 review + approve してください。
```

5. **テンプレートの移行 / 共有**

```text
`tpl-xxx` を bundle.zip としてエクスポートし、別マシンのテンプレートライブラリに
bundle-import して、新しい template_id とステータスを一覧してください。
```

**アシスタントが守るべきハードルール**
- セマンティックな判読は [skills/template-distill/SKILL.md](skills/template-distill/SKILL.md) に従います；決定論的ステップを「即興」で行いません。
- `measured / id / image_sha256 / source_image / schema_version` は保護フィールドで、含む patch は拒否されます。キャンバスを変更するには `measure --force` を再実行するか、`canvas.height_in` を 3% の許容範囲内で合わせます。
- `ready` には検証と人間によるレビューの両方が必要です；レビューなしの `approve` は拒否されます。`--force` はあなたの明示的な要求時のみ使用され、レビューに記録されます。
- プレビューはあなたに提示し、サンプルと照合したうえでのみ承認できます。
- 論文/教科書のサンプル図は既定でローカルライブラリに留めます — 公開 bundle をエクスポートしないでください（著作権）。

**FAQ**
- **プレビューが失敗 / 画像が出ない：** この v2.0.0 ブランチには `run-distill.ps1` の `--out` バインディングバグがあります；v2.0.1 を使うか、`python -m arcgis_agentkit.distill preview` を直接呼んでください。
- **図のバンドが検出されない：** サンプルが暗すぎる/ぼやけている、または明るいバンドがない；クリーンな全体スクリーンショットを使ってください。
- **3% キャンバス許容エラー：** `canvas.height_in` が測定値と一致していません；キャンバスを修正するか再測定してください — `measured` を手で編集しないこと。
- **`ready` テンプレートは変更できない：** spec をコピーするか新しい id で再インポートしてください。納品済みテンプレートを壊さないように。

---

---

## 🧰 全 125 個の MCP ツール（14 グループ）

アシスタントにできることすべてを名前で列挙します。クリックで展開 👇

<details>
<summary><b>📜 完全なツールカタログ — セッション 15 · レイヤ 19 · データ 11 · 選択 6 · スキーマ 11 · ジオプロセシング 7 · シンボロジ 6 · ビュー 7 · レイアウト 16 · ラスタ 5 · カタログ 6 · ユーティリティ 4 · ガード 2 · テンプレート 10</b></summary>

### セッション（15）

| ツール | 役割 |
|---|---|
| `ping` | ArcGIS Pro ブリッジに到達できるか確認し、どのプロジェクトに接続しているかを表示します。 |
| `get_capabilities` | 接続中のブリッジがサポートするすべてのコマンドをエリア別に一覧します。名前付きツール以外の機能の発見に使います。 |
| `get_queue_status` | ブリッジのキュー状態：レーン別の待ち数、実行中のコマンド、最近の完了履歴。呼び出しが遅い、キューされている、「lane busy」エラーになるときに確認します。 |
| `diagnose` | 接続のセルフチェック：ArcGIS Pro のバージョン、ライセンス、開いているプロジェクト、アクティブ地図、開いている地図ビュー、書き込み権限。うまく動かないときは最初に実行します。 |
| `get_arcgis_info` | ArcGIS Pro のバージョン、ライセンスレベル、利用可能なエクステンション、現在のプロジェクトパス。 |
| `get_project_info` | プロジェクトパス、既定のジオデータベースとツールボックス、地図、レイアウト、フォルダ・データベース接続。 |
| `save_project` | ArcGIS Pro プロジェクト（.aprx）を保存、または別の場所にコピー保存します。 |
| `list_maps` | すべての地図とシーンを座標系・レイヤ数付きで一覧します。 |
| `create_map` | プロジェクトに新しい地図またはシーンを作成します。 |
| `remove_map` | プロジェクトから地図を削除します。 |
| `activate_map` | 地図ビューを ArcGIS Pro UI で開く/アクティブ化します。カメラ系コマンドと export_map_view の前に必要です。 |
| `set_map_properties` | 地図の名前変更や座標系の変更を行います。 |
| `get_map_extent` | 地図内の全データレイヤの合算範囲。 |
| `get_environment` | 現在の arcpy ジオプロセシング環境設定を読み取ります。 |
| `set_environment` | ワークスペース、outputCoordinateSystem、範囲、マスク、cellSize、overwriteOutput、parallelProcessingFactor などの arcpy ジオプロセシング環境を設定します。 |

### レイヤ（19）

| ツール | 役割 |
|---|---|
| `get_layers` | 地図のレイヤ（描画順とグループ階層付き）とスタンドアロンテーブルを一覧します。プロジェクトの中身を見るならここから。 |
| `get_layer_info` | 1 レイヤの詳細：データソース、座標系、範囲、フィールド、フィーチャ数、レンダラー、ラベル状態。 |
| `add_layer` | パスまたはサービス URL からデータを地図に追加：フィーチャクラス、シェープファイル、ラスタ、テーブル、.lyrx レイヤファイル、Web サービス。 |
| `add_web_layer` | URL で Web サービスレイヤを追加（Feature/Map/Image サービス、WMS、WMTS、ベクタタイル）。 |
| `remove_layer` | 地図からレイヤまたはスタンドアロンテーブルを削除します。 |
| `rename_layer` | コンテンツのレイヤ名を変更します。 |
| `duplicate_layer` | 地図内でレイヤを複製し、別のシンボルを割り当てられるようにします。 |
| `set_layer_visibility` | 1 つのレイヤ、複数、または地図の全レイヤを表示/非表示します。 |
| `set_layer_transparency` | レイヤの透過率を設定します（0 = 不透明、100 = 完全透明）。 |
| `set_layer_scale_range` | レイヤが描画される縮尺範囲を制限します。 |
| `set_definition_query` | レイヤの定義クエリ（SQL where 句）を設定します。空文字列で解除。 |
| `move_layer` | レイヤを他のレイヤに対して並べ替える、またはグループレイヤへ移動します。 |
| `create_group_layer` | グループレイヤを作成し、必要に応じて既存レイヤをその中へ移動します。 |
| `zoom_to_layer` | 地図ビューをレイヤの範囲へズームします。 |
| `set_basemap` | ベースマップを設定：Topographic、Imagery、Imagery Hybrid、Streets、Navigation、Light Gray Canvas、Dark Gray Canvas、Terrain、Oceans、OpenStreetMap、National Geographic Style Map。 |
| `get_broken_layers` | 全地図の中でデータソースが欠けているレイヤを一覧します。 |
| `repair_layer_source` | 壊れたデータソースを修復するため、レイヤを新しいワークスペースやデータセットに向け直します。 |
| `add_join` | 共通フィールドでテーブルをレイヤに結合します。 |
| `remove_join` | レイヤの結合を外します。 |

### データ（11）

| ツール | 役割 |
|---|---|
| `get_features` | レイヤ、テーブル、データセットパスから属性行を読み取ります。where 句、フィールド部分指定、並べ替え、WKT ジオメトリを任意で指定。 |
| `count_features` | フィーチャ数をカウントします。where 句の一致条件も可能。 |
| `get_unique_values` | フィールドの一意値と各行数を返します。 |
| `get_field_statistics` | 数値フィールドの min / max / 平均 / 中央値 / 合計 / 標準偏差。 |
| `summarize_features` | 1 つ以上のフィールドで行をグループ化して集計 — 「カテゴリ別にいくつ/どれだけ」をジオプロセシングなしで素早く答える方法。 |
| `insert_features` | レイヤまたはテーブルに新しい行を挿入します。 |
| `update_features` | where 句に一致する行の属性やジオメトリを更新します。 |
| `delete_features` | where 句に一致する行を削除します。 |
| `save_edits` | 保留中の編集を確定します。ArcGIS Pro は編集を取り消せるようオープンに保つため、保存までデータもロックされます — それまでは編集済みデータセットの削除や上書きは失敗します。 |
| `discard_edits` | 保留中の編集を破棄します。 |
| `calculate_field` | レイヤ全体のフィールド値を計算します。例：式 "!Shape_Area! / 10000"。 |

### 選択（6）

| ツール | 役割 |
|---|---|
| `select_features` | SQL where 句でフィーチャを選択します。 |
| `select_by_location` | 別レイヤとの空間関係でフィーチャを選択します。 |
| `get_selection` | 現在の選択状態をレイヤごとに ObjectID 付き（必要なら属性行も）で報告します。 |
| `set_selection` | ObjectID で特定のフィーチャを選択します。 |
| `clear_selection` | 1 つのレイヤ、または地図のすべてのレイヤの選択を解除します。 |
| `zoom_to_selection` | 地図ビューを現在選択中のフィーチャへズームします。 |

### スキーマ（11）

| ツール | 役割 |
|---|---|
| `list_fields` | レイヤのフィールドを型、別名、長さ、ドメイン付きで一覧します。 |
| `add_field` | レイヤまたはテーブルにフィールドを追加します。 |
| `add_fields` | 複数のフィールドを 1 回の呼び出しで追加します。 |
| `delete_field` | 1 つ以上のフィールドを削除します。 |
| `alter_field` | フィールドの名前変更、別名/長さの変更を行います。 |
| `create_feature_class` | 空のフィーチャクラスを作成し（既定ではプロジェクトの既定ジオデータベース内）、地図に追加します。 |
| `create_table` | 空のスタンドアロンテーブルを作成します。 |
| `create_file_geodatabase` | 新しいファイルジオデータベースを作成します。 |
| `truncate_table` | スキーマを保ったまま、テーブルまたはフィーチャクラスの全行を削除します。 |
| `delete_dataset` | ディスクまたはジオデータベースからデータセットを削除します。元に戻せません。 |
| `export_features` | レイヤを新しいデータセットにエクスポートします — 現在の選択と定義クエリが反映されます。 |

### ジオプロセシング（7）

| ツール | 役割 |
|---|---|
| `run_geoprocessing_tool` | 任意の arcpy ジオプロセシングツールを実行 — 分析のための万能の逃げ道。レイヤ名は開いているレイヤに解決され、選択と定義クエリが適用されます。パラメータ名がわからない場合は先に describe_geoprocessing_tool を呼びます。 |
| `list_geoprocessing_tools` | 利用可能なジオプロセシングツールを名前で検索します。 |
| `list_toolboxes` | arcpy のツールボックスモジュールとプロジェクト内のツールボックスを一覧します。 |
| `describe_geoprocessing_tool` | 実行前にツールのパラメータ、データ型、既定値、使用説明を取得します。 |
| `run_python_toolbox_tool` | ディスク上のカスタム .pyt / .atbx / .tbx ツールボックスからツールを実行します。 |
| `check_extension` | ArcGIS エクステンションライセンスを確認し、必要に応じてチェックアウトします。 |
| `get_messages` | 直近のジオプロセシング操作のメッセージ。 |

### シンボロジ（6）

| ツール | 役割 |
|---|---|
| `set_layer_renderer` | レイヤのシンボルを変更：単一シンボル、カテゴリ別の値別、数値フィールドによる分類/連続配色。 |
| `get_layer_symbology` | レイヤの現在のレンダラー、クラス区切り、一意値、ラベル設定を確認します。 |
| `list_color_ramps` | プロジェクトで利用可能なカラーランプを一覧します。 |
| `set_layer_labeling` | ラベルをオン/オフし、式、フォント、ハローを設定します。 |
| `apply_symbology_from_layer` | .lyrx ファイルまたは別のレイヤからシンボルをコピーします。 |
| `save_layer_file` | シンボル付きのレイヤを .lyrx ファイルに保存して再利用します。 |

### ビュー（7）

| ツール | 役割 |
|---|---|
| `get_map_view` | 現在のカメラ位置：中心、縮尺、回転、可視範囲。ArcGIS Pro で地図のタブが開いている必要があります。 |
| `set_map_view` | 地図ビューを移動：範囲、中心点、縮尺、回転を設定します。 |
| `export_map_view` | 地図ビューを PNG にレンダリングし、目視できるよう画像を返します — 地図を視覚チェックする方法。ArcGIS Pro で地図のタブが開いている必要があります。 |
| `list_bookmarks` | 地図に定義された空間ブックマークを一覧します。 |
| `create_bookmark` | 現在のビューまたは指定範囲を名前付きブックマークとして保存します。 |
| `apply_bookmark` | 地図ビューをブックマークへズームします。 |
| `delete_bookmark` | 地図からブックマークを削除します。 |

### レイアウト（16）

| ツール | 役割 |
|---|---|
| `list_layouts` | 印刷レイアウトをページサイズと要素数付きで一覧します。 |
| `get_layout_info` | レイアウトを検査：ページ設定と全要素の位置・サイズ。 |
| `create_layout` | 新しいレイアウトページを作成します（既定ではページを満たす地図フレーム付き）。 |
| `delete_layout` | プロジェクトからレイアウトを削除します。 |
| `add_map_frame` | レイアウトのページ位置に地図フレームを追加します。 |
| `set_map_frame_extent` | レイアウトの地図フレームをレイヤ、範囲、縮尺のいずれかに合わせます。 |
| `add_layout_text` | タイトルや出典などのテキスト要素をレイアウトに追加します。 |
| `add_layout_legend` | レイアウトの地図フレームに連動した凡例を追加します。 |
| `add_layout_scale_bar` | レイアウトの地図フレームに連動した縮尺バーを追加します。 |
| `add_layout_north_arrow` | レイアウトの地図フレームに連動した北矢印を追加します。 |
| `add_layout_picture` | ロゴなどの画像をレイアウトに配置します。 |
| `set_layout_element` | 任意のレイアウト要素の移動、サイズ変更、名前変更、非表示、テキスト変更を行います。 |
| `delete_layout_element` | レイアウトから要素を削除します。 |
| `export_layout` | レイアウトを PDF / PNG / JPEG / SVG / TIFF にエクスポートします。PNG と JPEG は確認できるようインライン画像として返ります。 |
| `preview_layout` | ファイルを書かずにレイアウトを一時画像にレンダリングして返します — 構築中の視覚チェックに。 |
| `export_map_series` | レイアウトのマップシリーズ（マップブック）を複数ページ PDF にエクスポートします。 |

### ラスタ（5）

| ツール | 役割 |
|---|---|
| `get_raster_info` | ラスタの詳細：バンド、サイズ、セルサイズ、ピクセルタイプ、統計、CRS。 |
| `set_raster_symbology` | ラスタレイヤのカラーライザーとカラーランプを設定します。 |
| `raster_calculator` | マップ代数。ラスタを短い名前で束縛し、それらに対する Python 式を書きます。Spatial Analyst エクステンションが必要です。 |
| `sample_raster_values` | 地図座標またはポイントレイヤのフィーチャ位置でラスタのセル値を読み取ります。 |
| `zonal_statistics` | ゾーンポリゴン内のラスタ値を集計し、テーブルを返します。Spatial Analyst エクステンションが必要です。 |

### カタログ（6）

| ツール | 役割 |
|---|---|
| `list_workspace_contents` | ジオデータベースまたはフォルダ内のデータセットを一覧します。既定はプロジェクトの既定ジオデータベース。 |
| `list_folder` | ディスク上の GIS ファイルとサブフォルダを一覧します。既定はプロジェクトのホームフォルダ。 |
| `describe_dataset` | 任意のデータセットをパスで記述 — 種類、ジオメトリ、CRS、範囲、フィールド、行数 — 地図に追加せずに。 |
| `search_data` | プロジェクトのジオデータベース、ホームフォルダ、フォルダ接続から名前でデータセットを検索。追加前にデータを特定する最速の方法です。 |
| `get_project_items` | プロジェクトに登録されたフォルダ接続、データベース接続、ツールボックス。 |
| `add_folder_connection` | フォルダをプロジェクトに登録して、データを閲覧しやすくします。 |

### ユーティリティ（4）

| ツール | 役割 |
|---|---|
| `execute_arcpy_code` | ArcGIS Pro 内で Python を実行します。arcpy はインポート済みで、arcpy.mp.ArcGISProject('CURRENT') が開いているプロジェクト。変数は呼び出し間で保持され、print() 出力は取得され、最後の式の値が返ります。専用ツールがカバーしない作業はこれを使います。 |
| `get_pump_status` | メインスレッドディスパッチャーが導入済みかどうか。コマンドを開いている ArcGIS Pro プロジェクトに届かせる仕組みで、これがないとパスベースの作業（ジオプロセシング、データセット検査）だけが動きます。 |
| `stop_pump` | メインスレッドディスパッチャーを削除します。ユーザーが再導入するまでライブプロジェクト系コマンドは動かなくなります。 |
| `run_batch` | 複数のコマンドを 1 往復で実行 — 多段ワークフローが大幅に速くなります。 |

### ガード（2）

| ツール | 役割 |
|---|---|
| `guard_summon` | 手動ステップ用にオンデマンドのダイアログガードを召喚します。サードパーティ コード ライブラリの信頼ダイアログだけをクリックし、できない場合は最前面アラートが 30 秒以上表示されます。`timeout_s` 後に終了します。 |
| `guard_status` | ガードエンジンのパス/バージョン、セッションルート、直近 20 回の召喚セッション。 |

### テンプレート（10）

| ツール | 役割 |
|---|---|
| `template_list` | 蒸留された版式テンプレートを一覧します。ステータスやタグで絞り込み可能。 |
| `template_get` | テンプレート spec を 1 つ読みます（版式、配色、フォント、regions、pitfalls、レビュー）。 |
| `template_import` | サンプル図を下書きとしてインポートします（sha256 で重複排除）。 |
| `template_measure` | 図の bbox、アスペクト比、フレーム線、配色、DPI を測定します。 |
| `template_patch` | セマンティックフィールドを検証付きマージとして送信します。保護フィールドは拒否されます。 |
| `template_validate` | スキーマ + 幾何的一貫性チェック。 |
| `template_review` | 人間のレビュー判断を記録します。 |
| `template_approve` | レビュー済み・検証済みテンプレートを承認します（ready に）。 |
| `template_preview` | ワイヤーフレームプレビューを画像として返します。 |
| `template_apply` | ready テンプレートをマッピングコンテキストファイルに書き出します。 |

</details>

---

## 🗂️ リポジトリ構成

```
├── main (this branch) 🏠      AgentKit 2.0.1 preview: MCP add-in 2.0.1 + guard + distill
├── customer-v2.0.0 🎁         v2.0.0/v2.0.1 preview release branch (merged into main)
├── customer-v1.1.4 🎁         previous customer edition (add-in 1.1.4), preserved
├── history-main-pre-2.0 🗄️    main before the 2.0 merge, preserved as history
├── installer/ 🛺              ArcGISProMCP.Install — NuGet tool: download → verify → run setup
├── addin/ArcGISProMCP/ 🧩     C# add-in (125 tools, GuardClient, template commands)
├── src/arcgis_pro_mcp/ 🐍     upstream Python stdio/CLI server (kept for rebase)
├── src/arcgis_agentkit/ 🧰    guard / distill Python packages (CLI + engine)
├── skills/ 🎓                 assistant-facing skills (template-distill, arcgis-pro-mapping)
├── library/ 📚                template library schema (v1.1) and examples
├── docs/ 📖                   design, architecture, guard and distill notes
└── tests/ 🧪                  pytest (guard handshake/rules, full distill pipeline)
```

開発用インストール：

```powershell
python -m pip install -e .
arcgis-guard doctor
arcgis-distill doctor
```

## 🛣️ ロードマップ

1. ✅ **M1 — Guard 2.0**：召喚プロトコル、イベント優先の UIA 検出、add-in の自動 arm、ルールスキーマ、CLI、テスト。
2. ✅ **M2 — Distill 2.0**：import / measure / patch / validate / preview / review / approve / apply、schema v1.1、テンプレート MCP ツール。
3. ✅ **M3 — リリース**：2.0.0/2.0.1 のバージョニング、タグ、リリース資産、NuGet パッケージの出荷；コード署名とクリーン VM での通し検証が残っています。

詳細、受入基準、トレードオフ：[docs/design-2026-11-16.md](docs/design-2026-11-16.md)。2.0 が到着しました (๑•̀ㅂ•́)و✧

---

## 📜 ライセンス・クレジット・免責事項

- ⚖️ **ライセンス**：[AGPL-3.0-or-later](LICENSE)。MCP モジュールは AGPL-3.0-or-later プロジェクトに由来するため、統合リポジトリも AGPL です。
- 🙏 **上流プロジェクトと原作者**：[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP) by **Pisut Nakmuenwai** — ありがとうございます！💐 ローカル変更は [NOTICE](NOTICE)、[RELEASE-NOTES.md](https://github.com/XNYGggg/arcgis-pro-mcp-optimized/blob/customer-v2.0.0/RELEASE-NOTES.md)、カスタマーパッケージ内の `source/SOURCE-NOTICE.md` で宣言されています。
- 🏷️ **商標**：Esri とは無関係です。"ArcGIS" は Esri の商標です。サードパーティ製ツールであり、いかなる保証もありません。
- 🔒 **プライバシー / ネットワーク**：すべてのサービスは `127.0.0.1` にのみバインドされます；テレメトリゼロ、クラウド呼び出しゼロ。
- 👤 **メンテナー**：Law（[@XNYGggg](https://github.com/XNYGggg)）。

[🇨🇳 简体中文说明](README.zh-CN.md) · Made with ☕ + 🗺️ + ちょっとした完璧主義
