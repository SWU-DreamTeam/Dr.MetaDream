using UnityEngine;

public class Movement3D : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5.0f; // 이동 속도
    [SerializeField]
    private float gravity = -9.81f; // 중력 계수
    [SerializeField]
    private float jumpForce = 3.0f; // 뛰어 오르는 힘
    private Vector3 moveDirection;      // 이동 방향

    [SerializeField]
    private Transform cameraTransform;
    private CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // 플레이어가 땅을 밟고 있지 않다면
        // y축 이동방향에 gravity * Time.deltaTime을 더해준다
        if (characterController.isGrounded == false)
        {
            moveDirection.y += gravity * Time.deltaTime;
        }

        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
    }

    public void MoveTo(Vector3 direction)
    {
        // 카메라가 바라보고 있는 방향을 기준으로 방향키에 따라 이동할 수 있도록 함
        Vector3 movedis = cameraTransform.rotation * direction;
        moveDirection = new Vector3(movedis.x, moveDirection.y, movedis.z);
    }

    public void JumpTo()
    {
        if (characterController.isGrounded == true)
        {
            moveDirection.y = jumpForce;
        }
    }
}

