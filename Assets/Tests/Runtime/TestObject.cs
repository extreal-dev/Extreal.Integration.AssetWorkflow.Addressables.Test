using UnityEngine;

namespace Extreal.Integration.AssetWorkflow.Addressables.Test
{

    [CreateAssetMenu(
        menuName = "Extreal/Test/Integration.Assets.Adressables/" + nameof(TestObject),
        fileName = nameof(TestObject))]
    public class TestObject : ScriptableObject
    {
        [SerializeField] private string tag;

        public string Tag => tag;
    }
}
