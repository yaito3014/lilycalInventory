using System.Collections.Generic;
using UnityEngine;

#if LIL_NDMF
using nadena.dev.ndmf;
#endif

namespace jp.lilxyzw.avatarmodifier
{
    using System.Linq;
    using runtime;

    internal static class Processor
    {
        // Common
        private static bool shouldModify;
        // Material
        private static MaterialModifier[] modifiers;
        private static MaterialOptimizer[] optimizers;
        // Menu
        private static MenuFolder[] folders;
        private static AutoDresser[] dressers;
        private static ItemToggler[] togglers;
        private static Prop[] props;
        private static CostumeChanger[] costumeChangers;
        private static SmoothChanger[] smoothChangers;
        private static Material[] materials;

        internal static void FindComponent(BuildContext ctx)
        {
            // Resolve Dresser
            dressers = ctx.AvatarRootObject.GetComponentsInChildren<AutoDresser>(true).Where(c => c.enabled).ToArray();
            dressers.ResolveMenuName();
            dressers.DresserToChanger();

            var components = ctx.AvatarRootObject.GetComponentsInChildren<AvatarTagComponent>(true).Where(c => c.enabled).ToArray();
            modifiers = components.GetActiveComponents<MaterialModifier>();
            optimizers = components.GetActiveComponents<MaterialOptimizer>();
            folders = components.GetActiveComponents<MenuFolder>();
            togglers = components.GetActiveComponents<ItemToggler>();
            props = components.GetActiveComponents<Prop>();
            costumeChangers = components.GetActiveComponents<CostumeChanger>();
            smoothChangers = components.GetActiveComponents<SmoothChanger>();
            shouldModify = components.Length != 0;
            if(!shouldModify) return;
            components.GetActiveComponents<MenuBaseComponent>().ResolveMenuName();
            components.GetActiveComponents<CostumeChanger>().ResolveMenuName();
            ObjHelper.CheckApplyToAll(togglers, costumeChangers, smoothChangers);
        }

        internal static void Clone(BuildContext ctx)
        {
            if(!shouldModify) return;
            materials = Cloner.DeepCloneAssets(ctx);
        }

        internal static void Modify(BuildContext ctx)
        {
            if(!shouldModify) return;
            #if LIL_VRCSDK3A
            var controller = ctx.AvatarDescriptor.TryGetFXAnimatorController(ctx);
            var hasWriteDefaultsState = controller.HasWriteDefaultsState();
            var menu = VRChatHelper.CreateMenu(ctx);
            var parameters = VRChatHelper.CreateParameters(ctx);
            var menuDic = new Dictionary<MenuFolder, VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>();
            Modifier.ApplyMenuFolder(ctx, folders, menu, menuDic);
            Modifier.ApplyItemToggler(ctx, controller, hasWriteDefaultsState, togglers, menu, parameters, menuDic);
            Modifier.ApplyProp(ctx, controller, hasWriteDefaultsState, props, menu, parameters, menuDic);
            Modifier.ApplyCostumeChanger(ctx, controller, hasWriteDefaultsState, costumeChangers, menu, parameters, menuDic);
            Modifier.ApplySmoothChanger(ctx, controller, hasWriteDefaultsState, smoothChangers, menu, parameters, menuDic);
            ctx.AvatarDescriptor.MergeParameters(menu, parameters, ctx);
            #else
            // Not supported
            #endif
            Modifier.ApplyMaterialModifier(materials, modifiers);
        }

        internal static void RemoveComponent(BuildContext ctx)
        {
            if(!shouldModify) return;
            foreach(var component in ctx.AvatarRootObject.GetComponentsInChildren<AvatarTagComponent>(true))
                Object.DestroyImmediate(component);
        }

        internal static void Optimize(BuildContext ctx)
        {
            if(shouldModify && optimizers.Length != 0) Optimizer.OptimizeMaterials(materials);
        }
    }
}
