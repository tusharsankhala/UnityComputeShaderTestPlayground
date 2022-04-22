using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassData : MonoBehaviour
{
    public ComputeShader computeShader;
    public int texResolution;
    public Color clearColor;
    public Color circleColor;

    private int m_handleCircle;
    private int m_handleClearColor;
    
    private RenderTexture m_outputTexture;
    private Material m_mat;
    
    // Start is called before the first frame update
    void Start()
    {
        m_outputTexture = new RenderTexture(texResolution, texResolution, 0);
        m_outputTexture.enableRandomWrite = true;
        m_outputTexture.Create();

        InitShader();
    }

    void InitShader()
    {
        m_handleCircle = computeShader.FindKernel("Circles");
        m_handleClearColor = computeShader.FindKernel("ClearColor");
        
        computeShader.SetVector("clearColor", clearColor);
        computeShader.SetVector("circleColor", circleColor);
        computeShader.SetInt("texResolution", texResolution);
        
        computeShader.SetTexture(m_handleClearColor, "Result", m_outputTexture);
        computeShader.SetTexture(m_handleCircle, "Result", m_outputTexture);
        
        m_mat = GetComponent<Renderer>().material;
        m_mat.SetTexture("_MainTex", m_outputTexture);
        
        
    }

    void DispatchKernels(int count)
    {
        computeShader.SetFloat("time", Time.deltaTime);
        computeShader.Dispatch(m_handleClearColor, texResolution/8, texResolution/8, 1);
        computeShader.Dispatch(m_handleCircle, count, 1, 1);
    }
    
    // Update is called once per frame
    void OnDisable()
    {
        m_outputTexture.Release();
        
    }

    private void Update()
    {
        DispatchKernels(32);
    }
}
