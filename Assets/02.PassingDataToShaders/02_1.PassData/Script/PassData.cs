using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
    private int m_count = 10;
    
    struct  Circle
    {
        public Vector2 origin;
        public Vector2 velocity;
        public float radius;
    }

    private Circle[] circleData;
    private ComputeBuffer buffer;
    
    // Start is called before the first frame update
    void Start()
    {
        m_outputTexture = new RenderTexture(texResolution, texResolution, 0);
        m_outputTexture.enableRandomWrite = true;
        m_outputTexture.Create();

        m_mat = GetComponent<Renderer>().material;
        m_mat.SetTexture("_MainTex", m_outputTexture);
        
        InitData();
        InitShader();
    }

    void InitData()
    {
        m_handleCircle = computeShader.FindKernel("Circles");

        uint threadGroupSizeX;
        computeShader.GetKernelThreadGroupSizes(m_handleCircle, out threadGroupSizeX, out _, out _);
        int total = (int)threadGroupSizeX * m_count;
        circleData = new Circle[total];

        float speed = 100;
        float halfSpeed = speed * 0.5f;
        float minRadius = 10.0f;
        float maxRadius = 30.0f;
        float radiusRange = maxRadius - minRadius;

        for (int i = 0; i < total; ++i)
        {
            Circle circle = circleData[i];
            circle.origin.x = Random.value * texResolution;
            circle.origin.y = Random.value * texResolution;
            circle.velocity.x = (Random.value * speed) - halfSpeed;
            circle.velocity.y = (Random.value * speed) - halfSpeed;
            circle.radius = (Random.value * radiusRange) + minRadius;
            circleData[i] = circle;
        }
    }
    
    void InitShader()
    {
        m_handleClearColor = computeShader.FindKernel("Clear");
    	
        computeShader.SetVector( "clearColor", clearColor );
        computeShader.SetVector( "circleColor", circleColor );
        computeShader.SetInt( "texResolution", texResolution );

        int stride = (2 + 2 + 1) * sizeof(float);
        buffer = new ComputeBuffer(circleData.Length, stride);
        buffer.SetData(circleData);
        computeShader.SetBuffer(m_handleCircle, "circlesBuffer", buffer);
        
        computeShader.SetTexture(m_handleClearColor, "Result", m_outputTexture);
        computeShader.SetTexture(m_handleCircle, "Result", m_outputTexture);
        
        
    }

    void DispatchKernels(int count)
    {
        computeShader.Dispatch(m_handleClearColor, texResolution/8, texResolution/8, 1);
        computeShader.SetFloat("time", Time.time);
        computeShader.Dispatch(m_handleCircle, count, 1, 1);
    }
    
    // Update is called once per frame
    void OnDisable()
    {
        m_outputTexture.Release();
        
    }

    void OnDestroy()
    {
        buffer.Dispose();
    }
    
    private void Update()
    {
        DispatchKernels(10);
    }
}
