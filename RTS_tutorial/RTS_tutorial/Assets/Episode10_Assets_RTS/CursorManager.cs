using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }


    private GameObject markerInstance;  // The instantiated marker object
    bool isMarkerActive = false;  // Boolean to control marker activation
    float markerHeight = 0.0f;  // Height at which the marker should be placed (e.g., ground level)

    [Header("Marker Prefabs")]
    public GameObject walkableCursor;
    public GameObject selectableCursor;
    public GameObject attackableCursor;
    public GameObject unAvailableCursor;



    CursorType currentCursor;


    public enum CursorType
    {
        None,
        Walkable,
        UnAvailable,
        Selectable,
        Attackable
    }

    void Update()
    {
        // Check if the marker should be active
        if (isMarkerActive)
        {
            // Show the marker and hide the cursor
            markerInstance.SetActive(true);
            Cursor.visible = false;

            // Position the marker at the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                markerInstance.transform.position = hitInfo.point;
            }
        }
        else
        {
            // Hide the marker and show the cursor
            markerInstance?.SetActive(false);

            Cursor.visible = true;
        }
    }

    public void SetMarkerType(CursorType type)
    {
        if (type != currentCursor)
        {
            isMarkerActive = true;

            currentCursor = type;

            switch (type)
            {
                case CursorType.Walkable:
                    markerInstance?.SetActive(false);
                    markerInstance = walkableCursor;
                    return;
                case CursorType.Selectable:
                    markerInstance?.SetActive(false);
                    markerInstance = selectableCursor;
                    return;
                case CursorType.Attackable:
                    markerInstance?.SetActive(false);
                    markerInstance = attackableCursor;
                    return;
                case CursorType.UnAvailable:
                    markerInstance?.SetActive(false);
                    markerInstance = unAvailableCursor;
                    return;
                case CursorType.None:
                    markerInstance?.SetActive(false);
                    isMarkerActive = false;
                    return;
            }
        }
    }
}
