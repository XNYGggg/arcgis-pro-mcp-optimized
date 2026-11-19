[![English](https://img.shields.io/badge/Language-English-1f6feb?style=flat-square)](README.md) [![简体中文](https://img.shields.io/badge/Language-%E7%AE%80%E4%BD%93%E4%B8%AD%E6%96%87-d1242f?style=flat-square)](README.zh-CN.md) [![日本語](https://img.shields.io/badge/Language-%E6%97%A5%E6%9C%AC%E8%AA%9E-bc002d?style=flat-square)](README.ja.md) [![한국어](https://img.shields.io/badge/Language-%ED%95%9C%EA%B5%AD%EC%96%B4-0047a0?style=flat-square)](README.ko.md) [![Version](https://img.shields.io/badge/Version-Customer%201.1.11%20(1.14%20Enhanced)-2ea44f?style=flat-square)](VERSION.json) [![Changelog](https://img.shields.io/badge/Docs-Changelog-6f42c1?style=flat-square)](docs/CHANGELOG.md) [![License: AGPL v3+](https://img.shields.io/badge/License-AGPL--3.0--or--later-blue?style=flat-square)](LICENSE) [![ArcGIS Pro](https://img.shields.io/badge/ArcGIS%20Pro-3.7-00a1e0?style=flat-square)](https://www.esri.com/en-us/arcgis/products/arcgis-pro/overview) [![Platform](https://img.shields.io/badge/Platform-Windows%2011%20x64-lightgrey?style=flat-square)]()

# 🗺️ ArcGIS Pro MCP (Optimized) · Customer 1.1.11（1.14 拡張版）🚀

自然言語の一言で、AI アシスタントが開いている ArcGIS Pro プロジェクトを直接操作します——レイヤーの追加、シンボロジの変更、属性の検索、ジオプロセシングの実行、マップの出力。データはマシンの外に出ません。すべての通信は `127.0.0.1` 上で行われます 🔒。

> **🚀 このブランチは Customer 1.1.11（1.14 拡張版、ビルド番号 1.1.11）です**：**v1.1.4** の上に、1.14 以降の修正——1.1.5（F1–F10）、1.1.6（V-1–V-7）、2026-11-18 の実機フルテストで確認された P0–P2 欠陥の修正（1.1.7–1.1.9）、1.1.10 の再起動セルフヒーリング、1.1.11 のクリーンアップ修正——を積み重ねています。**v1.1.4 はすでに安定版として正式リリース済み（推奨）です**。このブランチは **独自の Release アセットを提供しません**——インストーラーは `install/` に同梱され、いつでも v1.1.4 にロールバックできます。

> **📌 バージョン注記（2026-11-16 更新）**：**1.0 ライン（v1.1.4 正式カスタマー版）は非常に安定しており、日常利用に最適です**。**2.0 ラインはテスト版**で、最適化が未完了のため GitHub Release アセットとタグは削除済み・**ダウンロード不可**です。2.0 のコードは `customer-v2.0.1` / `customer-v2.0.0` ブランチにのみ残っています。

> **🙏 アップストリームプロジェクトと原作者**：本カスタマー版は **[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)**（原作者 **Pisut Nakmuenwai**、AGPL-3.0-or-later）を基に、**v1.1.11** までローカルパッチを積み重ねたものです。完全な帰属表示と改変の告知は `NOTICE` および `source/SOURCE-NOTICE.md` にあります。

本パッケージは実行に必要な**すべてのコンポーネント**を同梱しており、完全オフラインで動作します。**Python のインストール不要、pip 不要、インターネット不要**で、ArcGIS Pro 本体以外に依存関係はありません。電池も付いてきます 🔋

---

## 🆚 1.14（v1.1.4）からのアップグレード内容

以下はすべて v1.1.4 カスタマーパッケージ以降に取り込まれた変更です。ツール数 113 → **115**（`add_table` と `restart_pro` を追加）。add-in のバイナリは `CC454A9F…5A50`（469,270 バイト）。

| ビルド | 種別 | 主な内容 |
|---|---|---|
| **1.1.5** | F1–F10 | `set_environment` の設定が実際に GP へ渡る（呼び出しごとの上書き対応）。`describe_geoprocessing_tool` が列挙値と既定値を返す。`create_map` は既定でベースマップなし（`project_default` で従来動作）。ラスタ/TIN のズームとシンボロジ読み取り。**`add_table` 新設**。`export_map_view` に `fit:"auto"`。`set_raster_symbology` が明示的な配色/ラベルに対応。`set_map_properties` の epsg が有効化。**`restart_pro` 新設** |
| **1.1.6** | V-1–V-7 | toolbox パスの誤記に「Did you mean …」を提示。名前付き引数エラーに使用例を添付。`get_pump_status` は Python ブリッジ未起動時に**構造化された down 状態**（`running:false, bridge_listening:false`）を返し、失敗扱いしない。エラーコード（`BRIDGE_NOT_LISTENING / BRIDGE_ERROR / BRIDGE_CLOSED / BRIDGE_TIMEOUT / UNSUPPORTED`）と `last_error_code/at`。状態照会に 5 秒の読み取りタイムアウト。`diagnose` が 6511 ポートを能動的にプローブ（`bridge_listening` / `probe_ms`）。ツール説明の修正（ネイティブ側は位置引数のみ、ブリッジ依存の注記）。HTTP トランスポートのアサーション強化。ブリッジ down の結果を 5 秒キャッシュ |
| **1.1.7** | P0 | `create_feature_class` / `create_table` の `fields` が**実際に機能**——作成後に `AddField` で逐次追加し、`describe_dataset` で名前/型/長さを検証。失敗時は作りかけのデータセットを削除。`run_batch` は画像を既定で path/bytes/sha に縮約（base64 でコンテキストを溢れさせない）。`get_features` にページング（`truncated` / `next_offset`）と `geometry_summary` / `geometry_precision`。`preview_layout` が `output_path` / `return_image` に対応 |
| **1.1.8** | P1 | `insert_features` / `update_features` / `delete_features` がスタンドアロン表とデータセットパスに対応（表は Table API、レイヤーは編集セッション）。`sample_raster_values` はネイティブなピクセルサンプリング——`points` / `point_layer`、インライン返却、副作用テーブル `raster_samples` なし（`write_table` で従来経路も維持）。`create_map` の SCENE を修正（`SceneLocal`）。`add_join` が GP 再登録後も維持（JoinRegistry + クライアント側接続、候補提示付き）。レイアウト要素の `x/y` を左下原点に統一し `GetBounds` で検証・範囲外は拒否。`run_batch` に各ステップのミリ秒と直列実行の表示。GP 出力のマップ追加は待機＋リトライ |
| **1.1.9** | P2 | `get_layer_info` / `set_raster_symbology` / `repair_layer_source` がデータセットパスを受付（一意一致は自動解決、複数一致はレイヤー名を列挙）。`repair_layer_source` にラスタ分岐を追加（`RasterDataset.ReplaceDataSource`）。`search_data` は重複排除。`check_extension` は `inspectable:false` のセマンティクスに修正（Pro はアドインにチェックアウトを公開しない）。GP 出力の空文字を除外。`get_features` はラスタレイヤー名に対して `get_raster_info` を案内 |
| **1.1.10** | 再起動セルフヒーリング | `restart_pro` のタイムアウト自己回復——保存ダイアログのプローブを子プロセス化し 15 秒のハードタイムアウト（超過時は断念してログ）。ボタン一致がローカライズされた `是(Y) / 否(N) / 取消(C)` に対応。`restart.log` は UTF-8 でフェーズ心跳を記録。TCP 接続は 5 秒タイムアウト。Pro を強制終了しない方針は不変。再起動スケジュールフラグは 10 分で失効し、アシスタントが落ちても新しいリクエストを永久に拒否しない。デプロイは `scripts/update_installed.py`（位置引数・ハッシュ不一致は拒否）に統一 |
| **1.1.11** | クリーンアップ修正 | `create_feature_class` / `create_table` の失敗時クリーンアップが、データセット削除前にデータソースでマップレイヤー/スタンドアロン表を解決し、オブジェクト同一性で削除——名前だけが同じ無関係なレイヤーは誤削除しない。クリーンアップの失敗はツールエラーに報告（例外を握りつぶさない）。`scripts/update_installed.py` の本機パス固定を廃止。ビルドはソースパスを DLL/PDB からマップ除去 |

本機実機検証（**ArcGIS Pro 3.7 のみ**）：1.1.7 回帰 21/21 PASS、1.1.8 連続 2 ラウンド 0 失敗、1.1.9 P2 10/10 と P1 再実行 0 失敗、1.1.10 ハンドシェイク `1.1.10.0`、1.1.11 ハンドシェイク `1.1.11.0`・ping `command_count=112`、D1 フィールド回帰は現行テストプロジェクトに適用可能なアサーションすべて PASS（4 件の旧アサーションは退役プロジェクトのレイヤー名を参照）、`get_features` のページング/サマリ/精度は実機再確認済み。他の Pro バージョンは未検証。詳細は `docs/CHANGELOG.md` と `RELEASE-NOTES.md`。

---

## 🖥️ システム要件

| 項目 | 要件 |
|---|---|
| 🪟 OS | **Windows 11 x64**（64 ビット Home / Pro / Enterprise。ArcGIS Pro 3.7 が公式サポート）。**アーキテクチャ：x64（AMD64）のみ——ARM64 非対応** |
| 🗺️ ArcGIS Pro | **3.7**（Pro 3.7 / .NET 10 向けビルド。実機検証は 3.7 のみ、以降のバージョンは未検証） |
| 🔑 権限 | 一般ユーザーで可。インストールに**管理者権限は不要** |
| 📦 その他の依存 | なし（PowerShell と .NET は Windows / Pro に同梱） |

---

## 🚀 5 分で始める

1. 📂 **解凍**：任意のフォルダに展開（例：`D:\ArcGIS-Pro-MCP-Customer`）。
2. 🛠️ **ArcGIS Pro を閉じて**、`install\00-install.cmd` をダブルクリック。アドインのインストール、ランタイムの配置、Claude Desktop / Claude Code の設定を自動で行います。
3. 🖥️ **ArcGIS Pro を起動**。初回読み込みで「アドインを信頼しますか」と聞かれたら**「はい」**をクリック（安全です 😄）。Pro 上部の **MCP タブ**で状態が **running**（ポート `127.0.0.1:6520`）であることを確認。
4. 🔌 **AI クライアントを接続**（下記「設定」参照）。次に `install\01-check.cmd` をダブルクリックし、すべて `[OK]` になれば準備完了 (๑•̀ㅂ•́)و✧

> チェック時に Pro が起動していないとポート検査は警告になります——Pro を起動して再実行してください 👌

---

## 🎛️ 設定 —— AI クライアントの接続

**前提**：ArcGIS Pro が起動中で、**MCP タブ**が **running** であること。すべてのポートは本機のみにバインドされます：

| ポート | 用途 |
|---|---|
| `6520` | MCP over HTTP —— すべての AI クライアントはここへ接続 |
| `6510` | 旧 TCP ブリッジ（最新クライアントは使用しない） |
| `6511` | オプションの Pro 内 Python ブリッジ（`execute_arcpy_code`） |

### 🟣 Claude Code

```powershell
claude mcp add --transport http arcgis http://127.0.0.1:6520/mcp
claude mcp list
```

手動での等価設定（`~/.claude.json`）：

```json
{ "mcpServers": { "arcgis": { "type": "http", "url": "http://127.0.0.1:6520/mcp" } } }
```

削除は `claude mcp remove arcgis`、再追加は上記コマンド。

### 🖥️ Claude Desktop（同梱の stdio ブリッジを使用）

Claude Desktop は HTTP に直接接続できず、ローカルプロセスの起動しかできません——パッケージ同梱の純 PowerShell ブリッジを使います（Python 不要）：

1. **Claude Desktop を完全終了**（トレイアイコン含む）。
2. `install\02-configure-claude.cmd` を実行、または手動で設定を編集（先にバックアップ）。
3. 設定ファイル `claude_desktop_config.json` は次のうち「存在する最初の」フォルダにあります：
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

4. Claude Desktop を再起動し、ツール一覧に `arcgis` が現れれば成功。

### 🟢 Codex

`~/.codex/config.toml` に追加（既存の `arcgis` ブロックは `env` サブテーブルも含め丸ごと置換）：

```toml
[mcp_servers.arcgis]
url = "http://127.0.0.1:6520/mcp"
```

保存後に Codex を再起動。

### 🔧 その他の MCP クライアント

Streamable HTTP で `http://127.0.0.1:6520/mcp` に接続するか、上記の汎用 JSON を使用。（Antigravity は `serverUrl` を使い `type` は不要。）

### ✅ 接続の確認

- `install\01-check.cmd` —— Pro のバージョン、アドインハッシュ、ランタイム、ポート、クライアント設定を検査。
- `netstat -ano | findstr 6520` —— ポートがリッスンしているか確認。
- ログ：`%LOCALAPPDATA%\ArcGIS-MCP\addin.log`。

---

## 🎯 使い方

**基本ループ**：データのあるプロジェクトを開く → やりたいことを話す → アシスタントがツールを選ぶ → 結果を見る（画像はインラインで返ります）🎨

| こう言えば | 実際に起きること |
|---|---|
| 「区県を面積で階級彩色、薄赤から濃緑へ」 | `set_layer_renderer`（階級色彩） |
| 「人口 50 万以上の県を選択して新規フィーチャクラスに書き出す」 | `select_features` → `export_features` |
| 「道路に 500 m のバッファを作る」 | `run_geoprocessing_tool`（Buffer） |
| 「郷鎮ごとの耕地面積を集計」 | `summarize_features` / `zonal_statistics` |
| 「今のマップビューのスクリーンショットを撮って」 | `export_map_view`（PNG を返却） |
| 「タイトル・凡例・縮尺記号・方位記号付きの A3 縦 PDF を出力」 | `create_layout` → `add_map_frame` → `add_layout_*` → `export_layout` |
| 「12 レイヤーを同じシンボロジで一括出力」 | `run_batch` または `execute_arcpy_code` |
| 「NAME・AREA_HA・CODE フィールドを持つ空のフィーチャクラスを作成」 | `create_feature_class`（フィールドは実際に作成・検証される） |

**ツール名で呼び出す**：下記の各ツールはクライアントから名前で直接呼び出せます。`run_batch` は複数コマンドを 1 往復にまとめます。専用ツールがない場合は `execute_arcpy_code` を使います。

**編集ルール**：編集は `save_edits` まで保留され、保留中はデータがロックされます（削除/上書きは失敗）。`discard_edits` で破棄。`delete_features` / `delete_dataset` は取り消し不可——先に影響範囲の説明を求めましょう。

**バッチと高速化**：反復の多い大規模作業は、RunCode 用の冪等 arcpy スクリプトを書かせて一度に実行。大規模プロジェクトは範囲/OID で分割しチェックポイントを残します。`run_batch` 内の画像結果は自動で path/bytes/sha に縮約され、会話を圧迫しません。

---

## 🧩 3 つのモジュール：役割・使い方・プロンプトの書き方

本プロジェクトは「1 リポジトリ・3 モジュール」設計です。各モジュールは独立して使え、実際には組み合わせて使います：

| モジュール | 一言の役割 | 使うとき | 本版の状態 |
|---|---|---|---|
| 🧩 **MCP アドイン** | AI と Pro を結ぶ唯一の幹線：115 の MCP ツール + RunCode / Python ブリッジの 2 つの arcpy 経路 | 日常のすべての操作、バッチスクリプト、`CURRENT` が必要な対話 | ✅ インストールすれば即利用可 |
| 🛡️ **ダイアログガード** | 「サードパーティコード」信頼ダイアログを自動クリック。押せなければ最前面でリング通知 | 初回の RunCode.pyt 実行、無人バッチ | ✅ 独立スクリプト `03-dialog-guard.cmd` |
| 🧪 **テンプレート蒸留** | サンプル図を再利用可能なレイアウトテンプレートに蒸留し、バッチ出力の様式を統一 | 多数の図に同じ様式、論文/報告書図の再現 | ❌ 本版には含まず。V2.01（v2.0.1）を参照 |

### 🧩 モジュール 1：MCP アドイン（コア）

- **役割**：AI と Pro の唯一のチャネル——レイヤー、シンボロジ、ジオプロセシング、マップ出力のすべてがここを通ります。arcpy は 2 経路：**RunCode ツールボックス**は単発/バッチ、**Python ブリッジ**は変数の永続化や `CURRENT` が必要な対話向け。
- **使い方**：
  1. インストール後 Pro を開き、上部の **MCP タブ**が **running**（`127.0.0.1:6520`）であることを確認；
  2. RunCode：アシスタントに `run_python_toolbox_tool` を `%LOCALAPPDATA%\ArcGIS Pro MCP\runcode\RunCode.pyt` のパスで呼ばせる；
  3. Python ブリッジ（任意）：Pro メニュー **分析 → Python** でブリッジの起動コードを貼り付け（ポート 6511。`CURRENT` が必要なら `start_pump()` を追加——後述「RunCode と Python ブリッジ」参照）。
- **プロンプトの書き方**：**対象 + 操作 + 出力**を明示し、バッチでは「まず 1 件作って確認させて」を添える。
  - ✅ 「`区県境界` を `AREA` で自然分類 5 級、薄赤から濃緑、凡例は右下、A3 横 PDF を `D:\out\` に 300 dpi で出力。」
  - ✅ 「RunCode で冪等スクリプト：全レイヤーのラベルをフィールド `NAME`・サイズ 10 でオン。既にオンのものはスキップ。」
  - ✅ 「この arcpy は `CURRENT` が必要：ブリッジ起動コードを私のユーザー名で埋めて送ってください。Pro に自分で貼ります。」
  - ❌ 「データを適当に処理して」——対象・操作・出力がなく、アシスタントは聞き返すしかありません。

### 🛡️ モジュール 2：ダイアログガード（RunCode 実行時のみ必要）

- **役割**：Pro が初めて第三者 `.pyt` を実行するとき「サードパーティコードライブラリ」信頼ダイアログが出て、無人実行が止まります。ガードはこの「はい(Y)」だけをクリックし、押せない場合は最前面通知（最低 30 秒、10 秒ごとにリング）。上書き・削除などの確認ダイアログには**一切触れません**。
- **使い方**（詳細は後述「ダイアログガードの使い方」）：
  ```text
  install\03-dialog-guard.cmd                    # 既定 600 秒
  install\03-dialog-guard.cmd -DurationSeconds 120
  install\03-dialog-guard.cmd -AlertOnly         # 通知のみ、クリックしない
  ```
  Pro プロセスごとに初回の信頼で済みますが、Pro 再起動後は再度ガードが必要です。
- **プロンプトの書き方**：「ガード起動」と「何を実行するか」を同じ文に入れる。
  - ✅ 「RunCode.pyt を実行します：バックグラウンドでダイアログガードを 600 秒起動、信頼ダイアログは『はい』を自動クリック、押せなければ最前面でリング通知して。」
  - ✅ 「今回は通知のみ、自動クリックしないで。」
  - ❌ 「すべてのダイアログを消して」——ガードは信頼ダイアログのみ担当。他の確認はあなたの判断です。

### 🧪 モジュール 3：テンプレート蒸留（2.x 機能）

- **役割**：サンプルの完成図を再利用可能なレイアウトテンプレートに蒸留。以後のバッチ出力はテンプレート適用で様式統一・再現可能に。
- **使い方**：本版には**含まれません**（`template_*` ツールなし）。**V2.01（v2.0.1）** にアップグレード後：`import → measure → patch → validate → preview → review → approve → apply` の流れで 10 個の `template_*` ツールを呼び出します。テンプレートライブラリは `%LOCALAPPDATA%\ArcGIS Pro MCP\library` にあり、アップグレード時も保持されます。
- **プロンプトの書き方**：テンプレートに名前を付け、流れと確認ポイントを書き込みます。
  - ✅ 「`D:\samples\fig3.png` を『研究区概観図』というテンプレートに蒸留：import → measure → patch → validate → preview、プレビューをまず私に確認させて、確認後に review + approve。」
  - ✅ 「『研究区概観図』テンプレートで `maps` の各マップを A3 PDF に出力。タイトルはマップ名、出力先は `D:\out\maps\`。」
  - ❌ 「この図のようにやって」——テンプレート名・流れ・確認点がなく、様式を再現できません。

### 🧭 プロンプトの共通原則（3 モジュール共通）

1. **モジュールを指名**：通常操作は指名不要（既定は MCP）。スクリプトは「**RunCode** を使って」、永続セッションは「**Python ブリッジ**を使って」、無人実行は先に「**ダイアログガード**を起動」、様式統一は「**テンプレート**を使って」。
2. **対象を具体的に**：レイヤー名・フィールド名・パスはバッククォートでそのまま書く。「あのレイヤー」「さっきのデータ」は避ける。
3. **出力を固定**：形式（PNG / PDF / 新規フィーチャクラス）、サイズや解像度、保存先。
4. **確認ポイントを残す**：バッチ・上書き・削除系は「まず 1 件見せて」または「先に影響範囲を説明して」。

> マップ出力プロンプトの完全な四要素とテンプレートは次の節を参照。

---

## 🎨 マップ作成の使い方（プロンプトガイド）

この節では「AI への伝え方」を掘り下げます。ツール名を覚える必要はなく、次の四要素を伝えれば、アシスタントが MCP 呼び出しに分解します。

### 四つを言えば一度で合う

| 要素 | 伝えること | 例 |
|---|---|---|
| データ | レイヤー名、データパス、現在の選択 | 「現在のマップの `区県境界` レイヤー」 |
| 操作 | 分類フィールド/方法、バッファ距離、統計の口径 | 「`AREA` で 5 級、自然分類」 |
| スタイル | 配色、フォントサイズ、凡例/縮尺記号/方位記号の位置、ベースマップ | 「薄赤から濃緑、凡例は右下」 |
| 出力 | 画像/PDF/新規フィーチャクラス、解像度、保存先 | 「A3 横 PDF、300 dpi、`D:\out\` へ」 |

**良いプロンプト vs 悪いプロンプト**
- ❌ 「地図をきれいにして」——目標がなく、アシスタントは推測するしかない。
- ✅ 「`土地利用` を `DLMC` の一意値でシンボル化、凡例は右下、A3 横 PDF を `D:\out\landuse.pdf` に 300 dpi で出力。」
- ❌ 「地図出して」——範囲・レイアウト・出力がない。
- ✅ 「`研究区` レイヤーを範囲に、縮尺 1:10000、A4 縦 PNG、タイトル『研究区土地利用現況』、凡例・縮尺記号・方位記号付き。まずプレビューを確認させて。」

### ツールグループ別プロンプト早見表

| モジュール（ツール群） | こう言える | アシスタントが使うもの |
|---|---|---|
| セッションとプロジェクト | 「まず `diagnose` で接続と現在のプロジェクトを確認、その後プロジェクトを `D:\proj\成果.aprx` に名前を付けて保存」 | diagnose / save_project |
| レイヤー | 「`roads` を `ベース` グループへ移動、透明度 40%、1:50000 より拡大時のみ表示。`POI` はオフ」 | move_layer / set_layer_transparency / set_layer_scale_range / set_layer_visibility |
| データと属性 | 「`parcels` の先頭 20 行を面積降順で読む。`DLMC` 別の面積を集計」 | get_features / summarize_features |
| 選択 | 「`POP` が 50 万超の県を選択し `D:\out\counties_big.shp` に出力。次に `研究区` 内の地塊を位置で選択」 | select_features / select_by_location / export_features |
| フィールドとデータセット | 「`parcels` に DOUBLE フィールド `AREA_HA` を追加し `!Shape_Area!/10000` で計算。EPSG:4547 のポリゴンフィーチャクラス `成果_2026` を新規作成」 | add_field / calculate_field / create_feature_class |
| ジオプロセシング | 「道路に 500 m バッファを作り融合。`研究区` のマスクで土地利用ラスタをクリップ」 | run_geoprocessing_tool / run_batch |
| シンボロジ | 「面積で 5 級・自然分類・薄赤から濃緑。または `TYPE` で一意値配色」 | set_layer_renderer |
| マップビュー | 「`研究区` にズーム、縮尺 1:10000、1600×900 の PNG を」 | set_map_view / zoom_to_layer / export_map_view |
| レイアウトと出力 | 「A3 横レイアウトを作成、タイトル…、凡例は 3 レイヤーのみ、縮尺記号と方位記号を追加、300 dpi PDF を出力」 | create_layout / add_layout_* / export_layout |
| ラスタ | 「DEM のバンド・セルサイズ・統計を読む。NDVI を計算」 | get_raster_info / raster_calculator / zonal_statistics |
| カタログとデータ探索 | 「プロジェクトとフォルダ接続から名前に `2025` を含む shapefile を検索」 | search_data / describe_dataset / list_folder |
| ユーティリティ | 「一度に：ベースマップを隠す→区県へズーム→PNG 出力。arcpy で全レイヤー名に接頭辞を付与」 | run_batch / execute_arcpy_code |

### 標準マップ出力プロンプトテンプレート（コピーして使う）

```text
現在のプロジェクトの <マップ名> で地図を出力：
- 範囲：<研究区レイヤー/範囲>、縮尺 1:<...>
- データ：<レイヤー1>、<レイヤー2>（必要に応じてフィールドとフィルター条件を指定）
- シンボル：<フィールドで階級/一意値/単色>、配色 <ランプまたは16進>
- レイアウト：<A3 横 / A4 縦>、タイトル "<タイトル>"、凡例 <位置>、縮尺記号 <位置>、方位記号 <位置>
- 出力：<PDF / PNG>、<300> dpi、<絶対パス> に保存
- 流れ：まずレイアウトを自己点検し、プレビューを私に確認させて、確認後に正式ファイルを出力。
```

### バッチ出力プロンプト

```text
`maps` の各マップを、同一の A3 横レイアウトで PDF に出力：
タイトルはマップ名、凡例は表示中のレイヤー、縮尺記号と方位記号は右下、
出力先は `D:\out\maps\`、300 dpi。まず 1 枚だけ出して確認させて、OK 後にバッチ。
```

### よくある失敗の一言リカバリ

- 「凡例が主図に重なった」→ アシスタントが `legend` の座標やレイアウトを調整。
- 「ページ比率が変/余白が多すぎる」→ map frame とページ比率を調整。
- 「配色が派手すぎる/論文風でない」→ ランプ、16 進色、参考画像を指定。
- 「まだ出力しないで、プレビューを見せて」→ アシスタントは先に `export_map_view` またはレイアウトプレビュー。

**効果**：「データ・操作・スタイル・出力」を一度に伝えれば、通常 1 ラウンドで最終稿に近い図ができます。以後の修正は差分だけ（「凡例を右へ 1 cm」）で済みます。

---

## 🛡️ ダイアログガードの使い方（1.x 版）

**何を解決するか**：ArcGIS Pro が初めて第三者 `.pyt` / Python ツール（RunCode.pyt など）を実行すると「サードパーティコードライブラリ」信頼ダイアログが出て、無人実行が止まります。本版には MCP ガードツールはなく、パッケージに**独立したガードスクリプト**を同梱しています：起動すると Pro のウィンドウをポーリングし、この種の「はい(Y)」だけをクリックし、設定時間で自動終了します。

**起動方法**

```text
install\03-dialog-guard.cmd                    # 既定 600 秒
install\03-dialog-guard.cmd -DurationSeconds 120
install\03-dialog-guard.cmd -AlertOnly         # 最前面通知のみ、自動クリックなし
install\03-dialog-guard.cmd -NotifyTest        # 通知ウィンドウのテスト（30 秒）
```

`03-dialog-guard.cmd` を直接ダブルクリックするか、ターミナル操作ができる AI アシスタントにバックグラウンド起動させてもよいです。

**プロンプト例**

| あなたの発言 | アシスタントの動作 |
|---|---|
| 「Pro で手動で RunCode.pyt を実行します。先にダイアログガードを 600 秒起動して。第三者コード信頼ダイアログが出たら『はい』を自動クリック、押せなければ最前面でリング通知」 | バックグラウンドで `install\03-dialog-guard.cmd -DurationSeconds 600` を実行 |
| 「今回は通知のみ、自動クリックしないで」 | `-AlertOnly` で起動 |
| 「通知ウィンドウが出るかテストして」 | `-NotifyTest`（30 秒）を起動 |
| 「ガードのログはどこ？」 | `%LOCALAPPDATA%\ArcGIS Pro MCP\guard\watch_pro_dialog.log` |

**境界と効果**
- 「サードパーティ / コードライブラリ / third-party / code library」の文脈を持つ信頼ダイアログのみ処理。上書き・削除などの確認ダイアログには一切触れない。
- ボタンが見つからない場合、最前面通知は最低 30 秒表示され 10 秒ごとにリング。
- Pro プロセスごとに初回信頼の後は出なくなる。Pro 再起動後は再度必要。
- 2.x では同じ機能が `guard_summon` / `guard_status` に進化し、RunCode 呼び出し前に自動 arm/settle。本版はスクリプトを手動起動。

---

## 🐍 RunCode と Python ブリッジの使い方（プロンプトガイド）

arcpy チャネルは 2 段階：**RunCode ツールボックス**は単発/バッチ、**Python ブリッジ**は変数を跨呼び出しで保持する対話向け。

### RunCode ツールボックス

パス：`%LOCALAPPDATA%\ArcGIS Pro MCP\runcode\RunCode.pyt`。アシスタントに `run_python_toolbox_tool` をこのパスで呼ばせるだけ。Pro の「挿入 → ツールボックス → ツールボックスの追加」で自分で追加してもよいです。

**プロンプト例**
- 「RunCode.pyt で現在のプロジェクト全レイヤーのラベルをオン、フィールド `NAME`・サイズ 10。」
- 「冪等な arcpy スクリプトを書いて RunCode で全レイヤー名に接頭辞 `LYR_` を付与。付け済みはスキップ。」
- 「このスクリプトは RunCode.pyt 内で実行します。先に引数とパスを検査してから実行して。」

**初回実行**時に「サードパーティコードライブラリ」信頼ダイアログが出ます：上記のダイアログガードを先に起動するか、一度手動で「はい」をクリック。同一 Pro プロセスでは 1 回だけ。

### Python ブリッジ（`CURRENT` または永続変数が必要なとき）

1. Pro メニュー **分析 → Python** で Python ウィンドウを開く；
2. 貼り付けて実行（既定インストール先のパス）：

```python
import sys; sys.path.insert(0, r"C:\Users\<ユーザー名>\AppData\Local\ArcGIS Pro MCP\plugin")
import mcp_bridge; print(mcp_bridge.start_server(port=6511))
```

3. `CURRENT`（現在のプロジェクト）が必要ならもう一行（メインスレッドで実行必須）：

```python
print(mcp_bridge.start_pump())
```

**プロンプト例**
- 「この後の arcpy は `CURRENT` が必要。Pro の Python ウィンドウにブリッジ起動コードを貼るよう促して、その後 `execute_arcpy_code` で実行して。」
- 「ブリッジ起動コードを私のユーザー名で埋めてコピーして。Pro に自分で貼ります。」
- 「先に `get_pump_status` で pump の準備を確認。未準備なら貼り付けを促して。」
- 「永続セッションで 3 段階：まずレイヤー読み取り、次にフィールド作成、最後に計算——変数は呼び出し間で保持。」

**効果**：RunCode は「バッチを一度に」、Python ブリッジは「多段対話・現在プロジェクト」を担います。どちらも Python の別途インストールは不要。

---

## 🧪 蒸留モジュールについて（2.x 機能）

本版はテンプレート蒸留を**含みません**：`template_*` ツールも `guard_summon` / `guard_status` もありません。サンプル図のレイアウトを再利用可能なテンプレートにし、多数の図の様式をそろえたい場合は **V2.01（v2.0.1）** にアップグレードしてください：

- 2.x は 10 個の `template_*` ツールを提供：import / measure / patch / validate / preview / review / approve / apply / list / get；
- テンプレートライブラリは `%LOCALAPPDATA%\ArcGIS Pro MCP\library`（アップグレードで保持）；
- 典型的な使い方：「`D:\samples\fig3.png` をテンプレート『研究区概観図』に蒸留。import → measure → patch → validate → preview の流れで、プレビュー確認後に review + approve。」

1.1.11 と 2.x の能力比較：

| 能力 | 1.1.11（本ブランチ） | 2.0.1（推奨アップグレード） |
|---|---|---|
| ネイティブ MCP ツール | 115 個（12 グループ） | 125 個（14 グループ） |
| RunCode / Python ブリッジ | ✅ | ✅ |
| ダイアログガード | 独立スクリプト `03-dialog-guard.cmd` | `guard_summon` / `guard_status` + RunCode 自動 arm/settle |
| テンプレート蒸留 | ❌ | ✅ 10 個の `template_*` ツール + schema v1.1 |

---

## 🧰 全 115 個の MCP ツール（12 グループ）

アシスタントにできることの全リストです。クリックで展開 👇

<details>
<summary><b>📜 完全ツール一覧 —— セッション 16 · レイヤー 20 · データ 11 · 選択 6 · フィールド/データセット 11 · ジオプロセシング 7 · シンボロジ 6 · ビュー 7 · レイアウト 16 · ラスタ 5 · カタログ 6 · ユーティリティ 4</b></summary>

### セッションとプロジェクト（16）

| ツール | 用途 |
|---|---|
| `ping` | ブリッジの到達性を確認し、接続中のプロジェクトを返す。 |
| `get_capabilities` | ブリッジが対応する全コマンドを領域別に列挙し、名前付きツール以外の能力を発見する。 |
| `get_queue_status` | 3 レーンキューの状態：レーン別の深さ、実行中のコマンド、最近の完了。遅い/待たされるときにまず見る。 |
| `diagnose` | 接続の自己診断：Pro バージョン、ライセンス、開いているプロジェクト、アクティブマップ、マップビュー、書き込み権限。Python ブリッジのポートも能動プローブ（`bridge_listening` / `probe_ms`）。問題時はまずこれ。 |
| `get_arcgis_info` | ArcGIS Pro のバージョン、ライセンスレベル、利用可能な拡張、現在のプロジェクトパス。 |
| `get_project_info` | プロジェクトパス、既定のジオデータベースとツールボックス、マップ、レイアウト、フォルダ/データベース接続。 |
| `save_project` | プロジェクト（.aprx）を保存、または別の場所にコピー保存。 |
| `list_maps` | すべてのマップとシーンを座標系・レイヤー数付きで列挙。 |
| `create_map` | プロジェクトにマップまたはシーンを新規作成（既定でベースマップなし。`project_default` で従来動作）。 |
| `remove_map` | プロジェクトからマップを削除。 |
| `activate_map` | Pro UI でマップビューを開く/アクティブ化。カメラ系コマンドと export_map_view の前に必須。 |
| `set_map_properties` | マップの名前変更や座標系の変更（`epsg` が有効）。 |
| `get_map_extent` | マップ内の全データレイヤーの合成範囲。 |
| `get_environment` | 現在の arcpy ジオプロセシング環境を読み取る。 |
| `set_environment` | arcpy 環境（ワークスペース、出力座標系、範囲、マスク、セルサイズ、上書き出力、並列係数、スナップ ラスタなど）を設定。値は実際に GP へ渡り、呼び出しごとの上書きも可能。 |
| `restart_pro` | プロジェクトを保存・再オープンしてから ArcGIS Pro を再起動——再ビルドしたアドインをデスクトップ操作なしで読み込む方法。応答は Pro 終了前に返り、ブリッジ復帰後にクライアントが再接続する。 |

### レイヤー（20）

| ツール | 用途 |
|---|---|
| `get_layers` | マップ内のレイヤー（描画順とグループ入れ子）とスタンドアロン表を列挙。プロジェクト把握はここから。 |
| `get_layer_info` | レイヤー 1 つの詳細：データソース、座標系、範囲、フィールド、フィーチャ数、レンダラー、ラベル状態。データセットパスも可（1.1.9+）。 |
| `add_layer` | パスまたはサービス URL からデータを追加：フィーチャクラス、シェープファイル、ラスタ、表、.lyrx、Web サービス。 |
| `add_table` | スタンドアロン表（ジオデータベース表またはフォルダ内の表）をマップに追加。 |
| `add_web_layer` | URL で Web レイヤーを追加（Feature/Map/Image サービス、WMS、WMTS、ベクタータイル）。 |
| `remove_layer` | マップからレイヤーまたはスタンドアロン表を削除。 |
| `rename_layer` | コンテンツリストのレイヤー名を変更。 |
| `duplicate_layer` | マップ内でレイヤーを複製し、別のシンボロジで表示可能に。 |
| `set_layer_visibility` | 1 つ/複数/全レイヤーの表示切替。 |
| `set_layer_transparency` | レイヤーの透明度（0 不透明、100 透明）。 |
| `set_layer_scale_range` | レイヤーが描画される縮尺範囲を制限。 |
| `set_definition_query` | レイヤーの定義クエリ（SQL where）を設定。空文字で解除。 |
| `move_layer` | レイヤーの順序変更、またはグループへ移動。 |
| `create_group_layer` | グループレイヤーを作成（既存レイヤーを移動可能）。 |
| `zoom_to_layer` | マップビューをレイヤー範囲へズーム。 |
| `set_basemap` | ベースマップを設定：地形、イメージ、イメージ ハイブリッド、ストリート、ナビゲーション、ライトグレー、ダークグレー、地形陰影、海洋、OpenStreetMap、ナショナルジオグラフィックなど。 |
| `get_broken_layers` | 全マップでデータソースが欠落したレイヤーを列挙。 |
| `repair_layer_source` | レイヤーを新しいワークスペース/データセットに向け直して修復（ラスタも対応、1.1.9+）。 |
| `add_join` | 共通フィールドで表をレイヤーに結合（クライアント側 JoinRegistry。GP 再登録後も維持）。 |
| `remove_join` | レイヤーの結合を解除。 |

### データと属性（11）

| ツール | 用途 |
|---|---|
| `get_features` | レイヤー/表/データセットパスから属性行を読み取り。where、フィールド部分集合、並び順、ジオメトリ対応。ページは `truncated` / `next_offset` を返し、ジオメトリは要約または精度指定付き WKT。 |
| `count_features` | フィーチャ数を集計（where 可）。 |
| `get_unique_values` | フィールドの一意値と行数。 |
| `get_field_statistics` | 数値フィールドの min/max/mean/median/sum/標準偏差。 |
| `summarize_features` | 1 つ以上のフィールドでグループ集計。「カテゴリごとに何件/いくつ」を GP なしで即答。 |
| `insert_features` | レイヤー、スタンドアロン表、データセットパスへ行を挿入。 |
| `update_features` | where 条件に一致する行の属性と/またはジオメトリを更新（レイヤーと表の両方）。 |
| `delete_features` | where 条件に一致する行を削除（レイヤーと表の両方）。 |
| `save_edits` | 保留中の編集をコミット。未保存の編集はデータをロックし、削除/上書きが失敗する。 |
| `discard_edits` | 保留中の編集を破棄。 |
| `calculate_field` | フィールド計算。例：`!Shape_Area! / 10000`。 |

### 選択（6）

| ツール | 用途 |
|---|---|
| `select_features` | SQL where 条件でフィーチャを選択。 |
| `select_by_location` | 別レイヤーとの空間関係で選択。 |
| `get_selection` | 現在の選択状況をレイヤー別に報告（ObjectID と任意で属性行）。 |
| `set_selection` | ObjectID 指定で選択。 |
| `clear_selection` | 単一レイヤーまたはマップ全体の選択を解除。 |
| `zoom_to_selection` | マップビューを選択フィーチャへズーム。 |

### フィールドとデータセット（11）

| ツール | 用途 |
|---|---|
| `list_fields` | レイヤーのフィールドを型・別名・長さ・ドメイン付きで列挙。 |
| `add_field` | レイヤーまたは表にフィールドを追加。 |
| `add_fields` | 1 回の呼び出しで複数フィールドを追加。 |
| `delete_field` | 1 つ以上のフィールドを削除。 |
| `alter_field` | フィールド名または別名/長さを変更。 |
| `create_feature_class` | 空のフィーチャクラスを作成（既定はプロジェクトの既定ジオデータベース）しマップに追加。要求フィールドは作成後に逐次追加・検証され、不一致時は新データセットを削除して失敗。 |
| `create_table` | 空のスタンドアロン表を作成。要求フィールドは作成後に逐次追加・検証され、不一致時は新表を削除して失敗。 |
| `create_file_geodatabase` | 新しいファイル ジオデータベースを作成。 |
| `truncate_table` | 表/フィーチャクラスの全行を削除（スキーマ保持）。 |
| `delete_dataset` | ディスクまたはジオデータベースからデータセットを削除。取り消し不可。 |
| `export_features` | 選択と定義クエリを反映してレイヤーを新データセットに出力。 |

### ジオプロセシング（7）

| ツール | 用途 |
|---|---|
| `run_geoprocessing_tool` | 任意の arcpy ジオプロセシング ツールを実行——分析の汎用チャネル。レイヤー名はライブ レイヤーに解決され、選択と定義クエリが反映される。引数名が不明なら先に describe_geoprocessing_tool。 |
| `list_geoprocessing_tools` | 利用可能なジオプロセシング ツールを名前で検索。 |
| `list_toolboxes` | arcpy ツールボックスモジュールとプロジェクト内のツールボックスを列挙。 |
| `describe_geoprocessing_tool` | 実行前に引数、データ型、既定値、列挙値、使用説明を取得。 |
| `run_python_toolbox_tool` | ディスク上のカスタム .pyt / .atbx / .tbx のツールを実行（ネイティブ側は位置引数のみ）。パス誤記時は「Did you mean …」を提示。 |
| `check_extension` | アドインが拡張ライセンスを検査できるかを報告（Pro では `inspectable:false`——不足の有無は実際にツールを実行して判断）。 |
| `get_messages` | 直近のジオプロセシング操作のメッセージ。 |

### シンボロジ（6）

| ツール | 用途 |
|---|---|
| `set_layer_renderer` | レイヤーのシンボロジ変更：単一シンボル、カテゴリ別一意値、数値フィールドによる階級/連続カラー。 |
| `get_layer_symbology` | 現在のレンダラー、階級の区切り、一意値、ラベル設定を確認。 |
| `list_color_ramps` | プロジェクトで利用可能なカラーランプを列挙。 |
| `set_layer_labeling` | ラベルのオン/オフと式、フォント、ハローを設定。 |
| `apply_symbology_from_layer` | .lyrx または別レイヤーからシンボロジをコピー。 |
| `save_layer_file` | レイヤーとシンボロジを .lyrx に保存し再利用。 |

### マップビュー（7）

| ツール | 用途 |
|---|---|
| `get_map_view` | 現在のカメラ位置：中心、縮尺、回転、可視範囲。Pro でマップタブを開いている必要あり。 |
| `set_map_view` | マップビューを移動：範囲、中心、縮尺、回転を設定。 |
| `export_map_view` | マップビューを PNG にレンダリングして画像を返す（目視確認用）。マップタブが必要。`fit:"auto"` でビュー比率に合わせる。 |
| `list_bookmarks` | マップの空間ブックマークを列挙。 |
| `create_bookmark` | 現在のビューまたは指定範囲を名前付きブックマークに保存。 |
| `apply_bookmark` | マップビューをブックマークへズーム。 |
| `delete_bookmark` | ブックマークを削除。 |

### レイアウトと印刷（16）

| ツール | 用途 |
|---|---|
| `list_layouts` | 印刷レイアウトをページサイズと要素数付きで列挙。 |
| `get_layout_info` | レイアウトを検査：ページ設定と各要素の位置・サイズ。 |
| `create_layout` | 新しいレイアウトページを作成（既定でページを満たすマップフレーム付き）。 |
| `delete_layout` | プロジェクトからレイアウトを削除。 |
| `add_map_frame` | 指定ページ位置にマップフレームを追加。 |
| `set_map_frame_extent` | レイアウトのマップフレームをレイヤー、範囲、縮尺に合わせる。 |
| `add_layout_text` | タイトルやクレジットなどのテキスト要素を追加。 |
| `add_layout_legend` | マップフレームに連動する凡例を追加。 |
| `add_layout_scale_bar` | マップフレームに連動する縮尺記号を追加。 |
| `add_layout_north_arrow` | マップフレームに連動する方位記号を追加。 |
| `add_layout_picture` | レイアウトに画像（ロゴなど）を配置。 |
| `set_layout_element` | 任意のレイアウト要素を移動・リサイズ・改名・非表示・テキスト変更（ページ座標は左下原点。範囲外は拒否）。 |
| `delete_layout_element` | レイアウトから要素を削除。 |
| `export_layout` | レイアウトを PDF / PNG / JPEG / SVG / TIFF に出力。PNG/JPEG はインラインで返り確認可能。 |
| `preview_layout` | レイアウトを一時画像にレンダリング（`output_path` 指定時のみ書き込み）。作成中の確認用。 |
| `export_map_series` | レイアウトのマップシリーズ（マップブック）を複数ページ PDF に出力。 |

### ラスタ（5）

| ツール | 用途 |
|---|---|
| `get_raster_info` | ラスタの詳細：バンド、サイズ、セルサイズ、ピクセル型、統計、座標系。 |
| `set_raster_symbology` | ラスタレイヤーのカラライザーとカラーランプを設定。明示的な色とラベルに対応。 |
| `raster_calculator` | マップ代数：ラスタを短い別名に束ねて Python 式を書く。Spatial Analyst が必要。 |
| `sample_raster_values` | マップ座標（`points`）またはポイントレイヤーのフィーチャでセル値を読み取り——インライン返却、`write_table=true` 以外は副作用テーブルなし。 |
| `zonal_statistics` | ゾーンポリゴン内のラスタ値を集計してテーブルを返す。Spatial Analyst が必要。 |

### カタログとデータ探索（6）

| ツール | 用途 |
|---|---|
| `list_workspace_contents` | ジオデータベースまたはフォルダ内のデータセットを列挙。既定はプロジェクトの既定ジオデータベース。 |
| `list_folder` | ディスク上の GIS ファイルとサブフォルダを列挙。既定はプロジェクトのホームフォルダ。 |
| `describe_dataset` | 任意のデータセットをパスで記述：型、ジオメトリ、座標系、範囲、フィールド、行数（マップに追加不要）。 |
| `search_data` | プロジェクトのジオデータベース、ホームフォルダ、フォルダ接続から名前で検索——データ追加前の最速の探索手段。重複排除済み。 |
| `get_project_items` | プロジェクトに登録されたフォルダ接続、データベース接続、ツールボックス。 |
| `add_folder_connection` | フォルダをプロジェクトに登録して参照しやすくする。 |

### ユーティリティ（4）

| ツール | 用途 |
|---|---|
| `execute_arcpy_code` | ArcGIS Pro 内で Python を実行：arcpy はインポート済み、arcpy.mp.ArcGISProject('CURRENT') は開いているプロジェクト、変数は呼び出し間で保持、print 出力は捕捉され、末尾の式の値が返る。専用ツールがない場合に使用。Python ブリッジ（6511）が応答。 |
| `get_pump_status` | メインスレッド ディスパッチャーが導入済みか。Python ブリッジ未起動時は構造化された down 状態（`running:false, bridge_listening:false`、code と hint 付き）を返し、エラーにしない。 |
| `stop_pump` | メインスレッド ディスパッチャーを除去。ライブプロジェクト系コマンドは再導入まで動作しない。 |
| `run_batch` | 複数コマンドを 1 往復で実行——多段ワークフローが大幅に速い。画像結果は既定で path/bytes/sha に縮約され、各ステップのミリ秒も返る。 |

</details>

---

## 📦 パッケージ内容

| パス | 内容 | 説明 |
|---|---|---|
| `install\ArcGISProMCP.esriAddinX` | 🧩 add-in v1.1.11 | 115 の MCP ツール + すべての GP ツール。HTTP MCP 6520 / 旧 TCP 6510 |
| `install\runtime\stdio-bridge.ps1` | 🌉 stdio ブリッジ | ローカルプロセスしか起動できないクライアント（Claude Desktop など）を 6520 に接続。純 PowerShell、Python 不要 |
| `install\runtime\runcode\RunCode.pyt` | 🐍 RunCode ツールボックス | 任意の arcpy を高頻度実行するときに使用。初回は信頼確認が 1 回出る |
| `install\runtime\guard\` | 🛡️ ダイアログガード | 「サードパーティコードライブラリ実行」信頼ダイアログを自動クリック。失敗時は最前面通知（30 秒以上・リング） |
| `install\runtime\plugin\` | 🔌 Python ブリッジ（任意） | Pro の Python ウィンドウで起動し、`execute_arcpy_code` の永続セッションを提供（ポート 6511） |
| `install\*.cmd / *.ps1` | 🛠️ インストールと自己診断 | インストール / チェック / Claude 設定 / ダイアログガード / アンインストール |
| `clients\` | 📁 クライアント接続ガイド | Claude Code、Claude Desktop、Codex、汎用 JSON |
| `docs\` | 📖 使用ガイド、トラブルシューティング、作業ルール、変更履歴 | まず使用ガイド |
| `source\` | 📜 完全ソーススナップショット | 本バイナリに対応するソース（AGPL-3.0 遵守）。上流の LICENSE / NOTICE とビルド成果物 `source\dist\` を含む |

### 🔍 依存関係監査（「全部同梱」の根拠）

- 🧩 add-in：自己完結の C# アセンブリ。Pro 3.7 のサポート環境に付属する .NET Desktop Runtime 10 x64 上で動作——.NET の別途インストール不要。
- 🌉 stdio ブリッジ、🛡️ ダイアログガード：Windows 標準の PowerShell 5.1 + WinForms——別途インストール不要。
- 🐍 RunCode ツールボックス、Python ブリッジ：ArcGIS Pro 同梱の Python（arcpy）内で動作——別途の Python 不要。
- ✅ **したがって本パッケージには pip / インターネットが必要な外部依存はありません。**「オフラインで動作」は本パッケージ自身のインストールと実行経路を指します。ArcGIS Pro 3.7 の前提（.NET Desktop Runtime 10 x64、WebView2 ≥132）は Pro のインストーラーが提供します。

---

## 🧪 オプションコンポーネントの使い方

**🐍 RunCode ツールボックス**（arcpy を多用する場合）
- パス：`%LOCALAPPDATA%\ArcGIS Pro MCP\runcode\RunCode.pyt`
- AI が `run_python_toolbox_tool` を呼ぶときにこのパスを渡すだけ。Pro の「挿入 → ツールボックス → ツールボックスの追加」でも可。
- 各 Pro セッションの初回実行で「サードパーティコードライブラリ」信頼ダイアログが出ます：先に `03-dialog-guard.cmd` を起動すればガードが「はい」を自動クリック ✨。

**🔌 Python ブリッジ**（arcpy セッションを呼び出し間で保持したい場合）
1. ArcGIS Pro メニュー **分析 → Python** で Python ウィンドウを開く；
2. 貼り付けて実行（既定インストール先のパス）：
   ```python
   import sys; sys.path.insert(0, r"C:\Users\<ユーザー名>\AppData\Local\ArcGIS Pro MCP\plugin")
   import mcp_bridge; print(mcp_bridge.start_server(port=6511))
   ```
3. `CURRENT`（現在のプロジェクト）が必要ならもう一行（メインスレッドで実行）：
   ```python
   print(mcp_bridge.start_pump())
   ```

**🛡️ ダイアログガード**（RunCode / ENVI→Pro 連携フローの前に推奨）
```
03-dialog-guard.cmd                 # 600 秒ガード
03-dialog-guard.cmd -DurationSeconds 120
03-dialog-guard.cmd -NotifyTest     # 通知ウィンドウのテスト（30 秒）
03-dialog-guard.cmd -AlertOnly      # 通知のみ、自動クリックなし
```
安全境界：「サードパーティ / コードライブラリ / third-party / code library」の文脈を持つ信頼ダイアログのみ処理。**上書き・削除などの確認ダイアログには絶対に触れません** 🙅。ログ：`%LOCALAPPDATA%\ArcGIS Pro MCP\guard\watch_pro_dialog.log`。

---

## 🔒 セキュリティに関する注意

- 🏠 add-in とすべてのブリッジサービスは `127.0.0.1` のみをリッスンし、LAN 上の他のマシンからは接続できません。
- ⚖️ 本ソフトウェアは本機のみに `127.0.0.1` でサービスを提供し、第三者へのネットワークサービスは行いません。改変してネットワーク経由で他者に提供する場合、AGPL-3.0 §13 のソース提供義務が適用されます。
- 🔏 add-in は未署名ビルドです：初回読み込み時に ArcGIS Pro が信頼の確認を求めます（正常な流れ）。本パッケージの `SHA256SUMS.txt` で照合してください。
- 🕵️ 本パッケージにはテレメトリやネットワーク送信は一切含まれません。
- 🧹 アンインストール：`99-uninstall.cmd`（クライアント設定を残すか選択可能）。

---

## 📜 バージョンとライセンス

- 🏷️ バージョン：**1.1.11**（Customer 1.1.11 · 1.14 拡張版。ビルド日は `VERSION.json`、ツール数 115）
- 👤 メンテナ：**Law**（[@XNYGggg](https://github.com/XNYGggg)）
- 🙏 アップストリームプロジェクトと原作者：**[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)**、作者 **Pisut Nakmuenwai**（AGPL-3.0-or-later）。本パッケージは 1.1.11 までローカルパッチを積み重ねています（変更点は `docs\CHANGELOG.md` と `RELEASE-NOTES.md`）。
- ⚖️ ライセンス：**AGPL-3.0-or-later**（完全なソーススナップショットと上流 NOTICE を同梱）
- 🏷️ 商標：「ArcGIS」は Esri の商標です。本プロジェクトはサードパーティ製ツールであり、Esri とは無関係です。

🆘 よくある質問は `docs\排障手册.md`、応用的な使い方は `docs\使用指南.md` を参照。良い地図作成を 🎨 (｀・ω・´)ゞ

---

<!-- LDU-THANKS -->

<h2 align="center">🙏 謝辞</h2>

<p align="center">
  本研究開発は <strong>魯東大学（Ludong University）</strong> の支援を受けて行われました。ここに深く感謝します。
</p>

<p align="center">
  <a href="https://www.ldu.edu.cn/"><img src="assets/ldu-emblem.png" width="104" alt="魯東大学校章"></a>
</p>

<h3 align="center">魯東大学 · Ludong University</h3>

> 魯東大学は 1930 年創立、山東省煙台市に位置する省立総合大学です。応用型人材育成を主導とし、工学・農学・理学・文学が調和して発展しています。現在 26 学部を擁し、工学・化学・材料科学など 6 学科が ESI 世界ランキング上位 1% に入り、「厚徳・博学・日新・篤行」の校訓を掲げています。

<p align="center">
  📍 中国山東省煙台市芝罘区紅旗中路 186 号（郵便番号 264025）<br>
  🌐 <a href="https://www.ldu.edu.cn/">大学公式サイト</a> ·
  <a href="https://jwc.ldu.edu.cn/">教務処</a> ·
  <a href="https://lib.ldu.edu.cn/">図書館</a> ·
  <a href="https://ehall.ldu.edu.cn/">オンラインサービス</a>
</p>
