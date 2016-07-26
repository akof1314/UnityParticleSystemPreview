using System;
using System.Reflection;
using UnityEditor;

public static class InspectorWindowUtil
{
    public static Action repaintAllInspectors;

    public static void Init()
    {
        if (repaintAllInspectors != null)
        {
            return;
        }
        Assembly assembly = Assembly.GetAssembly(typeof(EditorGUIUtility));
        Type type = assembly.GetType("UnityEditor.InspectorWindow");
        MethodInfo repaintAllInspectorsInfo = type.GetMethod("RepaintAllInspectors", BindingFlags.NonPublic | BindingFlags.Static);
        repaintAllInspectors = (Action)Delegate.CreateDelegate(typeof (Action), repaintAllInspectorsInfo);
    }
}
