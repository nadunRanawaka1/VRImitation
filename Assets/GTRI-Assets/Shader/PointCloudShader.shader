Shader "Custom/PointCloudShader"
{
    Properties
    {
        _Color ("PointCloud Color", Color) = (1,1,1,1)
        [NoScaleOffset]_DepthTex ("Depth", 2D) = "white" {}
		[NoScaleOffset]_ColorTex ("Color", 2D) = "white" {}
		_PointSize("Point Size", Float) = 4.0
		_CameraIntrinsic ("CameraIntrinsic", Vector) = (0, 0, 0, 0) // (cx, cy, fx, fy)
		_DepthScale ("DepthScale", float) = 1.0
		_Width ("Width", int) = 1
		_Height ("Height", int) = 1
		[Toggle(USE_DISTANCE)]_UseDistance ("Scale by distance?", float) = 0
    }
    SubShader
    {
		Pass
		{
			Tags { "RenderType"="Opaque" }
			LOD 200

			CGPROGRAM
			#pragma target 3.0
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			#pragma require geometry

			struct v2f
			{
			};

			struct g2f
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
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

			[maxvertexcount(4)]
			void geom(point v2f i[1], uint primID : SV_PrimitiveID, inout TriangleStream<g2f> triStream)
			{
				g2f o;
				
				int3 texCoord = int3(primID % _Width, primID / _Width, 0);
				//int3 colorCoord = texCoord;
				int3 colorCoord = int3(texCoord.x, _Height - texCoord.y, 0);

				o.color.rgb = _ColorTex.Load(colorCoord);
				o.color.a = 1.0;

				float4 v = float4(0.0, 0.0, 0.0, 1.0);
				v.z = _DepthTex.Load(texCoord).r * _DepthScale;
				v.x = (texCoord.x - _CameraIntrinsic.x) * v.z / _CameraIntrinsic.z;
				v.y = (texCoord.y - _CameraIntrinsic.y) * v.z / _CameraIntrinsic.w;
				v.y = -v.y;
				v = mul(transformationMatrix, v);

				float2 p = _PointSize * 0.001;
				p.y *= _ScreenParams.x / _ScreenParams.y;
				
				o.vertex = UnityObjectToClipPos(v);
				#ifdef USE_DISTANCE
				o.vertex += float4(-p.x, p.y, 0, 0);
				#else
				o.vertex += float4(-p.x, p.y, 0, 0) * o.vertex.w;
				#endif
				triStream.Append(o);

				o.vertex = UnityObjectToClipPos(v);
				#ifdef USE_DISTANCE
				o.vertex += float4(-p.x, -p.y, 0, 0);
				#else
				o.vertex += float4(-p.x, -p.y, 0, 0) * o.vertex.w;
				#endif
				triStream.Append(o);

				o.vertex = UnityObjectToClipPos(v);
				#ifdef USE_DISTANCE
				o.vertex += float4(p.x, p.y, 0, 0);
				#else
				o.vertex += float4(p.x, p.y, 0, 0) * o.vertex.w;
				#endif
				triStream.Append(o);

				o.vertex = UnityObjectToClipPos(v);
				#ifdef USE_DISTANCE
				o.vertex += float4(p.x, -p.y, 0, 0);
				#else
				o.vertex += float4(p.x, -p.y, 0, 0) * o.vertex.w;
				#endif
				triStream.Append(o);
			}
        
			v2f vert ()
			{
				return (v2f)0;
			}

			fixed4 frag (g2f i) : COLOR
			{
				return i.color;
			}
			ENDCG
		}
    }
}

