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

// left, top, right, bottom
//  x	  y		z	   a
float4 walls[11];
int wall_count = 0;

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

bool is_within(float2 src, float4 wall) {
	return (wall.x < src.x && src.x < wall.z && wall.y < src.y && src.y < wall.a);
}

// Returns the (slope, intersection) of a line defined by two points.
// Returns (NULL, i) if the line is parallel to the y axis, where i is x = i.
float2 line_equation(float2 src, float2 dst) {
	if (src.x - dst.x == 0) return float2(-30.01, src.x);
	float slope = (src.y - dst.y) / (src.x - dst.x);
	float intersection = src.y - src.x * slope;
	return float2(slope, intersection);
}

// Given two lines (s0, i0), (s1, i1), return the (x, y) point of intersection.
// If the lines are parallel, or the same, return NULL.
float2 intersection(float2 src, float2 dst) {
	if (src.x == dst.x) return float2(-30.01, -30.01);
	float x = (dst.y - src.y) / (src.x - dst.x);
	float y = ((src.x * dst.y) - (dst.x * src.y)) / (src.x - dst.x);
	return float2(x, y);
}

// Return the y-coordinate where the line (slope, intersection) intersects this x coordinate.
// Returns NULL if the equation never or always intersects x.
float x_intersect(float2 eqn, float x) {
	if (eqn.x == -30.01) return -30.01;
	return x * eqn.x + eqn.y;
}

// Return the x-coordinate where the line (slope, intersection) intersects this y coordinate.
// Returns NULL if the equation never or always intersects y.
float y_intersect(float2 eqn, float y) {
	if (eqn.x == 0) return -30.01;
	if (eqn.x == -30.01) return eqn.y;
	return (y - eqn.y) / eqn.x;
}

bool is_between(float one, float two, float test) {
	if (test == -30.01) return false;
	float minval = min(one, two);
	float maxval = max(one, two);
	return minval <= test && test <= maxval;
}

bool line_intersects(float2 src, float2 dst, float4 wall) {
	float2 top_left = float2(wall.x, wall.y);
	float2 top_right = float2(wall.z, wall.y);
	float2 bottom_left = float2(wall.x, wall.a);
	float2 bottom_right = float2(wall.z, wall.a);

	if (is_within(src, wall) || is_within(dst, wall)) return true;
	float2 src_line = line_equation(src, dst);

	float left_x = x_intersect(src_line, wall.x);
	float right_x = x_intersect(src_line, wall.z);
	float top_y = y_intersect(src_line, wall.y);
	float bottom_y = y_intersect(src_line, wall.a);

	if (is_between(src.x, dst.x, left_x) ||
		is_between(src.x, dst.x, right_x) ||
		is_between(src.y, dst.y, top_y) ||
		is_between(src.y, dst.y, bottom_y)) {
		return true;
	}

	return false;
}



//bool point_in_polygon

// Our pixel shader
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(TextureSampler, input.TextureCoordinate);

	float y_pos = (height - (height * input.Position.y)) / 2;
	float x_pos = (width * input.Position.x + width) / 2;
	float mouse_distance = get_dist(x_pos, y_pos, mouse_x, mouse_y);
	float orb_distance = get_dist(x_pos, y_pos, orb_x, orb_y);
	float dist = min(mouse_distance, orb_distance);

	/*[loop]
	for (int i = 0; i < 6; i++) {
		if (line_intersects(float2(x_pos, y_pos), float2(mouse_x, mouse_y), walls[i])) {
			dist = light_radius;
		}
	}*/
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