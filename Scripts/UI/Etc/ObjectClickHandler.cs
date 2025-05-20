using UnityEngine; // Unity의 주요 기능 사용을 위한 네임스페이스

namespace MyNamespace
{
    internal class ObjectClickHandler : MonoBehaviour
    {
        private Camera mainCamera; // 메인 카메라를 저장할 필드

        void Start()
        {
            mainCamera = Camera.main; // 씬의 메인 카메라 설정

            if (mainCamera == null)
            {
                Debug.LogWarning("메인 카메라가 설정되지 않았습니다. 씬에 카메라가 있는지 확인하세요."); // 카메라가 없을 경우 경고 메시지
            }
        }

        void Update()
        {
            if (mainCamera == null) return; // 메인 카메라가 없으면 업데이트 중단

            if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 버튼 클릭 감지
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition); // 마우스 위치로부터 광선 생성
                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo)) // 광선이 오브젝트와 충돌하는지 확인
                {
                    GameObject clickedObject = hitInfo.collider.gameObject; // 충돌한 오브젝트 가져오기

                    Debug.Log($"클릭된 오브젝트: {clickedObject.name}, 태그: {clickedObject.tag}"); // 클릭된 오브젝트의 정보 출력

                    if (clickedObject.CompareTag("Button")) // 오브젝트의 태그가 "Button"인지 확인
                    {
                        Renderer renderer = clickedObject.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            renderer.material.color = Color.red; // 오브젝트의 색상을 빨간색으로 변경
                        }
                        else
                        {
                            Debug.LogWarning("Renderer 컴포넌트가 없습니다."); // Renderer 컴포넌트가 없을 경우 경고 메시지
                        }
                    }
                }
            }
        }
    }
}
