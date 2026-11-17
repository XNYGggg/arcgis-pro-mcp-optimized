[![English](https://img.shields.io/badge/Language-English-1f6feb?style=flat-square)](README.md) [![简体中文](https://img.shields.io/badge/Language-%E7%AE%80%E4%BD%93%E4%B8%AD%E6%96%87-d1242f?style=flat-square)](README.zh-CN.md) [![日本語](https://img.shields.io/badge/Language-%E6%97%A5%E6%9C%AC%E8%AA%9E-bc002d?style=flat-square)](README.ja.md) [![한국어](https://img.shields.io/badge/Language-%ED%95%9C%EA%B5%AD%EC%96%B4-0047a0?style=flat-square)](README.ko.md) [![Version](https://img.shields.io/badge/Version-1.1.4%20(stable)-2ea44f?style=flat-square)](../../releases/tag/v1.1.4) [![License: AGPL v3+](https://img.shields.io/badge/License-AGPL--3.0--or--later-blue?style=flat-square)](LICENSE) [![ArcGIS Pro](https://img.shields.io/badge/ArcGIS%20Pro-3.7%2B-00a1e0?style=flat-square)](https://www.esri.com/en-us/arcgis/products/arcgis-pro/overview) [![Platform](https://img.shields.io/badge/Platform-Windows%2011%20x64-lightgrey?style=flat-square)]() [![Release](https://img.shields.io/github/v/release/XNYGggg/arcgis-pro-mcp-optimized?style=flat-square&label=release)](../../releases)

# 🗺️ ArcGIS Pro MCP(최적화판) · 고객 에디션 v1.1.4 · 안정판(권장) 🎁

자연어 한 문장이면 AI 어시스턴트가 이미 열어 둔 ArcGIS Pro 프로젝트를 직접 조작합니다: 레이어 추가, 심볼 변경, 속성 조회, 지오프로세싱 실행, 지도 내보내기까지. 데이터는 컴퓨터를 떠나지 않으며, 모든 통신은 `127.0.0.1` 안에서만 이루어집니다 🔒.

> **📌 버전 상태(2026-11-16 갱신)**: v1.1.4(add-in 1.1.4, 113개 도구)가 **안정판 · 권장** 릴리스입니다. 다른 2.0 라인(v2.0.0 / v2.0.1)은 **테스트판**입니다: 최적화가 미완성이고 GitHub Release 자산과 태그가 제거되었으며 **사용을 권장하지 않습니다**(NuGet 2.0.x 다운로드는 실패합니다). 2.0 코드는 이제 `customer-v2.0.1` / `customer-v2.0.0` 브랜치에만 남아 있습니다. 브랜치 · 태그 · Release 대응표는 [BRANCHES.md](https://github.com/XNYGggg/arcgis-pro-mcp-optimized/blob/main/BRANCHES.md)를 참조하세요.

> **🙏 업스트림 프로젝트와 원저자**: 이 고객 에디션은 **[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)**(원저자 **Pisut Nakmuenwai**, AGPL-3.0-or-later)를 기반으로 로컬 패치를 **v1.1.4**까지 누적한 것입니다. 전체 저작자 표시와 변경 고지는 패키지 루트의 `NOTICE`와 `source/SOURCE-NOTICE.md`에 있습니다.

이 고객 에디션은 실행에 필요한 **모든 구성 요소**를 함께 담았습니다. 오프라인으로 동작하며 **Python도 pip도 네트워크도 필요 없습니다** — ArcGIS Pro 본체 외에 다른 의존성이 없습니다. 배터리까지 포함되어 있습니다 🔋

---

## 🖥️ 시스템 요구 사항

| 항목 | 요구 사항 |
|---|---|
| 🪟 OS | **Windows 11 x64**(64비트 Home / Pro / Enterprise, ArcGIS Pro 3.7 공식 지원). **아키텍처: x64(AMD64) 전용 — ARM64는 지원하지 않습니다** |
| 🗺️ ArcGIS Pro | **3.7 이상**(이 add-in은 3.7 / .NET 10 기준으로 빌드됨) |
| 🔑 권한 | 표준 사용자면 충분하며, 설치에 **관리자 권한이 필요 없습니다** |
| 📦 기타 의존성 | 없음(PowerShell과 .NET은 Windows / Pro에 기본 포함) |

---

## 🚀 빠른 시작(5분)

1. 📂 패키지를 원하는 폴더에 **압축 해제**합니다(예: `D:\ArcGIS-Pro-MCP-customer`).
2. 🛠️ **ArcGIS Pro를 종료**한 뒤 `install\00-install.cmd`를 더블클릭합니다. 스크립트가 add-in 설치, 런타임 배포, Claude Desktop / Claude Code 구성을 자동으로 수행합니다.
3. 🖥️ **ArcGIS Pro를 엽니다**. 처음 로드할 때 "추가 기능을 신뢰하지 않음"이 표시되면 "예 / 신뢰"를 클릭하세요(안전합니다 😄). Pro 상단의 **MCP 탭**에서 상태가 **running**(포트 `127.0.0.1:6520`)인지 확인합니다.
4. 🔌 **AI 클라이언트를 연결**하고(아래 "구성" 참조) `install\01-check.cmd`를 더블클릭해 자체 점검을 실행합니다. 전부 `[OK]`면 준비 완료입니다 (๑•̀ㅂ•́)و✧

> 자체 점검 시 ArcGIS Pro가 실행 중이 아니면 포트 검사가 경고로 표시됩니다 — Pro를 열고 다시 실행하세요 👌

---

## 🎛️ 구성 — AI 클라이언트 연결

**전제 조건**: ArcGIS Pro가 열려 있고 **MCP 탭**이 **running**을 표시해야 합니다. 모든 포트는 로컬호스트에만 바인딩됩니다:

| 포트 | 용도 |
|---|---|
| `6520` | HTTP 기반 MCP — 모든 AI 클라이언트가 여기에 연결 |
| `6510` | 레거시 TCP 브리지(최신 클라이언트는 사용하지 않음) |
| `6511` | 선택적 Pro 내 Python 브리지(`execute_arcpy_code`) |

### 🟣 Claude Code

```powershell
claude mcp add --transport http arcgis http://127.0.0.1:6520/mcp
claude mcp list
```

수동으로 구성하려면(`~/.claude.json`):

```json
{ "mcpServers": { "arcgis": { "type": "http", "url": "http://127.0.0.1:6520/mcp" } } }
```

제거는 `claude mcp remove arcgis`, 다시 추가하려면 위 명령을 재실행하세요.

### 🖥️ Claude Desktop(번들 stdio 브리지 사용)

Claude Desktop은 HTTP로 직접 연결할 수 없고 로컬 프로세스 실행만 지원합니다. 패키지에 포함된 순수 PowerShell 브리지(Python 불필요)를 사용하세요:

1. **Claude Desktop을 완전히 종료**합니다(트레이 아이콘 포함).
2. `install\02-configure-claude.cmd`를 실행하거나 구성을 직접 편집합니다(먼저 백업하세요).
3. 구성 파일이 위치하는 곳은 다음 중 **처음으로 존재하는** 디렉터리입니다:
   `%APPDATA%\Claude`, `%APPDATA%\Claude-3p`, `%LOCALAPPDATA%\Claude-3p`, `%LOCALAPPDATA%\Claude` — 파일명은 `claude_desktop_config.json`.

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

4. Claude Desktop을 다시 열고 도구 목록에 `arcgis`가 나타나면 완료입니다.

### 🟢 Codex

`~/.codex/config.toml`에 다음을 추가합니다(기존 `arcgis` 섹션이 있으면 이전 `env` 하위 테이블 포함 섹션 전체를 교체하세요):

```toml
[mcp_servers.arcgis]
url = "http://127.0.0.1:6520/mcp"
```

저장하고 Codex를 재시작합니다.

### 🔧 기타 MCP 클라이언트

Streamable HTTP로 `http://127.0.0.1:6520/mcp`에 연결하거나 위의 범용 JSON을 사용하세요. (Antigravity는 `serverUrl`을 사용하며 `type`은 필요 없습니다.)

### ✅ 연결 확인

- `install\01-check.cmd` — Pro 버전, add-in 해시, 런타임, 포트, 클라이언트 구성을 점검합니다.
- `netstat -ano | findstr 6520` — 포트가 수신 중인지 확인합니다.
- 로그: `%LOCALAPPDATA%\ArcGIS-MCP\addin.log`.

---

## 🎯 사용 방법

**기본 흐름**: 데이터가 있는 프로젝트 열기 → 원하는 것을 말하기 → 어시스턴트가 도구 선택 → 결과 확인(이미지는 인라인으로 돌아옵니다) 🎨

| 이렇게 말할 수 있습니다 | 실제로 일어나는 일 |
|---|---|
| "군별로 면적 기준 단계 구분 색상을, 연한 빨강에서 진한 초록으로" | `set_layer_renderer`(단계 구분) |
| "인구 50만 이상인 군을 선택해서 새 피처 클래스로 내보내 줘" | `select_features` → `export_features` |
| "도로를 500미터 버퍼해 줘" | `run_geoprocessing_tool`(버퍼) |
| "향·진별 경지면적 합계를 내 줘" | `summarize_features` / `zonal_statistics` |
| "현재 지도 뷰 스크린샷을 보여 줘" | `export_map_view`(PNG 반환) |
| "제목·범례·축척 막대·북쪽 화살표가 있는 A3 세로 PDF를 만들어 줘" | `create_layout` → `add_map_frame` → `add_layout_*` → `export_layout` |
| "레이어 12개를 같은 심볼로 일괄 내보내 줘" | `run_batch` 또는 `execute_arcpy_code` |

**도구 이름으로 직접 호출**: 아래 표의 모든 도구는 이름으로 직접 호출할 수 있습니다. `run_batch`는 여러 명령을 한 번의 왕복으로 연쇄 실행합니다. 전용 도구가 없으면 `execute_arcpy_code`로 대체합니다.

**편집 규칙**: 편집은 `save_edits` 전까지 보류 상태입니다 — 보류 중에는 데이터가 잠기며(삭제·덮어쓰기는 실패) `discard_edits`로 보류 중인 편집을 버릴 수 있습니다. `delete_features` / `delete_dataset`은 되돌릴 수 없습니다 — 실행 전에 어시스턴트에게 영향을 설명해 달라고 하세요.

**배치와 속도**: 반복이 많거나 큰 작업은 어시스턴트가 멱등 arcpy 스크립트를 작성해 RunCode로 한 번에 실행하게 하세요. 큰 프로젝트는 범위 / OID별로 체크포인트를 두고 분할 처리합니다.

---

## 🎨 매핑과 지도 제작(프롬프트 가이드)

이 섹션은 "AI에게 어떻게 말할지"를 설명합니다. 도구 이름을 외울 필요는 없습니다. 아래 4가지 요소만 갖추면 어시스턴트가 MCP 도구 호출로 분해해 줍니다.

### 네 가지만 말하면 한 번에 끝납니다

| 요소 | 말할 내용 | 예시 |
|---|---|---|
| 데이터 | 레이어 이름, 데이터 경로, 현재 선택 상태 | "현재 지도의 `county_boundaries` 레이어" |
| 작업 | 분류 필드 / 방법, 버퍼 거리, 통계 정의 | "`AREA` 기준 5개 클래스, 자연 분류" |
| 스타일 | 색상 구성, 글꼴 크기, 범례·축척 막대·북쪽 화살표 배치, 베이스맵 | "연한 빨강에서 진한 초록, 범례는 오른쪽 아래" |
| 출력 | 이미지 / PDF / 새 피처 클래스, 해상도, 저장 경로 | "A3 가로 PDF, 300 dpi, `D:\out\`에 저장" |

**좋은 프롬프트 vs 나쁜 프롬프트**
- ❌ "지도를 더 예쁘게 만들어 줘" — 목표가 없어 어시스턴트가 추측할 수밖에 없습니다.
- ✅ "`landuse`를 `DLMC` 기준 고유값 심볼로 하고, 범례를 오른쪽 아래에 두고, A3 가로 PDF를 300 dpi로 `D:\out\landuse.pdf`에 내보내 줘."
- ❌ "지도를 내보내 줘" — 범위, 레이아웃, 출력 형식이 지정되지 않았습니다.
- ✅ "`study_area` 레이어를 범위로, 축척 1:10000, A4 세로 PNG, 제목 '대상 구역 토지이용', 범례·축척 막대·북쪽 화살표 포함. 먼저 미리보기를 보내서 확인받아 줘."

### 도구 그룹별 프롬프트 치트시트

| 모듈(도구 그룹) | 이렇게 말할 수 있습니다 | 어시스턴트가 쓰는 도구 |
|---|---|---|
| 세션과 프로젝트 | "먼저 `diagnose`로 연결과 현재 프로젝트를 확인해 줘. 그다음 프로젝트를 `D:\proj\result.aprx`로 저장해 줘" | diagnose / save_project |
| 레이어 | "`roads`를 `basemap` 그룹으로 옮기고, 투명도 40%, 축척 1:50000 이하에서만 표시. `POI` 레이어는 꺼 줘" | move_layer / set_layer_transparency / set_layer_scale_range / set_layer_visibility |
| 데이터와 속성 | "`parcels`의 처음 20행을 면적 내림차순으로 읽어 줘. `DLMC`별로 면적을 집계해 줘" | get_features / summarize_features |
| 선택 | "`POP`이 50만 이상인 군을 선택해서 `D:\out\counties_big.shp`로 내보내 줘. 그다음 `study_area` 내 필지를 위치로 선택해 줘" | select_features / select_by_location / export_features |
| 필드와 데이터셋 | "`parcels`에 DOUBLE 필드 `AREA_HA`를 추가하고 `!Shape_Area!/10000`으로 계산해 줘. EPSG:4547로 폴리곤 피처 클래스 `result_2026`을 만들어 줘" | add_field / calculate_field / create_feature_class |
| 지오프로세싱 | "도로를 500미터 버퍼하고 디졸브해 줘. `study_area` 마스크로 토지이용 래스터를 클립해 줘" | run_geoprocessing_tool / run_batch |
| 심볼 | "면적으로 5개 클래스 자연 분류, 연한 빨강에서 진한 초록. 또는 `TYPE` 기준 고유값 색상" | set_layer_renderer |
| 지도 뷰 | "`study_area`로 확대하고 축척 1:10000, 1600×900 PNG를 찍어 줘" | set_map_view / zoom_to_layer / export_map_view |
| 레이아웃과 내보내기 | "A3 가로 레이아웃을 만들고, 제목은…, 범례는 레이어 3개만, 축척 막대와 북쪽 화살표를 추가해서 300 dpi PDF로 내보내 줘" | create_layout / add_layout_* / export_layout |
| 래스터 | "DEM의 밴드, 셀 크기, 통계를 읽어 줘. NDVI 식으로 식생지수를 계산해 줘" | get_raster_info / raster_calculator / zonal_statistics |
| 카탈로그와 데이터 탐색 | "프로젝트와 폴더 연결에서 이름에 `2025`가 들어간 셰이프파일을 찾아 줘" | search_data / describe_dataset / list_folder |
| 유틸리티 | "한 번에: 베이스맵 숨기기 → 군으로 확대 → PNG 내보내기. arcpy로 모든 레이어 이름에 접두사를 붙여 줘" | run_batch / execute_arcpy_code |

### 표준 매핑 프롬프트 템플릿(복사해서 사용)

```text
현재 프로젝트의 <지도 이름>에서 지도를 만들어 주세요:
- 범위: <대상 구역 레이어 / 범위>, 축척 1:<...>
- 데이터: <레이어1>, <레이어2> (필요하면 필드와 필터를 명시)
- 심볼: <필드 기준 단계 구분 / 고유값 / 단일 색상>, 색상 구성 <램프 또는 hex>
- 레이아웃: <A3 가로 / A4 세로>, 제목 "<제목>", 범례 <위치>, 축척 막대 <위치>, 북쪽 화살표 <위치>
- 출력: <PDF / PNG>, <300> dpi, <절대 경로>에 저장
- 진행 방식: 먼저 레이아웃을 확인하고 미리보기를 보내 승인을 받은 뒤 최종 파일을 내보내세요.
```

### 배치 매핑 프롬프트

```text
`maps`의 각 지도에 대해 같은 A3 가로 레이아웃 템플릿으로 PDF를 내보내 주세요:
지도 이름을 제목으로 쓰고, 범례에는 표시 중인 레이어를 남기고, 축척 막대와 북쪽 화살표는 오른쪽 아래에,
`D:\out\maps\`에 300 dpi로 출력. 먼저 1개를 내보내 확인받고, 승인 후 나머지를 배치 처리하세요.
```

### 흔한 문제에 대한 한 줄 수정

- "범례가 지도 본체를 가리고 있어" → 어시스턴트가 `legend` 위치나 레이아웃을 조정합니다.
- "페이지 비율이 이상해 / 여백이 너무 많아" → 지도 프레임과 페이지 비율을 조정합니다.
- "색이 너무 쨍해 / 논문용으로 안 맞아" → 색상 구성, hex 색상, 참고 이미지를 지정합니다.
- "아직 내보내지 말고 미리보기를 보여 줘" → 어시스턴트가 먼저 `export_map_view` 또는 레이아웃 미리보기를 실행합니다.

**효과**: 데이터·작업·스타일·출력을 한 번에 전달하면 어시스턴트는 보통 한 번의 왕복으로 최종 지도에 가까운 결과를 만듭니다. 그다음엔 차이만 짚어 주세요("범례를 오른쪽으로 1cm"). 전부 다시 말할 필요가 없습니다.

---

## 🛡️ 대화 상자 가드 사용(1.x 에디션)

**해결하는 문제**: ArcGIS Pro가 서드파티 `.pyt` / Python 도구(RunCode.pyt 등)를 처음 실행할 때 "서드파티 코드 라이브러리" 신뢰 대화 상자가 표시되어 무인 작업이 거기서 멈춥니다. 1.1.4에는 MCP 가드 도구가 없습니다 — 대신 패키지에 **독립 실행형 가드 스크립트**가 포함되어 있습니다. 시작하면 Pro 창을 폴링하며 이런 종류의 "예(Y)" 대화 상자만 클릭하고, 지정한 시간이 지나면 자동으로 종료됩니다.

**시작 방법**

```text
install\03-dialog-guard.cmd                    # 기본 600초
install\03-dialog-guard.cmd -DurationSeconds 120
install\03-dialog-guard.cmd -AlertOnly         # 최상위 경고만, 자동 클릭 없음
install\03-dialog-guard.cmd -NotifyTest        # 경고창만 테스트(30초)
```

`03-dialog-guard.cmd`를 더블클릭하거나, 터미널을 제어할 수 있는 AI 어시스턴트에게 백그라운드로 시작하게 해도 됩니다.

**프롬프트 예시**

| 이렇게 말하면 | 어시스턴트가 하는 일 |
|---|---|
| "이제 Pro에서 직접 RunCode.pyt를 실행할 거야. 먼저 대화 상자 가드를 600초 시작해 줘. 서드파티 코드 신뢰 대화 상자의 '예'를 자동 클릭하고, 클릭할 수 없으면 최상위 경고를 띄우고 비프음을 울려 줘" | 백그라운드에서 `install\03-dialog-guard.cmd -DurationSeconds 600` 실행 |
| "이번엔 경고만, 자동 클릭은 하지 마" | `-AlertOnly`로 시작 |
| "경고창이 뜨는지 테스트해 줘" | `-NotifyTest`로 시작(30초) |
| "가드 로그는 어디에 있어?" | `%LOCALAPPDATA%\ArcGIS Pro MCP\guard\watch_pro_dialog.log` |

**경계와 동작**
- 문맥에 "third-party / code library / 第三方 / 代码库"가 포함된 신뢰 대화 상자만 처리합니다. 덮어쓰기·삭제 등 다른 확인 대화 상자는 절대 건드리지 않습니다.
- 버튼을 클릭할 수 없으면 최상위 경고창이 최소 30초 유지되고 10초마다 비프음이 울립니다.
- 신뢰는 Pro 프로세스마다 한 번만 요청됩니다. Pro를 재시작하면 다시 나타납니다.
- 2.x에서는 이것이 `guard_summon` / `guard_status` 도구로 승격되어 RunCode 호출 전 자동 arm/settle이 붙었습니다. 1.1.4에서는 스크립트를 직접 시작합니다.

---

## 🐍 RunCode와 Python 브리지(프롬프트 가이드)

1.1.4의 arcpy 채널은 2계층입니다. **RunCode 도구 상자**는 단발 / 배치 스크립트에, **Python 브리지**는 변수를 호출 간에 유지해야 하는 대화형 세션에 적합합니다.

### RunCode 도구 상자

경로: `%LOCALAPPDATA%\ArcGIS Pro MCP\runcode\RunCode.pyt`. 어시스턴트가 이 경로를 `run_python_toolbox_tool`에 넘기게 하세요. Pro의 "삽입 → 도구 상자 → 도구 상자 추가"에서 직접 추가할 수도 있습니다.

**프롬프트 예시**
- "RunCode.pyt로 현재 프로젝트의 모든 레이어 라벨을 켜 줘. 라벨 필드는 `NAME`, 글꼴 크기 10."
- "멱등 arcpy 스크립트를 작성해서 RunCode로 실행하고, 모든 레이어 이름 앞에 `LYR_`을 붙여 줘. 이미 붙은 것은 건너뛰고."
- "이 스크립트를 RunCode.pyt로 실행할 예정이야 — 먼저 파라미터와 경로를 확인해 주고 실행해 줘."

**첫 실행** 시 "서드파티 코드 라이브러리" 신뢰 대화 상자가 표시됩니다. 먼저 위의 대화 상자 가드를 시작하거나 한 번 직접 "예"를 클릭하세요. 신뢰는 Pro 프로세스마다 한 번이면 됩니다.

### Python 브리지(`CURRENT` 또는 지속 변수가 필요할 때)

1. Pro 메뉴 **분석 → Python**에서 Python 창을 엽니다;
2. 아래를 붙여 넣고 실행합니다(경로는 기본 설치 위치 기준):

```python
import sys; sys.path.insert(0, r"C:\Users\<your-username>\AppData\Local\ArcGIS Pro MCP\plugin")
import mcp_bridge; print(mcp_bridge.start_server(port=6511))
```

3. `CURRENT`(열려 있는 프로젝트)가 필요하면 한 줄을 더 추가합니다(메인 스레드에서 실행해야 합니다):

```python
print(mcp_bridge.start_pump())
```

**프롬프트 예시**
- "다음 arcpy 작업은 `CURRENT`가 필요해. Pro의 Python 창에 브리지 시작 코드를 붙여 넣으라고 알려 줘. 그다음 내가 `execute_arcpy_code`로 실행할게."
- "브리지 시작 코드에 내 사용자 이름을 넣어서 복사해 줘. Pro에 직접 붙여 넣을게."
- "먼저 `get_pump_status`로 펌프가 준비됐는지 확인해 줘. 안 됐으면 붙여 넣기를 알려 줘."
- "지속 세션을 3단계로: 레이어 읽기, 필드 생성, 계산 — 변수는 호출 간에 유지되어야 해."

**효과**: RunCode는 "배치 스크립트를 한 번에 실행", Python 브리지는 "현재 프로젝트가 필요한 다단계 대화"를 담당합니다. 둘 다 별도 Python 설치가 필요 없습니다.

---

## 🧪 증류 모듈 안내(2.x 기능)

1.1.4에는 템플릿 증류(distillation)가 **포함되지 않습니다**: `template_*` 도구도, `guard_summon` / `guard_status`도 없습니다. 샘플 그림의 레이아웃을 재사용 가능한 템플릿으로 만들어 여러 지도를 일관되게 유지하고 싶다면 **V2.01(v2.0.1)**로 업그레이드하세요:

- 2.x는 10개의 `template_*` 도구를 제공: import / measure / patch / validate / preview / review / approve / apply / list / get;
- 템플릿 라이브러리는 `%LOCALAPPDATA%\ArcGIS Pro MCP\library`에 있으며, 업그레이드해도 기존 템플릿이 유지됩니다;
- 대표 사용법: "`D:\samples\fig3.png`를 '대상 구역 개요'라는 이름의 템플릿으로 증류해 줘. import → measure → patch → validate → preview 흐름으로 미리보기를 보여 줘서 확인받고, review + approve까지."

1.1.4와 2.x 기능 비교:

| 기능 | 1.1.4(이 브랜치) | 2.0.1(권장하지 않음) |
|---|---|---|
| 네이티브 MCP 도구 | 113(12개 그룹) | 125(14개 그룹) |
| RunCode / Python 브리지 | ✅ | ✅ |
| 대화 상자 가드 | 독립 스크립트 `03-dialog-guard.cmd` | `guard_summon` / `guard_status` + RunCode 전 자동 arm/settle |
| 템플릿 증류 | ❌ | ✅ 10개 `template_*` 도구 + schema v1.1 |

---

## 🧰 전체 113개 MCP 도구(12개 그룹)

어시스턴트가 할 수 있는 모든 기능을 이름으로 나열합니다. 클릭해서 펼치세요 👇

<details>
<summary><b>📜 전체 도구 목록 — 세션 15 · 레이어 19 · 데이터 11 · 선택 6 · 필드/데이터셋 11 · 지오프로세싱 7 · 심볼 6 · 뷰 7 · 레이아웃 16 · 래스터 5 · 카탈로그 6 · 유틸리티 4</b></summary>

### 세션과 프로젝트(15)

| 도구 | 용도 |
|---|---|
| `ping` | 브리지 연결성을 확인하고 연결된 프로젝트를 반환합니다. |
| `get_capabilities` | 브리지가 지원하는 모든 명령을 영역별로 나열합니다. 이름 있는 도구 외의 기능을 발견하기 위한 것입니다. |
| `get_queue_status` | 3레인 큐 상태: 레인별 대기 수, 현재 실행 중인 명령, 최근 완료분. 호출이 느리거나 밀릴 때 가장 먼저 확인합니다. |
| `diagnose` | 연결 자체 점검: Pro 버전, 라이선스, 열린 프로젝트, 활성 지도, 지도 뷰, 쓰기 권한. 문제가 있을 때 먼저 실행합니다. |
| `get_arcgis_info` | ArcGIS Pro 버전, 라이선스 수준, 사용 가능한 확장 모듈, 현재 프로젝트 경로. |
| `get_project_info` | 프로젝트 경로, 기본 지오데이터베이스와 도구 상자, 지도, 레이아웃, 폴더·데이터베이스 연결. |
| `save_project` | 프로젝트(.aprx)를 저장하거나 다른 위치에 사본으로 저장합니다. |
| `list_maps` | 모든 지도·씬을 좌표계와 레이어 수와 함께 나열합니다. |
| `create_map` | 프로젝트에 새 지도 또는 씬을 만듭니다. |
| `remove_map` | 프로젝트에서 지도를 삭제합니다. |
| `activate_map` | 지도 뷰를 Pro UI에서 열고 활성화합니다. 카메라 명령과 export_map_view 전에 필요합니다. |
| `set_map_properties` | 지도 이름 변경이나 좌표계 변경을 수행합니다. |
| `get_map_extent` | 지도 내 모든 데이터 레이어를 합친 범위를 반환합니다. |
| `get_environment` | 현재 arcpy 지오프로세싱 환경 설정을 읽습니다. |
| `set_environment` | arcpy 환경을 설정합니다(워크스페이스, 출력 좌표계, 범위, 마스크, 셀 크기, 출력 덮어쓰기, 병렬 처리 계수 등). |

### 레이어(19)

| 도구 | 용도 |
|---|---|
| `get_layers` | 지도의 레이어(그리기 순서와 그룹 중첩 포함)와 독립 테이블을 나열합니다. 프로젝트 내용을 파악하는 출발점입니다. |
| `get_layer_info` | 레이어 하나의 상세 정보: 데이터 원본, 좌표계, 범위, 필드, 피처 수, 렌더러, 라벨 상태. |
| `add_layer` | 경로 또는 서비스 URL로 데이터를 지도에 추가: 피처 클래스, 셰이프파일, 래스터, 테이블, .lyrx 레이어 파일, 웹 서비스. |
| `add_web_layer` | URL로 웹 레이어 추가(Feature / Map / Image 서비스, WMS, WMTS, 벡터 타일). |
| `remove_layer` | 지도에서 레이어 또는 독립 테이블을 제거합니다. |
| `rename_layer` | 콘텐츠의 레이어 이름을 바꿉니다. |
| `duplicate_layer` | 지도 내에서 레이어를 복제해 다른 심볼을 지정할 수 있게 합니다. |
| `set_layer_visibility` | 레이어 하나, 여러 개, 또는 지도의 모든 레이어 표시/숨김을 전환합니다. |
| `set_layer_transparency` | 레이어 투명도를 설정합니다(0 불투명, 100 완전 투명). |
| `set_layer_scale_range` | 레이어가 그려지는 축척 범위를 제한합니다. |
| `set_definition_query` | 레이어의 정의 쿼리(SQL where)를 설정합니다. 빈 문자열이면 해제. |
| `move_layer` | 레이어를 다른 레이어 기준으로 재배치하거나 그룹 레이어로 옮깁니다. |
| `create_group_layer` | 그룹 레이어를 만들고, 기존 레이어를 선택적으로 그 안으로 옮깁니다. |
| `zoom_to_layer` | 지도 뷰를 레이어 범위로 확대합니다. |
| `set_basemap` | 베이스맵을 설정: Topographic, Imagery, Imagery Hybrid, Streets, Navigation, Light Gray Canvas, Dark Gray Canvas, Terrain, Oceans, OpenStreetMap, National Geographic 등. |
| `get_broken_layers` | 모든 지도에서 데이터 원본이 누락된 레이어를 나열합니다. |
| `repair_layer_source` | 끊어진 원본을 고치기 위해 레이어를 새 워크스페이스/데이터셋으로 다시 지정합니다. |
| `add_join` | 공통 필드로 테이블을 레이어에 조인합니다. |
| `remove_join` | 레이어의 조인을 해제합니다. |

### 데이터와 속성(11)

| 도구 | 용도 |
|---|---|
| `get_features` | 레이어, 테이블, 데이터셋 경로에서 속성 행을 읽습니다. where, 필드 지정, 정렬, WKT 지오메트리 지원. |
| `count_features` | 피처 수를 셉니다. where 절 지정 가능. |
| `get_unique_values` | 필드의 고유값과 각각의 행 수를 반환합니다. |
| `get_field_statistics` | 숫자 필드의 min/max/평균/중앙값/합계/표준편차. |
| `summarize_features` | 하나 이상의 필드로 그룹화해 집계 — "범주별 합계"를 지오프로세싱 없이 빠르게 답하는 방법. |
| `insert_features` | 레이어 또는 테이블에 새 행을 삽입합니다. |
| `update_features` | where 절에 맞는 행의 속성이나 지오메트리를 갱신합니다. |
| `delete_features` | where 절에 맞는 행을 삭제합니다. |
| `save_edits` | 보류 중인 편집을 확정합니다. 저장 전 편집은 데이터를 잠그므로 그때까지 삭제·덮어쓰기가 실패합니다. |
| `discard_edits` | 보류 중인 편집을 버립니다. |
| `calculate_field` | 필드를 계산합니다(예: "!Shape_Area! / 10000"). |

### 선택(6)

| 도구 | 용도 |
|---|---|
| `select_features` | SQL where 절로 피처를 선택합니다. |
| `select_by_location` | 다른 레이어와의 공간 관계로 피처를 선택합니다. |
| `get_selection` | 현재 선택 상태를 보고합니다(레이어별, ObjectID와 선택적 속성 행 포함). |
| `set_selection` | ObjectID로 특정 피처를 선택합니다. |
| `clear_selection` | 레이어 하나 또는 지도의 모든 레이어 선택을 해제합니다. |
| `zoom_to_selection` | 지도 뷰를 현재 선택된 피처로 확대합니다. |

### 필드와 데이터셋(11)

| 도구 | 용도 |
|---|---|
| `list_fields` | 레이어의 필드를 유형, 별칭, 길이, 도메인과 함께 나열합니다. |
| `add_field` | 레이어 또는 테이블에 필드를 추가합니다. |
| `add_fields` | 여러 필드를 한 번의 호출로 추가합니다. |
| `delete_field` | 하나 이상의 필드를 삭제합니다. |
| `alter_field` | 필드 이름을 바꾸거나 별칭·길이를 변경합니다. |
| `create_feature_class` | 빈 피처 클래스를 만들고(기본값: 프로젝트 기본 지오데이터베이스) 지도에 추가합니다. |
| `create_table` | 빈 독립 테이블을 만듭니다. |
| `create_file_geodatabase` | 새 파일 지오데이터베이스를 만듭니다. |
| `truncate_table` | 스키마를 유지한 채 테이블 또는 피처 클래스의 모든 행을 삭제합니다. |
| `delete_dataset` | 디스크 또는 지오데이터베이스에서 데이터셋을 삭제합니다. 되돌릴 수 없습니다. |
| `export_features` | 레이어를 새 데이터셋으로 내보냅니다. 현재 선택과 정의 쿼리가 반영됩니다. |

### 지오프로세싱(7)

| 도구 | 용도 |
|---|---|
| `run_geoprocessing_tool` | 모든 arcpy 지오프로세싱 도구 실행 — 만능 분석 채널. 레이어 이름은 열린 레이어로 해석되어 선택과 정의 쿼리가 적용됩니다. 파라미터 이름을 모를 때는 먼저 describe_geoprocessing_tool을 호출합니다. |
| `list_geoprocessing_tools` | 사용 가능한 지오프로세싱 도구를 이름으로 검색합니다. |
| `list_toolboxes` | arcpy 도구 상자 모듈과 프로젝트의 도구 상자를 나열합니다. |
| `describe_geoprocessing_tool` | 실행 전에 도구의 파라미터, 데이터 유형, 기본값, 사용 설명을 조회합니다. |
| `run_python_toolbox_tool` | 디스크의 사용자 지정 .pyt / .atbx / .tbx 도구 상자에서 도구를 실행합니다. |
| `check_extension` | ArcGIS 확장 모듈 라이선스를 확인하고 필요하면 체크아웃합니다. |
| `get_messages` | 가장 최근 지오프로세싱 작업의 메시지를 가져옵니다. |

### 심볼(6)

| 도구 | 용도 |
|---|---|
| `set_layer_renderer` | 레이어 심볼을 변경: 단일 심볼, 범주별 고유값, 숫자 필드 기준 단계 구분/연속 색상 구성. |
| `get_layer_symbology` | 레이어의 현재 렌더러, 클래스 구분, 고유값, 라벨 설정을 확인합니다. |
| `list_color_ramps` | 프로젝트에서 사용 가능한 색상 램프를 나열합니다. |
| `set_layer_labeling` | 라벨 켜기/끄기, 식, 글꼴, 헤일로를 설정합니다. |
| `apply_symbology_from_layer` | .lyrx 파일 또는 다른 레이어에서 심볼을 복사합니다. |
| `save_layer_file` | 심볼이 포함된 레이어를 .lyrx 파일로 저장해 재사용합니다. |

### 지도 뷰(7)

| 도구 | 용도 |
|---|---|
| `get_map_view` | 현재 카메라 위치: 중심, 축척, 회전, 가시 범위. Pro에서 지도 탭이 열려 있어야 합니다. |
| `set_map_view` | 지도 뷰 이동: 범위, 중심점, 축척, 회전을 설정합니다. |
| `export_map_view` | 지도 뷰를 PNG로 렌더링해 육안 확인용 이미지를 반환합니다. 지도 탭이 열려 있어야 합니다. |
| `list_bookmarks` | 지도에 정의된 공간 북마크를 나열합니다. |
| `create_bookmark` | 현재 뷰 또는 지정 범위를 이름 있는 북마크로 저장합니다. |
| `apply_bookmark` | 지도 뷰를 북마크로 확대합니다. |
| `delete_bookmark` | 지도에서 북마크를 삭제합니다. |

### 레이아웃과 내보내기(16)

| 도구 | 용도 |
|---|---|
| `list_layouts` | 인쇄 레이아웃을 페이지 크기와 요소 수와 함께 나열합니다. |
| `get_layout_info` | 레이아웃 검사: 페이지 설정과 모든 요소의 위치·크기. |
| `create_layout` | 새 레이아웃 페이지를 만듭니다(기본값: 페이지를 채우는 지도 프레임 포함). |
| `delete_layout` | 프로젝트에서 레이아웃을 삭제합니다. |
| `add_map_frame` | 레이아웃의 페이지 위치에 지도 프레임을 추가합니다. |
| `set_map_frame_extent` | 레이아웃의 지도 프레임을 레이어, 범위, 축척에 맞춥니다. |
| `add_layout_text` | 제목이나 출처 등 텍스트 요소를 레이아웃에 추가합니다. |
| `add_layout_legend` | 레이아웃의 지도 프레임과 연동된 범례를 추가합니다. |
| `add_layout_scale_bar` | 지도 프레임과 연동된 축척 막대를 추가합니다. |
| `add_layout_north_arrow` | 지도 프레임과 연동된 북쪽 화살표를 추가합니다. |
| `add_layout_picture` | 로고 같은 이미지를 레이아웃에 배치합니다. |
| `set_layout_element` | 임의의 레이아웃 요소를 이동·크기 조정·이름 변경·숨기기·텍스트 변경합니다. |
| `delete_layout_element` | 레이아웃에서 요소를 제거합니다. |
| `export_layout` | 레이아웃을 PDF / PNG / JPEG / SVG / TIFF로 내보냅니다. PNG/JPEG는 확인용 이미지로 인라인 반환됩니다. |
| `preview_layout` | 파일을 쓰지 않고 레이아웃을 임시 이미지로 렌더링 — 작업 중 확인용. |
| `export_map_series` | 레이아웃의 지도 시리즈(맵북)를 여러 페이지 PDF로 내보냅니다. |

### 래스터(5)

| 도구 | 용도 |
|---|---|
| `get_raster_info` | 래스터 상세: 밴드, 크기, 셀 크기, 픽셀 유형, 통계, 좌표계. |
| `set_raster_symbology` | 래스터 레이어의 컬러라이저와 색상 램프를 설정합니다. |
| `raster_calculator` | 맵 대수 연산: 래스터를 짧은 별칭에 묶고 Python 식을 작성합니다. Spatial Analyst 확장 모듈이 필요합니다. |
| `sample_raster_values` | 지도 좌표 또는 포인트 레이어 피처 위치에서 래스터 셀 값을 읽습니다. |
| `zonal_statistics` | 존 폴리곤 내 래스터 값을 집계해 테이블로 반환합니다. Spatial Analyst 확장 모듈이 필요합니다. |

### 카탈로그와 데이터 탐색(6)

| 도구 | 용도 |
|---|---|
| `list_workspace_contents` | 지오데이터베이스 또는 폴더 내 데이터셋을 나열합니다. 기본값은 프로젝트의 기본 지오데이터베이스. |
| `list_folder` | 디스크의 GIS 파일과 하위 폴더를 나열합니다. 기본값은 프로젝트 홈 폴더. |
| `describe_dataset` | 어떤 데이터셋이든 경로로 설명: 유형, 지오메트리, 좌표계, 범위, 필드, 행 수(지도에 추가하지 않고). |
| `search_data` | 프로젝트의 지오데이터베이스, 홈 폴더, 폴더 연결에서 이름으로 데이터셋을 검색 — 추가 전에 데이터를 찾는 가장 빠른 방법. |
| `get_project_items` | 프로젝트에 등록된 폴더 연결, 데이터베이스 연결, 도구 상자. |
| `add_folder_connection` | 폴더를 프로젝트에 등록해 데이터를 쉽게 찾아볼 수 있게 합니다. |

### 유틸리티(4)

| 도구 | 용도 |
|---|---|
| `execute_arcpy_code` | ArcGIS Pro 안에서 Python 실행: arcpy가 이미 임포트되어 있고, arcpy.mp.ArcGISProject('CURRENT')가 열린 프로젝트이며, 변수는 호출 간에 유지되고, print 출력이 캡처되며, 마지막 식의 값이 반환됩니다. 전용 도구가 맞지 않을 때 사용합니다. |
| `get_pump_status` | 메인 스레드 디스패처가 설치되어 있는지 여부. 명령이 열린 Pro 프로젝트에 도달할 수 있는지를 좌우합니다 — 없으면 경로 기반 작업만 실행됩니다. |
| `stop_pump` | 메인 스레드 디스패처를 제거합니다. 사용자가 다시 설치할 때까지 라이브 프로젝트 명령이 동작하지 않습니다. |
| `run_batch` | 여러 명령을 한 번의 왕복으로 실행 — 다단계 워크플로가 훨씬 빨라집니다. |

</details>

---

## 📦 패키지 구성

| 경로 | 내용 | 비고 |
|---|---|---|
| `install\ArcGISProMCP.esriAddinX` | 🧩 add-in v1.1.4 | 네이티브 113개 도구 + 모든 GP 도구; HTTP MCP 6520 / 레거시 TCP 6510 |
| `install\runtime\stdio-bridge.ps1` | 🌉 stdio 브리지 | 로컬 프로세스만 실행할 수 있는 클라이언트(Claude Desktop 등)를 6520으로 연결; 순수 PowerShell, Python 불필요 |
| `install\runtime\runcode\RunCode.pyt` | 🐍 RunCode 도구 상자 | 임의의 arcpy를 자주 실행하는 용도; 첫 실행 시 신뢰 대화 상자가 한 번 표시됨 |
| `install\runtime\guard\` | 🛡️ 대화 상자 가드 | "서드파티 코드 라이브러리" 신뢰 대화 상자를 자동 클릭; 실패 시 최상위 경고(30초 이상, 비프음 포함) |
| `install\runtime\plugin\` | 🔌 Python 브리지(선택) | Pro의 Python 창에서 시작; 지속 세션 `execute_arcpy_code` 제공(포트 6511) |
| `install\*.cmd / *.ps1` | 🛠️ 설치와 자체 점검 | 설치 / 자체 점검 / Claude 구성 / 대화 상자 가드 / 제거 |
| `clients\` | 📁 클라이언트 설정 문서 | Claude Code, Claude Desktop, Codex, 범용 JSON |
| `docs\` | 📖 사용 가이드, 문제 해결, 작업 규칙, 변경 기록 | 사용 가이드부터 시작하세요 |
| `source\` | 📜 전체 소스 스냅샷 | 이 패키지 바이너리에 대응하는 소스(AGPL-3.0 준수 요건), 업스트림 LICENSE / NOTICE 포함 |

### 🔍 의존성 감사("완전 번들"인 이유)

- 🧩 add-in: 자체 완결형 C# 어셈블리로, Pro 3.7 지원 환경에 포함된 .NET Desktop Runtime 10 x64에서 동작합니다 — 별도 .NET 설치가 필요 없습니다.
- 🌉 stdio 브리지, 🛡️ 대화 상자 가드: PowerShell 5.1 + WinForms(Windows 기본 포함) — 추가 설치가 없습니다.
- 🐍 RunCode 도구 상자, Python 브리지: ArcGIS Pro에 포함된 Python(arcpy)에서 동작 — 별도 Python 설치가 필요 없습니다.
- ✅ **따라서 이 패키지에는 pip나 네트워크 액세스가 필요한 외부 의존성이 없습니다.** "오프라인 동작"은 패키지 자체의 설치·실행 경로를 뜻합니다. ArcGIS Pro 3.7의 설치 전제 조건(.NET Desktop Runtime 10 x64, WebView2 ≥132)은 Pro 자체 설치 프로그램이 제공합니다.

---

## 🧪 선택 구성 요소 사용

**🐍 RunCode 도구 상자**(arcpy 작업이 잦을 때)
- 경로: `%LOCALAPPDATA%\ArcGIS Pro MCP\runcode\RunCode.pyt`
- AI가 이 경로를 `run_python_toolbox_tool`에 넘기게 하세요. 또는 Pro의 "삽입 → 도구 상자 → 도구 상자 추가"로 직접 추가합니다.
- 각 Pro 세션의 첫 실행에서 "서드파티 코드 라이브러리" 신뢰 대화 상자가 표시됩니다: 먼저 `03-dialog-guard.cmd`를 시작하면 가드가 대신 "예"를 클릭합니다 ✨.

**🔌 Python 브리지**(arcpy 세션을 호출 간에 유지해야 할 때)
1. ArcGIS Pro 메뉴 **분석 → Python**에서 Python 창을 엽니다;
2. 붙여 넣고 실행합니다(경로는 기본 설치 위치 기준으로 작성됨):
   ```python
   import sys; sys.path.insert(0, r"C:\Users\<your-username>\AppData\Local\ArcGIS Pro MCP\plugin")
   import mcp_bridge; print(mcp_bridge.start_server(port=6511))
   ```
3. `CURRENT`(열려 있는 프로젝트)가 필요하면 한 줄을 더 추가합니다(메인 스레드에서 실행해야 합니다):
   ```python
   print(mcp_bridge.start_pump())
   ```

**🛡️ 대화 상자 가드**(RunCode 실행 전이나 ENVI→Pro 연계 워크플로 전에 권장)
```
03-dialog-guard.cmd                 # 600초 가드
03-dialog-guard.cmd -DurationSeconds 120
03-dialog-guard.cmd -NotifyTest     # 경고창만 테스트(30초)
03-dialog-guard.cmd -AlertOnly      # 경고만, 자동 클릭 없음
```
안전 경계: 문맥에 "third-party / code library / 第三方 / 代码库"가 포함된 신뢰 대화 상자만 처리하며, **덮어쓰기·삭제 등 다른 확인 대화 상자는 절대 건드리지 않습니다** 🙅. 로그는 `%LOCALAPPDATA%\ArcGIS Pro MCP\guard\watch_pro_dialog.log`.

---

## 🔒 보안 참고 사항

- 🏠 add-in과 모든 브리지 서비스는 `127.0.0.1`에만 바인딩됩니다. 같은 LAN의 다른 컴퓨터는 접속할 수 없습니다.
- ⚖️ 이 소프트웨어는 `127.0.0.1`에서의 로컬 서비스 제공만 수행하며, 네트워크를 통해 제3자에게 서비스를 제공하지 않습니다. 수정해 제3자에게 네트워크 서비스를 제공한다면 AGPL-3.0 §13의 소스 제공 의무가 적용됩니다.
- 🔏 add-in은 서명되지 않은 빌드입니다: ArcGIS Pro가 첫 로드 시 신뢰 확인을 요청하는데 이는 정상 흐름입니다. 이 패키지의 `SHA256SUMS.txt`로 검증하세요.
- 🕵️ 이 패키지에는 텔레메트리도 네트워크 보고도 없습니다.
- 🧹 제거: `99-uninstall.cmd`(클라이언트 구성은 선택적으로 유지 가능).

---

## 📜 버전 및 라이선스

- 🏷️ 버전: **1.1.4**(빌드 날짜는 `VERSION.json`, 113개 도구)
- 👤 유지관리자: **Law**([@XNYGggg](https://github.com/XNYGggg))
- 🙏 업스트림 프로젝트와 원저자: **[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)** by **Pisut Nakmuenwai**(AGPL-3.0-or-later). 이 패키지는 그 위에 1.1.4까지 패치를 누적했습니다(변경 내용은 `docs\CHANGELOG.md`와 `RELEASE-NOTES.md`에 기록).
- ⚖️ 라이선스: **AGPL-3.0-or-later**(패키지에 전체 소스 스냅샷과 업스트림 NOTICE 포함)
- 🏷️ 상표: "ArcGIS"는 Esri의 상표입니다. 이 프로젝트는 서드파티 도구이며 Esri와 제휴 관계가 없습니다.

🆘 FAQ는 `docs\排障手册.md`(문제 해결 매뉴얼), 고급 사용법은 `docs\使用指南.md`(사용 가이드)에 있습니다. 즐거운 매핑 되세요 🎨 (｀・ω・´)ゞ

---

<!-- LDU-THANKS -->

<h2 align="center">🙏 감사의 말</h2>

<p align="center">
  본 프로젝트는 <strong>루둥대학(鲁东大学)</strong>의 지원을 받아 개발되었습니다. 진심으로 감사드립니다.
</p>

<p align="center">
  <a href="https://www.ldu.edu.cn/"><img src="assets/ldu-emblem.png" width="104" alt="루둥대학 교표"></a>
</p>

<h3 align="center">루둥대학 · Ludong University</h3>

> 루둥대학은 1930년에 설립된 산둥성 옌타이시 소재 산둥성 소속 종합대학으로, 응용형 인재 양성을 주도로 공학·농학·이학·인문학이 조화롭게 발전하고 있습니다. 26개 단과대학을 두고 있으며, 공학·화학·재료과학 등 6개 분야가 ESI 세계 순위 상위 1%에 진입했습니다. 교훈은 ‘후덕(厚德)·박학(博學)·일신(日新)·독행(篤行)’입니다.

<p align="center">
  📍 중국 산둥성 옌타이시 즈푸구 홍치중로 186호 (우편번호 264025)<br>
  🌐 <a href="https://www.ldu.edu.cn/">공식 홈페이지</a> ·
  <a href="https://jwc.ldu.edu.cn/">교무처</a> ·
  <a href="https://lib.ldu.edu.cn/">도서관</a> ·
  <a href="https://ehall.ldu.edu.cn/">학생 포털</a>
</p>
