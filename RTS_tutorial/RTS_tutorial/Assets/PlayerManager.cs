using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public GameObject myPlayer;
    public string myPlayerName;
    private Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Register(string id, GameObject go)
    {
        players[id] = go;
    }
    public void Deluser(string id)
    {
        players.Remove(id);
    }

    public GameObject GetPlayer(string id)
    {
        return players.TryGetValue(id, out var go) ? go : null;
    }

    public bool Exists(string id) => players.ContainsKey(id);
}
