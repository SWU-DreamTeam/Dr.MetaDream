
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "LowPolyPeople/LPEDP_BodyShader"
{
	Properties
	{
		[Toggle]_HeadScalp("HeadScalp", Float) = 0
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_MaskMapRGBA("MaskMapRGBA", 2D) = "black" {}
		_MainSkinColor("MainSkinColor", Color) = (0.5514706,0.3681597,0.3203395,0)
		_SoftSkinColor("SoftSkinColor", Color) = (0.9264706,0.7425389,0.7425389,0)
		_Glow("Glow", Range( 0 , 2)) = 0.25
		_SkinSmoothness("SkinSmoothness", Range( 0 , 1)) = 0.1
		_HairSmoothness("HairSmoothness", Range( 0 , 1)) = 0.1
		_FacialHair_1stColor("FacialHair_1stColor", Color) = (0,0,0,0)
		_FacialHair_2ndColor("FacialHair_2ndColor", Color) = (0,0,0,0)
		_Hair_1stColor("Hair_1stColor", Color) = (0,0,0,0)
		_Hair_2ndColor("Hair_2ndColor", Color) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform sampler2D _MaskMapRGBA;
		uniform float4 _MaskMapRGBA_ST;
		uniform float4 _SoftSkinColor;
		uniform float4 _MainSkinColor;
		uniform float4 _FacialHair_1stColor;
		uniform float4 _FacialHair_2ndColor;
		uniform float4 _Hair_1stColor;
		uniform float4 _Hair_2ndColor;
		uniform float _Glow;
		uniform float _SkinSmoothness;
		uniform float _HairSmoothness;
		uniform float _HeadScalp;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_MaskMapRGBA = i.uv_texcoord * _MaskMapRGBA_ST.xy + _MaskMapRGBA_ST.zw;
			float4 tex2DNode57 = tex2D( _MaskMapRGBA, uv_MaskMapRGBA );
			float4 MaskTexture97 = tex2DNode57;
			float4 lerpResult83 = lerp( ( MaskTexture97 * _SoftSkinColor ) , _MainSkinColor , tex2DNode57);
			float4 weightedBlendVar107 = MaskTexture97;
			float4 weightedAvg107 = ( ( weightedBlendVar107.x*_FacialHair_1stColor + weightedBlendVar107.y*_FacialHair_2ndColor + weightedBlendVar107.z*float4( 0,0,0,0 ) + weightedBlendVar107.w*float4( 0,0,0,0 ) )/( weightedBlendVar107.x + weightedBlendVar107.y + weightedBlendVar107.z + weightedBlendVar107.w ) );
			float4 Final_facialHairColor108 = weightedAvg107;
			float BeardMask140 = saturate( ( 1.0 - ( ( distance( float3( 0.25,0.75,0.25 ) , i.vertexColor.rgb ) - 0.2 ) / max( 0.0 , 1E-05 ) ) ) );
			float4 lerpResult89 = lerp( lerpResult83 , Final_facialHairColor108 , BeardMask140);
			float4 weightedBlendVar121 = MaskTexture97;
			float4 weightedAvg121 = ( ( weightedBlendVar121.x*_Hair_1stColor + weightedBlendVar121.y*_Hair_2ndColor + weightedBlendVar121.z*_Hair_2ndColor + weightedBlendVar121.w*float4( 0,0,0,0 ) )/( weightedBlendVar121.x + weightedBlendVar121.y + weightedBlendVar121.z + weightedBlendVar121.w ) );
			float4 Final_hairColor122 = weightedAvg121;
			float HairMask137 = ( saturate( ( 1.0 - ( ( distance( float3( 1,0.25,0.25 ) , i.vertexColor.rgb ) - 0.2 ) / max( 0.0 , 1E-05 ) ) ) ) + saturate( ( 1.0 - ( ( distance( float3( 1,0,0.5 ) , i.vertexColor.rgb ) - 0.2 ) / max( 0.0 , 1E-05 ) ) ) ) );
			float4 lerpResult124 = lerp( lerpResult89 , Final_hairColor122 , HairMask137);
			o.Albedo = lerpResult124.rgb;
			o.Emission = ( lerpResult124 * _Glow ).rgb;
			float clampResult143 = clamp( ( BeardMask140 + HairMask137 ) , 0.0 , 1.0 );
			float lerpResult129 = lerp( _SkinSmoothness , _HairSmoothness , clampResult143);
			o.Smoothness = lerpResult129;
			o.Alpha = 1;
			clip( lerp(1.0,( 1.0 - saturate( ( 1.0 - ( ( distance( float3( 0,1,0.67 ) , i.vertexColor.rgb ) - 0.2 ) / max( 0.0 , 1E-05 ) ) ) ) ),_HeadScalp) - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	//CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16400
792;450;1316;815;-484.3012;1210.45;2.388393;True;True
Node;AmplifyShaderEditor.CommentaryNode;66;-2174.813,-1419.717;Float;False;2659.954;906.2883;UV1 Coloring;19;83;82;84;57;58;89;97;103;109;125;126;124;106;107;108;120;121;122;130;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;57;-2150.33,-1382.565;Float;True;Property;_MaskMapRGBA;MaskMapRGBA;2;0;Create;True;0;0;False;0;None;dbc65edb6f2debb4094b2035e62236d0;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;97;-1760.843,-1260.141;Float;False;MaskTexture;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;75;980.4275,-658.0974;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;106;-1637.845,-794.7976;Float;False;97;MaskTexture;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;104;-1675.721,-670.2837;Float;False;Property;_FacialHair_1stColor;FacialHair_1stColor;8;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;138;922.1589,355.1851;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;105;-1668.099,-482.7748;Float;False;Property;_FacialHair_2ndColor;FacialHair_2ndColor;9;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;134;1353.085,-218.2022;Float;True;Color Mask;-1;;173;eec747d987850564c95bde0e5a6d1867;0;4;1;FLOAT3;0,0,0;False;3;FLOAT3;1,0.25,0.25;False;4;FLOAT;0.2;False;5;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;135;1356.246,25.19287;Float;True;Color Mask;-1;;174;eec747d987850564c95bde0e5a6d1867;0;4;1;FLOAT3;0,0,0;False;3;FLOAT3;1,0,0.5;False;4;FLOAT;0.2;False;5;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WeightedBlendNode;107;-1305.505,-787.7512;Float;False;5;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;58;-2115.956,-1144.017;Float;False;Property;_SoftSkinColor;SoftSkinColor;4;0;Create;True;0;0;False;0;0.9264706,0.7425389,0.7425389,0;0.9264706,0.7425389,0.7425389,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;139;1325.97,359.1331;Float;True;Color Mask;-1;;175;eec747d987850564c95bde0e5a6d1867;0;4;1;FLOAT3;0.25,0.75,0.25;False;3;FLOAT3;0.25,0.75,0.25;False;4;FLOAT;0.2;False;5;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;118;-730.4326,-634.9513;Float;False;Property;_Hair_1stColor;Hair_1stColor;10;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;136;1725.9,-71.01839;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;120;-692.5566,-757.7673;Float;False;97;MaskTexture;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;119;-748.8104,-465.6423;Float;False;Property;_Hair_2ndColor;Hair_2ndColor;11;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;140;1678.817,371.9643;Float;False;BeardMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;82;-1438.799,-1229.259;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WeightedBlendNode;121;-360.2167,-752.419;Float;False;5;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;137;2039.649,-72.5985;Float;False;HairMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;84;-2116.716,-952.8248;Float;False;Property;_MainSkinColor;MainSkinColor;3;0;Create;True;0;0;False;0;0.5514706,0.3681597,0.3203395,0;0.5514706,0.3681597,0.3203395,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;108;-1080.253,-791.5867;Float;False;Final_facialHairColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;83;-1132.899,-1265.733;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;103;-732.7886,-958.938;Float;False;140;BeardMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;109;-836.5261,-1097.001;Float;False;108;Final_facialHairColor;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;130;352.2186,-668.9847;Float;False;140;BeardMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;122;-134.9642,-756.2543;Float;False;Final_hairColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;141;350.0033,-580.1284;Float;False;137;HairMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;125;-361.1407,-1077.586;Float;False;122;Final_hairColor;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;142;585.7026,-627.8916;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;126;-307.1944,-960.6239;Float;False;137;HairMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;89;-336.9273,-1251.295;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;79;1517.232,-659.1949;Float;True;Color Mask;-1;;176;eec747d987850564c95bde0e5a6d1867;0;4;1;FLOAT3;0,0,0;False;3;FLOAT3;0,1,0.67;False;4;FLOAT;0.2;False;5;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;127;562.1255,-776.0921;Float;False;Property;_HairSmoothness;HairSmoothness;7;0;Create;True;0;0;False;0;0.1;0.1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;124;82.48633,-1282.482;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;143;715.4934,-647.6198;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;86;562.043,-882.0462;Float;False;Property;_SkinSmoothness;SkinSmoothness;6;0;Create;True;0;0;False;0;0.1;0.1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;145;1807.315,-590.5247;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;81;547.4067,-995.2021;Float;False;Property;_Glow;Glow;5;0;Create;True;0;0;False;0;0.25;0.36;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;129;963.9424,-829.0096;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;85;1079.691,-984.2939;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ToggleSwitchNode;78;2090.72,-688.0931;Float;True;Property;_HeadScalp;HeadScalp;0;0;Create;True;0;0;False;0;0;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2485.086,-900.0922;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;LowPolyPeople/LPEDP_BodyShader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;97;0;57;0
WireConnection;134;1;75;0
WireConnection;135;1;75;0
WireConnection;107;0;106;0
WireConnection;107;1;104;0
WireConnection;107;2;105;0
WireConnection;139;1;138;0
WireConnection;136;0;134;0
WireConnection;136;1;135;0
WireConnection;140;0;139;0
WireConnection;82;0;97;0
WireConnection;82;1;58;0
WireConnection;121;0;120;0
WireConnection;121;1;118;0
WireConnection;121;2;119;0
WireConnection;121;3;119;0
WireConnection;137;0;136;0
WireConnection;108;0;107;0
WireConnection;83;0;82;0
WireConnection;83;1;84;0
WireConnection;83;2;57;0
WireConnection;122;0;121;0
WireConnection;142;0;130;0
WireConnection;142;1;141;0
WireConnection;89;0;83;0
WireConnection;89;1;109;0
WireConnection;89;2;103;0
WireConnection;79;1;75;0
WireConnection;124;0;89;0
WireConnection;124;1;125;0
WireConnection;124;2;126;0
WireConnection;143;0;142;0
WireConnection;145;0;79;0
WireConnection;129;0;86;0
WireConnection;129;1;127;0
WireConnection;129;2;143;0
WireConnection;85;0;124;0
WireConnection;85;1;81;0
WireConnection;78;1;145;0
WireConnection;0;0;124;0
WireConnection;0;2;85;0
WireConnection;0;4;129;0
WireConnection;0;10;78;0
ASEEND*/
//CHKSM=57F15BE98BA486BFDBBFB734837245D8F3C7E0D1