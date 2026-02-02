# 로비 씬 사용법

## 씬 구성

- **Assets/Scenes/Lobby.unity**: 로비 메인 씬
- **Build Settings**: Lobby가 첫 번째 씬으로 등록됨 (플레이 시 로비부터 시작)

## 씬 내용

- **Main Camera**: 배경색 다크 퍼플 (로비 톤)
- **Global Light 2D**: 2D 라이트
- **LobbyCanvas**: UI Canvas + **LobbyUI** 스크립트
  - 실행 시 EventSystem이 없으면 자동 생성
  - Awake에서 CanvasScaler·GraphicRaycaster 설정 후 로비 UI 전부 생성

## UI 구성 (런타임 생성)

- **상단**: C(동전), 💎(보석), ▲(파워스톤), 설정 버튼
- **중앙**: THE TOWER 타이틀, 육각형 placeholder, Total Coin Bonus, Difficulty(Tier ±), Highest Wave, C x배수, **BATTLE** 버튼
- **하단**: 네비 (배틀, 워크샵, 컨테이너, 연구, 상점)

## 데이터

- **GameData** (Scripts/Game/GameData.cs): Coins, Gems, PowerStones, CurrentTier, HighestWave, 티어별 코인 배수

## BATTLE 버튼

- **Battle** 씬이 Build Settings에 있으면 해당 씬 로드
- 없으면 콘솔에 안내 메시지 출력 (Battle 씬을 만들고 Build Settings에 추가하면 됨)

## LobbyCanvas에 스크립트가 안 붙어 있을 때

Unity가 스크립트 GUID를 바꾼 경우 **LobbyCanvas**에 **LobbyUI** (Scripts/UI/LobbyUI.cs) 컴포넌트를 수동으로 추가하면 됩니다.
