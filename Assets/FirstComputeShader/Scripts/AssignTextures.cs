using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignTextures : MonoBehaviour
{
    public ComputeShader shader;
    public int texResolution = 256;

    private Renderer m_renderer;
    private RenderTexture  m_outputTexture;
    private int m_KernelHandle;
    
    // Start is called before the first frame update
    void Start()
    {
        m_outputTexture = new RenderTexture(texResolution, texResolution, 0);
        m_outputTexture.enableRandomWrite = true;
        m_outputTexture.Create();

        m_renderer = GetComponent<Renderer>();
        m_renderer.enabled = true;

        InitShader();
    }

    private void InitShader()
    {
        m_KernelHandle = shader.FindKernel("SolidSquare");
        shader.SetInt("texResolution", texResolution);
        shader.SetTexture(m_KernelHandle, "Result", m_outputTexture);
        m_renderer.material.SetTexture("_MainTex", m_outputTexture);

        DispatchShader(texResolution/8, texResolution / 8);
    }

    private void DispatchShader(int x, int y)
    {
        shader.Dispatch(m_KernelHandle, x, y, 1);
    }

    void OnDestory()
    {
        m_outputTexture.Release();
    }
}
