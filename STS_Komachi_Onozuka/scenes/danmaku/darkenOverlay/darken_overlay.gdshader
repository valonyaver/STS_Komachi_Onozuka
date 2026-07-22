shader_type canvas_item;

uniform vec2 origin_screen_pos;
uniform float front_radius : hint_range(0.0, 4000.0) = 0.0;
uniform float front_softness : hint_range(0.0, 300.0) = 120.0;
uniform float intensity : hint_range(0.0, 1.0) = 1.0;
uniform vec4 darken_color : source_color = vec4(0.0, 0.0, 0.0, 0.5);

void fragment() {
    vec2 frag = FRAGCOORD.xy;
    float dist = distance(frag, origin_screen_pos);
    float expand_mask = 1.0 - smoothstep(front_radius, front_radius + front_softness, dist);

    COLOR = vec4(darken_color.rgb, darken_color.a * expand_mask * intensity);
}