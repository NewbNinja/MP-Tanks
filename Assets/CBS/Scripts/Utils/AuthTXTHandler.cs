using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Utils
{
    public class AuthTXTHandler
    {
        public const string ErrorTitle = "Error";
        public const string InvalidInput = "Invalid inputs";
        public const string InvalidPassword = "Passwords are not the same";
        public const string PolicyError = "Please accept the privacy policy";

        public const string RegisterComplete = "Registration Complete";
        public const string RegistrationMessage = "You are registered successfully";


        public const string SuccessTitle = "Success";
        public const string RecoveryError = "Something went wrong. Failed to recover password";
        private const string RecoveryMessage = "Compilation instructions sent by mail ";

        public static string GetRecoveryMessage(string email)
        {
            return RecoveryMessage + "<color=black>" + email + "</color>";
        }

    }
}
