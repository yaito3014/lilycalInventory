using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

#if LIL_NDMF
using nadena.dev.ndmf;
#endif

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal partial class Modifier
    {
        internal static void ResolveMultiConditions(BuildContext ctx, AnimatorController controller, bool hasWriteDefaultsState, ItemToggler[] togglers, CostumeChanger[] costumeChangers)
        {
            var toggleBools = new Dictionary<GameObject, HashSet<string>>();
            var toggleInts = new Dictionary<GameObject, Dictionary<string, HashSet<(int,bool)>>>();
            togglers.GatherConditions(toggleBools);
            costumeChangers.GatherConditions(toggleInts);
            var multiConditionObjects = toggleBools.Select(b => b.Key).Concat(toggleInts.Select(i => i.Key)).GroupBy(o => o.name).Where(g => g.Count() > 1).Select(g => g.ElementAt(0)).ToArray();

            foreach(var t in togglers)
                t.parameter.objects = t.parameter.objects.Where(o => !multiConditionObjects.Contains(o.obj)).ToArray();
            foreach(var c in costumeChangers)
                foreach(var t in c.costumes)
                    t.parametersPerMenu.objects = t.parametersPerMenu.objects.Where(o => !multiConditionObjects.Contains(o.obj)).ToArray();

            foreach(var o in multiConditionObjects)
            {
                var bools = new string[]{};
                if(toggleBools.ContainsKey(o)) bools = toggleBools[o].ToArray();
                var ints = new (string,(int,bool)[])[]{};
                if(toggleInts.ContainsKey(o)) ints = toggleInts[o].Select(b => (b.Key,b.Value.ToArray())).ToArray();

                var toggler = new ObjectToggler
                {
                    obj = o.gameObject,
                    value = !o.gameObject.activeSelf
                };

                var clipOff = new AnimationClip();
                var clipOn = new AnimationClip();
                clipOff.name = $"{o.name}_Off";
                clipOn.name = $"{o.name}_On";

                toggler.ToClipDefault(clipOff);
                toggler.ToClip(clipOn);

                AssetDatabase.AddObjectToAsset(clipOff, ctx.AssetContainer);
                AssetDatabase.AddObjectToAsset(clipOn, ctx.AssetContainer);
                AnimationHelper.AddMultiConditionLayer(controller, hasWriteDefaultsState, clipOff, clipOn, o.name, bools, ints, o.activeSelf);
            }
        }
    }
}