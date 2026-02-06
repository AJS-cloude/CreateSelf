# 워크샵 업그레이드 항목 — 아이템 프리팹용 자료

> 각 행 = 업그레이드 UI **아이템 1개**. `UPGRADESITEM` 프리팹 인스턴스로 탭 그리드에 넣을 때 참고.

**프리팹 1개**로 모든 항목을 쓸 경우: 같은 프리팹을 복제한 뒤, **버튼 이름(Button/ID)** 만 아래 표에 맞춰 넣으면 `BattleUI.BindUpgradeGrid`에서 자동 인식합니다.

---

## 공통: 아이템 UI 구조

| 요소 | 용도 |
|------|------|
| **Text [0]** | 표시명 (예: 데미지, 체력) |
| **Text [1]** | 현재 값 (예: 10, 100, x1.25) |
| **Text [2]** | 비용 (예: $50, C 100) |
| **Button** | 구매 버튼. **이름 = 아래 "버튼/ID 이름"** 으로 설정 |

---

## 3.1 공격 업그레이드 (Attack)

| # | 표시명 | 버튼/ID 이름 | Max Lv | 설명 |
|---|--------|----------------|--------|------|
| 1 | 데미지 | `BuyDamage` | 6000 | 기본 피해량. 초반 우선, 후반은 무료 공격업그레이드에 의존 |
| 2 | 공격속도 | `BuyAttackSpeed` | 99 | 발사 속도 |
| 3 | 치명타율 | `BuyCriticalChance` | 79 | 치명타 확률 (max 시 약 80%) |
| 4 | 치명타 계수 | `BuyCriticalMultiplier` | 150 | 치명타 피해 배율 |
| 5 | 범위 | `BuyRange` | 200 | 타워 사거리 |
| 6 | 데미지/미터 | `BuyDamagePerMeter` | 200 | 타워에서 멀수록 피해 증가 |
| 7 | 멀티샷 확률 | `BuyMultishotChance` | 99 | 한 번에 여러 발 발사 (max 시 약 49.5%) |
| 8 | 멀티샷 표적 | `BuyMultishotTargets` | 7 | 멀티샷 시 발사 수 (적이 많을 때) |
| 9 | 고속발사 확률 | `BuyRapidFireChance` | 85 | 일정 시간 4배속 발사 (max 약 34%) |
| 10 | 고속발사 지속시간 | `BuyRapidFireDuration` | 99 | 고속발사 유지 시간 |
| 11 | 바운스샷 확률 | `BuyBounceChance` | 85 | 명중 후 다른 적으로 튕김 (max 약 68%) |
| 12 | 바운스샷 표적 | `BuyBounceTargets` | 7 | 튕기는 횟수 |
| 13 | 바운스샷 범위 | `BuyBounceRange` | 60 | 튕길 수 있는 거리 |
| 14 | 슈퍼치명타 확률 | `BuySuperCritChance` | 100 | 치명타 시 슈퍼치명타 확률 (실제 = 표기×0.8) |
| 15 | 슈퍼치명타 증폭 | `BuySuperCritAmp` | 120 | 슈퍼치명타 피해 배율 |

---

## 3.2 방어 업그레이드 (Defense)

| # | 표시명 | 버튼/ID 이름 | Max Lv | 설명 |
|---|--------|----------------|--------|------|
| 1 | 체력 | `BuyHealth` | 6000 | 타워 최대 HP. 원거리/보스 대비 필수 |
| 2 | 체력 재생 | `BuyRegen` | 6000 | 초당 회복 (흡혈보다 비중 낮음) |
| 3 | 방어율% | `BuyDefense` | 99 | 받는 피해 % 감소 (max 약 49.5%), 절대방어보다 먼저 적용 |
| 4 | 절대 방어 | `BuyAbsoluteDefense` | 5000 | 고정 피해 감소 (방어율 적용 후) |
| 5 | 반사 데미지 | `BuyReflectDamage` | 99 | 적이 타워 공격 시 적 최대체력 1% 피해 (보스 50%). 보스 3방 버티면 확정 처치 |
| 6 | 흡혈 | `BuyLifesteal` | 80 | 타격 피해의 일부만큼 타워 회복. 생존 핵심 |
| 7 | 넉백 확률 | `BuyKnockbackChance` | 80 | 공격 시 적 넉백 (max 80%) |
| 8 | 넉백 강도 | `BuyKnockbackStrength` | 40 | 넉백 거리 |
| 9 | 오브 속도 | `BuyOrbSpeed` | 38 | 타워 주변 회전 오브 속도. 오브 접촉 = 일반 적 즉사 |
| 10 | 오브 | `BuyOrbCount` | 4 | 오브 개수 (연구로 추가 가능) |
| 11 | 충격파 크기 | `BuyShockwaveSize` | 35 | 주기적 충격파 범위 (보스 제외) |
| 12 | 충격파 주기 | `BuyShockwaveInterval` | 40 | 충격파 간격 (max 시 약 14초) |
| 13 | 지뢰 확률 | `BuyMineChance` | 50 | 적 처치 시 지뢰 생성 (max 30%) |
| 14 | 지뢰 피해 | `BuyMineDamage` | 200 | 지뢰 피해량 |
| 15 | 지뢰 반경 | `BuyMineRadius` | 50 | 지뢰 폭발 범위 |
| 16 | 죽음의 거부 | `BuyDeathDenial` | 75 | 타워 치명타 무시 (max 30%). 에너지보호막과 시너지 |
| 17 | 벽 체력 | `BuyWallHealth` | 1800 | 타워 주변 벽(방어막) HP, 타워 체력에 비례 |
| 18 | 벽 재건 | `BuyWallRebuild` | 300 | 벽 파괴 후 재건 시간 |

