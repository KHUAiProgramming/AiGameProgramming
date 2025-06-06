using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class AttackerController : MonoBehaviour
{
    public enum MoveDirection
    {
        None,
        Forward,
        Backward,
        Left,
        Right
    }

    public enum AttackType
    {
        None,
        Strong,
        Weak
    }

    [SerializeField] private Animator animator;
    [SerializeField] private float attackCooldown = 2.5f; // 0.0f에서 수정
    [SerializeField] private float blockCooldown = 5.0f;
    [SerializeField] private float lastAttackTime = -111111.0f;
    [SerializeField] private float lastBlockTime = -111111.0f; // 추가
    [SerializeField] private float lastDodgeTime = -111111.0f; // 추가
    [SerializeField] private float strongAttackDuration = 1.5f; // 2f에서 1.5f로 수정 (제안서 스펙)
    [SerializeField] private float weakAttackDuration = 0.5f;
    [SerializeField] private float blockDuration = 1.0f;
    [SerializeField] private float dodgeDistance = 2.0f;
    [SerializeField] private float dodgeDuration = 0.5f;
    [SerializeField] private float dodgeCooldown = 5.0f; // 추가
    [SerializeField] private float invincibilityDuration = 0.3f; // 무적 지속시간

    // 애니메이션 클립 실제 길이 (Inspector에서 설정)
    [SerializeField] private float strongAttackClipLength = 2.0f; // 실제 애니메이션 클립 길이
    [SerializeField] private float weakAttackClipLength = 1.0f;   // 실제 애니메이션 클립 길이
    [SerializeField] private float blockClipLength = 1.5f;       // 실제 애니메이션 클립 길이
    [SerializeField] private float dodgeClipLength = 1.0f;       // 실제 애니메이션 클립 길이

    // HP 관리 시스템
    [SerializeField] private float maxHP = 100f;
    [SerializeField] private float currentHP;

    private MoveDirection currentDirection = MoveDirection.None;

    private SwordHitbox swordHitbox;

    public float AttackCoolDown => attackCooldown;
    public float BlockCoolDown => blockCooldown;

    // HP 관련 프로퍼티
    public float MaxHP => maxHP;
    public float CurrentHP => currentHP;
    public float HPPercentage => currentHP / maxHP;
    public bool IsDead => currentHP <= 0f;

    private bool isAttacking = false;
    private bool isBlocking = false;
    private bool isDodging = false; // 회피 상태 여부 추가
    private bool isInvincible = false; // 무적 상태 여부

    // 애니메이션 이벤트 시스템용 상태
    public bool canBeCountered = false;  // 상대방이 읽을 수 있는 반격 가능 상태
    public AttackType currentAttackType = AttackType.None;

    // 무적 상태 확인용 프로퍼티
    public bool IsInvincible => isInvincible;
    public bool IsDodging => isDodging; // 추가

    // 상태 확인용 프로퍼티 추가
    public bool IsAttacking => isAttacking;
    public bool IsBlocking => isBlocking;
    public bool CanBeCountered => canBeCountered;
    public AttackType CurrentAttackType => currentAttackType;

    // 쿨타임 확인용 프로퍼티
    public float AttackCooldownRemaining => Mathf.Max(0f, attackCooldown - (Time.time - lastAttackTime));
    public float BlockCooldownRemaining => Mathf.Max(0f, blockCooldown - (Time.time - lastBlockTime));
    public float DodgeCooldownRemaining => Mathf.Max(0f, dodgeCooldown - (Time.time - lastDodgeTime));

    void Start()
    {
        animator = GetComponent<Animator>();
        swordHitbox = GetComponentInChildren<SwordHitbox>();

        // HP 초기화
        currentHP = maxHP;
    }

    public void Move(Vector3 direction)
    {
        if (IsDead || isAttacking || isBlocking || isDodging)
        {
            return;
        }
        MoveDirection _direction = MovementUtils.GetMoveDirection(direction);
        SetMoveDirection(_direction);
    }
    public void Stop()
    {
        SetMoveDirection(MoveDirection.None);
    }

    private void SetMoveDirection(MoveDirection direction)
    {
        if (currentDirection == direction) return;
        currentDirection = direction;
        UpdateMovementAnimation();
    }

    private void UpdateMovementAnimation()
    {
        animator.SetBool("Forward", false);
        animator.SetBool("Backward", false);
        animator.SetBool("Left", false);
        animator.SetBool("Right", false);

        switch (currentDirection)
        {
            case MoveDirection.Forward:
                animator.SetBool("Forward", true);
                break;
            case MoveDirection.Backward:
                animator.SetBool("Backward", true);
                break;
            case MoveDirection.Left:
                animator.SetBool("Left", true);
                break;
            case MoveDirection.Right:
                animator.SetBool("Right", true);
                break;
            case MoveDirection.None:
                break;
        }
    }

    public bool CanAttack()
    {
        return !IsDead && !isAttacking && !isBlocking && !isDodging &&
               Time.time - lastAttackTime >= attackCooldown;
    }

    public bool CanBlock()
    {
        return !IsDead && !isAttacking && !isBlocking && !isDodging &&
               Time.time - lastBlockTime >= blockCooldown;
    }

    public bool CanDodge()
    {
        return !IsDead && !isAttacking && !isBlocking && !isDodging &&
               Time.time - lastDodgeTime >= dodgeCooldown;
    }

    public bool Attack()
    {
        if (!CanAttack()) return false;

        StopCoroutine(StrongAttackCoroutine());
        StartCoroutine(StrongAttackCoroutine());
        return true;
    }

    public bool WeakAttack()
    {
        if (!CanAttack()) return false;

        StopCoroutine(WeakAttackCoroutine());
        StartCoroutine(WeakAttackCoroutine());
        return true;
    }

    public bool Block()
    {
        if (!CanBlock()) return false;

        StopCoroutine(BlockCoroutine());
        StartCoroutine(BlockCoroutine());
        return true;
    }

    public bool Dodge(Vector3 direction)
    {
        if (!CanDodge()) return false;

        StopCoroutine(DodgeCoroutine(direction));
        StartCoroutine(DodgeCoroutine(direction));
        return true;
    }

    private IEnumerator StrongAttackCoroutine()
    {
        isAttacking = true;
        canBeCountered = true;
        currentAttackType = AttackType.Strong;
        Stop(); // 이동 중단

        // 애니메이션 속도 조정 (의도한 시간 / 실제 클립 길이)
        float animationSpeed = strongAttackClipLength / strongAttackDuration;
        animator.SetFloat("AttackSpeed", animationSpeed);
        animator.SetTrigger("onAttack");

        yield return new WaitForSeconds(strongAttackDuration);

        // 애니메이션 속도 원복
        animator.SetFloat("AttackSpeed", 1.0f);

        // 공격 완료 후 정리
        lastAttackTime = Time.time;
        isAttacking = false;
        canBeCountered = false;
        currentAttackType = AttackType.None;
    }

    private IEnumerator WeakAttackCoroutine()
    {
        isAttacking = true;
        canBeCountered = true;
        currentAttackType = AttackType.Weak;
        Stop(); // 이동 중단

        // 애니메이션 속도 조정
        float animationSpeed = weakAttackClipLength / weakAttackDuration;
        animator.SetFloat("WeakAttackSpeed", animationSpeed);
        animator.SetTrigger("onWeakAttack");

        yield return new WaitForSeconds(weakAttackDuration);

        // 애니메이션 속도 원복
        animator.SetFloat("WeakAttackSpeed", 1.0f);

        // 공격 완료 후 정리
        lastAttackTime = Time.time;
        isAttacking = false;
        canBeCountered = false;
        currentAttackType = AttackType.None;
    }

    private IEnumerator BlockCoroutine()
    {
        isBlocking = true;
        Stop(); // 이동 중단

        // 애니메이션 속도 조정
        float animationSpeed = blockClipLength / blockDuration;
        animator.SetFloat("BlockSpeed", animationSpeed);
        animator.SetTrigger("onBlock");

        yield return new WaitForSeconds(blockDuration);

        // 애니메이션 속도 원복
        animator.SetFloat("BlockSpeed", 1.0f);

        lastBlockTime = Time.time;
        isBlocking = false;
    }

    private IEnumerator DodgeCoroutine(Vector3 direction)
    {
        isDodging = true;
        isInvincible = true;
        Stop(); // 이동 중단

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + direction.normalized * dodgeDistance;

        // 애니메이션 속도 조정
        float animationSpeed = dodgeClipLength / dodgeDuration;
        animator.SetFloat("DodgeSpeed", animationSpeed);
        animator.SetTrigger("onDodge");

        float elapsedTime = 0f;
        while (elapsedTime < dodgeDuration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / dodgeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;

        // 애니메이션 속도 원복
        animator.SetFloat("DodgeSpeed", 1.0f);

        // 무적 시간 추가 대기 (회피 동작 후에도 잠시 무적)
        yield return new WaitForSeconds(invincibilityDuration);

        lastDodgeTime = Time.time;
        isDodging = false;
        isInvincible = false;
    }

    // 애니메이션 이벤트에서 호출될 함수들

    // 공격 시작 (0% 지점)
    public void OnAttackStart()
    {
        Debug.Log($"{gameObject.name} attack started - counterable");
    }

    // 17% 지점 - 반격 마지노선 (더 이상 반격 불가)
    public void OnAttackCommit()
    {
        canBeCountered = false;
        Debug.Log($"{gameObject.name} attack committed - no longer counterable");
    }

    // 83% 지점 - 실제 데미지 발생
    public void OnAttackHit()
    {
        swordHitbox.EnableHitbox();
        Debug.Log($"{gameObject.name} attack hit - damage active");
    }

    // 100% 지점 - 공격 완료
    public void OnAttackEnd()
    {
        swordHitbox.DisableHitbox();
        Debug.Log($"{gameObject.name} attack ended - damage inactive");
    }

    // 방어 시작 (17% 지점)
    public void OnBlockStart()
    {
        Debug.Log($"{gameObject.name} block started");
    }

    // 방어 활성화 (83% 지점)
    public void OnBlockActive()
    {
        Debug.Log($"{gameObject.name} block active - damage will be blocked");
    }

    // 회피 시작 (17% 지점) 
    public void OnDodgeStart()
    {
        Debug.Log($"{gameObject.name} dodge started");
    }

    // 회피 무적 시작 (83% 지점)
    public void OnDodgeInvincible()
    {
        isInvincible = true;
        Debug.Log($"{gameObject.name} dodge invincible - cannot take damage");
    }

    // 데미지 처리 시스템
    public bool TakeDamage(float damage)
    {
        if (IsDead || isInvincible) return false;

        // 방어 중일 때는 데미지 무효화
        if (isBlocking)
        {
            Debug.Log($"{gameObject.name} blocked {damage} damage!");
            return false;
        }

        currentHP = Mathf.Max(0f, currentHP - damage);
        Debug.Log($"{gameObject.name} took {damage} damage. HP: {currentHP}/{maxHP}");

        // 사망 처리
        if (IsDead)
        {
            OnDeath();
        }

        return true;
    }

    // HP 회복
    public void Heal(float amount)
    {
        if (IsDead) return;

        currentHP = Mathf.Min(maxHP, currentHP + amount);
        Debug.Log($"{gameObject.name} healed {amount}. HP: {currentHP}/{maxHP}");
    }

    // 사망 처리
    private void OnDeath()
    {
        Debug.Log($"{gameObject.name} has died!");

        // 모든 행동 중단
        StopAllCoroutines();
        Stop();

        // 상태 초기화
        isAttacking = false;
        isBlocking = false;
        isDodging = false;
        isInvincible = false;

        // 사망 애니메이션 트리거 (있다면)
        if (animator != null)
        {
            animator.SetTrigger("onDeath");
        }
    }

    // HP 초기화 (리스폰용)
    public void ResetHP()
    {
        currentHP = maxHP;
        Debug.Log($"{gameObject.name} HP reset to full: {currentHP}/{maxHP}");
    }

    public bool IsMoving() => currentDirection != MoveDirection.None;
    public bool IsBusy() => IsDead || isAttacking || isBlocking || isDodging;
    public MoveDirection GetCurrentDirection() => currentDirection;
}