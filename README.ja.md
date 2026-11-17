[![English](https://img.shields.io/badge/Language-English-1f6feb?style=flat-square)](README.md) [![简体中文](https://img.shields.io/badge/Language-%E7%AE%80%E4%BD%93%E4%B8%AD%E6%96%87-d1242f?style=flat-square)](README.zh-CN.md) [![日本語](https://img.shields.io/badge/Language-%E6%97%A5%E6%9C%AC%E8%AA%9E-bc002d?style=flat-square)](README.ja.md) [![한국어](https://img.shields.io/badge/Language-%ED%95%9C%EA%B5%AD%EC%96%B4-0047a0?style=flat-square)](README.ko.md) [![Version](https://img.shields.io/badge/Version-1.1.4%20(stable)-2ea44f?style=flat-square)](../../releases/tag/v1.1.4) [![License: AGPL v3+](https://img.shields.io/badge/License-AGPL--3.0--or--later-blue?style=flat-square)](LICENSE) [![ArcGIS Pro](https://img.shields.io/badge/ArcGIS%20Pro-3.7%2B-00a1e0?style=flat-square)](https://www.esri.com/en-us/arcgis/products/arcgis-pro/overview) [![Platform](https://img.shields.io/badge/Platform-Windows%2011%20x64-lightgrey?style=flat-square)]() [![Release](https://img.shields.io/github/v/release/XNYGggg/arcgis-pro-mcp-optimized?style=flat-square&label=release)](../../releases)

# 🗺️ ArcGIS Pro MCP（最適化版）· カスタマーエディション v1.1.4 · 安定版（推奨）🎁

自然言語の一文だけで、AI アシスタントが今開いている ArcGIS Pro プロジェクトを直接操作できます。レイヤの追加、シンボルの変更、属性の照会、ジオプロセシングの実行、地図のエクスポートまでこなします。データがマシン外に出ることはなく、すべての通信は `127.0.0.1` の中で完結します 🔒。

> **📌 バージョン状況（2026-11-16 更新）**：v1.1.4（add-in 1.1.4、113 ツール）が**安定版・推奨**リリースです。もう一本の 2.0 系（v2.0.0 / v2.0.1）は**テスト版**です：最適化は未完成で、GitHub Release のアセットとタグは削除済み、**使用は推奨しません**（NuGet 2.0.x のダウンロードは失敗します）。2.0 のコードは `customer-v2.0.1` / `customer-v2.0.0` ブランチにのみ残っています。ブランチ・タグ・Release の対応表は [BRANCHES.md](https://github.com/XNYGggg/arcgis-pro-mcp-optimized/blob/main/BRANCHES.md) を参照してください。

> **🙏 上流プロジェクトと原作者**：本カスタマーエディションは **[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)**（原作者 **Pisut Nakmuenwai**、AGPL-3.0-or-later）を基に、ローカルパッチを **v1.1.4** まで積み上げたものです。完全な帰属表示と変更通知は、パッケージルートの `NOTICE` と `source/SOURCE-NOTICE.md` に記載しています。

本カスタマーエディションは動作に必要な**すべてのコンポーネント**を同梱しています。オフラインで動作し、**Python も pip もネットワークも不要**——ArcGIS Pro 本体以外に依存関係はありません。電池まで付いてきます 🔋

---

## 🖥️ 動作環境

| 項目 | 要件 |
|---|---|
| 🪟 OS | **Windows 11 x64**（64 ビット Home / Pro / Enterprise、ArcGIS Pro 3.7 が公式サポート）。**アーキテクチャ：x64（AMD64）のみ — ARM64 は非対応** |
| 🗺️ ArcGIS Pro | **3.7 以降**（本 add-in は 3.7 / .NET 10 向けにビルド） |
| 🔑 権限 | 標準ユーザーで可。インストールに**管理者権限は不要** |
| 📦 その他の依存 | なし（PowerShell と .NET は Windows / Pro に同梱） |

---

## 🚀 クイックスタート（5 分）

1. 📂 パッケージを任意のフォルダに**解凍**します（例：`D:\ArcGIS-Pro-MCP-customer`）。
2. 🛠️ **ArcGIS Pro を終了**してから、`install\00-install.cmd` をダブルクリックします。スクリプトが add-in のインストール、ランタイムの配置、Claude Desktop / Claude Code の設定を自動で行います。
3. 🖥️ **ArcGIS Pro を開きます**。初回読み込み時に「アドインが信頼されていません」と表示されたら「はい / 信頼する」をクリックしてください（安全です 😄）。Pro 上部の **MCP タブ**でステータスが **running**（ポート `127.0.0.1:6520`）であることを確認します。
4. 🔌 **AI クライアントを接続**し（下の「設定方法」参照）、`install\01-check.cmd` をダブルクリックしてセルフチェックを実行します。すべて `[OK]` なら準備完了です (๑•̀ㅂ•́)و✧

> セルフチェック時に ArcGIS Pro が開いていない場合、ポートチェックは警告になります — Pro を開いてからもう一度実行してください 👌

---

## 🎛️ 設定 — AI クライアントの接続

**前提**：ArcGIS Pro が開いていて、**MCP タブ**が **running** を表示していること。すべてのポートはローカルホストにのみバインドされます：

| ポート | 用途 |
|---|---|
| `6520` | HTTP 経由の MCP — すべての AI クライアントはここに接続 |
| `6510` | レガシー TCP ブリッジ（最近のクライアントは使用しない） |
| `6511` | 任意の Pro 内 Python ブリッジ（`execute_arcpy_code`） |

### 🟣 Claude Code

```powershell
claude mcp add --transport http arcgis http://127.0.0.1:6520/mcp
claude mcp list
```

手動で設定する場合（`~/.claude.json`）：

```json
{ "mcpServers": { "arcgis": { "type": "http", "url": "http://127.0.0.1:6520/mcp" } } }
```

削除は `claude mcp remove arcgis`、再追加は上記コマンドを再実行します。

### 🖥️ Claude Desktop（同梱の stdio ブリッジを使用）

Claude Desktop は HTTP で直接接続できず、ローカルプロセスの起動のみに対応します。パッケージ同梱の純 PowerShell ブリッジ（Python 不要）を使用してください：

1. **Claude Desktop を完全に終了**します（トレイアイコンも含む）。
2. `install\02-configure-claude.cmd` を実行するか、設定を手動で編集します（先にバックアップを）。
3. 設定ファイルの場所は次のうち**最初に存在する**ディレクトリです：
   `%APPDATA%\Claude`、`%APPDATA%\Claude-3p`、`%LOCALAPPDATA%\Claude-3p`、`%LOCALAPPDATA%\Claude` — ファイル名は `claude_desktop_config.json`。

```json
{
  "mcpServers": {
    "arcgis": {
      "command": "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe",
      "args": ["-NoProfile", "-ExecutionPolicy", "Bypass", "-File",
               "C:\\Users\\<your-username>\\AppData\\Local\\ArcGIS Pro MCP\\stdio-bridge.ps1"]
    }
  }
}
```

4. Claude Desktop を再起動し、ツール一覧に `arcgis` が表示されれば完了です。

### 🟢 Codex

`~/.codex/config.toml` に以下を追加します（古い `arcgis` セクションがある場合は、古い `env` サブテーブルを含めてセクションごと置き換えます）：

```toml
[mcp_servers.arcgis]
url = "http://127.0.0.1:6520/mcp"
```

保存して Codex を再起動します。

### 🔧 その他の MCP クライアント

Streamable HTTP で `http://127.0.0.1:6520/mcp` に接続するか、上記の汎用 JSON を使用します。（Antigravity は `serverUrl` を使い、`type` は不要です。）

### ✅ 接続の確認

- `install\01-check.cmd` — Pro のバージョン、add-in ハッシュ、ランタイム、ポート、クライアント設定をチェックします。
- `netstat -ano | findstr 6520` — ポートが待ち受けていることを確認します。
- ログ：`%LOCALAPPDATA%\ArcGIS-MCP\addin.log`。

---

## 🎯 使い方

**基本の流れ**：データのあるプロジェクトを開く → やりたいことを言う → アシスタントがツールを選ぶ → 結果を確認する（画像はインラインで返ってきます）🎨

| こう言えます | 実際に起きること |
|---|---|
| 「郡ごとに面積で段彩を、薄い赤から濃い緑で」 | `set_layer_renderer`（段彩） |
| 「人口 50 万以上の郡を選択して、新しいフィーチャクラスとしてエクスポートして」 | `select_features` → `export_features` |
| 「道路を 500 メートルバッファして」 | `run_geoprocessing_tool`（バッファ） |
| 「郷鎮ごとの耕地面積合計を出して」 | `summarize_features` / `zonal_statistics` |
| 「現在の地図ビューのスクリーンショットを見せて」 | `export_map_view`（PNG を返す） |
| 「タイトル・凡例・縮尺バー・北矢印付きの A3 縦 PDF を作って」 | `create_layout` → `add_map_frame` → `add_layout_*` → `export_layout` |
| 「12 レイヤを同じシンボルで一括エクスポートして」 | `run_batch` または `execute_arcpy_code` |

**ツール名で直接呼び出す**：以下の表のすべてのツールは名前で直接呼び出せます。`run_batch` は複数コマンドを 1 往復で連鎖実行します。専用ツールがない場合は `execute_arcpy_code` にフォールバックします。

**編集ルール**：編集は `save_edits` まで保留状態です — 保留中はデータがロックされ（削除・上書きは失敗）、`discard_edits` で保留中の編集を破棄できます。`delete_features` / `delete_dataset` は元に戻せません — 実行前に影響を説明してもらってください。

**バッチと高速化**：繰り返しの多い作業や大きなジョブでは、アシスタントに冪等な arcpy スクリプトを書かせて RunCode で一度に実行してもらいます。大きなプロジェクトは範囲 / OID ごとにチェックポイント付きで分割処理します。

---

## 🎨 マッピングと地図作成（プロンプトガイド）

このセクションは「AI への話しかけ方」を説明します。ツール名を覚える必要はありません。下の 4 要素を押さえれば、アシスタントが MCP ツール呼び出しに分解してくれます。

### 4 つを言えば一発で決まる

| 要素 | 伝えること | 例 |
|---|---|---|
| データ | レイヤ名、データパス、現在の選択状態 | 「現在の地図の `county_boundaries` レイヤ」 |
| 操作 | 分類フィールド / 方法、バッファ距離、統計の定義 | 「`AREA` で 5 クラス、自然分類」 |
| スタイル | 配色、フォントサイズ、凡例・縮尺バー・北矢印の配置、ベースマップ | 「薄い赤から濃い緑、凡例は右下」 |
| 出力 | 画像 / PDF / 新フィーチャクラス、解像度、保存先 | 「A3 横 PDF、300 dpi、`D:\out\` に保存」 |

**良いプロンプトと悪いプロンプト**
- ❌ 「地図をもっときれいにして」 — 目的がなく、アシスタントは推測するしかありません。
- ✅ 「`landuse` を `DLMC` の値別シンボルにして、凡例を右下に置き、A3 横 PDF を 300 dpi で `D:\out\landuse.pdf` にエクスポートして。」
- ❌ 「地図をエクスポートして」 — 範囲・レイアウト・出力形式が未指定です。
- ✅ 「`study_area` レイヤを範囲に、縮尺 1:10000、A4 縦 PNG、タイトル『対象区域の土地利用』、凡例・縮尺バー・北矢印付きで。まずプレビューを送って確認させて。」

### ツールグループ別プロンプト早見表

| モジュール（ツールグループ） | こう言えます | アシスタントが使うツール |
|---|---|---|
| セッションとプロジェクト | 「まず `diagnose` で接続と現在のプロジェクトを確認して。それからプロジェクトを `D:\proj\result.aprx` として保存して」 | diagnose / save_project |
| レイヤ | 「`roads` を `basemap` グループに移動して、透過率 40%、縮尺 1:50000 以細でのみ表示。`POI` レイヤは非表示に」 | move_layer / set_layer_transparency / set_layer_scale_range / set_layer_visibility |
| データと属性 | 「`parcels` の先頭 20 行を面積の降順で読んで。`DLMC` ごとに面積を集計して」 | get_features / summarize_features |
| 選択 | 「`POP` が 50 万以上の郡を選択して `D:\out\counties_big.shp` にエクスポート。そのあと `study_area` 内の地番を位置で選択して」 | select_features / select_by_location / export_features |
| フィールドとデータセット | 「`parcels` に DOUBLE フィールド `AREA_HA` を追加して `!Shape_Area!/10000` で計算して。EPSG:4547 でポリゴンフィーチャクラス `result_2026` を作って」 | add_field / calculate_field / create_feature_class |
| ジオプロセシング | 「道路を 500 メートルバッファしてディゾルブ。`study_area` のマスクで土地利用ラスタをクリップして」 | run_geoprocessing_tool / run_batch |
| シンボロジ | 「面積で 5 クラス自然分類、薄い赤から濃い緑。または `TYPE` の値別配色」 | set_layer_renderer |
| 地図ビュー | 「`study_area` にズームして縮尺 1:10000、1600×900 の PNG を撮って」 | set_map_view / zoom_to_layer / export_map_view |
| レイアウトとエクスポート | 「A3 横レイアウトを作成、タイトルは…、凡例は 3 レイヤのみ、縮尺バーと北矢印を追加、300 dpi PDF でエクスポート」 | create_layout / add_layout_* / export_layout |
| ラスタ | 「DEM のバンド・セルサイズ・統計を読んで。NDVI 式で植生指数を計算して」 | get_raster_info / raster_calculator / zonal_statistics |
| カタログとデータ探索 | 「プロジェクトとフォルダ接続から、名前に `2025` を含むシェープファイルを探して」 | search_data / describe_dataset / list_folder |
| ユーティリティ | 「一気に：ベースマップを隠す → 郡にズーム → PNG エクスポート。arcpy で全レイヤ名に接頭辞を付けて」 | run_batch / execute_arcpy_code |

### 標準マッピングプロンプトのテンプレート（コピーして使う）

```text
現在のプロジェクトの <地図名> から地図を作成してください：
- 範囲：<対象区域レイヤ / 範囲>、縮尺 1:<...>
- データ：<レイヤ1>、<レイヤ2>（必要ならフィールドとフィルタを明記）
- シンボル：<フィールドによる段彩 / 値別 / 単一色>、配色 <ランプまたは hex>
- レイアウト：<A3 横 / A4 縦>、タイトル「<タイトル>」、凡例 <位置>、縮尺バー <位置>、北矢印 <位置>
- 出力：<PDF / PNG>、<300> dpi、<絶対パス> に保存
- 進め方：まずレイアウトを確認し、プレビューを送って承認をもらってから、最終ファイルをエクスポートする。
```

### バッチマッピングプロンプト

```text
`maps` の各地図について、同じ A3 横レイアウトテンプレートで PDF をエクスポートしてください：
地図名をタイトルに使い、凡例には表示中のレイヤを残し、縮尺バーと北矢印は右下に、
`D:\out\maps\` に 300 dpi で出力。まず 1 枚をエクスポートして確認させて、承認後に残りをバッチ処理。
```

### よくある失敗への一言修正

- 「凡例が地図本体を隠している」→ アシスタントが `legend` の位置かレイアウトを調整します。
- 「ページ比率がおかしい / 余白が多すぎる」→ 地図フレームとページの比率を調整します。
- 「色が派手すぎる / 論文に合わない」→ 配色、hex 色、参考画像を指定します。
- 「まだエクスポートしないで、プレビューを見せて」→ アシスタントが先に `export_map_view` かレイアウトプレビューを実行します。

**効果**：データ・操作・スタイル・出力を一度に伝えると、アシスタントは通常 1 往復で最終図に近いものを作ります。あとは差分だけを指摘してください（「凡例を右に 1 cm」など）。すべてを言い直す必要はありません。

---

## 🛡️ ダイアログガードの使用（1.x 版）

**解決する問題**：ArcGIS Pro がサードパーティ製 `.pyt` / Python ツール（RunCode.pyt など）を初めて実行する際、「サードパーティ コード ライブラリ」の信頼ダイアログが表示され、無人タスクがそこで止まってしまいます。1.1.4 には MCP ガードツールはありません — 代わりにパッケージには**スタンドアロンのガードスクリプト**が同梱されています。起動すると Pro のウィンドウをポーリングし、この種の「はい(Y)」ダイアログだけをクリックし、指定時間が経過すると自動で終了します。

**起動方法**

```text
install\03-dialog-guard.cmd                    # 既定で 600 秒
install\03-dialog-guard.cmd -DurationSeconds 120
install\03-dialog-guard.cmd -AlertOnly         # 最前面警告のみ、自動クリックなし
install\03-dialog-guard.cmd -NotifyTest        # 警告ウィンドウのみテスト（30 秒）
```

`03-dialog-guard.cmd` をダブルクリックするか、ターミナルを操作できる AI アシスタントにバックグラウンドで起動させてもかまいません。

**プロンプト例**

| こう言う | アシスタントの動作 |
|---|---|
| 「これから Pro で手動で RunCode.pyt を実行する。先にダイアログガードを 600 秒起動して。サードパーティコードの信頼ダイアログの『はい』を自動クリックし、クリックできなければ最前面警告を出してビープを鳴らして」 | バックグラウンドで `install\03-dialog-guard.cmd -DurationSeconds 600` を実行 |
| 「今回は警告だけで、自動クリックはしないで」 | `-AlertOnly` で起動 |
| 「警告ウィンドウがポップアップするかテストして」 | `-NotifyTest` で起動（30 秒） |
| 「ガードのログはどこ？」 | `%LOCALAPPDATA%\ArcGIS Pro MCP\guard\watch_pro_dialog.log` |

**境界と動作**
- コンテキストに「third-party / code library / 第三方 / 代码库」を含む信頼ダイアログのみを処理します。上書き・削除などの確認ダイアログには一切触れません。
- ボタンをクリックできない場合、最前面警告ウィンドウが少なくとも 30 秒表示され、10 秒ごとにビープ音が鳴ります。
- 信頼は Pro プロセスごとに 1 回だけ要求されます。Pro を再起動すると再表示されます。
- 2.x ではこれが `guard_summon` / `guard_status` ツールに昇格し、RunCode 呼び出し前の自動 arm / settle が付きました。1.1.4 では手動でスクリプトを起動します。

---

## 🐍 RunCode と Python ブリッジ（プロンプトガイド）

1.1.4 の arcpy チャネルは 2 階層です。**RunCode ツールボックス**は単発 / バッチスクリプト向き、**Python ブリッジ**は変数を呼び出し間で保持したい対話セッション向きです。

### RunCode ツールボックス

パス：`%LOCALAPPDATA%\ArcGIS Pro MCP\runcode\RunCode.pyt`。アシスタントにこのパスを `run_python_toolbox_tool` に渡させます。Pro の「挿入 → ツールボックス → ツールボックスの追加」から自分で追加することもできます。

**プロンプト例**
- 「RunCode.pyt を使って、現在のプロジェクトのすべてのレイヤのラベルをオンに。ラベルフィールドは `NAME`、フォントサイズ 10。」
- 「冪等な arcpy スクリプトを書いて RunCode で実行し、全レイヤ名に `LYR_` を付け足して。既に付いているものはスキップして。」
- 「このスクリプトを RunCode.pyt で実行する予定 — 先にパラメータとパスを確認してから実行して。」

**初回実行**時は「サードパーティ コード ライブラリ」の信頼ダイアログが表示されます。先に上のダイアログガードを起動するか、一度手動で「はい」をクリックしてください。信頼は Pro プロセスごとに 1 回だけで済みます。

### Python ブリッジ（`CURRENT` や永続変数が必要なとき）

1. Pro のメニュー **解析 → Python** から Python ウィンドウを開きます；
2. 以下を貼り付けて実行します（パスは既定のインストール先の場合）：

```python
import sys; sys.path.insert(0, r"C:\Users\<your-username>\AppData\Local\ArcGIS Pro MCP\plugin")
import mcp_bridge; print(mcp_bridge.start_server(port=6511))
```

3. `CURRENT`（開いているプロジェクト）が必要な場合は、もう 1 行追加します（メインスレッドで実行する必要があります）：

```python
print(mcp_bridge.start_pump())
```

**プロンプト例**
- 「次の arcpy 作業は `CURRENT` が必要。Pro の Python ウィンドウにブリッジ起動コードを貼るようリマインドして。その後 `execute_arcpy_code` で実行する。」
- 「ブリッジ起動コードに私のユーザー名を入れてコピーして。Pro に自分で貼るから。」
- 「まず `get_pump_status` でポンプが準備できているか確認して。できていなければ貼り付けをリマインドして。」
- 「永続セッションを 3 ステップで：レイヤを読む、フィールドを作る、計算する — 変数は呼び出し間で保持して。」

**効果**：RunCode は「バッチスクリプトを一気に実行」、Python ブリッジは「現在のプロジェクトが必要な多段対話」を担当します。どちらも別途の Python インストールは不要です。

---

## 🧪 蒸留モジュールについて（2.x の機能）

1.1.4 にはテンプレート蒸留（distillation）は**含まれていません**：`template_*` ツールも `guard_summon` / `guard_status` もありません。サンプル図のレイアウトを再利用可能なテンプレートにして、複数の地図を揃えたい場合は **V2.01（v2.0.1）** にアップグレードしてください：

- 2.x は 10 個の `template_*` ツールを提供：import / measure / patch / validate / preview / review / approve / apply / list / get；
- テンプレートライブラリは `%LOCALAPPDATA%\ArcGIS Pro MCP\library` にあり、アップグレードしても既存テンプレートは保持されます；
- 典型的な使い方：「`D:\samples\fig3.png` を『対象区域概況』という名前のテンプレートに蒸留して。import → measure → patch → validate → preview の流れで、プレビューを見せて確認させてから review + approve。」

1.1.4 と 2.x の機能比較：

| 機能 | 1.1.4（このブランチ） | 2.0.1（非推奨） |
|---|---|---|
| ネイティブ MCP ツール | 113（12 グループ） | 125（14 グループ） |
| RunCode / Python ブリッジ | ✅ | ✅ |
| ダイアログガード | スタンドアロンスクリプト `03-dialog-guard.cmd` | `guard_summon` / `guard_status` + RunCode 前の自動 arm / settle |
| テンプレート蒸留 | ❌ | ✅ 10 個の `template_*` ツール + schema v1.1 |

---

## 🧰 全 113 個の MCP ツール（12 グループ）

アシスタントが使えるすべての機能を名前で列挙します。クリックで展開 👇

<details>
<summary><b>📜 ツール一覧 — セッション 15 · レイヤ 19 · データ 11 · 選択 6 · フィールド/データセット 11 · ジオプロセシング 7 · シンボロジ 6 · ビュー 7 · レイアウト 16 · ラスタ 5 · カタログ 6 · ユーティリティ 4</b></summary>

### セッションとプロジェクト（15）

| ツール | 目的 |
|---|---|
| `ping` | ブリッジの疎通を確認し、接続中のプロジェクトを返します。 |
| `get_capabilities` | ブリッジが対応する全コマンドをエリア別に一覧します。名前付きツール以外の機能を発見するためのものです。 |
| `get_queue_status` | 3 レーンのキュー状態：レーンごとの待ち数、実行中のコマンド、最近の完了分。呼び出しが遅い・詰まると感じたら最初に確認します。 |
| `diagnose` | 接続セルフチェック：Pro のバージョン、ライセンス、開いているプロジェクト、アクティブ地図、地図ビュー、書き込み権限。異常時は最初に実行します。 |
| `get_arcgis_info` | ArcGIS Pro のバージョン、ライセンスレベル、利用可能なエクステンション、現在のプロジェクトパス。 |
| `get_project_info` | プロジェクトパス、既定のジオデータベースとツールボックス、地図、レイアウト、フォルダ・データベース接続。 |
| `save_project` | プロジェクト（.aprx）を保存、または別の場所にコピー保存します。 |
| `list_maps` | すべての地図・シーンを座標系とレイヤ数付きで一覧します。 |
| `create_map` | プロジェクトに新しい地図またはシーンを作成します。 |
| `remove_map` | プロジェクトから地図を削除します。 |
| `activate_map` | 地図ビューを Pro UI で開く / アクティブにします。カメラ系コマンドと export_map_view の前に必要です。 |
| `set_map_properties` | 地図の名前変更や座標系の変更を行います。 |
| `get_map_extent` | 地図内の全データレイヤを合わせた範囲を返します。 |
| `get_environment` | 現在の arcpy ジオプロセシング環境設定を読み取ります。 |
| `set_environment` | arcpy 環境を設定します（ワークスペース、出力座標系、範囲、マスク、セルサイズ、出力上書き、並列処理係数など）。 |

### レイヤ（19）

| ツール | 目的 |
|---|---|
| `get_layers` | 地図のレイヤ（描画順とグループ階層付き）とスタンドアロンテーブルを一覧します。プロジェクトの中身を把握する最初の一歩。 |
| `get_layer_info` | 1 つのレイヤの詳細：データソース、座標系、範囲、フィールド、フィーチャ数、レンダラー、ラベルの状態。 |
| `add_layer` | パスまたはサービス URL でデータを地図に追加：フィーチャクラス、シェープファイル、ラスタ、テーブル、.lyrx レイヤファイル、Web サービス。 |
| `add_web_layer` | URL で Web レイヤを追加（Feature / Map / Image サービス、WMS、WMTS、ベクタタイル）。 |
| `remove_layer` | 地図からレイヤまたはスタンドアロンテーブルを削除します。 |
| `rename_layer` | コンテンツのレイヤ名を変更します。 |
| `duplicate_layer` | 地図内でレイヤを複製し、別のシンボルを割り当てられるようにします。 |
| `set_layer_visibility` | 1 つのレイヤ、複数レイヤ、または地図内の全レイヤの表示 / 非表示を切り替えます。 |
| `set_layer_transparency` | レイヤの透過率を設定します（0 が不透明、100 が完全透明）。 |
| `set_layer_scale_range` | レイヤを描画する縮尺範囲を制限します。 |
| `set_definition_query` | レイヤの定義クエリ（SQL where）を設定します。空文字で解除。 |
| `move_layer` | レイヤを別のレイヤを基準に並べ替える、またはグループレイヤへ移動します。 |
| `create_group_layer` | グループレイヤを作成し、既存レイヤをオプションで中へ移動します。 |
| `zoom_to_layer` | 地図ビューをレイヤの範囲にズームします。 |
| `set_basemap` | ベースマップを設定：Topographic、Imagery、Imagery Hybrid、Streets、Navigation、Light Gray Canvas、Dark Gray Canvas、Terrain、Oceans、OpenStreetMap、National Geographic など。 |
| `get_broken_layers` | 全地図の中から、データソースが見つからないレイヤを一覧します。 |
| `repair_layer_source` | 壊れたデータソースを修復するため、レイヤを新しいワークスペース / データセットに向け直します。 |
| `add_join` | 共通フィールドでテーブルをレイヤに結合します。 |
| `remove_join` | レイヤの結合を解除します。 |

### データと属性（11）

| ツール | 目的 |
|---|---|
| `get_features` | レイヤ・テーブル・データセットパスから属性行を読み取ります。where、フィールド指定、並べ替え、WKT ジオメトリに対応。 |
| `count_features` | フィーチャ数をカウントします。where 条件の指定も可能。 |
| `get_unique_values` | フィールドの一意値と、それぞれの行数を返します。 |
| `get_field_statistics` | 数値フィールドの min / max / 平均 / 中央値 / 合計 / 標準偏差。 |
| `summarize_features` | 1 つ以上のフィールドでグループ化して集計 — 「カテゴリごとの合計」をジオプロセシングなしで素早く答える手段。 |
| `insert_features` | レイヤまたはテーブルに新しい行を挿入します。 |
| `update_features` | where 条件に一致する行の属性やジオメトリを更新します。 |
| `delete_features` | where 条件に一致する行を削除します。 |
| `save_edits` | 保留中の編集を確定します。未保存の編集はデータをロックし、それまで削除・上書きは失敗します。 |
| `discard_edits` | 保留中の編集を破棄します。 |
| `calculate_field` | フィールドを計算します（例：「!Shape_Area! / 10000」）。 |

### 選択（6）

| ツール | 目的 |
|---|---|
| `select_features` | SQL where 句でフィーチャを選択します。 |
| `select_by_location` | 別のレイヤとの空間リレーションでフィーチャを選択します。 |
| `get_selection` | 現在の選択状態を報告します（レイヤごと、ObjectID とオプションで属性行付き）。 |
| `set_selection` | ObjectID で特定のフィーチャを選択します。 |
| `clear_selection` | 1 つのレイヤ、または地図内のすべてのレイヤの選択を解除します。 |
| `zoom_to_selection` | 地図ビューを現在の選択フィーチャにズームします。 |

### フィールドとデータセット（11）

| ツール | 目的 |
|---|---|
| `list_fields` | レイヤのフィールドを型・別名・長さ・ドメイン付きで一覧します。 |
| `add_field` | レイヤまたはテーブルにフィールドを追加します。 |
| `add_fields` | 複数フィールドを 1 回の呼び出しで追加します。 |
| `delete_field` | 1 つまたは複数のフィールドを削除します。 |
| `alter_field` | フィールドの名前変更、別名・長さの変更を行います。 |
| `create_feature_class` | 空のフィーチャクラスを作成し（既定ではプロジェクトの既定ジオデータベース内）、地図に追加します。 |
| `create_table` | 空のスタンドアロンテーブルを作成します。 |
| `create_file_geodatabase` | 新しいファイルジオデータベースを作成します。 |
| `truncate_table` | スキーマを保ったまま、テーブルまたはフィーチャクラスの全行を削除します。 |
| `delete_dataset` | ディスクまたはジオデータベースからデータセットを削除します。元に戻せません。 |
| `export_features` | レイヤを新しいデータセットにエクスポートします。現在の選択と定義クエリが反映されます。 |

### ジオプロセシング（7）

| ツール | 目的 |
|---|---|
| `run_geoprocessing_tool` | 任意の arcpy ジオプロセシングツールを実行 — 万能の分析チャネル。レイヤ名は開いているレイヤに解決され、選択や定義クエリが適用されます。パラメータ名が不明なときは先に describe_geoprocessing_tool を呼びます。 |
| `list_geoprocessing_tools` | 利用可能なジオプロセシングツールを名前で検索します。 |
| `list_toolboxes` | arcpy のツールボックスモジュールとプロジェクトのツールボックスを一覧します。 |
| `describe_geoprocessing_tool` | 実行前にツールのパラメータ・データ型・既定値・使用説明を照会します。 |
| `run_python_toolbox_tool` | ディスク上のカスタム .pyt / .atbx / .tbx ツールボックスからツールを実行します。 |
| `check_extension` | ArcGIS エクステンションのライセンスを確認し、必要に応じてチェックアウトします。 |
| `get_messages` | 直近のジオプロセシング処理のメッセージを取得します。 |

### シンボロジ（6）

| ツール | 目的 |
|---|---|
| `set_layer_renderer` | レイヤのシンボルを変更：単一シンボル、カテゴリの値別、数値フィールドによる段彩 / 連続配色。 |
| `get_layer_symbology` | レイヤの現在のレンダラー、クラス区切り、一意値、ラベル設定を確認します。 |
| `list_color_ramps` | プロジェクトで利用可能なカラーランプを一覧します。 |
| `set_layer_labeling` | ラベルのオン / オフ、式・フォント・ハローを設定します。 |
| `apply_symbology_from_layer` | .lyrx ファイルまたは別のレイヤからシンボルをコピーします。 |
| `save_layer_file` | シンボル付きのレイヤを .lyrx ファイルに保存し、再利用できるようにします。 |

### 地図ビュー（7）

| ツール | 目的 |
|---|---|
| `get_map_view` | 現在のカメラ位置：中心、縮尺、回転、可視範囲。Pro で地図タブが開いている必要があります。 |
| `set_map_view` | 地図ビューを移動：範囲、中心点、縮尺、回転を設定します。 |
| `export_map_view` | 地図ビューを PNG にレンダリングし、目視確認用に画像を返します。地図タブが開いている必要があります。 |
| `list_bookmarks` | 地図に定義された空間ブックマークを一覧します。 |
| `create_bookmark` | 現在のビューまたは指定範囲を名前付きブックマークとして保存します。 |
| `apply_bookmark` | 地図ビューをブックマークにズームします。 |
| `delete_bookmark` | 地図からブックマークを削除します。 |

### レイアウトとエクスポート（16）

| ツール | 目的 |
|---|---|
| `list_layouts` | 印刷レイアウトをページサイズと要素数付きで一覧します。 |
| `get_layout_info` | レイアウトを検査：ページ設定と全要素の位置・サイズ。 |
| `create_layout` | 新しいレイアウトページを作成します（既定ではページ全面のマップフレーム付き）。 |
| `delete_layout` | プロジェクトからレイアウトを削除します。 |
| `add_map_frame` | レイアウトのページ上の位置にマップフレームを追加します。 |
| `set_map_frame_extent` | レイアウトのマップフレームをレイヤ・範囲・縮尺に合わせます。 |
| `add_layout_text` | タイトルや出典などのテキスト要素をレイアウトに追加します。 |
| `add_layout_legend` | レイアウトのマップフレームに連動した凡例を追加します。 |
| `add_layout_scale_bar` | マップフレームに連動した縮尺バーを追加します。 |
| `add_layout_north_arrow` | マップフレームに連動した北矢印を追加します。 |
| `add_layout_picture` | ロゴなどの画像をレイアウトに配置します。 |
| `set_layout_element` | 任意のレイアウト要素の移動・サイズ変更・名前変更・非表示・テキスト変更を行います。 |
| `delete_layout_element` | レイアウトから要素を削除します。 |
| `export_layout` | レイアウトを PDF / PNG / JPEG / SVG / TIFF にエクスポートします。PNG / JPEG は確認用に画像としてインラインで返ります。 |
| `preview_layout` | ファイルを書き出さずにレイアウトを作業用画像にレンダリング — 構築中の確認に。 |
| `export_map_series` | レイアウトのマップシリーズ（マップブック）を複数ページ PDF にエクスポートします。 |

### ラスタ（5）

| ツール | 目的 |
|---|---|
| `get_raster_info` | ラスタの詳細：バンド、サイズ、セルサイズ、ピクセルタイプ、統計、座標系。 |
| `set_raster_symbology` | ラスタレイヤのカラーライザーとカラーランプを設定します。 |
| `raster_calculator` | マップ代数演算：ラスタを短い別名に結び付け、Python 式を書きます。Spatial Analyst エクステンションが必要です。 |
| `sample_raster_values` | 地図座標またはポイントレイヤのフィーチャ位置でラスタのセル値を読み取ります。 |
| `zonal_statistics` | ゾーンポリゴン内のラスタ値を集計し、テーブルを返します。Spatial Analyst エクステンションが必要です。 |

### カタログとデータ探索（6）

| ツール | 目的 |
|---|---|
| `list_workspace_contents` | ジオデータベースまたはフォルダ内のデータセットを一覧します。既定はプロジェクトの既定ジオデータベース。 |
| `list_folder` | ディスク上の GIS ファイルとサブフォルダを一覧します。既定はプロジェクトのホームフォルダ。 |
| `describe_dataset` | 任意のデータセットをパスで記述：種類、ジオメトリ、座標系、範囲、フィールド、行数（地図に追加せずに）。 |
| `search_data` | プロジェクトのジオデータベース・ホームフォルダ・フォルダ接続から名前でデータセットを検索 — 追加前にデータを特定する最速の方法。 |
| `get_project_items` | プロジェクトに登録されたフォルダ接続、データベース接続、ツールボックス。 |
| `add_folder_connection` | フォルダをプロジェクトに登録し、データを閲覧しやすくします。 |

### ユーティリティ（4）

| ツール | 目的 |
|---|---|
| `execute_arcpy_code` | ArcGIS Pro 内で Python を実行：arcpy はインポート済み、arcpy.mp.ArcGISProject('CURRENT') が開いているプロジェクト、変数は呼び出し間で保持、print 出力は取得され、最後の式の値が返ります。専用ツールが合わないときに使用します。 |
| `get_pump_status` | メインスレッドディスパッチャーが導入済みかどうか。コマンドが開いている Pro プロジェクトに届くかを左右します — 未導入ではパスベースの作業しか実行できません。 |
| `stop_pump` | メインスレッドディスパッチャーを解除します。ユーザーが再導入するまでライブプロジェクト系コマンドは動きません。 |
| `run_batch` | 複数コマンドを 1 往復で実行 — 多段ワークフローが大幅に速くなります。 |

</details>

---

## 📦 パッケージ内容

| パス | 内容 | 備考 |
|---|---|---|
| `install\ArcGISProMCP.esriAddinX` | 🧩 add-in v1.1.4 | ネイティブ 113 ツール + 全 GP ツール；HTTP MCP 6520 / レガシー TCP 6510 |
| `install\runtime\stdio-bridge.ps1` | 🌉 stdio ブリッジ | ローカルプロセスしか起動できないクライアント（Claude Desktop など）を 6520 に橋渡しします。純 PowerShell で Python 不要 |
| `install\runtime\runcode\RunCode.pyt` | 🐍 RunCode ツールボックス | 任意の arcpy を頻繁に実行する用途。初回のみ信頼ダイアログが 1 回表示されます |
| `install\runtime\guard\` | 🛡️ ダイアログガード | 「サードパーティ コード ライブラリ」の信頼ダイアログを自動クリック。失敗時は最前面警告（30 秒以上、ビープ付き） |
| `install\runtime\plugin\` | 🔌 Python ブリッジ（任意） | Pro の Python ウィンドウで起動。永続セッションの `execute_arcpy_code` を提供（ポート 6511） |
| `install\*.cmd / *.ps1` | 🛠️ インストールとセルフチェック | インストール / セルフチェック / Claude 設定 / ダイアログガード / アンインストール |
| `clients\` | 📁 クライアント設定ドキュメント | Claude Code、Claude Desktop、Codex、汎用 JSON |
| `docs\` | 📖 使用ガイド、トラブルシューティング、作業ルール、変更履歴 | まずは使用ガイドから |
| `source\` | 📜 完全なソーススナップショット | 本パッケージのバイナリに対応するソース（AGPL-3.0 準拠の要件）。上流の LICENSE / NOTICE を含みます |

### 🔍 依存関係の監査（なぜ「完全同梱」と言えるのか）

- 🧩 add-in：自己完結型の C# アセンブリで、Pro 3.7 のサポート環境に含まれる .NET Desktop Runtime 10 x64 上で動作します — 別途の .NET インストールは不要です。
- 🌉 stdio ブリッジ、🛡️ ダイアログガード：PowerShell 5.1 + WinForms（Windows 同梱）— 追加インストールはありません。
- 🐍 RunCode ツールボックス、Python ブリッジ：ArcGIS Pro に同梱の Python（arcpy）上で動作 — 別途の Python インストールは不要です。
- ✅ **したがって本パッケージには、pip やネットワークアクセスを必要とする外部依存はありません。**「オフラインで動作」はパッケージ自身のインストールと実行経路を指します。ArcGIS Pro 3.7 のインストール前提（.NET Desktop Runtime 10 x64、WebView2 ≥132）は Pro 本体のインストーラーが提供します。

---

## 🧪 オプションコンポーネントの使用

**🐍 RunCode ツールボックス**（arcpy 作業が多い場合）
- パス：`%LOCALAPPDATA%\ArcGIS Pro MCP\runcode\RunCode.pyt`
- AI にこのパスを `run_python_toolbox_tool` へ渡させます。または Pro の「挿入 → ツールボックス → ツールボックスの追加」で追加します。
- 各 Pro セッションの初回実行で「サードパーティ コード ライブラリ」の信頼ダイアログが表示されます：先に `03-dialog-guard.cmd` を起動すれば、ガードが代わりに「はい」をクリックします ✨。

**🔌 Python ブリッジ**（arcpy セッションを呼び出し間で保持したい場合）
1. ArcGIS Pro のメニュー **解析 → Python** から Python ウィンドウを開きます；
2. 貼り付けて実行します（パスは既定のインストール先を想定して記入済み）：
   ```python
   import sys; sys.path.insert(0, r"C:\Users\<your-username>\AppData\Local\ArcGIS Pro MCP\plugin")
   import mcp_bridge; print(mcp_bridge.start_server(port=6511))
   ```
3. `CURRENT`（開いているプロジェクト）が必要な場合は、もう 1 行追加します（メインスレッドで実行する必要があります）：
   ```python
   print(mcp_bridge.start_pump())
   ```

**🛡️ ダイアログガード**（RunCode 実行前や ENVI→Pro 連携ワークフローの前に推奨）
```
03-dialog-guard.cmd                 # 600 秒ガード
03-dialog-guard.cmd -DurationSeconds 120
03-dialog-guard.cmd -NotifyTest     # 警告ウィンドウのみテスト（30 秒）
03-dialog-guard.cmd -AlertOnly      # 警告のみ、自動クリックなし
```
安全境界：コンテキストに「third-party / code library / 第三方 / 代码库」を含む信頼ダイアログのみを処理し、**上書き・削除などの確認ダイアログには決して触れません** 🙅。ログは `%LOCALAPPDATA%\ArcGIS Pro MCP\guard\watch_pro_dialog.log`。

---

## 🔒 セキュリティに関する注意

- 🏠 add-in とすべてのブリッジサービスは `127.0.0.1` にのみバインドされます。LAN 上の他のマシンからは接続できません。
- ⚖️ 本ソフトウェアは `127.0.0.1` 上でのローカルサービス提供のみを行い、ネットワーク経由で第三者にサービスを提供しません。改変して第三者へのネットワーク提供を行う場合、AGPL-3.0 §13 のソース提供義務が適用されます。
- 🔏 add-in は未署名ビルドです：ArcGIS Pro は初回読み込み時に信頼の確認を求めます — これは通常のフローです。本パッケージの `SHA256SUMS.txt` で検証してください。
- 🕵️ 本パッケージにはテレメトリもネットワーク報告もありません。
- 🧹 アンインストール：`99-uninstall.cmd`（クライアント設定はオプションで保持可能）。

---

## 📜 バージョンとライセンス

- 🏷️ バージョン：**1.1.4**（ビルド日時は `VERSION.json`；113 ツール）
- 👤 メンテナー：**Law**（[@XNYGggg](https://github.com/XNYGggg)）
- 🙏 上流プロジェクトと原作者：**[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)** by **Pisut Nakmuenwai**（AGPL-3.0-or-later）。本パッケージはこれを基に 1.1.4 までパッチを積み上げています（変更内容は `docs\CHANGELOG.md` と `RELEASE-NOTES.md` に記載）。
- ⚖️ ライセンス：**AGPL-3.0-or-later**（パッケージには完全なソーススナップショットと上流の NOTICE が含まれます）
- 🏷️ 商標：「ArcGIS」は Esri の商標です。本プロジェクトはサードパーティ製ツールであり、Esri とは関係ありません。

🆘 FAQ は `docs\排障手册.md`（トラブルシューティング）、高度な使い方は `docs\使用指南.md`（使用ガイド）にあります。楽しいマッピングを 🎨 (｀・ω・´)ゞ