---

## 3.3 유틸리티 업그레이드 (Utility)

| # | 표시명 | 버튼/ID 이름 | Max Lv | 설명 |
|---|--------|----------------|--------|------|
| 1 | 현금 보너스 | `BuyCashBonus` | 149 | 획득 현금 배율 |
| 2 | 현금/웨이브 | `BuyCashPerWave` | 149 | 웨이브 완료 시 현금 (초반 경제 핵심) |
| 3 | 동전/킬 보너스 | `BuyCoinPerKill` | 149 | 처치 시 동전 |
| 4 | 동전/웨이브 | `BuyCoinPerWave` | 149 | 웨이브 완료 시 동전 |
| 5 | 무료 공격업그레이드 | `BuyFreeAttackUpgrade` | 99 | 웨이브 완료 시 무료 공격 업그레이드 확률 |
| 6 | 무료 방어업그레이드 | `BuyFreeDefenseUpgrade` | 99 | 웨이브 완료 시 무료 방어 업그레이드 확률 |
| 7 | 무료 유틸리티업그레이드 | `BuyFreeUtilityUpgrade` | 99 | 웨이브 완료 시 무료 유틸 업그레이드 확률 |
| 8 | 이자/웨이브 | `BuyInterestPerWave` | 99 | 웨이브 종료 시 보유 현금의 n% (상한 있음) |
| 9 | 회복량 | `BuyHealAmount` | 300 | 회복 패키지 1회당 회복 % (max 134% 등) |
| 10 | 최대 회복 | `BuyMaxHeal` | 500 | 최대체력 초과 회복 상한 (max 시 기본체력의 16.5배 등) |
| 11 | 패키지 확률 | `BuyPackageChance` | 60 | 웨이브당 회복 패키지 등장 확률 (max 30%) |
| 12 | 적 공격력 레벨 건너뛰기 | `BuyEnemyAtkSkip` | 699 | 웨이브 완료 시 적 공격력 유지 확률 (max 35%) |
| 13 | 적 체력 레벨 건너뛰기 | `BuyEnemyHpSkip` | 699 | 웨이브 완료 시 적 체력 유지 확률 (max 35%) |

---

## 요약: 탭별 아이템 개수

| 탭 | 아이템 수 | 비고 |
|----|-----------|------|
| 공격 | 15 | 데미지, 공격속도, 치명타, 범위, 멀티샷, 고속발사, 바운스샷, 슈퍼치명타 |
| 방어 | 18 | 체력, 재생, 방어율, 절대방어, 반사, 흡혈, 넉백, 오브, 충격파, 지뢰, 죽음의 거부, 벽 |
| 유틸 | 13 | 현금/동전 보너스·웨이브, 무료 업그레이드 3종, 이자, 회복·패키지, 적 스탯 건너뛰기 |
| **합계** | **46** | |

---

## 코드 연동 시 참고

- **BattleUI**: `CreateUpgradeItemFromPrefab(parent, 표시명, 현재값문자열, 비용문자열, 버튼/ID이름)`  
  → 위 표의 **표시명** / **버튼/ID 이름** 을 그대로 넣으면 됨.
- **BindUpgradeGrid**: 자식의 `Button.name`으로 타입 판별.  
  → 새로 추가하는 항목은 `UpgradeConfig.GetDisplayName(buttonName)` 및 `BindUpgradeGrid` 내 분기에 해당 버튼 이름을 추가해야 함.
- **UpgradeConfig**: 표시명 상수·비용 공식은 `UpgradeConfig`에 추가 후, 위 표시명·버튼 이름과 맞추면 유지보수에 유리함.

이 문서는 `TheTower_GameDesign_Reference.md` §3 워크샵을 프리팹 단위로 풀어 둔 자료입니다.
