using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SimpleError
{
    public PlayFabErrorCode ErrorCode;
    public string Message;
    public string Stack;

    public static SimpleError FromTemplate(PlayFabError error)
    {
        return new SimpleError
        {
            ErrorCode = error.Error,
            Message = error.ErrorMessage,
            Stack = error.Error.ToString()
        };
    }

    public static SimpleError FromTemplate(ScriptExecutionError error)
    {
        return new SimpleError
        {
            ErrorCode = PlayFabErrorCode.CloudScriptAPIRequestError,
            Message = error.Message,
            Stack = error.StackTrace
        };
    }

    public static SimpleError FromTemplate(PlayFab.CloudScriptModels.FunctionExecutionError error)
    {
        return new SimpleError
        {
            ErrorCode = PlayFabErrorCode.CloudScriptAPIRequestError,
            Message = error.Message,
            Stack = error.StackTrace
        };
    }

    public static SimpleError FailedToGrandPack()
    {
        return new SimpleError
        {
            ErrorCode = PlayFabErrorCode.ItemNotFound,
            Message = "Failed to grand currency pack"
        };
    }

    public static SimpleError CredentialNotFound()
    {
        return new SimpleError
        {
            ErrorCode = PlayFabErrorCode.InvalidAuthToken,
            Message = "Credential Not Found"
        };
    }
}
