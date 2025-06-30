using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class RegInputManager : MonoBehaviour
{
    public List<Selectable> tabOrder;

    //public TMP_InputField inputUserID;
    //public TMP_InputField inputUsername;
    //public TMP_InputField inputPassword;
    //public TMP_InputField inputAge;
    //public TMP_InputField inputPhone;
    //public Button Send;
    //public Button Back;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GameObject current = EventSystem.current.currentSelectedGameObject;

            int currentIndex = tabOrder.FindIndex(sel => sel.gameObject == current);
            if (currentIndex >= 0)
            {
                int nextIndex = (currentIndex + 1) % tabOrder.Count;
                tabOrder[nextIndex].Select();
            }
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            RegManager.Instance.OnRegClicked();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            RegManager.Instance.OnBackClicked();
        }
    }
}
