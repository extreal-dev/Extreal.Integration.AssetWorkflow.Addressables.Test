using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Extreal.Core.Logging;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.TestTools;
using UniRx;
using Extreal.Integration.AssetWorkflow.Addressables.Test;

namespace Extreal.Integration.AssetWorkflow.Addressables.Custom.ResourceProviders.Test
{
#pragma warning disable IDE0065
    using Addressables = UnityEngine.AddressableAssets.Addressables;
#pragma warning restore IDE0065

    public class CryptoAssetBundleResourceTest
    {
        private AssetProvider assetProvider;


        private const string CryptoName = "CryptoCube";
        private const string OriginName = "OriginCube";

        private const string RemotePrefix = "Remote";
        private const string LocalPrefix = "Local";
        private const string UncompressedPrefix = "Uncompressed";
        private const string Lz4Prefix = "Lz4";
        private const string NotUseAbcPrefix = "NotUseAbc";
        private const string AssetBundleCrcDisabledPrefix = "AssetBundleCrcDisabled";
        private const string AssetBundleCrcEnabledExcludingCachedPrefix
            = "AssetBundleCrcEnabledExcludingCached";
        private const string UseUwrForLocalPrefix = "UseUwrForLocal";
        private const string HttpRedirectLimitPrefix = "HttpRedirectLimit";
        private const string CacheClearWhenSpaceIsNeededInCachePrefix = "CacheClearWhenSpaceIsNeededInCache";
        private const string CacheClearWhenNewVersionLoadedPrefix = "CacheClearWhenNewVersionLoaded";
        private const string FailedCryptoPrefix = "Failed";
        private const string NoOptionsPrefix = "NoOptions";
        private const string AcquisitionFailurePrefix = "AcquisitionFailure";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            const string bundleDirectoryPath = "Temp/com.unity.addressables/Decrypted/";
            if (Directory.Exists(bundleDirectoryPath))
            {
                Directory.Delete(bundleDirectoryPath);
            }
        }

        [SetUp]
        public void Initialize()
        {
            LoggingManager.Initialize(LogLevel.Debug);

            _ = Caching.ClearCache();

            assetProvider = new AssetProvider();
        }

        [TearDown]
        public void Dispose()
            => assetProvider.Dispose();

        [UnityTest]
        public IEnumerator LoadAssetFromRemoteCrypto()
            => UniTask.ToCoroutine(() => LoadAssetFromRemoteAsync(RemotePrefix + CryptoName));

        [UnityTest]
        public IEnumerator LoadAssetFromRemoteOrigin()
            => UniTask.ToCoroutine(() => LoadAssetFromRemoteAsync(RemotePrefix + OriginName));

