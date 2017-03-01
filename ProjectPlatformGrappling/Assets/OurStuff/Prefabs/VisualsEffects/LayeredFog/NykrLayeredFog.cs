using UnityEngine;
using System;

[ExecuteInEditMode]
[RequireComponent (typeof(Camera))]
[AddComponentMenu ("Nykr/NykrLayeredFog")]

public class NykrLayeredFog : UnityStandardAssets.ImageEffects.PostEffectsBase
{
    
    [Tooltip("Apply distance-based fog")]
    public bool m_distanceFog = true;
    public FogMode m_fogMode = FogMode.Linear;
    [Range(0.0f, 0.1f)]
    public float m_fogDensity = 0.01f;
    public float m_fogStartDistance;
    public float m_fogEndDistance;
    [Tooltip("Exclude far plane pixeld from distance-based fog")]
    public bool m_excludeFarPixels = true;
    public bool m_useRadialDistance = false;
    public bool m_heightFog = false;
    public float m_height = 1.0f;
    [Range(0.001f, 10.0f)]
    public float m_heightDensity = 2.0f;
    public float m_startDistance = 0.0f;
    public Gradient m_gradient;

    public Shader m_fogShader = null;
    
    private Material m_fogMaterial = null;
    private Texture2D m_fogTexture = null;
    //var sceneMode = RenderSettings.fogMode;
    //var sceneDensity = RenderSettings.fogDensity;
    //var sceneStart = RenderSettings.fogStartDistance;
    //var sceneEnd = RenderSettings.fogEndDistance;


