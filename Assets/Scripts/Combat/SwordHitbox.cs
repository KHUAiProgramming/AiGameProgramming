using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damage = 30f; // 25f → 30f로 증가

    // 컴포넌트
    private Collider hitboxCollider;

    // 상태
    private bool isActive = false;
    private bool hasHit = false; // 한 번의 공격에 한 번만 히트

    // 프로퍼티
    public bool IsActive => isActive;
    public float Damage => damage;

    void Awake()
    {
        hitboxCollider = GetComponent<Collider>();
        if (hitboxCollider == null)
        {
            Debug.LogError($"SwordHitbox on {gameObject.name} requires a Collider component!");
        }
        else
        {
            // Trigger로 설정
            hitboxCollider.isTrigger = true;
        }

        // 시작할 때는 비활성화
        DisableHitbox();
    }

    void Start()
    {
        // 컴포넌트 검증
        if (hitboxCollider != null && !hitboxCollider.isTrigger)
        {
            Debug.LogWarning($"SwordHitbox Collider on {gameObject.name} should be set as Trigger!");
            hitboxCollider.isTrigger = true;
        }
    }

    public void EnableHitbox()
    {
        if (!isActive)
        {
            isActive = true;
            hasHit = false; // 새로운 공격 시작시 히트 상태 초기화

            if (hitboxCollider != null)
                hitboxCollider.enabled = true;

            Debug.Log($"SwordHitbox {gameObject.name} enabled");
        }
    }

    public void DisableHitbox()
    {
        if (isActive)
        {
            isActive = false;

            if (hitboxCollider != null)
                hitboxCollider.enabled = false;

            Debug.Log($"SwordHitbox {gameObject.name} disabled");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"OnTriggerEnter with: {other.name} (Layer: {other.gameObject.layer})");
        if (!isActive) return;
        if (hasHit) return; // 이미 한 번 히트했으면 더 이상 히트하지 않음

        // 자기 자신은 제외
        if (other.transform.root == transform.root) return;

        Debug.Log($"SwordHitbox trigger with: {other.name} (Layer: {other.gameObject.layer})");

        // 공격자 찾기 (이 검을 휘두른 사람)
        string attackerName = transform.root.name;

        // AttackerController 먼저 찾기
        AttackerController attackerController = other.GetComponent<AttackerController>();
        if (attackerController == null)
        {
            attackerController = other.GetComponentInParent<AttackerController>();
        }
        if (attackerController != null)
        {
            Debug.Log($"Found AttackerController on {other.name}");

            // 방어 중인지 체크
            bool wasBlocking = attackerController.IsBlocking;

            // 데미지 받음 이벤트 발생 (전투에 영향 없음)
            CombatEvents.OnDamageTaken?.Invoke(other.name, damage);

            attackerController.TakeDamage(damage);
            hasHit = true;

            // 공격 성공 이벤트 발생 (전투에 영향 없음)
            CombatEvents.OnAttackAttempt?.Invoke(attackerName, true, damage);

            // 방어 실패 이벤트 발생 (만약 방어 중이었다면) (전투에 영향 없음)
            if (wasBlocking)
            {
                CombatEvents.OnBlockAttempt?.Invoke(other.name, false);
            }

            return;
        }

        // DefenderController 찾기
        DefenderController defenderController = other.GetComponent<DefenderController>();
        if (defenderController == null)
        {
            defenderController = other.GetComponentInParent<DefenderController>();
        }
        if (defenderController != null)
        {
            Debug.Log($"Found DefenderController on {other.name}");

            // 방어 중인지 체크
            bool wasBlocking = defenderController.IsBlocking;

            // 데미지 받음 이벤트 발생 (전투에 영향 없음)
            CombatEvents.OnDamageTaken?.Invoke(other.name, damage);

            defenderController.TakeDamage(damage);
            hasHit = true;

            // 공격 성공 이벤트 발생 (전투에 영향 없음)
            CombatEvents.OnAttackAttempt?.Invoke(attackerName, true, damage);

            // 방어 실패 이벤트 발생 (만약 방어 중이었다면) (전투에 영향 없음)
            if (wasBlocking)
            {
                CombatEvents.OnBlockAttempt?.Invoke(other.name, false);
            }

            return;
        }

        Debug.Log($"No controller found on {other.name}");
    }

    // 데미지 설정 (런타임에서 변경 가능)
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }
}