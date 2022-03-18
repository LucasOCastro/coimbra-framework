# Changelog

## [2.2.0] - 2022-03-19

- Added ScriptableSettings to allow easy sharing of ScriptableObject data with non-MonoBehaviour objects.
- Added API documentation for GameObjectUtility.
- Added support for PreloadedAsset inside the editor, including cleanup of missing/null references.
- Added missing assert on non-generic APIs of ServiceLocator.
- Changed GameObjectEventListener to be internal.
- Changed GameObjects to send the GameObject instead of the GameObjectEventListener.
- Fixed IApplicationService missing RequireImplementors attribute.

## [2.1.0] - 2022-03-18

- Added FixedUpdateEvent, LateUpdateEvent, and UpdateServiceEvent to listen to the respective Unity loop callback.
- Added OwningLocator field to IService locator to be able to create services composed of multiple services easily.
- Added overloads that accepts event data by reference in the IEventService to avoid large struct copies.
- Added missing readonly keyword on built-in events.
- Changed ServiceLocator to be Serializable to be able to see which ServiceLocator a MonoBehaviour system belongs to.
- Deprecated IFixedUpdateService, ILateUpdateService, and IUpdateService in favor of new built-in events.

## [2.0.0] - 2022-03-16

- Added IService and enforce its usage in ServiceLocator APIs
- Added IFixedUpdateService, ILateUpdateService, and IUpdateService to register on the respective Unity loop callback.
- Added object.GetValid() and object.IsValid() APIs to make safer to work with abstractions and Unity Objects.
- Added ApplicationFocusEvent, ApplicationPauseEvent, and ApplicationQuitEvent. They are meant to be listened through the EventService.
- Added proper hide flags for all default implementations.
- Added GetValid call for ManagedField.Value and ServiceLocator.Get APIs to make those compatible with `?.` operator.
- Changed folder structure to group similar types together.
- Changed all services to implement the Dispose method to allow per-service cleanup.
- Fixed ServiceLocator not being compatible with Enter Play Mode Options.
- Refactored IEventService and its default implementation:
  - Added HasAnyListeners and HasListener APIs.
  - Added Invoke API that accepts a type and constructs a default event data to be used.
  - Added option to ignore the exception when the event key doesn't match.
  - Changed AddListener API to return an EventHandle.
  - Changed RemoveListener to use an EventHandle, allowing to remove anonymous method too.
- Refactored ITimerService and its default implementation:
  - Changed ITimerService to not have ref params to simplify its usage.
  - Changed TimerHandle to use System.Guid to better ensure uniqueness and made it a readonly struct.
  - Fixed TimerService not working for concurrent timers and added test cases to ensure minimum stability.
- Removed IApplicationService and its default implementation.
- Renamed all default implementations to `System` instead of `Service`.
- Renamed Coimbra.Services to Coimbra.Systems.

## [1.1.0] - 2022-03-08

- Added per-instance option to fallback to the ServiceLocator.Shared on non-shared instances. Enabled by default.
- Added runtime check to ensure that ServiceLocator is only used with interface types. Enabled by default, but can be disabled per-instance (except on the Shared instance).

## [1.0.0] - 2022-01-11

- Initial release