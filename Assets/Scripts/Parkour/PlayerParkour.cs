using UnityEngine;

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
    public Vault vault;

    // -------------------------
    // Mantle 설정
    public Mantle mantle;
    public float mantleReachHeight = 1.4f; //Mantle 가능한 높이 판단용

    // -------------------------
    // Wall Jump 설정
    public WallJump wallJump;
    public float wallCheckDistance = 0.8f;


    // -------------------------
    // Wall Run 설정
    public WallRun wallRun;

    // -------------------------
    // Ledge Grab 설정
    public LedgeGrab ledgeGrab;

    void Update()
    {
        // =========================
        // 착지하면 초기화
        // =========================

        if (playerMovement.isGrounded)
        {
            wallJump.ResetWallJump();
            wallRun.ResetWallRun();
        }

        // =========================
        // Ledge Grab
        // =========================

        ledgeGrab.UpdateLedgeGrab();

        if (ledgeGrab.isLedgeGrabbing)
        {
            return;
        }

        // Vault / Mantle 중이면
        // 다른 파쿠르 검사 안 함
        if (vault.isVaulting || mantle.isMantling)
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
                    vault.StartVault(hit);
                
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
                    mantle.StartMantle(hit);
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
            wallRun.ResetWallRunUsed();
        }

        playerMovement.blockNormalJump = false;

        // =========================
        // Wall Jump
        // =========================

        if (wallDetected &&
            !playerMovement.isGrounded &&
            !wallJump.wallJumpUsed)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                wallJump.StartWallJump(wallHit);
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
            !wallJump.wallJumpUsed &&
            !wallRun.wallRunUsed;

        if (canWallRun)
        {
            if (!wallRun.isWallRunning)
            {
                wallRun.StartWallRun();
            }

            wallRun.UpdateWallRun();
        }
        else
        {
            wallRun.StopWallRun();
        }

    }
}