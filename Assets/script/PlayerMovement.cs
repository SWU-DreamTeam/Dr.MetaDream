using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    private Animator animator;

    void Start()
    {
        // Animator ������Ʈ�� �ʱ�ȭ�մϴ�.
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // �÷��̾� �̵� ó��
        Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;
        transform.Translate(movement);

        // ����Ű �Է��� Ȯ���մϴ�.
        bool isMoving = horizontal != 0 || vertical != 0;

        // ����Ű �Է��� ���� ���� Animator�� Ȱ��ȭ�ϰ�, ���� ���� ��Ȱ��ȭ�մϴ�.
        if (isMoving)
        {
            if (!animator.enabled)
            {
                animator.enabled = true; // Animator Ȱ��ȭ
            }
            animator.SetBool("isMoving", true);
        }
        else
        {
            if (animator.enabled)
            {
                animator.SetBool("isMoving", false);
                animator.enabled = false; // Animator ��Ȱ��ȭ
            }
        }
    }
}
