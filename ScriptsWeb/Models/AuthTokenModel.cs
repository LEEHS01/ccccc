using System;

namespace Onthesys.WebBuild
{
    [System.Serializable]
    public class AuthTokenModel
    {
        public bool is_succeed;
        public string message;
        public string auth_code;
    }

    public class PasswordChangePayload
    {
        public string currentPassword;
        public string newPassword;
    }
}
