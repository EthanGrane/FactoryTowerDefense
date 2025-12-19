using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "New Enemy Wave", menuName = "FACTORY/ENEMY/Wave")]
public class EnemyWaveSO : ScriptableObject
{
    [Header("Enemy")]
    public EnemyWaveSquad[] waves;
    
    [Header("Reward")]
    public bool hasReward = true;
    public Item rewardItem;
    public int rewardAmount = 5;
}

[System.Serializable]
public class EnemyWaveSquad
{
    public EnemyTierSO enemy;
    public int quantity = 1;
}

[CustomEditor(typeof(EnemyWaveSO))]
[CanEditMultipleObjects]
public class EnemyWaveSOEditor : Editor
{
    SerializedProperty wavesProp;

    private void OnEnable()
    {
        wavesProp = serializedObject.FindProperty("waves");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Inspector por defecto (YA soporta multi-edit)
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("=== SUMMARY ===", EditorStyles.boldLabel);

        // Multi-object summary
        foreach (Object obj in targets)
        {
            EnemyWaveSO waveSO = (EnemyWaveSO)obj;
            DrawWaveSummary(waveSO);
            EditorGUILayout.Space(6);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawWaveSummary(EnemyWaveSO waveSO)
    {
        float totalTier = 0f;

        if (waveSO.waves != null)
        {
            foreach (var squad in waveSO.waves)
            {
                if (squad.enemy == null) continue;
                totalTier += (squad.enemy.tierIndex + 1) * squad.quantity;
            }
        }

        EditorGUILayout.LabelField(
            waveSO.name,
            $"Total Tier: {totalTier}",
            EditorStyles.helpBox
        );

        if (waveSO.waves == null) return;

        EditorGUI.indentLevel++;

        foreach (var squad in waveSO.waves)
        {
            if (squad.enemy == null) continue;

            float squadTier =
                (squad.enemy.tierIndex + 1) * squad.quantity;

            EditorGUILayout.LabelField(
                $"{squad.enemy.tierName} × {squad.quantity} | Tier: {squadTier}"
            );
        }

        EditorGUI.indentLevel--;
    }
}
