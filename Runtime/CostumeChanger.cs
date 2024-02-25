using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.avatarmodifier.runtime
{
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(CostumeChanger))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + nameof(CostumeChanger))]
    internal class CostumeChanger : MenuBaseComponent
    {
        [NotKeyable] public Costume[] costumes;
        [NotKeyable] [LILLocalize] public bool isSave = true;
        [NotKeyable] [LILLocalize] public bool isLocalOnly = false;
    }
}
