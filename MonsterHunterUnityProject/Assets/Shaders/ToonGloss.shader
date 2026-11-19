Shader "ArcPh1r3/ToonGloss"
{
	Properties
	{
		_Color("Color", Color) = (0.5, 0.65, 1, 1)
		_MainTex("Main Texture", 2D) = "white" {}
		_EmTex("Emission", 2D) = "black" {}
		_EmPower("Emission Power", float) = 1

		[HDR]
		_RimColor("Rim Color", Color) = (1,1,1,1)
		_RimAmount("Rim Amount", Range(0, 1)) = 0.3
		_SpecularColor("Specular Color", Color) = (0.9,0.9,0.9,1)
		_Glossiness("Glossiness", Float) = 32

		_Overlay("Overlay Color", Color) = (1, 1, 1, 1)
		_OverlayPower("Overlay Power", float) = 0

		_OutlineColor("Outline Color", Color) = (0, 0, 0, 0)
		_OutlineSize("Outline Size", float) = 0
	}
		SubShader
		{
			Tags
			{
				"LightMode" = "ForwardBase"
				"PassFlags" = "OnlyDirectional"
			}

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fwdbase

				#include "UnityCG.cginc"
				#include "Lighting.cginc"
				#include "AutoLight.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float4 uv : TEXCOORD0;
					float3 normal: NORMAL;
				};

				struct v2f
				{
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
					float3 worldNormal: NORMAL;
					float3 viewDir : TEXCOORD1;
					SHADOW_COORDS(2)
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				sampler2D _EmTex;
				float _EmPower;
				float4 _Overlay;
				float _OverlayPower;
				uniform float4 _OverlayTex_ST;
				uniform float4 _EmTex_ST;

				v2f vert(appdata v)
				{
					v2f o;

					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					o.worldNormal = UnityObjectToWorldNormal(v.normal);
					o.viewDir = WorldSpaceViewDir(v.vertex);

					TRANSFER_SHADOW(o)

					return o;
				}

				float4 _Color;
				float4 _RimColor;
				float _RimAmount;
				float _Glossiness;
				float4 _SpecularColor;

				float4 frag(v2f i) : SV_Target
				{
					float3 normal = normalize(i.worldNormal);
					float3 viewDir = normalize(i.viewDir);
					float NdotL = dot(_WorldSpaceLightPos0, normal);
					float shadow = SHADOW_ATTENUATION(i);

					// light + shadows
					float lightIntensity = smoothstep(0, 0.01, NdotL * shadow);
					float4 light = lightIntensity * _LightColor0;

					// specular stuff
					float3 halfVector = normalize(_WorldSpaceLightPos0 + viewDir);
					float NdotH = dot(normal, halfVector);

					float specularIntensity = pow(NdotH * lightIntensity, _Glossiness * _Glossiness);
					float specularIntensitySmooth = smoothstep(0.005, 0.01, specularIntensity);
					float4 specular = specularIntensitySmooth * _SpecularColor;

					// rim
					float4 rimDot = 1 - dot(viewDir, normal);
					float rimIntensity = smoothstep((1 - _RimAmount) - 0.01, (1 - _RimAmount) + 0.01, rimDot);
					float4 rim = rimIntensity * _RimColor;

					// texture + emission
					float4 sample = tex2D(_MainTex, i.uv);
					fixed4 emission = tex2D(_EmTex, i.uv * _EmTex_ST.xy + _EmTex_ST.zw);

					// apply lighting
					sample = _Color * sample * (unity_AmbientSky + light + specular + rim);

					// Apply emission lighting
					half eIntensity = max(emission.r, emission.g);
					eIntensity = max(eIntensity, emission.b);
					sample = emission * _EmPower + sample * (1 - eIntensity);

					// Apply overlay after all lighting and whatever
					sample = lerp(sample, _Overlay, _OverlayPower);

					return sample;
				}
				ENDCG
			}
			UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"

					// This pass renders the outlines
					Cull Front
					Pass
					{
						Blend SrcAlpha OneMinusSrcAlpha
						CGPROGRAM
						#pragma vertex vert
						#pragma fragment frag

						#include "UnityCG.cginc"

						struct appdata
						{
							float4 vertex : POSITION;
							float3 normal : NORMAL;
						};

						struct v2f
						{
							float4 vertex : SV_POSITION;
						};

						float _OutlineSize;
						v2f vert(appdata v)
						{
							v2f o;
							float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
							half3 worldNormal = UnityObjectToWorldNormal(v.normal);
							worldPos.xyz = worldPos.xyz + worldNormal * _OutlineSize * 0.001;
							o.vertex = mul(UNITY_MATRIX_VP, worldPos);
							return o;
						}

						float4 _OutlineColor;
						fixed4 frag(v2f i) : SV_Target
						{
							return _OutlineColor;
						}
						ENDCG
					}// End Outline Pass
		}
}