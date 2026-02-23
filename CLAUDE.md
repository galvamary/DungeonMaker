# CLAUDE.md

이 파일은 Claude Code (claude.ai/code)가 이 저장소의 코드를 다룰 때 참고하는 가이드입니다.

## 프로젝트 개요

Unity 2D 게임 프로젝트 "던전 메이커"로, 플레이어가 마왕이 되어 던전을 설계하고 용사를 물리치며 재화를 모으는 게임입니다. AI 생성 코드를 활용하여 Unity 게임 개발을 단계별로 학습하기 위한 프로젝트입니다.

## 주요 개발 명령어

### Unity 관련 작업
- 스크립트 생성: `Assets/Scripts/` 디렉토리에 `.cs` 파일 생성
- 프리팹 생성: 스크립트에서 참조하거나 `Assets/Prefabs/`에 저장
- 씬 수정: `Assets/Scenes/MainScene.unity` (메인 씬) 편집

### 테스트
- Unity 에디터의 Play 모드를 통해 테스트 진행
- 커맨드라인 테스트 러너는 설정되어 있지 않음

## 상위 아키텍처

### 프로젝트 구조
- **Assets/**: 모든 게임 에셋의 메인 개발 디렉토리
  - `Scenes/`: MainScene.unity (단일 메인 씬)
  - `Scripts/`: 시스템별로 정리된 ~45개의 C# 파일:
    - `Battle/`: BattleManager, BattleSetup, BattleTurnSystem, ChampionAI, MonsterController, BattleEntity, UI 패널
    - `Champion/`: Champion, ChampionSpawner, ChampionPathfinder
    - `Camera/`: CameraController
    - `Grid/`: GridManager, GridClickHandler, GridVisualizer
    - `Room/`: RoomManager, Room 타입들
    - `Shop/`: ShopManager, ShopUI
    - `Data/`: ScriptableObject (MonsterData, ChampionData, SkillData)
    - `UI/`: CurrencyUI, VictoryUI, DefeatUI, GameOverUI, WarningMessageUI, SettingsUI, FadeEffect
    - 루트: GameManager, GameStateManager, AudioManager, SaveState
  - `Prefabs/`: 5개의 UI 프리팹 (MonsterInventoryItem, ShopMonsterPanel, SkillButtonPrefab 등)
  - `Animation/`: 스프라이트 애니메이션 (Fire, Heal, Slash)

### 게임 시스템 (구현 완료)
1. **던전 건설 시스템**: 그리드 기반 방 배치 (입구, 전투, 보물, 보스 방)
2. **몬스터 관리**: 구매, 배치, 드래그 앤 드롭, 전투 시스템
3. **용사 AI**: BFS 기반 길찾기와 피로도 시스템을 활용한 자율 탐험
4. **전투 시스템**: 스킬 선택, 타겟팅, HP/MP 관리가 포함된 턴제 전투
5. **경제 시스템**: 골드와 명성 재화, 이벤트 기반 UI 업데이트
6. **UI 시스템**: 재화 표시, 상점, 몬스터 인벤토리, 설정, 승리/패배 화면, 경고 메시지

### 아키텍처 패턴
- **싱글톤**: GameManager, BattleManager, AudioManager로 전역 상태 관리
- **ScriptableObject**: MonsterData, ChampionData, SkillData로 데이터 정의
- **델리게이트 이벤트**: OnGoldChanged, OnReputationChanged, OnGameOver로 시스템 간 느슨한 결합
- **영속성**: PlayerPrefs 기반 저장/불러오기 (SaveState 직렬화)
- **게임 페이즈**: 준비 → 탐험 → 전투 (GameStateManager가 관리)

### Unity 설정
- Unity 버전: Unity 6 (6000.2.7f2)
- 렌더 파이프라인: Universal Render Pipeline (URP)
- 타일맵 지원이 활성화된 2D 프로젝트
- 주요 패키지: 2D 기능, Visual Scripting, Timeline, UGUI, TextMesh Pro

## 구현 가이드라인

기능 구현 시:
1. `Assets/Scripts/` 하위의 적절한 디렉토리에 스크립트 생성
2. Unity 네이밍 컨벤션 준수 (클래스는 PascalCase, 메서드는 camelCase)
3. Unity 내장 시스템 활용 (MonoBehaviour, ScriptableObject 등)
4. Unity UI 시스템으로 UI 구현 (Canvas, UI 요소)
5. 충돌 감지에 Unity 2D 물리 사용
6. 새 매니저 클래스는 기존 싱글톤 패턴 따르기
7. 새 데이터 정의에는 ScriptableObject 사용
8. 시스템 간 통신에 델리게이트 이벤트 사용

## 남은 구현 과제

- 보스 방 고유 메커니즘 및 보스 몬스터 타입
- 버프/디버프 스킬 메커니즘 (SkillData에 프레임워크 존재)
- 다중 용사 지원
- UI 개선 및 애니메이션
- 모든 액션에 대한 효과음
- 스킬 시각 효과 (프레임워크 존재)
