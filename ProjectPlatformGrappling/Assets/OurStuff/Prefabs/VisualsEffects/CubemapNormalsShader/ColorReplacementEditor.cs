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

    private void CreateTextureConfigLayouts()
    {

        //GUILayout.BeginVertical();
        //GUILayout.BeginHorizontal();

        int textureConfigCount = m_shaderController.m_texConfigs.Length;
        if (textureConfigCount > 00)
        {
            for (int i = 0; i < textureConfigCount; i++)
            {
                //Debug.Log("lol");
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                //EditorGUILayout.LabelField(SetTextureLabels(i, textureConfigCount), GUILayout.MaxWidth(100));
                m_shaderController.m_texConfigs[i].name = EditorGUILayout.TextField(SetTextureLabels(i, textureConfigCount) ,m_shaderController.m_texConfigs[i].name);
                GUILayout.Button("Load...", GUILayout.MaxWidth(50), GUILayout.MinWidth(40));

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
        }
        //GUILayout.EndHorizontal();
        //GUILayout.EndVertical();
    }

    private string SetTextureLabels(int index, int maxCount)
    {
        string returnString = "";
        if (maxCount == 1)
        {
            returnString = "Control Texture";
        }
        else
        {
            switch(index)
            {
                case 0:
                    returnString = "Top";
                    break;
                case 1:
                    returnString = "Bottom";
                    break;
                default:
                    returnString = "Texture";
                    break;
            }
        }

        return returnString;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SerializedProperty textures = serializedObject.FindProperty("m_texConfigs");
        SerializedProperty config = serializedObject.FindProperty("m_shaderType");
        SerializedProperty resolution = serializedObject.FindProperty("m_resolution");
        //Debug.Log(textures);

        CreateTextureConfigLayouts();

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
