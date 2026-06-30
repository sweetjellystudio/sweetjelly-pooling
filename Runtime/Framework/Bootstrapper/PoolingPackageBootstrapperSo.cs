namespace SWeetJelly.Pooling.Framework.Bootstrapper
{
    using UnityEngine;
    using SWeetJelly.Bootstrapper.Framework.Unity;
    using SWeetJelly.Pooling.Framework.Unity;
    [CreateAssetMenu(
        fileName = "Pooling Package Bootstrapper", 
        menuName = "SWeetJelly/Bootstrapper/Packages/Pooling")]
    public class PoolingPackageBootstrapperSo : PackageBootstrapperSo
    {
        [SerializeField] private GameObjectPoolDatabaseSo database;
        public override string PackageName => "Pooling";

        public override void Bootstrap(PackageBootstrapContext context)
        {
            var service = context.HolderObject.GetComponent<GameObjectPoolServiceBehaviour>();

            if (service == null)
            {
                service = context.HolderObject.AddComponent<GameObjectPoolServiceBehaviour>();
            }

            service.Initialise(database, context.HolderObject.transform);

            context.Services.Register<IGameObjectPoolService>(service);
        }
    }
}