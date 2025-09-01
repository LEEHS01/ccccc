using Onthesys.WebBuild;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PageSmsPasswordChange : MonoBehaviour
{
    [Header("UI Components - Inspector에서 연결")]
    public TMP_InputField txbCurrentPassword;
    public TMP_InputField txbNewPassword;
    public TMP_InputField txbRetypePassword;
    public Button btnClose;
    public Button btnSave;

    private void Start()
    {
        // 기본 설정
        ConfigureInputFields();

        // 버튼 이벤트 연결
        if (btnClose != null)
            btnClose.onClick.AddListener(OnClickClose);
        if (btnSave != null)
            btnSave.onClick.AddListener(OnClickSave);

        // 페이지 비활성화
        gameObject.SetActive(false);
    }

    private void ConfigureInputFields()
    {
        // 비밀번호 타입 설정
        if (txbCurrentPassword != null)
            txbCurrentPassword.contentType = TMP_InputField.ContentType.Password;
        if (txbNewPassword != null)
            txbNewPassword.contentType = TMP_InputField.ContentType.Password;
        if (txbRetypePassword != null)
            txbRetypePassword.contentType = TMP_InputField.ContentType.Password;
    }

    private void OnClickClose()
    {
        // 입력 필드 초기화
        ClearInputFields();

        // SMS 관리 페이지로 이동
        UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsManage));
    }

    private void OnClickSave()
    {
        // TODO: 나중에 구현
        Debug.Log("저장 버튼 클릭 - 기능 미구현");

        // 임시로 닫기 동작 수행
        OnClickClose();
    }

    private void ClearInputFields()
    {
        if (txbCurrentPassword != null)
            txbCurrentPassword.text = string.Empty;
        if (txbNewPassword != null)
            txbNewPassword.text = string.Empty;
        if (txbRetypePassword != null)
            txbRetypePassword.text = string.Empty;
    }

    private void OnDestroy()
    {
        if (btnClose != null)
            btnClose.onClick.RemoveListener(OnClickClose);
        if (btnSave != null)
            btnSave.onClick.RemoveListener(OnClickSave);
    }
}