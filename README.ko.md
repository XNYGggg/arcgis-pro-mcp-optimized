[![English](https://img.shields.io/badge/Language-English-1f6feb?style=flat-square)](README.md) [![简体中文](https://img.shields.io/badge/Language-%E7%AE%80%E4%BD%93%E4%B8%AD%E6%96%87-d1242f?style=flat-square)](README.zh-CN.md) [![日本語](https://img.shields.io/badge/Language-%E6%97%A5%E6%9C%AC%E8%AA%9E-bc002d?style=flat-square)](README.ja.md) [![한국어](https://img.shields.io/badge/Language-%ED%95%9C%EA%B5%AD%EC%96%B4-0047a0?style=flat-square)](README.ko.md) [![License: AGPL v3+](https://img.shields.io/badge/License-AGPL--3.0--or--later-blue?style=flat-square)](LICENSE) [![ArcGIS Pro](https://img.shields.io/badge/ArcGIS%20Pro-3.7%2B-00a1e0?style=flat-square)](https://www.esri.com/en-us/arcgis/products/arcgis-pro/overview) [![Platform](https://img.shields.io/badge/Platform-Windows%2011-lightgrey?style=flat-square)]()

# 🗺️ ArcGIS Pro MCP · AgentKit ⚡

> 🚧 **작성 중이며, 다운로드는 제공되지 않습니다.**
>
> 🚧 **Under development — not open for download.**

이 브랜치는 ArcGIS Pro MCP 2.x 개발 버전(2.0.2 개발 라인)입니다. 기능과 문서가 아직 작성 중이며 **어떤 다운로드도 제공하지 않습니다**: Release 자산, 설치 프로그램, NuGet 패키지가 모두 없습니다.

This branch holds the in-progress ArcGIS Pro MCP 2.x line (2.0.2). The feature set and documentation are still being written, and **nothing is available for download** here — no release assets, no installers, no packages.

- 개발 진행 상황은 [CHANGELOG.md](CHANGELOG.md)와 커밋 기록을 참조하세요. 완료되면 별도로 공지합니다.
- Progress is tracked in [CHANGELOG.md](CHANGELOG.md) and the commit history. A separate announcement will be made once this version is finished.

---

## 🧩 세 모듈 요약 / Three modules at a glance

> 2.x의 정식 구성은 "핵심 MCP + 온디맨드 Guard / Distill"입니다. 우선 요약판을 싣고, 전체 문서는 정식 릴리스와 함께 제공합니다. / 2.x settles on a core MCP plus on-demand Guard / Distill; full docs ship with the release.

| 모듈 / Module | 역할 / What it does | 사용 방법 / How to use | 예시 / Example |
|---|---|---|---|
| 🧩 MCP | 125개 기본 도구 + RunCode / Python 브리지 / 125 native tools plus RunCode and the Python bridge | add-in 설치 후 클라이언트를 `127.0.0.1:6520/mcp`에 연결 / install the add-in, connect to `127.0.0.1:6520/mcp` | "`counties`를 `AREA`로 5개 클래스로 나누고 A3 PDF를 `D:\out\`에 내보내 줘" |
| 🛡️ Guard | 서드파티 신뢰 대화 상자만 클릭 / clicks only the third-party trust dialog | `guard_summon`으로 소환, `guard_status`로 해산 확인 / summon with `guard_summon`, verify with `guard_status` | "RunCode 전에 가드를 소환하고, 끝나면 해산 확인" |
| 🧪 Distill | 샘플 도면 → 재사용 가능한 레이아웃 템플릿 / sample figure → reusable layout template | 10개 `template_*`: import → measure → patch → validate → preview → review → approve → apply | "`fig3.png`를 템플릿으로 증류하고, 미리보기 확인 후 approve" |

**프롬프트 4원칙 / Four prompt rules**: 모듈 지정, 대상 구체화, 출력 확정, 확인 지점 마련. / Name the module, specify the objects, pin the output, and build in a checkpoint.

---

업스트림 프로젝트: [Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)(원저자 Pisut Nakmuenwai, AGPL-3.0-or-later). 저작자 표시와 라이선스: [NOTICE](NOTICE), [LICENSE](LICENSE).

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
