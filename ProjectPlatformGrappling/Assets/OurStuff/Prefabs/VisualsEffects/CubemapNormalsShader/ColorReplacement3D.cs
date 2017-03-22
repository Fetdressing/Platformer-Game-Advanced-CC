using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[ExecuteInEditMode]
public class ColorReplacement3D : MonoBehaviour
{
    public enum ShaderTypes { Simple, BaseGradient, TopBottomBased };
    //public Shader m_shader;
    private Material m_material;
    private Cubemap m_cubemap;

    public int m_resolution = 32;

    public ShaderTypes m_shaderType = ShaderTypes.Simple;
    private ShaderTypes m_currentType = ShaderTypes.Simple;

    public Color a00 = Color.white;
    public Color a10 = Color.green;
    public Color a01 = Color.blue;
    public Color a11 = Color.red;


    [System.Serializable]
    public class CubeTextureConfig
    {
        public Color a00 = Color.white;
        public Color a10 = Color.green;
        public Color a01 = Color.blue;
        public Color a11 = Color.red;
    }

    public CubeTextureConfig[] m_texConfigs;

    // Use this for initialization
    void Start()
    {
        m_material = GetComponent<Renderer>().sharedMaterial;
        SetupConfig();
        GenerateCubeMap();
        m_currentType = m_shaderType;
    }

    private void OnValidate()
    {
        m_material = GetComponent<Renderer>().sharedMaterial;
        SetupConfig();
        m_currentType = m_shaderType;
        GenerateCubeMap();
    }

    public void RedrawCubemap()
    {
        SetupConfig();
        m_currentType = m_shaderType;
        GenerateCubeMap();
    }
    private void SetupConfig()
    {
        int mapCount = 1;
        switch (m_shaderType)
        {
            case ShaderTypes.Simple:
                mapCount = 1;
                break;
            case ShaderTypes.BaseGradient:
                mapCount = 1;
                break;
            case ShaderTypes.TopBottomBased:
                mapCount = 2;
                break;
            default:
                mapCount = 1;
                break;
        }
        m_currentType = m_shaderType;

        CubeTextureConfig[] temp = new CubeTextureConfig[mapCount];
        if (m_texConfigs.Length > 0)
        {
            if (m_texConfigs.Length >= mapCount)
            {
                for (int i = 0; i < m_texConfigs.Length; i++)
                {
                    if (i < temp.Length)
                    {
                        temp[i] = m_texConfigs[i];
                    }
                }
            }
            else
            {
                for (int i = 0; i < mapCount; i++)
                {
                    if (i < m_texConfigs.Length)
                    {
                        temp[i] = m_texConfigs[i];
                    }
                    else
                    {
                        temp[i] = new CubeTextureConfig();
                    }
                }
            }

            m_texConfigs = new CubeTextureConfig[mapCount];
            m_texConfigs = temp;
        }
        else
        {
            m_texConfigs = new CubeTextureConfig[mapCount];
        }
    }

    private void GenerateCubeMap()
    {
        switch (m_shaderType)
        {
            case ShaderTypes.Simple:
                CreateFlatsideTexures();
                break;
            case ShaderTypes.BaseGradient:
                CreateSimpleGradientTexture();
                break;
            case ShaderTypes.TopBottomBased:
                CreateTopBottomBasedGradient();
                break;
            default:
                CreateFlatsideTexures();
                break;
        }
    }
    private Color[] GetFlatSideColor(Color p_inColor)
    {
        Color[] flatSideColor = new Color[m_resolution * m_resolution];
        for (int i = 0; i < m_resolution * m_resolution; i++)
        {
            flatSideColor[i] = p_inColor;
        }
        return flatSideColor;
    }

    private void CreateTopBottomBasedGradient()
    {
        if (m_texConfigs.Length == 2)
        {
            Texture2D top, bottom, front, back, right, left;
            top = bottom = front = back = right = left = new Texture2D(m_resolution, m_resolution);
            top = GenerateGradient(m_texConfigs[0].a00, m_texConfigs[0].a10, m_texConfigs[0].a01, m_texConfigs[0].a11);
            bottom = GenerateGradient(m_texConfigs[1].a00, m_texConfigs[1].a10, m_texConfigs[1].a01, m_texConfigs[1].a11);
            front = GenerateGradient(m_texConfigs[0].a01, m_texConfigs[0].a11, m_texConfigs[1].a00, m_texConfigs[1].a10);
            back = GenerateGradient(m_texConfigs[0].a10, m_texConfigs[0].a00, m_texConfigs[1].a11, m_texConfigs[1].a01);

            right = GenerateGradient(m_texConfigs[0].a00, m_texConfigs[0].a01, m_texConfigs[1].a01, m_texConfigs[1].a00);
            left = GenerateGradient(m_texConfigs[0].a11, m_texConfigs[0].a10, m_texConfigs[1].a10, m_texConfigs[1].a11);

            ApplyToFaces(top, bottom, right, left, front, back);
        }

    }

