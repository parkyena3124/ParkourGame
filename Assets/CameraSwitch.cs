using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    public Transform firstPersonPoint;
    public Transform thirdPersonPoint;
    public Transform player;

    public MouseLook mouseLook;

    public KeyCode switchKey = KeyCode.V;

    public float thirdPersonSensitivity = 150f;
    public float thirdPersonDistance = 4f;
    public float thirdPersonHeight = 1.2f;

    bool isThirdPerson = false;

    float yaw = 0f;
    float pitch = 15f;

    void Start()
    {
        // 시작은 1인칭
        isThirdPerson = false;

        if (mouseLook != null)
        {
            mouseLook.enabled = true;
        }
    }

    void Update()
    {
        // -------------------------
        // 1인칭 / 3인칭 전환
        // -------------------------

        if (Input.GetKeyDown(switchKey))
        {
            isThirdPerson = !isThirdPerson;

            if (mouseLook != null)
            {
                // 1인칭일 때만 MouseLook 사용
                mouseLook.enabled = !isThirdPerson;
            }

            if (isThirdPerson)
            {
                yaw = player.eulerAngles.y;
            }
        }

        // -------------------------
        // 3인칭 마우스 입력
        // -------------------------

        if (isThirdPerson)
        {
            float mouseX =
                Input.GetAxis("Mouse X") *
                thirdPersonSensitivity *
                Time.deltaTime;

            float mouseY =
                Input.GetAxis("Mouse Y") *
                thirdPersonSensitivity *
                Time.deltaTime;

            yaw += mouseX;
            pitch -= mouseY;

            pitch = Mathf.Clamp(
                pitch,
                -60f,
                75f
            );

            // 플레이어 좌우 회전
            player.rotation =
                Quaternion.Euler(
                    0f,
                    yaw,
                    0f
                );
        }
    }

    void LateUpdate()
    {
        if (isThirdPerson)
        {
            ThirdPersonCamera();
        }
        else
        {
            FirstPersonCamera();
        }
    }

    // =========================
    // 1인칭
    // =========================

    void FirstPersonCamera()
    {
        // 위치만 1인칭 위치로 이동
        // 회전은 MouseLook이 담당
        transform.position =
            firstPersonPoint.position;
    }

    // =========================
    // 3인칭
    // =========================

    void ThirdPersonCamera()
    {
        Vector3 target =
            player.position +
            Vector3.up *
            thirdPersonHeight;

        Quaternion rotation =
            Quaternion.Euler(
                pitch,
                yaw,
                0f
            );

        Vector3 offset =
            rotation *
            new Vector3(
                0f,
                0f,
                -thirdPersonDistance
            );

        transform.position =
            target + offset;

        transform.LookAt(target);
    }
}