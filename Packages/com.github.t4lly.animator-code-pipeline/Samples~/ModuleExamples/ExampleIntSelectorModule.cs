using AnimatorAsCode.V1;

namespace ProjectAnimatorCode.Samples
{
    // Demonstrates:
    // - Int parameter
    // - Multiple required avatar objects
    // - One-of-many state selection
    // - Modular Avatar parameter registration
    public sealed class ExampleIntSelectorModule : AnimatorCodePipeline.AnimatorCodeModule
    {
        public override string Id => "sample.int-selector";

        public override void Build(AnimatorCodePipeline.AnimatorCodeBuildContext context)
        {
            var optionA = context.RequireGameObject("REPLACE/WITH/OPTION_A");
            var optionB = context.RequireGameObject("REPLACE/WITH/OPTION_B");
            var optionC = context.RequireGameObject("REPLACE/WITH/OPTION_C");

            var layer = context.Layer("ExampleIntSelector");
            var selection = layer.IntParameter("ExampleSelection");

            var stateA = layer.NewState("Option A")
                .WithAnimation(
                    context.Aac.NewClip("Select A")
                        .Toggling(optionA, true)
                        .Toggling(optionB, false)
                        .Toggling(optionC, false));

            var stateB = layer.NewState("Option B")
                .WithAnimation(
                    context.Aac.NewClip("Select B")
                        .Toggling(optionA, false)
                        .Toggling(optionB, true)
                        .Toggling(optionC, false));

            var stateC = layer.NewState("Option C")
                .WithAnimation(
                    context.Aac.NewClip("Select C")
                        .Toggling(optionA, false)
                        .Toggling(optionB, false)
                        .Toggling(optionC, true));

            stateA.TransitionsTo(stateB).When(selection.IsEqualTo(1));
            stateA.TransitionsTo(stateC).When(selection.IsEqualTo(2));

            stateB.TransitionsTo(stateA).When(selection.IsEqualTo(0));
            stateB.TransitionsTo(stateC).When(selection.IsEqualTo(2));

            stateC.TransitionsTo(stateA).When(selection.IsEqualTo(0));
            stateC.TransitionsTo(stateB).When(selection.IsEqualTo(1));

            context.ModularAvatar
                .NewParameter(selection)
                .WithDefaultValue(0);
        }
    }
}
