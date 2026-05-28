using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Панели интерфейса")]
    [SerializeField] private GameObject _connectionPanel;
    [SerializeField] private GameObject _waitingPanel;
    [SerializeField] private GameObject _gamePanel;
    [SerializeField] private GameObject _resultsPanel;   
    
    [Header("Текстовые элементы")]
    [SerializeField] private TMP_Text _waitingText;
    [SerializeField] private TMP_Text _timerText;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState.Value == GameManager.GameState.InProgress)
        {
            if (_timerText != null)
            {
                float timeRemaining = Mathf.Max(0f, GameManager.Instance.MatchTimer.Value);
                _timerText.text = $"Осталось времени: {timeRemaining:F0} сек";
            }
        }
    }

    public void UpdateUI(GameManager.GameState state, int connectedPlayers)
    {
        if (_connectionPanel != null) _connectionPanel.SetActive(false);
        if (_waitingPanel != null) _waitingPanel.SetActive(false);
        if (_gamePanel != null) _gamePanel.SetActive(false);
        if (_resultsPanel != null) _resultsPanel.SetActive(false);
        
        switch (state)
        {
            case GameManager.GameState.WaitingForPlayers:
                if (_waitingPanel != null) _waitingPanel.SetActive(true);
                if (_waitingText != null)
                {
                    _waitingText.text = $"Ожидание игроков: {connectedPlayers}";
                }
                break;
            
            case GameManager.GameState.Countdown:
                if (_waitingPanel != null) _waitingPanel.SetActive(true);
                if (_waitingText != null && GameManager.Instance != null)
                {
                    _waitingText.text = $"Матч начинается через {GameManager.Instance.CountdownTimer.Value}";
                }
                break;

            case GameManager.GameState.InProgress:
                if (_gamePanel != null) _gamePanel.SetActive(true);
                break;

            case GameManager.GameState.ShowingResults:
                if (_resultsPanel != null) _resultsPanel.SetActive(true);
                break;
        }
    }
}
