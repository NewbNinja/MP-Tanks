using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TanksMP;

public class JustTesting : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        EncryptionTest();
    }


    private void EncryptionTest()
    {
        Encryptor.GetInstance().encrypt = true;                 // Enable encryption

        string tesA = Encryptor.Encrypt("Test string 123!");    // ENCRYPT something
        Debug.Log("Encrypted String:  " + tesA);

        tesA = Encryptor.Decrypt(tesA);                         // DECRYPT something
        Debug.Log("Decrypted String:  " + tesA);

        Encryptor.GetInstance().encrypt = false;                // Disable encryption
    }
}
