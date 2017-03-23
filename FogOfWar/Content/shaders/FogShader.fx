// Texture sampler
// Based from http://joshuasmyth.maglevstudios.com/post/XNA-and-Monogame-Introduction-to-Pixel-Shaders1
texture Texture;
sampler TextureSampler = sampler_state
{
	Texture = <Texture>;
};

// This data comes from the sprite batch vertex shader
struct VertexShaderOutput
{
	float4 Position : TEXCOORD0;
	float4 Color : COLOR0;
	float2 TextureCoordinate : TEXCOORD1;
};

int width;
int height;

float mouse_x;
float mouse_y;

// Our pixel shader
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(TextureSampler, input.TextureCoordinate);

	float y_pos = (height - (height * input.Position.y)) / 2;
	float x_pos = (width * input.Position.x + width) / 2;
	float normalized_y = abs(y_pos - mouse_y) / height;
	float normalized_x = abs(x_pos - mouse_x) / width;
	float saturation = 1 - max(normalized_y, normalized_x);
	return color * saturation;
}

// Compile our shader
technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();
	}
}