Shader "Custom/PointCloudQuest"
{
    Properties
    {
        _Color ("PointCloud Color", Color) = (1,1,1,1)
        [NoScaleOffset]_DepthTex ("Depth", 2D) = "white" {}
		[NoScaleOffset]_ColorTex ("Color", 2D) = "white" {}
		_PointSize("Point Size", Float) = 10.0
		_CameraIntrinsic ("CameraIntrinsic", Vector) = (0, 0, 0, 0) // (cx, cy, fx, fy)
		_DepthScale ("DepthScale", float) = 1.0
		_Width ("Width", int) = 1
		_Height ("Height", int) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

			struct appdata
			{
				uint vertID : SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

            struct v2f
            {
                float4 vertex : POSITION;
				float4 color : COLOR;
				float size : PSIZE;
				UNITY_VERTEX_OUTPUT_STEREO
            };

            float _DepthScale;
			float _PointSize;
			Texture2D _DepthTex;
			Texture2D _ColorTex;
			float4 _CameraIntrinsic;
			int _Width;
			int _Height;
			float4x4 transformationMatrix;
			fixed4 _Color;

            v2f vert (appdata i)
            {
                v2f o;
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				int3 texCoord = int3(i.vertID % _Width, i.vertID / _Width, 0);
				int3 colorCoord = int3(texCoord.x, _Height - texCoord.y, 0);

				o.color.rgb = _ColorTex.Load(colorCoord);
				o.color.a = 1.0;

				float4 v = float4(0.0, 0.0, 0.0, 1.0);
				v.z = _DepthTex.Load(texCoord).r * _DepthScale;
				v.x = (texCoord.x - _CameraIntrinsic.x) * v.z / _CameraIntrinsic.z;
				v.y = -(texCoord.y - _CameraIntrinsic.y) * v.z / _CameraIntrinsic.w;
				//v.y = -v.y;
				v = mul(transformationMatrix, v);

                o.vertex = UnityObjectToClipPos(v);
				o.size = _PointSize;
                return o;
            }

            fixed4 frag (v2f i) : COLOR
            {
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                return i.color;
            }
            ENDCG
        }
    }
}

