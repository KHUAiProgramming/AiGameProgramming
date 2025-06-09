using System.Collections;
using UnityEngine;

public class DefenderController : MonoBehaviour
{
    public enum AttackType
    {
        None,
        Weak
    }

    public enum CombatState
    {
        Idle,
        WindUp,
        Commit,
        Active,
        Recovery,
        Blocking,
        Dodging
    }

    [Header("Defender Stats - 방어형 특화")]
    [SerializeField] private float moveSpeed = 3.5f; // 공격형보다 느림

    [Header("Combat Timing")]
    [Range(0f, 100f)][SerializeField] private float commitPoint = 25f;
    [Range(0f, 100f)][SerializeField] private float activePoint = 75f;
    [SerializeField] private float dodgeDistance = 2.0f;

    [Header("Animation Speed")]
    [SerializeField] private float attackSpeed = 1.0f;
    [SerializeField] private float blockSpeed = 1.0f;
    [SerializeField] private float dodgeSpeed = 1.0f;

    [Header("Cooldown Settings - 원래값 유지")]
    [SerializeField] private float attackCooldown = 2.5f; // 원래대로
    [SerializeField] private float blockCooldown = 2.0f; // 원래대로
    [SerializeField] private float dodgeCooldown = 4.0f; // 원래대로

    [Header("Duration Settings - 원래값 유지")]
    [SerializeField] private float attackDuration = 1.2f;
    [SerializeField] private float blockDuration = 1.5f; // 원래대로
    [SerializeField] private float dodgeDuration = 0.8f;

    [Header("Status - 원래값 유지")]
    [SerializeField] private float maxHP = 100f; // 원래대로
    [SerializeField] private float currentHP = 100f;

    [SerializeField] private float stunDuration = 2.0f;
    [SerializeField] private AttackerController targetAttacker;


    // Components
    private Animator animator;
    private Rigidbody rb;
    private SwordHitbox swordHitbox;

    // Combat State
    [SerializeField] private CombatState currentCombatState = CombatState.Idle;
    [SerializeField] private float stateTimer = 0f;
    private AttackType currentAttackType = AttackType.None;

    // Timing
    private float lastAttackTime = -1000f;
    private float lastBlockTime = -1000f;
    private float lastDodgeTime = -1000f;
    private float windUpTime;
    private float commitTime;

    // States
    private bool isBlocking = false;
    private bool isDodging = false;
    private bool isInvincible = false;
    private bool isAttacking = false;
    private bool justFinishedBlocking = false; // 방어 직후 상태 추적
    private Vector3 currentMoveDirection = Vector3.zero;
    private Transform currentTarget;

    // Properties
    public float MaxHP => maxHP;
    public float CurrentHP => currentHP;
    public float HPPercentage => currentHP / maxHP;
    public bool IsDead => currentHP <= 0f;
    public bool IsInvincible => isInvincible;
    public bool IsDodging => isDodging;
    public bool IsAttacking => isAttacking;
    public bool IsBlocking => isBlocking;
    public bool JustFinishedBlocking => justFinishedBlocking; // 방어 완료 감지용
    public AttackType CurrentAttackType => currentAttackType;
    public CombatState CurrentCombatState => currentCombatState;
    public float StateProgress => stateTimer / attackDuration;
    public float AttackCooldownRemaining => Mathf.Max(0f, attackCooldown - (Time.time - lastAttackTime));
    public float BlockCooldownRemaining => Mathf.Max(0f, blockCooldown - (Time.time - lastBlockTime));
    public float DodgeCooldownRemaining => Mathf.Max(0f, dodgeCooldown - (Time.time - lastDodgeTime));
    public float MoveSpeed => moveSpeed;
    private float attackMultiplier = 1.0f;
    private float blockMultiplier = 1.0f;
    private float dodgeMultiplier = 1.0f;

    // 전투 통계 (전투에 영향 없음)
    [Header("전투 통계")]
    public CombatStats combatStats = new CombatStats();

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        swordHitbox = GetComponentInChildren<SwordHitbox>();

