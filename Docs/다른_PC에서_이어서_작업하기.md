# 다른 컴퓨터에서 이어서 작업하기

## 방법 1: Git 사용 (권장)

### 지금 컴퓨터에서

1. **Git이 없다면** [git-scm.com](https://git-scm.com/)에서 설치
2. 프로젝트 폴더에서 터미널/명령 프롬프트 열기
3. **처음 한 번만** (아직 안 했다면):
   ```bash
   git init
   git add .
   git commit -m "Initial: Lobby, Battle 씬 및 UI"
   ```
4. **원격 저장소** (GitHub, GitLab, 비공개 서버 등) 연결 후:
   ```bash
   git remote add origin <저장소_URL>
   git push -u origin main
   ```
5. 이후 작업할 때마다:
   ```bash
   git add .
   git commit -m "작업 내용 요약"
   git push
   ```

### 다른 컴퓨터에서

1. **같은 Unity 버전 설치**: **Unity 6000.3.6f1**  
   (Hub → 설치 → 버전 선택)
2. Git 설치 후 프로젝트 받기:
   ```bash
   git clone <저장소_URL> CreateSelf
   cd CreateSelf
   ```
3. **Unity Hub**에서 "Add" → `CreateSelf` 폴더 선택
4. 프로젝트 더블클릭으로 열기  
   → Unity가 `Library` 등을 자동 생성 (처음엔 1~2분 걸릴 수 있음)
5. 이어서 작업 후 저장하고:
   ```bash
   git add .
   git commit -m "작업 내용"
   git push
   ```

---

## 방법 2: 폴더 통째로 복사 (Git 없을 때)

### 지금 컴퓨터에서

- **복사할 것**: 프로젝트 **전체 폴더** (`CreateSelf` 폴더 통째로)
- **복사 방법**: USB, 클라우드(OneDrive/Google Drive/드롭박스), 압축 후 전송 등

### 다른 컴퓨터에서

1. **Unity 6000.3.6f1** 설치 (Hub에서)
2. 복사해 둔 `CreateSelf` 폴더를 원하는 위치에 붙여넣기
3. Unity Hub → "Add" → 그 `CreateSelf` 폴더 선택
4. 프로젝트 열기

**참고**: `Library` 폴더가 있으면 그대로 두고 가져와도 되고, 용량 줄이려면 `Library`를 지우고 가져와도 됩니다.  
지우면 다른 PC에서 Unity가 다시 생성하므로, 첫 실행 시 조금 더 걸립니다.

---

## 꼭 맞추면 좋은 것

| 항목 | 내용 |
|------|------|
| **Unity 버전** | **6000.3.6f1** (ProjectVersion.txt 기준) |
| **Input System** | 프로젝트에 포함됨. 다른 PC에서 열면 그대로 유지 |
| **EventSystem** | Lobby/Battle 씬에 직접 넣어 둔 경우, 씬 파일에 저장되어 있음 |

---

## 요약

- **Git 쓸 때**: `git push` → 다른 PC에서 `git clone` 또는 `git pull` → Unity로 열기
- **그냥 복사할 때**: `CreateSelf` 폴더 통째로 복사 → 다른 PC에 붙여넣기 → 같은 Unity 버전으로 열기

두 경우 모두 **다른 PC에는 같은 Unity 버전(6000.3.6f1)**을 설치하는 것이 중요합니다.
