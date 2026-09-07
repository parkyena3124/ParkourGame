using UnityEngine;
using System.Collections;

public class PlayerParkour : MonoBehaviour
{
    // -------------------------
    // 장애물 감지
    // -------------------------

    public float detectDistance = 1.5f;

    public float maxVaultHeight = 1.2f;
    public float maxMantleHeight = 2.2f;

    // -------------------------
    // 플레이어 컴포넌트
    // -------------------------

    public CharacterController controller;
    public PlayerMovement playerMovement;

    // -------------------------
    // Vault 설정
    // -------------------------

    public float vaultDuration = 0.4f;
    public float vaultForwardDistance = 2.4f;
    public float vaultHeight = 1.3f;

    bool isVaulting = false;

    // -------------------------
    // Mantle 설정
    // -------------------------

    public float mantleDuration = 0.6f;
    public float mantleForwardDistance = 1.2f;

    bool isMantling = false;

    void Update()
    {
        // Vault 중이거나 Mantle 중에는 새로운 파쿠르 검사 안 함
        if (isVaulting || isMantling)
        {
            return;
        }

        // 플레이어보다 조금 아래에서 Ray 시작
        Vector3 rayStart =
            transform.position +
            Vector3.down * 0.7f;

        Ray ray =
            new Ray(
                rayStart,
                transform.forward
            );

        // Scene에서 빨간 Ray 표시
        Debug.DrawRay(
            rayStart,
            transform.forward * detectDistance,
            Color.red
        );

        // 앞에 장애물이 있는지 검사
        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            detectDistance))
        {
            // 장애물 맨 위 Y 위치
            float obstacleTop =
                hit.collider.bounds.max.y;

            // 플레이어 발 위치
            float playerFeet =
                transform.position.y - 1f;

            // 장애물 높이 계산
            float obstacleHeight =
                obstacleTop -
                playerFeet;

            Debug.Log(
                "장애물 높이: " +
                obstacleHeight
            );

            // -------------------------
            // Vault
            // -------------------------

            if (obstacleHeight <= maxVaultHeight)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    StartCoroutine(Vault(hit));
                }
            }

            // -------------------------
            // Mantle
            // -------------------------

            else if (obstacleHeight <= maxMantleHeight)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Debug.Log("Mantle 시작!");
                    StartCoroutine(Mantle(hit));
                }
            }

            // -------------------------
            // 너무 높은 벽
            // -------------------------

            else
            {
                Debug.Log("너무 높음");
            }
        }
    }

    // =========================
    // Vault
    // =========================

    IEnumerator Vault(RaycastHit hit)
    {
        isVaulting = true;

        // Vault 중에는 일반 이동 / 점프 / 중력 정지
        playerMovement.enabled = false;

        Vector3 startPosition =
            transform.position;

        Vector3 obstacleCenter =
     hit.collider.bounds.center;

        Vector3 obstacleSize =
            hit.collider.bounds.size;

        // 장애물 반대편 끝 위치
        Vector3 obstacleBack =
            obstacleCenter +
            transform.forward *
            (obstacleSize.z / 2f);

        // 장애물을 완전히 지난 뒤 착지할 위치
        Vector3 endPosition =
            obstacleBack +
            transform.forward * 0.8f;

        // 높이는 시작 위치와 같게
        endPosition.y =
            startPosition.y;

        float time = 0f;

        while (time < vaultDuration)
        {
            time += Time.deltaTime;

            float progress =
                time / vaultDuration;

            // 앞으로 이동
            Vector3 position =
                Vector3.Lerp(
                    startPosition,
                    endPosition,
                    progress
                );

            // 위로 올라갔다 내려오는 곡선
            position.y +=
                Mathf.Sin(
                    progress *
                    Mathf.PI
                ) * vaultHeight;

            // 현재 위치에서 목표 위치까지 이동
            Vector3 moveAmount =
                position -
                transform.position;

            controller.Move(moveAmount);

            // 다음 프레임까지 대기
            yield return null;
        }

        // 일반 이동 다시 활성화
        playerMovement.enabled = true;

        isVaulting = false;
    }



    IEnumerator Mantle(RaycastHit hit)
    {
        isMantling = true;

        // 일반 이동 / 점프 / 중력 잠시 중지
        playerMovement.enabled = false;

        Vector3 startPosition = transform.position;

        // 장애물의 가장 높은 위치
        float obstacleTop =
            hit.collider.bounds.max.y;

        // 플레이어 CharacterController의 절반 높이
        float playerHalfHeight =
            controller.height / 2f;

        // 벽 위에 섰을 때 플레이어 중심의 Y 위치
        float targetY =
            obstacleTop + playerHalfHeight;

        // 먼저 벽 위 높이까지 올라감
        Vector3 upPosition =
            new Vector3(
                startPosition.x,
                targetY,
                startPosition.z
            );

        // 벽 위에서 살짝 안쪽으로 이동
        Vector3 endPosition =
            upPosition +
            transform.forward * mantleForwardDistance;

        float halfDuration = mantleDuration / 2f;

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
                targetPosition - transform.position
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
                targetPosition - transform.position
            );

            yield return null;
        }

        // 일반 이동 다시 활성화
        playerMovement.enabled = true;

        isMantling = false;
    }
}