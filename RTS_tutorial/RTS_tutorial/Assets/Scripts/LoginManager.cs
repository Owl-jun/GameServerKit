using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System;
using Unity.VisualScripting.Antlr3.Runtime;
using System.Threading.Tasks;

public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance { get; private set; }
    private SceneController sC;
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

    private UIManager uimanager; 

    [Header("UI")]
    public TMP_InputField inputUsername;
    public TMP_InputField inputPassword;
    public Button loginButton;
    public Button createButton;

    [Header("Server Info")]
    public string loginUrl = "http://localhost:5044/api/User/login";
    public string logoutUrl = "http://localhost:5044/api/User/logout";

    private void Start()
    {
        uimanager = FindObjectOfType<UIManager>();
        loginButton.onClick.AddListener(OnLoginClicked);
        createButton.onClick.AddListener(OnRegClicked);
        sC = SceneController.Instance;
    }

    private void OnRegClicked()
    {
        uimanager.OnClickRegisterOpen();
    }   

    public void OnLoginClicked()
    {
        string id = inputUsername.text.Trim();
        string pw = inputPassword.text.Trim();

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw))
        {
            uimanager.PopupPanel.GetComponentInChildren<TMP_Text>().text = "아이디와 비밀번호를 입력해주세요.";
            uimanager.OnCompleted();
            Debug.LogWarning("아이디와 비밀번호를 입력해주세요.");
            return;
        }

        StartCoroutine(LoginCoroutine(id, pw));
    }

    IEnumerator LoginCoroutine(string id, string pw)
    {
        // JSON 데이터 만들기
        string jsonData = JsonUtility.ToJson(new LoginRequest { username = id, password = pw });

        // 요청 생성
        UnityWebRequest request = new UnityWebRequest(loginUrl, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        uimanager.PopupPanel.GetComponentInChildren<TMP_Text>().text = "서버에 로그인 요청 중...";
        uimanager.OnCompleted();
        Debug.Log("서버에 로그인 요청 중...");

        yield return request.SendWebRequest();


        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            uimanager.PopupPanel.GetComponentInChildren<TMP_Text>().text = "로그인 실패: " + request.error;
            uimanager.OnCompleted();
            Debug.LogError("로그인 실패: " + request.error);
        }
        else
        {
            string responseText = request.downloadHandler.text;
            LoginResponse res = JsonUtility.FromJson<LoginResponse>(responseText);
            Debug.Log("로그인 성공! 토큰: " + res.token);

            GameManager.token = res.token;
            GameManager.userId = id;
            Debug.Log(GameManager.token);
            // 다음 씬으로 이동하는 경우
            sC.LoadScene(SceneController.SceneType.Sample);
        }
    }
    private void OnApplicationQuit()
    {
    }

    public bool LogoutSent { get; private set; } = false;

    public void SendLogoutBlocking()
    {
        if (LogoutSent) return;

        string token = GameManager.token;
        if (string.IsNullOrEmpty(token)) return;

        var request = new UnityWebRequest(logoutUrl, "POST");
        request.SetRequestHeader("Authorization", $"Bearer {token}");
        request.downloadHandler = new DownloadHandlerBuffer();
        var op = request.SendWebRequest();

        float timeout = 1f;
        float startTime = Time.realtimeSinceStartup;

        while (!op.isDone && Time.realtimeSinceStartup - startTime < timeout) { }

        LogoutSent = true;

        if (request.result != UnityWebRequest.Result.Success)
            Debug.LogWarning("로그아웃 실패: " + request.error);

        else
            Debug.Log("로그아웃 성공");
    }



    // DTO 클래스들
    [System.Serializable]
    public class LoginRequest
    {
        public string username;
        public string password;
    }

    public class LogoutRequest
    {
        public string Authorization;
    }

    [System.Serializable]
    public class LoginResponse
    {
        public string token;
    }
}