    private Texture2D GenerateGradient(Color c4, Color c3, Color c2, Color c1)
    {
        Texture2D faceTexture = new Texture2D(m_resolution, m_resolution);

        for (int i = 0; i < m_resolution; i++)
        {
            float fromLeft = (float)i / (float)m_resolution;
            //Debug.Log(fromLeft);
            float fromRight = 1.0f - fromLeft;
            Color tl = c1 * fromRight;
            Color tr = c2 * fromLeft;
            Color verticalWeight = tl + tr;

            for (int j = 0; j < m_resolution; j++)
            {
                float fromTop = (float)j / (float)m_resolution;
                Color bl = c3 * fromRight;
                Color br = c4 * fromLeft;
                Color pixelColor = ((tl + tr) * fromTop) + ((bl + br) * (1.0f - fromTop));
                faceTexture.SetPixel(i, j, pixelColor);
            }
        }

        return faceTexture;
    }

    private void CreateSimpleGradientTexture()
    {
        Texture2D[] sides = new Texture2D[6];

        for (int i = 0; i < 6; i++)
        {
            sides[i] = GenerateGradient(m_texConfigs[0].a11, m_texConfigs[0].a01, m_texConfigs[0].a10, m_texConfigs[0].a00);
        }

        ApplyToFaces(sides[0], sides[1], sides[2], sides[3], sides[4], sides[5]);

    }

    private void ApplyToFaces(Texture2D top, Texture2D down, Texture2D right, Texture2D left, Texture2D front, Texture2D back)
    {
        m_cubemap = new Cubemap(m_resolution, TextureFormat.ARGB32, false);

        m_cubemap.SetPixels(top.GetPixels(), CubemapFace.PositiveY); // Up
        m_cubemap.SetPixels(down.GetPixels(), CubemapFace.NegativeY); // Down

        m_cubemap.SetPixels(right.GetPixels(), CubemapFace.PositiveX); // Right
        m_cubemap.SetPixels(left.GetPixels(), CubemapFace.NegativeX); // Left

        m_cubemap.SetPixels(front.GetPixels(), CubemapFace.PositiveZ); // Front
        m_cubemap.SetPixels(back.GetPixels(), CubemapFace.NegativeZ); // Back

        m_material.SetTexture("_cubeTex", m_cubemap);
        m_cubemap.Apply();
    }



    private void CreateFlatsideTexures()
    {

        m_cubemap = new Cubemap(m_resolution, TextureFormat.ARGB32, false);
        m_cubemap.SetPixels(GetFlatSideColor(m_texConfigs[0].a11), CubemapFace.PositiveY); // Up
        m_cubemap.SetPixels(GetFlatSideColor(m_texConfigs[0].a00), CubemapFace.NegativeY); // Down

        m_cubemap.SetPixels(GetFlatSideColor(m_texConfigs[0].a01), CubemapFace.PositiveX); // Right
        m_cubemap.SetPixels(GetFlatSideColor(m_texConfigs[0].a01), CubemapFace.PositiveZ); // Front

        m_cubemap.SetPixels(GetFlatSideColor(m_texConfigs[0].a10), CubemapFace.NegativeX); // Left
        m_cubemap.SetPixels(GetFlatSideColor(m_texConfigs[0].a10), CubemapFace.NegativeZ); // Back

        m_material.SetTexture("_cubeTex", m_cubemap);
        m_cubemap.Apply();
    }

    public void SaveTextures()
    {
        for (int i = 0; i < m_texConfigs.Length; i++)
        {
            Texture2D tex = new Texture2D(m_resolution, m_resolution, TextureFormat.RGB24, false);
            if (i == 0)
            {
                tex.SetPixels(m_cubemap.GetPixels(CubemapFace.PositiveY));
            }
            else
            {
                tex.SetPixels(m_cubemap.GetPixels(CubemapFace.NegativeY));
            }
            byte[] bytes = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);
            //print(CreateNameAndFilePath(i));
            //File.WriteAllBytes(Application.dataPath + "/OurStuff/Prefabs/VisualEffects/CubemapNormalsShader/Textures/t", bytes);
            File.WriteAllBytes(CreateNameAndFilePath(i), bytes);
        }
    }

    private string CreateNameAndFilePath(int index = 0, string name = "temptex")
    {
        string outString = "";
        string specificPath = string.Format("/OurStuff/Prefabs/VisualsEffects/CubemapNormalsShader/Textures/")  ;
        outString = string.Format("{0}{1}{2}_{3}.png", Application.dataPath, specificPath, name, index.ToString());
        //outString = string.Format("{0}/tex{1}.png", Application.dataPath, index.ToString());

        // Debug out
        //print(outString);
        return @outString;
    }
}
