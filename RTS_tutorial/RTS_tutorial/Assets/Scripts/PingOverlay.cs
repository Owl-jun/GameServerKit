using TMPro;
using UnityEngine;

public class PingOverlay : MonoBehaviour
{
    public static PingOverlay Instance;

    [SerializeField] private TextMeshProUGUI pingText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdatePing(long ping)
    {
        pingText.text = $"Ping: {ping} ms";
    }
}