        private async UniTask LoadAssetFromRemoteAsync(string assetName)
        {
            using var disposableCube = await assetProvider.LoadAssetAsync<GameObject>(assetName);

            Assert.That(disposableCube.Result, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator LoadAssetFromLocalCrypto()
            => UniTask.ToCoroutine(() => LoadAssetFromLocalAsync(LocalPrefix + CryptoName));

        [UnityTest]
        public IEnumerator LoadAssetFromLocalOrigin()
            => UniTask.ToCoroutine(() => LoadAssetFromLocalAsync(LocalPrefix + OriginName, false));

        private async UniTask LoadAssetFromLocalAsync(string assetName, bool isCrypto = true)
        {
            using var disposableCube = await assetProvider.LoadAssetAsync<GameObject>(assetName);

            Assert.That(disposableCube.Result, Is.Not.Null);

            var options = isCrypto ? AesCbcAssetBundleProvider.Options : AssetBundleTestProvider.Options;
            var cab = new CachedAssetBundle(options.BundleName, Hash128.Parse(options.Hash));
            Assert.That(Caching.IsVersionCached(cab), Is.False);
        }

        [UnityTest]
        public IEnumerator LoadAssetWithUncompressedCrypto()
            => UniTask.ToCoroutine(() => LoadAssetWithUncompressedAsync(UncompressedPrefix + CryptoName, 165328L));

        [UnityTest]
        public IEnumerator LoadAssetWithUncompressedOrigin()
            => UniTask.ToCoroutine(() => LoadAssetWithUncompressedAsync(UncompressedPrefix + OriginName, 165296L));

        private async UniTask LoadAssetWithUncompressedAsync(string assetName, long expectedSize)
        {
            var size = await assetProvider.GetDownloadSizeAsync(assetName);
            using var disposableCube = await assetProvider.LoadAssetAsync<GameObject>(assetName);

            Assert.That(disposableCube.Result, Is.Not.Null);
            Assert.That(size, Is.EqualTo(expectedSize));
        }

        [UnityTest]
        public IEnumerator LoadAssetWithLz4Crypto()
            => UniTask.ToCoroutine(() => LoadAssetWithLz4Async(Lz4Prefix + CryptoName, 53920L));

        [UnityTest]
        public IEnumerator LoadAssetWithLz4Origin()
            => UniTask.ToCoroutine(() => LoadAssetWithLz4Async(Lz4Prefix + OriginName, 53882L));

        private async UniTask LoadAssetWithLz4Async(string assetName, long expectedSize)
        {
            var size = await assetProvider.GetDownloadSizeAsync(assetName);
            using var disposableCube = await assetProvider.LoadAssetAsync<GameObject>(assetName);

            Assert.That(disposableCube.Result, Is.Not.Null);
            Assert.That(size, Is.EqualTo(expectedSize));
        }

        [UnityTest]
        public IEnumerator LoadAssetWithLzmaCrypto()
            => UniTask.ToCoroutine(() => LoadAssetWithLzmaAsync(RemotePrefix + CryptoName, 32592L));

        [UnityTest]
        public IEnumerator LoadAssetWithLzmaOrigin()
            => UniTask.ToCoroutine(() => LoadAssetWithLzmaAsync(RemotePrefix + OriginName, 32554L));

        private async UniTask LoadAssetWithLzmaAsync(string assetName, long expectedSize)
        {
            var size = await assetProvider.GetDownloadSizeAsync(assetName);
            using var disposableCube = await assetProvider.LoadAssetAsync<GameObject>(assetName);

            Assert.That(disposableCube.Result, Is.Not.Null);
            Assert.That(size, Is.EqualTo(expectedSize));
        }

        [UnityTest]
        public IEnumerator LoadAssetNotUsingAbcCrypto()
            => UniTask.ToCoroutine(() => LoadAssetNotUsingAbcAsync(NotUseAbcPrefix + CryptoName));

        [UnityTest]
        public IEnumerator LoadAssetNotUsingAbcOrigin()
            => UniTask.ToCoroutine(() => LoadAssetNotUsingAbcAsync(NotUseAbcPrefix + OriginName, false));

        private async UniTask LoadAssetNotUsingAbcAsync(string assetName, bool isCrypto = true)
        {
            using var disposableCube = await assetProvider.LoadAssetAsync<GameObject>(assetName);

            Assert.That(disposableCube.Result, Is.Not.Null);

            var options = isCrypto ? AesCbcAssetBundleProvider.Options : AssetBundleTestProvider.Options;
            var cab = new CachedAssetBundle(options.BundleName, Hash128.Parse(options.Hash));
            Assert.That(Caching.IsVersionCached(cab), Is.False);
        }

        [UnityTest]
        public IEnumerator LoadAssetUsingAbcCrypto()
            => UniTask.ToCoroutine(() => LoadAssetUsingAbcAsync(RemotePrefix + CryptoName));

        [UnityTest]
        public IEnumerator LoadAssetUsingAbcOrigin()
            => UniTask.ToCoroutine(() => LoadAssetUsingAbcAsync(RemotePrefix + OriginName, false));

        private async UniTask LoadAssetUsingAbcAsync(string assetName, bool isCrypto = true)
        {
            using var disposableCube = await assetProvider.LoadAssetAsync<GameObject>(assetName);

            Assert.That(disposableCube.Result, Is.Not.Null);

            var options = isCrypto ? AesCbcAssetBundleProvider.Options : AssetBundleTestProvider.Options;
            var cab = new CachedAssetBundle(options.BundleName, Hash128.Parse(options.Hash));
            Assert.That(Caching.IsVersionCached(cab), Is.True);
        }

        [UnityTest]
        public IEnumerator LoadAssetWithAssetBundleCrcDisabled() => UniTask.ToCoroutine(async () =>
        {
            using var disposableCube = await assetProvider.LoadAssetAsync<GameObject>(AssetBundleCrcDisabledPrefix + CryptoName);

            Assert.That(disposableCube.Result, Is.Not.Null);
            LogAssert.Expect(LogType.Log, new Regex("The Asset Bundle CRC option is Disabled"));

            var options = AesCbcAssetBundleProvider.Options;
            Assert.That(options.Crc, Is.Zero);
        });

        [UnityTest]
        public IEnumerator LoadAssetWithAssetBundleCrcEnabledExcludingCached() => UniTask.ToCoroutine(async () =>
        {
            using var disposableCube = await assetProvider.LoadAssetAsync<GameObject>(AssetBundleCrcEnabledExcludingCachedPrefix + CryptoName);

            Assert.That(disposableCube.Result, Is.Not.Null);
            LogAssert.Expect(LogType.Log, new Regex("The Asset Bundle CRC option is Enabled, Excluding Cached"));

            var options = AesCbcAssetBundleProvider.Options;
            Assert.That(options.Crc, Is.Not.Zero);
            Assert.That(options.UseCrcForCachedBundle, Is.False);
        });

        [UnityTest]
        public IEnumerator LoadAssetWithAssetBundleCrcEnabledIncludingCached() => UniTask.ToCoroutine(async () =>
        {
            using var disposableCube = await assetProvider.LoadAssetAsync<GameObject>(RemotePrefix + CryptoName);

            Assert.That(disposableCube.Result, Is.Not.Null);
            LogAssert.Expect(LogType.Log, new Regex("The Asset Bundle CRC option is Enabled, Including Cached"));

            var options = AesCbcAssetBundleProvider.Options;
            Assert.That(options.Crc, Is.Not.Zero);
            Assert.That(options.UseCrcForCachedBundle, Is.True);
        });

        [UnityTest]
        public IEnumerator LoadAssetFromLocalUsingUwrCrypto()
            => UniTask.ToCoroutine(() => LoadAssetFromLocalUsingUwrAsync(UseUwrForLocalPrefix + CryptoName));

        [UnityTest]
        public IEnumerator LoadAssetFromLocalUsingUwrOrigin()
            => UniTask.ToCoroutine(() => LoadAssetFromLocalUsingUwrAsync(UseUwrForLocalPrefix + OriginName, false));

        private async UniTask LoadAssetFromLocalUsingUwrAsync(string assetName, bool isCrypto = true)
        {
            using var disposableCube = await assetProvider.LoadAssetAsync<GameObject>(assetName);

            Assert.That(disposableCube.Result, Is.Not.Null);

            var options = isCrypto ? AesCbcAssetBundleProvider.Options : AssetBundleTestProvider.Options;
            var cab = new CachedAssetBundle(options.BundleName, Hash128.Parse(options.Hash));
            Assert.That(Caching.IsVersionCached(cab), Is.True);
        }

        [UnityTest]
        public IEnumerator LoadAssetWithHttpRedirectLimit() => UniTask.ToCoroutine(async () =>
        {
            using var disposableCube = await assetProvider.LoadAssetAsync<GameObject>(HttpRedirectLimitPrefix + CryptoName);

            Assert.That(disposableCube.Result, Is.Not.Null);
            LogAssert.Expect(LogType.Log, new Regex("HTTP Redirect Limit is specified: 10"));

            var options = AesCbcAssetBundleProvider.Options;
            Assert.That(options.RedirectLimit, Is.EqualTo(10));
        });

        [UnityTest]
        public IEnumerator LoadAssetWithoutHttpRedirectLimit() => UniTask.ToCoroutine(async () =>
        {
            using var disposableCube = await assetProvider.LoadAssetAsync<GameObject>(RemotePrefix + CryptoName);

            Assert.That(disposableCube.Result, Is.Not.Null);
            LogAssert.Expect(LogType.Log, new Regex("HTTP Redirect Limit is not specified"));

            var options = AesCbcAssetBundleProvider.Options;
            Assert.That(options.RedirectLimit, Is.EqualTo(-1));
        });

        [UnityTest]
        public IEnumerator LoadAssetWithCacheClearWhenSpaceIsNeededInCacheCrypto() => UniTask.ToCoroutine(()
            => LoadAssetWithCacheClearWhenSpaceIsNeededInCacheAsync(
                CacheClearWhenSpaceIsNeededInCachePrefix + CryptoName));

        [UnityTest]
        public IEnumerator LoadAssetWithCacheClearWhenSpaceIsNeededInCacheOrigin() => UniTask.ToCoroutine(()
            => LoadAssetWithCacheClearWhenSpaceIsNeededInCacheAsync(
                CacheClearWhenSpaceIsNeededInCachePrefix + OriginName, false));

        private async UniTask LoadAssetWithCacheClearWhenSpaceIsNeededInCacheAsync
        (
            string assetName,
            bool isCrypto = true
        )
        {
            var cabBefore = default(CachedAssetBundle);
            {
                using var disposableCube
                    = await assetProvider.LoadAssetAsync<GameObject>(assetName);
                var options = isCrypto ? AesCbcAssetBundleProvider.Options : AssetBundleTestProvider.Options;
                cabBefore = new CachedAssetBundle(options.BundleName, Hash128.Parse(options.Hash));

                Assert.That(Caching.IsVersionCached(cabBefore), Is.True);
            }

            MoveDirectory("ServerData\\StandaloneWindows64", "ServerData\\Temp");
            MoveDirectory("ServerData\\Save", "ServerData\\StandaloneWindows64");

            _ = await Addressables.UpdateCatalogs().Task.ConfigureAwait(true);

            {
                using var disposableCube
                    = await assetProvider.LoadAssetAsync<GameObject>(assetName);
                var options = isCrypto ? AesCbcAssetBundleProvider.Options : AssetBundleTestProvider.Options;
                var cabAfter = new CachedAssetBundle(options.BundleName, Hash128.Parse(options.Hash));

                Assert.That(cabAfter, Is.Not.EqualTo(cabBefore));
                Assert.That(Caching.IsVersionCached(cabAfter), Is.True);
                Assert.That(Caching.IsVersionCached(cabBefore), Is.True);
            }

            MoveDirectory("ServerData\\StandaloneWindows64", "ServerData\\Save");
            MoveDirectory("ServerData\\Temp", "ServerData\\StandaloneWindows64");

            _ = await Addressables.UpdateCatalogs().Task.ConfigureAwait(true);
        }

        [UnityTest]
        public IEnumerator LoadAssetWithCacheClearWhenNewVersionLoadedCrypto() => UniTask.ToCoroutine(()
            => LoadAssetWithCacheClearWhenNewVersionLoadedAsync(
                CacheClearWhenNewVersionLoadedPrefix + CryptoName));

        [UnityTest]
        public IEnumerator LoadAssetWithCacheClearWhenNewVersionLoadedOrigin() => UniTask.ToCoroutine(()
            => LoadAssetWithCacheClearWhenNewVersionLoadedAsync(
                CacheClearWhenNewVersionLoadedPrefix + OriginName, false));

        private async UniTask LoadAssetWithCacheClearWhenNewVersionLoadedAsync
        (
            string assetName,
            bool isCrypto = true
        )
        {
            var cabBefore = default(CachedAssetBundle);
            {
                using var disposableCube
                    = await assetProvider.LoadAssetAsync<GameObject>(assetName);
                var options = isCrypto ? AesCbcAssetBundleProvider.Options : AssetBundleTestProvider.Options;
                cabBefore = new CachedAssetBundle(options.BundleName, Hash128.Parse(options.Hash));

                Assert.That(Caching.IsVersionCached(cabBefore), Is.True);
            }

            MoveDirectory("ServerData\\StandaloneWindows64", "ServerData\\Temp");
            MoveDirectory("ServerData\\Save", "ServerData\\StandaloneWindows64");

            _ = await Addressables.UpdateCatalogs().Task.ConfigureAwait(true);

            {
                using var disposableCube
                    = await assetProvider.LoadAssetAsync<GameObject>(assetName);
                var options = isCrypto ? AesCbcAssetBundleProvider.Options : AssetBundleTestProvider.Options;
                var cabAfter = new CachedAssetBundle(options.BundleName, Hash128.Parse(options.Hash));

                Assert.That(cabAfter, Is.Not.EqualTo(cabBefore));
                Assert.That(Caching.IsVersionCached(cabAfter), Is.True);
                Assert.That(Caching.IsVersionCached(cabBefore), Is.False);
            }

            MoveDirectory("ServerData\\StandaloneWindows64", "ServerData\\Save");
            MoveDirectory("ServerData\\Temp", "ServerData\\StandaloneWindows64");

            _ = await Addressables.UpdateCatalogs().Task.ConfigureAwait(true);
        }

        private static void MoveDirectory(string folderFrom, string folderTo)
        {
            foreach (var pathFrom in Directory.EnumerateFiles(folderFrom))
            {
                var pathTo = pathFrom.Replace(folderFrom, folderTo);

                var targetFolder = Path.GetDirectoryName(pathTo);
                if (!Directory.Exists(targetFolder) && targetFolder != null)
                {
                    _ = Directory.CreateDirectory(targetFolder);
                }

                File.Move(pathFrom, pathTo);
            }
        }

        [UnityTest]
        public IEnumerator LoadAssetFromCache() => UniTask.ToCoroutine(async () =>
        {
            await assetProvider.DownloadAsync(RemotePrefix + CryptoName);
            using var disposableCube = await assetProvider.LoadAssetAsync<GameObject>(RemotePrefix + CryptoName);

            Assert.That(disposableCube.Result, Is.Not.Null);
        });

        [UnityTest]
        public IEnumerator GetDownloadStatus() => UniTask.ToCoroutine(async () =>
        {
            var downloadingAssetName = default(string);
            var downloadedStatuses = new Dictionary<string, List<NamedDownloadStatus>>();

            using var onDownloadingDisposable = assetProvider.OnDownloading
                .Subscribe(assetName => downloadingAssetName = assetName);

            using var onDownloadedDisposable = assetProvider.OnDownloaded
                .Subscribe(downloadStatus =>
                {
                    if (downloadedStatuses.TryGetValue(downloadStatus.AssetName, out var namedDownloadStatuses))
                    {
                        namedDownloadStatuses.Add(downloadStatus);
                    }
                    else
                    {
                        downloadedStatuses[downloadStatus.AssetName]
                            = new List<NamedDownloadStatus> { downloadStatus };
                    }
                });

            await assetProvider.DownloadAsync(RemotePrefix + CryptoName);

            Assert.That(downloadingAssetName, Is.EqualTo(RemotePrefix + CryptoName));
            Assert.That(downloadedStatuses.Keys, Does.Contain(RemotePrefix + CryptoName));

            var lastDownloadStatus = downloadedStatuses[RemotePrefix + CryptoName].Last();
            Assert.That(lastDownloadStatus.DownloadedBytes, Is.EqualTo(lastDownloadStatus.TotalBytes));
            Assert.That(lastDownloadStatus.IsDone, Is.True);
            Assert.That(lastDownloadStatus.Percent, Is.EqualTo(1f));
        });

        [UnityTest]
        public IEnumerator LoadAssetWithFailedCrypto() => UniTask.ToCoroutine(async () =>
        {
            LogAssert.ignoreFailingMessages = true;
            try
            {
                using var disposableCube = await assetProvider.LoadAssetAsync<GameObject>(FailedCryptoPrefix + CryptoName);
                Assert.Fail("This test must raise exceptions");
            }
            catch (OperationException e)
            {
                Assert.That(e, Has.InnerException);
                Assert.That(e.InnerException.GetType(), Is.EqualTo(typeof(Exception)));
                Assert.That(e.InnerException.Message, Is.EqualTo("Dependency Exception"));
            }
            catch (Exception e)
            {
                Assert.That(e.Message, Is.EqualTo("Dependency Exception"));
            }
        });

        [UnityTest]
        public IEnumerator FetchWithoutOptions() => UniTask.ToCoroutine(async () =>
        {
            LogAssert.ignoreFailingMessages = true;
            try
            {
                using var disposableCube = await assetProvider.LoadAssetAsync<GameObject>(NoOptionsPrefix + CryptoName);
                Debug.LogWarning("No exception is raised");
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        });

        [UnityTest]
        public IEnumerator LoadAssetFromAcquisitionFailure() => UniTask.ToCoroutine(async () =>
        {
            LogAssert.ignoreFailingMessages = true;
            try
            {
                _ = await assetProvider.LoadAssetAsync<GameObject>(AcquisitionFailurePrefix + CryptoName);
                Assert.Fail("This test must raise exceptions");
            }
            catch (OperationException e)
            {
                Assert.That(e, Has.InnerException);
                Assert.That(e.InnerException.GetType(), Is.EqualTo(typeof(Exception)));
                Assert.That(e.InnerException.Message, Does.Contain("Dependency Exception"));
            }
            catch (Exception e)
            {
                Assert.That(e.Message, Does.Contain("Dependency Exception"));
            }
        });
    }
}
