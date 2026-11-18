[![English](https://img.shields.io/badge/Language-English-1f6feb?style=flat-square)](README.md) [![简体中文](https://img.shields.io/badge/Language-%E7%AE%80%E4%BD%93%E4%B8%AD%E6%96%87-d1242f?style=flat-square)](README.zh-CN.md) [![日本語](https://img.shields.io/badge/Language-%E6%97%A5%E6%9C%AC%E8%AA%9E-bc002d?style=flat-square)](README.ja.md) [![한국어](https://img.shields.io/badge/Language-%ED%95%9C%EA%B5%AD%EC%96%B4-0047a0?style=flat-square)](README.ko.md) [![Version](https://img.shields.io/badge/Version-Customer%201.16%20(1.14%20Enhanced)-2ea44f?style=flat-square)](VERSION.json) [![Changelog](https://img.shields.io/badge/Docs-Changelog-6f42c1?style=flat-square)](docs/CHANGELOG.md) [![License: AGPL v3+](https://img.shields.io/badge/License-AGPL--3.0--or--later-blue?style=flat-square)](LICENSE) [![ArcGIS Pro](https://img.shields.io/badge/ArcGIS%20Pro-3.7%2B-00a1e0?style=flat-square)](https://www.esri.com/en-us/arcgis/products/arcgis-pro/overview) [![Platform](https://img.shields.io/badge/Platform-Windows%2011%20x64-lightgrey?style=flat-square)]()

# 🗺️ ArcGIS Pro MCP(최적화판) · Customer 1.16(1.14 확장판) 🚀

자연어 한 문장이면 AI 어시스턴트가 이미 열어 둔 ArcGIS Pro 프로젝트를 직접 조작합니다: 레이어 추가, 심볼 변경, 속성 조회, 지오프로세싱 실행, 지도 내보내기까지. 데이터는 컴퓨터를 떠나지 않으며, 모든 통신은 `127.0.0.1` 안에서만 이루어집니다 🔒.

> **🚀 이 브랜치는 Customer 1.16(1.14 확장판, 빌드 번호 1.1.6)입니다**: 1.14(v1.1.4) 위에 1.1.5의 F1–F10과 1.1.6의 V-1–V-7을 누적한 것입니다. **1.1.4 정식 릴리스는 아직 준비 중입니다. 기대해 주세요**; 이 브랜치는 **Release/배포판 자산을 제공하지 않으며**, 설치 파일은 저장소의 `install/`에 있고 언제든 v1.1.4로 롤백할 수 있습니다.

> **📌 버전 안내(2026-11-16)**: **1.0 계열(v1.1.4 정식 패키지)은 매우 안정적이어서 일상 사용을 권장**; **2.0은 테스트 버전**으로 최적화가 미완성이며, GitHub Release 자산과 태그가 제거되어 현재 다운로드할 수 없습니다.

> **🙏 업스트림 프로젝트와 원저자**: 이 고객 에디션은 **[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)**(원저자 **Pisut Nakmuenwai**, AGPL-3.0-or-later)를 기반으로 로컬 패치를 **v1.1.4**까지 누적한 것입니다. 전체 저작자 표시와 변경 고지는 패키지 루트의 `NOTICE`와 `source/SOURCE-NOTICE.md`에 있습니다.

이 패키지는 실행에 필요한 **모든 구성 요소**를 함께 담은 완전 오프라인 패키지입니다. **Python 설치도, pip도, 인터넷 연결도 필요 없으며** ArcGIS Pro 자체 외에 다른 의존성이 없습니다. 배터리까지 넣어 두었습니다 🔋

---

## 🖥️ 시스템 요구 사항

| 항목 | 요구 사항 |
|---|---|
| 🪟 운영체제 | **Windows 11 x64**(64비트 Home / Pro / Enterprise, ArcGIS Pro 3.7 공식 지원). **아키텍처: x64(AMD64) 전용, ARM64 미지원** |
| 🗺️ ArcGIS Pro | **3.7 이상**(이 add-in은 3.7 / .NET 10 기준 빌드) |
| 🔑 권한 | 일반 사용자로 충분하며 설치에 **관리자 권한이 필요 없습니다** |
| 📦 기타 의존성 | 없음(PowerShell과 .NET은 Windows / Pro에 기본 포함) |

---

## 🚀 5분 만에 시작하기

1. 📂 이 패키지를 원하는 폴더에 **압축 해제**(예: `D:\ArcGIS-Pro-MCP-Customer`).
2. 🛠️ **ArcGIS Pro를 닫고** `install\00-install.cmd`를 더블 클릭. 스크립트가 add-in 설치, 런타임 배포, Claude Desktop / Claude Code 구성까지 자동으로 처리합니다.
3. 🖥️ **ArcGIS Pro를 실행**합니다. 처음 로드할 때 "추가 기능을 신뢰하시겠습니까"가 나오면 **예**를 클릭하세요(안전합니다 😄). Pro 상단의 **MCP 탭**에서 상태가 **running**(포트 `127.0.0.1:6520`)인지 확인합니다.
4. 🔌 **AI 클라이언트를 연결**(아래 "구성 방법" 참고)한 뒤 `install\01-check.cmd`를 더블 클릭해 자체 점검을 실행하고, 모두 `[OK]`이면 사용 준비 완료 (๑•̀ㅂ•́)و✧

> 점검 시 ArcGIS Pro가 꺼져 있으면 포트 검사가 경고를 표시합니다. Pro를 켜고 다시 실행하세요 👌

---

## 🎛️ 구성 방법 —— AI 클라이언트 연결

**전제**: ArcGIS Pro가 열려 있고 **MCP 탭**이 **running**이어야 합니다. 모든 포트는 이 컴퓨터에만 바인딩됩니다:

| 포트 | 용도 |
|---|---|
| `6520` | MCP over HTTP —— 모든 AI 클라이언트가 여기에 연결 |
| `6510` | 레거시 TCP 브리지(최신 클라이언트는 사용하지 않음) |
| `6511` | 선택적 Pro 내부 Python 브리지(`execute_arcpy_code`) |

### 🟣 Claude Code

```powershell
claude mcp add --transport http arcgis http://127.0.0.1:6520/mcp
claude mcp list
```

수동 동등 구성(`~/.claude.json`):

```json
{ "mcpServers": { "arcgis": { "type": "http", "url": "http://127.0.0.1:6520/mcp" } } }
```

제거는 `claude mcp remove arcgis`, 다시 추가는 위 명령으로.

### 🖥️ Claude Desktop(내장 stdio 브리지 사용)

Claude Desktop은 HTTP에 직접 연결할 수 없고 로컬 프로세스만 실행할 수 있습니다. 패키지에 포함된 순수 PowerShell 브리지를 사용하세요(Python 불필요):

1. **Claude Desktop을 완전히 종료**(트레이 아이콘 포함).
2. `install\02-configure-claude.cmd`를 실행하거나 설정을 직접 편집(먼저 백업).
3. 설정 파일은 다음 중 "존재하는" 첫 폴더의 `claude_desktop_config.json`:
   `%APPDATA%\Claude`, `%APPDATA%\Claude-3p`, `%LOCALAPPDATA%\Claude-3p`, `%LOCALAPPDATA%\Claude`.

```json
{
  "mcpServers": {
    "arcgis": {
      "command": "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe",
      "args": ["-NoProfile", "-ExecutionPolicy", "Bypass", "-File",
               "C:\\Users\\<사용자 이름>\\AppData\\Local\\ArcGIS Pro MCP\\stdio-bridge.ps1"]
    }
  }
}
```

4. Claude Desktop을 다시 열고 도구 목록에 `arcgis`가 나타나면 성공입니다.

### 🟢 Codex

`~/.codex/config.toml`에 추가(기존 `arcgis` 섹션이 있으면 이전 `env` 하위 테이블까지 통째로 교체):

```toml
[mcp_servers.arcgis]
url = "http://127.0.0.1:6520/mcp"
```

저장 후 Codex를 재시작합니다.

### 🔧 기타 MCP 클라이언트

Streamable HTTP로 `http://127.0.0.1:6520/mcp`에 연결하거나 위의 범용 JSON을 사용하세요. (Antigravity는 `serverUrl`을 쓰고 `type`은 넣지 않습니다.)

### ✅ 연결 확인

- `install\01-check.cmd` —— Pro 버전, add-in 해시, 런타임, 포트, 클라이언트 구성을 점검.
- `netstat -ano | findstr 6520` —— 포트가 수신 중인지 확인.
- 로그: `%LOCALAPPDATA%\ArcGIS-MCP\addin.log`.

---

## 🎯 사용 방법

**기본 흐름**: 데이터가 있는 프로젝트 열기 → 요구 사항 말하기 → 어시스턴트가 도구 선택 → 결과 확인(이미지는 인라인으로 바로 반환) 🎨

| 이렇게 말하면 | 실제로 일어나는 일 |
|---|---|
| "면적으로 시·군을 단계 구분 색상으로, 연한 빨강에서 진한 초록까지" | `set_layer_renderer`(단계 구분) |
| "인구 50만 초과 시·군을 선택해 새 피처 클래스로 내보내" | `select_features` → `export_features` |
| "도로에 500미터 버퍼를 만들어" | `run_geoprocessing_tool`(Buffer) |
| "읍·면별 경지 면적을 집계해" | `summarize_features` / `zonal_statistics` |
| "현재 지도 화면을 캡처해서 보여줘" | `export_map_view`(PNG 반환) |
| "제목·범례·축척 막대·방위표가 있는 A3 세로 PDF를 만들어" | `create_layout` → `add_map_frame` → `add_layout_*` → `export_layout` |
| "12개 레이어를 같은 심볼로 일괄 내보내" | `run_batch` 또는 `execute_arcpy_code` |

**도구 이름으로 호출**: 아래 표의 모든 도구는 클라이언트에서 이름으로 직접 호출할 수 있습니다. `run_batch`는 한 번의 왕복으로 여러 명령을 연결하고, 전용 도구가 없으면 `execute_arcpy_code`가 최후의 수단입니다.

**편집 규칙**: 편집은 `save_edits` 전까지 보류되며, 보류 중에는 데이터가 잠겨 삭제/덮어쓰기가 실패합니다. `discard_edits`로 폐기할 수 있습니다. `delete_features` / `delete_dataset`은 되돌릴 수 없으니 먼저 어시스턴트에게 영향 범위를 설명하게 하세요.

**배치와 속도**: 반복이 많거나 데이터가 큰 작업은 RunCode로 멱등 arcpy 스크립트를 작성해 한 번에 실행하고, 큰 프로젝트는 범위/OID로 나누어 체크포인트를 남기세요.

---

## 🧰 전체 115개 MCP 도구(12개 그룹)

어시스턴트가 할 수 있는 모든 일을 이름별로 정리했습니다. 클릭해서 펼치세요 👇

<details>
<summary><b>📜 전체 도구 목록 —— 세션 15 · 레이어 19 · 데이터 11 · 선택 6 · 필드/데이터셋 11 · 지오프로세싱 7 · 심볼 6 · 뷰 7 · 레이아웃 16 · 래스터 5 · 카탈로그 6 · 유틸리티 4</b></summary>

### 세션과 프로젝트(15)

| 도구 | 용도 |
|---|---|
| `ping` | 브리지 연결 가능 여부를 확인하고 연결된 프로젝트를 반환합니다. |
| `get_capabilities` | 브리지가 지원하는 모든 명령을 영역별로 나열(이름 있는 도구 외의 기능을 발견할 때 사용). |
| `get_queue_status` | 3개 레인 큐 상태: 레인별 깊이, 현재 실행 명령, 최근 완료 기록. 호출이 느리거나 대기될 때 확인. |
| `diagnose` | 연결 자체 점검: Pro 버전, 라이선스, 열린 프로젝트, 활성 지도, 지도 뷰, 쓰기 권한. 문제가 생기면 먼저 실행. |
| `get_arcgis_info` | ArcGIS Pro 버전, 라이선스 수준, 사용 가능한 확장, 현재 프로젝트 경로. |
| `get_project_info` | 프로젝트 경로, 기본 지오데이터베이스와 도구 상자, 지도, 레이아웃, 폴더/데이터베이스 연결. |
| `save_project` | 프로젝트(.aprx)를 저장하거나 지정 위치에 사본 저장. |
| `list_maps` | 모든 지도와 씬을 좌표계·레이어 수와 함께 나열. |
| `create_map` | 프로젝트에 새 지도 또는 씬 생성. |
| `remove_map` | 프로젝트에서 지도 삭제. |
| `activate_map` | Pro UI에서 지도 뷰 열기/활성화. 카메라 계열 명령과 export_map_view 전에 필요. |
| `set_map_properties` | 지도 이름 변경 또는 좌표계 변경. |
| `get_map_extent` | 지도 내 모든 데이터 레이어의 결합 범위. |
| `get_environment` | 현재 arcpy 지오프로세싱 환경 설정 읽기. |
| `set_environment` | arcpy 환경 설정(작업 공간, 출력 좌표계, 범위, 마스크, 셀 크기, 출력 덮어쓰기, 병렬 계수 등). |

### 레이어(19)

| 도구 | 용도 |
|---|---|
| `get_layers` | 지도 내 레이어(그리기 순서·그룹 중첩 포함)와 독립 테이블을 나열. 프로젝트 내용 파악은 여기서 시작. |
| `get_layer_info` | 레이어 하나의 전체 정보: 데이터 원본, 좌표계, 범위, 필드, 피처 수, 렌더러, 라벨 상태. |
| `add_layer` | 경로 또는 서비스 URL로 데이터 추가: 피처 클래스, 셰이프파일, 래스터, 테이블, .lyrx 레이어 파일, 웹 서비스. |
| `add_web_layer` | URL로 웹 레이어 추가(Feature/Map/Image 서비스, WMS, WMTS, 벡터 타일). |
| `remove_layer` | 지도에서 레이어 또는 독립 테이블 제거. |
| `rename_layer` | 콘텐츠 목록의 레이어 이름 변경. |
| `duplicate_layer` | 지도 안에서 레이어를 복제해 다른 심볼로 표현. |
| `set_layer_visibility` | 단일·여러·전체 레이어 표시/숨기기. |
| `set_layer_transparency` | 레이어 투명도 설정(0 불투명, 100 완전 투명). |
| `set_layer_scale_range` | 레이어가 표시되는 축척 범위 제한. |
| `set_definition_query` | 정의 쿼리(SQL where) 설정, 빈 문자열이면 해제. |
| `move_layer` | 레이어 순서 변경 또는 그룹 레이어로 이동. |
| `create_group_layer` | 그룹 레이어 생성, 기존 레이어를 넣을 수 있음. |
| `zoom_to_layer` | 지도 뷰를 레이어 범위로 확대. |
| `set_basemap` | 베이스맵 설정: 지형, 이미지, 이미지 하이브리드, 스트리트, 내비게이션, 라이트 그레이 캔버스, 다크 그레이 캔버스, 지형 음영, 해양, OpenStreetMap, 내셔널 지오그래픽 등. |
| `get_broken_layers` | 모든 지도에서 데이터 원본이 없는 레이어 나열. |
| `repair_layer_source` | 레이어를 새 작업 공간 또는 데이터셋으로 다시 지정해 끊긴 원본 복구. |
| `add_join` | 공통 필드로 테이블을 레이어에 조인. |
| `remove_join` | 레이어의 조인 해제. |

### 데이터와 속성(11)

| 도구 | 용도 |
|---|---|
| `get_features` | 레이어·테이블·데이터셋 경로에서 속성 행 읽기. where, 필드 부분 집합, 정렬, WKT 지오메트리 지원. |
| `count_features` | 피처 수 계산(where 조건 가능). |
| `get_unique_values` | 필드의 고유값과 각 행 수 반환. |
| `get_field_statistics` | 숫자 필드의 min/max/mean/median/sum/표준편차. |
| `summarize_features` | 하나 이상의 필드로 그룹화해 집계. "범주별 개수/합계"를 GP 없이 답하는 지름길. |
| `insert_features` | 레이어 또는 테이블에 새 행 삽입. |
| `update_features` | where 조건에 맞는 행의 속성 및/또는 지오메트리 갱신. |
| `delete_features` | where 조건에 맞는 행 삭제. |
| `save_edits` | 보류 중인 편집 커밋. 저장되지 않은 편집은 데이터를 잠가 삭제/덮어쓰기 실패 원인이 됨. |
| `discard_edits` | 보류 중인 편집 폐기. |
| `calculate_field` | 필드 계산. 예: "!Shape_Area! / 10000". |

### 선택(6)

| 도구 | 용도 |
|---|---|
| `select_features` | SQL where 조건으로 피처 선택. |
| `select_by_location` | 다른 레이어와의 공간 관계로 선택. |
| `get_selection` | 현재 선택 상황을 레이어별로 보고(ObjectID와 선택적 속성 행). |
| `set_selection` | ObjectID로 특정 피처 선택. |
| `clear_selection` | 단일 레이어 또는 지도 전체 선택 해제. |
| `zoom_to_selection` | 지도 뷰를 선택된 피처로 확대. |

### 필드와 데이터셋(11)

| 도구 | 용도 |
|---|---|
| `list_fields` | 레이어 필드를 유형·별칭·길이·도메인과 함께 나열. |
| `add_field` | 레이어 또는 테이블에 필드 추가. |
| `add_fields` | 한 번의 호출로 여러 필드 추가. |
| `delete_field` | 하나 이상의 필드 삭제. |
| `alter_field` | 필드 이름 변경 또는 별칭/길이 수정. |
| `create_feature_class` | 빈 피처 클래스 생성(기본은 프로젝트 기본 지오데이터베이스) 후 지도에 추가. |
| `create_table` | 빈 독립 테이블 생성. |
| `create_file_geodatabase` | 새 파일 지오데이터베이스 생성. |
| `truncate_table` | 테이블/피처 클래스의 모든 행 삭제, 스키마 유지. |
| `delete_dataset` | 디스크 또는 지오데이터베이스에서 데이터셋 삭제. 되돌릴 수 없음. |
| `export_features` | 현재 선택과 정의 쿼리를 반영해 레이어를 새 데이터셋으로 내보내기. |

### 지오프로세싱(7)

| 도구 | 용도 |
|---|---|
| `run_geoprocessing_tool` | 임의의 arcpy 지오프로세싱 도구 실행——분석의 범용 채널. 레이어 이름은 실시간 레이어로 해석되어 선택과 정의 쿼리가 적용됩니다. 매개변수 이름을 모르면 먼저 describe_geoprocessing_tool 호출. |
| `list_geoprocessing_tools` | 사용 가능한 지오프로세싱 도구를 이름으로 검색. |
| `list_toolboxes` | arcpy 도구 상자 모듈과 프로젝트의 도구 상자 나열. |
| `describe_geoprocessing_tool` | 실행 전에 도구의 매개변수, 데이터 유형, 기본값, 사용 설명을 조회. |
| `run_python_toolbox_tool` | 디스크의 사용자 정의 .pyt / .atbx / .tbx 도구 상자 도구 실행. |
| `check_extension` | ArcGIS 확장 라이선스 확인 및 필요 시 체크아웃. |
| `get_messages` | 가장 최근 지오프로세싱 작업의 메시지. |

### 심볼(6)

| 도구 | 용도 |
|---|---|
| `set_layer_renderer` | 레이어 심볼 변경: 단일 심볼, 범주별 고유값, 숫자 필드 기반 단계/연속 색상. |
| `get_layer_symbology` | 레이어의 현재 렌더러, 클래스 구분, 고유값, 라벨 설정 확인. |
| `list_color_ramps` | 프로젝트에서 사용 가능한 색상 램프 나열. |
| `set_layer_labeling` | 라벨 켜기/끄기, 표현식·글꼴·헤일로 설정. |
| `apply_symbology_from_layer` | .lyrx 파일 또는 다른 레이어에서 심볼 복사. |
| `save_layer_file` | 레이어와 심볼을 .lyrx로 저장해 재사용. |

### 지도 뷰(7)

| 도구 | 용도 |
|---|---|
| `get_map_view` | 현재 카메라 위치: 중심, 축척, 회전, 가시 범위. Pro에서 지도 탭이 열려 있어야 함. |
| `set_map_view` | 지도 뷰 이동: 범위, 중심점, 축척, 회전 설정. |
| `export_map_view` | 지도 뷰를 PNG로 렌더링해 이미지로 반환(육안 확인용). 지도 탭이 열려 있어야 함. |
| `list_bookmarks` | 지도에 정의된 공간 북마크 나열. |
| `create_bookmark` | 현재 뷰 또는 지정 범위를 이름 있는 북마크로 저장. |
| `apply_bookmark` | 지도 뷰를 북마크로 확대. |
| `delete_bookmark` | 지도 북마크 삭제. |

### 레이아웃과 인쇄(16)

| 도구 | 용도 |
|---|---|
| `list_layouts` | 인쇄 레이아웃을 페이지 크기·요소 수와 함께 나열. |
| `get_layout_info` | 레이아웃 확인: 페이지 설정과 각 요소의 위치·크기. |
| `create_layout` | 새 레이아웃 페이지 생성(기본은 페이지를 채우는 지도 프레임 포함). |
| `delete_layout` | 프로젝트에서 레이아웃 삭제. |
| `add_map_frame` | 레이아웃 페이지의 지정 위치에 지도 프레임 추가. |
| `set_map_frame_extent` | 레이아웃의 지도 프레임을 레이어, 범위 또는 축척으로 지정. |
| `add_layout_text` | 제목, 크레디트 등 텍스트 요소 추가. |
| `add_layout_legend` | 지도 프레임과 연동된 범례 추가. |
| `add_layout_scale_bar` | 지도 프레임과 연동된 축척 막대 추가. |
| `add_layout_north_arrow` | 지도 프레임과 연동된 방위표 추가. |
| `add_layout_picture` | 로고 등 이미지를 레이아웃에 배치. |
| `set_layout_element` | 임의의 레이아웃 요소 이동·크기 조정·이름 변경·숨기기·텍스트 변경. |
| `delete_layout_element` | 레이아웃에서 요소 삭제. |
| `export_layout` | 레이아웃을 PDF / PNG / JPEG / SVG / TIFF로 내보내기. PNG/JPEG은 확인용으로 인라인 반환. |
| `preview_layout` | 레이아웃을 임시 이미지로 렌더링해 반환(파일 미기록). 구축 중 확인에 편리. |
| `export_map_series` | 레이아웃의 지도 시리즈(지도책)를 여러 페이지 PDF로 내보내기. |

### 래스터(5)

| 도구 | 용도 |
|---|---|
| `get_raster_info` | 래스터 세부 정보: 밴드, 크기, 셀 크기, 픽셀 유형, 통계, 좌표계. |
| `set_raster_symbology` | 래스터 레이어의 컬러라이저와 색상 램프 설정. |
| `raster_calculator` | 지도 대수: 래스터에 짧은 별칭을 붙이고 Python 표현식 작성. Spatial Analyst 필요. |
| `sample_raster_values` | 지도 좌표 또는 포인트 레이어 피처 위치에서 래스터 셀 값 읽기. |
| `zonal_statistics` | 존 폴리곤 내 래스터 값을 집계해 표로 반환. Spatial Analyst 필요. |

### 카탈로그와 데이터 탐색(6)

| 도구 | 용도 |
|---|---|
| `list_workspace_contents` | 지오데이터베이스 또는 폴더 내 데이터셋 나열. 기본은 프로젝트 기본 지오데이터베이스. |
| `list_folder` | 디스크의 GIS 파일과 하위 폴더 나열. 기본은 프로젝트 홈 폴더. |
| `describe_dataset` | 임의의 데이터셋을 경로로 설명: 유형, 지오메트리, 좌표계, 범위, 필드, 행 수(지도에 추가하지 않음). |
| `search_data` | 프로젝트 지오데이터베이스, 홈 폴더, 폴더 연결에서 이름으로 데이터 검색. 데이터 추가 전 가장 빠른 방법. |
| `get_project_items` | 프로젝트에 등록된 폴더 연결, 데이터베이스 연결, 도구 상자. |
| `add_folder_connection` | 폴더를 프로젝트에 등록해 데이터를 쉽게 탐색. |

### 유틸리티(4)

| 도구 | 용도 |
|---|---|
| `execute_arcpy_code` | ArcGIS Pro 내부에서 Python 실행: arcpy가 이미 임포트되어 있고, arcpy.mp.ArcGISProject('CURRENT')는 열린 프로젝트이며, 변수는 호출 간 유지되고, print 출력은 캡처되며, 마지막 표현식의 값이 반환됩니다. 전용 도구가 없을 때 사용. |
| `get_pump_status` | 메인 스레드 디스패처가 설치되어 있는지. 이것이 명령이 열린 Pro 프로젝트에 접근할 수 있는지를 결정하며, 없으면 경로 기반 작업만 가능. |
| `stop_pump` | 메인 스레드 디스패처 제거. 사용자가 다시 설치할 때까지 라이브 프로젝트 계열 명령이 동작하지 않습니다. |
| `run_batch` | 한 번의 왕복으로 여러 명령 실행. 다단계 워크플로가 훨씬 빨라집니다. |

</details>

---

## 📦 패키지 내용

| 경로 | 내용 | 설명 |
|---|---|---|
| `install\ArcGISProMCP.esriAddinX` | 🧩 add-in v1.1.6(Customer 1.16 / 1.14 확장판) | 115개 도구(112개 명령) + 모든 GP 도구. HTTP MCP 6520 / 레거시 TCP 6510 |
| `install\runtime\stdio-bridge.ps1` | 🌉 stdio 브리지 | 로컬 프로세스만 실행할 수 있는 클라이언트(예: Claude Desktop)를 6520으로 중계. 순수 PowerShell, Python 불필요 |
| `install\runtime\runcode\RunCode.pyt` | 🐍 RunCode 도구 상자 | 임의 arcpy를 자주 실행할 때. 첫 실행 시 신뢰 대화상자가 한 번 표시 |
| `install\runtime\guard\` | 🛡️ 대화상자 가드 | "타사 코드" 신뢰 대화상자를 자동 클릭. 실패 시 최상위 알림(30초 이상, 소리 포함) |
| `install\runtime\plugin\` | 🔌 Python 브리지(선택) | Pro의 Python 창에서 시작해 `execute_arcpy_code` 영구 세션 제공(포트 6511) |
| `install\*.cmd / *.ps1` | 🛠️ 설치와 자체 점검 | 설치 / 점검 / Claude 구성 / 대화상자 가드 / 제거 |
| `clients\` | 📁 클라이언트 연결 안내 | Claude Code, Claude Desktop, Codex, 범용 JSON |
| `docs\` | 📖 사용 가이드, 문제 해결, 작업 규칙, 변경 로그 | 먼저 사용 가이드 참고 |
| `source\` | 📜 전체 소스 스냅샷 | 이 패키지 바이너리에 대응하는 소스(AGPL-3.0 준수). 업스트림 LICENSE / NOTICE 포함 |

### 🔍 의존성 감사("전부 포함"의 이유)

- 🧩 add-in: 자체 완결형 C# 어셈블리로, 지원되는 Pro 3.7에 포함된 .NET Desktop Runtime 10 x64에서 실행——.NET 추가 설치 불필요.
- 🌉 stdio 브리지, 🛡️ 대화상자 가드: Windows 기본 제공 PowerShell 5.1 + WinForms——추가 설치 불필요.
- 🐍 RunCode 도구 상자, Python 브리지: ArcGIS Pro 기본 제공 Python(arcpy)에서 실행——별도 Python 불필요.
- ✅ **따라서 pip나 인터넷 설치가 필요한 외부 의존성이 없습니다.** "오프라인 사용 가능"은 이 패키지 자체의 설치·실행 체인을 의미하며, ArcGIS Pro 3.7의 전제 조건(.NET Desktop Runtime 10 x64, WebView2 ≥132)은 Pro 설치 프로그램이 제공합니다.

---

## 🧪 선택 구성 요소 사용법

**🐍 RunCode 도구 상자**(arcpy 고빈도 작업)
- 경로: `%LOCALAPPDATA%\ArcGIS Pro MCP\runcode\RunCode.pyt`
- AI가 `run_python_toolbox_tool`을 호출할 때 이 경로를 넘기거나, Pro에서 "삽입 → 도구 상자 → 도구 상자 추가"로 선택.
- Pro 세션마다 첫 실행 시 "타사 코드" 신뢰 대화상자가 뜹니다. 먼저 `03-dialog-guard.cmd`를 실행하면 가드가 "예"를 자동 클릭합니다 ✨.

**🔌 Python 브리지**(호출 간 arcpy 세션 유지가 필요할 때)
1. ArcGIS Pro 메뉴 **분석 → Python**에서 Python 창 열기;
2. 붙여넣고 실행(경로는 기본 설치 위치 기준):
   ```python
   import sys; sys.path.insert(0, r"C:\Users\<사용자 이름>\AppData\Local\ArcGIS Pro MCP\plugin")
   import mcp_bridge; print(mcp_bridge.start_server(port=6511))
   ```
3. `CURRENT`(열린 프로젝트)가 필요하면 한 줄 추가(메인 스레드에서 실행해야 함):
   ```python
   print(mcp_bridge.start_pump())
   ```

**🛡️ 대화상자 가드**(RunCode / ENVI→Pro 연동 플로우 전 권장)
```
03-dialog-guard.cmd                 # 600초 가드
03-dialog-guard.cmd -DurationSeconds 120
03-dialog-guard.cmd -NotifyTest     # 알림 창만 테스트(30초)
03-dialog-guard.cmd -AlertOnly      # 알림만, 자동 클릭 없음
```
안전 경계: "타사 / 코드 라이브러리" 맥락의 신뢰 대화상자만 처리하며, **덮어쓰기·삭제 등 다른 확인 대화상자는 절대 건드리지 않습니다** 🙅. 로그: `%LOCALAPPDATA%\ArcGIS Pro MCP\guard\watch_pro_dialog.log`.

---

## 🔒 보안 안내

- 🏠 add-in과 모든 브리지 서비스는 `127.0.0.1`에서만 수신하며, LAN의 다른 컴퓨터는 연결할 수 없습니다.
- ⚖️ 이 소프트웨어는 `127.0.0.1`에서 이 컴퓨터에만 서비스를 제공하며, 제3자에게 네트워크 서비스를 제공하지 않습니다. 수정 후 네트워크로 타인에게 제공하는 경우 AGPL-3.0 §13의 소스 제공 의무가 적용됩니다.
- 🔏 add-in은 서명되지 않은 빌드입니다. 처음 로드할 때 ArcGIS Pro가 신뢰 확인을 요구하며, 이는 정상 흐름입니다. 이 패키지의 `SHA256SUMS.txt`로 검증하세요.
- 🕵️ 이 패키지에는 어떤 텔레메트리나 네트워크 보고도 포함되지 않습니다.
- 🧹 제거: `99-uninstall.cmd`(클라이언트 구성을 유지할지 선택 가능).

---

## 📜 버전과 라이선스

- 🏷️ 버전: **Customer 1.16(1.14 확장판, 빌드 번호 1.1.6)**(빌드 날짜는 `VERSION.json`, 도구 수 115)
- 👤 유지관리자: **Law**([@XNYGggg](https://github.com/XNYGggg))
- 🙏 업스트림 프로젝트와 원저자: **[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)**, 저자 **Pisut Nakmuenwai**(AGPL-3.0-or-later). 이 패키지는 그 위에 패치를 1.1.4까지 누적했습니다(변경점은 `docs\CHANGELOG.md`와 `RELEASE-NOTES.md`).
- ⚖️ 라이선스: **AGPL-3.0-or-later**(전체 소스 스냅샷과 업스트림 NOTICE 동봉)
- 🏷️ 상표: "ArcGIS"는 Esri의 상표입니다. 이 프로젝트는 제3자 도구이며 Esri와 제휴 관계가 없습니다.

🆘 자주 묻는 질문은 `docs\排障手册.md`(중국어), 고급 사용법은 `docs\使用指南.md`(중국어)를 참고하세요. 좋은 지도 만드세요 🎨 (｀・ω・´)ゞ
