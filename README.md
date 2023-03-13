# Extreal.Integration.AssetWorkflow.Addressables.Test

## How to test

### Initialization

1. Enable Hosting in Addressables Hosting window (`Window > AssetManagement > Addressables > Hosting`).
1. If Hosting is unable to be enabled, do the following.
    1. Change Port number.
    1. Remove all files in `ServerData/StandaloneWindows64/` and `ServerData/Save/`
    1. Run `Build > New Build > Test Encrypt Build Script` in Addressables Groups window (`Window > AssetManagement > Addressables > Groups`).
    1. Remove the file that starts with "acquisition".
    1. Move all files in `ServerData/StandaloneWindows64/` to `ServerData/Save/`
    1. Change the color of CubeMaterial in `Assets/Tests/Materials/`
    1. Run `Build > New Build > Test Encrypt Build Script` in Addressables Groups window again.
    1. Remove the file that starts with "acquisition".

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

- Check the source code around the asserted log content for the following tests.
  - LoadAssetWithAssetBundleCrcDisabledCrypto
  - LoadAssetWithAssetBundleCrcEnabledExcludingCachedCrypto
  - LoadAssetWithAssetBundleCrcEnabledIncludingCachedCrypto
  - LoadAssetWithHttpRedirectLimitCrypto
  - LoadAssetWithoutHttpRedirectLimitCrypto
