using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    private Animator animator;

    void Start()
    {
        // Animator 컴포넌트를 초기화합니다.
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 플레이어 이동 처리
        Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;
        transform.Translate(movement);

        // 방향키 입력을 확인합니다.
        bool isMoving = horizontal != 0 || vertical != 0;

        // 방향키 입력이 있을 때만 Animator를 활성화하고, 없을 때는 비활성화합니다.
        if (isMoving)
        {
            if (!animator.enabled)
            {
                animator.enabled = true; // Animator 활성화
            }
            animator.SetBool("isMoving", true);
        }
        else
        {
            if (animator.enabled)
            {
                animator.SetBool("isMoving", false);
                animator.enabled = false; // Animator 비활성화
            }
        }
    }
}
