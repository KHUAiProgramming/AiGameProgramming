using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class DefenderRLAgent : Agent
{
    [Header("Training References")]
    [SerializeField] private DefenderController defenderController;
    [SerializeField] private AttackerController btAttacker; // BT 공격형 상대
    [SerializeField] private Transform attackerTransform;
    [SerializeField] private Transform wallTransform1;
    [SerializeField] private Transform wallTransform2;
    [SerializeField] private Transform wallTransform3;
    [SerializeField] private Transform wallTransform4;

    [Header("Training Settings")]
    [SerializeField] private float maxEpisodeTime = 30f;
    [SerializeField] private float episodeTimer = 0f;

    [Header("Reward Settings")]
    [SerializeField] private float damageReward = 1.0f;
    [SerializeField] private float deathPenalty = -5.0f;
    [SerializeField] private float timeoutPenalty = -1.0f;
    [SerializeField] private float blockSuccessReward = 1.5f; // 방어 성공 보상
    [SerializeField] private float stunEnemyReward = 2.0f; // 상대 스턴시키기 보상
    [SerializeField] private float kickThroughPenalty = -1.0f; // 발차기로 방어 뚫림

    private Vector3 initialDefenderPosition;
    private Vector3 initialAttackerPosition;
    private float previousDefenderHP;
    private float previousAttackerHP;
    private bool previouslyBlocking = false;

    public override void Initialize()
    {
        // 컴포넌트 참조 확인
        if (defenderController == null)
            defenderController = GetComponent<DefenderController>();

        if (btAttacker == null)
            btAttacker = FindObjectOfType<AttackerController>();

        if (attackerTransform == null && btAttacker != null)
            attackerTransform = btAttacker.transform;

        // 초기 위치 저장
        initialDefenderPosition = transform.position;
        if (attackerTransform != null)
            initialAttackerPosition = attackerTransform.position;
    }

    public override void OnEpisodeBegin()
    {
        // 위치 초기화
        transform.position = initialDefenderPosition;
        if (attackerTransform != null)
            attackerTransform.position = initialAttackerPosition;

        // HP 초기화
        defenderController.ResetHP();
        btAttacker.ResetHP();

        // 이전 HP 저장
        previousDefenderHP = defenderController.CurrentHP;
        previousAttackerHP = btAttacker.CurrentHP;

        // 타이머 초기화
        episodeTimer = 0f;
        previouslyBlocking = false;

        Debug.Log("DefenderRL Episode Started");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 자신의 상태 (7개)
        sensor.AddObservation(defenderController.CurrentHP / defenderController.MaxHP); // 정규화된 HP
        sensor.AddObservation(defenderController.IsAttacking ? 1f : 0f);
        sensor.AddObservation(defenderController.IsBlocking ? 1f : 0f);
        sensor.AddObservation(defenderController.IsDodging ? 1f : 0f);
        sensor.AddObservation(defenderController.JustFinishedBlocking ? 1f : 0f);
        sensor.AddObservation(defenderController.AttackCooldownRemaining / 3.5f);
        sensor.AddObservation(defenderController.DodgeCooldownRemaining / 4.0f);

        // 상대방 상태 (9개)
        sensor.AddObservation(btAttacker.CurrentHP / btAttacker.MaxHP);
        sensor.AddObservation(btAttacker.IsAttacking ? 1f : 0f);
        sensor.AddObservation(btAttacker.IsKickAttacking ? 1f : 0f);
        sensor.AddObservation(btAttacker.IsBlocking ? 1f : 0f);
        sensor.AddObservation(btAttacker.IsDodging ? 1f : 0f);
        sensor.AddObservation(btAttacker.IsStunned ? 1f : 0f);
        sensor.AddObservation(btAttacker.AttackCooldownRemaining / 3.0f);
        sensor.AddObservation(btAttacker.BlockCooldownRemaining / 2.0f);
        sensor.AddObservation(btAttacker.DodgeCooldownRemaining / 3.5f);

        // 상대적 위치 및 거리 (4개)
        Vector3 relativePosition = attackerTransform.position - transform.position;
        sensor.AddObservation(relativePosition.x / 10f); // 정규화
        sensor.AddObservation(relativePosition.z / 10f);
        sensor.AddObservation(relativePosition.magnitude / 10f); // 거리
        sensor.AddObservation(Vector3.Dot(transform.forward, relativePosition.normalized)); // 방향

        Vector3 wallrelativePosition1 = wallTransform1.position - transform.position;
        Vector3 wallrelativePosition2 = wallTransform2.position - transform.position;
        Vector3 wallrelativePosition3 = wallTransform3.position - transform.position;
        Vector3 wallrelativePosition4 = wallTransform4.position - transform.position;
        sensor.AddObservation(wallrelativePosition1.magnitude / 10f);
        sensor.AddObservation(wallrelativePosition2.magnitude / 10f);
        sensor.AddObservation(wallrelativePosition3.magnitude / 10f);
        sensor.AddObservation(wallrelativePosition4.magnitude / 10f);

        // 총 24개 관찰값
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        episodeTimer += Time.fixedDeltaTime;

        // 연속 행동 (이동)
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        Vector3 moveDirection = new Vector3(moveX, 0, moveZ);

        // 이동 실행
        if (moveDirection.magnitude > 0.1f)
        {
            defenderController.Move(moveDirection.normalized);
        }
        else
        {
            defenderController.Stop();
        }

        // 이산 행동 (전투 액션)
        int combatAction = actions.DiscreteActions[0];
        ExecuteCombatAction(combatAction);

        // 보상 계산
        CalculateRewards();

        // 에피소드 종료 조건 확인
        CheckEpisodeEnd();
    }

    private void ExecuteCombatAction(int action)
    {
        switch (action)
        {
            case 0: // 아무것도 하지 않음
                break;
            case 1: // 일반공격 (Attack)
                if (defenderController.CanAttack())
                {
                    defenderController.SetTarget(attackerTransform);
                    defenderController.Attack();
                }
                break;
            case 2: // 방어 (Block)
                if (defenderController.CanBlock())
                {
                    defenderController.Block();
                }
                break;
            case 3: // 회피 (Dodge)
                if (defenderController.CanDodge())
                {
                    Vector3 dodgeDirection = (transform.position - attackerTransform.position).normalized;
                    defenderController.Dodge(dodgeDirection);
                }
                break;
        }
    }

    private void CalculateRewards()
    {
        // HP 변화 기반 보상
        float defenderHPDelta = defenderController.CurrentHP - previousDefenderHP;
        float attackerHPDelta = btAttacker.CurrentHP - previousAttackerHP;

        // 상대에게 데미지를 입혔을 때 보상
        if (attackerHPDelta < 0)
        {
            float damageDealt = Mathf.Abs(attackerHPDelta);
            AddReward(damageDealt * damageReward);
        }

        // 자신이 데미지를 받았을 때 페널티
        if (defenderHPDelta < 0)
        {
            float damageTaken = Mathf.Abs(defenderHPDelta);

            // 발차기로 방어가 뚫렸을 때 추가 페널티
            if (previouslyBlocking && btAttacker.IsKickAttacking)
            {
                AddReward(kickThroughPenalty);
            }

            AddReward(-damageTaken * 0.5f);
        }

        // 방어 성공 보상 (상대가 스턴되었을 때)
        if (btAttacker.IsStunned && previouslyBlocking)
        {
            AddReward(stunEnemyReward);
        }

        // 성공적인 방어 보상
        if (defenderController.JustFinishedBlocking)
        {
            AddReward(blockSuccessReward);
        }

        // 거리 기반 보상 (적당한 거리 유지)
        float distance = Vector3.Distance(transform.position, attackerTransform.position);
        if (distance > 6f || distance < 1f)
        {
            AddReward(-0.001f);
        }

        // 벽과의 거리 기반 보상 (벽과 멀어지도록록)
        float walldistance1 = Vector3.Distance(transform.position, wallTransform1.position);
        float walldistance2 = Vector3.Distance(transform.position, wallTransform2.position);
        float walldistance3 = Vector3.Distance(transform.position, wallTransform3.position);
        float walldistance4 = Vector3.Distance(transform.position, wallTransform4.position);
        if (walldistance1 < 3f) AddReward(-0.001f);
        if (walldistance2 < 3f) AddReward(-0.001f);
        if (walldistance3 < 3f) AddReward(-0.001f);
        if (walldistance4 < 3f) AddReward(-0.001f);

        // 시간 페널티 (너무 오래 걸리지 않도록)
        AddReward(-0.0005f);

        // 이전 상태 업데이트
        previousDefenderHP = defenderController.CurrentHP;
        previousAttackerHP = btAttacker.CurrentHP;
        previouslyBlocking = defenderController.IsBlocking;
    }

    private void CheckEpisodeEnd()
    {
        // 승리 조건: 상대 처치
        if (btAttacker.IsDead)
        {
            AddReward(200f); // 승리 보상
            EndEpisode();
            return;
        }

        // 패배 조건: 자신 사망
        if (defenderController.IsDead)
        {
            AddReward(deathPenalty);
            EndEpisode();
            return;
        }

        // 시간 초과
        if (episodeTimer >= maxEpisodeTime)
        {
            AddReward(timeoutPenalty);
            EndEpisode();
            return;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // 테스트용 수동 조작
        var continuousActions = actionsOut.ContinuousActions;
        var discreteActions = actionsOut.DiscreteActions;

        // WASD로 이동
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");

        // 전투 액션
        discreteActions[0] = 0; // 기본값

        if (Input.GetKeyDown(KeyCode.J)) discreteActions[0] = 1; // 일반공격
        else if (Input.GetKey(KeyCode.Space)) discreteActions[0] = 2; // 방어
        else if (Input.GetKeyDown(KeyCode.LeftShift)) discreteActions[0] = 3; // 회피
    }
}