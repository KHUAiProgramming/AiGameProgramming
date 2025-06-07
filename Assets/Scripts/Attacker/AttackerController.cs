using System.Collections;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class AttackerController : MonoBehaviour
{
    public enum AttackType
    {
        None,
        Strong
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
    [Header("Attacker Stats - 공격형 특화")]
    [SerializeField] private float moveSpeed = 4.5f; // 방어형보다 빠름

    [Header("Combat Timing")]
    [SerializeField] private float dodgeDistance = 1.5f;

    [Header("Cooldown Settings - 원래값 유지")]
    [SerializeField] private float attackCooldown = 2.5f; // 원래대로
    [SerializeField] private float blockCooldown = 2.5f; // 원래대로
    [SerializeField] private float dodgeCooldown = 5.0f; // 원래대로

    [Header("Status - 원래값 유지")]
    [SerializeField] private float maxHP = 100f; // 원래대로
    [SerializeField] private float currentHP = 100f;

    // Components
    private Animator animator;
    private Rigidbody rb;
    private SwordHitbox swordHitbox;
    private SwordHitbox swordHitboxStrong;

    // Combat State
    [SerializeField] private CombatState currentCombatState = CombatState.Idle;
    [SerializeField] private float stateTimer = 0f;
    private AttackType currentAttackType = AttackType.None;

    // Timing
    private float lastAttackTime = -1000f;
    private float lastBlockTime = -1000f;
    private float lastDodgeTime = -1000f;

    // Animation durations (calculated from clips) - 원래값 유지
    private float strongattackCasting = 2.0f;
    private float attackDuration = 1.0f; // 원래대로
    private float strongattackDuration = 4.8f;
    private float blockDuration = 1.0f; // 원래대로
    private float dodgeDuration = 0.6f;

    // States
    private bool isAttacking = false;
    private bool isStrongAttacking = false;
    private bool isBlocking = false;
    private bool isDodging = false;
    private bool isInvincible = false;
    private Vector3 currentMoveDirection = Vector3.zero;

    private float dodgeMultiplier = 1.0f; // 회피 속도 조절
    private float attackMultiplier = 1.0f; // 공격 속도 조절
    private float blockMultiplier = 1.0f; // 블록 속도 조절

    // Target for rotation
    private Transform currentTarget;

    // Properties
    public float MaxHP => maxHP;
    public float CurrentHP => currentHP;
    public float HPPercentage => currentHP / maxHP;
    public bool IsDead => currentHP <= 0f;
    public bool IsInvincible => isInvincible;
    public bool IsDodging => isDodging;
    public bool IsAttacking => isAttacking;
    public bool IsStrongAttacking => isStrongAttacking;
    public bool IsBlocking => isBlocking;
    public float StateProgress => stateTimer / attackDuration;
    public float AttackCooldownRemaining => Mathf.Max(0f, attackCooldown - (Time.time - lastAttackTime));
    public float BlockCooldownRemaining => Mathf.Max(0f, blockCooldown - (Time.time - lastBlockTime));
    public float DodgeCooldownRemaining => Mathf.Max(0f, dodgeCooldown - (Time.time - lastDodgeTime));
    public float MoveSpeed => moveSpeed;


    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        SwordHitbox[] swordHitboxes = GetComponentsInChildren<SwordHitbox>();

        foreach (var comp in swordHitboxes)
        {
            if (comp.gameObject.name == "SwordHitbox")
                swordHitbox = comp;
            if (comp.gameObject.name == "SwordHitbox_StrongAttack")
                swordHitboxStrong = comp;
        }
        

        // 컴포넌트 검증
        if (animator == null) Debug.LogError("Animator not found on " + gameObject.name);
        if (rb == null) Debug.LogError("Rigidbody not found on " + gameObject.name);
        if (swordHitbox == null) Debug.LogWarning("SwordHitbox not found in children of " + gameObject.name);
        if (swordHitboxStrong == null) Debug.LogWarning("SwordHitbox not found in children of " + gameObject.name);

        // 애니메이션 클립 길이 가져오기
        SetAnimationDurations();
        currentHP = maxHP;
    }

    void Update()
    {
        if (currentCombatState != CombatState.Idle && currentCombatState != CombatState.Blocking && currentCombatState != CombatState.Dodging)
        {
            stateTimer += Time.deltaTime;
        }
    }

    public void Move(Vector3 direction)
    {
        if (IsAttacking || IsStrongAttacking || IsBlocking || IsDodging) return;

        direction.y = 0;
        direction = direction.normalized;

        if (direction.magnitude > 0.1f)
        {
            currentMoveDirection = direction;
            Vector3 restrictedDirection = GetClosest4Direction(direction);

            if (rb != null)
            {
                rb.velocity = new Vector3(restrictedDirection.x * moveSpeed, rb.velocity.y, restrictedDirection.z * moveSpeed);
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
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
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

    public bool CanAttack()
    {
        return !IsDead && !IsAttacking && !IsStrongAttacking && !IsBlocking && !IsDodging &&
               Time.time - lastAttackTime >= attackCooldown;
    }

    public bool CanBlock()
    {
        return !IsDead && !IsAttacking && !IsStrongAttacking && !IsBlocking && !IsDodging &&
               Time.time - lastBlockTime >= blockCooldown;
    }

    public bool CanDodge()
    {
        return !IsDead && !IsAttacking && !IsStrongAttacking && !IsBlocking && !IsDodging &&
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
            direction.y = 0; // Y축 회전 방지

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

        StartCoroutine(AttackCorutine());
        return true;
    }

    public bool StrongAttack()
    {
        if (!CanAttack()) return false;

        // 타겟을 향해 회전
        RotateTowardsTarget();

        StartCoroutine(StrongAttackCorutine());
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

    private IEnumerator StrongAttackCorutine()
    {
        isStrongAttacking = true;

        if (animator != null)
        {
            animator.SetFloat("AttackSpeed", attackMultiplier);
            animator.SetTrigger("onStrongAttack");
        }

        // Casting
        if (swordHitboxStrong != null)
        {
            yield return new WaitForSeconds(strongattackCasting);
            swordHitboxStrong.EnableHitbox();
        }

        yield return new WaitForSeconds(strongattackDuration - strongattackCasting);

        if (swordHitboxStrong != null)
        {
            swordHitboxStrong.DisableHitbox();
        }

        lastAttackTime = Time.time;
        isStrongAttacking = false;
        currentCombatState = CombatState.Idle;
    }

    public bool Block()
    {
        if (!CanBlock()) return false;
        StartCoroutine(BlockCoroutine());
        return true;
    }

    public bool Dodge(Vector3 direction)
    {
        if (!CanDodge()) return false;
        StartCoroutine(DodgeCoroutine(direction));
        return true;
    }

    private IEnumerator BlockCoroutine()
    {
        Debug.Log($"{gameObject.name} is blocking! (Attacker)");
        isBlocking = true;
        currentCombatState = CombatState.Blocking;

        if (animator != null)
        {
            animator.SetFloat("BlockSpeed", blockMultiplier);
            animator.SetTrigger("onBlock");
        }

        yield return new WaitForSeconds(blockDuration);

        Debug.Log($"{gameObject.name} finished blocking (Attacker)");
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
        while (elapsedTime < dodgeDuration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / dodgeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        isInvincible = false;
        yield return new WaitForSeconds(dodgeDuration - 0.1f);

        lastDodgeTime = Time.time;
        isDodging = false;
    }

    public void TakeDamage(float damage)
    {
        if (IsInvincible || IsDead) return;

        if (IsBlocking)
        {
            Debug.Log($"{gameObject.name} blocked {damage} damage!");
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

    private void OnDeath()
    {
        StopAllCoroutines();
        currentCombatState = CombatState.Idle;
        currentAttackType = AttackType.None;
        isBlocking = false;
        isDodging = false;
        isInvincible = false;
    }

    // HP 리셋 (CombatManager용) - DefenderController와 동일한 인터페이스
    public void ResetHP()
    {
        currentHP = maxHP;
        currentCombatState = CombatState.Idle;
        currentAttackType = AttackType.None;
        isBlocking = false;
        isDodging = false;
        isInvincible = false;
        StopAllCoroutines();
    }

    // 애니메이션 클립 길이 설정 - DefenderController와 동일한 방식
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

    public float GetDistanceTo(Transform target)
    {
        if (target == null) return float.MaxValue;
        return Vector3.Distance(transform.position, target.position);
    }

    public bool IsMoving() => currentMoveDirection.magnitude > 0.1f;
}