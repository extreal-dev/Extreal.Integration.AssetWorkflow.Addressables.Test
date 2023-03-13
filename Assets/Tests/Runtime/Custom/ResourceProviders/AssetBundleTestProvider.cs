using System.ComponentModel;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Extreal.Integration.AssetWorkflow.Addressables.Test
{
    [DisplayName("AssetBundle Test Provider")]

    public class AssetBundleTestProvider : AssetBundleProvider
    {
        public static AssetBundleRequestOptions Options { get; private set; }

        public override void Provide(ProvideHandle providerInterface)
        {
            Options = providerInterface.Location?.Data as AssetBundleRequestOptions;
            base.Provide(providerInterface);
        }
    }
}
