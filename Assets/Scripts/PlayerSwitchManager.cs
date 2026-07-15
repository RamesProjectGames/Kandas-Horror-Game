using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSwitchManager : MonoBehaviour
{
    public static PlayerSwitchManager Instance { get; private set; }

    [Header("Player Switching")]
    [SerializeField] private KeyCode switchPlayerKey = KeyCode.Tab;
    [SerializeField] private InputActionReference switchPlayerAction;
    [SerializeField] private PlayerController selectedPlayerToSwitchTo;

    private readonly List<PlayerController> players = new List<PlayerController>();
    private PlayerController activePlayer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        RefreshPlayerList();
    }

    private void Start()
    {
        RefreshPlayerList();
        if (activePlayer == null && players.Count > 0)
        {
            SetActivePlayer(players[0]);
        }
    }

    private void Update()
    {
        if (switchPlayerAction != null && switchPlayerAction.action != null && switchPlayerAction.action.WasPressedThisFrame())
        {
            SwitchToSelectedPlayerOrNext();
        }
        else if (switchPlayerAction == null && Input.GetKeyDown(switchPlayerKey))
        {
            SwitchToSelectedPlayerOrNext();
        }
    }

    public void RegisterPlayer(PlayerController player)
    {
        if (player == null || players.Contains(player))
        {
            return;
        }

        players.Add(player);
        if (activePlayer == null)
        {
            SetActivePlayer(player);
        }
    }

    public void RefreshPlayerList()
    {
        players.RemoveAll(player => player == null);

        foreach (var player in FindObjectsByType<PlayerController>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            if (!players.Contains(player))
            {
                players.Add(player);
            }
        }
    }

    public void SetActivePlayer(PlayerController targetPlayer)
    {
        RefreshPlayerList();
        if (targetPlayer == null)
        {
            return;
        }

        if (!players.Contains(targetPlayer))
        {
            players.Add(targetPlayer);
        }

        activePlayer = targetPlayer;

        foreach (var player in players)
        {
            if (player != null)
            {
                player.SetActivePlayer(player == targetPlayer);
            }
        }
    }

    public void SwitchToPlayer(PlayerController targetPlayer)
    {
        SetActivePlayer(targetPlayer);
    }

    public void SwitchToNextPlayer()
    {
        RefreshPlayerList();
        if (players.Count <= 1)
        {
            return;
        }

        int currentIndex = activePlayer == null ? -1 : players.IndexOf(activePlayer);
        if (currentIndex < 0)
        {
            currentIndex = 0;
        }

        int nextIndex = (currentIndex + 1) % players.Count;
        SetActivePlayer(players[nextIndex]);
    }

    private void SwitchToSelectedPlayerOrNext()
    {
        if (selectedPlayerToSwitchTo != null)
        {
            SwitchToPlayer(selectedPlayerToSwitchTo);
        }
        else
        {
            SwitchToNextPlayer();
        }
    }
}
