Shader "ProjectFulcrum/MinimapLayerCombiner"
{
    Properties {
 
        _MainTex ("Base", 2D) = "white" {}
 
        _BlendTex ("Overlay", 2D) = "white" {}
 
    }
 
    SubShader {
        Tags { "Queue" = "Transparent" }
        Pass 
        {
            Blend SrcAlpha OneMinusSrcAlpha
 
            // Apply base texture
 
            SetTexture [_MainTex] 
            {
                combine texture
            }
 
            // Blend in the alpha texture using the lerp operator
 
            SetTexture [_BlendTex] 
            {
                combine texture lerp (texture) previous
            }
 
        }
 
    }
 
}