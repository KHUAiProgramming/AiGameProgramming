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

    [SerializeField] private Animator animator;
    // [SerializeField] private float attackCooldown = 2.5f;
    [SerializeField] private float attackCooldown = 0.0f;
    [SerializeField] private float blockCooldown = 5.0f;
    [SerializeField] private float lastAttackTime = -111111.0f;
    [SerializeField] private float lastWeakAttackTime = -111111.0f;
    // [SerializeField] private float blockCoolDown = 5.0f;
    [SerializeField] private float strongAttackDuration = 2f;
    [SerializeField] private float weakAttackDuration = 0.5f;

    private MoveDirection currentDirection = MoveDirection.None;

    private SwordHitbox swordHitbox;


    public float AttackCoolDown => attackCooldown;
    public float BlockCoolDown => blockCooldown;

    private bool isAttacking = false;
    private bool isBlocking = false;
    [SerializeField] private float dodgeDistance = 2.0f;
    [SerializeField] private float dodgeDuration = 0.5f;
    [SerializeField] private float invincibilityDuration = 0.3f; // 무적 지속시간

    private bool isInvincible = false; // 무적 상태 여부

    // 무적 상태 확인용 프로퍼티
    public bool IsInvincible => isInvincible;
    void Start()
    {
        animator = GetComponent<Animator>();
        swordHitbox = GetComponentInChildren<SwordHitbox>();
    }

    public void Move(Vector3 direction)
    {
        if (isAttacking || isBlocking)
        {
            return;
        }
        MoveDirection _direction = GetClosestDirection(direction);
        transform.LookAt(transform.position + direction);
        SetMoveDirection(_direction);
    }
    public void Stop()
    {
        SetMoveDirection(MoveDirection.None);
    }

    // 앞뒤좌우만 움직이게 방향 4개 중에 가장 근접한 방향 하나로 지정
    private MoveDirection GetClosestDirection(Vector3 moveVector)
    {
        if (moveVector.magnitude < 0.1f) return MoveDirection.None;

        float absX = Mathf.Abs(moveVector.x);
        float absZ = Mathf.Abs(moveVector.z);

        if (absX > absZ)
        {
            return moveVector.x > 0 ? MoveDirection.Right : MoveDirection.Left;
        }
        else
        {
            return moveVector.z > 0 ? MoveDirection.Forward : MoveDirection.Backward;
        }
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
        return !isAttacking && !isBlocking &&
               Time.time - lastAttackTime >= attackCooldown;
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

    private IEnumerator StrongAttackCoroutine()
    {
        isAttacking = true;
        Stop(); // 이동 중단


        swordHitbox.EnableHitbox(); // collider 활성화
        animator.SetTrigger("onAttack");
        yield return new WaitForSeconds(strongAttackDuration);
        swordHitbox.DisableHitbox();

        lastAttackTime = Time.time;
        isAttacking = false;
    }

    private IEnumerator WeakAttackCoroutine()
    {
        isAttacking = true;
        Stop(); // 이동 중단

        Debug.Log("Strong Attack Coroutine Started");
        swordHitbox.EnableHitbox(); // collider enable
        animator.SetTrigger("onWeakAttack");
        yield return new WaitForSeconds(weakAttackDuration);
        swordHitbox.DisableHitbox(); // disable

        lastAttackTime = Time.time;
        isAttacking = false;
    }

    public bool IsMoving() => currentDirection != MoveDirection.None;
    public bool IsAttacking() => isAttacking;
    public bool IsBlocking() => isBlocking;
    public bool IsBusy() => isAttacking || isBlocking;
    public MoveDirection GetCurrentDirection() => currentDirection;
}