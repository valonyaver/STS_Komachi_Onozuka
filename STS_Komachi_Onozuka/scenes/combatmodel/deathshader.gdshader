shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_linear;
uniform vec2 center = vec2(0.5, 0.5); // (0.5, 0.5) is the exact center of the rect
uniform float progress : hint_range(0.0, 3.0) = 0.0;
uniform float num_waves : hint_range(1.0, 20.0) = 1.0;
uniform float wave_spacing : hint_range(0.01, 2.0) = 0.2;
uniform float ring_thickness : hint_range(0.01, 1.0) = 0.15;
uniform float edge_softness : hint_range(0.001, 0.2) = 0.02;
uniform float max_radius : hint_range(0.1, 2.0) = 1.4;

const int MAX_RING_ITER = 20;

void fragment() {
    // Screen aspect ratio correction so rings stay perfectly circular
    float aspect = SCREEN_PIXEL_SIZE.y / SCREEN_PIXEL_SIZE.x;
    
    // Distance from the rect's center using UV
    vec2 diff = UV - center;
    diff.x *= aspect;
    float dist = length(diff) * 2.0;

    vec4 bg = texture(screen_texture, SCREEN_UV);
    int ring_count = int(min(num_waves, float(MAX_RING_ITER)));
    float invert_amount = 0.0;

    for (int i = 0; i < MAX_RING_ITER; i++) {
        if (i >= ring_count) break;
        float ring_progress = progress - float(i) * wave_spacing;
        if (ring_progress < 0.0) continue;
        if (ring_progress - ring_thickness > max_radius) continue;

        float outer = min(ring_progress, max_radius);
        float inner = max(0.0, ring_progress - ring_thickness);
        float outer_mask = 1.0 - smoothstep(outer - edge_softness, outer + edge_softness, dist);
        float inner_mask = smoothstep(inner - edge_softness, inner + edge_softness, dist);
        invert_amount = max(invert_amount, outer_mask * inner_mask);
    }

    vec3 inverted = vec3(1.0) - bg.rgb;
    COLOR = vec4(mix(bg.rgb, inverted, invert_amount), bg.a);
}