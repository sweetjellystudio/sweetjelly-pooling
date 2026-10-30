#   Pooling architecture

Pooling owns generic object reuse and a separate Unity GameObject pooling feature. DDD informs ownership of reuse rules and vocabulary; Clean Architecture informs Unity-independent core contracts/rules and outer technology/composition responsibilities. These principles do not require identical layers or a single engine for both APIs. See [usage][usage] and the [decision][decision].

##  Responsibilities and dependencies

Contract contains IPool<T>, IPoolable and PoolId. Domain implements ObjectPool<T> with factory/lifecycle delegates and explicit active/inactive membership; these assemblies disable Unity references. No additional App layer is needed for this direct API. ObjectPool<T> does not automatically call IPoolable.

Framework.Unity contains GameObjectPool, its builder/configuration/database, spawn settings and IGameObjectPoolService. Its public contract deliberately exposes Unity objects. It declares internal Contract/Domain references but GameObjectPool uses independent prefab lists, not ObjectPool<T>. Availability is determined by activeSelf by default or activeInHierarchy when requested. An inactive parent can make the same activeSelf instance available again under the latter rule. Spawn with activation disabled also leaves an instance available; this is not exclusive leasing.

Framework.Bootstrapper is the composition root integrating Bootstrapper 2.0.0. It locates or creates a service on the context holder, initializes the database and registers the Unity-facing service. Runtime has no Utils dependency. Unity object operations are synchronous and intended for the Unity main thread; no async/cancellation/thread-safety API is provided.

##  Resource and failure ownership

ObjectPool<T> owns its membership and invokes supplied delegates. A failed rent callback restores the inactive item; a failed return callback retains active membership. Exceptions propagate. Clear empties membership before invoking destroy callbacks; a thrown destroy callback propagates and later callbacks are not guaranteed. This existing generic behavior is preserved, not replaced by a new error policy.

GameObject pools own their instantiated clones even when callers reparent them. Clear deactivates and destroys clones, empties lists and permits later initialization. Runtime destruction is deferred to Unity's frame boundary; non-playing Editor cleanup is immediate. A destroyed clone is replaced in its original slot without adding a duplicate list entry. Pool initialization failure clears partial clones. No automatic IPoolable hooks are enabled.

The service publishes a pool only after successful initialization. Database initialization failure removes newly added pools and a newly created default container, restoring previous configuration and keeping pre-existing pools. Existing null-database diagnostics are retained. Typed Spawn with a missing component returns the instance before throwing InvalidOperationException. Foreign Despawn still throws; null/destroyed input retains the existing no-op behavior.

Service Clear and OnDestroy reclaim instances, including those under external parents. A default container created by the service is owned; an externally supplied/serialized container and unrelated children are not destroyed. Direct consumers own the service component's lifetime. Clear is reusable rather than permanent disposal.

A bootstrap module owns a component only when it created it. Failed registration cleans newly created service resources; repeated bootstrap failure preserves the previous successful module ownership. Shutdown unregisters only the same service identity. Owned components and pools are destroyed; borrowed components and their pool lifetime remain with the caller. Initialization can configure an uninitialized borrowed service, so callers retain responsibility for that component even if later registration fails. One asset supports one active runtime context, not simultaneous independent contexts.

##  Compatibility and verification

2.0.0 requires Unity 6.3 LTS and Bootstrapper 2.0.0. Public pooling APIs, serialized field names and asset GUIDs remain. Utils is removed because source, samples and asmdefs do not use it. Availability rules, weighted selection and explicit index selection remain. PoolId keys and callback/error semantics are not redesigned.

Correctness tests belong to the package. EditMode covers generic membership/callbacks, reuse, failure rollback, activation rules and bootstrap ownership. PlayMode covers deferred destruction, external parents and clear/rebuild. Two Samples~ paths show direct and database/bootstrap consumption. Host combinations, isolated tests, actual samples/Inspector and a minimal Windows Player build are separate evidence. Other platforms, Editor versions and IL2CPP require their own runs.

[usage]: README.md
[decision]: docs/adr/0001-pool-reuse-and-resource-ownership.md
