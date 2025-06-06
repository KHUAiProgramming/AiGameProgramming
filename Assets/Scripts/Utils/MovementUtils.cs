using UnityEngine;

public static class MovementUtils
{
    /// <summary>
    /// 4방향 이동을 위한 MoveDirection enum 변환
    /// 계단식 움직임을 방지하기 위해 한 번 정해진 방향을 유지
    /// </summary>
    public static AttackerController.MoveDirection GetMoveDirection(Vector3 direction)
    {
        if (direction.magnitude < 0.1f) return AttackerController.MoveDirection.None;

        // 더 큰 축을 우선으로 선택 (계단식 움직임 방지)
        float absX = Mathf.Abs(direction.x);
        float absZ = Mathf.Abs(direction.z);

        if (absX > absZ)
        {
            return direction.x > 0 ? AttackerController.MoveDirection.Right : AttackerController.MoveDirection.Left;
        }
        else
        {
            return direction.z > 0 ? AttackerController.MoveDirection.Forward : AttackerController.MoveDirection.Backward;
        }
    }
}