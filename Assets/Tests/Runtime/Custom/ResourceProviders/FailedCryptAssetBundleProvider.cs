namespace Extreal.Integration.AssetWorkflow.Addressables.Custom.ResourceProviders.Test
{
    [System.ComponentModel.DisplayName("Failed Crypto AssetBundle Provider")]
    public class FailedCryptoAssetBundleProvider : CryptoAssetBundleProviderBase
    {
        public override ICryptoStreamFactory CryptoStreamFactory => new FailedCryptoStreamFactory();
    }
}
