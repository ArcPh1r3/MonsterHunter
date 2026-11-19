using MonsterHunter.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MonsterHunter.Content.MonsterParts
{
    [RegisterPart(nameof(ImpEye))]
    internal class ImpEye : BaseMonsterPart
    {
        public override string xmlKey => "rob_monHun_impEye";
        public override Color32 partColor => new Color32(242, 4, 67, 255);
        public override string partName => "Imp Eyes";
    }
}
