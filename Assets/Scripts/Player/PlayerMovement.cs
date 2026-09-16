using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    // -------------------------
    // 이동 설정
    // -------------------------

    public float speed = 5f;
    public float sprintSpeed = 9f;

    // -------------------------
    // 점프 / 중력
    // -------------------------

    public float gravity = -9.81f;
    public float jumpHeight = 1.2f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    // 다른 스크립트에서도 확인할 수 있도록 public
    public bool isGrounded;

    // Wall Jump에서 사용할 수 있도록 public
    public Vector3 velocity;

    // Wall Jump 가능한 상황에서
    // 일반 점프가 동시에 실행되는 것을 막음
    public bool blockNormalJump = false;

    // -------------------------
    // 웅크리기
    // -------------------------

    public float standingHeight = 2f;
    public float crouchingHeight = 1f;

    public Transform playerCamera;

    public float standingCameraHeight = 0.7f;
    public float crouchingCameraHeight = 0.2f;
    public float crouchSpeed = 10f;

    // -------------------------
    // 슬라이드
    // -------------------------

    public float slideSpeed = 14f;
    public float slideDuration = 0.8f;

    bool isSliding = false;

    float slideTimer = 0f;
    Vector3 slideDirection;

    void Update()
    {
        // =========================
        // 바닥 감지
        // =========================

        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundDistance,
            groundMask
        );
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // =========================
        // 웅크리기
        // =========================

        if (Input.GetKey(KeyCode.LeftControl))
        {
            controller.height =
                crouchingHeight;

            Vector3 cameraPos =
                playerCamera.localPosition;

            cameraPos.y = Mathf.Lerp(
                cameraPos.y,
                crouchingCameraHeight,
                crouchSpeed * Time.deltaTime
            );

            playerCamera.localPosition =
                cameraPos;
        }
        else
        {
            controller.height =
                standingHeight;

            Vector3 cameraPos =
                playerCamera.localPosition;

            cameraPos.y = Mathf.Lerp(
                cameraPos.y,
                standingCameraHeight,
                crouchSpeed * Time.deltaTime
            );

            playerCamera.localPosition =
                cameraPos;
        }

        // =========================
        // WASD 이동
        // =========================

        float x =
            Input.GetAxis("Horizontal");

        float z =
            Input.GetAxis("Vertical");

        Vector3 move =
            transform.right * x +
            transform.forward * z;

        // =========================
        // 달리기
        // =========================

        float currentSpeed =
            speed;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed =
                sprintSpeed;
        }

        // =========================
        // 슬라이드 시작
        // =========================

        if (Input.GetKey(KeyCode.LeftShift) &&
            Input.GetKeyDown(KeyCode.LeftControl) &&
            !isSliding)
        {
            isSliding = true;

            slideTimer =
                slideDuration;

            slideDirection =
                transform.forward;
        }

        // =========================
        // 슬라이드 이동
        // =========================

        if (isSliding)
        {
            slideTimer -=
                Time.deltaTime;

            float slidePower =
                slideSpeed *
                (slideTimer / slideDuration);

            controller.Move(
                slideDirection *
                slidePower *
                Time.deltaTime
            );

            if (slideTimer <= 0f)
            {
                isSliding = false;
            }
        }

        // =========================
        // 일반 이동
        // =========================

        if (!isSliding)
        {
            controller.Move(
                move *
                currentSpeed *
                Time.deltaTime
            );
        }

        // =========================
        // 일반 점프
        // =========================

        if (Input.GetButtonDown("Jump") &&
            isGrounded &&
            !blockNormalJump)
        {
            velocity.y =
                Mathf.Sqrt(
                    jumpHeight *
                    -2f *
                    gravity
                );
        }

        // =========================
        // 중력
        // =========================

        velocity.y +=
            gravity *
            Time.deltaTime;

        controller.Move(
            velocity *
            Time.deltaTime
        );
    }
}