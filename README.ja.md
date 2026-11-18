[![English](https://img.shields.io/badge/Language-English-1f6feb?style=flat-square)](README.md) [![简体中文](https://img.shields.io/badge/Language-%E7%AE%80%E4%BD%93%E4%B8%AD%E6%96%87-d1242f?style=flat-square)](README.zh-CN.md) [![日本語](https://img.shields.io/badge/Language-%E6%97%A5%E6%9C%AC%E8%AA%9E-bc002d?style=flat-square)](README.ja.md) [![한국어](https://img.shields.io/badge/Language-%ED%95%9C%EA%B5%AD%EC%96%B4-0047a0?style=flat-square)](README.ko.md) [![Version](https://img.shields.io/badge/Version-Customer%201.16%20(1.14%20Enhanced)-2ea44f?style=flat-square)](VERSION.json) [![Changelog](https://img.shields.io/badge/Docs-Changelog-6f42c1?style=flat-square)](docs/CHANGELOG.md) [![License: AGPL v3+](https://img.shields.io/badge/License-AGPL--3.0--or--later-blue?style=flat-square)](LICENSE) [![ArcGIS Pro](https://img.shields.io/badge/ArcGIS%20Pro-3.7%2B-00a1e0?style=flat-square)](https://www.esri.com/en-us/arcgis/products/arcgis-pro/overview) [![Platform](https://img.shields.io/badge/Platform-Windows%2011%20x64-lightgrey?style=flat-square)]()

# 🗺️ ArcGIS Pro MCP（最適化版）· Customer 1.16（1.14 拡張版）🚀

自然言語の一文だけで、AI アシスタントが今開いている ArcGIS Pro プロジェクトを直接操作できます：レイヤの追加、シンボルの変更、属性の照会、ジオプロセシングの実行、地図のエクスポートまで。データがマシン外に出ることはなく、すべての通信は `127.0.0.1` の中で完結します 🔒。

> **🚀 このブランチは Customer 1.16（1.14 拡張版、ビルド番号 1.1.6）です**：1.14（v1.1.4）の上に 1.1.5 の F1–F10 と 1.1.6 の V-1–V-7 を積み上げたものです。**1.1.4 の正式リリースはまだ準備中です。ご期待ください**；このブランチは **Release／配布版のアセットを提供しません**。インストーラはリポジトリの `install/` にあり、いつでも v1.1.4 にロールバックできます。

> **📌 バージョンの説明（2026-11-16）**：**1.0 系（v1.1.4 正式パッケージ）は非常に安定しており、日常利用に推奨**；**2.0 はテスト版**で最適化が未完了のため、GitHub Release のアセットとタグは削除済みで、現在ダウンロードできません。

> **🙏 上流プロジェクトと原作者**：本カスタマーエディションは **[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)**（原作者 **Pisut Nakmuenwai**、AGPL-3.0-or-later）を基に、ローカルパッチを **v1.1.4** まで積み上げたものです。完全な帰属表示と変更通知は、パッケージルートの `NOTICE` と `source/SOURCE-NOTICE.md` に記載しています。

本パッケージは実行に必要な**すべてのコンポーネント**を同梱しており、完全オフラインで動作します。**Python のインストールも pip もネット接続も不要**で、ArcGIS Pro 本体以外に依存はありません。電池も入っていますよ 🔋

---

## 🖥️ システム要件

| 項目 | 要件 |
|---|---|
| 🪟 OS | **Windows 11 x64**（64 ビット Home / Pro / Enterprise。ArcGIS Pro 3.7 の公式サポート対象）。**アーキテクチャ：x64（AMD64）のみ、ARM64 非対応** |
| 🗺️ ArcGIS Pro | **3.7 以降**（本 add-in は 3.7 / .NET 10 向けビルド） |
| 🔑 権限 | 一般ユーザーで可。インストールに**管理者権限は不要** |
| 📦 その他の依存 | なし（PowerShell と .NET は Windows / Pro に同梱） |

---

## 🚀 5 分ではじめられる

1. 📂 本パッケージを任意のフォルダに**解凍**（例：`D:\ArcGIS-Pro-MCP-Customer`）。
2. 🛠️ **ArcGIS Pro を閉じて**から `install\00-install.cmd` をダブルクリック。add-in のインストール、ランタイムの配置、Claude Desktop / Claude Code の設定を自動で行います。
3. 🖥️ **ArcGIS Pro を起動**。初回読み込み時に「アドインを信頼しますか」と表示されたら「はい」をクリック（安全です 😄）。Pro 上部の **MCP タブ**で状態が **running**（ポート `127.0.0.1:6520`）であることを確認します。
4. 🔌 **AI クライアントを接続**（下の「設定方法」参照）し、`install\01-check.cmd` をダブルクリックして自己診断。すべて `[OK]` なら準備完了です (๑•̀ㅂ•́)و✧

> 自己診断時に ArcGIS Pro が開いていないと、ポート検査が警告を出します。Pro を開いてからもう一度実行してください 👌

---

## 🎛️ 設定方法 —— AI クライアントの接続

**前提**：ArcGIS Pro が起動し、**MCP タブ**が **running** になっていること。すべてのポートは本機にのみバインドされます：

| ポート | 用途 |
|---|---|
| `6520` | MCP over HTTP —— すべての AI クライアントはここに接続 |
| `6510` | 旧版 TCP ブリッジ（最新クライアントは未使用） |
| `6511` | 任意の Pro 内 Python ブリッジ（`execute_arcpy_code`） |

### 🟣 Claude Code

```powershell
claude mcp add --transport http arcgis http://127.0.0.1:6520/mcp
claude mcp list
```

手動での同等設定（`~/.claude.json`）：

```json
{ "mcpServers": { "arcgis": { "type": "http", "url": "http://127.0.0.1:6520/mcp" } } }
```

削除は `claude mcp remove arcgis`、再追加は上のコマンドで。

### 🖥️ Claude Desktop（同梱の stdio ブリッジを使用）

Claude Desktop は HTTP に直接接続できず、ローカルプロセスの起動しかできません。同梱の純 PowerShell ブリッジを使います（Python 不要）：

1. **Claude Desktop を完全に終了**（トレイアイコンを含む）。
2. `install\02-configure-claude.cmd` を実行するか、設定を手動で編集（先にバックアップを）。
3. 設定ファイルは次のうち「存在する」最初のフォルダにある `claude_desktop_config.json`：
   `%APPDATA%\Claude`、`%APPDATA%\Claude-3p`、`%LOCALAPPDATA%\Claude-3p`、`%LOCALAPPDATA%\Claude`。

```json
{
  "mcpServers": {
    "arcgis": {
      "command": "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe",
      "args": ["-NoProfile", "-ExecutionPolicy", "Bypass", "-File",
               "C:\\Users\\<ユーザー名>\\AppData\\Local\\ArcGIS Pro MCP\\stdio-bridge.ps1"]
    }
  }
}
```

4. Claude Desktop を開き直し、ツール一覧に `arcgis` が現れれば成功です。

### 🟢 Codex

`~/.codex/config.toml` に追記（既存の `arcgis` セクションがある場合は、古い `env` サブテーブルを含めて丸ごと置き換え）：

```toml
[mcp_servers.arcgis]
url = "http://127.0.0.1:6520/mcp"
```

保存して Codex を再起動します。

### 🔧 その他の MCP クライアント

Streamable HTTP で `http://127.0.0.1:6520/mcp` に接続するか、上の汎用 JSON を使用。（Antigravity は `serverUrl` を使い、`type` は付けません。）

### ✅ 接続の確認

- `install\01-check.cmd` —— Pro のバージョン、add-in のハッシュ、ランタイム、ポート、クライアント設定を検査。
- `netstat -ano | findstr 6520` —— ポートが待ち受けていることを確認。
- ログ：`%LOCALAPPDATA%\ArcGIS-MCP\addin.log`。

---

## 🎯 使い方

**基本の流れ**：データのあるプロジェクトを開く → 要望を伝える → アシスタントがツールを選ぶ → 結果を確認（画像はそのままインラインで返ります）🎨

| こう言えば | 実際に起きること |
|---|---|
| 「面積で県を分级色彩にして、薄い赤から濃い緑へ」 | `set_layer_renderer`（段階色分け） |
| 「人口 50 万人超の県を選択して新しいフィーチャクラスに書き出す」 | `select_features` → `export_features` |
| 「道路に 500 メートルのバッファを作る」 | `run_geoprocessing_tool`（Buffer） |
| 「各郷鎮の耕地面積を集計して」 | `summarize_features` / `zonal_statistics` |
| 「今の地図表示を画像で見せて」 | `export_map_view`（PNG を返す） |
| 「タイトル・凡例・縮尺記号・方位記号付きの A3 縦 PDF を出して」 | `create_layout` → `add_map_frame` → `add_layout_*` → `export_layout` |
| 「12 レイヤを同じシンボルで一括エクスポート」 | `run_batch` または `execute_arcpy_code` |

**ツール名での呼び出し**：下の表の各ツールはクライアントから名前で直接呼べます。`run_batch` は 1 往復で複数コマンドをつなげます。専用ツールがない場合は `execute_arcpy_code` が最後の手段です。

**編集のルール**：編集は `save_edits` まで保留され、保留中はデータがロックされて削除・上書きが失敗します。`discard_edits` で破棄できます。`delete_features` / `delete_dataset` は取り消せません——先にアシスタントへ影響範囲の説明を求めましょう。

**バッチと高速化**：繰り返しの多い大規模作業では、RunCode で冪等な arcpy スクリプトを書いて一度に実行します。大規模プロジェクトは範囲/OID で分割し、チェックポイントを残します。

---

## 🧰 全 115 の MCP ツール（12 グループ）

アシスタントができることをすべて名前付きで並べています。クリックで展開 👇

<details>
<summary><b>📜 ツール一覧 —— セッション 15 · レイヤ 19 · データ 11 · 選択 6 · フィールド/データセット 11 · ジオプロセシング 7 · シンボル 6 · ビュー 7 · レイアウト 16 · ラスタ 5 · カタログ 6 · ユーティリティ 4</b></summary>

### セッションとプロジェクト（15）

| ツール | 用途 |
|---|---|
| `ping` | ブリッジの疎通を確認し、接続先のプロジェクトを返します。 |
| `get_capabilities` | ブリッジが対応する全コマンドを分野別に一覧（名前付きツール以外の機能を発見するのに使用）。 |
| `get_queue_status` | 3 レーンのキューの状態：各レーンの深さ、実行中のコマンド、最近の完了記録。呼び出しが遅い・待たされる時に確認。 |
| `diagnose` | 接続の自己診断：Pro バージョン、ライセンス、開いているプロジェクト、アクティブ地図、マップビュー、書き込み権限。問題発生時はまずこれを実行。 |
| `get_arcgis_info` | ArcGIS Pro のバージョン、ライセンスレベル、利用可能な拡張機能、現在のプロジェクトパス。 |
| `get_project_info` | プロジェクトのパス、既定ジオデータベースとツールボックス、地図、レイアウト、フォルダ/データベース接続。 |
| `save_project` | プロジェクト（.aprx）を保存、または指定先にコピーを保存。 |
| `list_maps` | すべての地図とシーンを座標系・レイヤ数付きで一覧。 |
| `create_map` | プロジェクトに新しい地図またはシーンを作成。 |
| `remove_map` | プロジェクトから地図を削除。 |
| `activate_map` | Pro の UI で地図ビューを開く/アクティブ化。カメラ系コマンドと export_map_view の前に必要。 |
| `set_map_properties` | 地図の名前変更、または座標系の変更。 |
| `get_map_extent` | 地図内の全データレイヤの結合範囲。 |
| `get_environment` | 現在の arcpy ジオプロセシング環境設定を取得。 |
| `set_environment` | arcpy 環境を設定（ワークスペース、出力座標系、範囲、マスク、セルサイズ、出力の上書き、並列係数など）。 |

### レイヤ（19）

| ツール | 用途 |
|---|---|
| `get_layers` | 地図内のレイヤ（描画順・グループ入れ子を含む）とスタンドアロン テーブルを一覧。まずここから。 |
| `get_layer_info` | 1 レイヤの詳細：データソース、座標系、範囲、フィールド、フィーチャ数、レンダラ、ラベル状態。 |
| `add_layer` | パスまたはサービス URL からデータを追加：フィーチャクラス、シェープファイル、ラスタ、テーブル、.lyrx レイヤファイル、Web サービス。 |
| `add_web_layer` | URL で Web レイヤを追加（Feature/Map/Image サービス、WMS、WMTS、ベクタタイル）。 |
| `remove_layer` | 地図からレイヤまたはスタンドアロン テーブルを削除。 |
| `rename_layer` | コンテンツ一覧のレイヤ名を変更。 |
| `duplicate_layer` | 地図内でレイヤを複製し、別のシンボルで表示できるように。 |
| `set_layer_visibility` | 単一・複数・全レイヤの表示/非表示を切り替え。 |
| `set_layer_transparency` | レイヤの透過率を設定（0 は不透明、100 は全透過）。 |
| `set_layer_scale_range` | レイヤを表示する縮尺範囲を制限。 |
| `set_definition_query` | 定義クエリ（SQL where）を設定。空文字列で解除。 |
| `move_layer` | レイヤの順序変更、またはグループレイヤへの移動。 |
| `create_group_layer` | グループレイヤを作成し、既存レイヤを入れることも可能。 |
| `zoom_to_layer` | マップビューをレイヤの範囲にズーム。 |
| `set_basemap` | ベースマップを設定：地形、イメージ、イメージ ハイブリッド、ストリート、ナビゲーション、ライトグレー キャンバス、ダークグレー キャンバス、地形（陰影）、海洋、OpenStreetMap、ナショナル ジオグラフィックなど。 |
| `get_broken_layers` | 全地図でデータソースが欠落したレイヤを一覧。 |
| `repair_layer_source` | レイヤを新しいワークスペースまたはデータセットに向け直して修復。 |
| `add_join` | 共通フィールドでテーブルをレイヤに結合。 |
| `remove_join` | レイヤの結合を解除。 |

### データと属性（11）

| ツール | 用途 |
|---|---|
| `get_features` | レイヤ・テーブル・データセットパスから属性行を読み取り。where、フィールド指定、並べ替え、WKT ジオメトリに対応。 |
| `count_features` | フィーチャ数を数える（where 条件可）。 |
| `get_unique_values` | フィールドの一意値と各行数を返す。 |
| `get_field_statistics` | 数値フィールドの min/max/mean/median/sum/標準偏差。 |
| `summarize_features` | 1 つ以上のフィールドでグループ化して集計。「カテゴリごとの件数/合計」を GP なしで答える近道。 |
| `insert_features` | レイヤまたはテーブルに行を挿入。 |
| `update_features` | where 条件に一致する行の属性/ジオメトリを更新。 |
| `delete_features` | where 条件に一致する行を削除。 |
| `save_edits` | 保留中の編集をコミット。未保存の編集はデータをロックし、削除・上書きが失敗する原因に。 |
| `discard_edits` | 保留中の編集を破棄。 |
| `calculate_field` | フィールド演算。例："!Shape_Area! / 10000"。 |

### 選択（6）

| ツール | 用途 |
|---|---|
| `select_features` | SQL where 条件でフィーチャを選択。 |
| `select_by_location` | 別レイヤとの空間関係で選択。 |
| `get_selection` | 現在の選択状況をレイヤ別に報告（ObjectID と任意で属性行）。 |
| `set_selection` | ObjectID 指定でフィーチャを選択。 |
| `clear_selection` | 単一レイヤまたは地図全体の選択を解除。 |
| `zoom_to_selection` | 選択フィーチャにマップビューをズーム。 |

### フィールドとデータセット（11）

| ツール | 用途 |
|---|---|
| `list_fields` | レイヤのフィールドを型・エイリアス・長さ・ドメイン付きで一覧。 |
| `add_field` | レイヤまたはテーブルにフィールドを追加。 |
| `add_fields` | 1 回の呼び出しで複数フィールドを追加。 |
| `delete_field` | 1 つ以上のフィールドを削除。 |
| `alter_field` | フィールド名の変更、またはエイリアス/長さの変更。 |
| `create_feature_class` | 空のフィーチャクラスを作成（既定はプロジェクトの既定ジオデータベース）し、地図に追加。 |
| `create_table` | 空のスタンドアロン テーブルを作成。 |
| `create_file_geodatabase` | 新しいファイル ジオデータベースを作成。 |
| `truncate_table` | テーブル/フィーチャクラスの全行を削除し、スキーマは保持。 |
| `delete_dataset` | ディスクまたはジオデータベースからデータセットを削除。取り消し不可。 |
| `export_features` | 現在の選択と定義クエリを反映してレイヤを新しいデータセットにエクスポート。 |

### ジオプロセシング（7）

| ツール | 用途 |
|---|---|
| `run_geoprocessing_tool` | 任意の arcpy ジオプロセシング ツールを実行する万能チャネル。レイヤ名は実レイヤに解決され、選択と定義クエリが効きます。パラメータ名が不明な場合は先に describe_geoprocessing_tool を。 |
| `list_geoprocessing_tools` | 利用可能なジオプロセシング ツールを名前で検索。 |
| `list_toolboxes` | arcpy ツールボックス モジュールとプロジェクト内のツールボックスを一覧。 |
| `describe_geoprocessing_tool` | 実行前にツールのパラメータ、データ型、既定値、使用説明を取得。 |
| `run_python_toolbox_tool` | ディスク上のカスタム .pyt / .atbx / .tbx ツールボックスのツールを実行。 |
| `check_extension` | ArcGIS 拡張機能ライセンスを確認し、必要に応じてチェックアウト。 |
| `get_messages` | 直近のジオプロセシング操作のメッセージ。 |

### シンボル（6）

| ツール | 用途 |
|---|---|
| `set_layer_renderer` | レイヤのシンボルを変更：単一シンボル、カテゴリ別一意値、数値フィールドによる段階/連続色。 |
| `get_layer_symbology` | レイヤの現在のレンダラ、クラス区分、一意値、ラベル設定を確認。 |
| `list_color_ramps` | プロジェクトで利用可能なカラーランプを一覧。 |
| `set_layer_labeling` | ラベルのオン/オフ、式・フォント・ハローを設定。 |
| `apply_symbology_from_layer` | .lyrx ファイルまたは別レイヤからシンボルをコピー。 |
| `save_layer_file` | レイヤとシンボルを .lyrx として保存し再利用。 |

### マップビュー（7）

| ツール | 用途 |
|---|---|
| `get_map_view` | 現在のカメラ位置：中心、縮尺、回転、可視範囲。Pro で地図タブを開いている必要あり。 |
| `set_map_view` | マップビューを移動：範囲、中心点、縮尺、回転を設定。 |
| `export_map_view` | マップビューを PNG にレンダリングして画像として返す（目視確認用）。地図タブを開いている必要あり。 |
| `list_bookmarks` | 地図上の空間ブックマークを一覧。 |
| `create_bookmark` | 現在のビューまたは指定範囲を名前付きブックマークとして保存。 |
| `apply_bookmark` | マップビューをブックマークにズーム。 |
| `delete_bookmark` | 地図のブックマークを削除。 |

### レイアウトと印刷（16）

| ツール | 用途 |
|---|---|
| `list_layouts` | 印刷レイアウトをページサイズと要素数付きで一覧。 |
| `get_layout_info` | レイアウトを確認：ページ設定と各要素の位置・サイズ。 |
| `create_layout` | 新しいレイアウト ページを作成（既定でページ全面のマップフレーム付き）。 |
| `delete_layout` | プロジェクトからレイアウトを削除。 |
| `add_map_frame` | レイアウト ページの指定位置にマップフレームを追加。 |
| `set_map_frame_extent` | レイアウトのマップフレームをレイヤ、範囲、縮尺に向ける。 |
| `add_layout_text` | タイトルやクレジットなどのテキスト要素を追加。 |
| `add_layout_legend` | マップフレームに連動した凡例を追加。 |
| `add_layout_scale_bar` | マップフレームに連動した縮尺記号を追加。 |
| `add_layout_north_arrow` | マップフレームに連動した方位記号を追加。 |
| `add_layout_picture` | ロゴなどの画像をレイアウトに配置。 |
| `set_layout_element` | 任意のレイアウト要素の移動・サイズ変更・名前変更・非表示・テキスト変更。 |
| `delete_layout_element` | レイアウトから要素を削除。 |
| `export_layout` | レイアウトを PDF / PNG / JPEG / SVG / TIFF にエクスポート。PNG/JPEG は確認用にインラインで返る。 |
| `preview_layout` | レイアウトを一時画像にレンダリングして返す（ファイル書き込みなし）。構築中の確認に便利。 |
| `export_map_series` | レイアウトのマップシリーズ（マップブック）を複数ページ PDF にエクスポート。 |

### ラスタ（5）

| ツール | 用途 |
|---|---|
| `get_raster_info` | ラスタの詳細：バンド、サイズ、セル サイズ、ピクセル タイプ、統計、座標系。 |
| `set_raster_symbology` | ラスタ レイヤのカラライザとカラーランプを設定。 |
| `raster_calculator` | マップ代数：ラスタに短い別名を付け、Python 式を記述。Spatial Analyst が必要。 |
| `sample_raster_values` | 地図座標またはポイント レイヤのフィーチャ位置でラスタ セル値を読み取り。 |
| `zonal_statistics` | ゾーン ポリゴン内のラスタ値を集計して表を返す。Spatial Analyst が必要。 |

### カタログとデータ探索（6）

| ツール | 用途 |
|---|---|
| `list_workspace_contents` | ジオデータベースまたはフォルダ内のデータセットを一覧。既定はプロジェクトの既定ジオデータベース。 |
| `list_folder` | ディスク上の GIS ファイルとサブフォルダを一覧。既定はプロジェクトのホーム フォルダ。 |
| `describe_dataset` | 任意のデータセットをパスで説明：種類、ジオメトリ、座標系、範囲、フィールド、行数（地図に追加せず）。 |
| `search_data` | プロジェクトのジオデータベース、ホーム フォルダ、フォルダ接続から名前でデータを検索。追加前の最速の探し方。 |
| `get_project_items` | プロジェクトに登録されたフォルダ接続、データベース接続、ツールボックス。 |
| `add_folder_connection` | フォルダをプロジェクトに登録し、データを閲覧しやすく。 |

### ユーティリティ（4）

| ツール | 用途 |
|---|---|
| `execute_arcpy_code` | ArcGIS Pro 内で Python を実行。arcpy はインポート済み、arcpy.mp.ArcGISProject('CURRENT') は開いているプロジェクト、変数は呼び出し間で保持、print 出力は捕捉され、末尾の式の値が返ります。専用ツールがない時に使用。 |
| `get_pump_status` | メインスレッド ディスパッチャがインストールされているか。これがコマンドから開いている Pro プロジェクトへ到達できるかを決め、ない場合はパスベースの作業のみ可能。 |
| `stop_pump` | メインスレッド ディスパッチャを削除。ユーザーが再インストールするまでライブ プロジェクト系コマンドは動作しません。 |
| `run_batch` | 1 往復で複数コマンドを実行。多段ワークフローが大幅に速くなります。 |

</details>

---

## 📦 パッケージ内容

| パス | 内容 | 説明 |
|---|---|---|
| `install\ArcGISProMCP.esriAddinX` | 🧩 add-in v1.1.6（Customer 1.16 / 1.14 拡張版） | 115 ツール（112 コマンド）+ 全 GP ツール。HTTP MCP 6520 / 旧 TCP 6510 |
| `install\runtime\stdio-bridge.ps1` | 🌉 stdio ブリッジ | ローカルプロセスしか起動できないクライアント（Claude Desktop など）を 6520 に中継。純 PowerShell、Python 不要 |
| `install\runtime\runcode\RunCode.pyt` | 🐍 RunCode ツールボックス | 任意の arcpy を高頻度で実行する時に。初回実行で信頼ダイアログが一度表示 |
| `install\runtime\guard\` | 🛡️ ダイアログ ガード | 「サードパーティ コード」信頼ダイアログを自動クリック。失敗時は最前面で通知（30 秒以上、音あり） |
| `install\runtime\plugin\` | 🔌 Python ブリッジ（任意） | Pro の Python ウィンドウで起動し、`execute_arcpy_code` の永続セッションを提供（ポート 6511） |
| `install\*.cmd / *.ps1` | 🛠️ インストールと自己診断 | インストール / 自己診断 / Claude 設定 / ダイアログ ガード / アンインストール |
| `clients\` | 📁 クライアント接続説明 | Claude Code、Claude Desktop、Codex、汎用 JSON |
| `docs\` | 📖 使用ガイド、トラブルシューティング、作業ルール、更新履歴 | まず使用ガイドを |
| `source\` | 📜 完全なソース スナップショット | 本パッケージのバイナリに対応するソース（AGPL-3.0 準拠）。上流の LICENSE / NOTICE を含む |

### 🔍 依存関係の監査（「全部同梱」の理由）

- 🧩 add-in：自己完結の C# アセンブリ。サポート対象の Pro 3.7 に付属する .NET Desktop Runtime 10 x64 上で動作し、.NET の追加インストールは不要。
- 🌉 stdio ブリッジ、🛡️ ダイアログ ガード：Windows 標準の PowerShell 5.1 + WinForms で動作。追加インストール不要。
- 🐍 RunCode ツールボックス、Python ブリッジ：ArcGIS Pro 付属の Python（arcpy）で動作。別途 Python は不要。
- ✅ **したがって、pip やネット接続が必要な外部依存はありません。**「オフラインで使える」は本パッケージ自身のインストールと実行チェーンを指します。ArcGIS Pro 3.7 自体の前提（.NET Desktop Runtime 10 x64、WebView2 ≥132）は Pro のインストーラが提供します。

---

## 🧪 オプション コンポーネントの使い方

**🐍 RunCode ツールボックス**（arcpy を高頻度で使う場合）
- パス：`%LOCALAPPDATA%\ArcGIS Pro MCP\runcode\RunCode.pyt`
- AI が `run_python_toolbox_tool` を呼ぶ時にこのパスを渡すか、Pro の「挿入 → ツールボックス → ツールボックスの追加」で選択。
- Pro セッションごとの初回実行で「サードパーティ コード」信頼ダイアログが出ます：先に `03-dialog-guard.cmd` を実行すれば、ガードが「はい」を自動クリックします ✨。

**🔌 Python ブリッジ**（呼び出し間で arcpy セッションを保持したい場合）
1. ArcGIS Pro のメニュー **分析 → Python** で Python ウィンドウを開く；
2. 貼り付けて実行（パスは既定のインストール先）：
   ```python
   import sys; sys.path.insert(0, r"C:\Users\<ユーザー名>\AppData\Local\ArcGIS Pro MCP\plugin")
   import mcp_bridge; print(mcp_bridge.start_server(port=6511))
   ```
3. `CURRENT`（開いているプロジェクト）が必要な場合は 1 行追加（メインスレッドでの実行が必要）：
   ```python
   print(mcp_bridge.start_pump())
   ```

**🛡️ ダイアログ ガード**（RunCode / ENVI→Pro 連携フローの前に推奨）
```
03-dialog-guard.cmd                 # 600 秒ガード
03-dialog-guard.cmd -DurationSeconds 120
03-dialog-guard.cmd -NotifyTest     # 通知ウィンドウのみテスト（30 秒）
03-dialog-guard.cmd -AlertOnly      # 通知のみ、自動クリックなし
```
安全境界：「サードパーティ / コードライブラリ」の文脈を持つ信頼ダイアログのみ処理し、**上書き・削除などの確認ダイアログには一切触れません** 🙅。ログ：`%LOCALAPPDATA%\ArcGIS Pro MCP\guard\watch_pro_dialog.log`。

---

## 🔒 セキュリティについて

- 🏠 add-in とすべてのブリッジ サービスは `127.0.0.1` のみで待ち受け、LAN 上の他のマシンからは接続できません。
- ⚖️ 本ソフトウェアは `127.0.0.1` で本機サービスのみを提供し、第三者へのネットワーク サービスは提供しません。改変してネットワーク経由で他者に提供する場合、AGPL-3.0 §13 のソース提供義務が適用されます。
- 🔏 add-in は未署名ビルドです。初回読み込み時に ArcGIS Pro が信頼確認を求めます（正常な流れ）。本パッケージの `SHA256SUMS.txt` で検証してください。
- 🕵️ 本パッケージにはテレメトリやネットワーク報告は一切含まれません。
- 🧹 アンインストール：`99-uninstall.cmd`（クライアント設定を残すか選択可能）。

---

## 📜 バージョンとライセンス

- 🏷️ バージョン：**Customer 1.16（1.14 拡張版、ビルド番号 1.1.6）**（ビルド日は `VERSION.json`、ツール数 115）
- 👤 メンテナ：**Law**（[@XNYGggg](https://github.com/XNYGggg)）
- 🙏 上流プロジェクトと原作者：**[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)**、作者 **Pisut Nakmuenwai**（AGPL-3.0-or-later）。本パッケージはその上にパッチを 1.1.4 まで積み上げています（変更点は `docs\CHANGELOG.md` と `RELEASE-NOTES.md`）。
- ⚖️ ライセンス：**AGPL-3.0-or-later**（完全なソース スナップショットと上流 NOTICE を同梱）
- 🏷️ 商標：「ArcGIS」は Esri の商標です。本プロジェクトはサードパーティ製ツールであり、Esri とは無関係です。

🆘 よくある質問は `docs\排障手册.md`（中国語）、上級用法は `docs\使用指南.md`（中国語）。よい地図づくりを 🎨 (｀・ω・´)ゞ
