using Godot;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku.Nodes
{


    public partial class NTimestopLaser : NDanmakuLaser
    {
        ShaderMaterial _mat;

        protected override void OnBeamReady()
        {
            _mat = (ShaderMaterial)Beam.Material;
        }

        protected override void OnWidthProgressChanged(float t)
        {
            _mat.SetShaderParameter("beam_half_width", t);
        }
    }
}
