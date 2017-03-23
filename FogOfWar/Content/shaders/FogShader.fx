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

float fog_density = 1; // 0 is more dense, 1 is less dense
float light_radius = 100; // pixels
float brightness = 4; // larger is brighter

// Our pixel shader
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(TextureSampler, input.TextureCoordinate);

	float y_pos = (height - (height * input.Position.y)) / 2;
	float x_pos = (width * input.Position.x + width) / 2;

	// I should go study linear algebra again
	float x_dist = abs(x_pos - mouse_x);
	float y_dist = abs(y_pos - mouse_y);
	float sum_dist = pow(x_dist, 2) + pow(y_dist, 2);
	float dist = sqrt(sum_dist);
	float normalized_dist = (light_radius - dist) / height;
	normalized_dist = normalized_dist * brightness;

	color.rgb = color.rgb * normalized_dist;
	return color;
}

// Compile our shader
technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();
	}
}