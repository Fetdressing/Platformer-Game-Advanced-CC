using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor (typeof(ColorReplacement3D))]
public class ColorReplacementEditor : Editor
{

    public Texture2D tex1;
    private ColorReplacement3D m_shaderController;

    public void OnEnable()
    {
        m_shaderController = (ColorReplacement3D)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SerializedProperty textures = serializedObject.FindProperty("m_texConfigs");
        SerializedProperty config = serializedObject.FindProperty("m_shaderType");
        SerializedProperty resolution = serializedObject.FindProperty("m_resolution");
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(resolution, true);
        EditorGUILayout.PropertyField(config, true);
        EditorGUILayout.PropertyField(textures, true);
        
        if(EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }

        if(GUILayout.Button("Redraw"))
        {
            m_shaderController.RedrawCubemap();
        }
        if(GUILayout.Button("Save Textures"))
        {
            m_shaderController.SaveTextures();
        }
        //GUILayout.Space(50);

        //SetCreationMethod();
        //GUILayout.BeginVertical();
        //GUILayout.BeginHorizontal();
        //m_shaderController.a00 = EditorGUILayout.ColorField("A 0 0", m_shaderController.a00);
        //m_shaderController.a10 = EditorGUILayout.ColorField("A 1 0", m_shaderController.a10);
        //GUILayout.EndHorizontal();
        //GUILayout.EndVertical();


        //base.OnInspectorGUI();
    }

}
