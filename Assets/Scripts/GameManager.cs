using System.Collections;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [SerializeField] private int _requiredPlayers = 2;
    [SerializeField] private float _matchDuration = 60f;

    public enum GameState
    {
        WaitingForPlayers,
        Countdown,
        InProgress,
        ShowingResults
    }
    
    public readonly SyncVar<GameState> CurrentState = new SyncVar<GameState>(GameState.WaitingForPlayers);
    public readonly SyncVar<int> ConnectedPlayers = new SyncVar<int>(0);
    public readonly SyncVar<float> MatchTimer = new SyncVar<float>(60f);
    
    public readonly SyncVar<int> CountdownTimer = new SyncVar<int>(3);

    private Coroutine _countdownCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        base.ServerManager.OnRemoteConnectionState += OnPlayerConnectionChanged;
        ConnectedPlayers.Value = base.ServerManager.Clients.Count;
        MatchTimer.Value = _matchDuration;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        base.ServerManager.OnRemoteConnectionState -= OnPlayerConnectionChanged;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        CurrentState.OnChange += OnGameStateChanged;
        ConnectedPlayers.OnChange += OnPlayersCountChanged;
        
        CountdownTimer.OnChange += OnCountdownTimerChanged;
        
        UIManager.Instance?.UpdateUI(CurrentState.Value, ConnectedPlayers.Value);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        CurrentState.OnChange -= OnGameStateChanged;
        ConnectedPlayers.OnChange -= OnPlayersCountChanged;
        CountdownTimer.OnChange -= OnCountdownTimerChanged;
    }

    private void Update()
    {
        if (!base.IsServerInitialized) return;
        if (CurrentState.Value != GameState.InProgress) return;

        MatchTimer.Value -= Time.deltaTime;

        if (MatchTimer.Value <= 0f)
        {
            EndMatch();
        }
    }

    private void OnPlayerConnectionChanged(NetworkConnection conn, FishNet.Transporting.RemoteConnectionStateArgs args)
    {
        if (!base.IsServerInitialized) return;

        ConnectedPlayers.Value = base.ServerManager.Clients.Count;
        
        CheckLobbyState();
    }
    
    private void CheckLobbyState()
    {
        if (CurrentState.Value == GameState.WaitingForPlayers && ConnectedPlayers.Value >= _requiredPlayers)
        {
            StartCountdown();
        }
        else if ((CurrentState.Value == GameState.InProgress || CurrentState.Value == GameState.Countdown) && ConnectedPlayers.Value < _requiredPlayers)
        {
            if (_countdownCoroutine != null) StopCoroutine(_countdownCoroutine);
            ResetToLobby();
        }
    }
    
    private void StartCountdown()
    {
        CurrentState.Value = GameState.Countdown;
        if (_countdownCoroutine != null) StopCoroutine(_countdownCoroutine);
        _countdownCoroutine = StartCoroutine(CountdownWorkflow());
    }

    private IEnumerator CountdownWorkflow()
    {
        CountdownTimer.Value = 3;

        while (CountdownTimer.Value > 0)
        {
            yield return new WaitForSeconds(1f);
            CountdownTimer.Value--;
        }
        
        StartMatch();
    }

    private void StartMatch()
    {
        MatchTimer.Value = _matchDuration;
        CurrentState.Value = GameState.InProgress;
        Debug.Log("[Server] Match started!");
    }

    private void EndMatch()
    {
        CurrentState.Value = GameState.ShowingResults;
        Debug.Log("[Server] Match ended! Showing results...");
        
        Invoke(nameof(ResetToLobby), 5f);
    }

    private void ResetToLobby()
    {
        if (!base.IsServerInitialized) return;

        Debug.Log("[Server] Resetting lobby and players stats...");
        
        foreach (NetworkConnection conn in base.ServerManager.Clients.Values)
        {
            foreach (NetworkObject nob in conn.Objects)
            {
                PlayerNetwork pn = nob.GetComponent<PlayerNetwork>();
                if (pn != null)
                {
                    pn.HP.Value = 100;
                    pn.Score.Value = 0;
                    pn.IsAlive.Value = true;
                    pn.UpdateVisualObserversRpc(true); 
                    pn.transform.position = pn.GetRandomSpawnPosition();
                    PlayerShooting shooting = nob.GetComponent<PlayerShooting>();
                    if (shooting != null)
                    {
                        shooting.CurrentAmmo.Value = shooting._maxAmmo;
                    }
                }
            }
        }

        MatchTimer.Value = _matchDuration;
        CurrentState.Value = GameState.WaitingForPlayers;
        ConnectedPlayers.Value = base.ServerManager.Clients.Count; 
        
        Debug.Log("[Server] Lobby reset complete. Checking if we can start a new match...");
        
        CheckLobbyState();
    }

    private void OnGameStateChanged(GameState prev, GameState next, bool asServer)
    {
        UIManager.Instance?.UpdateUI(next, ConnectedPlayers.Value);
    }

    private void OnPlayersCountChanged(int prev, int next, bool asServer)
    {
        UIManager.Instance?.UpdateUI(CurrentState.Value, next);
    }
    
    private void OnCountdownTimerChanged(int prev, int next, bool asServer)
    {
        if (CurrentState.Value == GameState.Countdown)
        {
            UIManager.Instance?.UpdateUI(CurrentState.Value, ConnectedPlayers.Value);
        }
    }
}
