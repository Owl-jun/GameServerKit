using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChatManager : MonoBehaviour
{
    public static ChatManager Instance { get; private set; }
    private NetworkManager NM => NetworkManager.Instance;

    [Header("UI References")]
    public Canvas canvas;
    public TMP_InputField inputField;
    public Transform messageParent;
    public GameObject messagePrefab;
    public ScrollRect scrollRect;

    private bool isChatInputActive = false;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 중복 방지
            return;
        }

        Instance = this;
    }
    void Start()
    {
        if (!canvas || !inputField || !messageParent || !messagePrefab || !scrollRect)
        {
            Debug.LogError("[ChatManager] 필요한 UI 요소가 할당되지 않았습니다.");
            return;
        }

        canvas.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!isChatInputActive)
            {
                OpenChatInput();
            }
            else if (!string.IsNullOrWhiteSpace(inputField.text))
            {
                SubmitChat();
            }
            else
            {
                CloseChatInput();
            }
        }
    }

    void OpenChatInput()
    {
        isChatInputActive = true;
        canvas.gameObject.SetActive(true);
        inputField.text = "";
        inputField.ActivateInputField();
        EventSystem.current.SetSelectedGameObject(inputField.gameObject);
    }

    void CloseChatInput()
    {
        isChatInputActive = false;
        inputField.DeactivateInputField();
        EventSystem.current.SetSelectedGameObject(null);
        canvas.gameObject.SetActive(false);
    }

    void SubmitChat()
    {
        if (NM.isRunning == false)
        {
            DisplayMessage("채팅 서버와의 연결이 끊어졌습니다. 재접속을 시도합니다.");
            NM.ConnectToChatServer("127.0.0.1", 12345);
        }

        string senderId = PlayerManager.Instance.myPlayerName ?? "Unknown";
        string msg = inputField.text.Trim();

        if (string.IsNullOrEmpty(msg))
            return;

        // 본인 메시지도 화면에 출력
        DisplayMessage($"[나] {msg}");

        if (NM.isRunning == true)
            NM.SendChatMessage(0x05,$"{senderId} {msg}");

        inputField.text = "";
        CloseChatInput();
    }

    public void DisplayMessage(string message)
    {
        string myname = PlayerManager.Instance.myPlayerName;
        if (myname == message.Substring(0,myname.Length))
        {
            Debug.Log(message.Substring(0, myname.Length));
            return;
        }
        var go = Instantiate(messagePrefab, messageParent);
        var textComponent = go.GetComponentInChildren<TMP_Text>();

        if (textComponent == null)
        {
            Debug.LogError("[ChatManager] messagePrefab에 TMP_Text가 없습니다.");
            return;
        }
        
        
        textComponent.text = message;
        
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
