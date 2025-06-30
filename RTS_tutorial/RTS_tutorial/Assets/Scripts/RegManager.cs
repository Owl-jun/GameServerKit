using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System;
using Unity.VisualScripting.Antlr3.Runtime;
using System.Threading.Tasks;

public class RegManager : MonoBehaviour
{
    public static RegManager Instance { get; private set; }
    private UIManager uimanager;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    [Header("UI")]
    public TMP_InputField inputUsername;
    public TMP_InputField inputPassword;
    public TMP_InputField inputName;
    public TMP_InputField inputAge;
    public TMP_InputField inputPhone;
    public Button createButton;
    public Button backButton;
    public TMP_Text resultTxt;

    [Header("Server Info")]
    public string createUrl = "http://127.0.0.1:5044/api/User/reg";

    private void Start()
    {
        uimanager = FindObjectOfType<UIManager>();
        createButton.onClick.AddListener(OnRegClicked);
        backButton.onClick.AddListener(OnBackClicked);
    }

    public void OnBackClicked()
    {
        uimanager.OnClickRegisterCancel();
    }

    public void OnRegClicked()
    {
        string id = inputUsername.text.Trim();
        string pw = inputPassword.text.Trim();
        string name = inputName.text.Trim();
        int age = Convert.ToInt32(inputAge.text.Trim());
        string phone = inputPhone.text.Trim();

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw))
        {
            Debug.LogWarning("���̵� Ȥ�� ��й�ȣ�� �Է����ּ���.");
            resultTxt.text = "Failed : Check ID or PWD...";
            uimanager.OnCompleted();
            return;
        }
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(phone))
        {
            Debug.LogWarning("�̸� Ȥ�� �޴�����ȣ�� �Է����ּ���.");
            resultTxt.text = "Failed : Required Name and Phone ...";
            uimanager.OnCompleted();
            return;
        }
        StartCoroutine(RegCoroutine(id, pw, name,age,phone));
    }

    IEnumerator RegCoroutine(string id, string pw, string name, int age, string phone)
    {
        // JSON ������ �����
        string jsonData = JsonUtility.ToJson(new RegRequest { Username = id, Password = pw , Name = name , Age = age, Phone = phone });

        // ��û ����
        UnityWebRequest request = new UnityWebRequest(createUrl, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log("������ ȸ������ ��û ��...");

        yield return request.SendWebRequest();


        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("ȸ������ ����: " + request.error);
            resultTxt.text= "Failed!";
            uimanager.OnCompleted();
        }
        else
        {
            string responseText = request.downloadHandler.text;
            Debug.Log("ȸ������ ����! " + responseText);
            resultTxt.text = "Success!";
            uimanager.OnCompleted();

            // ���� ������ �̵��ϴ� ���
            // SceneManager.LoadScene("SampleScene");
        }
    }


    // DTO Ŭ������
    [System.Serializable]
    public class RegRequest
    {
        public string Username;
        public string Password;
        public string Name;
        public int Age;
        public string Phone;
    }

}
