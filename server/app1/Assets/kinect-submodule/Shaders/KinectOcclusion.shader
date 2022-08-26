// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/KinectOcclusion"
{
    Properties
    {
        _KinectRGBTex("Texture", 2D) = "white" {}
        _KinectDepthTex("Texture", 2D) = "white" {}
        _MainCameraRGBTex("Texture", 2D) = "white" {}

        _DebugDisplayDepth("Debug display depth (>0 if display)", Range(0,1)) = 0
        _UnityCamSupport("Support UnityCam (>0 if unity cam is active)", Range(0,1)) = 0
        _VerticalMirrorProcessedImage("Vertical mirror (>0 if mirror is active)", Range(0,1)) = 0
        _HorizontalMirrorProcessedImage("Horizontal mirror (>0 if mirror is active)", Range(0,1)) = 0

        _KinectDepthScale("Depth scale from kinect ", Range(0,3)) = 0.5
        _UnityDepthScale("Depth scale from unity camera", Range(0,3)) = 0.5
        _DepthClippingDistance("Depth clipping distance ", Range(0,100)) = 20
    }
    
    SubShader
    {
        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            //#pragma target 5.0
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _KinectRGBTex;
            float4 _KinectRGBTex_ST;
            uniform half4 _KinectRGBTex_TexelSize;

            // compute buffer = only DirectX11 !
            uniform StructuredBuffer<float> _KinectDepthBuffer;
            uniform StructuredBuffer<float> _KinectDepthSpaceBufferX;
            uniform StructuredBuffer<float> _KinectDepthSpaceBufferY;
            uniform StructuredBuffer<float2> _KinectDepthSpaceBuffer;

            int _KinectDepthHeight;
            int _KinectDepthWidth;
            int _KinectColorHeight;
            int _KinectColorWidth;

            sampler2D _MainCameraRGBTex;
            float4 _MainCameraRGBTex_ST;

            sampler2D _CameraDepthTexture;
            sampler2D _CameraTexture;

            uniform fixed _KinectDepthScale;
            uniform fixed _UnityDepthScale;
            uniform fixed _DepthClippingDistance;

            int _DebugDisplayDepth;
            int _UnityCamSupport;
            int _VerticalMirrorProcessedImage;
            int _HorizontalMirrorProcessedImage;


            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.uv);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = float4(0,0,1,1);


                // mirror depth
                i.vertex.x = _KinectColorWidth - i.vertex.x;


                i.uv.y = 1 - i.uv.y;
                float depth = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.uv));
                i.uv.y = 1 - i.uv.y;
                //depth = pow(Linear01Depth(depth), _DepthLevel);
                depth = LinearEyeDepth(depth);

                int indexColor = -1;
                if (_UnityCamSupport > 0)
                {
                    indexColor = (_KinectColorHeight - i.vertex.y) * _KinectColorWidth + (i.vertex.x - _KinectColorWidth / 2);
                }
                else
                {
                    indexColor = (i.vertex.y) * _KinectColorWidth + (i.vertex.x - _KinectColorWidth / 2);
                }

                float d_x = _KinectDepthSpaceBuffer[indexColor].x;
                float d_y = _KinectDepthSpaceBuffer[indexColor].y;

                int indexDepth = d_y * _KinectDepthWidth + d_x;
            
                //https://answers.unity.com/questions/877170/render-scene-depth-to-a-texture.html

                // debug
                float4 dU = float4(depth * _UnityDepthScale,
                    depth * _UnityDepthScale,
                    depth * _UnityDepthScale, 1);

                float4 dK = float4(_KinectDepthBuffer[indexDepth] * _KinectDepthScale,
                    _KinectDepthBuffer[indexDepth] * _KinectDepthScale,
                    _KinectDepthBuffer[indexDepth] * _KinectDepthScale,1);

                
                if (depth > _DepthClippingDistance)
                {
                    // main camera seeing nothing <=> no virtual obstacle
                    i.uv.x = 1 - i.uv.x;
                    col = tex2D(_KinectRGBTex, i.uv);
                    if (_DebugDisplayDepth > 0) { col = dK; }
                }
                else if (_KinectDepthBuffer[indexDepth] <= 0)
                {
                    // no depth from kinect <=> virtual object in front
                    i.uv.y = 1 - i.uv.y;
                    col = tex2D(_MainCameraRGBTex, i.uv);
                    if (_DebugDisplayDepth > 0) { col = dU; }

                }
                else if (depth * _UnityDepthScale < _KinectDepthBuffer[indexDepth] * _KinectDepthScale)
                {
                    i.uv.y = 1 - i.uv.y;
                    col = tex2D(_MainCameraRGBTex, i.uv);
                    if (_DebugDisplayDepth > 0) { col = dU; }
                }
                else
                {
                    i.uv.x = 1 - i.uv.x;
                    col = tex2D(_KinectRGBTex, i.uv);
                    if (_DebugDisplayDepth > 0) { col = dK; }
                }

                // debug : display mapped kinect depth map
                //int index = i.vertex.y * _KinectDepthWidth + i.vertex.x;
                //col = float4(_KinectDepthBuffer[index] * _KinectDepthScale,
                //    _KinectDepthBuffer[index] * _KinectDepthScale,
                //    _KinectDepthBuffer[index] * _KinectDepthScale,1);

                // debug : display unity depth map
                //col = float4(depth * _UnityDepthScale,
                //    depth * _UnityDepthScale,
                //    depth * _UnityDepthScale, 1);


                // operation o coordinate system for RGB map

                //i.uv.y = 1 - i.uv.y;
                //col = tex2D(_MainCameraRGBTex, i.uv);

                //i.uv.x = 1 - i.uv.x;
                //col = tex2D(_KinectRGBTex, i.uv);







                return col;
            }
        ENDCG
        }
    }
}