        if (animator == null) Debug.LogError("Animator not found on " + gameObject.name);
        if (rb == null) Debug.LogError("Rigidbody not found on " + gameObject.name);
        if (swordHitbox == null) Debug.LogWarning("SwordHitbox not found in children of " + gameObject.name);
        if (targetAttacker == null)
        {
            GameObject attackerobj = GameObject.Find("Maria WProp J J Ong");
            targetAttacker = attackerobj.GetComponent<AttackerController>();
        }

        SetAnimationDurations();
        currentHP = maxHP;

        // 통계 이벤트 구독 (전투에 영향 없음)
        CombatEvents.OnAttackAttempt += OnAttackAttemptHandler;
        CombatEvents.OnBlockAttempt += OnBlockAttemptHandler;
        CombatEvents.OnStunCaused += OnStunCausedHandler;
        CombatEvents.OnDodgeAttempt += OnDodgeAttemptHandler;
        CombatEvents.OnDamageTaken += OnDamageTakenHandler;
        CombatEvents.OnKickAttempt += OnKickAttemptHandler;
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제
        CombatEvents.OnAttackAttempt -= OnAttackAttemptHandler;
        CombatEvents.OnBlockAttempt -= OnBlockAttemptHandler;
        CombatEvents.OnStunCaused -= OnStunCausedHandler;
        CombatEvents.OnDodgeAttempt -= OnDodgeAttemptHandler;
        CombatEvents.OnDamageTaken -= OnDamageTakenHandler;
        CombatEvents.OnKickAttempt -= OnKickAttemptHandler;
    }

    void Update()
    {
        if (currentCombatState != CombatState.Idle && currentCombatState != CombatState.Blocking && currentCombatState != CombatState.Dodging)
        {
            stateTimer += Time.deltaTime;
        }

        // 방어 완료 상태를 한 프레임 후 리셋
        if (justFinishedBlocking)
        {
            StartCoroutine(justFinishedBlockingCoroutine());
        }
    }
    private IEnumerator justFinishedBlockingCoroutine()
    {
        yield return new WaitForSeconds(0.01f);
        justFinishedBlocking = false;
    }

    // Movement
    public void Move(Vector3 direction)
    {
        if (IsAttacking || IsBlocking || IsDodging) return;

        currentMoveDirection = direction;
        direction.y = 0;
        direction = direction.normalized;

        if (direction.magnitude > 0.1f)
        {
            Vector3 restrictedDirection = GetClosest4Direction(direction);

            if (rb != null)
            {
                rb.linearVelocity = new Vector3(restrictedDirection.x * moveSpeed, rb.linearVelocity.y, restrictedDirection.z * moveSpeed);
            }

            UpdateMovementAnimation(restrictedDirection);
        }
        else
        {
            Stop();
        }
    }

    public void Stop()
    {
        currentMoveDirection = Vector3.zero;

        if (rb != null)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }

        UpdateMovementAnimation(Vector3.zero);
    }

    private void UpdateMovementAnimation(Vector3 direction)
    {
        if (animator == null) return;

        animator.SetBool("Forward", false);
        animator.SetBool("Backward", false);
        animator.SetBool("Left", false);
        animator.SetBool("Right", false);
        animator.SetBool("IsMoving", false);

        if (direction.magnitude > 0.1f)
        {
            animator.SetBool("IsMoving", true);

            if (direction == Vector3.forward)
                animator.SetBool("Forward", true);
            else if (direction == Vector3.back)
                animator.SetBool("Backward", true);
            else if (direction == Vector3.left)
                animator.SetBool("Left", true);
            else if (direction == Vector3.right)
                animator.SetBool("Right", true);
        }
    }

    private Vector3 GetClosest4Direction(Vector3 direction)
    {
        // X축과 Z축 중 절댓값이 큰 쪽을 선택
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            // X축이 더 크면 좌우 이동
            return direction.x > 0 ? Vector3.right : Vector3.left;
        }
        else
        {
            // Z축이 더 크면 앞뒤 이동
            return direction.z > 0 ? Vector3.forward : Vector3.back;
        }
    }

    // Combat
    public bool CanAttack()
    {
        return !IsDead && !IsAttacking && !IsBlocking && !IsDodging &&
               Time.time - lastAttackTime >= attackCooldown;
    }

    public bool CanBlock()
    {
        return !IsDead && !IsAttacking && !IsBlocking && !IsDodging &&
               Time.time - lastBlockTime >= blockCooldown;
    }

    public bool CanDodge()
    {
        return !IsDead && !IsAttacking && !IsBlocking && !IsDodging &&
               Time.time - lastDodgeTime >= dodgeCooldown;
    }

    public void SetTarget(Transform target)
    {
        currentTarget = target;
    }

    private void RotateTowardsTarget()
    {
        Transform target = currentTarget;
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0;

            if (direction.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }

    public bool Attack()
    {
        if (!CanAttack()) return false;

        // 타겟을 향해 회전
        RotateTowardsTarget();

        // 공격 시도 이벤트 발생 (전투에 영향 없음)
        CombatEvents.OnAttackAttempt?.Invoke(gameObject.name, false, 0f);

        StartCoroutine(AttackCorutine());
        return true;
    }

    private IEnumerator AttackCorutine()
    {
        isAttacking = true;

        if (swordHitbox != null)
        {
            swordHitbox.EnableHitbox();
        }

        if (animator != null)
        {
            animator.SetFloat("AttackSpeed", attackMultiplier);
            animator.SetTrigger("onAttack");
        }

        yield return new WaitForSeconds(attackDuration);

        if (swordHitbox != null)
        {
            swordHitbox.DisableHitbox();
        }

        lastAttackTime = Time.time;
        isAttacking = false;
        currentCombatState = CombatState.Idle;
    }

    public bool Block()
    {
        if (!CanBlock()) return false;

        // 방어 시도 이벤트 발생 (전투에 영향 없음)
        CombatEvents.OnBlockAttempt?.Invoke(gameObject.name, true);

        StartCoroutine(BlockCoroutine());
        return true;
    }

    public bool Dodge(Vector3 direction)
    {
        if (!CanDodge())
        {
            // 회피 실패 이벤트 (전투에 영향 없음)
            CombatEvents.OnDodgeAttempt?.Invoke(gameObject.name, false);
            return false;
        }

        // 회피 성공 이벤트 발생 (전투에 영향 없음)
        CombatEvents.OnDodgeAttempt?.Invoke(gameObject.name, true);

        StartCoroutine(DodgeCoroutine(direction));
        return true;
    }

    private IEnumerator BlockCoroutine()
    {
        isBlocking = true;
        currentCombatState = CombatState.Blocking;

        if (animator != null)
        {
            animator.SetFloat("BlockSpeed", blockMultiplier);
            animator.SetTrigger("onBlock");
        }

        Stop();
        yield return new WaitForSeconds(1.5f); // 원래대로

        // 방어 완료 직후 상태 플래그 설정
        justFinishedBlocking = true;

        lastBlockTime = Time.time;
        isBlocking = false;
        currentCombatState = CombatState.Idle;
    }

    private IEnumerator DodgeCoroutine(Vector3 direction)
    {
        isDodging = true;
        isInvincible = true;
        currentCombatState = CombatState.Dodging;

        Vector3 dodgeDirection = GetClosest4Direction(direction.normalized);
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + dodgeDirection * dodgeDistance;

        if (animator != null)
        {
            animator.SetFloat("DodgeSpeed", dodgeMultiplier);
            animator.SetTrigger("onDodge");
        }

        float elapsedTime = 0f;
        while (elapsedTime < 0.8f)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / 0.8f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        lastDodgeTime = Time.time;
        isDodging = false;
        isInvincible = false;
        currentCombatState = CombatState.Idle;
    }

    public void TakeDamage(float damage)
    {
        if (IsInvincible || IsDead) return;

        if (IsBlocking && !targetAttacker.IsKickAttacking)
        {
            Debug.Log($"{gameObject.name} blocked {damage} damage!");

            AttackerStun();
            return;
        }

        currentHP = Mathf.Max(0, currentHP - damage);
        Debug.Log($"{gameObject.name} took {damage} damage. HP: {currentHP}/{maxHP}");

        if (IsDead)
        {
            Debug.Log($"{gameObject.name} has been defeated!");
            OnDeath();
        }
    }

    private void AttackerStun()
    {
        AttackerController attacker = targetAttacker;
        if (attacker != null)
        {
            attacker.Stun(stunDuration);

            // 자신의 스턴 유발 통계 증가
            combatStats.stunsCausedByBlock++;

            Debug.Log($"스턴: {stunDuration}s, 방어로 스턴 유발 총 {combatStats.stunsCausedByBlock}회");
        }
    }

    private void OnDeath()
    {
        StopAllCoroutines();
        currentCombatState = CombatState.Idle;
        currentAttackType = AttackType.None;
        isBlocking = false;
        isDodging = false;
        isInvincible = false;
    }

    public void ResetHP()
    {
        currentHP = maxHP;

        // 모든 액션 상태 초기화
        isAttacking = false;
        isBlocking = false;
        isDodging = false;
        isInvincible = false;
        justFinishedBlocking = false;

        // 쿨다운 초기화
        lastAttackTime = -1000f;
        lastBlockTime = -1000f;
        lastDodgeTime = -1000f;

        // 상태 초기화
        currentCombatState = CombatState.Idle;
        currentAttackType = AttackType.None;

        // 이동 중지
        Stop();
        StopAllCoroutines();

        Debug.Log($"{gameObject.name} 상태 완전 초기화 완료");
    }

    private void SetAnimationDurations()
    {
        if (animator == null) return;

        // 공격 애니메이션 길이 가져오기
        AnimationClip attackClip = GetAnimationClip("slash");
        if (attackClip != null)
            attackMultiplier = attackClip.length / attackDuration;

        // 블록 애니메이션 길이 가져오기
        AnimationClip blockClip = GetAnimationClip("blocking2");
        if (blockClip != null)
            blockMultiplier = blockClip.length / blockDuration;

        // 회피 애니메이션 길이 가져오기
        AnimationClip dodgeClip = GetAnimationClip("Standing Dive Forward");
        if (dodgeClip != null)
            dodgeMultiplier = dodgeClip.length / dodgeDuration;
    }

    private AnimationClip GetAnimationClip(string stateName)
    {
        if (animator == null) return null;

        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        if (controller == null) return null;

        foreach (AnimationClip clip in controller.animationClips)
        {
            if (clip.name.ToLower().Contains(stateName.ToLower()))
            {
                return clip;
            }
        }

        return null;
    }

    public void ResetStats()
    {
        combatStats.Reset();
    }

    // 통계 이벤트 핸들러들 (전투에 영향 없음)
    private void OnAttackAttemptHandler(string agentName, bool success, float damage)
    {
        if (agentName == gameObject.name)
        {
            combatStats.attackAttempts++;
            if (success)
            {
                combatStats.attackHits++;
                combatStats.damageDealt += damage;
            }
            else
            {
                combatStats.attackMisses++;
            }
        }
    }

    private void OnBlockAttemptHandler(string agentName, bool success)
    {
        if (agentName == gameObject.name)
        {
            combatStats.blockAttempts++;
            if (success)
                combatStats.blockSuccesses++;
            else
                combatStats.blockFailures++;
        }
    }

    private void OnStunCausedHandler(string stunner, string stunned)
    {
        if (stunner == gameObject.name)
        {
            combatStats.stunsCausedByBlock++;
        }
        if (stunned == gameObject.name)
        {
            combatStats.stunsTaken++;
        }
    }

    private void OnDodgeAttemptHandler(string agentName, bool success)
    {
        if (agentName == gameObject.name)
        {
            combatStats.dodgeAttempts++;
            if (success)
                combatStats.dodgeSuccesses++;
        }
    }

    private void OnDamageTakenHandler(string agentName, float damage)
    {
        if (agentName == gameObject.name)
        {
            combatStats.damageTaken += damage;
        }
    }

    private void OnKickAttemptHandler(string agentName, bool success)
    {
        if (agentName == gameObject.name)
        {
            combatStats.kickAttempts++;
            if (success)
                combatStats.kickThroughDefense++;
        }
    }

}