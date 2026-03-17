Shader "Custom/PeppersGhostSplit"
{
    Properties
    {
        _MainTex ("Render Texture", 2D) = "black" {}
        _Quadrant ("Quadrant (0=Top 1=Bottom 2=Left 3=Right)", Float) = 0
    }

    SubShader
    {
        //transparent so discarded pixels show through
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        //I dont really understand this, copilot is just helping me comment it 

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            // Attributes are the data passed from the mesh to the vertex shader.
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            // Varyings are the data passed from the vertex shader to the fragment shader.
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float _Quadrant;
            
            // Vertex shader: Transforms object space to homogeneous clip space and passes UVs through.
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv          = IN.uv;
                return OUT;
            }

            //fragment shader which discards pixels outside the desired quadrant and samples the texture for the rest
            half4 frag(Varyings IN) : SV_Target
            {
                float u = IN.uv.x;
                float v = IN.uv.y;

                float aspect = 16.0 / 9.0; 
                float correctedU = (u - 0.5) / aspect + 0.5;
                float2 correctedUV = float2(correctedU, v);

                //every quad is the bottom quad but displaced angularily 
                bool keep = (u + v < 1.0) && (v < u);

                if (!keep) discard;

                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, correctedUV);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}