    public override bool CheckResources()
    {
        CheckSupport(true);
        m_fogMaterial = CheckShaderAndCreateMaterial(m_fogShader, m_fogMaterial);
        if(!isSupported)
        {
            ReportAutoDisable();
        }
        return isSupported;
    }
    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture p_source, RenderTexture p_destination)
    {
        if(CheckResources()==false || (!m_distanceFog && !m_heightFog))
        {
            Graphics.Blit(p_source, p_destination);
            return;
        }
        Camera cam = GetComponent<Camera>();
        Transform camTransform = cam.transform;
        float camNear = cam.nearClipPlane;
        float camFar = cam.farClipPlane;
        float camFov = cam.fieldOfView;
        float camAspect = cam.aspect;

        Matrix4x4 frustumCorners = Matrix4x4.identity;

        float fovWHalfv = camFov * 0.5f;

        Vector3 toRight = camTransform.right * camNear * Mathf.Tan(fovWHalfv * Mathf.Deg2Rad) * camAspect;
        Vector3 toTop = camTransform.up * camNear * Mathf.Tan(fovWHalfv * Mathf.Deg2Rad);
        Vector3 topLeft = (camTransform.forward * camNear - toRight + toTop);
        float camScale = topLeft.magnitude * camFar / camNear;

        topLeft.Normalize();
        topLeft *= camScale;

        Vector3 topRight = (camTransform.forward * camNear + toRight + toTop);
        topRight.Normalize();
        topRight *= camScale;


        Vector3 bottomRight = (camTransform.forward * camNear + toRight - toTop);
        bottomRight.Normalize();
        bottomRight *= camScale;

        Vector3 bottomLeft = (camTransform.forward * camNear - toRight - toTop);
        bottomLeft.Normalize();
        bottomLeft *= camScale;

        frustumCorners.SetRow(0, topLeft);
        frustumCorners.SetRow(1, topRight);
        frustumCorners.SetRow(2, bottomRight);
        frustumCorners.SetRow(3, bottomLeft);

        var camPos = camTransform.position;
        float FdotC = camPos.y - m_height;
        float paramk = (FdotC <= 0.0f ? 1.0f : 0.0f);
        float excludeDepth = (m_excludeFarPixels ? 1.0f : 2.0f);
        m_fogMaterial.SetMatrix("_FrustumCornersWS", frustumCorners);
        m_fogMaterial.SetVector("_CameraWS", camPos);
        m_fogMaterial.SetVector("_HeightParams", new Vector4(m_height, FdotC, paramk, m_heightDensity * 0.5f));
        m_fogMaterial.SetVector("_DistanceParams", new Vector4(-Mathf.Max(m_startDistance, 0.0f), excludeDepth, 0.0f, 0.0f));

        var sceneMode = m_fogMode;
        var sceneDensity = (m_fogDensity * 0.1);
        var sceneStart = m_fogStartDistance;
        var sceneEnd = m_fogEndDistance;
        
        bool linear = (sceneMode == FogMode.Linear);
        float diff = linear ? sceneEnd - sceneStart : 0.0f;
        float invDiff = Mathf.Abs(diff) > 0.0001f ? 1.0f / diff : 0.0f;

        Vector4 sceneParams;
        sceneParams.x = (float)sceneDensity * 1.2011224087f; // density /sqrt(ln(2)), used by Exp2 fog mode
        sceneParams.y = (float)sceneDensity * 1.4426950408f; // density / ln(2), used by Exp for mode
        sceneParams.z = linear ? -invDiff : 0.0f;
        sceneParams.w = linear ? sceneEnd * invDiff : 0.0f;

        m_fogMaterial.SetVector("_SceneFogParams", sceneParams);

        int gradientKeyCount = CreateTextureFromGradient(m_gradient);


        m_fogMaterial.SetVector("_SceneFogMode", new Vector4((int)sceneMode, m_useRadialDistance ? 1 : 0, gradientKeyCount, sceneEnd));
        m_fogMaterial.SetTexture("_GradientTexture", m_fogTexture);
        

        int pass = 0;
        if (m_distanceFog && m_heightFog)
            pass = 0;
        else if (m_distanceFog)
            pass = 1;
        else
            pass = 2;
        CustomGraphicsBlit(p_source, p_destination, m_fogMaterial, pass);
    }

    int CreateTextureFromGradient(Gradient p_gradient)
    {

        //Create texture with the same size as the number of gradient keys
        //Alpha channel fort each is populated with time to enable interpolation in shader
        Matrix4x4 keyMatrix =  Matrix4x4.identity;
        Vector4 row1 = Vector4.zero;
        Vector4 row2 = Vector4.zero;
        int keyCount = p_gradient.colorKeys.Length;
        m_fogTexture = new Texture2D(100, 1);
        for(int i = 0; i<100; i++)
        {
            float keyTime = i * .01f;
            Color keyColor = p_gradient.Evaluate(keyTime);
            m_fogTexture.SetPixel(i, 1, keyColor); 
        }
        m_fogTexture.filterMode = FilterMode.Bilinear;
        //m_fogTexture.wrapMode = TextureWrapMode.Repeat;
        m_fogTexture.Apply();

        //keyMatrix.SetRow(1, row1);
        //keyMatrix.SetRow(2, row2);
        m_fogMaterial.SetVector("_GradientKeyTimes1", row1);
        m_fogMaterial.SetVector("_GradientKeyTimes2", row2);
        //m_fogMaterial.SetMatrix("_GradientKeyTimes", keyMatrix);


        return keyCount;
    }

    static void CustomGraphicsBlit(RenderTexture p_source, RenderTexture p_dest, Material p_fxMaterial, int p_passNr)
    {
        RenderTexture.active = p_dest;

        p_fxMaterial.SetTexture("_MainTex", p_source);
        //p_fxMaterial.SetTexture("_GradientTexture", p_source);
        //p_fxMaterial.SetTexture("_GradientTexture",)
        GL.PushMatrix();
        GL.LoadOrtho();

        p_fxMaterial.SetPass(p_passNr);

        GL.Begin(GL.QUADS);

        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 3.0f); // BL

        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f); // BR

        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f); // TR

        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f); // TL

        GL.End();
        GL.PopMatrix();


    }
}
