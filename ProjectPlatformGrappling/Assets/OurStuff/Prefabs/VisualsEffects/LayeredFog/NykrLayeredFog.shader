Shader "NykrShaders/NykrLayeredFog"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "black" {}
		_GradientTexture("Texture", 2D) = "white" {}
	}

	CGINCLUDE

		#include "UnityCG.cginc"

		uniform sampler2D _MainTex;
		uniform sampler2D _GradientTexture;
		uniform sampler2D_float _CameraDepthTexture;

		// x = fog height
		// y = FdotC (CameraY-FogHeight)
		// z = k (FdotC > 0.0)
		// w = a/2
		uniform float4 _HeightParams;
		uniform float4 _DistanceParams; //x = start distance

		int4 _SceneFogMode; // x= fog mode, y = radial bool
		float4 _SceneFogParams;
#ifndef UNITY_APPLY_FOG
		half4 unity_fogColor;	//Kanske är denna som skall ändras för att få mitt coola resultat
		//half4 unity_fogDensity; 
#endif
		uniform float4 _MainTex_TexelSize;
		uniform float4x4 _FrustumCornersWS;
		uniform float4 _CameraWS;

		struct v2f 
		{
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
			float2 uv_depth : TEXCOORD1;
			float4 interpolatedRay : TEXCOORD2;
		};

		v2f vert(appdata_img v)
		{
			v2f output;
			half index = v.vertex.z;
			v.vertex.z = 0.1;
			output.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			output.uv = v.texcoord.xy;
			output.uv_depth = v.texcoord.xy;

#if UNITY_UV_STARTS_AT_TOP
			if (_MainTex_TexelSize.y < 0)
			{
				output.uv.y = 1 - output.uv.y;
			}
#endif
			output.interpolatedRay = _FrustumCornersWS[(int)index];
			output.interpolatedRay.w = index;
			return output;
		}

		//Applies one of standard fog formulas, given fog coordinate (i.e. distance)
		half ComputeFogFactor(float p_coord)
		{
			float fogFac = 0.0f;
			if (_SceneFogMode.x == 1)//Linead
			{
				// factor = (end-z)/(end-start) = z * (-1/(end-start)) + (end/(end-start))
				fogFac = p_coord * _SceneFogParams.z + _SceneFogParams.w;
			}
			if (_SceneFogMode.x == 2)// exponential
			{
				// factor = exp(-density*z)
				fogFac = _SceneFogParams.y * p_coord;
				fogFac = exp2(-fogFac);
			}
			if (_SceneFogMode.x == 3) // exponential2
			{
				// factor = exp(-(density*z)^2)
				fogFac = _SceneFogParams.x * p_coord;
				fogFac = exp2(-fogFac*fogFac);
			}
			return saturate(fogFac);
		}

		float4 CalculateColor(float p_depth)
		{
			float colorBit = 1.0f / _SceneFogMode.z;
			float depthPercentage = p_depth / _SceneFogMode.w;
			float2 coord = float2(saturate(depthPercentage), 0.5f);
			float4 color = tex2D(_GradientTexture, coord);
			color.w = 1.0f;
			return color;
		}

		//Distance based fog
		float ComputeDistance(float3 p_camdir, float p_zdepth)
		{
			float distance;
			if (_SceneFogMode.y == 1)
			{
				distance = length(p_camdir);
			}
			else
			{
				distance = p_zdepth * _ProjectionParams.z;
			}
			// Built-in fog starts at near plane, so match that by
			// subtracting the near value. Not a perfect approximation
			// if near plane is very large, but good enough.
			distance -= _ProjectionParams.y;
			return distance;
		}
		// Linear half-space fog, from https://www.terathon.com/lengyel/Lengyel-UnifiedFog.pdf
		float ComputeHalfSpace(float3 p_wsDir)
		{
			float3 wPos = _CameraWS + p_wsDir;
			float FH = _HeightParams.x;
			float3 C = _CameraWS;
			float3 V = p_wsDir;
			float3 P = wPos;
			float3 aV = _HeightParams.w * V;
			float FdotC = _HeightParams.y;
			float k = _HeightParams.z;
			float FdotP = P.y - FH;
			float FdotV = p_wsDir.y;
			float c1 = k * (FdotP + FdotC);
			float c2 = (1 - 2 * k)*FdotP;
			float g = min(c2, 0.0);
			g = -length(aV) * (c1 - g * g / abs(FdotV + 1.0e-5f));
			return g;
		}
		half4 ComputeFog(v2f i, bool distance, bool height) : SV_TARGET
		{
			half4 sceneColor = tex2D(_MainTex, i.uv);

			//Reconstruct worldspace position and direction towards this screen pixel
			float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv_depth);
			float dpth = Linear01Depth(rawDepth);
			float4 wsDir = dpth * i.interpolatedRay;
			float4 wsPos = _CameraWS + wsDir;

			// Compute fog distance
			float g = _DistanceParams.x;
			if (distance)
			{
				g += ComputeDistance(wsDir, dpth);
			}
			if (height)
			{
				g += ComputeHalfSpace(wsDir);
			}

			// Compute fog amount
			half fogFac = ComputeFogFactor(max(0.0, g));
			// Do not fog skybox
			if (rawDepth == _DistanceParams.y)
			{
				fogFac = 1.0f;
			}

			float4 color = CalculateColor(g);
			return lerp(color, sceneColor, fogFac);
		}
		

ENDCG

	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always Fog { Mode Off }


			// 0: distance + height
			Pass
		{
			CGPROGRAM
#pragma vertex vert
#pragma fragment frag
			half4 frag(v2f i) : SV_Target{ return ComputeFog(i, true, true); }
			ENDCG
		}
			// 1: distance
			Pass
		{
			CGPROGRAM
#pragma vertex vert
#pragma fragment frag
			half4 frag(v2f i) : SV_Target{ return ComputeFog(i, true, false); }
			ENDCG
		}
			// 2: height
			Pass
		{
			CGPROGRAM
#pragma vertex vert
#pragma fragment frag
			half4 frag(v2f i) : SV_Target{ return ComputeFog(i, false, true); }
			ENDCG
		}
	}
		Fallback off
}
