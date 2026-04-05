---
name: dungeon-mental-model
description: 던전 메이커 프로젝트에서 새 기능을 구현하거나 시스템을 설계할 때 사용하는 아키텍처 멘탈 모델. "기능 추가", "시스템 구현", "어떻게 만들까", "구현해줘", "새로운 방 타입", "몬스터 추가", "스킬 구현", "AI 행동", "전투 로직", "UI 구현", "이벤트 연결", "데이터 설계" 등 던전 메이커 게임의 기능 개발과 관련된 모든 요청에서 반드시 이 스킬을 사용하라. 설계 방향을 잡거나 코드를 작성하기 전에 항상 이 멘탈 모델을 먼저 참고하라.
---

# 던전 메이커 아키텍처 멘탈 모델

이 스킬은 던전 메이커(2D 턴제 디펜스 게임) 프로젝트에서 기능을 구현할 때 **어떤 패턴으로, 어떤 계층에, 어떤 구조로** 만들어야 하는지 판단하는 가이드다.

코드를 작성하기 전에 이 멘탈 모델을 거쳐야 한다. 왜냐하면 이 프로젝트는 일관된 아키텍처를 유지하면서 확장 가능한 게임을 만드는 것이 목표이기 때문이다.

## 핵심 게임 루프

모든 기능은 이 루프의 어딘가에 위치한다. 새 기능을 구현할 때 먼저 이 루프에서 어느 페이즈에 속하는지 파악하라.

```
[준비 페이즈] → [실행 페이즈] → [정산 페이즈] → (반복)
  방 배치          용사 탐험        보상/결과
  몬스터 배치      전투 발생        재화 획득
  자원 관리        AI 행동         진행도 갱신
```

## 3계층 아키텍처

모든 코드는 이 3계층 중 하나에 속해야 한다. 계층을 섞지 마라.

| 계층 | 역할 | 예시 | 위치 |
|------|------|------|------|
| **Data** | 수치와 상태만 | ScriptableObject, SaveState | `Assets/Scripts/Data/` |
| **Logic** | 규칙과 흐름 | Manager, System, Controller | `Assets/Scripts/{시스템별}/` |
| **View** | 보여주는 것만 | UI, 애니메이션, VFX, 사운드 | `Assets/Scripts/UI/` |

**통신 규칙:**
- 수직(위→아래): 직접 참조 OK
- 수평(같은 레벨): 반드시 이벤트 채널로
- View→Logic: 절대 직접 호출 금지, 이벤트 구독만

## 기능 구현 의사결정 플로우

새 기능 요구사항이 왔을 때, 아래 질문을 순서대로 거쳐라:

### 1. "이것은 데이터인가?"
수치, 속성, 설정값이라면 → **ScriptableObject로 정의**

새 몬스터, 새 스킬, 새 방 타입 등은 코드가 아니라 ScriptableObject 에셋으로 추가한다. 코드 수정 없이 밸런싱이 가능해야 하기 때문이다.

```
ScriptableObject 계층:
  EntityData → MonsterData / ChampionData / BossData
  SkillData → 데미지 / 힐 / 버프·디버프
  RoomData → 전투방 / 함정방 / 특수방
```

### 2. "이것은 상태 전환인가?"
페이즈, 턴, 모드 변경이라면 → **State Machine으로 관리**

턴제 게임은 본질적으로 "지금 누구의 차례인가"를 추적하는 상태 머신이다.

```
GameState: Lobby → Preparation → Exploration → GameOver
BattleState: Setup → PlayerTurn → EnemyTurn → Result
```

각 State는 `Enter()`, `Execute()`, `Exit()` 인터페이스를 구현한다.

### 3. "이것은 행동(액션)인가?"
공격, 이동, 스킬 사용이라면 → **Command Pattern으로 구현**

행동을 객체화하면 플레이어 입력과 AI 모두 동일한 Command로 처리할 수 있고, Undo/Redo와 전투 리플레이가 가능해진다.

```csharp
ICommand {
    Execute()    // 실행
    Undo()       // 되돌리기
    GetPreview() // 미리보기
}
```

### 4. "여러 시스템이 반응해야 하는가?"
데미지 발생 시 UI도 갱신, VFX도 재생, 통계도 기록해야 한다면 → **이벤트 채널로 통신**

```
[전투] ── OnDamageDealt ──→ [UI] 데미지 표시
                          ├→ [VFX] 이펙트 재생
                          ├→ [Audio] 타격음
                          └→ [통계] 데이터 기록
```

**이벤트 방식 선택:**
- 동일 시스템 내: C# delegate/event (성능 최고)
- 시스템 간 통신: ScriptableObject 이벤트 채널 (Inspector 설정 가능)
- 단순 UI 바인딩: UnityEvent

### 5. "AI 행동의 변형인가?"
용사마다 다른 길찾기, 다른 타겟 선택이라면 → **Strategy Pattern으로 교체 가능하게**

```
IPathfindingStrategy: BFS / A* / Random
ITargetingStrategy: Closest / Weakest / HighestThreat
```

if-else 분기 대신 전략 객체를 교체하면 새 AI 추가가 독립적이다.

### 6. "빈번한 생성/파괴가 있는가?"
이펙트, 데미지 팝업, 투사체 등 → **Object Pool 사용**

## 파일 배치 가이드

새 스크립트를 만들 때 이 구조를 따라라:

```
Assets/Scripts/
├── Core/           # GameManager, GameStateManager, SaveSystem
├── Data/           # ScriptableObject 정의
├── Events/         # 이벤트 채널
├── Battle/         # 전투 시스템 (Commands/, States/, Entities/)
├── Champion/       # 용사 시스템
├── Dungeon/        # 던전 시스템 (Grid/, Room/)
├── AI/             # AI 전략, 길찾기
├── Economy/        # 경제 시스템
├── UI/             # UI (이벤트 구독만)
└── Utils/          # Object Pool 등 유틸리티
```

## 구현 시 체크리스트

기능 구현 전에 이 항목들을 확인하라:

- [ ] 이 기능은 게임 루프의 어느 페이즈에 속하는가?
- [ ] 3계층 중 어디에 해당하는가? (Data / Logic / View)
- [ ] 위 6가지 패턴 중 어떤 것이 적합한가?
- [ ] 기존 시스템과의 통신은 이벤트로 하고 있는가?
- [ ] 새 콘텐츠(몬스터, 스킬, 방)는 ScriptableObject로 정의했는가?
- [ ] 하드코딩된 수치 없이 데이터 계층에서 참조하고 있는가?

## 안티패턴 경고

이것들은 하지 마라:

- **재미 검증 전에 과도한 아키텍처 투자**: 최소 프로토타입으로 먼저 테스트
- **YAGNI 위반**: "나중에 필요할 것 같아서" 미리 구현하지 않기
- **싱글톤 남용**: 이벤트 채널로 대체할 수 있으면 대체하라
- **UI에서 로직 직접 호출**: 반드시 이벤트 구독 패턴을 사용
- **View 계층에 게임 규칙 배치**: 데미지 계산은 Logic 계층에서
