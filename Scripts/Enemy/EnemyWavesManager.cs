using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyManager))]
public class EnemyWavesManager : MonoBehaviour
{
    public EnemyWaveSO[] waves;
    public GameObject enemyPrefab;

    public int currentWave = 0;

    const int SecondsBetweenWaves = 30;
    const float SpawnDelay = 0.15f;

    EnemyBasePoint enemyBasePosition;
    Coroutine enemyWavesCoroutine;
    bool isSpawningWave = false;


    private void Awake()
    {
        enemyBasePosition = FindFirstObjectByType<EnemyBasePoint>();
    }

    private void Start()
    {
        EnemyManager.Instance.onAllEnemiesDead += OnAllEnemiesDead;
        enemyWavesCoroutine = StartCoroutine(StartWaveRound());
    }

    void OnAllEnemiesDead()
    {
        if (isSpawningWave)
            return;

        if (currentWave >= waves.Length)
            return;

        if (enemyWavesCoroutine != null)
            StopCoroutine(enemyWavesCoroutine);

        enemyWavesCoroutine = StartCoroutine(StartWaveRound());
    }

    IEnumerator StartWaveRound()
    {
        isSpawningWave = true;

        yield return new WaitForSeconds(SecondsBetweenWaves);

        EnemyWaveSO wave = waves[currentWave];

        for (int i = 0; i < wave.waves.Length; i++)
        {
            EnemyWaveSquad squad = wave.waves[i];
            yield return StartCoroutine(SpawnWaveSquad(squad));
        }

        isSpawningWave = false;

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
}
