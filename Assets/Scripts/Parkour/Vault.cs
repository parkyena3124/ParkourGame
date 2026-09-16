using UnityEngine;
using System.Collections;

public class Vault : MonoBehaviour
{
    // =========================
    // Vault 설정
    // =========================

    public float vaultDuration = 0.4f;
    public float vaultHeight = 1.3f;

    // =========================
    // 필요한 컴포넌트
    // =========================

    public CharacterController controller;
    public PlayerMovement playerMovement;

    // =========================
    // Vault 상태
    // =========================

    public bool isVaulting = false;

    // =========================
    // Vault 실행
    // =========================

    public void StartVault(RaycastHit hit)
    {
        if (isVaulting)
        {
            return;
        }

        StartCoroutine(
            VaultMovement(hit)
        );
    }

    // =========================
    // 실제 Vault 이동
    // =========================

    IEnumerator VaultMovement(RaycastHit hit)
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
}
