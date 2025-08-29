using Onthesys.WebBuild;
using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PageSmsPasswordChange : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    [Header("UI Components - Inspector에서 연결")]
    public TMP_InputField txbCurrentPassword;
    public TMP_InputField txbNewPassword;
    public TMP_InputField txbRetypePassword;
    public Button btnClose;
    public Button btnSave;


    private void Start()
    {
        // InputField 설정
        ConfigureInputFields();
        
        // 버튼 이벤트 연결
        btnClose.onClick.AddListener(OnClickClose);
        btnSave.onClick.AddListener(OnClickSave);
        
        // UiManager 이벤트 등록
        UiManager.Instance.Register(UiEventType.ResponsePasswordChange, OnPasswordChangeResponse);

        gameObject.SetActive(false);
    }

    private void ConfigureInputFields()
    {
        // 비밀번호 타입 설정
        txbCurrentPassword.contentType = TMP_InputField.ContentType.Password;
        txbNewPassword.contentType = TMP_InputField.ContentType.Password;
        txbRetypePassword.contentType = TMP_InputField.ContentType.Password;
        
        // 초기화
        txbCurrentPassword.text = string.Empty;
        txbNewPassword.text = string.Empty;
        txbRetypePassword.text = string.Empty;
    }

    private void OnClickClose()
    {
        ClearInputFields();
        UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsManage));
    }

    private void OnClickSave()
    {
        if (!ValidateInputs()) return;
        
        // 버튼 비활성화
        btnSave.interactable = false;
        btnClose.interactable = false;
        
        // 비밀번호 변경 요청
        UiManager.Instance.Invoke(UiEventType.RequestPasswordChange, 
            (txbCurrentPassword.text, txbNewPassword.text));
    }

    private bool ValidateInputs()
    {
        // 현재 비밀번호 확인
        if (string.IsNullOrWhiteSpace(txbCurrentPassword.text))
        {
            UiManager.Instance.Invoke(UiEventType.PopupError, 
                ("변경 실패", "현재 비밀번호를 입력해주세요."));
            return false;
        }
        
        // 새 비밀번호 확인
        if (string.IsNullOrWhiteSpace(txbNewPassword.text))
        {
            UiManager.Instance.Invoke(UiEventType.PopupError, 
                ("변경 실패", "새 비밀번호를 입력해주세요."));
            return false;
        }
        
        // 비밀번호 재입력 확인
        if (string.IsNullOrWhiteSpace(txbRetypePassword.text))
        {
            UiManager.Instance.Invoke(UiEventType.PopupError, 
                ("변경 실패", "새 비밀번호를 다시 입력해주세요."));
            return false;
        }
        
        // 비밀번호 일치 확인
        if (txbNewPassword.text != txbRetypePassword.text)
        {
            UiManager.Instance.Invoke(UiEventType.PopupError, 
                ("변경 실패", "새 비밀번호가 일치하지 않습니다."));
            txbRetypePassword.text = string.Empty;
            return false;
        }
        
        // 현재와 새 비밀번호 동일 체크
        if (txbCurrentPassword.text == txbNewPassword.text)
        {
            UiManager.Instance.Invoke(UiEventType.PopupError, 
                ("변경 실패", "새 비밀번호는 현재 비밀번호와 달라야 합니다."));
            return false;
        }
        
        // 최소 길이 체크
        if (txbNewPassword.text.Length < 4)
        {
            UiManager.Instance.Invoke(UiEventType.PopupError, 
                ("변경 실패", "비밀번호는 최소 4자 이상이어야 합니다."));
            return false;
        }
        
        return true;
    }

    private void OnPasswordChangeResponse(object obj)
    {
        btnSave.interactable = true;
        btnClose.interactable = true;
        
        if (obj is not (bool isSucceed, string message)) return;
        
        if (isSucceed)
        {
            UiManager.Instance.Invoke(UiEventType.PopupError, 
                ("변경 성공", "비밀번호가 변경되었습니다.\n다시 로그인해주세요."));
            
            ClearInputFields();
            
            // 로그인 페이지로 이동
            UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsAuth));
        }
        else
        {
            UiManager.Instance.Invoke(UiEventType.PopupError, 
                ("변경 실패", message ?? "비밀번호 변경에 실패했습니다."));
            
            if (message != null && message.Contains("현재 비밀번호"))
            {
                txbCurrentPassword.text = string.Empty;
            }
        }
    }

    private void ClearInputFields()
    {
        txbCurrentPassword.text = string.Empty;
        txbNewPassword.text = string.Empty;
        txbRetypePassword.text = string.Empty;
    }

    private void OnDestroy()
    {
        if (btnClose != null) btnClose.onClick.RemoveListener(OnClickClose);
        if (btnSave != null) btnSave.onClick.RemoveListener(OnClickSave);
        
        if (UiManager.Instance != null)
        {
            UiManager.Instance.Unregister(UiEventType.ResponsePasswordChange, OnPasswordChangeResponse);
        }
    }
}