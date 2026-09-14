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

    bool wallJumpUsed = false;

    // -------------------------
    // Wall Run 설정
    // -------------------------

    public float wallRunGravity = -1.5f;
    public float wallRunDuration = 1.2f;

    bool isWallRunning = false;
    bool wallRunUsed = false;

    float wallRunTimer = 0f;

    // -------------------------
    // Ledge Grab 설정
    // -------------------------

    public float ledgeGrabDistance = 1.2f;
    public float ledgeGrabHeight = 0.8f;

    public float ledgeWallOffset = 0.45f;
    public float ledgeHangOffset = 0.9f;

    bool isLedgeGrabbing = false;

    RaycastHit currentLedgeHit;

    void Update()
    {
        // =========================
        // 착지하면 초기화
        // =========================

        if (playerMovement.isGrounded)
        {
            wallJumpUsed = false;
            wallRunUsed = false;
        }

        // =========================
        // Ledge Grab 중
        // =========================

        if (isLedgeGrabbing)
        {
            playerMovement.velocity.y = 0f;

            // Space = Mantle
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isLedgeGrabbing = false;

                StartCoroutine(
                    Mantle(currentLedgeHit)
                );
            }

            // S = 떨어지기
            else if (Input.GetKeyDown(KeyCode.S))
            {
                DropLedge();
            }

            return;
        }

        // Vault / Mantle 중이면
        // 다른 파쿠르 검사 안 함
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
            transform.forward * detectDistance,
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
        // 좌우 벽 감지
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

        // 벽에서 떨어지면
        // Wall Run 다시 사용 가능
        if (!wallDetected)
        {
            wallRunUsed = false;
        }

        playerMovement.blockNormalJump = false;

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

        // =========================
        // Wall Run
        // =========================

        bool movingForward =
            Input.GetAxis("Vertical") > 0.1f;

        bool canWallRun =
            wallDetected &&
            !playerMovement.isGrounded &&
            movingForward &&
            !wallJumpUsed &&
            !wallRunUsed;

        if (canWallRun)
        {
            if (!isWallRunning)
            {
                isWallRunning = true;

                wallRunTimer =
                    wallRunDuration;
            }

            wallRunTimer -=
                Time.deltaTime;

            if (wallRunTimer > 0f)
            {
                playerMovement.velocity.y =
                    wallRunGravity;
            }
            else
            {
                isWallRunning = false;
                wallRunUsed = true;
            }
        }
        else
        {
            isWallRunning = false;
        }

        // =========================
        // Ledge Grab 감지
        // =========================

        bool falling =
            playerMovement.velocity.y < -0.5f;

        bool movingTowardWall =
            Input.GetAxis("Vertical") > 0.1f;

        if (!playerMovement.isGrounded &&
            falling &&
            movingTowardWall)
        {
            CheckLedgeGrab();
        }
    }

    // =========================
    // Ledge Grab 감지
    // =========================

    void CheckLedgeGrab()
    {
        Vector3 ledgeRayStart =
            transform.position +
            Vector3.up *
            ledgeGrabHeight;

        Debug.DrawRay(
            ledgeRayStart,
            transform.forward *
            ledgeGrabDistance,
            Color.green
        );

        if (Physics.Raycast(
            ledgeRayStart,
            transform.forward,
            out RaycastHit ledgeHit,
            ledgeGrabDistance))
        {
            float obstacleTop =
                ledgeHit.collider.bounds.max.y;

            float heightDifference =
                obstacleTop -
                transform.position.y;

            // 너무 가까운 높이 또는
            // 너무 높은 난간은 잡지 않음
            if (heightDifference > 0.4f &&
                heightDifference <= 1.2f)
            {
                StartCoroutine(
                    SmoothGrabLedge(
                        ledgeHit,
                        obstacleTop
                    )
                );
            }
        }
    }

    // =========================
    // Ledge Grab 시작
    // =========================

    IEnumerator SmoothGrabLedge(
        RaycastHit ledgeHit,
        float obstacleTop)
    {
        isLedgeGrabbing = true;

        currentLedgeHit =
            ledgeHit;

        playerMovement.enabled = false;
        playerMovement.velocity.y = 0f;

        Vector3 startPosition =
            transform.position;

        Vector3 wallNormal =
            ledgeHit.normal;

        Vector3 targetPosition =
            ledgeHit.collider.bounds.center +
            wallNormal *
            ledgeWallOffset;

        targetPosition.y =
            obstacleTop -
            ledgeHangOffset;

        float grabDuration = 0.15f;

        float time = 0f;

        while (time < grabDuration)
        {
            time += Time.deltaTime;

            float progress =
                time / grabDuration;

            Vector3 newPosition =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    progress
                );

            controller.Move(
                newPosition -
                transform.position
            );

            yield return null;
        }
    }

    // =========================
    // Ledge Grab 놓기
    // =========================

    void DropLedge()
    {
        isLedgeGrabbing = false;

        // 벽에서 살짝 떨어짐
        controller.Move(
            -transform.forward * 0.3f
        );

        playerMovement.enabled = true;

        playerMovement.velocity.y = -2f;
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
            time += Time.deltaTime;

            float progress =
                time / vaultDuration;

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

        playerMovement.velocity.y = -2f;

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
        // 1단계 - 위로
        // -------------------------

        float time = 0f;

        while (time < halfDuration)
        {
            time += Time.deltaTime;

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
        // 2단계 - 앞으로
        // -------------------------

        time = 0f;

        while (time < halfDuration)
        {
            time += Time.deltaTime;

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

        playerMovement.velocity.y = -2f;

        playerMovement.enabled = true;

        isMantling = false;
    }

    // =========================
    // Wall Jump
    // =========================

    void WallJump(RaycastHit wallHit)
    {
        wallJumpUsed = true;

        Vector3 wallNormal =
            wallHit.normal;

        playerMovement.velocity.y = 0f;

        playerMovement.velocity.y =
            wallJumpUpForce;

        controller.Move(
            wallNormal *
            wallJumpSideForce *
            Time.deltaTime
        );
    }
}