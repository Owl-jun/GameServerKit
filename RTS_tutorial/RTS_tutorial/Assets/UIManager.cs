using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    [Header("UI Panels")]
    public GameObject loginPanel;
    public GameObject registerPanel;
    public GameObject PopupPanel;
    public Button xBtn;
    void Start()
    {
        // �⺻������ �α��� �г��� Ȱ��, ȸ�������� ��Ȱ��
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        PopupPanel.SetActive(false);
        xBtn.onClick.AddListener(CloseBtn);
    }

    private void CloseBtn()
    {
        PopupPanel.SetActive(false);
    }

    public void OnClickRegisterOpen()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
    }

    public void OnClickRegisterCancel()
    {
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);
    }
    public void OnCompleted()
    {
        PopupPanel.SetActive(true);
        StartCoroutine(HidePopupAfterDelay(5f));
    }

    private IEnumerator HidePopupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        PopupPanel.SetActive(false);
    }
}
