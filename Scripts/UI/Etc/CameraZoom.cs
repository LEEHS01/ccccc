using UnityEngine;

internal class CameraZoom : MonoBehaviour
{
    private Camera mainCamera; // 카메라 객체 저장

    void Start()
    {
        // 메인 카메라를 초기화
        mainCamera = Camera.main;

        // 카메라가 없는 경우 경고 메시지 출력
        if (mainCamera == null)
        {
            Debug.LogWarning("메인 카메라가 설정되지 않았습니다. 씬에 카메라가 있는지 확인하세요.");
        }
    }

    void Update()
    {
        // 마우스 왼쪽 버튼 클릭 감지
        if (Input.GetMouseButtonDown(0))
        {
            // 카메라로부터 마우스 클릭 위치까지의 Ray 생성
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo; // Raycast 결과 정보를 저장할 변수

            // Raycast를 쏘아서 충돌한 오브젝트가 있는지 확인
            if (Physics.Raycast(ray, out hitInfo))
            {
                // Raycast가 충돌한 오브젝트 가져오기
                GameObject clickedObject = hitInfo.collider.gameObject;

                // 클릭된 오브젝트의 태그 또는 이름으로 확인
                if (clickedObject.CompareTag("Button"))
                {
                    // 버튼 오브젝트가 클릭되었을 때 실행할 코드
                    Debug.Log("버튼 클릭됨: " + clickedObject.name);

                    // 예시: 클릭된 오브젝트의 색상 변경
                    Renderer renderer = clickedObject.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = Color.red;
                    }
                }
            }
        }
    }
}
