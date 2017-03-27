texture Texture;
sampler TextureSampler = sampler_state
{
	Texture = <Texture>;
};

// From sprite batch vertex shader
struct VertexShaderOutput
{
	float4 Position : TEXCOORD0;
	float4 Color : COLOR0;
	float2 TextureCoordinate : TEXCOORD1;
};

bool renderFog = false;

int polygonCount = 0;
float xPoints[128];
float yPoints[128];
float multiples[128];
float constants[128];

int width;
int height;

float mouseX;
float mouseY;

float orbX;
float orbY;

float fogDensity = 1; // 0 is more dense, 1 is less dense
float lightRadius = 100; // pixels
float brightness = 4; // larger is brighter
float intensity = 500; // larger is less bright

float getLineSlope(float xi, float yi, float xj, float yj) {
	return (yj - yi) / (xj - xi);
}

float getLineConstant(float xi, float yi, float xj, float yj) {
	float slope = getLineSlope(xi, yi, xj, yj);
	if (isnan(slope)) {
		return slope;
	}

	return yi - (xi * slope);
}

bool isPointOnLineSegment(float xi, float yi, float xj, float yj, float xTest, float yTest) {
	float slope = getLineSlope(xi, yi, xj, yj);
	float intercept = getLineConstant(xi, yi, xj, yj);
	float xMax = max(xi, xj);
	float xMin = min(xi, xj);
	float yMax = max(yi, yj);
	float yMin = min(yi, yj);

	if (xTest < xMin || xTest > xMax || yTest < yMin || yTest > yMax) {
		return false;
	}

	return isnan(slope) || slope * xTest + intercept == yTest;
}

bool xor(bool left, bool right) {
	return (left || right) && !(left && right);
}

/**
* Returns true if the point lies within the geometry of the area.
* Returns false for edges.
* Algorithm courtesy of Patrick Mullen.
*/
bool isValidPoint(float2 source) {
	int j = polygonCount - 1;
	bool isValid = false;
	bool isOnLineSegment = false;

	for (int i = 0; i < polygonCount; i++) {
		if ((yPoints[i] < source.y && yPoints[j] >= source.y
			|| yPoints[j] < source.y && yPoints[i] >= source.y)) {
			isValid = xor(isValid, source.y * multiples[i] + constants[i] < source.x);
		}

		isOnLineSegment = isOnLineSegment || isPointOnLineSegment(
			xPoints[i], yPoints[i], xPoints[j], yPoints[j], source.x, source.y);
		j = i;
	}

	return isValid || isOnLineSegment;
}

float get_dist(float x, float y, float dst_x, float dst_y) {
	// There's probably a nice linear algebra way to do this.
	float x_dist = abs(x - dst_x);
	float y_dist = abs(y - dst_y);
	float sum_dist = pow(x_dist, 2) + pow(y_dist, 2);
	return sqrt(sum_dist);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(TextureSampler, input.TextureCoordinate);

	float yPos = (height - (height * input.Position.y)) / 2;
	float xPos = (width * input.Position.x + width) / 2;
	float mouseDistance = get_dist(xPos, yPos, mouseX, mouseY);
	//float orbDistance = get_dist(xPos, yPos, orbX, orbY);
	//float dist = min(mouseDistance, orbDistance);
	float dist = mouseDistance;

	float normalizedDist = (lightRadius - dist) / intensity;
	normalizedDist = normalizedDist * brightness;

	if (!isValidPoint(float2(xPos, yPos))) {
		normalizedDist = 0;
	} else if (!renderFog) {
		normalizedDist = 1;
	}

	color.rgb = color.rgb * normalizedDist;
	return color;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_4_0 PixelShaderFunction();
	}
}