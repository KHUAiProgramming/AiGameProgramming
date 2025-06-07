using System.Collections;
using UnityEngine;

public class DefenderController : MonoBehaviour
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
        Weak,
        Counter
    }

    [Header("Defender Stats")]
    [SerializeField] private Animator animator;
    [SerializeField] private float attackCooldown = 2.5f;
    [SerializeField] private float blockCooldown = 2.5f;
    [SerializeField] private float lastAttackTime = -111111.0f;
    [SerializeField] private float lastBlockTime = -111111.0f;
    [SerializeField] private float lastDodgeTime = -111111.0f;

    [SerializeField] private float weakAttackDuration = 0.5f;
    [SerializeField] private float counterDuration = 0.5f;
    [SerializeField] private float blockDuration = 1.0f;
    [SerializeField] private float dodgeDistance = 2.0f;
    [SerializeField] private float dodgeDuration = 0.5f;
    [SerializeField] private float dodgeCooldown = 5.0f;
    [SerializeField] private float invincibilityDuration = 0.3f;
    [SerializeField] private float counterWindowDuration = 0.5f;

    [SerializeField] private float maxHP = 100f;
    [SerializeField] private float currentHP;

    // 방어형 고유 특성
    private bool isCounterWindowOpen = false;
    private float counterWindowEndTime = 0f;
    private bool blockSuccessful = false;

    // 상태 관리
    private bool isAttacking = false;
    private bool isBlocking = false;
    private bool isDodging = false;
    private bool isInvincible = false;
    public bool canBeCountered = false;
    public AttackType currentAttackType = AttackType.None;

    // 프로퍼티들
    public bool IsAttacking => isAttacking;
    public bool IsBlocking => isBlocking;
    public bool IsDodging => isDodging;
    public bool IsInvincible => isInvincible;
    public bool CanBeCountered => canBeCountered;
    public AttackType CurrentAttackType => currentAttackType;
    public bool IsCounterWindowOpen => isCounterWindowOpen;
    public float CounterWindowRemaining => Mathf.Max(0f, counterWindowEndTime - Time.time);

    public float CurrentHP => currentHP;
    public float MaxHP => maxHP;
    public float HPPercentage => currentHP / maxHP;
    public bool IsDead => currentHP <= 0f;

    public float AttackCooldownRemaining => Mathf.Max(0f, attackCooldown - (Time.time - lastAttackTime));
    public float BlockCooldownRemaining => Mathf.Max(0f, blockCooldown - (Time.time - lastBlockTime));
    public float DodgeCooldownRemaining => Mathf.Max(0f, dodgeCooldown - (Time.time - lastDodgeTime));

    void Start()
    {
        currentHP = maxHP;
        animator = GetComponent<Animator>();
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

    public bool CanCounter()
    {
        return !IsDead && !isAttacking && !isBlocking && !isDodging &&
               isCounterWindowOpen && Time.time <= counterWindowEndTime &&
               Time.time - lastAttackTime >= attackCooldown;
    }

    // 약공격 (Defender는 강공격 없음)
    public bool WeakAttack()
    {
        if (!CanAttack()) return false;

        StartCoroutine(WeakAttackCoroutine());
        return true;
    }

    // 반격 (Defender 고유 기술)
    public bool Counter()
    {
        if (!CanCounter()) return false;

        StartCoroutine(CounterCoroutine());
        return true;
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

    private IEnumerator WeakAttackCoroutine()
    {
        isAttacking = true;
        canBeCountered = true;
        currentAttackType = AttackType.Weak;

        if (animator) animator.SetTrigger("DoSwing");
        yield return new WaitForSeconds(weakAttackDuration);

        lastAttackTime = Time.time;
        isAttacking = false;
        canBeCountered = false;
        currentAttackType = AttackType.None;
    }

    private IEnumerator CounterCoroutine()
    {
        isAttacking = true;
        currentAttackType = AttackType.Counter;
        isCounterWindowOpen = false; // 반격 기회 소모

        if (animator) animator.SetTrigger("onCounter");
        yield return new WaitForSeconds(counterDuration);

        lastAttackTime = Time.time; // 공격 쿨타임 공유
        isAttacking = false;
        currentAttackType = AttackType.None;
    }

    private IEnumerator BlockCoroutine()
    {
        isBlocking = true;
        if (animator) animator.SetTrigger("onBlock");
        yield return new WaitForSeconds(blockDuration);

        if (blockSuccessful)
        {
            isCounterWindowOpen = true;
            counterWindowEndTime = Time.time + counterWindowDuration;
        }
        blockSuccessful = false;

        lastBlockTime = Time.time;
        isBlocking = false;
    }

    private IEnumerator DodgeCoroutine(Vector3 direction)
    {
        isDodging = true;
        isInvincible = true;

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + direction.normalized * dodgeDistance;

        if (animator) animator.SetTrigger("onDodge");

        float elapsedTime = 0f;
        while (elapsedTime < dodgeDuration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / dodgeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(invincibilityDuration);

        lastDodgeTime = Time.time;
        isDodging = false;
        isInvincible = false;
    }

    public bool TakeDamage(float damage)
    {
        if (IsDead || isInvincible) return false;

        if (isBlocking)
        {
            blockSuccessful = true; // 반격 기회 생성
            Debug.Log($"{gameObject.name} blocked {damage} damage! Counter window opens.");
            return false;
        }

        currentHP = Mathf.Max(0f, currentHP - damage);
        if (IsDead) OnDeath();
        return true;
    }

    private void OnDeath()
    {
        StopAllCoroutines();
        isAttacking = false;
        isBlocking = false;
        isDodging = false;
        isInvincible = false;
        isCounterWindowOpen = false;
    }

    void Update()
    {
        if (isCounterWindowOpen && Time.time > counterWindowEndTime)
        {
            isCounterWindowOpen = false;
        }
    }

    // 이동 관련 (AttackerController와 호환성을 위해)
    public void Move(Vector3 direction)
    {
        if (!isDodging && !isAttacking && !isBlocking)
        {
            transform.Translate(direction * 2.5f * Time.deltaTime);
        }
    }

    public void Stop()
    {
        // 이동 중단 (필요시 구현)
    }
}