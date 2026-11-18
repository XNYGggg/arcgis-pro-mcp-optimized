[![简体中文](https://img.shields.io/badge/Language-%E7%AE%80%E4%BD%93%E4%B8%AD%E6%96%87-d1242f?style=flat-square)](README.md) [![日本語](https://img.shields.io/badge/Language-%E6%97%A5%E6%9C%AC%E8%AA%9E-bc002d?style=flat-square)](README.ja.md) [![한국어](https://img.shields.io/badge/Language-%ED%95%9C%EA%B5%AD%EC%96%B4-0047a0?style=flat-square)](README.ko.md)

# 🗺️ ArcGIS Pro MCP(최적화판) · 고객 에디션 v2.0.1(V2.01) 🎁

> 📦 **현재 권장 버전**: [**Release v2.0.1**](https://github.com/XNYGggg/arcgis-pro-mcp-optimized/releases/tag/v2.0.1) → `ArcGIS-Pro-MCP-v2.0.1-r2.zip`(이 패키지. 문서 개정판으로 바이너리는 초판 자산과 완전히 동일). 이전 버전 v2.0.0과 v1.1.4는 여전히 [Releases](https://github.com/XNYGggg/arcgis-pro-mcp-optimized/releases)에 남아 있습니다.
>
> 🗂️ 브랜치 / 태그 / Release 대응표: [BRANCHES.md](https://github.com/XNYGggg/arcgis-pro-mcp-optimized/blob/main/BRANCHES.md); 이 패키지의 스냅샷 브랜치: `customer-v2.0.1`.

자연어 한 문장이면 AI 어시스턴트가 이미 열어 둔 ArcGIS Pro 프로젝트를 직접 조작합니다: 레이어 추가, 심볼 변경, 속성 조회, 지오프로세싱 실행, 지도 내보내기까지. 데이터는 컴퓨터를 떠나지 않으며, 모든 통신은 `127.0.0.1` 안에서만 이루어집니다 🔒.

> **🙏 업스트림 프로젝트와 원저자**: 이 고객 에디션은 **[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)**(원저자 **Pisut Nakmuenwai**, AGPL-3.0-or-later)를 기반으로 로컬 패치를 **v2.0.1(V2.01)**까지 누적한 것입니다. 전체 저작자 표시와 변경 고지는 패키지 루트의 `NOTICE`와 `source/SOURCE-NOTICE.md`에 있습니다.

## 🆙 V2.01의 V2.0 대비 업그레이드(v2.0.1)

V2.01은 2.0 라인의 **실기 검증 패치판**으로, 사용자에게 보이는 문제 3개를 수정했습니다. 나머지 기능은 2.0과 동일합니다(문제 수정만 하고 아키텍처는 변경하지 않음):

| # | 문제(v2.0.0) | V2.01의 수정 | 실기 검증(Pro 3.7) |
|---|---|---|---|
| 1 | 가드 오탐: 신뢰 대화 상자의 텍스트가 버튼보다 먼저 나타나, 첫 스캔에서 버튼을 못 찾으면 30초 경고가 뜸 | 버튼이 잠시 없으면 2.5초 기다렸다가 경고(로그 `dialog_button_wait`) | 완전히 새로 신뢰하는 `.pyt`: 자동으로 「是(Y)」 클릭, 경고 없음 |
| 2 | `template_preview` 실패: `--out`이 `ValueFromRemainingArguments`에 의해 `-OutVariable`로 잘못 바인딩됨 | 런처를 `$args`를 쓰는 단순 스크립트로 변경 | `template_preview`가 정상적으로 PNG 반환 |
| 3 | 업그레이드 시 템플릿 라이브러리 유실: 설치 프로그램이 runtime 디렉터리를 통째로 교체하며 사용자의 `library/`를 `.bak`으로 이동 | 교체 전에 사용자의 `library/`를 병합 | 재설치 후에도 템플릿 목록 유지, preview 정상 |

## ✨ 2.x 주요 기능(V2.0에서 도입)

- 🛡️ **온디맨드 대화 상자 가드**: `run_python_toolbox_tool`과 `.pyt/.tbx/.atbx` GP 호출은 자동으로 가드를 소환하고 종료 시 자동으로 해산합니다. 작업 사이에는 시스템에 가드 프로세스가 없습니다. "第三方代码库执行(서드파티 코드 실행)" 신뢰 대화 상자만 클릭하고, 덮어쓰기/삭제 대화 상자는 경고만 하고 클릭하지 않습니다.
- 🧪 **템플릿 증류**: 샘플 도판 → 레이아웃 spec을 하나의 파이프라인(import → measure → patch → validate → preview → review → approve → apply)으로. Pro에 내장된 Python을 사용하며 별도 설치가 필요 없습니다.
- 🧰 **125개 MCP 도구**: 113개 업스트림 도구 외에 `guard_summon / guard_status`와 10개의 `template_*`를 추가.

이 고객 에디션은 실행에 필요한 **모든 구성 요소**를 함께 담았습니다. 오프라인으로 동작하며 **Python도 pip도 네트워크도 필요 없습니다** — ArcGIS Pro 본체 외에 다른 의존성이 없습니다. 배터리까지 포함되어 있습니다 🔋

---

## 🖥️ 시스템 요구 사항

| 항목 | 요구 사항 |
|---|---|
| 🪟 OS | **Windows 11 x64**(64비트 Home / Pro / Enterprise, ArcGIS Pro 3.7 공식 지원). **아키텍처: x64(AMD64) 전용 — ARM64는 지원하지 않습니다** |
| 🗺️ ArcGIS Pro | **3.7 검증 완료**; 이후 버전은 미검증(이 add-in은 3.7 / .NET 10 기준으로 빌드) |
| 🔑 권한 | 표준 사용자면 충분하며, 설치에 **관리자 권한이 필요 없습니다** |
| 📦 기타 의존성 | 없음(PowerShell과 .NET은 Windows / Pro에 기본 포함) |

## ⚠️ 알려진 한계

- 🔏 add-in이 **서명되지 않았습니다**: 첫 로드 시 Pro 안에서 "신뢰"를 클릭해야 합니다. 일부 조직의 보안 정책이 서명되지 않은 추가 기능을 차단할 수 있으며, 관리자의 허용이 필요합니다.
- 🧪 **깨끗한 Windows 가상 머신에서의 전체 흐름 검증은 미완료**입니다 — 먼저 테스트 머신에서 시험 설치를 권장합니다.
- 🪟 Windows 전용; ArcGIS Pro 3.7은 검증 완료, 이후 버전은 미검증.
- 🚨 덮어쓰기/삭제 계열 확인 대화 상자의 네거티브 테스트는 규칙 테스트 위주이며, 실제 대화 상자 매트릭스를 전부 커버하지는 않았습니다.

---

## 🚀 빠른 시작(5분)

1. 📂 패키지를 원하는 폴더에 **압축 해제**합니다(예: `D:\ArcGIS-Pro-MCP-customer`).
2. 🛠️ **ArcGIS Pro를 종료**한 뒤 `install\00-install.cmd`를 더블클릭합니다. 스크립트가 add-in 설치, 런타임 배포, Claude Desktop / Claude Code 구성을 자동으로 수행합니다.
3. 🖥️ **ArcGIS Pro를 엽니다**. 처음 로드할 때 "추가 기능을 신뢰하지 않음"이 표시되면 "예 / 신뢰"를 클릭하세요(안전합니다 😄). Pro 상단의 **MCP 탭**에서 상태가 **running**(포트 `127.0.0.1:6520`)인지 확인합니다.
4. 🔌 **AI 클라이언트를 연결**하고(아래 "구성 방법" 참조) `install\01-check.cmd`를 더블클릭해 자체 점검을 실행합니다. 전부 `[OK]`면 준비 완료입니다 (๑•̀ㅂ•́)و✧

> 자체 점검 시 ArcGIS Pro가 실행 중이 아니면 포트 검사가 경고로 표시됩니다 — Pro를 열고 다시 실행하세요 👌

---

## 🎛️ 구성 방법 — AI 클라이언트 연결

**전제**: ArcGIS Pro가 열려 있고 **MCP 탭**이 **running**을 표시해야 합니다. 모든 포트는 로컬호스트에만 바인딩됩니다:

| 포트 | 용도 |
|---|---|
| `6520` | MCP over HTTP —— 모든 AI 클라이언트가 여기에 연결 |
| `6510` | 구버전 TCP 브리지(현대 클라이언트는 사용하지 않음) |
| `6511` | 선택적 Pro 내 Python 브리지(`execute_arcpy_code`) |

### 🟣 Claude Code

```powershell
claude mcp add --transport http arcgis http://127.0.0.1:6520/mcp
claude mcp list
```

수동 등가 구성(`~/.claude.json`):

```json
{ "mcpServers": { "arcgis": { "type": "http", "url": "http://127.0.0.1:6520/mcp" } } }
```

제거는 `claude mcp remove arcgis`, 다시 추가는 위 명령을 재실행하세요.

### 🖥️ Claude Desktop(번들 stdio 브리지 사용)

Claude Desktop은 HTTP에 직접 연결할 수 없고 로컬 프로세스만 실행할 수 있습니다——패키지에 포함된 순수 PowerShell 브리지(Python 불필요)를 사용하세요:

1. **Claude Desktop을 완전히 종료**합니다(트레이 아이콘 포함).
2. `install\02-configure-claude.cmd`를 실행하거나 구성을 수동 편집합니다(먼저 백업하세요).
3. 구성 파일이 위치하는 곳은 다음 중 **처음으로 존재하는** 디렉터리입니다:
   `%APPDATA%\Claude`, `%APPDATA%\Claude-3p`, `%LOCALAPPDATA%\Claude-3p`, `%LOCALAPPDATA%\Claude` —— 파일명 `claude_desktop_config.json`.

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

4. Claude Desktop을 다시 열고 도구 목록에 `arcgis`가 나타나면 성공입니다.

### 🟢 Codex

`~/.codex/config.toml`에 추가합니다(기존 `arcgis` 섹션이 있으면 이전 `env` 하위 테이블 포함 섹션 전체를 교체):

```toml
[mcp_servers.arcgis]
url = "http://127.0.0.1:6520/mcp"
```

저장 후 Codex를 재시작합니다.

### 🔧 기타 MCP 클라이언트

Streamable HTTP로 `http://127.0.0.1:6520/mcp`에 연결하거나 위의 범용 JSON을 사용합니다. (Antigravity는 `serverUrl`을 사용하며 `type`은 필요 없습니다.)

### ✅ 연결 확인

- `install\01-check.cmd` —— Pro 버전, add-in 해시, 런타임, 포트, 클라이언트 구성을 점검합니다.
- `netstat -ano | findstr 6520` —— 포트가 수신 중인지 확인합니다.
- 로그: `%LOCALAPPDATA%\ArcGIS-MCP\addin.log`.

---

## 🎯 사용 방법

**기본 흐름**: 데이터가 있는 프로젝트 열기 → 원하는 것을 말하기 → 어시스턴트가 자동으로 도구 선택 → 결과 확인(이미지는 인라인으로 직접 반환됩니다) 🎨

| 이렇게 말할 수 있습니다 | 실제로 일어나는 일 |
|---|---|
| "区县(구·현)을 면적으로 단계 구분 색상으로, 연한 빨강에서 진한 초록으로" | `set_layer_renderer`(단계 구분 색상) |
| "인구 50만 초과 현을 선택해서 새 피처 클래스로 내보내 줘" | `select_features` → `export_features` |
| "도로를 500미터 버퍼해 줘" | `run_geoprocessing_tool`(Buffer) |
| "향·진(乡镇)별 경지면적을 집계해 줘" | `summarize_features` / `zonal_statistics` |
| "현재 지도 뷰 스크린샷을 보여 줘" | `export_map_view`(PNG 반환) |
| "제목·범례·축척 막대·북쪽 화살표가 있는 A3 세로 PDF를 내 줘" | `create_layout` → `add_map_frame` → `add_layout_*` → `export_layout` |
| "레이어 12개를 같은 심볼로 일괄 내보내 줘" | `run_batch` 또는 `execute_arcpy_code` |

**도구 이름으로 호출**: 아래 표의 모든 도구는 클라이언트에서 이름으로 직접 호출할 수 있습니다; `run_batch`는 여러 명령을 한 번의 왕복으로 연쇄 실행; 전용 도구가 없으면 `execute_arcpy_code`로 대체합니다.

**편집 규칙**: 편집은 `save_edits` 전까지 보류됩니다. 보류 중에는 데이터가 잠기며(삭제/덮어쓰기 실패), `discard_edits`로 보류 중인 편집을 버릴 수 있습니다. `delete_features` / `delete_dataset`은 되돌릴 수 없습니다——실행 전에 어시스턴트에게 영향 범위를 설명하게 하세요.

**배치와 속도 향상**: 반복이 많고 데이터가 큰 경우, 어시스턴트가 RunCode용 멱등 arcpy 스크립트를 작성해 한 번에 실행하게 하세요; 큰 프로젝트는 범위/OID별로 분할하고 체크포인트를 남깁니다.

---

## 🧩 세 모듈 요약: 역할, 사용 방법, 프롬프트 작성법

이 프로젝트는 "한 저장소, 세 모듈" 구조입니다: 핵심 **MCP**가 실제 작업을 맡고, **Guard**와 **Distill**은 필요할 때만 등장해 일이 끝나면 바로 물러납니다:

| 모듈 | 역할(한 줄) | 사용 방법(요점) | 상태 |
|---|---|---|---|
| 🧩 **MCP** | 125개 기본 도구 + RunCode / Python 브리지 두 개의 arcpy 채널로 AI가 눈앞의 Pro 프로젝트를 직접 조작 | add-in 설치 → MCP 탭이 running(`127.0.0.1:6520`) → 클라이언트를 `127.0.0.1:6520/mcp`에 연결 | ✅ |
| 🛡️ **Guard** | "서드파티 코드 라이브러리" 신뢰 대화 상자가 뜰 수 있는 작업 직전에만 소환되어 "예(Y)"를 자동 클릭하고, 작업이 끝나면 즉시 해산 | 어시스턴트가 `guard_summon`으로 소환하고 `guard_status`로 확인. RunCode 호출 전에는 자동 arm/settle | ✅ |
| 🧪 **Distill** | 샘플 도면 → 재사용 가능한 레이아웃 템플릿. 배치 출도의 레이아웃을 통일하고 재현 가능하게 | 10개 `template_*` 도구를 import → measure → patch → validate → preview → review → approve → apply 순서로 호출. 템플릿 라이브러리는 `%LOCALAPPDATA%\ArcGIS Pro MCP\library` | ✅ |

### ✍️ 모듈별 프롬프트 작성법

**🧩 MCP —— 대상 + 작업 + 출력, 배치는 먼저 하나**
- ✅ "`county_boundaries`를 `AREA` 기준 자연 분류 5개 클래스, 연한 빨강에서 진한 초록, 범례는 오른쪽 아래, A3 가로 PDF로 `D:\out\`에 300 dpi로 내보내 줘."
- ✅ "RunCode로 멱등 스크립트를 작성해 줘: 모든 레이어 이름 앞에 `LYR_` 접두사를 붙이고, 이미 붙어 있으면 건너뛰기."
- ✅ "이 arcpy는 `CURRENT`가 필요해: Python 브리지 시작 코드를 채워서 보내 줘. 내가 직접 붙여넣을게."
- ❌ "데이터 좀 처리해 줘." — 대상·작업·출력이 없어 어시스턴트가 되묻기만 합니다.

**🛡️ Guard —— "가드 소환"과 "무엇을 실행할지"를 한 문장에**
- ✅ "이번 RunCode는 배치 내보내기야: 먼저 `guard_summon`으로 가드를 소환하고, 끝나면 `guard_status`로 해산을 확인해 줘."
- ✅ "가드는 서드파티 신뢰 대화 상자만 클릭해. 덮어쓰기·삭제 확인은 내가 결정할게."
- ❌ "팝업을 전부 닫아 줘." — 가드는 신뢰 대화 상자만 처리합니다. 다른 확인은 사용자가 결정해야 합니다.

**🧪 Distill —— 템플릿 이름 + 흐름 + 확인 지점**
- ✅ "`D:\samples\fig3.png`를 '연구 지역 개요도' 템플릿으로 증류해 줘: import → measure → patch → validate → preview. 미리보기를 먼저 보여 주고, 내가 확인한 뒤 review + approve."
- ✅ "'연구 지역 개요도' 템플릿으로 `maps`의 각 지도를 A3 PDF로 내보내 줘. 제목은 지도 이름, 출력은 `D:\out\maps\`."
- ❌ "이 그림처럼 만들어 줘." — 템플릿 이름·흐름·확인 지점이 없어 레이아웃을 재현할 수 없습니다.

### 🧭 세 모듈 공통 규칙

1. **모듈을 지정**: 일반 작업은 기본 MCP. 스크립트는 "RunCode로", 지속 세션은 "Python 브리지로", 무인 실행 전에는 "가드를 소환해", 레이아웃 통일은 "템플릿으로"라고 명시합니다.
2. **대상을 구체적으로**: 레이어 이름·필드 이름·경로는 백틱으로 그대로 씁니다. "그 레이어"는 피합니다.
3. **출력을 확정**: 형식(PNG / PDF / 새 피처 클래스), 크기·해상도, 저장 경로.
4. **확인 지점 두기**: 배치·덮어쓰기·삭제 전에는 "먼저 하나만 보여 줘", "영향 범위를 먼저 설명해 줘".

> 자세한 절차는 아래 가드/증류 모듈 사용법을, 출도 프롬프트의 4요소와 템플릿은 "매핑과 지도 제작 — 프롬프트 가이드"를 참고하세요.

---

## 🎨 출도·제도 사용법(프롬프트 가이드)

이 섹션은 "AI에게 어떻게 말할지"를 파고듭니다: 도구 이름을 외울 필요는 없습니다. 아래 4요소를 전달하면 어시스턴트가 자동으로 MCP 도구 호출로 분해합니다.

### 네 가지만 말하면 한 번에 제대로 나옵니다

| 요소 | 전달할 내용 | 예시 |
|---|---|---|
| 데이터 | 레이어 이름, 데이터 경로, 현재 선택 세트 | "현재 지도의 `区县边界` 레이어" |
| 작업 | 분류 필드/방법, 버퍼 거리, 통계 기준 | "`AREA` 기준 5개 등급, 자연 분류" |
| 스타일 | 색상, 글꼴 크기, 범례/축척 막대/북쪽 화살표 위치, 베이스맵 | "연한 빨강에서 진한 초록, 범례는 오른쪽 아래" |
| 출력 | 이미지/PDF/새 피처 클래스, 해상도, 저장 경로 | "A3 가로 PDF, 300 dpi, `D:\out\`에 저장" |

**좋은 프롬프트 vs 나쁜 프롬프트**
- ❌ "지도를 예쁘게 해 줘" —— 목표가 없어 어시스턴트는 추측할 수밖에 없습니다.
- ✅ "`土地利用`을 `DLMC` 기준 고유값으로 심볼화하고, 색은 템플릿 `tpl-xxx` 참조, 범례는 오른쪽 아래, 그다음 A3 가로 PDF를 300 dpi로 `D:\out\landuse.pdf`에 내보내 줘."
- ❌ "지도 하나 내 줘" —— 범위, 레이아웃, 출력이 지정되지 않았습니다.
- ✅ "`研究区` 레이어를 범위로, 축척 1:10000, A4 세로 PNG, 제목 '연구구역 토지이용 현황', 범례·축척 막대·북쪽 화살표 포함. 먼저 미리보기 이미지를 보내 확인받아 줘."

### 도구 그룹별 프롬프트 치트시트

| 모듈(도구 그룹) | 이렇게 말할 수 있습니다 | 어시스턴트가 쓰는 것 |
|---|---|---|
| 세션과 프로젝트 | "먼저 `diagnose`로 연결과 현재 프로젝트를 보고; 그다음 프로젝트를 `D:\proj\成果.aprx`로 다른 이름으로 저장해 줘" | diagnose / save_project |
| 레이어 | "`roads`를 `底图` 그룹으로 옮기고, 투명도 40%, 1:50000 이하에서만 표시; `POI` 레이어는 꺼 줘" | move_layer / set_layer_transparency / set_layer_scale_range / set_layer_visibility |
| 데이터와 속성 | "`parcels`의 처음 20행을 면적 내림차순으로 읽고; `DLMC` 클래스별 면적을 집계해 줘" | get_features / summarize_features |
| 선택 | "`POP`이 50만 초과인 현을 선택해 `D:\out\counties_big.shp`로 내보내고; 그다음 `研究区` 안에 들어오는 필지를 위치로 선택해 줘" | select_features / select_by_location / export_features |
| 필드와 데이터셋 | "`parcels`에 DOUBLE 필드 `AREA_HA`를 추가하고 `!Shape_Area!/10000`으로 계산; EPSG:4547 폴리곤 피처 클래스 `成果_2026`을 새로 만들어 줘" | add_field / calculate_field / create_feature_class |
| 지오프로세싱 | "도로를 500미터 버퍼하고 디졸브; `研究区`를 마스크로 토지이용 래스터를 클립해 줘" | run_geoprocessing_tool / run_batch |
| 심볼화 | "면적으로 5개 등급·자연 분류·연한 빨강에서 진한 초록; 또는 `TYPE` 기준 고유값 색상" | set_layer_renderer |
| 지도 뷰 | "`研究区`로 확대하고 축척 1:10000, 1600×900 PNG를 찍어 줘" | set_map_view / zoom_to_layer / export_map_view |
| 레이아웃과 출도 | "A3 가로 레이아웃을 만들고, 제목…, 범례는 레이어 3개만, 축척 막대와 북쪽 화살표를 추가, 300 dpi PDF로 내보내 줘" | create_layout / add_layout_* / export_layout |
| 래스터 | "DEM의 밴드·셀 크기·통계를 읽고; NDVI 식으로 식생지수를 계산해 줘" | get_raster_info / raster_calculator / zonal_statistics |
| 카탈로그와 데이터 탐색 | "프로젝트와 폴더 연결에서 이름에 `2025`가 포함된 셰이프파일을 찾아 줘" | search_data / describe_dataset / list_folder |
| 유틸리티 | "한 번에: 베이스맵 숨기기→区县(구·현)로 확대→PNG 내보내기; arcpy로 모든 레이어 이름에 접두사 붙이기" | run_batch / execute_arcpy_code |

### 표준 출도 프롬프트 템플릿(복사해서 바로 사용)

```text
현재 프로젝트의 <지도 이름>에서 지도를 만들어 주세요:
- 범위: <연구구역 레이어/범위>, 축척 1:<...>
- 데이터: <레이어1>, <레이어2> (필요하면 필드와 필터 조건 명시)
- 심볼: <필드 기준 등급화/고유값/단일 색상>, 색상 <램프 또는 16진수>
- 레이아웃: <A3 가로 / A4 세로>, 제목 "<제목>", 범례 <위치>, 축척 막대 <위치>, 북쪽 화살표 <위치>
- 출력: <PDF / PNG>, <300> dpi, <절대 경로>에 저장
- 진행 방식: 먼저 레이아웃을 자체 점검하고 미리보기 이미지를 제게 보내 확인받으세요. 확인 후 정식 파일을 내보내세요.
```

### 배치 출도 프롬프트

```text
`maps`의 각 지도에 대해 같은 A3 가로 레이아웃 템플릿으로 PDF를 내보내 주세요:
제목은 지도 이름, 범례는 표시 중인 레이어만 남기고, 축척 막대와 북쪽 화살표는 오른쪽 아래,
출력은 `D:\out\maps\`, 300 dpi. 먼저 1장을 내보내 보여 주고, 확인 후 배치 처리하세요.
```

### 흔한 실패의 한 줄 복구

- "범례가 본도를 덮고 있어" → 어시스턴트가 `legend` 좌표나 레이아웃을 조정합니다.
- "페이지 비율이 이상해/여백이 너무 많아" → map frame과 페이지 비율을 조정합니다.
- "색이 너무 튀어/논문 스타일에 안 맞아" → 색 램프, 16진수, 또는 템플릿 ID를 지정합니다.
- "아직 내보내지 말고, 미리보기를 보여 줘" → 어시스턴트가 먼저 `export_map_view` 또는 레이아웃 미리보기를 실행합니다.

**효과**: "데이터·작업·스타일·출력"을 한 번에 다 전달하면, 어시스턴트는 보통 한 번의 왕복으로 최종 원고에 가까운 지도를 만듭니다; 이후 수정은 차이만 짚어 주면 됩니다("범례를 오른쪽으로 1cm"). 전부 다시 말할 필요가 없습니다.

---

## 🛡️ 가드 모듈 사용법(온디맨드 대화 상자 가드)

**해결하는 문제**: ArcGIS Pro가 서드파티 `.pyt` / Python 도구(RunCode.pyt 등)를 처음 실행할 때 "서드파티 코드 라이브러리" 신뢰 대화 상자가 떠서 무인 운영에서 작업이 멈춥니다. 가드는 대화 상자가 나올 수 있는 작업 전에만 소환되어 자동으로 「是(Y)」를 클릭하고, 작업이 끝나면 즉시 종료됩니다.

**두 가지 트리거 방식**
1. **자동(2.x 기본)**: 어시스턴트가 `run_python_toolbox_tool` / `execute_arcpy_code`를 호출하기 전에 add-in이 자동으로 가드를 arm하고, 작업 종료 시 settle해 해산합니다——보통 당신이 뭔가 말할 필요가 없습니다.
2. **수동 소환 `guard_summon`**: Pro 안에서 직접 `.pyt`를 수동 실행하는 경우, ENVI→Pro 가져오기, 그 밖의 어시스턴트 밖 작업의 경우.

**프롬프트 예시**

| 이렇게 말하면 | 어시스턴트가 하는 일 |
|---|---|
| "다음에 Pro에서 직접 RunCode.pyt를 실행할 거야. 먼저 가드를 10분간 소환해 줘; 서드파티 코드 신뢰 대화 상자가 나오면 자동으로 '是'를 클릭하고, 못 하면 최상위에 소리 나는 알림을 띄워 줘" | `guard_summon`(timeout_s≈600) |
| "이번엔 경고만, 자동 클릭은 하지 마" | `guard_summon`(alert_only) |
| "방금 가드가 정상 종료됐는지 봐 줘" | `guard_status` |
| "RunCode 실행 전에 가드를 켜 줘" | 자동 arm + settle |

**경계와 파라미터**
- "第三方代码库执行(서드파티 코드 실행)" 신뢰 대화 상자만 클릭; 덮어쓰기, 삭제 등 다른 확인 대화 상자는 절대 건드리지 않습니다.
- 버튼을 클릭할 수 없을 때는 최상위 알림 창이 최소 30초 표시되고 10초마다 울립니다.
- `timeout_s`는 가드 수명을 제어하고 타임아웃 시 자동 종료됩니다; `guard_status`로 엔진 경로·버전과 최근 20개 세션을 확인할 수 있습니다.
- 각 Pro 프로세스는 최초 신뢰 후 다시 표시되지 않습니다; Pro를 재시작하면 다시 필요해집니다.
- 1.1.4에는 `guard_summon`이 없습니다. 패키지 내 `install\03-dialog-guard.cmd`(기본 600초)로 같은 가드를 시작합니다.

**효과**: 무인 운영의 RunCode / ENVI 연계 제도가 대화 상자에서 멈추지 않습니다; 자동 클릭이 실패해도 최상위 표시+알림음 폴백이 있어 조용히 멈추는 일은 없습니다.

---

## 🧪 증류 모듈 사용법(샘플 도판 → 재사용 가능한 레이아웃 템플릿)

"증류"는 모델 학습**이 아닙니다**. **샘플 도판**(논문 그림, 교재 그림, 우수 제도 사례) 속의 레이아웃 지식——도곽 위치, 색 구성, 글꼴 크기, 범례/축척 막대/북쪽 화살표 배치, 실패하기 쉬운 지점——을 재사용 가능한 JSON 템플릿(spec)으로 추출해, 이후 출도에 그대로 적용하는 것입니다.

**증류로 얻는 것**: `template_id` + spec 하나. 내용은 `canvas`(캔버스 크기/DPI), `map_frame`, `title / legend / scale_bar / north_arrow`, `palette`, `typography`, `regions[]`, `pitfalls[]`, `measured`(측정 증거), `review`(검토 기록). 템플릿 라이브러리 기본 위치는 `%LOCALAPPDATA%\ArcGIS Pro MCP\library`.

**효과**
- 레이아웃 일관성: 같은 연구팀/같은 프로젝트의 여러 그림에서 도곽·글꼴 크기·범례 위치가 통일됩니다.
- 반복 감소: 매번 "범례는 오른쪽 아래, 제목 22호, 색은 연한 빨강에서 진한 초록"을 구두로 말할 필요가 없습니다.
- 감사 가능: 각 템플릿에 측정 출처, patch 내용, 검토자와 날짜가 있어 담당자가 바뀌어도 재현할 수 있습니다.
- 실패 방지: `pitfalls[]`가 "범례가 그림을 덮음" 같은 문제를 실행 가능한 검사로 바꿉니다.

**전체 흐름과 프롬프트**

| 단계 | 이렇게 말할 수 있습니다 | 어시스턴트 호출 | 당신이 검수할 것 |
|---|---|---|---|
| 1 등록 | "`D:\samples\fig3.png`를 템플릿으로 증류해 줘. 이름 '연구구역 개요도', 태그: 논문 스타일" | `template_import` | 반환된 `template_id` |
| 2 측정 | "이어서 측정해 줘" | `template_measure` | `h_lines/v_lines`, `palette_top`, DPI가 합리적인지 |
| 3 의미론 | 어시스턴트가 그림을 보고 patch 작성(도곽, 범례, 글꼴 크기, 구역, pitfalls) | `template_patch` | patch의 좌표/글꼴 크기가 기대와 맞는지 |
| 4 검증 | "검증해 줘" | `template_validate` | 오류 0; warning이 허용 가능한지 |
| 5 미리보기 | "미리보기 이미지를 보여 줘" | `template_preview` | 와이어프레임과 샘플의 나란히 비교가 일치하는지 |
| 6 검토/승인 | "확인했어. 검토자 류하오원(柳皓文), 비고: 레이아웃 대조 통과" → "승인해 줘" | `template_review` → `template_approve` | 상태가 `ready`로 바뀜 |
| 7 적용 | "템플릿 `tpl-xxx`로 현재 프로젝트를 출도해 줘" | `template_apply` → 제도 도구 | 산출 지도가 템플릿 레이아웃과 일치하는지 |

**그대로 복사할 수 있는 프롬프트**

1. **단일 도판 전체 증류**

```text
`D:\samples\fig3.png`를 레이아웃 템플릿으로 증류해 주세요:
- 이름: 연구구역 개요도; 태그: 논문 스타일, A4 가로
- 전체 흐름으로 import → measure → patch → validate → preview 실행
- 의미 필드(도곽, 범례, 축척 막대, 북쪽 화살표, 글꼴 크기, 색 구성, regions, pitfalls)는 당신이 그림을 보고 판단
- 각 단계의 핵심 결과를 보고; 미리보기 이미지를 먼저 제게 보여 확인받고, 제가 OK한 뒤 review + approve
- measured 값을 직접 편집하지 마세요
```

2. **배치 등록(먼저 측정만)**

```text
`D:\samples\` 아래의 모든 PNG를 스캔해서 하나씩 import + measure 하고, 이름은 파일명,
태그는 일괄 `후보 템플릿` 추가. 의미 patch는 아직 쓰지 말고, 측정 집계표를
제게 보여 주세요. 제가 고른 뒤 하나씩 정련합니다.
```

3. **템플릿으로 출도**

```text
템플릿 `tpl-20261116-172854-57ec`로 현재 프로젝트를 출도해 주세요:
먼저 template_apply로 템플릿을 읽고, map_frame/palette/typography/pitfalls에 따라
A4 가로 레이아웃을 만들고, 제목 "연구구역 토지이용 현황", 300 dpi PDF를 `D:\out\landuse.pdf`에 내보내세요.
완료 후 자체 점검 결과와 미리보기 이미지를 제게 보내 검수받으세요.
```

4. **ready 템플릿 수정**

```text
`tpl-xxx`의 범례를 오른쪽 아래(x=0.78, y=0.78, w=0.16, h=0.16)로 옮기고 제목 글꼴 크기를 22로 변경,
validate와 preview를 다시 실행해 보여 주세요; measured는 건드리지 마세요. 확인 후 다시 review + approve.
```

5. **템플릿 이전/공유**

```text
`tpl-xxx`를 bundle.zip으로 내보내고; 그다음 다른 머신의 템플릿 라이브러리에 bundle-import 한 뒤,
가져온 후의 template_id와 상태를 나열해 주세요.
```

**어시스턴트가 반드시 지켜야 할 하드 룰**
- 의미론적 판독은 패키지 내 `skills/template-distill/SKILL.md`에 따라 수행합니다; 결정적 단계에서 "즉흥"을 부려서는 안 됩니다.
- `measured / id / image_sha256 / source_image / schema_version`은 보호 필드이며, patch에 포함하면 거부됩니다; 캔버스를 바꾸려면 `measure --force`를 다시 실행하거나 `canvas.height_in`을 실측과 3% 허용 오차 내로 맞추세요.
- `ready`는 검증과 인간 review를 모두 통과해야 합니다; review 없는 `approve`는 거부됩니다. `--force`는 당신이 명시적으로 요구할 때만 사용되며 검토 기록에 기록됩니다.
- 미리보기 이미지를 당신에게 보여 주고 샘플과 나란히 대조한 뒤에만 승인할 수 있습니다.
- 학술지/교재 샘플 도판은 기본적으로 로컬 라이브러리에만 두고, 공개 bundle을 내보내지 마세요(저작권).

**자주 묻는 질문**
- **미리보기 실패/이미지가 안 뜸**: 이 버전(v2.0.1)은 v2.0.0의 `run-distill.ps1 --out` 파라미터 바인딩 문제를 수정했습니다; 그래도 실패하면 먼저 `install\01-check.cmd` 자체 점검을 실행하세요.
- **도곽을 측정하지 못함**: 샘플 그림이 너무 어둡거나 흐리거나, 밝은 띠가 뚜렷하지 않은 경우——선명한 전체 그림 스크린샷으로 바꾸세요.
- **검증이 3% 허용 오차 오류를 보고**: `canvas.height_in`이 실측과 맞지 않습니다; measured를 직접 편집하지 말고 canvas를 고치거나 다시 측정하세요.
- **템플릿 상태 `ready`는 변경할 수 없음**: spec을 복사하거나 새 id로 다시 가져오세요. 납품된 템플릿을 깨뜨리지 마세요.

---

## 🧰 전체 125개 MCP 도구(14개 그룹)

어시스턴트가 할 수 있는 모든 일을 이름으로 여기에 나열합니다. 클릭해서 펼치세요 👇

<details>
<summary><b>📜 전체 도구 목록 —— 세션 15 · 레이어 19 · 데이터 11 · 선택 6 · 필드/데이터셋 11 · 지오프로세싱 7 · 심볼화 6 · 뷰 7 · 레이아웃 16 · 래스터 5 · 카탈로그 6 · 유틸리티 4 · 가드 2 · 템플릿 10</b></summary>

### 세션과 프로젝트(15)

| 도구 | 용도 |
|---|---|
| `ping` | 브리지 연결성을 확인하고 연결된 프로젝트를 반환합니다. |
| `get_capabilities` | 브리지가 지원하는 모든 명령을 영역별로 나열합니다. 이름 있는 도구 외의 기능을 발견하기 위한 것입니다. |
| `get_queue_status` | 3레인 큐 상태: 레인별 대기 수, 현재 명령, 최근 완료 기록. 호출이 느려지거나 대기될 때 가장 먼저 봅니다. |
| `diagnose` | 연결 자체 점검: Pro 버전, 라이선스, 열린 프로젝트, 활성 지도, 지도 뷰, 쓰기 권한. 문제가 생기면 먼저 실행. |
| `get_arcgis_info` | ArcGIS Pro 버전, 라이선스 수준, 사용 가능한 확장 모듈, 현재 프로젝트 경로. |
| `get_project_info` | 프로젝트 경로, 기본 지오데이터베이스와 도구 상자, 지도, 레이아웃, 폴더·데이터베이스 연결. |
| `save_project` | 프로젝트(.aprx)를 저장하거나 지정 위치에 사본으로 저장합니다. |
| `list_maps` | 모든 지도와 씬을 좌표계·레이어 수와 함께 나열합니다. |
| `create_map` | 프로젝트에 새 지도 또는 씬을 만듭니다. |
| `remove_map` | 프로젝트에서 지도를 삭제합니다. |
| `activate_map` | 지도 뷰를 Pro UI에서 열거나 활성화합니다; 카메라 명령과 export_map_view 전에 필수. |
| `set_map_properties` | 지도 이름 변경이나 좌표계 변경을 수행합니다. |
| `get_map_extent` | 지도 내 모든 데이터 레이어의 합산 범위. |
| `get_environment` | 현재 arcpy 지오프로세싱 환경 설정을 읽습니다. |
| `set_environment` | arcpy 환경을 설정합니다(워크스페이스, 출력 좌표계, 범위, 마스크, 셀 크기, 출력 덮어쓰기, 병렬 처리 계수 등). |

### 레이어(19)

| 도구 | 용도 |
|---|---|
| `get_layers` | 지도의 레이어(그리기 순서와 그룹 중첩 포함)와 독립 테이블을 나열합니다; 프로젝트 내용은 여기서부터 봅니다. |
| `get_layer_info` | 단일 레이어의 완전한 상세: 데이터 원본, 좌표계, 범위, 필드, 피처 수, 렌더러, 라벨 상태. |
| `add_layer` | 경로 또는 서비스 URL로 데이터를 지도에 추가: 피처 클래스, 셰이프파일, 래스터, 테이블, .lyrx 레이어 파일, 웹 서비스. |
| `add_web_layer` | URL로 웹 레이어 추가(Feature/Map/Image 서비스, WMS, WMTS, 벡터 타일). |
| `remove_layer` | 지도에서 레이어 또는 독립 테이블을 삭제합니다. |
| `rename_layer` | 콘텐츠 목록의 레이어 이름을 변경합니다. |
| `duplicate_layer` | 지도 내에서 레이어를 복제해 다른 심볼화로 표시할 수 있게 합니다. |
| `set_layer_visibility` | 단일, 여러 개, 또는 지도의 모든 레이어 표시/숨김을 전환합니다. |
| `set_layer_transparency` | 레이어 투명도를 설정합니다(0 불투명, 100 완전 투명). |
| `set_layer_scale_range` | 레이어의 가시 축척 범위를 제한합니다. |
| `set_definition_query` | 레이어의 정의 쿼리(SQL where)를 설정합니다. 빈 문자열이면 해제. |
| `move_layer` | 레이어의 상대 순서를 변경하거나 그룹 레이어로 옮깁니다. |
| `create_group_layer` | 그룹 레이어를 만들고, 기존 레이어를 그 안으로 옮길 수도 있습니다. |
| `zoom_to_layer` | 지도 뷰를 레이어 범위로 확대합니다. |
| `set_basemap` | 베이스맵을 설정: 지형, 이미지, 이미지 하이브리드, 거리, 내비게이션, 라이트 그레이 캔버스, 다크 그레이 캔버스, 지형 음영, 해양, OpenStreetMap, 내셔널 지오그래픽 스타일 등. |
| `get_broken_layers` | 모든 지도에서 데이터 원본이 누락된 레이어를 나열합니다. |
| `repair_layer_source` | 레이어를 새 워크스페이스나 데이터셋으로 다시 지정해 끊어진 데이터 원본을 복구합니다. |
| `add_join` | 공통 필드로 테이블을 레이어에 조인합니다. |
| `remove_join` | 레이어의 조인을 해제합니다. |

### 데이터와 속성(11)

| 도구 | 용도 |
|---|---|
| `get_features` | 레이어, 테이블, 데이터셋 경로의 속성 행을 읽습니다. where, 필드 부분 집합, 정렬, WKT 지오메트리 지원. |
| `count_features` | 피처 수를 집계합니다. where 조건을 붙일 수 있습니다. |
| `get_unique_values` | 필드의 고유값과 각 행 수를 반환합니다. |
| `get_field_statistics` | 숫자 필드의 min/max/mean/median/sum/표준편차. |
| `summarize_features` | 하나 이상의 필드로 그룹화해 집계. "범주별로 몇 개/합계 얼마"에 답하는 가장 짧은 방법입니다(GP 불필요). |
| `insert_features` | 레이어 또는 테이블에 새 행을 삽입합니다. |
| `update_features` | where 조건에 맞는 행의 속성이나 지오메트리를 갱신합니다. |
| `delete_features` | where 조건에 맞는 행을 삭제합니다. |
| `save_edits` | 보류 중인 편집을 확정합니다; 저장 전 편집은 데이터를 잠가 삭제나 덮어쓰기가 실패하는 원인이 됩니다. |
| `discard_edits` | 보류 중인 편집을 버립니다. |
| `calculate_field` | 필드 계산. 예: 식 "!Shape_Area! / 10000". |

### 선택(6)

| 도구 | 용도 |
|---|---|
| `select_features` | SQL where 조건으로 피처를 선택합니다. |
| `select_by_location` | 다른 레이어와의 공간 관계로 피처를 선택합니다. |
| `get_selection` | 현재 선택 상황을 보고합니다(레이어별, ObjectID와 선택적 속성 행 포함). |
| `set_selection` | ObjectID로 지정한 피처를 선택합니다. |
| `clear_selection` | 단일 레이어 또는 지도의 전체 선택을 해제합니다. |
| `zoom_to_selection` | 지도 뷰를 현재 선택된 피처로 확대합니다. |

### 필드와 데이터셋(11)

| 도구 | 용도 |
|---|---|
| `list_fields` | 레이어 필드를 유형·별칭·길이·도메인과 함께 나열합니다. |
| `add_field` | 레이어 또는 테이블에 필드를 추가합니다. |
| `add_fields` | 한 번의 호출로 여러 필드를 추가합니다. |
| `delete_field` | 하나 이상의 필드를 삭제합니다. |
| `alter_field` | 필드 이름을 바꾸거나 별칭/길이를 변경합니다. |
| `create_feature_class` | 빈 피처 클래스를 만들고(기본값: 프로젝트 기본 지오데이터베이스) 지도에 추가합니다. |
| `create_table` | 빈 독립 테이블을 만듭니다. |
| `create_file_geodatabase` | 새 파일 지오데이터베이스를 만듭니다. |
| `truncate_table` | 테이블 또는 피처 클래스의 모든 행을 지우고 구조는 유지합니다. |
| `delete_dataset` | 디스크 또는 지오데이터베이스에서 데이터셋을 삭제합니다; 되돌릴 수 없습니다. |
| `export_features` | 현재 선택과 정의 쿼리에 따라 레이어를 새 데이터셋으로 내보냅니다. |

### 지오프로세싱(7)

| 도구 | 용도 |
|---|---|
| `run_geoprocessing_tool` | 모든 arcpy 지오프로세싱 도구 실행——분석의 범용 채널; 레이어 이름은 라이브 레이어로 해석되어 선택과 정의 쿼리가 반영됩니다. 파라미터 이름을 모르면 먼저 describe_geoprocessing_tool을 호출하세요. |
| `list_geoprocessing_tools` | 사용 가능한 지오프로세싱 도구를 이름으로 검색합니다. |
| `list_toolboxes` | arcpy 도구 상자 모듈과 프로젝트 내 도구 상자를 나열합니다. |
| `describe_geoprocessing_tool` | 실행 전에 도구의 파라미터, 데이터 유형, 기본값, 사용 설명을 조회합니다. |
| `run_python_toolbox_tool` | 디스크의 사용자 지정 .pyt / .atbx / .tbx 도구 상자 내 도구를 실행합니다. |
| `check_extension` | ArcGIS 확장 모듈 라이선스를 확인하고 필요하면 체크아웃합니다. |
| `get_messages` | 가장 최근 지오프로세싱 작업의 메시지. |

### 심볼화(6)

| 도구 | 용도 |
|---|---|
| `set_layer_renderer` | 레이어 심볼화를 변경: 단일 심볼, 범주별 고유값, 숫자 필드 기준 등급화/연속 색상 램프. |
| `get_layer_symbology` | 레이어의 현재 렌더러, 등급 구분, 고유값, 라벨 설정을 확인합니다. |
| `list_color_ramps` | 프로젝트에서 사용 가능한 색상 램프를 나열합니다. |
| `set_layer_labeling` | 라벨 켜기/끄기를 전환하고 식, 글꼴, 헤일로를 설정합니다. |
| `apply_symbology_from_layer` | .lyrx 파일 또는 다른 레이어에서 심볼화를 복사합니다. |
| `save_layer_file` | 레이어와 그 심볼화를 .lyrx 파일로 저장해 재사용합니다. |

### 지도 뷰(7)

| 도구 | 용도 |
|---|---|
| `get_map_view` | 현재 카메라 위치: 중심, 축척, 회전, 가시 범위; Pro에서 지도 탭이 열려 있어야 합니다. |
| `set_map_view` | 지도 뷰를 이동: 범위, 중심점, 축척, 회전을 설정합니다. |
| `export_map_view` | 지도 뷰를 PNG로 렌더링해 이미지를 반환하고 육안 확인에 사용합니다; 지도 탭이 열려 있어야 합니다. |
| `list_bookmarks` | 지도에 정의된 공간 북마크를 나열합니다. |
| `create_bookmark` | 현재 뷰 또는 지정 범위를 이름 있는 북마크로 저장합니다. |
| `apply_bookmark` | 지도 뷰를 북마크로 확대합니다. |
| `delete_bookmark` | 지도의 북마크를 삭제합니다. |

### 레이아웃과 출도(16)

| 도구 | 용도 |
|---|---|
| `list_layouts` | 인쇄 레이아웃을 페이지 크기와 요소 수와 함께 나열합니다. |
| `get_layout_info` | 레이아웃 확인: 페이지 설정과 각 요소의 위치·크기. |
| `create_layout` | 새 레이아웃 페이지를 만듭니다(기본값: 페이지를 채우는 지도 프레임 포함). |
| `delete_layout` | 프로젝트에서 레이아웃을 삭제합니다. |
| `add_map_frame` | 레이아웃 페이지의 지정 위치에 지도 프레임을 추가합니다. |
| `set_map_frame_extent` | 레이아웃의 지도 프레임을 특정 레이어, 범위 또는 축척에 맞춥니다. |
| `add_layout_text` | 제목, 크레딧 등 텍스트 요소를 레이아웃에 추가합니다. |
| `add_layout_legend` | 레이아웃의 지도 프레임과 연동된 범례를 추가합니다. |
| `add_layout_scale_bar` | 지도 프레임과 연동된 축척 막대를 추가합니다. |
| `add_layout_north_arrow` | 지도 프레임과 연동된 북쪽 화살표를 추가합니다. |
| `add_layout_picture` | 레이아웃 위에 이미지(로고 등)를 배치합니다. |
| `set_layout_element` | 임의의 레이아웃 요소를 이동, 확대/축소, 이름 변경, 숨기기 또는 텍스트 변경합니다. |
| `delete_layout_element` | 레이아웃에서 요소를 삭제합니다. |
| `export_layout` | 레이아웃을 PDF / PNG / JPEG / SVG / TIFF로 내보냅니다; PNG/JPEG는 확인용 이미지로 인라인 반환됩니다. |
| `preview_layout` | 레이아웃을 임시 이미지로 렌더링해 반환합니다(파일 미기록). 만들면서 확인하기에 편리합니다. |
| `export_map_series` | 레이아웃의 지도 시리즈(맵북)를 여러 페이지 PDF로 내보냅니다. |

### 래스터(5)

| 도구 | 용도 |
|---|---|
| `get_raster_info` | 래스터 상세: 밴드, 크기, 셀 크기, 픽셀 유형, 통계, 좌표계. |
| `set_raster_symbology` | 래스터 레이어의 컬러라이저와 색상 램프를 설정합니다. |
| `raster_calculator` | 래스터 계산(맵 대수): 래스터에 짧은 이름을 묶고 Python 식을 씁니다; Spatial Analyst 확장 모듈이 필요합니다. |
| `sample_raster_values` | 지도 좌표 또는 포인트 레이어 피처 위치에서 래스터 셀 값을 읽습니다. |
| `zonal_statistics` | 존 통계: 구역 폴리곤 내 래스터 값을 집계해 테이블을 반환합니다; Spatial Analyst 확장 모듈이 필요합니다. |

### 카탈로그와 데이터 탐색(6)

| 도구 | 용도 |
|---|---|
| `list_workspace_contents` | 지오데이터베이스 또는 폴더 내 데이터셋을 나열합니다; 기본값은 프로젝트 기본 지오데이터베이스. |
| `list_folder` | 디스크의 GIS 파일과 하위 폴더를 나열합니다; 기본값은 프로젝트 홈 폴더. |
| `describe_dataset` | 어떤 데이터셋이든 경로로 설명합니다: 유형, 지오메트리, 좌표계, 범위, 필드, 행 수(지도에 추가하지 않고). |
| `search_data` | 프로젝트의 지오데이터베이스, 홈 폴더, 폴더 연결에서 이름으로 데이터를 검색. 데이터 추가 전에 위치를 찾는 가장 빠른 방법입니다. |
| `get_project_items` | 프로젝트에 등록된 폴더 연결, 데이터베이스 연결, 도구 상자. |
| `add_folder_connection` | 폴더를 프로젝트에 등록해 그 데이터를 쉽게 찾아볼 수 있게 합니다. |

### 유틸리티(4)

| 도구 | 용도 |
|---|---|
| `execute_arcpy_code` | ArcGIS Pro 안에서 Python을 실행합니다: arcpy는 임포트되어 있고, arcpy.mp.ArcGISProject('CURRENT')는 열린 프로젝트이며, 변수는 호출 간에 유지되고, print 출력은 캡처되며, 마지막 식의 값이 반환됩니다. 전용 도구가 없을 때 사용합니다. |
| `get_pump_status` | 메인 스레드 디스패처가 설치되어 있는지 여부; 명령이 열린 Pro 프로젝트에 접근할 수 있는지를 결정합니다. 없으면 경로 기반 작업만 가능합니다. |
| `stop_pump` | 메인 스레드 디스패처를 제거합니다; 사용자가 다시 설치할 때까지 실시간 프로젝트 계열 명령은 동작하지 않습니다. |
| `run_batch` | 한 번의 왕복으로 여러 명령을 실행합니다. 다단계 워크플로가 훨씬 빨라집니다. |

### 가드(2)

| 도구 | 용도 |
|---|---|
| `guard_summon` | 수동 작업을 위해 온디맨드 대화 상자 가드를 소환합니다: "第三方代码库执行(서드파티 코드 실행)" 신뢰 대화 상자만 클릭; 클릭할 수 없을 때는 최상위에 30초 이상 경고; `timeout_s`로 종료됩니다. |
| `guard_status` | 가드 엔진 경로/버전, 세션 루트와 최근 20개 소환 세션. |

### 템플릿 증류(10)

| 도구 | 용도 |
|---|---|
| `template_list` | 템플릿을 나열합니다. 상태나 태그로 필터링 가능. |
| `template_get` | 템플릿 spec을 읽습니다(레이아웃, 색 구성, 글꼴, 구역, 실패하기 쉬운 지점과 검토 기록). |
| `template_import` | 샘플 도판을 초안으로 가져옵니다(sha256으로 중복 제거). |
| `template_measure` | 도곽 바운딩 박스, 종횡비, 강한 프레임 선, 주 색상, DPI를 측정합니다. |
| `template_patch` | 검증 병합 방식으로 의미 필드를 제출합니다; 보호 필드는 거부됩니다. |
| `template_validate` | schema + 기하 일관성 검증. |
| `template_review` | 인간 검토 결론을 기록합니다. |
| `template_approve` | 검토·검증된 템플릿을 승인합니다(ready로 설정). |
| `template_preview` | 와이어프레임 미리보기 이미지를 반환합니다. |
| `template_apply` | ready 템플릿을 제도 작업 컨텍스트 파일로 작성합니다. |

</details>

---

## 📦 패키지 구성

| 경로 | 내용 | 설명 |
|---|---|---|
| `install\ArcGISProMCP.esriAddinX` | 🧩 add-in v2.0.1 | 125개 네이티브 도구 + 모든 GP 도구; HTTP MCP 6520 / 구 TCP 6510 |
| `install\runtime\stdio-bridge.ps1` | 🌉 stdio 브리지 | 로컬 프로세스만 실행할 수 있는 클라이언트(Claude Desktop 등)를 6520으로 연결; 순수 PowerShell, Python 불필요 |
| `install\runtime\runcode\RunCode.pyt` | 🐍 RunCode 도구 상자 | 임의의 arcpy를 고빈도로 실행할 때 사용; 첫 실행에서 신뢰 대화 상자가 1회 표시(2.0부터 가드 자동 소환) |
| `install\runtime\guard\` | 🛡️ 온디맨드 대화 상자 가드 2.0 | `guard.ps1` 엔진 + 규칙 schema + 구버전 호환 레이어; 실패 시 최상위 알림(30초 이상, 알림음) |
| `install\runtime\distill\` | 🧪 템플릿 증류 런타임 | `run-distill.ps1`은 Pro 내장 Python 사용; 라이브러리 기본값 `%LOCALAPPDATA%\ArcGIS Pro MCP\library` |
| `install\runtime\plugin\` | 🔌 Python 브리지(선택) | Pro의 Python 창에서 시작해 `execute_arcpy_code` 지속 세션 제공(포트 6511) |
| `install\*.cmd / *.ps1` | 🛠️ 설치와 자체 점검 | 설치 / 자체 점검 / Claude 구성 / 대화 상자 가드 / 제거 |
| `clients\` | 📁 클라이언트 연결 설명 | Claude Code, Claude Desktop, Codex, 범용 JSON |
| `docs\` | 📖 사용 가이드, 문제 해결, 작업 규칙, 업데이트 로그 | 먼저 사용 가이드를 보세요 |
| `source\` | 📜 전체 소스 스냅샷 | 이 패키지 바이너리에 대응하는 소스(AGPL-3.0 준수 요건), 업스트림 LICENSE / NOTICE 포함 |

### 🔍 의존성 감사("전부 패키징"인 이유)

- 🧩 add-in: 자체 완결형 C# 어셈블리로, Pro 3.7 지원 환경에 포함된 .NET Desktop Runtime 10 x64에서 동작——이 패키지에 별도 .NET 설치가 필요 없습니다.
- 🌉 stdio 브리지, 🛡️ 대화 상자 가드: Windows 기본 포함 PowerShell 5.1 + WinForms——별도 설치가 없습니다.
- 🐍 RunCode 도구 상자, Python 브리지: ArcGIS Pro 내장 Python(arcpy)에서 동작——별도의 Python 설치가 필요 없습니다.
- ✅ **따라서 이 패키지에는 pip/네트워크로 설치해야 하는 외부 의존성이 없습니다.** "오프라인 사용 가능"은 이 패키지 자체의 설치·실행 경로를 의미합니다; ArcGIS Pro 3.7의 설치 전제 조건(.NET Desktop Runtime 10 x64, WebView2 ≥132)은 Pro 설치 프로그램이 제공합니다.

---

## 🧪 선택 구성 요소 사용법

**🐍 RunCode 도구 상자**(arcpy를 고빈도로 쓰는 상황)
- 경로: `%LOCALAPPDATA%\ArcGIS Pro MCP\runcode\RunCode.pyt`
- AI가 `run_python_toolbox_tool`을 호출할 때 이 경로를 전달하기만 하면 됩니다; 또는 Pro의 "삽입 → 도구 상자 → 도구 상자 추가"에서 선택합니다.
- 각 Pro 세션의 첫 실행에서 "서드파티 코드 라이브러리" 신뢰 대화 상자가 뜹니다: 먼저 `03-dialog-guard.cmd`를 실행해 두면 가드가 자동으로 "예"를 클릭합니다 ✨.

**🔌 Python 브리지**(arcpy 세션을 호출 간에 유지해야 할 때)
1. ArcGIS Pro 메뉴 **분석 → Python**에서 Python 창을 엽니다;
2. 붙여 넣고 실행합니다(경로는 기본 설치 위치에 맞게 작성됨):
   ```python
   import sys; sys.path.insert(0, r"C:\Users\<your-username>\AppData\Local\ArcGIS Pro MCP\plugin")
   import mcp_bridge; print(mcp_bridge.start_server(port=6511))
   ```
3. `CURRENT`(현재 프로젝트)가 필요할 때는 한 줄을 추가합니다(메인 스레드에서 실행해야 합니다):
   ```python
   print(mcp_bridge.start_pump())
   ```

**🛡️ 대화 상자 가드**(RunCode 실행 전이나 ENVI→Pro 연계 흐름 전에 시작 권장)
```
03-dialog-guard.cmd                 # 600초 가드
03-dialog-guard.cmd -DurationSeconds 120
03-dialog-guard.cmd -NotifyTest     # 알림 창만 테스트(30초)
03-dialog-guard.cmd -AlertOnly      # 경고만, 자동 클릭 없음
```
안전 경계: "第三方 / 代码库 / third-party / code library" 문맥이 있는 신뢰 대화 상자만 처리합니다; **덮어쓰기, 삭제 등 다른 확인 대화 상자는 절대 건드리지 않습니다** 🙅. 로그는 `%LOCALAPPDATA%\ArcGIS Pro MCP\guard\watch_pro_dialog.log`.

---

## 🔒 보안 설명

- 🏠 add-in과 모든 브리지 서비스는 `127.0.0.1`만 수신하며, 같은 LAN의 다른 컴퓨터는 연결할 수 없습니다.
- ⚖️ 이 소프트웨어는 `127.0.0.1`에서의 로컬 서비스 제공만 수행하며 제3자에게 네트워크 서비스를 제공하지 않습니다; 수정 후 네트워크를 통해 타인에게 서비스를 제공한다면 AGPL-3.0 §13의 소스 제공 의무가 적용됩니다.
- 🔏 add-in은 서명되지 않은 빌드입니다: 첫 로드 시 ArcGIS Pro가 신뢰 확인을 요구하는데 이는 정상 흐름입니다; 이 패키지의 `SHA256SUMS.txt`로 대조하세요.
- 🕵️ 이 패키지에는 텔레메트리나 네트워크 보고가 전혀 포함되지 않습니다.
- 🧹 제거: `99-uninstall.cmd`(클라이언트 구성을 남길지 선택 가능).

---

## 📜 버전과 라이선스

- 🏷️ 버전: **2.0.1**(빌드 날짜는 `VERSION.json`; 도구 수 125)
- 👤 유지관리자: **Law**([@XNYGggg](https://github.com/XNYGggg))
- 🙏 업스트림 프로젝트와 원저자: **[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)**, 원저자 **Pisut Nakmuenwai**(AGPL-3.0-or-later). 이 패키지는 그 위에 2.0.1까지 패치를 누적했습니다(변경점은 `docs\CHANGELOG.md`와 `RELEASE-NOTES.md`에 기재).
- ⚖️ 라이선스: **AGPL-3.0-or-later**(이 패키지에는 전체 소스 스냅샷과 업스트림 NOTICE가 포함)
- 🏷️ 상표: "ArcGIS"는 Esri의 상표입니다; 이 프로젝트는 서드파티 도구이며 Esri와 제휴 관계가 없습니다.

🆘 FAQ는 `docs\排障手册.md`(문제 해결 매뉴얼), 고급 사용법은 `docs\使用指南.md`(사용 가이드)에 있습니다. 즐거운 제도 되세요 🎨 (｀・ω・´)ゞ

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

> 루둥대학은 1930년에 설립되었으며, 산둥성 옌타이시에 위치한 공립 종합대학입니다. 응용형 인재 양성을 주도로 공학·농학·이학·인문학이 조화롭게 발전하고 있습니다. 26개 단과대학을 두고 있으며, 공학·화학·재료과학 등 6개 분야가 ESI 세계 순위 상위 1%에 진입했습니다. 교훈은 ‘후덕(厚德)·박학(博學)·일신(日新)·독행(篤行)’입니다.

<p align="center">
  📍 중국 산둥성 옌타이시 즈푸구 홍치중로 186호 (우편번호 264025)<br>
  🌐 <a href="https://www.ldu.edu.cn/">공식 홈페이지</a> ·
  <a href="https://jwc.ldu.edu.cn/">교무처</a> ·
  <a href="https://lib.ldu.edu.cn/">도서관</a> ·
  <a href="https://ehall.ldu.edu.cn/">학생 포털</a>
</p>
