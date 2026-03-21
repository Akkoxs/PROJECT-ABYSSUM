Shader "Custom/PeppersGhostSplit"
{
    Properties
    {
        _MainTex ("Render Texture", 2D) = "black" {} //the input texture which is the output of the camera 
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" } //transparent means it renders after all opaque 
        Blend SrcAlpha OneMinusSrcAlpha //alpha blending for transparency
        ZWrite Off //doesnt write to depth buffer so it wont occlude objects behind 
        Cull Off //renders both front and back faces of the mesh, ensures visibility from all angles

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            //a struct to define the input attributes for the vertex shader (object space position and UV coordinates)
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            //a struct to define data passed from the vertex shader to the fragment shader (homogeneous clip space position and UV coords)
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
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz); //transforms vertex positions from object space to homogeneous clip space using a built-in function
                OUT.uv          = IN.uv;
                return OUT;
            }

            //fragment shader which discards pixels outside the desired quadrant and samples the texture for the rest
            half4 frag(Varyings IN) : SV_Target
            {
                float u = IN.uv.x;
                float v = IN.uv.y;

                float aspect = 16.0 / 9.0; //ensures a certain aspect ratio 
                float correctedU = (u - 0.5) / aspect + 0.5; //adjust the UV coords for aspect 
                float2 correctedUV = float2(correctedU, v); //packages corrected UV coords.

                //this is where the fragmentation magic occurs
                //we make 2 cuts across the texture to get ourselves our isosceles quadrant in UV space (imagine a square, we cut twice diagonally across it and only keep the bottom quadrant)
                //in splitscreen.cs we take this and repeat it 4 times with a certain angular displacement  
                //(u+v < 1.0) ensures we are below the diagonal line from (0,1) to (1,0) and (v < u) ensures we are to the right of the diagonal line from (0,0) to (1,1)
                bool keep = (u + v < 1.0) && (v < u);
                if (!keep) discard;

                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, correctedUV);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}
