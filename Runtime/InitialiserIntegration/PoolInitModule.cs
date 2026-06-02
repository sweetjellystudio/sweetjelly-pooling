namespace SWeetJelly.Pooling.InitialiserIntegration
{
    using UnityEngine;
    using SWeetJelly.Initialiser.Contract;
    using SWeetJelly.Initialiser.Framework;
    using SWeetJelly.Pooling.Framework;

    [RegisterModule("Pooling")]
    [CreateAssetMenu(
        fileName = "Pool Init Module", 
        menuName = "SWeetJelly/Initialiser/Modules/Pooling")]
    public class PoolInitModule : InitModuleSo
    {
        [SerializeField] private GameObjectPoolDatabaseSo database;
        public override string ModuleName => "Pooling";

        public override void Install(InitModuleInstallContext context)
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