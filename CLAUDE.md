# ProjectSH AI 가이드라인

## 환경 및 경로
- 게임 엔진: Unity 2022.3.62f2 (LTS) / URP 14.0.12
- `PROJECT_ROOT`: `C:\Projects\ProjectSH`
- `TODO_DIR`: `D:\Obsidian_Vaults\Vault_GameDev\ProjectSH\`
- `LOG_DIR`: `D:\Obsidian_Vaults\Vault_GameDev\ProjectSH\Logs\`
- `REPORTS_DIR`: `D:\Obsidian_Vaults\Vault_GameDev\Reports\`
- 지식 베이스(Obsidian): `D:\Obsidian_Vaults\Vault_GameDev\ProjectSH`

## 행동 원칙
- **추측 금지**: 혼란스러우면 멈추고 질문. 에러 시 임의 해결 금지 → 콘솔 스택 트레이스 요청 후 분석.
- **단순함 우선**: 요청 기능 외 추가 없음. 200줄→50줄 가능하면 다시 써라.
- **범위 준수**: 지시받은 파일·범위만 수정. 멀쩡한 코드 리팩토링 금지. 미사용 코드 발견 시 삭제 말고 알릴 것.
- **실행**: 다단계 작업은 계획 먼저 (`plan-mode`). 각 단계 완료 직후 ToDoList 갱신. 단계 끝나면 결과 보고 후 대기 — 다음 단계 임의 제안·실행 금지.

## Unity 작업 규칙
- **UnityMCP 필수**: 씬·컴포넌트·에셋 상태 반드시 UnityMCP 툴로 확인, 추측 금지.
- **스레드 안전성**: Unity API 메인 스레드에서만 호출.
- **에셋 수정 금지**: FBX·텍스처·오디오 등 바이너리 에셋 스캔·수정 금지.
- **validate_script 오탐 주의**: "Duplicate method signature"는 false positive 가능 — 실제 컴파일 오류는 `read_console`로 확인.
- **execute_code CodeDom 제한**: 기본 컴파일러는 C# 6 — 로컬 함수 사용 불가. System.Text.StringBuilder + 인라인 문자열 조합으로 대체.
- **씬 런타임 버그 진단**: private 필드 확인은 `System.Reflection`으로, 위치 이상은 `transform.localPosition` vs `transform.position` 비교로 원인 좁히기.
- **UI 작업 순서**: SerializeField stub 먼저 추가 → prefab에서 GameObject 레퍼런스 연결 → 구현. 구현 전 연결 요청 대기.
- **meta 파일**: 파일·폴더 이동·이름 변경 시 반드시 `.meta`도 함께 이동. Unity Editor 밖에서 변경 시 필수.

## 프로젝트 레이아웃
- `Assets/` — 모든 게임 콘텐츠. C# 스크립트, 씬, 프리팹, 에셋 배치.
- `Packages/manifest.json` — Unity 패키지 의존성. 직접 csproj 편집 금지.
- `ProjectSettings/` — 엔진 설정 (입력, 물리, 태그, 품질, 그래픽).
- `Assembly-CSharp.csproj` / `.sln` — Unity 자동 생성. 직접 편집 금지.
- `Library/`, `Logs/`, `UserSettings/` — 로컬 캐시. 커밋 금지.

## 아키텍처 참조
- 신규 모듈 기획·모듈 간 의존성 탐색 시에만 로드:
  `D:\Obsidian_Vaults\Vault_GameDev\ProjectSH\ProjectSH_Architecture.md`
- 단순 버그 수정·단일 스크립트 작업 시 로드 금지.

## 워크플로우 (스킬 자동 트리거)
- 업무 시작/종료 → `work-start` / `work-end`
- 기능 개발·버그 수정 계획 → `plan-mode`
- C# 스크립트 작성·리팩토링 → `unity_csharp_generator`
- 작업 리포트·진행 현황 등 HTML 리포트 생성 → `report-generator`

## 컨텍스트 관리
- 사용자가 "계속해", "계속", "이어서", "재개", "계속해줘" 입력 시:
  1. `/compact` 실행 요청
  2. 실행 후 이어서 진행 (건너뛰길 원하면 그냥 진행)

## /caveman-commit 규칙
- **순서**: ① `git status --short` 실행 → ② 수정된 파일 중 `*.cs`, `*.prefab`, `*.unity`, `*.asset`만 선별 → ③ 전부 `git add`
- `Library/`, `UserSettings/`, `Logs/` 제외.
- stage 대상 없으면 알리고 중단.
- **커밋 메시지 언어**: 한글 작성. type/scope 영문 유지 (`feat(player): ...`), 요약·본문 한글.

## 구현 팁
- **코딩 컨벤션**: 신규 코드 컨벤션 준수. 기존 코드 컨벤션 맞춰 수정 금지. C# 작성·리팩토링 시 `D:\Obsidian_Vaults\Vault_GameDev\Skills\projectsh_csharp_syntax.md`(ProjectSH 전용 컨벤션) 숙지 후 진행. ※ BoxingStar 공유 `unity_csharp_syntax.md`와 별개 — 혼동·편집 금지.
- **Edit 툴 탭/스페이스 불일치**: `old_string` 매칭 실패 원인 1위 — Read 후 실제 들여쓰기 문자 확인 필수.
- 대규모·반복 구현은 `Agent` 툴 + `model:"haiku"`로 비용 절감.
- **Write 툴 신규 파일 생성 실패**: 존재하지 않는 경로에도 Write 툴이 "File has not been read yet" 에러 → 신규 파일은 PowerShell `Out-File -Encoding utf8` 사용.
- **WorkLog 디렉토리**: `LOG_DIR` 경로 없을 수 있음 → 쓰기 전 `Test-Path` 확인 후 `New-Item -ItemType Directory` 생성.
- **테스트**: `com.unity.test-framework` 설치됨. 테스트는 Unity Editor Test Runner (Window > General > Test Runner) 사용. 테스트 asmdef는 `Tests/` 폴더에 생성.
