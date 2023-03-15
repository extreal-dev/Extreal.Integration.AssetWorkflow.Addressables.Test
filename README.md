# Extreal.Integration.AssetWorkflow.Addressables.Test

## How to test

### Initialization

1. Enable Hosting in Addressables Hosting window (`Window > AssetManagement > Addressables > Hosting`).
    - If Hosting is unable to be enabled, change port number and repeat this operation again.
1. Build asset bundle
    1. Remove all files except .gitignore in `ServerData/StandaloneWindows64/` and `ServerData/Save/`, If there are files in the directory.
    1. Run `Build > New Build > Test Encrypt Build Script` in Addressables Groups window (`Window > AssetManagement > Addressables > Groups`).
    1. Remove the file whose name begins with "acquisition".
    1. Move all files in `ServerData/StandaloneWindows64/` to `ServerData/Save/`
    1. Change the color of CubeMaterial in `Assets/Tests/Materials/`
    1. Run `Build > New Build > Test Encrypt Build Script` in Addressables Groups window again.
    1. Remove the file whose name begins with "acquisition".
1. Change `Play Mode Script` in Addressables Groups to `Use Existing Build (Windows)`.

### Code coverage measurement

1. Check that Hosting is enabled and go back to [Initialization](#initialization) if it is disabled.
1. Run `Extreal.Integration.AssetWorkflow.Addressables.Test.dll` in Test Runner window.
1. Completed all tests.

### Remote test

"Xxx" is "Crypto" or "Origin".

- If Hosting is enabled, the "LoadAssetFromRemoteXxx" test succeeds.
- Otherwise, LoadAssetFromRemoteXxx fails.
  - The "LoadAssetFromLocalXxx" test and the "LoadAssetFromLocalUsingUwr" test succeed whether Hosting is enabled.

### Watch the source code

Check the source code around the asserted log content for the following tests.

- CryptoAssetBundleResourceTest
  - LoadAssetWithAssetBundleCrcDisabledCrypto
  - LoadAssetWithAssetBundleCrcEnabledExcludingCachedCrypto
  - LoadAssetWithAssetBundleCrcEnabledIncludingCachedCrypto
  - LoadAssetWithHttpRedirectLimitCrypto
  - LoadAssetWithoutHttpRedirectLimitCrypto

### Retry test

Since it is difficult to control the Hosting Service from the test code, retries are tested manually.
Retry tests should be tested by executing the following test methods individually to toggle Enable of the Hosting Service.

- AssetProviderTest
  - Download
  - LoadAssetWithAssetNameSuccess
  - LoadSceneSuccess
