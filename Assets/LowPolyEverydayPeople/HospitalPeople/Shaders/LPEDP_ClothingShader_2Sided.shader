// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "LowPolyPeople/LPEDP_ClothingShader2Sided"
{
	Properties
	{
		_MaskMapRGBA("MaskMapRGBA", 2D) = "black" {}
		_BaseColor("BaseColor", Color) = (0,0,0,0)
		_Color1("Color1", Color) = (0,0,0,0)
		_Color2("Color2", Color) = (0,0,0,0)
		_Color3("Color3", Color) = (0,0,0,0)
		_EmissivePower("EmissivePower", Range( 0 , 1)) = 0
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
		_Metalness("Metalness", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _BaseColor;
		uniform float4 _Color1;
		uniform sampler2D _MaskMapRGBA;
		uniform float4 _MaskMapRGBA_ST;
		uniform float4 _Color2;
		uniform float4 _Color3;
		uniform float _EmissivePower;
		uniform float _Metalness;
		uniform float _Smoothness;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_MaskMapRGBA = i.uv_texcoord * _MaskMapRGBA_ST.xy + _MaskMapRGBA_ST.zw;
			float4 tex2DNode57 = tex2D( _MaskMapRGBA, uv_MaskMapRGBA );
			float4 lerpResult162 = lerp( _BaseColor , _Color1 , tex2DNode57.r);
			float4 lerpResult165 = lerp( lerpResult162 , _Color2 , tex2DNode57.g);
			float4 lerpResult174 = lerp( lerpResult165 , _Color3 , tex2DNode57.b);
			o.Albedo = lerpResult174.rgb;
			o.Emission = ( lerpResult174 * _EmissivePower ).rgb;
			o.Metallic = _Metalness;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16400
1920;77;1920;951;4822.74;1432.627;2.93528;True;True
Node;AmplifyShaderEditor.ColorNode;163;-2951.702,-1754.6;Float;False;Property;_BaseColor;BaseColor;1;0;Create;True;0;0;False;0;0,0,0,0;1,0.2647059,0.2647059,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;57;-3193.201,-1336.573;Float;True;Property;_MaskMapRGBA;MaskMapRGBA;0;0;Create;True;0;0;False;0;None;dbc65edb6f2debb4094b2035e62236d0;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;164;-2939.726,-1542.733;Float;False;Property;_Color1;Color1;2;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;162;-2608.745,-1569.521;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;167;-2730.949,-1342.282;Float;False;Property;_Color2;Color2;3;0;Create;True;0;0;False;0;0,0,0,0;1,0,0.7655172,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;165;-2350.386,-1331.573;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;168;-2558.928,-1097.325;Float;False;Property;_Color3;Color3;4;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;171;-2290.147,-897.7782;Float;False;Property;_EmissivePower;EmissivePower;5;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;174;-2024.74,-1275.722;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;170;-1845.147,-1100.778;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;173;-1902.147,-832.7782;Float;False;Property;_Smoothness;Smoothness;6;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;172;-1930.147,-981.7782;Float;False;Property;_Metalness;Metalness;7;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-1107.272,-968.8933;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;LowPolyPeople/LPEDP_ClothingShader2Sided;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;162;0;163;0
WireConnection;162;1;164;0
WireConnection;162;2;57;1
WireConnection;165;0;162;0
WireConnection;165;1;167;0
WireConnection;165;2;57;2
WireConnection;174;0;165;0
WireConnection;174;1;168;0
WireConnection;174;2;57;3
WireConnection;170;0;174;0
WireConnection;170;1;171;0
WireConnection;0;0;174;0
WireConnection;0;2;170;0
WireConnection;0;3;172;0
WireConnection;0;4;173;0
ASEEND*/
//CHKSM=395BB65EFD69D93E63491D47902332FDECAF36A0