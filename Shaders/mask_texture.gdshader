shader_type canvas_item;

uniform sampler2D mask;

void fragment()
{
	vec4 mask_color = texture(mask, UV).rgba;
	vec4 sprite_color = texture(TEXTURE, UV).rgba;
	
	if (mask_color.a == 0.0)
	{
		sprite_color.a = 0.0;
	}
	COLOR = sprite_color;
}