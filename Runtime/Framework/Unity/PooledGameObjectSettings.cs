using UnityEngine;

namespace SWeetJelly.Pooling.Framework.Unity
{
    public class PooledGameObjectSettings
    {
        //activate
        private bool activate;
        private bool useActiveOnHierarchy;
        public bool Activate => activate;
        public bool UseActiveOnHierarchy => useActiveOnHierarchy;

        //position
        private Vector3 position;
        private bool applyPosition;
        public Vector3 Position => position;
        public bool ApplyPosition => applyPosition;

        //localPosition
        private Vector3 localPosition;
        private bool applyLocalPosition;
        public Vector3 LocalPosition => localPosition;
        public bool ApplyLocalPosition => applyLocalPosition;

        //eulerRotation
        private Vector3 eulerRotation;
        private bool applyEulerRotation;
        public Vector3 EulerRotation => eulerRotation;
        public bool ApplyEulerRotation => applyEulerRotation;

        //localEulerRotation
        private Vector3 localEulerRotation;
        private bool applyLocalEulerRotation;
        public Vector3 LocalEulerRotation => localEulerRotation;
        public bool ApplyLocalEulerRotation => applyLocalEulerRotation;

        //rotation
        private Quaternion rotation;
        private bool applyRotation;
        public Quaternion Rotation => rotation;
        public bool ApplyRotation => applyRotation;

        //localRotation
        private Quaternion localRotation;
        private bool applyLocalRotation;
        public Quaternion LocalRotation => localRotation;
        public bool ApplyLocalRotation => applyLocalRotation;

        //localScale
        private Vector3 localScale;
        private bool applyLocalScale;
        public Vector3 LocalScale => localScale;
        public bool ApplyLocalScale => applyLocalScale;

        //parent
        private Transform parent;
        private bool applyParent;
        public Transform Parent => parent;
        public bool ApplyParent => applyParent;

        public PooledGameObjectSettings(bool activate = true, bool useActiveOnHierarchy = false)
        {
            this.activate = activate;
            this.useActiveOnHierarchy = useActiveOnHierarchy;

            applyPosition = false;
            applyEulerRotation = false;
            applyLocalEulerRotation = false;
            applyRotation = false;
            applyLocalRotation = false;
            applyLocalScale = false;
            applyParent = false;
        }

        public PooledGameObjectSettings SetActivate(bool activate)
        {
            this.activate = activate;
            return this;
        }

        public PooledGameObjectSettings SetPosition(Vector3 position)
        {
            this.position = position;
            applyPosition = true;
            return this;
        }

        public PooledGameObjectSettings SetLocalPosition(Vector3 localPosition)
        {
            this.localPosition = localPosition;
            applyLocalPosition = true;
            return this;
        }

        public PooledGameObjectSettings SetEulerRotation(Vector3 eulerRotation)
        {
            this.eulerRotation = eulerRotation;
            applyEulerRotation = true;
            return this;
        }

        public PooledGameObjectSettings SetLocalEulerRotation(Vector3 eulerRotation)
        {
            this.localEulerRotation = eulerRotation;
            applyLocalEulerRotation = true;
            return this;
        }

        public PooledGameObjectSettings SetRotation(Quaternion rotation)
        {
            this.rotation = rotation;
            applyRotation = true;
            return this;
        }

        public PooledGameObjectSettings SetLocalRotation(Quaternion rotation)
        {
            this.localRotation = rotation;
            applyLocalRotation = true;
            return this;
        }

        public PooledGameObjectSettings SetLocalScale(Vector3 localScale)
        {
            this.localScale = localScale;
            applyLocalScale = true;
            return this;
        }

        public PooledGameObjectSettings SetLocalScale(float localScale)
        {
            this.localScale = localScale * Vector3.one;
            applyLocalScale = true;
            return this;
        }

        public PooledGameObjectSettings SetParent(Transform parent)
        {
            this.parent = parent;
            applyParent = true;
            return this;
        }
    }
}