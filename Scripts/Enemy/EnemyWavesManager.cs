using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyManager))]
public class EnemyWavesManager : MonoBehaviour
{
    public static EnemyWavesManager Instance;
    
    public EnemyWaveSO[] waves;
    public GameObject enemyPrefab;

    public int currentWave = 0;
    
    public Action<WavePhase> onPhaseChanged;

    const int SecondsBetweenWaves = 60;
    const float SpawnDelay = 0.2f;

    EnemyBasePoint enemyBasePosition;
    Coroutine enemyWavesCoroutine;
    WavePhase wavePhase = WavePhase.Planning;
    
    private float phaseTimerCountdown = 0;
    
    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(this);
        
        enemyBasePosition = FindFirstObjectByType<EnemyBasePoint>();
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
    
    private void Start()
    {
        EnemyManager.Instance.onAllEnemiesDead += OnAllEnemiesDead;
        enemyWavesCoroutine = StartCoroutine(StartWaveRound());
    }

    [ContextMenu("Start Wave")]
    public void StartWave()
    {
        phaseTimerCountdown = 999;
    }
    
    void OnAllEnemiesDead()
    {
        if (wavePhase == WavePhase.Spawning)
            return;

        if (currentWave >= waves.Length)
            return;

        if (enemyWavesCoroutine != null)
            StopCoroutine(enemyWavesCoroutine);

        GameManager.Instance.AddItemToInventory(waves[currentWave].rewardItem, waves[currentWave].rewardAmount);
        
        enemyWavesCoroutine = StartCoroutine(StartWaveRound());
    }

    IEnumerator StartWaveRound()
    {
        yield return new WaitForSeconds(1);
        ChangePhase(WavePhase.Planning);

        phaseTimerCountdown = 0;
        while (phaseTimerCountdown < SecondsBetweenWaves && currentWave != 0)
        {
            phaseTimerCountdown += Time.deltaTime;
            yield return null;
        }

        ChangePhase(WavePhase.Spawning);
        EnemyWaveSO wave = waves[currentWave];
        for (int i = 0; i < wave.waves.Length; i++)
        {
            EnemyWaveSquad squad = wave.waves[i];
            yield return StartCoroutine(SpawnWaveSquad(squad));
        }

        ChangePhase(WavePhase.WaitingNext);

        if (EnemyManager.Instance.GetEnemiesAliveCount() == 0)
        {
            OnAllEnemiesDead();
            yield break;
        }

        currentWave++;
        enemyWavesCoroutine = null;
    }

    IEnumerator SpawnWaveSquad(EnemyWaveSquad squad)
    {
        for (int i = 0; i < squad.quantity; i++)
        {
            Enemy enemy = Instantiate(
                enemyPrefab,
                enemyBasePosition.transform.position,
                Quaternion.identity
            ).GetComponent<Enemy>();

            EnemyManager.Instance.ApplyTier(enemy, squad.enemy);

            yield return new WaitForSeconds(SpawnDelay);
        }
    }

    void ChangePhase(WavePhase newPhase)
    {
        wavePhase = newPhase;
        onPhaseChanged?.Invoke(newPhase);
    }
    
    public WavePhase GetWavePhase() => wavePhase;
    
    public float GetPhaseTimerCountdown() => phaseTimerCountdown;
}
public enum WavePhase
{
    Planning,      // Tiempo entre oleadas: se puede modificar el path
    Spawning,      // Enemigos en camino: no se puede modificar el path
    WaitingNext    // Espera entre oleadas después de morir todos los enemigos
}

