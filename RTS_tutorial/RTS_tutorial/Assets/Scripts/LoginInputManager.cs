using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LoginInputManager : MonoBehaviour
{
    public TMP_InputField inputUsername;
    public TMP_InputField inputPassword;
    public Button login;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            var current = EventSystem.current.currentSelectedGameObject;

            if (current == inputUsername.gameObject)
                inputPassword.Select();
            else if (current == inputPassword.gameObject)
                inputUsername.Select();  // �� �ٽ� ������ �ǵ��ƿ�
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            LoginManager.Instance.OnLoginClicked();
        }
    }
}
