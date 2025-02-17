using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GridTool : EditorWindow
{
    private Vector3 gridCellSize = new Vector3(3, 3, 3);
    private GameObject objectToPaint;
    private bool eraseMode = false;

    [MenuItem("Tools/3D Grid Painter")]
    public static void ShowWindow()
    {
        GetWindow<GridTool>("3D Grid Painter");
    }

    private void OnGUI()
    {
        GUILayout.Label("3D Grid Painter", EditorStyles.boldLabel);

        // Налаштування розміру гріду
        gridCellSize = EditorGUILayout.Vector3Field("Grid Cell Size", gridCellSize);

        // Об'єкт для малювання
        objectToPaint = (GameObject)EditorGUILayout.ObjectField("Object to Paint", objectToPaint, typeof(GameObject), false);

        // Режим стирання
        eraseMode = EditorGUILayout.Toggle("Erase Mode", eraseMode);

        EditorGUILayout.HelpBox(eraseMode
            ? "Утримуйте Shift, щоб стерти об'єкти у гріді."
            : "Клікни лівою кнопкою миші, щоб намалювати об'єкти у гріді.", MessageType.Info);
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (objectToPaint == null && !eraseMode)
            return;

        Event e = Event.current;

        // Робота з кліками миші
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 hitPosition = hit.point;

                // Обчислення позиції в гріді
                Vector3 snappedPosition = new Vector3(
                    Mathf.Round(hitPosition.x / gridCellSize.x) * gridCellSize.x,
                    Mathf.Round(hitPosition.y / gridCellSize.y) * gridCellSize.y,
                    Mathf.Round(hitPosition.z / gridCellSize.z) * gridCellSize.z
                );

                if (eraseMode)
                {
                    // Видалення об'єкта
                    Collider[] colliders = Physics.OverlapSphere(snappedPosition, 0.1f);
                    foreach (Collider collider in colliders)
                    {
                        if (collider.gameObject != null)
                        {
                            Undo.DestroyObjectImmediate(collider.gameObject);
                        }
                    }
                }
                else
                {
                    // Створення об'єкта
                    GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(objectToPaint);
                    Undo.RegisterCreatedObjectUndo(newObject, "Paint Object");
                    newObject.transform.position = snappedPosition;
                }
            }

            e.Use();
        }
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
}
