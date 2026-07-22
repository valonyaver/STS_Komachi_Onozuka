shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_linear;
uniform float beam_half_width : hint_range(0.0, 1.0) = 0.0;
uniform float edge_softness : hint_range(0.0, 0.5) = 0.05;

uniform float border_thickness : hint_range(0.0, 0.2) = 0.02;
uniform vec4 border_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);

uniform float streak_speed : hint_range(0.0, 20.0) = 4.0;
uniform float streak_width : hint_range(0.0, 0.2) = 0.03;
uniform float streak_band : hint_range(0.0, 0.5) = 0.1;
uniform vec4 streak_color : source_color = vec4(1.0, 1.0, 1.0, 0.8);

const int STREAK_COUNT = 3;

float hash11(float p) {
    p = fract(p * 0.1031);
    p *= p + 33.33;
    p *= p + p;
    return fract(p);
}

void fragment() {
    float dist_from_center = abs(UV.y - 0.5) * 2.0;
    float mask = 1.0 - smoothstep(beam_half_width - edge_softness, beam_half_width, dist_from_center);
    if (mask <= 0.001) {
        discard;
    }

    vec4 screen_col = texture(screen_texture, SCREEN_UV);
    float gray = dot(screen_col.rgb, vec3(0.299, 0.587, 0.114));
    vec3 desaturated = vec3(gray);

    float streak_total = 0.0;
    for (int i = 0; i < STREAK_COUNT; i++) {
        float seed = float(i) * 17.0 + 3.0;
        float speed_var = 0.7 + hash11(seed) * 0.6;
        float phase = hash11(seed + 1.0);
        float side = step(0.5, hash11(seed + 2.0));
        float band_center = mix(0.15, 0.85, side);
        band_center += (hash11(seed + 3.0) - 0.5) * 0.15;

        float x_pos = fract(TIME * streak_speed * speed_var * 0.2 + phase);
        float dx = abs(UV.x - x_pos);
        dx = min(dx, 1.0 - dx);
        float line_x = 1.0 - smoothstep(0.0, streak_width, dx);

        float dy = abs(UV.y - band_center);
        float line_y = 1.0 - smoothstep(0.0, streak_band, dy);

        streak_total += line_x * line_y;
    }
    streak_total = clamp(streak_total, 0.0, 1.0);

    vec3 with_streaks = mix(desaturated, streak_color.rgb, streak_total * streak_color.a);

    // Border: a thin band right at the beam's outer edge
    float border_mask = 1.0 - smoothstep(0.0, border_thickness, abs(dist_from_center - beam_half_width));
    vec3 final_color = mix(with_streaks, border_color.rgb, border_mask * border_color.a);

    COLOR = vec4(final_color, mask);
}