# RL Combat Training Setup

## 개요

이 프로젝트는 Behavior Tree 에이전트와 RL 에이전트 간의 전투 학습을 구현합니다.

### 훈련 시나리오

1. **AttackerRL vs BT Defender**: RL 공격형이 BT 수비형과 대전하며 학습
2. **DefenderRL vs BT Attacker**: RL 수비형이 BT 공격형과 대전하며 학습

## 필요한 Unity 설정

### 1. ML-Agents 패키지 설치

Unity Package Manager에서 ML-Agents 패키지가 설치되어 있는지 확인하세요.

### 2. 씬 설정

#### AttackerRL 훈련용 씬:

-   AttackerRLAgent 컴포넌트를 공격형 캐릭터에 추가
-   BT DefenderController가 있는 수비형 캐릭터 배치
-   Behavior Parameters 설정:
    -   Behavior Name: "AttackerRLAgent"
    -   Vector Observation Space: 20
    -   Continuous Actions: 2 (이동)
    -   Discrete Actions: 5 (전투액션)

#### DefenderRL 훈련용 씬:

-   DefenderRLAgent 컴포넌트를 수비형 캐릭터에 추가
-   BT AttackerController가 있는 공격형 캐릭터 배치
-   Behavior Parameters 설정:
    -   Behavior Name: "DefenderRLAgent"
    -   Vector Observation Space: 20
    -   Continuous Actions: 2 (이동)
    -   Discrete Actions: 4 (전투액션)

## 훈련 실행

### 전제 조건

```bash
pip install mlagents
```

### AttackerRL 훈련

```bash
chmod +x train_attacker.sh
./train_attacker.sh
```

### DefenderRL 훈련

```bash
chmod +x train_defender.sh
./train_defender.sh
```

## 액션 스페이스

### AttackerRL 액션

-   **연속 액션 (2개)**: moveX, moveZ (-1 to 1)
-   **이산 액션 (5개)**:
    -   0: 아무것도 하지 않음
    -   1: 일반공격
    -   2: 발차기 공격
    -   3: 방어
    -   4: 회피

### DefenderRL 액션

-   **연속 액션 (2개)**: moveX, moveZ (-1 to 1)
-   **이산 액션 (4개)**:
    -   0: 아무것도 하지 않음
    -   1: 일반공격
    -   2: 방어
    -   3: 회피

## 보상 시스템

### AttackerRL 보상

-   상대에게 데미지: +1.0 \* damage
-   자신이 데미지 받음: -0.5 \* damage
-   발차기로 방어 뚫기: +2.0
-   일반공격으로 스턴: -1.0
-   승리: +10.0
-   패배: -5.0

### DefenderRL 보상

-   상대에게 데미지: +1.0 \* damage
-   성공적인 방어: +1.5
-   상대 스턴시키기: +2.0
-   발차기로 방어 뚫림: -1.0
-   승리: +10.0
-   패배: -5.0

## 모니터링

### Tensorboard

```bash
tensorboard --logdir results
```

### 훈련 진행 확인

-   `results/` 폴더에서 훈련 로그 확인
-   Unity Console에서 에피소드 시작/종료 로그 확인

## 트러블슈팅

### 자주 발생하는 문제

1. **Agent 컴포넌트 참조 오류**: Inspector에서 모든 참조가 올바르게 설정되었는지 확인
2. **Behavior Name 불일치**: YAML 파일의 behavior name과 Unity의 Behavior Parameters가 일치하는지 확인
3. **관찰값 크기 오류**: Vector Observation Space가 20으로 설정되었는지 확인

### 디버깅 팁

-   Heuristic 모드로 먼저 테스트 (Behavior Type을 "Heuristic Only"로 설정)
-   Console 로그로 에피소드 시작/종료 확인
-   Inspector에서 보상값 실시간 모니터링

## 파일 구조

```
AiGameProgramming/
├── Assets/Scripts/RL/
│   ├── AttackerRLAgent.cs
│   ├── DefenderRLAgent.cs
│   └── TrainingManager.cs
├── config/
│   ├── attacker_rl_config.yaml
│   ├── defender_rl_config.yaml
│   └── multi_agent_config.yaml
├── train_attacker.sh
├── train_defender.sh
└── README_RL_Setup.md
```

## 설정 참고사항

-   에피소드 최대 시간: 30초
-   시간 제한 초과 시 무승부 (-1 보상)
-   거리 기반 페널티로 너무 멀리 떨어지지 않도록 유도
-   Memory 사용으로 시간에 따른 패턴 학습 가능
