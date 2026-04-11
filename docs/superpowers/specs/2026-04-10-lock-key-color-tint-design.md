# Lock/Key 쌍 색상 틴트 구분

## 목적

Lock/Key 방을 여러 쌍 배치했을 때 어떤 잠금방이 어떤 열쇠방과 연결되어 있는지 시각적으로 구분할 수 없는 문제를 해결한다. 각 쌍에 고유한 색상 틴트를 적용하여 한눈에 쌍을 식별할 수 있게 한다.

## 설계

### 색상 생성: HSV 황금비 방식

쌍 개수에 제한이 없으므로, 황금비(0.618...)를 이용하여 색상환에서 균등하게 분포된 색을 자동 생성한다.

```
hue = (pairIndex * 0.618033988749895) % 1.0
saturation = 0.5  (원본 스프라이트가 보이도록 적당한 채도)
value = 1.0       (밝기 유지)
```

`Color.HSVToRGB(hue, saturation, value)`로 Unity Color 변환.

### 적용 시점

Key 방 배치 완료 시 (= `LinkLockAndKeyRooms()` 호출 시점) 즉시 두 방 모두에 틴트 적용.

### 수정 파일

#### 1. `Assets/Scripts/Room/RoomManager.cs`

- `lockKeyPairCount` 필드 추가 (int, 쌍 인덱스 카운터)
- `GeneratePairColor(int pairIndex)` 메서드 추가 (HSV 황금비 색상 생성)
- `LinkLockAndKeyRooms()` 수정: 연결 시 색상 생성 후 두 방에 적용
- `ResetAllRooms()` 시 `lockKeyPairCount = 0` 초기화

#### 2. `Assets/Scripts/Room/Room.cs`

- `pairColor` 필드 추가 (Color, 기본값 White)
- `SetPairColor(Color color)` 메서드 추가: `spriteRenderer.color = color` 설정
- `ClearPairColor()` 메서드 추가: `spriteRenderer.color = Color.white` 복원
- `UnlockRoom()` 수정: 스프라이트 변경 시 `spriteRenderer.color = Color.white`로 틴트 제거
- `RestoreLockKeySprite()` 수정: 복원 시 `pairColor`로 틴트 재적용

#### 3. `Assets/Scripts/Champion/ChampionPathfinder.cs`

- 변경 불필요. `UnlockRoom()`에서 틴트가 자동 제거되고, `RestoreLockKeySprite()`에서 자동 재적용됨.

### 색상 흐름

```
[준비 페이즈]
  Lock 배치 → Key 배치 → LinkLockAndKeyRooms() → 두 방에 색상 틴트 적용

[탐험 페이즈]
  Key 방 진입 → UnlockRoom() → 스프라이트를 Battle로 변경 + 틴트 제거 (Color.white)
  
[용사 사망 → 복원]
  RestoreLockKeySprite() → 원래 스프라이트 복원 + pairColor 틴트 재적용
```

### 엣지 케이스

- Lock/Key 없는 던전: pairCount = 0, 아무 영향 없음
- 방 초기화(ResetAllRooms): pairCount 리셋, 모든 색상 자연스럽게 제거
- 세이브/로드: Lock/Key 연결 복원 시 색상도 재생성 (pairIndex 순서대로)

## 검증 방법

1. Lock/Key 1쌍 배치 → 두 방이 같은 색인지 확인
2. Lock/Key 3쌍 배치 → 각 쌍이 서로 다른 색인지 확인
3. 탐험 시작 → Key 방 진입 시 틴트가 제거되고 Battle 스프라이트로 변하는지 확인
4. 용사 사망 → Lock/Key 방이 원래 스프라이트 + 틴트 색으로 복원되는지 확인
