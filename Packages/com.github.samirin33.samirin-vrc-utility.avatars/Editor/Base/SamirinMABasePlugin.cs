using System.Linq;
using nadena.dev.ndmf;
using Samirin33.NDMF.Components;
using Samirin33.NDMF.Components.Editor;
using Samirin33.NDMF.Base.Plugin;

[assembly: ExportsPlugin(typeof(SamirinMABasePlugin))]

namespace Samirin33.NDMF.Base.Plugin
{
    public class SamirinMABasePlugin : Plugin<SamirinMABasePlugin>
    {
        public override string QualifiedName => "SamirinMABase";

        public override string DisplayName => "SamirinMABase";

        protected override void Configure()
        {
            var seq = InPhase(BuildPhase.Transforming);
            seq.BeforePlugin("nadena.dev.modular-avatar");
            seq.Run("SamirinTransformingBeforeMA", ctx =>
            {
                SamirinMABase[] scripts = ctx.AvatarRootObject.GetComponentsInChildren<SamirinMABase>(true);
                var halfSyncParams = scripts.OfType<HalfSyncParam>().ToArray();
                if (halfSyncParams.Length > 0)
                    HalfSyncParamBuilder.Build(ctx.AvatarRootObject, halfSyncParams);
                foreach (var script in scripts)
                {
                    script.OnBuildTransformingBeforeMA(ctx.AvatarRootObject);
                }
            });

            seq = InPhase(BuildPhase.Transforming);
            seq.AfterPlugin("SamirinTransformingBeforeMA");
            seq.Run("SamirinTransformingAfterMA", ctx =>
            {
                SamirinMABase[] scripts = ctx.AvatarRootObject.GetComponentsInChildren<SamirinMABase>(true);
                foreach (var script in scripts)
                {
                    script.OnBuildTransformingAfterMA(ctx.AvatarRootObject);
                }
            });

            seq = InPhase(BuildPhase.Resolving);
            seq.BeforePlugin("nadena.dev.modular-avatar");
            seq.Run("SamirinResolvingBeforeMA", ctx =>
            {
                SamirinMABase[] scripts = ctx.AvatarRootObject.GetComponentsInChildren<SamirinMABase>(true);
                foreach (var script in scripts)
                {
                    script.OnBuildResolvingBeforeMA(ctx.AvatarRootObject);
                }
            });

            seq = InPhase(BuildPhase.Resolving);
            seq.AfterPlugin("SamirinResolvingBeforeMA");
            seq.Run("SamirinResolvingAfterMA", ctx =>
            {
                SamirinMABase[] scripts = ctx.AvatarRootObject.GetComponentsInChildren<SamirinMABase>(true);
                foreach (var script in scripts)
                {
                    script.OnBuildResolvingAfterMA(ctx.AvatarRootObject);
                }
            });
        }
    }
}