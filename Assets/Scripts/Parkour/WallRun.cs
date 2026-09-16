using UnityEngine;

public class WallRun : MonoBehaviour
{
    // =========================
    // Wall Run 설정
    // =========================

    public float wallRunGravity = -1.5f;
    public float wallRunDuration = 1.2f;

    // =========================
    // 필요한 컴포넌트
    // =========================

    public PlayerMovement playerMovement;

    // =========================
    // Wall Run 상태
    // =========================

    public bool isWallRunning = false;
    public bool wallRunUsed = false;

    float wallRunTimer = 0f;

    // =========================
    // Wall Run 실행
    // =========================

    public void StartWallRun()
    {
        // 이미 이번 공중 상태에서 사용했다면 실행하지 않음
        if (wallRunUsed)
        {
            return;
        }

        isWallRunning = true;

        wallRunTimer =
            wallRunDuration;
    }

    // =========================
    // Wall Run 유지
    // =========================

    public void UpdateWallRun()
    {
        if (!isWallRunning)
        {
            return;
        }

        wallRunTimer -=
            Time.deltaTime;

        // 떨어지는 속도를 느리게 만듦
        if (playerMovement.velocity.y < wallRunGravity)
        {
            playerMovement.velocity.y =
                wallRunGravity;
        }

        // 시간이 끝나면 Wall Run 종료
        if (wallRunTimer <= 0f)
        {
            isWallRunning = false;
            wallRunUsed = true;
        }
    }

    // =========================
    // Wall Run 종료
    // =========================

    public void StopWallRun()
    {
        isWallRunning = false;
    }

    // =========================
    // Wall Run 초기화
    // =========================

    public void ResetWallRun()
    {
        isWallRunning = false;
        wallRunUsed = false;
        wallRunTimer = 0f;
    }

    // =========================
    // Wall Run 사용 여부만 초기화
    // =========================

    public void ResetWallRunUsed()
    {
        wallRunUsed = false;
    }
}