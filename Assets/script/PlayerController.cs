using System;
using UnityEngine;
using Photon.Pun;


public class PlayerController : MonoBehaviourPun
{
    [SerializeField]
    private KeyCode jumpKeyCode = KeyCode.Space;   // ���� Ű ����
    [SerializeField]
    private CameraController cameraController;     // ī�޶� ��Ʈ�ѷ� ����
    private Movement3D movement3D;                 // 3D �̵� ����

    private void Awake()
    {
        movement3D = GetComponent<Movement3D>();

        // CameraController�� ������� ���� ���, �ڽ� ������Ʈ���� �˻�
        if (cameraController == null)
        {
            cameraController = GetComponentInChildren<CameraController>();
        }

        // ���� �÷��̾����� Ȯ��
        if (!photonView.IsMine)
        {
            if (cameraController != null)
            {
                cameraController.gameObject.SetActive(false);  // ���� �÷��̾ �ƴϸ� ī�޶� ��Ȱ��ȭ
            }
        }
        else
        {
            // ���� �÷��̾�� PhotonView �������� �ڵ����� ������ ����, ������ TransferOwnership ���ʿ�
            Debug.Log("���� �÷��̾��Դϴ�. ������ �ڵ� �Ҵ�.");
        }
    }

    private void Update()
    {
        // ���� �÷��̾ ���� ����
        if (!photonView.IsMine)
        {
            return;
        }

        HandleMovement();        // �̵� ó��
        HandleJump();            // ���� ó��
        HandleCameraRotation();  // ī�޶� ȸ�� ó��
    }

    private void HandleMovement()
    {
        // �Է°��� ������ �̵� ������ ���
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(x, 0, z).normalized;

        // �̵� ������ ���� ��� ȸ�� ó��
        if (moveDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 720 * Time.deltaTime);
        }

        // Movement3D ��ũ��Ʈ�� �̵� ���� ����
        movement3D.MoveTo(moveDirection);
    }

    private void HandleJump()
    {
        // ���� Ű �Է½� ���� ó��
        if (Input.GetKeyDown(jumpKeyCode))
        {
            movement3D.JumpTo();  // JumpTo�� Movement3D ��ũ��Ʈ���� ���ǵ� ���� �޼���
        }
    }

    private void HandleCameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (cameraController != null)
        {
            cameraController.RotateTo(mouseX, mouseY);

            // ĳ������ ȸ���� ī�޶��� ȸ�� ����� ��ġ��Ű��
            Vector3 cameraForward = cameraController.transform.forward;
            cameraForward.y = 0; // ���� ���⸸ ���
            transform.rotation = Quaternion.LookRotation(cameraForward);
        }
        else
        {
            Debug.LogError("ī�޶� ��Ʈ�ѷ��� ������� �ʾҽ��ϴ�.");
        }
    }
}