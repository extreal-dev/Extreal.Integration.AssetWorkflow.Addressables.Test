using UnityEngine.ResourceManagement.ResourceProviders;

namespace Extreal.Integration.AssetWorkflow.Addressables.Custom.ResourceProviders.Test
{
    [System.ComponentModel.DisplayName("AES-CBC AssetBundle Provider")]
    public class AesCbcAssetBundleProvider : CryptoAssetBundleProviderBase
    {
        public override ICryptoStreamFactory CryptoStreamFactory => new AesCbcStreamFactory();

        public static AssetBundleRequestOptions Options { get; private set; }

        public override void Provide(ProvideHandle providerInterface)
        {
            Options = providerInterface.Location?.Data as AssetBundleRequestOptions;
            base.Provide(providerInterface);
        }
    }
}
