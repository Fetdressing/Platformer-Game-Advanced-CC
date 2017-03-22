using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ColorReplacement : MonoBehaviour {
    //public Shader m_shader;
    private Material m_material;
    private Texture2D m_replacementTexture;

    public int m_resolution = 32;

    public Color colorTL = Color.red;
    public Color colorTR = Color.blue;
    public Color colorBL = Color.green;
    public Color colorBR = Color.white;
	// Use this for initialization
	void Start () {
        m_material = GetComponent<Renderer>().sharedMaterial;
        m_replacementTexture = new Texture2D(m_resolution, m_resolution);

        CreateGradientTexture();
    }

    private void CreateGradientTexture()
    {
        m_material = GetComponent<Renderer>().sharedMaterial;
        m_replacementTexture = new Texture2D(m_resolution, m_resolution);
        for (int i = 0; i < m_resolution; i++)
        {
            float fromLeft = (float)i / (float)m_resolution;
            //Debug.Log(fromLeft);
            float fromRight = 1.0f - fromLeft;
            Color tl = colorTL * fromRight;
            Color tr = colorTR * fromLeft;
            Color verticalWeight = tl + tr;

            for (int j = 0; j < m_resolution; j++)
            {
                if(j == 0)
                {

                }
                float fromTop = (float)j / (float)m_resolution;
                Color bl = colorBL * fromRight;
                Color br = colorBR * fromLeft;
                Color pixelColor = ((tl + tr) * fromTop) + ((bl + br) * (1.0f - fromTop));
                m_replacementTexture.SetPixel(i, j, pixelColor);
            }
        }

        m_material.mainTexture = m_replacementTexture;
        m_replacementTexture.Apply();
    }

    private void OnValidate()
    {
        CreateGradientTexture();
    }

    // Update is called once per frame
    void Update () {
		
	}
}
