using UnityEngine;
using System.Collections;

public class PlayerParkour : MonoBehaviour
{
    // -------------------------
    // 장애물 감지
    // -------------------------

    public float detectDistance = 1.5f;
    public float maxVaultHeight = 1.2f;

    // -------------------------
    // 플레이어 컴포넌트
    // -------------------------

    public CharacterController controller;
    public PlayerMovement playerMovement;

    // -------------------------
    // Vault 설정
    // -------------------------

    public float vaultDuration = 0.4f;
    public float vaultHeight = 1.3f;

    bool isVaulting = false;

    // -------------------------
    // Mantle 설정
    // -------------------------

    public float mantleDuration = 0.6f;
    public float mantleForwardDistance = 1.2f;
    public float mantleReachHeight = 1.4f;

    bool isMantling = false;

    // -------------------------
    // Wall Jump 설정
    // -------------------------

    public float wallCheckDistance = 0.8f;
    public float wallJumpUpForce = 6f;
    public float wallJumpSideForce = 6f;

    // Wall Jump를 이미 사용했는지 저장
    bool wallJumpUsed = false;

    void Update()
    {
        // =========================
        // 착지하면 Wall Jump 초기화
        // =========================

        if (playerMovement.isGrounded)
        {
            wallJumpUsed = false;
        }

        // Vault 또는 Mantle 중이면
        // 새로운 파쿠르 동작 검사 안 함
        if (isVaulting || isMantling)
        {
            return;
        }

        // =========================
        // 앞 장애물 감지
        // Vault / Mantle
        // =========================

        Vector3 rayStart =
            transform.position +
            Vector3.down * 0.7f;

        Ray ray =
            new Ray(
                rayStart,
                transform.forward
            );

        Debug.DrawRay(
            rayStart,
            transform.forward *
            detectDistance,
            Color.red
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            detectDistance))
        {
            float obstacleTop =
                hit.collider.bounds.max.y;

            float playerFeet =
                transform.position.y - 1f;

            float obstacleHeight =
                obstacleTop -
                playerFeet;

            float ledgeDistance =
                obstacleTop -
                transform.position.y;

            // -------------------------
            // Vault
            // -------------------------

            if (obstacleHeight <= maxVaultHeight)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    StartCoroutine(
                        Vault(hit)
                    );
                }
            }

            // -------------------------
            // Mantle
            // -------------------------

            else if (
                ledgeDistance <=
                mantleReachHeight)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    StartCoroutine(
                        Mantle(hit)
                    );
                }
            }
        }

        // =========================
        // Wall Jump 벽 감지
        // =========================

        RaycastHit wallHit;

        bool wallDetected =
            Physics.Raycast(
                transform.position,
                transform.right,
                out wallHit,
                wallCheckDistance
            )
            ||
            Physics.Raycast(
                transform.position,
                -transform.right,
                out wallHit,
                wallCheckDistance
            );

        // 벽 옆에서는 일반 점프와
        // Wall Jump가 동시에 실행되지 않도록 함
        if (playerMovement.isGrounded)
        {
            playerMovement.blockNormalJump = false;
        }
        else
        {
            playerMovement.blockNormalJump = wallDetected;
        }

        // =========================
        // Wall Jump
        // =========================

        if (wallDetected &&
            !playerMovement.isGrounded &&
            !wallJumpUsed)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                WallJump(wallHit);
            }
        }
    }

    // =========================
    // Vault
    // =========================

    IEnumerator Vault(RaycastHit hit)
    {
        isVaulting = true;

        playerMovement.enabled = false;

        Vector3 startPosition =
            transform.position;

        Vector3 obstacleCenter =
            hit.collider.bounds.center;

        Vector3 obstacleSize =
            hit.collider.bounds.size;

        Vector3 obstacleBack =
            obstacleCenter +
            transform.forward *
            (obstacleSize.z / 2f);

        Vector3 endPosition =
            obstacleBack +
            transform.forward * 0.8f;

        endPosition.y =
            startPosition.y;

        float time = 0f;

        while (time < vaultDuration)
        {
            time +=
                Time.deltaTime;

            float progress =
                time /
                vaultDuration;

            Vector3 position =
                Vector3.Lerp(
                    startPosition,
                    endPosition,
                    progress
                );

            position.y +=
                Mathf.Sin(
                    progress *
                    Mathf.PI
                ) *
                vaultHeight;

            Vector3 moveAmount =
                position -
                transform.position;

            controller.Move(
                moveAmount
            );

            yield return null;
        }

        playerMovement.enabled = true;

        isVaulting = false;
    }

    // =========================
    // Mantle
    // =========================

    IEnumerator Mantle(RaycastHit hit)
    {
        isMantling = true;

        playerMovement.enabled = false;

        Vector3 startPosition =
            transform.position;

        float obstacleTop =
            hit.collider.bounds.max.y;

        float playerHalfHeight =
            controller.height / 2f;

        float targetY =
            obstacleTop +
            playerHalfHeight;

        Vector3 upPosition =
            new Vector3(
                startPosition.x,
                targetY,
                startPosition.z
            );

        Vector3 endPosition =
            upPosition +
            transform.forward *
            mantleForwardDistance;

        float halfDuration =
            mantleDuration / 2f;

        // -------------------------
        // 1단계 - 위로 올라가기
        // -------------------------

        float time = 0f;

        while (time < halfDuration)
        {
            time +=
                Time.deltaTime;

            float progress =
                time /
                halfDuration;

            Vector3 targetPosition =
                Vector3.Lerp(
                    startPosition,
                    upPosition,
                    progress
                );

            controller.Move(
                targetPosition -
                transform.position
            );

            yield return null;
        }

        // -------------------------
        // 2단계 - 앞으로 올라서기
        // -------------------------

        time = 0f;

        while (time < halfDuration)
        {
            time +=
                Time.deltaTime;

            float progress =
                time /
                halfDuration;

            Vector3 targetPosition =
                Vector3.Lerp(
                    upPosition,
                    endPosition,
                    progress
                );

            controller.Move(
                targetPosition -
                transform.position
            );

            yield return null;
        }

        // Mantle 중 남아 있던 수직 속도 제거
        playerMovement.velocity.y = -2f;

        playerMovement.enabled = true;

        isMantling = false;
    }

    // =========================
    // Wall Jump
    // =========================

    void WallJump(RaycastHit wallHit)
    {
        // 이번 공중 상태에서는
        // Wall Jump를 사용했다고 기록
        wallJumpUsed = true;

        // 벽 표면의 바깥 방향
        Vector3 wallNormal =
            wallHit.normal;

        // 기존 위/아래 속도 초기화
        playerMovement.velocity.y = 0f;

        // 위쪽으로 점프
        playerMovement.velocity.y =
            wallJumpUpForce;

        // 벽 반대쪽으로 밀어냄
        controller.Move(
            wallNormal *
            wallJumpSideForce *
            Time.deltaTime
        );
    }
}