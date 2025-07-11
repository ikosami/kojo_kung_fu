using UnityEngine;
using UnityEditor;
using System.Reflection;

public class GenericBinderEditor : EditorWindow
{
    Component componentA;
    Component componentB;

    [MenuItem("Tools/Generic Binder")]
    static void Init()
    {
        GetWindow<GenericBinderEditor>("Generic Binder");
    }

    void OnGUI()
    {
        componentA = EditorGUILayout.ObjectField("Component A", componentA, typeof(Component), true) as Component;
        componentB = EditorGUILayout.ObjectField("Component B", componentB, typeof(Component), true) as Component;

        if (GUILayout.Button("Bind Same-Named Fields"))
        {
            if (componentA == null || componentB == null) return;
            BindSameNamedFields(componentA, componentB);
        }
    }

    void BindSameNamedFields(Component a, Component b)
    {
        var aFields = a.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var bFields = b.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var aField in aFields)
        {
            foreach (var bField in bFields)
            {
                if (aField.Name == bField.Name && aField.FieldType == bField.FieldType)
                {
                    bField.SetValue(b, aField.GetValue(a));
                }
            }
        }

        EditorUtility.SetDirty(b);
    }
}
