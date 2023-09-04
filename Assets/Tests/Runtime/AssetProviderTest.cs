using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UniRx;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.AddressableAssets;
using System.Linq;
using Extreal.Core.Common.Retry;
using Extreal.Core.Logging;

namespace Extreal.Integration.AssetWorkflow.Addressables.Test
{
    public class AssetProviderTest
    {
        private AssetProvider assetProvider;

        private string downloadingAssetName;
        private readonly Dictionary<string, List<AssetDownloadStatus>> downloadedStatuses
            = new Dictionary<string, List<AssetDownloadStatus>>();

        [SuppressMessage("Design", "CC0033")]
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        private const string CubeName = "Cube";
        private const string Cube1Name = "Cube1";
        private const string Cube2Name = "Cube2";
        private const string Cube3Name = "Cube3";
        private const string AdditiveSceneName = "AdditiveScene";
        private const string NotExistedName = "NotExisted";

        [SetUp]
        public void Initialize()
        {
            UnityEngine.AddressableAssets.Addressables.ResourceManager.WebRequestOverride
                = uwr => uwr.timeout = 2;

            LoggingManager.Initialize(LogLevel.Debug);

            if (Caching.ClearCache())
            {
                Debug.Log("Caching Clear !!!!!!!!!!!!!!!!!!!!!!!!");
            }

            assetProvider = new AssetProvider(new CountingRetryStrategy(5)).AddTo(disposables);

            downloadingAssetName = default;
            downloadedStatuses.Clear();

            _ = assetProvider.OnDownloading
                .Subscribe(assetName => downloadingAssetName = assetName)
                .AddTo(disposables);

            _ = assetProvider.OnDownloaded
                .Subscribe(downloadStatus =>
                {
                    if (downloadedStatuses.TryGetValue(downloadStatus.AssetName, out var namedDownloadStatuses))
                    {
                        namedDownloadStatuses.Add(downloadStatus);
                    }
                    else
                    {
                        downloadedStatuses[downloadStatus.AssetName]
                            = new List<AssetDownloadStatus> { downloadStatus };
                    }
                })
                .AddTo(disposables);

            _ = assetProvider.OnConnectRetrying
                .Subscribe(retryCount => Debug.Log($"***** OnConnectRetrying: {retryCount} *****"))
                .AddTo(disposables);

            _ = assetProvider.OnConnectRetried
                .Subscribe(result => Debug.Log($"***** OnConnectRetried: {result} *****"))
                .AddTo(disposables);
        }

        [TearDown]
        public void Dispose()
            => disposables.Clear();

        [OneTimeTearDown]
        public void OneTimeTeatDown()
            => disposables.Dispose();

        [UnityTest]
        public IEnumerator GetDownloadSize() => UniTask.ToCoroutine(async () =>
        {
            var cubeSize = await assetProvider.GetDownloadSizeAsync(Cube1Name);

            Assert.That(cubeSize, Is.Not.Zero);
        });

        [UnityTest]
        public IEnumerator Download() => UniTask.ToCoroutine(async () =>
        {
            LogAssert.ignoreFailingMessages = true;

            await assetProvider.DownloadAsync(CubeName);

            Assert.That(downloadingAssetName, Is.EqualTo(CubeName));
            Assert.That(downloadedStatuses.Keys, Does.Contain(CubeName));
            var lastDownloadStatus = downloadedStatuses[CubeName].Last();
            Assert.That(lastDownloadStatus.Status.DownloadedBytes, Is.EqualTo(lastDownloadStatus.Status.TotalBytes));
            Assert.That(lastDownloadStatus.Status.IsDone, Is.True);
            Assert.That(lastDownloadStatus.Status.Percent, Is.EqualTo(1f));
        });

        [UnityTest]
        public IEnumerator DownloadWithInterval() => UniTask.ToCoroutine(async () =>
        {
            await assetProvider.DownloadAsync(Cube2Name, TimeSpan.FromSeconds(0.5f));

            Assert.That(downloadingAssetName, Is.EqualTo(Cube2Name));
            Assert.That(downloadedStatuses.Keys, Does.Contain(Cube2Name));
            Assert.That(downloadedStatuses[Cube2Name].Last().Status.Percent, Is.EqualTo(1f));
        });

        [UnityTest]
        public IEnumerator DownloadWithNextFunc() => UniTask.ToCoroutine(async () =>
        {
            var isCalled = false;
#pragma warning disable CS1998
            Func<UniTask> nextFunc = async () => isCalled = true;
#pragma warning restore CS1998

            await assetProvider.DownloadAsync(Cube3Name, nextFunc: nextFunc);

            Assert.That(downloadingAssetName, Is.EqualTo(Cube3Name));
            Assert.That(downloadedStatuses.Keys, Does.Contain(Cube3Name));
            Assert.That(downloadedStatuses[Cube3Name].Last().Status.Percent, Is.EqualTo(1f));
            Assert.That(isCalled, Is.True);
        });

        [UnityTest]
        public IEnumerator LoadAssetWithAssetNameSuccess() => UniTask.ToCoroutine(async () =>
        {
            LogAssert.ignoreFailingMessages = true;
            using var disposableCube = await assetProvider.LoadAssetAsync<GameObject>(CubeName);

            Assert.That(disposableCube.Result, Is.Not.Null);
        });

        [UnityTest]
        public IEnumerator LoadAssetWithoutAssetNameSuccess() => UniTask.ToCoroutine(async () =>
        {
            using var disposableTestObject = await assetProvider.LoadAssetAsync<TestObject>();

            Assert.That(disposableTestObject.Result, Is.Not.Null);
            Assert.That(disposableTestObject.Result.Tag, Is.EqualTo(nameof(TestObject)));
        });

        [UnityTest]
        public IEnumerator LoadAssetFailure() => UniTask.ToCoroutine(async () =>
        {
            LogAssert.ignoreFailingMessages = true;
            try
            {
                _ = await assetProvider.LoadAssetAsync<GameObject>(NotExistedName);
                Assert.Fail("This test must raise exceptions");
            }
            catch (InvalidKeyException e)
            {
                Assert.That(e.Message, Does.Contain($"Key={NotExistedName}"));
            }
            catch (OperationException e)
            {
                Assert.That(e, Has.InnerException);
                Assert.That(e.InnerException.GetType(), Is.EqualTo(typeof(InvalidKeyException)));
                Assert.That(e.InnerException.Message, Does.Contain($"Key={NotExistedName}"));
            }
        });

        [UnityTest]
        public IEnumerator LoadSceneSuccess() => UniTask.ToCoroutine(async () =>
        {
            LogAssert.ignoreFailingMessages = true;
            using var disposableScene = await assetProvider.LoadSceneAsync(AdditiveSceneName);

            Assert.That(disposableScene.Result, Is.Not.Null);
        });

        [UnityTest]
        public IEnumerator LoadSceneFailure() => UniTask.ToCoroutine(async () =>
        {
            LogAssert.ignoreFailingMessages = true;
            try
            {
                _ = await assetProvider.LoadSceneAsync(NotExistedName);
                Assert.Fail("This test must raise exceptions");
            }
            catch (InvalidKeyException e)
            {
                Assert.That(e.Message, Does.Contain($"Key={NotExistedName}"));
            }
            catch (OperationException e)
            {
                Assert.That(e, Has.InnerException);
                Assert.That(e.InnerException.GetType(), Is.EqualTo(typeof(InvalidKeyException)));
                Assert.That(e.InnerException.Message, Does.Contain($"Key={NotExistedName}"));
            }
        });
    }
}
