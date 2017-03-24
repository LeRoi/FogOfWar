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

float orb_x;
float orb_y;

float fog_density = 1; // 0 is more dense, 1 is less dense
float light_radius = 100; // pixels
float brightness = 4; // larger is brighter
float intensity = 500; // larger is less bright

float get_dist(float x, float y, float dst_x, float dst_y) {
	// Refactor once I revisit linear algebra
	float x_dist = abs(x - dst_x);
	float y_dist = abs(y - dst_y);
	float sum_dist = pow(x_dist, 2) + pow(y_dist, 2);
	return sqrt(sum_dist);
}

// Our pixel shader
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(TextureSampler, input.TextureCoordinate);

	float y_pos = (height - (height * input.Position.y)) / 2;
	float x_pos = (width * input.Position.x + width) / 2;
	float mouse_distance = get_dist(x_pos, y_pos, mouse_x, mouse_y);
	float orb_distance = get_dist(x_pos, y_pos, orb_x, orb_y);
	float dist = min(mouse_distance, orb_distance);
	float normalized_dist = (light_radius - dist) / intensity;
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