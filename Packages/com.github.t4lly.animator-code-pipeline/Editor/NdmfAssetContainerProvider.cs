using AnimatorAsCode.V1;
using nadena.dev.ndmf;
using UnityEngine;

namespace AnimatorCodePipeline
{
    /// <summary>
    /// Bridges Animator As Code asset persistence to NDMF's asset saver.
    /// Matches the integration recommended for AAC 1.2.0+ with NDMF 1.6.0+.
    /// </summary>
    internal sealed class NdmfAssetContainerProvider : IAacAssetContainerProvider
    {
        private readonly BuildContext _context;

        public NdmfAssetContainerProvider(BuildContext context)
        {
            _context = context;
        }

        public void SaveAsPersistenceRequired(Object objectToAdd)
        {
            _context.AssetSaver.SaveAsset(objectToAdd);
        }

        public void SaveAsRegular(Object objectToAdd)
        {
            // NDMF crawls referenced temporary assets at the end of the build.
        }

        public void ClearPreviousAssets()
        {
            // Animator Code Pipeline is non-destructive and uses NDMF's temporary build context.
        }
    }
}
