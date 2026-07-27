using AnimatorAsCode.V1;
using UnityEngine;

namespace ProjectAnimatorCode.Samples
{
    // Demonstrates:
    // - RequireComponent<T>
    // - Float parameter
    // - Simple 1D Blend Tree
    // - Modular Avatar parameter registration
    // - Radial menu item
    public sealed class ExampleBlendShapeRadialModule : AnimatorCodePipeline.AnimatorCodeModule
    {
        public override string Id => "sample.blend-shape-radial";

        public override void Build(AnimatorCodePipeline.AnimatorCodeBuildContext context)
        {
            var renderer =
                context.RequireComponent<SkinnedMeshRenderer>("REPLACE/WITH/BODY/PATH");

            var layer = context.Layer("ExampleBlendShapeRadial");
            var weight = layer.FloatParameter("ExampleBlendShapeWeight");

            var blendTree = context.Aac.NewBlendTree()
                .Simple1D(weight)
                .WithAnimation(
                    context.Aac.NewClip("BlendShape 0")
                        .BlendShape(renderer, "REPLACE_BLENDSHAPE", 0f),
                    0f)
                .WithAnimation(
                    context.Aac.NewClip("BlendShape 100")
                        .BlendShape(renderer, "REPLACE_BLENDSHAPE", 100f),
                    1f);

            layer.NewState("Blend")
                .WithAnimation(blendTree);

            context.ModularAvatar
                .NewParameter(weight)
                .WithDefaultValue(0f);

            context.ModularAvatar
                .EditMenuItemOnSelf()
                .Name("Example Blend Shape")
                .Radial(weight);
        }
    }
}
