using System;
using AnimatorAsCode.V1;

namespace ProjectAnimatorCode.Samples
{
    // Demonstrates:
    // - Bool parameter groups
    // - Requiring multiple conditions to enter a state
    // - Returning when any condition becomes false
    [Serializable]
    public sealed class ExampleTwoConditionModule : AnimatorCodePipeline.AnimatorCodeModule
    {
        public override string Id => "sample.two-condition";

        public override void Build(AnimatorCodePipeline.AnimatorCodeBuildContext context)
        {
            var target = context.RequireGameObject("REPLACE/WITH/REAL/PATH");

            var layer = context.Layer("ExampleTwoCondition");

            var conditions = layer.BoolParameters(
                "ExampleMasterEnabled",
                "ExampleFeatureEnabled");

            var off = layer.NewState("Off")
                .WithAnimation(
                    context.Aac.NewClip("Off")
                        .Toggling(target, false));

            var on = layer.NewState("On")
                .WithAnimation(
                    context.Aac.NewClip("On")
                        .Toggling(target, true));

            off.TransitionsTo(on).When(conditions.AreTrue());
            on.TransitionsTo(off).When(conditions.IsAnyFalse());

            context.ModularAvatar
                .NewParameter(conditions)
                .WithDefaultValue(false);
        }
    }
}
