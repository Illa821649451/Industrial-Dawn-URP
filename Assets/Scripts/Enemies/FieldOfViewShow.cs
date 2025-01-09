using UnityEditor;
using UnityEngine;

//[CustomEditor(typeof(EnemyRevolver))]
public class FieldOfViewShow// : Editor
{
    /*private void OnSceneGUI()
    {
        
        EnemyRevolver fov = (EnemyRevolver)target;
        Vector3 posForDrawing = new Vector3(fov.transform.position.x, fov.transform.position.y + 3f, fov.transform.position.z);
        Handles.color = Color.white;
        Handles.DrawWireArc(posForDrawing, Vector3.up, Vector3.forward, 360, fov.viewRadius);

        Vector3 viewAngle01 = DirectionFromAngle(fov.transform.eulerAngles.y, -fov.viewAngle / 2);
        Vector3 viewAngle02 = DirectionFromAngle(fov.transform.eulerAngles.y, fov.viewAngle / 2);

        Handles.color = Color.yellow;
        Handles.DrawLine(posForDrawing, posForDrawing + viewAngle01 * fov.viewRadius);
        Handles.DrawLine(posForDrawing, posForDrawing + viewAngle02 * fov.viewRadius);

        if (fov.canSeePlayer)
        {
            Handles.color = Color.green;
            Handles.DrawLine(posForDrawing, fov.playerRef.transform.position + new Vector3(0,2,0));
        }
    }

    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }*/
}
