using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RandomSpawner2D))]
public class RandomSpawner2DInspector : Editor
{
	SerializedProperty prefab;
	SerializedProperty spawnCount;
	SerializedProperty delayBetweenSpawns;
	SerializedProperty parentForSpawn;
	SerializedProperty useBoxCollider2DArea;
	SerializedProperty areaCenter;
	SerializedProperty areaSize;
	SerializedProperty randomRotationZ;
	SerializedProperty randomUniformScaleMinMax;
	SerializedProperty autoStart;

	void OnEnable()
	{
		prefab = serializedObject.FindProperty("prefab");
		spawnCount = serializedObject.FindProperty("spawnCount");
		delayBetweenSpawns = serializedObject.FindProperty("delayBetweenSpawns");
		parentForSpawn = serializedObject.FindProperty("parentForSpawn");
		useBoxCollider2DArea = serializedObject.FindProperty("useBoxCollider2DArea");
		areaCenter = serializedObject.FindProperty("areaCenter");
		areaSize = serializedObject.FindProperty("areaSize");
		randomRotationZ = serializedObject.FindProperty("randomRotationZ");
		randomUniformScaleMinMax = serializedObject.FindProperty("randomUniformScaleMinMax");
		autoStart = serializedObject.FindProperty("autoStart");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.PropertyField(prefab);
		EditorGUILayout.PropertyField(spawnCount);
		EditorGUILayout.PropertyField(delayBetweenSpawns);
		EditorGUILayout.PropertyField(parentForSpawn);

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("スポーン領域", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(useBoxCollider2DArea);
		using (new EditorGUI.DisabledScope(useBoxCollider2DArea.boolValue))
		{
			EditorGUILayout.PropertyField(areaCenter);
			EditorGUILayout.PropertyField(areaSize);
		}

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("ランダム化", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(randomRotationZ);
		EditorGUILayout.PropertyField(randomUniformScaleMinMax);

		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(autoStart);

		serializedObject.ApplyModifiedProperties();

		EditorGUILayout.Space();
		var spawner = (RandomSpawner2D)target;
		using (new EditorGUI.DisabledScope(spawner == null || spawner.prefab == null))
		{
			if (GUILayout.Button("Spawn Now"))
			{
				spawner.StartSpawn();
				if (!Application.isPlaying)
				{
					// Edit モードでの Undo 対応とシーン変更フラグ
					Undo.RegisterCompleteObjectUndo(spawner.gameObject, "RandomSpawner2D Spawn");
					EditorUtility.SetDirty(spawner.gameObject);
				}
			}
		}
	}
}


