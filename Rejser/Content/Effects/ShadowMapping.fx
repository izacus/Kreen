float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 LightViewProjection;

float3 LightDirection;
float4 AmbientColor = float4(0.03, 0.03, 0.03, 0);
float DepthBias = 0.5f;

texture Texture;
sampler TextureSampler = sampler_state
{
	Texture = (Texture);
	
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	
	AddressU = Wrap;
	AddressV = Wrap;
};

texture ShadowMap;
sampler ShadowMapSampler = sampler_state
{
	Texture = <ShadowMap>;
	MinFilter = POINT;
	MagFilter = POINT;
	MipFilter = NONE;
	AddressU = Clamp;
	AddressV = Clamp;
};

// TECHNIQUE 1: CREATING SHADOW MAP
// =======================================================================
struct CreateShadowMap_VSOut
{
	float4 Position : POSITION;
	float Depth : TEXCOORD0;
};

CreateShadowMap_VSOut CreateShadowMap_VS(float4 Position : POSITION)
{
	CreateShadowMap_VSOut output;
	
	output.Position = mul(Position, mul(World, LightViewProjection));
	output.Depth = output.Position.z / output.Position.w;
	
	return output;
}

float4 CreateShadowMap_PS(CreateShadowMap_VSOut input) : COLOR
{
	return float4(-input.Depth, 0, 0, 0);
}

technique CreateShadowMap
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 CreateShadowMap_VS();
		PixelShader = compile ps_2_0 CreateShadowMap_PS();
	}
}

// TECHNIQUE 2: DRAWING OBJECT WITH SHADOW
// =========================================================================
struct DrawWithShadowMap_VSIn
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
};

struct DrawWithShadowMap_VSOut
{
	float4 Position : POSITION0;
	float3 Normal : TEXCOORD0;
	float2 TexCoord : TEXCOORD1;
	float4 WorldPos : TEXCOORD2;
};

DrawWithShadowMap_VSOut DrawWithShadowMap_VS(DrawWithShadowMap_VSIn input)
{
	DrawWithShadowMap_VSOut output;
	
	float4x4 WorldViewProj = mul(mul(World, View), Projection);
	
	// Transform model vertices and normal
	output.Position = mul(input.Position, WorldViewProj);
	output.Normal = normalize(mul(input.Normal, World));
	output.TexCoord = input.TexCoord;
	
	// Save vertex position in world space
	output.WorldPos = mul(input.Position, World);
	
	return output;
}

float4 DrawWithShadowMap_PS(DrawWithShadowMap_VSOut input) : COLOR
{
	// Check color of the model
	float4 diffuseColor = tex2D(TextureSampler, input.TexCoord);
	//float4 diffuseColor = float4(0.5, 0.5, 0.5, 0);
	
	// Intensity based on direction of the light
	float diffuseIntensity = saturate(dot(LightDirection, input.Normal));
	// Final diffuse color with added ambient color
	float4 diffuse = diffuseIntensity * diffuseColor + AmbientColor;
	
	// Find position of the pixel in the light's space
	float4 lightningPosition = mul(input.WorldPos, LightViewProjection);
	
	// Find the shadow map position of the pixel
	float2 shadowTextureCoordinate = 0.5 * lightningPosition.xy /
										   lightningPosition.w + float2(0.5, 0.5);
	shadowTextureCoordinate.y = 1.0f - shadowTextureCoordinate.y;
	
	float shadowDepth = tex2D(ShadowMapSampler, shadowTextureCoordinate).r;
	
	// Calculate current pixel depth
	float ourDepth = (lightningPosition.z / lightningPosition.w) - DepthBias;
	
	// Check if the pixel is in in front or behind the shadow map
	if (shadowDepth < ourDepth)
	{
		diffuse *= float4(0.2, 0.2, 0.2, 0);
	};
	
	return diffuse;
}

technique DrawWithShadowMap
{
	pass Pass0
	{
		VertexShader = compile vs_2_0 DrawWithShadowMap_VS();
		PixelShader = compile ps_2_0 DrawWithShadowMap_PS();
	}
}

float4 DrawWithoutShadowMap_PS(DrawWithShadowMap_VSOut input) : COLOR
{
	// Check color of the model
	float4 diffuseColor = tex2D(TextureSampler, input.TexCoord);
	
	// Intensity based on direction of the light
	float diffuseIntensity = saturate(dot(LightDirection, input.Normal));
	// Final diffuse color with added ambient color
	float4 diffuse = diffuseIntensity * diffuseColor + AmbientColor;
	
	return diffuse;
}

technique DrawWithoutShadowMap
{
	pass Pass0
	{
		VertexShader = compile vs_2_0 DrawWithShadowMap_VS();
		PixelShader = compile ps_2_0 DrawWithoutShadowMap_PS();
	}
}