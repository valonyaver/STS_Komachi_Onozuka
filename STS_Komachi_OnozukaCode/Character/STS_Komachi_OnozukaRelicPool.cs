using BaseLib.Abstracts;
using Godot;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Character
{
    public class STS_Komachi_OnozukaRelicPool : CustomRelicPoolModel
    {
        public override Color LabOutlineColor => Komachi_Character.Color;

        public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
        public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
    }
}