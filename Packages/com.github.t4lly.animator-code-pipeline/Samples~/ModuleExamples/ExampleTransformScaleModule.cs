using System;
using AnimatorAsCode.V1;
using UnityEngine;

namespace ProjectAnimatorCode.Samples
{
    // Demonstrates:
    // - RequireTransform
    // - Animating Transform scale instead of GameObject active state
    // - Preserving the authored scale as the enabled value
    [Serializable]
    public sealed class ExampleTransformScaleModule : AnimatorCodePipeline.AnimatorCodeModule
    {
        public override string Id => "sample.transform-scale";

        public override void Build(AnimatorCodePipeline.AnimatorCodeBuildContext context)
        {
            var target = context.RequireTransform("REPLACE/WITH/REAL/PATH");
            var authoredScale = target.localScale;

            var layer = context.Layer("ExampleTransformScale");
            var enabled = layer.BoolParameter("ExampleTransformScaleEnabled");

            var hidden = layer.NewState("Hidden")
                .WithAnimation(
                    context.Aac.NewClip("Hidden")
                        .Scaling(target, Vector3.zero));

            var visible = layer.NewState("Visible")
                .WithAnimation(
                    context.Aac.NewClip("Visible")
                        .Scaling(target, authoredScale));

            hidden.TransitionsTo(visible).When(enabled.IsTrue());
            visible.TransitionsTo(hidden).When(enabled.IsFalse());

            context.ModularAvatar
                .NewParameter(enabled)
                .WithDefaultValue(true);
        }
    }
}
