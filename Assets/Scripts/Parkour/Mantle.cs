using UnityEngine;
using System.Collections;

public class Mantle : MonoBehaviour
{
    // =========================
    // Mantle 설정
    // =========================

    public float mantleDuration = 0.6f;
    public float mantleForwardDistance = 1.2f;

    // =========================
    // 필요한 컴포넌트
    // =========================

    public CharacterController controller;
    public PlayerMovement playerMovement;

    // =========================
    // Mantle 상태
    // =========================

    public bool isMantling = false;

    // =========================
    // Mantle 시작
    // =========================

    public void StartMantle(RaycastHit hit)
    {
        if (isMantling)
        {
            return;
        }

        StartCoroutine(
            MantleMovement(hit)
        );
    }

    // =========================
    // 실제 Mantle 이동
    // =========================

    IEnumerator MantleMovement(RaycastHit hit)
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
            time += Time.deltaTime;

            float progress =
                time / halfDuration;

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
            time += Time.deltaTime;

            float progress =
                time / halfDuration;

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
}