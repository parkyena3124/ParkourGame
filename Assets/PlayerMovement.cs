using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    public float speed = 5f;
    public float sprintSpeed = 9f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;
    public float standingHeight = 2f;
    public float crouchingHeight = 1f;

    public Transform playerCamera;
    public float standingCameraHeight = 0.7f;
    public float crouchingCameraHeight = 0.2f;
    public float crouchSpeed = 10f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    public float slideSpeed = 14f;
    public float slideDuration = 0.8f;

    bool isSliding = false;
    float slideTimer = 0f;
    Vector3 slideDirection;

    Vector3 velocity;
    bool isGrounded;

    void Update()
    {
        // 발밑에 Ground가 있는지 확인
        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundDistance,
            groundMask
        );

        // 땅에 붙어있게 하기
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            controller.height = crouchingHeight;

            Vector3 cameraPos = playerCamera.localPosition;
            cameraPos.y = Mathf.Lerp(
                cameraPos.y,
                crouchingCameraHeight,
                crouchSpeed * Time.deltaTime
            );

            playerCamera.localPosition = cameraPos;
        }
        else
        {
            controller.height = standingHeight;

            Vector3 cameraPos = playerCamera.localPosition;
            cameraPos.y = Mathf.Lerp(
                cameraPos.y,
                standingCameraHeight,
                crouchSpeed * Time.deltaTime
            );

            playerCamera.localPosition = cameraPos;
        }

        // 달리는 중 Ctrl을 누르면 슬라이드 시작
        if (Input.GetKey(KeyCode.LeftShift) &&
            Input.GetKeyDown(KeyCode.LeftControl) &&
            !isSliding)
        {
            isSliding = true;
            slideTimer = slideDuration;
            slideDirection = transform.forward;
        }

        // 슬라이드 중
        if (isSliding)
        {
            slideTimer -= Time.deltaTime;

            float slidePower = slideSpeed * (slideTimer / slideDuration);

            controller.Move(
                slideDirection * slidePower * Time.deltaTime
            );

            if (slideTimer <= 0f)
            {
                isSliding = false;
            }
        }

        // WASD 이동
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        float currentSpeed = speed;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = sprintSpeed;
        }

        controller.Move(move * currentSpeed * Time.deltaTime);

        // 점프
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 중력
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}