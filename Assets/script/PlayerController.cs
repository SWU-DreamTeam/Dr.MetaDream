using System;
using UnityEngine;
using Photon.Pun;


public class PlayerController : MonoBehaviourPun
{
    [SerializeField]
    private KeyCode jumpKeyCode = KeyCode.Space;   // 점프 키 설정
    [SerializeField]
    private CameraController cameraController;     // 카메라 컨트롤러 연결
    private Movement3D movement3D;                 // 3D 이동 제어

    private void Awake()
    {
        movement3D = GetComponent<Movement3D>();

        // CameraController가 연결되지 않은 경우, 자식 오브젝트에서 검색
        if (cameraController == null)
        {
            cameraController = GetComponentInChildren<CameraController>();
        }

        // 로컬 플레이어인지 확인
        if (!photonView.IsMine)
        {
            if (cameraController != null)
            {
                cameraController.gameObject.SetActive(false);  // 로컬 플레이어가 아니면 카메라 비활성화
            }
        }
        else
        {
            // 로컬 플레이어는 PhotonView 소유권을 자동으로 가지고 있음, 별도의 TransferOwnership 불필요
            Debug.Log("로컬 플레이어입니다. 소유권 자동 할당.");
        }
    }

    private void Update()
    {
        // 로컬 플레이어만 제어 가능
        if (!photonView.IsMine)
        {
            return;
        }

        HandleMovement();        // 이동 처리
        HandleJump();            // 점프 처리
        HandleCameraRotation();  // 카메라 회전 처리
    }

    private void HandleMovement()
    {
        // 입력값을 가져와 이동 방향을 계산
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(x, 0, z).normalized;

        // 이동 방향이 있을 경우 회전 처리
        if (moveDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 720 * Time.deltaTime);
        }

        // Movement3D 스크립트에 이동 방향 전달
        movement3D.MoveTo(moveDirection);
    }

    private void HandleJump()
    {
        // 점프 키 입력시 점프 처리
        if (Input.GetKeyDown(jumpKeyCode))
        {
            movement3D.JumpTo();  // JumpTo는 Movement3D 스크립트에서 정의된 점프 메서드
        }
    }

    private void HandleCameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (cameraController != null)
        {
            cameraController.RotateTo(mouseX, mouseY);

            // 캐릭터의 회전을 카메라의 회전 방향과 일치시키기
            Vector3 cameraForward = cameraController.transform.forward;
            cameraForward.y = 0; // 수평 방향만 고려
            transform.rotation = Quaternion.LookRotation(cameraForward);
        }
        else
        {
            Debug.LogError("카메라 컨트롤러가 연결되지 않았습니다.");
        }
    }
}