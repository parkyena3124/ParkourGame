using UnityEngine;
using System.Collections;

public class LedgeGrab : MonoBehaviour
{
    // =========================
    // Ledge Grab 설정
    // =========================

    public float ledgeGrabDistance = 1.2f;
    public float ledgeGrabHeight = 0.8f;

    public float ledgeWallOffset = 0.45f;
    public float ledgeHangOffset = 0.9f;

    // =========================
    // 필요한 컴포넌트
    // =========================

    public CharacterController controller;
    public PlayerMovement playerMovement;
    public Mantle mantle;

    // =========================
    // Ledge Grab 상태
    // =========================

    public bool isLedgeGrabbing = false;

    RaycastHit currentLedgeHit;

    // =========================
    // Ledge Grab 입력 처리
    // =========================

    public void UpdateLedgeGrab()
    {
        // 이미 난간을 잡고 있다면
        if (isLedgeGrabbing)
        {
            playerMovement.velocity.y = 0f;

            // Space = Mantle
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isLedgeGrabbing = false;

                mantle.StartMantle(
                    currentLedgeHit
                );
            }

            // S = 떨어지기
            else if (Input.GetKeyDown(KeyCode.S))
            {
                DropLedge();
            }

            return;
        }

        // =========================
        // Ledge Grab 가능한지 검사
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
}