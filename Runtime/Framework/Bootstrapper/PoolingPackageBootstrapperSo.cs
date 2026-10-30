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
        private GameObjectPoolServiceBehaviour service;
        private bool ownsService;

        public override string PackageName => "Pooling";

        public override void Bootstrap(PackageBootstrapContext context)
        {
            var nextService = context.HolderObject.GetComponent<GameObjectPoolServiceBehaviour>();
            var created = nextService == null;
            if (created)
                nextService = context.HolderObject.AddComponent<GameObjectPoolServiceBehaviour>();

            try
            {
                nextService.Initialise(database, context.HolderObject.transform);
                context.Services.Register<IGameObjectPoolService>(nextService);
            }
            catch
            {
                if (created)
                {
                    nextService.Clear();
                    DestroyOwnedService(nextService);
                }
                throw;
            }
            service = nextService;
            ownsService = created;
        }

        public override void Shutdown(PackageBootstrapContext context)
        {
            if (ReferenceEquals(service, null)) return;
            if (context.Services.TryResolve<IGameObjectPoolService>(out var registered)
                && ReferenceEquals(registered, service))
                context.Services.Unregister<IGameObjectPoolService>();
            if (ownsService && service != null)
            {
                service.Clear();
                DestroyOwnedService(service);
            }
            service = null;
            ownsService = false;
        }

        private static void DestroyOwnedService(GameObjectPoolServiceBehaviour value)
        {
            if (Application.isPlaying) Destroy(value);
            else DestroyImmediate(value);
        }

    }
}