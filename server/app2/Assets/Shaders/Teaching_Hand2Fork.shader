Shader "Transparent/Diffuse ZWrite" {
    Properties{
        _Color("Main Color", Color) = (1,1,1,1)
        _MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
    }
        SubShader{
            Tags {"Queue" = "2499"}// "IgnoreProjector" = "True" "RenderType" = "Transparent" }
            LOD 200

        //// extra pass that renders to depth buffer only
        //Pass {
        //    ZWrite On
        //    Tags {"LightMode" = "ShadowCaster"}

        //    CGPROGRAM
        //    #pragma vertex vert
        //    #pragma fragment frag
        //    #pragma multi_compile_shadowcaster
        //    #include "UnityCG.cginc"

        //    struct v2f {
        //        V2F_SHADOW_CASTER;
        //    };

        //    v2f vert(appdata_base v)
        //    {
        //        v2f o;
        //        TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
        //        return o;
        //    }

        //    float4 frag(v2f i) : SV_Target
        //    {
        //        SHADOW_CASTER_FRAGMENT(i)
        //    }
        //    ENDCG
        //}

        //// paste in forward rendering passes from Transparent/Diffuse
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
        //Fallback "Transparent/VertexLit"
}