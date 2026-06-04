---
name: obsidian_md_generator
description: 옵시디언(Obsidian)용 마크다운(.md) 문서 신규 생성 또는 기존 문서 옵시디언 포맷으로 정리/수정 요청 시 사용. 기획서, QA 리포트, 회의록 등 세컨드 브레인 저장 시 트리거.
---

# 옵시디언 마크다운 문서 생성 SOP

1. **옵시디언 포맷 규칙 숙지 (필수):**
   문서 작성 전 반드시 `D:\Obsidian_Vaults\Vault_GameDev\Skills\obsidian_rules.md` 읽고 옵시디언 전용 마크다운 규칙(Properties, 위키 링크 등) 숙지.

2. **컨텍스트 및 연결성 파악:**
   - 새 문서와 볼트 내 기존 문서 연관성 파악, 필요 시 적절한 태그·위키 링크 구조 계획.
   - 필요 시 사용자가 지시한 로우 데이터(Raw Data) 파일 먼저 읽기.

3. **문서 생성 및 검증:**
   - 옵시디언 규칙에 따라 문서 작성. 최상단 Properties(YAML) 블록 포함 여부 자체 검증.
   - 옵시디언 볼트(`D:\Obsidian_Vaults\Vault_GameDev\...`) 내 지정 폴더 경로에 `.md` 확장자로 저장.
