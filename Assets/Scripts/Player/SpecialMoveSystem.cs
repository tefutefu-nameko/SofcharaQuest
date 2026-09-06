using System;
using UnityEngine;

public class SpecialMoveSystem : MonoBehaviour
{
    [Header("Gauge Settings")]
    public float maxGauge = 100f;
    [SerializeField] float currentGauge = 0f;

    PlayerStats playerStats;
    GameStateService gameStateService;

    // Events
    public event Action<float, float> OnGaugeChanged;
    public event Action<Sprite> OnSpecialMoveActivated;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        gameStateService = FindObjectOfType<GameStateService>();
        
        // Initialize gauge
        currentGauge = 0f;
        OnGaugeChanged?.Invoke(currentGauge, maxGauge);
    }

    void Update()
    {
        // Don't process input if game is over or paused
        if (gameStateService != null && (gameStateService.IsGameOver || gameStateService.CurrentState != GameState.Gameplay))
        {
            return;
        }

        // Check for Special Move activation input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryActivateSpecialMove();
        }
    }

    public void AddGauge(float baseAmount)
    {
        // Don't add gauge if game is over or paused
        if (gameStateService != null && (gameStateService.IsGameOver || gameStateService.CurrentState != GameState.Gameplay)) return;

        if (currentGauge >= maxGauge) return;

        float multiplier = playerStats && playerStats.Data ? playerStats.Data.SpecialMoveGaugeMultiplier : 1f;
        currentGauge += baseAmount * multiplier;

        if (currentGauge > maxGauge)
        {
            currentGauge = maxGauge;
        }

        OnGaugeChanged?.Invoke(currentGauge, maxGauge);
    }

    void TryActivateSpecialMove()
    {
        if (currentGauge >= maxGauge)
        {
            // Reset gauge
            currentGauge = 0f;
            OnGaugeChanged?.Invoke(currentGauge, maxGauge);

            // Trigger activation event with the cutin sprite
            Sprite cutin = playerStats && playerStats.Data ? playerStats.Data.SpecialMoveCutin : null;
            OnSpecialMoveActivated?.Invoke(cutin);

            // TODO: Here we can also call a character-specific special move component 
            // if we add one in the future (e.g. GetComponent<SpecialMoveBase>()?.Activate())
            Debug.Log("Special Move Activated!");
        }
    }
}
