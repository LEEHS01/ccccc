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

    // 다른 SMS 페이지처럼 ModelProvider 사용
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    private void Start()
    {
        // 기본 설정
        ConfigureInputFields();

        // 버튼 이벤트 연결
        if (btnClose != null)
            btnClose.onClick.AddListener(OnClickClose);
        if (btnSave != null)
            btnSave.onClick.AddListener(OnClickSave);

        // 이벤트 등록
        UiManager.Instance.Register(UiEventType.ResponsePasswordChange, OnPasswordChangeResponse);
        UiManager.Instance.Register(UiEventType.NavigateSms, OnNavigateSms);

        // 페이지 비활성화
        gameObject.SetActive(false);
    }

    private void OnNavigateSms(object obj)
    {
        if (obj is not Type type) return;

        if (type != typeof(PageSmsPasswordChange)) return;

        // 필요시 초기화 작업
        ClearInputFields();
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
        // 입력 검증
        if (!ValidateInputs())
            return;

        // 버튼 비활성화 (중복 클릭 방지)
        btnSave.interactable = false;

        // 비밀번호 변경 요청 - UiManager를 통해 이벤트 발생
        var passwordData = (currentPassword: txbCurrentPassword.text, newPassword: txbNewPassword.text);
        UiManager.Instance.Invoke(UiEventType.RequestPasswordChange, passwordData);
    }

    private void OnPasswordChangeResponse(object obj)
    {
        // 버튼 다시 활성화
        btnSave.interactable = true;

        if (obj is not (bool isSucceed, string message)) return;

        if (isSucceed)
        {
            // 성공 팝업 표시
            UiManager.Instance.Invoke(UiEventType.PopupError,
                ("변경 완료", "비밀번호가 성공적으로 변경되었습니다.\n다시 로그인해주세요."));

            // 입력 필드 초기화
            ClearInputFields();

            // 로그인 페이지로 이동 (약간의 딜레이 후)
            Invoke(nameof(NavigateToLogin), 1.5f);
        }
        else
        {
            // 실패 팝업 표시
            UiManager.Instance.Invoke(UiEventType.PopupError,
                ("변경 실패", message ?? "비밀번호 변경에 실패했습니다."));

            // 현재 비밀번호 필드만 초기화
            if (txbCurrentPassword != null)
                txbCurrentPassword.text = string.Empty;
        }
    }

    private bool ValidateInputs()
    {
        // 현재 비밀번호 확인
        if (string.IsNullOrWhiteSpace(txbCurrentPassword.text))
        {
            UiManager.Instance.Invoke(UiEventType.PopupError,
                ("입력 오류", "현재 비밀번호를 입력해주세요."));
            return false;
        }

        // 새 비밀번호 확인
        if (string.IsNullOrWhiteSpace(txbNewPassword.text))
        {
            UiManager.Instance.Invoke(UiEventType.PopupError,
                ("입력 오류", "새 비밀번호를 입력해주세요."));
            return false;
        }

        // 새 비밀번호 최소 길이 확인 (예: 4자 이상)
        if (txbNewPassword.text.Length < 4)
        {
            UiManager.Instance.Invoke(UiEventType.PopupError,
                ("입력 오류", "새 비밀번호는 4자 이상이어야 합니다."));
            return false;
        }

        // 비밀번호 재입력 확인
        if (txbNewPassword.text != txbRetypePassword.text)
        {
            UiManager.Instance.Invoke(UiEventType.PopupError,
                ("입력 오류", "새 비밀번호가 일치하지 않습니다."));
            return false;
        }

        // 현재 비밀번호와 새 비밀번호가 같은지 확인
        if (txbCurrentPassword.text == txbNewPassword.text)
        {
            UiManager.Instance.Invoke(UiEventType.PopupError,
                ("입력 오류", "새 비밀번호는 현재 비밀번호와 달라야 합니다."));
            return false;
        }

        return true;
    }

    private void NavigateToLogin()
    {
        // 로그인 페이지로 이동
        UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsAuth));
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