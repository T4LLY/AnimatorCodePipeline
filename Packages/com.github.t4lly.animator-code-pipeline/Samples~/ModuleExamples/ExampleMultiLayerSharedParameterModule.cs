using System;
using AnimatorAsCode.V1;

namespace ProjectAnimatorCode.Samples
{
    // Demonstrates:
    // - One module coordinating multiple generated layers
    // - Reusing the same Animator parameter name across layers
    // - One module inspecting multiple avatar objects
    [Serializable]
    public sealed class ExampleMultiLayerSharedParameterModule : AnimatorCodePipeline.AnimatorCodeModule
    {
        public override string Id => "sample.multi-layer-shared-parameter";

        public override void Build(AnimatorCodePipeline.AnimatorCodeBuildContext context)
        {
            var primaryTarget =
                context.RequireGameObject("REPLACE/WITH/PRIMARY_TARGET");

            var secondaryTarget =
                context.RequireGameObject("REPLACE/WITH/SECONDARY_TARGET");

            const string parameterName = "ExampleMultiLayerEnabled";

            var primaryLayer = context.Layer("ExampleMultiLayer.Primary");
            var primaryEnabled = primaryLayer.BoolParameter(parameterName);

            var primaryOff = primaryLayer.NewState("Primary Off")
                .WithAnimation(
                    context.Aac.NewClip("Primary Off")
                        .Toggling(primaryTarget, false));

            var primaryOn = primaryLayer.NewState("Primary On")
                .WithAnimation(
                    context.Aac.NewClip("Primary On")
                        .Toggling(primaryTarget, true));

            primaryOff.TransitionsTo(primaryOn).When(primaryEnabled.IsTrue());
            primaryOn.TransitionsTo(primaryOff).When(primaryEnabled.IsFalse());

            var secondaryLayer = context.Layer("ExampleMultiLayer.Secondary");

            // AAC parameters are requested on every layer that uses them.
            // The Animator parameter name remains the same.
            var secondaryEnabled = secondaryLayer.BoolParameter(parameterName);

            var secondaryOff = secondaryLayer.NewState("Secondary Off")
                .WithAnimation(
                    context.Aac.NewClip("Secondary Off")
                        .Toggling(secondaryTarget, false));

            var secondaryOn = secondaryLayer.NewState("Secondary On")
                .WithAnimation(
                    context.Aac.NewClip("Secondary On")
                        .Toggling(secondaryTarget, true));

            secondaryOff.TransitionsTo(secondaryOn).When(secondaryEnabled.IsTrue());
            secondaryOn.TransitionsTo(secondaryOff).When(secondaryEnabled.IsFalse());

            context.ModularAvatar
                .NewParameter(primaryEnabled)
                .WithDefaultValue(false);
        }
    }
}
