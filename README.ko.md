[![English](https://img.shields.io/badge/Language-English-1f6feb?style=flat-square)](README.md) [![简体中文](https://img.shields.io/badge/Language-%E7%AE%80%E4%BD%93%E4%B8%AD%E6%96%87-d1242f?style=flat-square)](README.zh-CN.md) [![日本語](https://img.shields.io/badge/Language-%E6%97%A5%E6%9C%AC%E8%AA%9E-bc002d?style=flat-square)](README.ja.md) [![한국어](https://img.shields.io/badge/Language-%ED%95%9C%EA%B5%AD%EC%96%B4-0047a0?style=flat-square)](README.ko.md) [![Version](https://img.shields.io/badge/Version-Customer%201.1.12-2ea44f?style=flat-square)](VERSION.json) [![Changelog](https://img.shields.io/badge/Docs-Changelog-6f42c1?style=flat-square)](docs/CHANGELOG.md) [![License: AGPL v3+](https://img.shields.io/badge/License-AGPL--3.0--or--later-blue?style=flat-square)](LICENSE) [![ArcGIS Pro](https://img.shields.io/badge/ArcGIS%20Pro-3.7-00a1e0?style=flat-square)](https://www.esri.com/en-us/arcgis/products/arcgis-pro/overview) [![Platform](https://img.shields.io/badge/Platform-Windows%2011%20x64-lightgrey?style=flat-square)]()

# 🗺️ ArcGIS Pro MCP (Optimized) · Customer 1.1.12 (1.14 개선판 + 템플릿 증류) 🚀

자연어 한 문장으로 AI 어시스턴트가 이미 열어 둔 ArcGIS Pro 프로젝트를 직접 조작합니다: 레이어 추가, 심볼 변경, 속성 조회, 지오프로세싱 실행, 지도 내보내기. 데이터는 이 컴퓨터를 떠나지 않으며 모든 통신은 `127.0.0.1`을 통해서만 이루어집니다 🔒.

> **🚀 이 브랜치는 Customer 1.1.12(1.14 개선판 + 템플릿 증류, 빌드 번호 1.1.12)입니다**: **v1.1.4** 위에 1.14 이후의 수정 — 1.1.5(F1–F10), 1.1.6(V-1–V-7), 2026-11-18 실기 전체 테스트에서 확인된 P0–P2 결함 수정(1.1.7–1.1.9), 1.1.10의 재시작 자가 치유, 1.1.11의 정리 수정, 1.1.12에서 2.x로부터 통합한 템플릿 증류 모듈 — 을 쌓았습니다. **v1.1.4는 이미 안정판으로 정식 릴리스되었습니다(권장)**; 이 브랜치는 **자체 Release 자산을 제공하지 않습니다** — 설치 패키지는 `install/`에 포함되어 있으며 언제든 v1.1.4로 롤백할 수 있습니다.

> **📌 버전 안내(2026-11-16 갱신)**: **1.0 계열(v1.1.4 정식 고객판)은 매우 안정적이며 일상 사용에 권장됩니다**; **2.0 계열은 테스트 버전**으로 최적화가 미완료이며 GitHub Release 자산과 태그가 제거되어 **다운로드할 수 없습니다**. 2.0 코드는 `customer-v2.0.1` / `customer-v2.0.0` 브랜치에만 남아 있습니다.

> **🙏 업스트림 프로젝트와 원저자**: 본 고객판은 **[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)**(원저자 **Pisut Nakmuenwai**, AGPL-3.0-or-later)를 기반으로 **v1.1.12**까지 로컬 패치를 누적한 것입니다. 전체 저작자 표시와 수정 고지는 `NOTICE` 및 `source/SOURCE-NOTICE.md`에 있습니다.

이 패키지는 실행에 필요한 **모든 구성 요소**를 함께 담고 있어 완전 오프라인으로 동작합니다. **Python 설치 불필요, pip 불필요, 인터넷 불필요**이며 ArcGIS Pro 자체 외에는 어떤 의존성도 없습니다. 배터리까지 들어 있습니다 🔋

---

## 🆚 1.14(v1.1.4) 대비 업그레이드 내용

아래는 모두 v1.1.4 고객 패키지 이후 반영된 변경입니다. 도구 수 113 → **125**(1.1.5에서 `add_table`과 `restart_pro` 추가, 1.1.12에서 2.x 템플릿 증류 모듈 통합: `template_*` 도구 10개 추가); add-in 바이너리 해시 `A20E263B…C6C7`(476,195 바이트).

| 빌드 | 유형 | 주요 내용 |
|---|---|---|
| **1.1.5** | F1–F10 | `set_environment` 설정이 실제로 GP에 전달(호출별 덮어쓰기 지원); `describe_geoprocessing_tool`이 열거형과 기본값 반환; `create_map` 기본 베이스맵 없음(`project_default`로 기존 동작); 래스터/TIN 확대와 심볼 읽기; **`add_table` 신설**; `export_map_view`에 `fit:"auto"`; `set_raster_symbology`가 명시적 색상/레이블 지원; `set_map_properties`의 epsg 적용; **`restart_pro` 신설** |
| **1.1.6** | V-1–V-7 | toolbox 경로 오타에 "Did you mean …" 제안; 이름 인자 오류에 사용 예시 첨부; `get_pump_status`가 Python 브리지 미기동 시 **구조화된 down 상태**(`running:false, bridge_listening:false`)를 반환하고 실패로 처리하지 않음; 오류 코드(`BRIDGE_NOT_LISTENING / BRIDGE_ERROR / BRIDGE_CLOSED / BRIDGE_TIMEOUT / UNSUPPORTED`)와 `last_error_code/at`; 상태 조회에 5초 읽기 타임아웃; `diagnose`가 6511 포트를 능동 프로브(`bridge_listening` / `probe_ms`); 도구 설명 수정(네이티브 측은 위치 인자만, 브리지 의존 표시); HTTP 전송 검증 강화; 브리지 down 결과 5초 캐시 |
| **1.1.7** | P0 | `create_feature_class` / `create_table`의 `fields`가 **실제로 동작** — 생성 후 `AddField`로 하나씩 추가하고 `describe_dataset`으로 이름/유형/길이를 검증; 실패 시 만들다 만 데이터셋 삭제; `run_batch` 이미지는 기본적으로 path/bytes/sha로 축약(base64로 컨텍스트를 넘치게 하지 않음); `get_features` 페이징(`truncated` / `next_offset`)과 `geometry_summary` / `geometry_precision`; `preview_layout`이 `output_path` / `return_image` 지원 |
| **1.1.8** | P1 | `insert_features` / `update_features` / `delete_features`가 독립 테이블과 데이터셋 경로 지원(테이블은 Table API, 레이어는 편집 세션); `sample_raster_values` 네이티브 픽셀 샘플링 — `points` / `point_layer`, 인라인 반환, `raster_samples` 부작용 테이블 없음(`write_table`은 기존 경로 유지); `create_map` SCENE 수정(`SceneLocal`); `add_join`이 GP 재등록 후에도 유지(JoinRegistry + 클라이언트 연결, 후보 제안 포함); 레이아웃 요소 `x/y`를 왼쪽 아래 원점으로 통일하고 `GetBounds` 검증, 범위 밖은 거부; `run_batch` 각 단계 밀리초와 직렬 표시; GP 결과 맵 추가는 대기+재시도 |
| **1.1.9** | P2 | `get_layer_info` / `set_raster_symbology` / `repair_layer_source`가 데이터셋 경로 수용(고유 일치는 자동 해석, 여러 일치는 레이어 이름 나열); `repair_layer_source`에 래스터 분기 추가(`RasterDataset.ReplaceDataSource`); `search_data` 중복 제거; `check_extension` 의미 수정(`inspectable:false`, Pro는 add-in에 체크아웃을 노출하지 않음); GP 출력 빈 문자열 필터; `get_features`가 래스터 레이어 이름에 `get_raster_info` 안내 |
| **1.1.10** | 재시작 자가 치유 | `restart_pro` 타임아웃 자가 치유 — 저장 대화상자 탐지를 자식 프로세스로 바꾸고 15초 하드 타임아웃(초과 시 포기하고 로그); 버튼 매칭이 현지화된 `是(Y) / 否(N) / 取消(C)` 지원; `restart.log`는 UTF-8로 단계 하트비트 기록; TCP 연결 5초 타임아웃; Pro를 강제 종료하지 않는 원칙 유지. 재시작 예약 플래그는 10분 후 만료되어 어시스턴트가 죽어도 새 요청을 영구히 거부하지 않음. 배포는 `scripts/update_installed.py`(위치 인자, 해시 불일치 시 거부)로 단일화 |
| **1.1.11** | 정리 수정 | `create_feature_class` / `create_table` 실패 시 정리가 데이터셋 삭제 전에 데이터 소스로 맵 레이어/독립 테이블을 해석하고 객체 동일성으로 제거 — 이름만 같은 무관한 레이어는 오삭제하지 않음. 정리 실패는 도구 오류에 보고(예외를 삼키지 않음). `scripts/update_installed.py`의 이 컴퓨터 경로 하드코딩 제거; 빌드가 소스 경로를 DLL/PDB에서 매핑 제거 |
| **1.1.12** | 템플릿 증류 | 2.x 템플릿 증류 모듈 통합: `template_*` 도구 10개 + `arcgis-distill` CLI(Pro 내장 Python 사용, 추가 의존성 없음) + 로컬 템플릿 라이브러리(`%LOCALAPPDATA%\ArcGIS Pro MCP\library`, 업그레이드 시 유지); `skills/template-distill`과 `docs/distill.md` 추가; 도구 수 115→125, 명령 112→122 |

이 컴퓨터 실기 검증(**ArcGIS Pro 3.7 전용**): 1.1.7 회귀 21/21 PASS; 1.1.8 연속 두 라운드 0 실패; 1.1.9 P2 10/10 및 P1 재실행 0 실패; 1.1.10 핸드셰이크 `1.1.10.0`; 1.1.11 핸드셰이크 `1.1.11.0`, ping `command_count=112`, D1 필드 회귀는 현재 테스트 프로젝트에 적용되는 모든 어서션 PASS(4개 레거시 어서션은 퇴역 프로젝트의 레이어 이름 참조), `get_features` 페이징/요약/정밀도 실기 재확인. 다른 Pro 버전은 미검증.

1.1.12(템플릿 증류 이식): 빌드 전 과정 통과(client-registration / installer / HTTP net8.0+net10.0), `tests/test_distill.py` 7/7, `tests/test_catalog_matches_bridge.py` 4/4 통과. 전문가급 지도 도판 50장 일괄 증류 테스트(증류 가능한 49장 전부 성공, 표지 1장은 정상 거부)와 3자 리뷰 기록은 `release/distill-verify-20261119/`에 있습니다. 실기(Pro 3.7, 실제 터미널): 설치 후 핸드셰이크 `1.1.12.0`, `ping` 명령 수 **122**; MCP 경유 `template_*` 전체 흐름(import→measure→patch→validate→preview→review→approve→apply) **11/11 통과**, 공식 자체 점검 21 통과 / 0 실패. 자세한 내용은 `docs/CHANGELOG.md`와 `RELEASE-NOTES.md`.

---

## 🖥️ 시스템 요구 사항

| 항목 | 요구 사항 |
|---|---|
| 🪟 OS | **Windows 11 x64**(64비트 Home / Pro / Enterprise, ArcGIS Pro 3.7 공식 지원). **아키텍처: x64(AMD64) 전용 — ARM64 미지원** |
| 🗺️ ArcGIS Pro | **3.7**(Pro 3.7 / .NET 10 기반 빌드. 실기 검증은 3.7만 수행, 이후 버전 미검증) |
| 🔑 권한 | 일반 사용자로 충분하며 설치에 **관리자 권한 불필요** |
| 📦 기타 의존성 | 없음(PowerShell과 .NET은 Windows / Pro에 포함) |

---

## 🚀 5분 시작

1. 📂 **압축 해제**: 원하는 폴더에 풀기(예: `D:\ArcGIS-Pro-MCP-Customer`).
2. 🛠️ **ArcGIS Pro를 닫고** `install\00-install.cmd`를 더블클릭. add-in 설치, 런타임 배포, Claude Desktop / Claude Code 설정을 자동으로 수행합니다.
3. 🖥️ **ArcGIS Pro 실행**. 첫 로드에서 "추가 기능을 신뢰하시겠습니까"가 나오면 **예**를 클릭(안전합니다 😄). Pro 상단의 **MCP 탭**에서 상태가 **running**(포트 `127.0.0.1:6520`)인지 확인.
4. 🔌 **AI 클라이언트 연결**(아래 "설정" 참조) 후 `install\01-check.cmd`를 더블클릭; 모든 항목이 `[OK]`면 준비 완료 (๑•̀ㅂ•́)و✧

> 검사 시 ArcGIS Pro가 꺼져 있으면 포트 검사가 경고로 나옵니다 — Pro를 켜고 다시 실행하세요 👌

---

## 🎛️ 설정 — AI 클라이언트 연결

**전제**: ArcGIS Pro가 열려 있고 **MCP 탭**이 **running** 상태. 모든 포트는 이 컴퓨터에만 바인딩됩니다:

| 포트 | 용도 |
|---|---|
| `6520` | MCP over HTTP — 모든 AI 클라이언트가 여기로 연결 |
| `6510` | 구형 TCP 브리지(최신 클라이언트는 사용하지 않음) |
| `6511` | 선택적 Pro 내 Python 브리지(`execute_arcpy_code`) |

### 🟣 Claude Code

```powershell
claude mcp add --transport http arcgis http://127.0.0.1:6520/mcp
claude mcp list
```

수동 동등 설정(`~/.claude.json`):

```json
{ "mcpServers": { "arcgis": { "type": "http", "url": "http://127.0.0.1:6520/mcp" } } }
```

제거는 `claude mcp remove arcgis`, 재추가는 위 명령.

### 🖥️ Claude Desktop(내장 stdio 브리지 사용)

Claude Desktop은 HTTP에 직접 연결할 수 없고 로컬 프로세스만 실행할 수 있습니다 — 패키지에 포함된 순수 PowerShell 브리지를 사용하세요(Python 불필요):

1. **Claude Desktop 완전 종료**(트레이 아이콘 포함).
2. `install\02-configure-claude.cmd` 실행 또는 수동 편집(먼저 백업).
3. 설정 파일 `claude_desktop_config.json`은 다음 중 "존재하는 첫 번째" 폴더에 있습니다:
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

4. Claude Desktop 재실행 후 도구 목록에 `arcgis`가 보이면 성공.

### 🟢 Codex

`~/.codex/config.toml`에 추가(기존 `arcgis` 블록이 있으면 `env` 하위 테이블 포함 통째로 교체):

```toml
[mcp_servers.arcgis]
url = "http://127.0.0.1:6520/mcp"
```

저장 후 Codex 재시작.

### 🔧 기타 MCP 클라이언트

Streamable HTTP로 `http://127.0.0.1:6520/mcp`에 연결하거나 위의 범용 JSON 사용. (Antigravity는 `serverUrl`을 쓰고 `type`은 넣지 않음.)

### ✅ 연결 확인

- `install\01-check.cmd` — Pro 버전, add-in 해시, 런타임, 포트, 클라이언트 설정 검사.
- `netstat -ano | findstr 6520` — 포트가 리스닝 중인지 확인.
- 로그: `%LOCALAPPDATA%\ArcGIS-MCP\addin.log`.

---

## 🎯 사용 방법

**기본 루프**: 데이터가 있는 프로젝트 열기 → 원하는 것을 말하기 → 어시스턴트가 도구 선택 → 결과 확인(이미지는 인라인으로 반환) 🎨

| 이렇게 말하면 | 실제로 일어나는 일 |
|---|---|
| "면적으로 구·현을 단계 구분 색상, 연한 빨강에서 진한 초록" | `set_layer_renderer`(단계 구분 색상) |
| "인구 50만 초과 현을 선택해 새 피처 클래스로 내보내기" | `select_features` → `export_features` |
| "도로에 500m 버퍼 만들기" | `run_geoprocessing_tool`(Buffer) |
| "향·진별 경지 면적 집계" | `summarize_features` / `zonal_statistics` |
| "현재 지도 뷰를 캡처해 줘" | `export_map_view`(PNG 반환) |
| "제목·범례·축척 막대·방위표가 있는 A3 세로 PDF 출력" | `create_layout` → `add_map_frame` → `add_layout_*` → `export_layout` |
| "12개 레이어를 같은 심볼로 일괄 내보내기" | `run_batch` 또는 `execute_arcpy_code` |
| "NAME, AREA_HA, CODE 필드가 있는 빈 피처 클래스 생성" | `create_feature_class`(필드가 실제로 생성·검증됨) |

**도구 이름으로 호출**: 아래 각 도구는 클라이언트에서 이름으로 직접 호출할 수 있습니다; `run_batch`는 여러 명령을 한 번의 왕복으로 묶습니다; 전용 도구가 없으면 `execute_arcpy_code`로 대체.

**편집 규칙**: 편집은 `save_edits` 전까지 보류되며 보류 중에는 데이터가 잠깁니다(삭제/덮어쓰기 실패). `discard_edits`로 폐기. `delete_features` / `delete_dataset`은 되돌릴 수 없으니 먼저 영향 범위 설명을 요청하세요.

**배치와 속도**: 반복이 많거나 데이터가 큰 작업은 RunCode용 멱등 arcpy 스크립트를 작성해 한 번에 실행; 큰 프로젝트는 범위/OID로 나누고 체크포인트를 남기세요. `run_batch` 내 이미지 결과는 기본적으로 path/bytes/sha로 축약되어 대화를 범람시키지 않습니다.

---

## 🧩 세 가지 모듈: 역할, 사용법, 프롬프트 작성법

본 프로젝트는 "한 저장소, 세 모듈" 설계입니다. 각 모듈은 독립적으로 쓸 수 있고 실제로는 함께 사용합니다:

| 모듈 | 한 줄 역할 | 사용 시점 | 본 버전 상태 |
|---|---|---|---|
| 🧩 **MCP 애드인** | AI와 Pro를 잇는 유일한 간선: 125개 MCP 도구 + RunCode / Python 브리지 두 arcpy 경로 | 일상의 모든 작업, 배치 스크립트, `CURRENT`가 필요한 대화형 세션 | ✅ 설치 즉시 사용 |
| 🛡️ **다이얼로그 가드** | "서드파티 코드 라이브러리" 신뢰 대화상자를 자동 클릭, 못 누르면 최상위 링 알림 | 첫 RunCode.pyt 실행, 무인 배치 | ✅ 독립 스크립트 `03-dialog-guard.cmd` |
| 🧪 **템플릿 증류** | 샘플 도면을 재사용 가능한 레이아웃 템플릿으로 증류해 배치 출력 양식 통일 | 여러 지도에 같은 양식, 논문/보고서 그림 재현 | ✅ 1.1.12부터 포함(2.x 이식) |

### 🧩 모듈 1: MCP 애드인(핵심)

- **역할**: AI와 Pro 사이의 유일한 채널 — 레이어, 심볼, 지오프로세싱, 지도 출력이 모두 여기를 통합니다. arcpy는 두 경로: **RunCode 도구상자**는 단발/배치, **Python 브리지**는 변수 유지나 `CURRENT`가 필요한 대화형에 적합.
- **사용 방법**:
  1. 설치 후 Pro를 열고 상단 **MCP 탭**이 **running**(`127.0.0.1:6520`)인지 확인;
  2. RunCode: 어시스턴트가 `run_python_toolbox_tool`을 호출할 때 `%LOCALAPPDATA%\ArcGIS Pro MCP\runcode\RunCode.pyt` 경로를 전달;
  3. Python 브리지(선택): Pro 메뉴 **분석 → Python**에서 브리지 시작 코드를 붙여넣기(포트 6511; `CURRENT`가 필요하면 `start_pump()` 한 줄 추가 — 아래 "RunCode와 Python 브리지" 참조).
- **프롬프트 작성법**: **대상 + 작업 + 출력**을 명확히; 배치 작업은 "하나 먼저 만들어 확인시켜 줘"를 덧붙임.
  - ✅ "`구현경계`를 `AREA`로 자연 분류 5단계, 연한 빨강에서 진한 초록, 범례는 오른쪽 아래, A3 가로 PDF를 `D:\out\`에 300 dpi로."
  - ✅ "RunCode로 멱등 스크립트: 모든 레이어 라벨 켜기, 필드 `NAME`, 크기 10; 이미 켜진 것은 건너뛰기."
  - ✅ "이 arcpy는 `CURRENT`가 필요해: 브리지 시작 코드를 내 사용자 이름으로 채워서 보내 줘; Pro에 내가 직접 붙여넣을게."
  - ❌ "데이터 좀 처리해 줘." — 대상·작업·출력이 없어 어시스턴트가 계속 되묻게 됩니다.

### 🛡️ 모듈 2: 다이얼로그 가드(RunCode 실행 시에만 필요)

- **역할**: Pro가 처음 서드파티 `.pyt`를 실행할 때 "서드파티 코드 라이브러리" 신뢰 대화상자가 떠서 무인 작업이 멈춥니다. 가드는 이 "예(Y)"만 클릭하고, 못 누르면 최상위 알림(최소 30초, 10초마다 링). 덮어쓰기·삭제 등 다른 확인 대화상자는 **절대 건드리지 않습니다**.
- **사용 방법**(자세한 내용은 아래 "다이얼로그 가드 사용법"):
  ```text
  install\03-dialog-guard.cmd                    # 기본 600초
  install\03-dialog-guard.cmd -DurationSeconds 120
  install\03-dialog-guard.cmd -AlertOnly         # 알림만, 자동 클릭 안 함
  ```
  Pro 프로세스마다 첫 신뢰 후에는 다시 뜨지 않지만, Pro 재시작 후에는 다시 가드가 필요합니다.
- **프롬프트 작성법**: "가드 시작"과 "무엇을 실행할지"를 같은 문장에 넣기.
  - ✅ "RunCode.pyt를 실행할게: 백그라운드로 다이얼로그 가드 600초 시작, 신뢰 대화상자는 '예'를 자동 클릭, 못 누르면 최상위로 링 알림."
  - ✅ "이번엔 알림만, 자동 클릭하지 마."
  - ❌ "모든 대화상자 좀 닫아 줘." — 가드는 신뢰 대화상자만 담당하며, 다른 확인은 사용자가 결정해야 합니다.

### 🧪 모듈 3: 템플릿 증류(1.1.12부터 포함, 2.x에서 이식)

- **역할**: 샘플 완성 도면을 재사용 가능한 레이아웃 템플릿으로 증류; 이후 배치 출력은 템플릿을 적용해 양식이 통일·재현됩니다.
- **사용 방법**: 본 버전에 **포함됨**(1.1.12부터, 2.x 이식). 어시스턴트에게 "이 그림을 템플릿으로 증류해 줘"라고 하면 `import → measure → patch → validate → preview → review → approve → apply` 흐름으로 10개 `template_*` 도구를 실행합니다. 결정적 단계는 `arcgis-distill`이 수행하고, 의미 필드는 어시스턴트가 `skills/template-distill/SKILL.md`에 따라 작성합니다. 템플릿 라이브러리는 `%LOCALAPPDATA%\ArcGIS Pro MCP\library`이며 업그레이드 시 유지됩니다.
- **프롬프트 작성법**: 템플릿에 이름을 붙이고 흐름과 확인 지점을 적기.
  - ✅ "`D:\samples\fig3.png`를 '연구구역 개관도' 템플릿으로 증류: import → measure → patch → validate → preview, 미리보기를 먼저 나에게 확인시켜 주고, 확인 후 review + approve."
  - ✅ "'연구구역 개관도' 템플릿으로 `maps`의 각 지도를 A3 PDF로 출력, 제목은 지도 이름, 출력은 `D:\out\maps\`."
  - ❌ "이 그림처럼 해 줘." — 템플릿 이름·흐름·확인 지점이 없어 양식을 재현할 수 없습니다.

### 🧭 프롬프트 공통 원칙(세 모듈 모두 적용)

1. **모듈 지정**: 일반 작업은 지정 불필요(기본 MCP); 스크립트는 "**RunCode** 사용", 지속 세션은 "**Python 브리지** 사용", 무인 실행은 먼저 "**다이얼로그 가드** 시작", 양식 통일은 "**템플릿** 사용".
2. **대상 구체화**: 레이어 이름·필드 이름·경로를 백틱으로 그대로 쓰기. "그 레이어", "아까 그 데이터"는 피하기.
3. **출력 고정**: 형식(PNG / PDF / 새 피처 클래스), 크기나 해상도, 저장 경로.
4. **확인 지점 남기기**: 배치·덮어쓰기·삭제 작업은 "하나 먼저 보여 줘" 또는 "영향 범위부터 설명해 줘".

> 지도 제작 프롬프트의 전체 4요소와 템플릿은 다음 절 참조.

---

## 🎨 지도 제작 사용법(프롬프트 가이드)

이 절에서는 "AI에게 어떻게 말할지"를 깊이 다룹니다. 도구 이름을 외울 필요 없이 아래 4요소를 전달하면 어시스턴트가 MCP 호출로 분해합니다.

### 네 가지를 말하면 한 번에 맞습니다

| 요소 | 전달할 것 | 예시 |
|---|---|---|
| 데이터 | 레이어 이름, 데이터 경로, 현재 선택 | "현재 지도의 `구현경계` 레이어" |
| 작업 | 분류 필드/방법, 버퍼 거리, 통계 기준 | "`AREA`로 5단계, 자연 분류" |
| 스타일 | 색상, 글꼴 크기, 범례/축척 막대/방위표 위치, 베이스맵 | "연한 빨강에서 진한 초록, 범례는 오른쪽 아래" |
| 출력 | 이미지/PDF/새 피처 클래스, 해상도, 저장 경로 | "A3 가로 PDF, 300 dpi, `D:\out\`에 저장" |

**좋은 프롬프트 vs 나쁜 프롬프트**
- ❌ "지도 좀 예쁘게 해 줘" — 목표가 없어 어시스턴트가 추측만 하게 됩니다.
- ✅ "`토지이용`을 `DLMC` 고유값으로 심볼화, 범례는 오른쪽 아래, A3 가로 PDF를 `D:\out\landuse.pdf`에 300 dpi로 내보내기."
- ❌ "지도 하나 뽑아 줘" — 범위·레이아웃·출력이 없습니다.
- ✅ "`연구구역` 레이어를 범위로, 축척 1:10000, A4 세로 PNG, 제목 '연구구역 토지이용 현황', 범례·축척 막대·방위표 포함; 미리보기를 먼저 확인시켜 줘."

### 도구 그룹별 프롬프트 속성표

| 모듈(도구 그룹) | 이렇게 말할 수 있음 | 어시스턴트가 사용 |
|---|---|---|
| 세션과 프로젝트 | "먼저 `diagnose`로 연결과 현재 프로젝트 확인; 그다음 프로젝트를 `D:\proj\결과.aprx`로 다른 이름 저장" | diagnose / save_project |
| 레이어 | "`roads`를 `베이스` 그룹으로 이동, 투명도 40%, 1:50000보다 확대 시에만 표시; `POI` 레이어 끄기" | move_layer / set_layer_transparency / set_layer_scale_range / set_layer_visibility |
| 데이터와 속성 | "`parcels`의 처음 20행을 면적 내림차순으로 읽기; `DLMC`별 면적 집계" | get_features / summarize_features |
| 선택 | "`POP`이 50만 초과인 현을 선택해 `D:\out\counties_big.shp`로 내보내기; 그다음 `연구구역` 안의 필지를 위치로 선택" | select_features / select_by_location / export_features |
| 필드와 데이터셋 | "`parcels`에 DOUBLE 필드 `AREA_HA` 추가하고 `!Shape_Area!/10000`로 계산; EPSG:4547 폴리곤 피처 클래스 `결과_2026` 생성" | add_field / calculate_field / create_feature_class |
| 지오프로세싱 | "도로에 500m 버퍼 후 디졸브; `연구구역` 마스크로 토지이용 래스터 클립" | run_geoprocessing_tool / run_batch |
| 심볼 | "면적으로 5단계, 자연 분류, 연한 빨강에서 진한 초록; 또는 `TYPE` 고유값 색상" | set_layer_renderer |
| 지도 뷰 | "`연구구역`으로 확대, 축척 1:10000, 1600×900 PNG로" | set_map_view / zoom_to_layer / export_map_view |
| 레이아웃과 출력 | "A3 가로 레이아웃 생성, 제목…, 범례는 3개 레이어만, 축척 막대와 방위표 추가, 300 dpi PDF 출력" | create_layout / add_layout_* / export_layout |
| 래스터 | "DEM의 밴드·셀 크기·통계 읽기; NDVI 계산" | get_raster_info / raster_calculator / zonal_statistics |
| 카탈로그와 데이터 탐색 | "프로젝트와 폴더 연결에서 이름에 `2025`가 들어간 shapefile 검색" | search_data / describe_dataset / list_folder |
| 유틸리티 | "한 번에: 베이스맵 숨기기→구·현으로 확대→PNG 내보내기; arcpy로 모든 레이어 이름에 접두어 붙이기" | run_batch / execute_arcpy_code |

### 표준 지도 제작 프롬프트 템플릿(복사해서 사용)

```text
현재 프로젝트의 <지도 이름>으로 지도 제작:
- 범위: <연구구역 레이어/범위>, 축척 1:<...>
- 데이터: <레이어1>, <레이어2> (필요 시 필드와 필터 조건 명시)
- 심볼: <필드로 단계 구분/고유값/단색>, 색상 <램프 또는 16진수>
- 레이아웃: <A3 가로 / A4 세로>, 제목 "<제목>", 범례 <위치>, 축척 막대 <위치>, 방위표 <위치>
- 출력: <PDF / PNG>, <300> dpi, <절대 경로>에 저장
- 흐름: 먼저 레이아웃을 자체 점검하고 미리보기를 나에게 보내 확인시킨 후 정식 파일을 출력.
```

### 배치 출력 프롬프트

```text
`maps`의 각 지도를 동일한 A3 가로 레이아웃으로 PDF 출력:
제목은 지도 이름, 범례는 표시 중인 레이어, 축척 막대와 방위표는 오른쪽 아래,
출력은 `D:\out\maps\`, 300 dpi. 먼저 한 장만 내보내 확인시킨 후 배치.
```

### 흔한 실수 한 줄 복구

- "범례가 주도를 가렸어" → 어시스턴트가 `legend` 좌표나 레이아웃 조정.
- "페이지 비율이 이상해/여백이 너무 많아" → map frame과 페이지 비율 조정.
- "색이 너무 튀어/논문 스타일이 아니야" → 램프, 16진수, 참고 이미지 지정.
- "아직 내보내지 말고 미리보기 보여 줘" → 어시스턴트가 먼저 `export_map_view` 또는 레이아웃 미리보기.

**효과**: "데이터·작업·스타일·출력"을 한 번에 전달하면 보통 한 라운드로 최종본에 가까운 지도를 얻습니다; 이후 수정은 차이만 지적하면 됩니다("범례를 오른쪽으로 1cm").

---

## 🛡️ 다이얼로그 가드 사용법(1.x 버전)

**무엇을 해결하나**: ArcGIS Pro가 처음 서드파티 `.pyt` / Python 도구(RunCode.pyt 등)를 실행하면 "서드파티 코드 라이브러리" 신뢰 대화상자가 떠서 무인 작업이 멈춥니다. 본 버전에는 MCP 가드 도구가 없고, 패키지에 **독립 가드 스크립트**가 포함됩니다: 시작하면 Pro 창을 폴링하며 이 "예(Y)"만 클릭하고 설정 시간 후 자동 종료합니다.

**시작 방법**

```text
install\03-dialog-guard.cmd                    # 기본 600초
install\03-dialog-guard.cmd -DurationSeconds 120
install\03-dialog-guard.cmd -AlertOnly         # 최상위 알림만, 자동 클릭 안 함
install\03-dialog-guard.cmd -NotifyTest        # 알림 창 테스트(30초)
```

`03-dialog-guard.cmd`를 직접 더블클릭하거나, 터미널 조작이 가능한 AI 어시스턴트가 백그라운드로 시작하게 할 수 있습니다.

**프롬프트 예시**

| 이렇게 말하면 | 어시스턴트가 하는 일 |
|---|---|
| "Pro에서 직접 RunCode.pyt를 실행할 거야. 먼저 다이얼로그 가드를 600초 시작해 줘; 서드파티 코드 신뢰 대화상자가 뜨면 '예'를 자동 클릭, 못 누르면 최상위로 링 알림" | 백그라운드로 `install\03-dialog-guard.cmd -DurationSeconds 600` 실행 |
| "이번엔 알림만, 자동 클릭하지 마" | `-AlertOnly`로 시작 |
| "알림 창이 뜨는지 테스트해 줘" | `-NotifyTest`(30초) 시작 |
| "가드 로그는 어디 있어?" | `%LOCALAPPDATA%\ArcGIS Pro MCP\guard\watch_pro_dialog.log` |

**경계와 효과**
- "서드파티 / 코드 라이브러리 / third-party / code library" 문맥의 신뢰 대화상자만 처리; 덮어쓰기·삭제 등 다른 확인 대화상자는 일절 건드리지 않음.
- 버튼을 못 찾으면 최상위 알림 창이 최소 30초 표시되고 10초마다 링.
- Pro 프로세스마다 첫 신뢰 후에는 뜨지 않음; Pro 재시작 후 다시 필요.
- 2.x에서는 같은 기능이 `guard_summon` / `guard_status`로 발전했고 RunCode 호출 전 자동 arm/settle; 본 버전은 스크립트를 수동 시작.

---

## 🐍 RunCode와 Python 브리지 사용법(프롬프트 가이드)

arcpy 경로는 두 단계: **RunCode 도구상자**는 단발/배치 스크립트, **Python 브리지**는 호출 간 변수를 유지해야 하는 대화형 세션에 적합합니다.

### RunCode 도구상자

경로: `%LOCALAPPDATA%\ArcGIS Pro MCP\runcode\RunCode.pyt`. 어시스턴트가 `run_python_toolbox_tool`을 호출할 때 이 경로를 전달하면 됩니다; Pro에서 "삽입 → 도구상자 → 도구상자 추가"로 직접 추가해도 됩니다.

**프롬프트 예시**
- "RunCode.pyt로 현재 프로젝트의 모든 레이어 라벨을 켜기, 필드 `NAME`, 크기 10."
- "멱등 arcpy 스크립트를 작성해 RunCode로 모든 레이어 이름에 접두어 `LYR_` 붙이기; 이미 붙은 것은 건너뛰기."
- "이 스크립트는 RunCode.pyt에서 실행할 거야; 먼저 인자와 경로를 점검한 후 실행해 줘."

**첫 실행** 시 "서드파티 코드 라이브러리" 신뢰 대화상자가 뜹니다: 위의 다이얼로그 가드를 먼저 시작하거나 한 번 직접 "예"를 클릭하세요. 같은 Pro 프로세스에서는 한 번만 신뢰하면 됩니다.

### Python 브리지(`CURRENT` 또는 지속 변수가 필요할 때)

1. Pro 메뉴 **분석 → Python**으로 Python 창 열기;
2. 붙여넣고 실행(기본 설치 위치 기준 경로):

```python
import sys; sys.path.insert(0, r"C:\Users\<사용자 이름>\AppData\Local\ArcGIS Pro MCP\plugin")
import mcp_bridge; print(mcp_bridge.start_server(port=6511))
```

3. `CURRENT`(현재 프로젝트)가 필요하면 한 줄 더(메인 스레드에서 실행 필수):

```python
print(mcp_bridge.start_pump())
```

**프롬프트 예시**
- "다음 arcpy는 `CURRENT`가 필요해. Pro의 Python 창에 브리지 시작 코드를 붙여넣도록 알려 주고, 그다음 `execute_arcpy_code`로 실행해 줘."
- "브리지 시작 코드를 내 사용자 이름으로 채워서 복사해 줘; Pro에 내가 직접 붙여넣을게."
- "먼저 `get_pump_status`로 pump 준비 상태 확인; 준비 안 됐으면 붙여넣기를 알려 줘."
- "지속 세션으로 세 단계: 먼저 레이어 읽기, 다음 필드 생성, 마지막 계산 — 변수가 호출 간에 유지되어야 해."

**효과**: RunCode는 "배치를 한 번에", Python 브리지는 "다단계 대화형·현재 프로젝트"를 담당하며, 둘 다 별도 Python 설치가 필요 없습니다.

---

## 🧪 증류 모듈에 대하여(1.1.12부터 포함)

본 브랜치는 **1.1.12**부터 템플릿 증류를 **포함**합니다(2.x에서 전면 이식): 10개 `template_*` 도구 + `arcgis-distill` CLI + 로컬 템플릿 라이브러리. 2.x 고유의 `guard_summon` / `guard_status`는 여전히 본 라인에 없으며, 다이얼로그 가드는 독립 스크립트 `03-dialog-guard.cmd`를 그대로 사용합니다.

- 10개 `template_*` 도구: import / measure / patch / validate / preview / review / approve / apply / list / get;
- 템플릿 라이브러리는 `%LOCALAPPDATA%\ArcGIS Pro MCP\library`(업그레이드 시 유지);
- 결정적 단계(measure / validate / preview / import)는 Pro 내장 Python의 `arcgis-distill`이 수행(추가 의존성 없음); 의미 필드는 어시스턴트가 `skills/template-distill/SKILL.md`에 따라 작성하며 도구 자체는 API 키가 필요 없습니다;
- 대표 사용법: "`D:\samples\fig3.png`를 '연구구역 개관도' 템플릿으로 증류; import → measure → patch → validate → preview 흐름으로, 미리보기 확인 후 review + approve."

1.1.12와 2.x 기능 비교:

| 기능 | 1.1.12(본 브랜치) | 2.x |
|---|---|---|
| 네이티브 MCP 도구 | 125개(13그룹) | 125개(14그룹, guard 2개 포함) |
| RunCode / Python 브리지 | ✅ | ✅ |
| 다이얼로그 가드 | 독립 스크립트 `03-dialog-guard.cmd` | `guard_summon` / `guard_status` + RunCode 자동 arm/settle |
| 템플릿 증류 | ✅ 10개 `template_*` 도구 + schema v1.1 | ✅ 동일 |

---

## 🧰 전체 125개 MCP 도구(13그룹)

어시스턴트가 할 수 있는 일의 전부를 이름으로 나열합니다. 클릭해서 펼치기 👇

<details>
<summary><b>📜 전체 도구 목록 — 세션 16 · 레이어 20 · 데이터 11 · 선택 6 · 필드/데이터셋 11 · 지오프로세싱 7 · 심볼 6 · 뷰 7 · 레이아웃 16 · 래스터 5 · 카탈로그 6 · 유틸리티 4 · 템플릿 증류 10</b></summary>

### 세션과 프로젝트(16)

| 도구 | 용도 |
|---|---|
| `ping` | 브리지 연결 가능 여부를 확인하고 연결된 프로젝트를 반환. |
| `get_capabilities` | 브리지가 지원하는 모든 명령을 영역별로 나열해 명명 도구 밖의 기능을 발견. |
| `get_queue_status` | 3레인 큐 상태: 레인별 깊이, 현재 명령, 최근 완료; 느리거나 대기가 생기면 먼저 확인. |
| `diagnose` | 연결 자가 진단: Pro 버전, 라이선스, 열린 프로젝트, 활성 지도, 지도 뷰, 쓰기 권한; Python 브리지 포트도 능동 프로브(`bridge_listening` / `probe_ms`). 문제 시 가장 먼저. |
| `get_arcgis_info` | ArcGIS Pro 버전, 라이선스 수준, 사용 가능한 확장, 현재 프로젝트 경로. |
| `get_project_info` | 프로젝트 경로, 기본 지오데이터베이스와 도구상자, 지도, 레이아웃, 폴더/데이터베이스 연결. |
| `save_project` | 프로젝트(.aprx) 저장 또는 다른 위치에 사본 저장. |
| `list_maps` | 모든 지도와 씬을 좌표계·레이어 수와 함께 나열. |
| `create_map` | 프로젝트에 지도 또는 씬 생성(기본 베이스맵 없음; `project_default`로 기존 동작). |
| `remove_map` | 프로젝트에서 지도 삭제. |
| `activate_map` | Pro UI에서 지도 뷰 열기/활성화; 카메라 명령과 export_map_view 전에 필수. |
| `set_map_properties` | 지도 이름 변경 또는 좌표계 변경(`epsg` 적용). |
| `get_map_extent` | 지도 내 모든 데이터 레이어의 합성 범위. |
| `get_environment` | 현재 arcpy 지오프로세싱 환경 읽기. |
| `set_environment` | arcpy 환경(작업 공간, 출력 좌표계, 범위, 마스크, 셀 크기, 출력 덮어쓰기, 병렬 계수, 스냅 래스터 등) 설정; 값이 실제로 GP에 전달되고 호출별 덮어쓰기 지원. |
| `restart_pro` | 프로젝트를 저장·재개방한 후 ArcGIS Pro 재시작 — 재빌드한 add-in을 데스크톱 조작 없이 로드하는 방법; 응답은 Pro 종료 전에 반환되고 브리지 복구 후 클라이언트가 재연결. |

### 레이어(20)

| 도구 | 용도 |
|---|---|
| `get_layers` | 지도의 레이어(그리기 순서와 그룹 중첩 포함)와 독립 테이블 나열; 프로젝트 파악은 여기서 시작. |
| `get_layer_info` | 레이어 하나의 상세: 데이터 소스, 좌표계, 범위, 필드, 피처 수, 렌더러, 라벨 상태; 데이터셋 경로도 허용(1.1.9+). |
| `add_layer` | 경로 또는 서비스 URL에서 데이터 추가: 피처 클래스, shapefile, 래스터, 테이블, .lyrx, 웹 서비스. |
| `add_table` | 독립 테이블(지오데이터베이스 테이블 또는 폴더의 테이블)을 지도에 추가. |
| `add_web_layer` | URL로 웹 레이어 추가(Feature/Map/Image 서비스, WMS, WMTS, 벡터 타일). |
| `remove_layer` | 지도에서 레이어 또는 독립 테이블 제거. |
| `rename_layer` | 콘텐츠 목록의 레이어 이름 변경. |
| `duplicate_layer` | 지도 안에서 레이어를 복제해 다른 심볼로 표시 가능. |
| `set_layer_visibility` | 하나/여러/모든 레이어 표시 전환. |
| `set_layer_transparency` | 레이어 투명도(0 불투명, 100 완전 투명). |
| `set_layer_scale_range` | 레이어가 그려지는 축척 범위 제한. |
| `set_definition_query` | 레이어 정의 쿼리(SQL where) 설정; 빈 문자열은 해제. |
| `move_layer` | 레이어 순서 변경 또는 그룹으로 이동. |
| `create_group_layer` | 그룹 레이어 생성(기존 레이어 이동 가능). |
| `zoom_to_layer` | 지도 뷰를 레이어 범위로 확대. |
| `set_basemap` | 베이스맵 설정: 지형, 이미지, 이미지 하이브리드, 거리, 내비게이션, 연회색, 진회색, 지형 음영, 해양, OpenStreetMap, 내셔널 지오그래픽 등. |
| `get_broken_layers` | 모든 지도에서 데이터 소스가 끊긴 레이어 나열. |
| `repair_layer_source` | 레이어를 새 작업 공간/데이터셋으로 다시 지정해 복구(래스터 포함, 1.1.9+). |
| `add_join` | 공통 필드로 테이블을 레이어에 조인(클라이언트 측 JoinRegistry; GP 재등록 후에도 유지). |
| `remove_join` | 레이어의 조인 제거. |

### 데이터와 속성(11)

| 도구 | 용도 |
|---|---|
| `get_features` | 레이어/테이블/데이터셋 경로에서 속성 행 읽기; where, 필드 부분 집합, 정렬, 지오메트리 지원. 페이지는 `truncated` / `next_offset`을 반환하고 지오메트리는 요약 또는 정밀도 지정 WKT. |
| `count_features` | 피처 수 집계(where 가능). |
| `get_unique_values` | 필드의 고유값과 각 행 수. |
| `get_field_statistics` | 수치 필드의 min/max/mean/median/sum/표준편차. |
| `summarize_features` | 하나 이상의 필드로 그룹 집계 — "범주별 몇 건/얼마"를 GP 없이 즉답. |
| `insert_features` | 레이어, 독립 테이블, 데이터셋 경로에 행 삽입. |
| `update_features` | where 조건에 맞는 행의 속성 및/또는 지오메트리 갱신(레이어와 테이블 모두). |
| `delete_features` | where 조건에 맞는 행 삭제(레이어와 테이블 모두). |
| `save_edits` | 보류 중인 편집 커밋; 저장되지 않은 편집은 데이터를 잠가 삭제/덮어쓰기가 실패. |
| `discard_edits` | 보류 중인 편집 폐기. |
| `calculate_field` | 필드 계산. 예: `!Shape_Area! / 10000`. |

### 선택(6)

| 도구 | 용도 |
|---|---|
| `select_features` | SQL where 조건으로 피처 선택. |
| `select_by_location` | 다른 레이어와의 공간 관계로 선택. |
| `get_selection` | 현재 선택 상황을 레이어별로 보고(ObjectID와 선택적 속성 행). |
| `set_selection` | ObjectID로 지정 선택. |
| `clear_selection` | 단일 레이어 또는 지도 전체 선택 해제. |
| `zoom_to_selection` | 지도 뷰를 선택 피처로 확대. |

### 필드와 데이터셋(11)

| 도구 | 용도 |
|---|---|
| `list_fields` | 레이어 필드를 유형·별칭·길이·도메인과 함께 나열. |
| `add_field` | 레이어 또는 테이블에 필드 추가. |
| `add_fields` | 한 번의 호출로 여러 필드 추가. |
| `delete_field` | 하나 이상의 필드 삭제. |
| `alter_field` | 필드 이름 또는 별칭/길이 변경. |
| `create_feature_class` | 빈 피처 클래스 생성(기본은 프로젝트 기본 지오데이터베이스) 후 지도에 추가; 요청 필드는 생성 후 하나씩 추가·검증되며 불일치 시 새 데이터셋 삭제 후 실패. |
| `create_table` | 빈 독립 테이블 생성; 요청 필드는 생성 후 추가·검증되며 불일치 시 새 테이블 삭제 후 실패. |
| `create_file_geodatabase` | 새 파일 지오데이터베이스 생성. |
| `truncate_table` | 테이블/피처 클래스의 모든 행 삭제(스키마 유지). |
| `delete_dataset` | 디스크 또는 지오데이터베이스에서 데이터셋 삭제; 되돌릴 수 없음. |
| `export_features` | 선택과 정의 쿼리를 반영해 레이어를 새 데이터셋으로 내보내기. |

### 지오프로세싱(7)

| 도구 | 용도 |
|---|---|
| `run_geoprocessing_tool` | 임의의 arcpy 지오프로세싱 도구 실행 — 분석의 범용 채널; 레이어 이름이 라이브 레이어로 해석되어 선택과 정의 쿼리가 반영됨. 인자 이름을 모르면 먼저 describe_geoprocessing_tool. |
| `list_geoprocessing_tools` | 사용 가능한 지오프로세싱 도구를 이름으로 검색. |
| `list_toolboxes` | arcpy 도구상자 모듈과 프로젝트 내 도구상자 나열. |
| `describe_geoprocessing_tool` | 실행 전에 인자, 데이터 유형, 기본값, 열거형, 사용 설명 조회. |
| `run_python_toolbox_tool` | 디스크의 사용자 .pyt / .atbx / .tbx 도구 실행(네이티브 측은 위치 인자만); 경로 오타 시 "Did you mean …" 제안. |
| `check_extension` | add-in이 확장 라이선스를 검사할 수 있는지 보고(Pro에서는 `inspectable:false` — 라이선스 부재 여부는 도구를 실제 실행해 판단). |
| `get_messages` | 최근 지오프로세싱 작업의 메시지. |

### 심볼(6)

| 도구 | 용도 |
|---|---|
| `set_layer_renderer` | 레이어 심볼 변경: 단일 심볼, 범주별 고유값, 수치 필드 기반 단계 구분/연속 색상. |
| `get_layer_symbology` | 현재 렌더러, 단계 구분 값, 고유값, 라벨 설정 확인. |
| `list_color_ramps` | 프로젝트에서 사용 가능한 색상 램프 나열. |
| `set_layer_labeling` | 라벨 켜기/끄기와 식, 글꼴, 헤일로 설정. |
| `apply_symbology_from_layer` | .lyrx 또는 다른 레이어에서 심볼 복사. |
| `save_layer_file` | 레이어와 심볼을 .lyrx로 저장해 재사용. |

### 지도 뷰(7)

| 도구 | 용도 |
|---|---|
| `get_map_view` | 현재 카메라 위치: 중심, 축척, 회전, 가시 범위; Pro에서 지도 탭이 열려 있어야 함. |
| `set_map_view` | 지도 뷰 이동: 범위, 중심, 축척, 회전 설정. |
| `export_map_view` | 지도 뷰를 PNG로 렌더링해 이미지 반환(육안 확인용); 지도 탭 필요; `fit:"auto"`로 뷰 비율에 맞춤. |
| `list_bookmarks` | 지도의 공간 북마크 나열. |
| `create_bookmark` | 현재 뷰 또는 지정 범위를 이름 있는 북마크로 저장. |
| `apply_bookmark` | 지도 뷰를 북마크로 확대. |
| `delete_bookmark` | 북마크 삭제. |

### 레이아웃과 인쇄(16)

| 도구 | 용도 |
|---|---|
| `list_layouts` | 인쇄 레이아웃을 페이지 크기·요소 수와 함께 나열. |
| `get_layout_info` | 레이아웃 검사: 페이지 설정과 각 요소의 위치·크기. |
| `create_layout` | 새 레이아웃 페이지 생성(기본으로 페이지를 채우는 지도 프레임 포함). |
| `delete_layout` | 프로젝트에서 레이아웃 삭제. |
| `add_map_frame` | 지정 페이지 위치에 지도 프레임 추가. |
| `set_map_frame_extent` | 레이아웃 지도 프레임을 레이어, 범위, 축척에 맞춤. |
| `add_layout_text` | 제목·크레디트 등 텍스트 요소 추가. |
| `add_layout_legend` | 지도 프레임에 연동되는 범례 추가. |
| `add_layout_scale_bar` | 지도 프레임에 연동되는 축척 막대 추가. |
| `add_layout_north_arrow` | 지도 프레임에 연동되는 방위표 추가. |
| `add_layout_picture` | 레이아웃에 이미지(로고 등) 배치. |
| `set_layout_element` | 임의 레이아웃 요소 이동·크기 조정·이름 변경·숨김·텍스트 변경(페이지 좌표는 왼쪽 아래 원점; 범위 밖은 거부). |
| `delete_layout_element` | 레이아웃에서 요소 삭제. |
| `export_layout` | 레이아웃을 PDF / PNG / JPEG / SVG / TIFF로 내보내기; PNG/JPEG는 인라인으로 반환해 확인 가능. |
| `preview_layout` | 레이아웃을 임시 이미지로 렌더링(`output_path` 지정 시에만 파일 기록). 작성 중 확인용. |
| `export_map_series` | 레이아웃의 지도 시리즈(맵북)를 여러 페이지 PDF로 내보내기. |

### 래스터(5)

| 도구 | 용도 |
|---|---|
| `get_raster_info` | 래스터 상세: 밴드, 크기, 셀 크기, 픽셀 유형, 통계, 좌표계. |
| `set_raster_symbology` | 래스터 레이어의 컬러라이저와 색상 램프 설정; 명시적 색상과 레이블 지원. |
| `raster_calculator` | 지도 대수: 래스터를 짧은 별칭에 묶고 Python 식 작성; Spatial Analyst 필요. |
| `sample_raster_values` | 지도 좌표(`points`) 또는 포인트 레이어 피처에서 셀 값 읽기 — 인라인 반환, `write_table=true`가 아니면 부작용 테이블 없음. |
| `zonal_statistics` | 존 폴리곤 내 래스터 값 집계 후 테이블 반환; Spatial Analyst 필요. |

### 카탈로그와 데이터 탐색(6)

| 도구 | 용도 |
|---|---|
| `list_workspace_contents` | 지오데이터베이스 또는 폴더 내 데이터셋 나열; 기본은 프로젝트 기본 지오데이터베이스. |
| `list_folder` | 디스크의 GIS 파일과 하위 폴더 나열; 기본은 프로젝트 홈 폴더. |
| `describe_dataset` | 경로로 임의 데이터셋 설명: 유형, 지오메트리, 좌표계, 범위, 필드, 행 수(지도에 추가 불필요). |
| `search_data` | 프로젝트 지오데이터베이스, 홈 폴더, 폴더 연결에서 이름으로 검색 — 데이터 추가 전 가장 빠른 탐색; 결과 중복 제거됨. |
| `get_project_items` | 프로젝트에 등록된 폴더 연결, 데이터베이스 연결, 도구상자. |
| `add_folder_connection` | 폴더를 프로젝트에 등록해 데이터 탐색을 쉽게. |

### 유틸리티(4)

| 도구 | 용도 |
|---|---|
| `execute_arcpy_code` | ArcGIS Pro 내부에서 Python 실행: arcpy가 이미 임포트되어 있고, arcpy.mp.ArcGISProject('CURRENT')는 열린 프로젝트, 변수는 호출 간 유지, print 출력은 캡처되며 마지막 식 값이 반환됨. 전용 도구가 없을 때 사용; Python 브리지(6511)가 응답. |
| `get_pump_status` | 메인 스레드 디스패처 설치 여부. Python 브리지 미기동 시 구조화된 down 상태(`running:false, bridge_listening:false`, code와 hint 포함)를 반환하고 오류로 처리하지 않음. |
| `stop_pump` | 메인 스레드 디스패처 제거; 라이브 프로젝트 명령은 재설치 전까지 동작하지 않음. |
| `run_batch` | 여러 명령을 한 번의 왕복으로 실행 — 다단계 워크플로가 훨씬 빠름; 이미지 결과는 기본적으로 path/bytes/sha로 축약되고 각 단계의 밀리초도 반환. |

### 템플릿 증류(10)

| 도구 | 용도 |
|---|---|
| `template_list` | 로컬 라이브러리의 레이아웃 템플릿 목록; 상태나 태그로 필터 가능. |
| `template_get` | 템플릿 1개의 전체 spec(레이아웃 기하, 팔레트, 타이포그래피, 구역, 함정, 리뷰 기록) 읽기. |
| `template_import` | 샘플 도면을 초안으로 라이브러리에 가져오기(이미지 sha256으로 중복 제거); 다음 단계는 template_measure. |
| `template_measure` | 샘플 도면 계측 — 도면 경계 상자, 종횡비, 강한 프레임 선, 팔레트, DPI — 을 수동 편집 금지인 measured 블록에 기록. |
| `template_patch` | 의미 필드(구역/범례/타이포그래피/팔레트/함정)를 검증 병합으로 제출; measured 등 보호 필드는 거부. |
| `template_validate` | schema와 기하 일관성 검사(도框/구역이 캔버스 내, 계측 허용 오차 ≤3%, 타이포그래피 순서, 색상, 함정). |
| `template_review` | 검증된 템플릿의 사람 리뷰 결과(리뷰어 + 메모)를 기록. |
| `template_approve` | 리뷰·검증을 마친 템플릿을 ready로 승인; 검증 오류나 리뷰 누락 시 거부(force는 덮어쓰고 기록). |
| `template_preview` | 템플릿 와이어프레임 PNG를 샘플 도면과 나란히 렌더링해 육안 확인에 제공. |
| `template_apply` | ready 템플릿을 제작 컨텍스트 JSON(레이아웃/팔레트/타이포그래피)으로 기록, 어시스턴트가 이를 따라 출도. |
</details>

---

## 📦 패키지 내용물

| 경로 | 내용 | 설명 |
|---|---|---|
| `install\ArcGISProMCP.esriAddinX` | 🧩 add-in v1.1.12 | 125개 MCP 도구 + 모든 GP 도구; HTTP MCP 6520 / 구형 TCP 6510 |
| `install\runtime\distill\` | 🧪 템플릿 증류 | `arcgis-distill` CLI + `arcgis_agentkit` 패키지(Pro 내장 Python 실행); 템플릿 라이브러리는 `%LOCALAPPDATA%\ArcGIS Pro MCP\library`(업그레이드 시 유지) |
| `install\runtime\stdio-bridge.ps1` | 🌉 stdio 브리지 | 로컬 프로세스만 실행할 수 있는 클라이언트(Claude Desktop 등)를 6520에 연결; 순수 PowerShell, Python 불필요 |
| `install\runtime\runcode\RunCode.pyt` | 🐍 RunCode 도구상자 | 임의 arcpy를 자주 실행할 때 사용; 첫 실행 시 신뢰 확인 1회 |
| `install\runtime\guard\` | 🛡️ 다이얼로그 가드 | "서드파티 코드 라이브러리 실행" 신뢰 대화상자를 자동 클릭; 실패 시 최상위 알림(30초 이상·링) |
| `install\runtime\plugin\` | 🔌 Python 브리지(선택) | Pro의 Python 창에서 시작해 `execute_arcpy_code` 지속 세션 제공(포트 6511) |
| `install\*.cmd / *.ps1` | 🛠️ 설치와 자가 검사 | 설치 / 검사 / Claude 설정 / 다이얼로그 가드 / 제거 |
| `clients\` | 📁 클라이언트 연결 안내 | Claude Code, Claude Desktop, Codex, 범용 JSON |
| `docs\` | 📖 사용 가이드, 문제 해결, 작업 규칙, 변경 로그 | 먼저 사용 가이드 |
| `source\` | 📜 전체 소스 스냅샷 | 이 바이너리에 대응하는 소스(AGPL-3.0 준수); 업스트림 LICENSE / NOTICE와 빌드 산출물 `source\dist\` 포함 |

### 🔍 의존성 감사("전부 포함"의 근거)

- 🧩 add-in: 자체 완결형 C# 어셈블리. Pro 3.7 지원 환경에 포함된 .NET Desktop Runtime 10 x64에서 동작 — .NET 별도 설치 불필요.
- 🌉 stdio 브리지, 🛡️ 다이얼로그 가드: Windows 기본 제공 PowerShell 5.1 + WinForms — 별도 설치 불필요.
- 🐍 RunCode 도구상자, Python 브리지: ArcGIS Pro 자체 Python(arcpy)에서 동작 — 별도 Python 불필요.
- ✅ **따라서 이 패키지에는 pip / 인터넷이 필요한 외부 의존성이 없습니다.** "오프라인 동작"은 이 패키지 자체의 설치·실행 경로를 뜻하며, ArcGIS Pro 3.7의 전제(.NET Desktop Runtime 10 x64, WebView2 ≥132)는 Pro 설치 프로그램이 제공합니다.

---

## 🧪 선택 구성 요소 사용법

**🐍 RunCode 도구상자**(arcpy 다용도)
- 경로: `%LOCALAPPDATA%\ArcGIS Pro MCP\runcode\RunCode.pyt`
- AI가 `run_python_toolbox_tool`을 호출할 때 이 경로를 전달하면 됩니다; Pro의 "삽입 → 도구상자 → 도구상자 추가"로 직접 추가해도 됩니다.
- 각 Pro 세션 첫 실행 시 "서드파티 코드 라이브러리" 신뢰 대화상자가 뜹니다: 먼저 `03-dialog-guard.cmd`를 실행하면 가드가 "예"를 자동 클릭 ✨.

**🔌 Python 브리지**(arcpy 세션을 호출 간 유지해야 할 때)
1. ArcGIS Pro 메뉴 **분석 → Python**으로 Python 창 열기;
2. 붙여넣고 실행(기본 설치 위치 기준 경로):
   ```python
   import sys; sys.path.insert(0, r"C:\Users\<사용자 이름>\AppData\Local\ArcGIS Pro MCP\plugin")
   import mcp_bridge; print(mcp_bridge.start_server(port=6511))
   ```
3. `CURRENT`(현재 프로젝트)가 필요하면 한 줄 더(메인 스레드에서 실행):
   ```python
   print(mcp_bridge.start_pump())
   ```

**🛡️ 다이얼로그 가드**(RunCode / ENVI→Pro 연동 흐름 전 권장)
```
03-dialog-guard.cmd                 # 600초 가드
03-dialog-guard.cmd -DurationSeconds 120
03-dialog-guard.cmd -NotifyTest     # 알림 창 테스트(30초)
03-dialog-guard.cmd -AlertOnly      # 알림만, 자동 클릭 안 함
```
안전 경계: "서드파티 / 코드 라이브러리 / third-party / code library" 문맥의 신뢰 대화상자만 처리; **덮어쓰기·삭제 등 다른 확인 대화상자는 절대 건드리지 않습니다** 🙅. 로그: `%LOCALAPPDATA%\ArcGIS Pro MCP\guard\watch_pro_dialog.log`.

---

## 🔒 보안 안내

- 🏠 add-in과 모든 브리지 서비스는 `127.0.0.1`만 리스닝하며 LAN의 다른 컴퓨터는 연결할 수 없습니다.
- ⚖️ 본 소프트웨어는 이 컴퓨터에만 `127.0.0.1`로 서비스를 제공하고 제3자에게 네트워크 서비스를 제공하지 않습니다. 수정 후 네트워크로 타인에게 제공한다면 AGPL-3.0 §13의 소스 제공 의무가 적용됩니다.
- 🔏 add-in은 미서명 빌드입니다: 첫 로드 시 ArcGIS Pro가 신뢰 확인을 요구하며 이는 정상 흐름입니다; 이 패키지의 `SHA256SUMS.txt`로 대조하세요.
- 🕵️ 이 패키지에는 텔레메트리나 네트워크 보고가 전혀 없습니다.
- 🧹 제거: `99-uninstall.cmd`(클라이언트 설정 유지 선택 가능).

---

## 📜 버전과 라이선스

- 🏷️ 버전: **1.1.12**(Customer 1.1.12 · 1.14 개선판 + 템플릿 증류; 빌드 날짜는 `VERSION.json`, 도구 수 125)
- 👤 관리자: **Law**([@XNYGggg](https://github.com/XNYGggg))
- 🙏 업스트림 프로젝트와 원저자: **[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)**, 저자 **Pisut Nakmuenwai**(AGPL-3.0-or-later). 본 패키지는 1.1.12까지 로컬 패치를 누적했습니다(변경점은 `docs\CHANGELOG.md`와 `RELEASE-NOTES.md`).
- ⚖️ 라이선스: **AGPL-3.0-or-later**(전체 소스 스냅샷과 업스트림 NOTICE 포함)
- 🏷️ 상표: "ArcGIS"는 Esri의 상표입니다; 본 프로젝트는 제3자 도구이며 Esri와 제휴 관계가 없습니다.

🆘 자주 묻는 질문은 `docs\排障手册.md`, 고급 사용법은 `docs\使用指南.md` 참조. 즐거운 지도 제작 되세요 🎨 (｀・ω・´)ゞ

---

<!-- LDU-THANKS -->

<h2 align="center">🙏 감사의 글</h2>

<p align="center">
  본 프로젝트의 연구 개발은 <strong>루둥대학(Ludong University, 鲁东大学)</strong>의 지원을 받았으며, 이에 깊이 감사드립니다.
</p>

<p align="center">
  <a href="https://www.ldu.edu.cn/"><img src="assets/ldu-emblem.png" width="104" alt="루둥대학 교장"></a>
</p>

<h3 align="center">鲁东大学 · Ludong University</h3>

> 루둥대학은 1930년에 설립되어 산둥성 옌타이에 위치한 성립 종합대학으로, 응용형 인재 양성을 주도로 공학·농학·이학·문학이 조화롭게 발전하고 있습니다. 현재 26개 단과대학을 두고 있으며 공학·화학·재료과학 등 6개 학과가 ESI 세계 순위 상위 1%에 진입했고, "후덕·박학·일신·독행(厚德、博学、日新、笃行)"의 교훈을 이어가고 있습니다.

<p align="center">
  📍 중국 산둥성 옌타이시 즈푸구 훙치중로 186호(우편번호 264025)<br>
  🌐 <a href="https://www.ldu.edu.cn/">대학 공식 사이트</a> ·
  <a href="https://jwc.ldu.edu.cn/">교무처</a> ·
  <a href="https://lib.ldu.edu.cn/">도서관</a> ·
  <a href="https://ehall.ldu.edu.cn/">온라인 서비스</a>
</p>
