using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Watermelon.SquadShooter
{
    [CustomEditor(typeof(StartPointHandles))]
    public class StartPointHandlesEditor : Editor
    {

        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
        static void DrawHandles(StartPointHandles startPointHandles, GizmoType gizmoType)
        {
            Handles.color = startPointHandles.diskColor;
            Handles.DrawWireDisc(startPointHandles.transform.position, startPointHandles.transform.up, startPointHandles.diskRadius, startPointHandles.thickness);

            if (!startPointHandles.displayText)
            {
                return;
            }

            Color backupColor = GUI.color;
            GUI.color = startPointHandles.textColor;

            if (startPointHandles.useTextVariable)
            {
                Handles.Label(startPointHandles.transform.position + startPointHandles.textPositionOffset, startPointHandles.text);
            }
            else
            {
                Handles.Label(startPointHandles.transform.position + startPointHandles.textPositionOffset, startPointHandles.gameObject.name);
            }

            GUI.color = backupColor;
        }

    }
}