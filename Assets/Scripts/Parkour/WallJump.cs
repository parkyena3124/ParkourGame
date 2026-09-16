using UnityEngine;

public class WallJump : MonoBehaviour
{
    // =========================
    // Wall Jump 설정
    // =========================

    public float wallJumpUpForce = 6f;
    public float wallJumpSideForce = 6f;

    // =========================
    // 필요한 컴포넌트
    // =========================

    public CharacterController controller;
    public PlayerMovement playerMovement;

    // =========================
    // Wall Jump 상태
    // =========================

    public bool wallJumpUsed = false;

    // =========================
    // Wall Jump 실행
    // =========================

    public void StartWallJump(RaycastHit wallHit)
    {
        // 이미 사용했다면 실행하지 않음
        if (wallJumpUsed)
        {
            return;
        }

        wallJumpUsed = true;

        // 벽의 바깥쪽 방향
        Vector3 wallNormal =
            wallHit.normal;

        // 기존 수직 속도 제거
        playerMovement.velocity.y = 0f;

        // 위쪽으로 점프
        playerMovement.velocity.y =
            wallJumpUpForce;

        // 벽 반대 방향으로 밀어냄
        controller.Move(
            wallNormal *
            wallJumpSideForce *
            Time.deltaTime
        );
    }

    // =========================
    // Wall Jump 초기화
    // =========================

    public void ResetWallJump()
    {
        wallJumpUsed = false;
    }
}