# [Coimbra Framework](Index.md): Scriptable Settings

Easily access a [ScriptableObject] from anywhere with option to preload those on the application startup.

[ScriptableSettings] are singleton-like objects meant to be used as globally-accessible read-only data containers.
Their creation usually happens inside the editor and are persistent at runtime.

> Most of those things aren't enforced anywhere, but it is strongly encouraged to only use [ScriptableSettings] for this use case to avoid code-smells and bad design decisions.

You can check its usage details in the [ScriptableSettings] APIs and in `Difficulty Settings` package sample.

## Implementing Settings

To implement a new settings you only need to:

1. Inherit from [ScriptableSettings].
2. Add you data fields (usually private fields with public getters).
3. Add either [CreateAssetMenuAttribute], [PreferencesAttribute], or [ProjectSettingsAttribute].
4. (Optional) If it is not an editor-only [ScriptableSettings] you can enable the `Preload` option in the inspector to add it to the `Preloaded Assets`.
5. (Optional) Add the [ScriptableSettingsProviderAttribute] to change from the default [FindAnywhereScriptableSettingsProvider] to either [LoadOrCreateScriptableSettingsProvider] or a custom one by implementing the [IScriptableSettingsProvider].

[FindAnywhereScriptableSettingsProvider]:<../Coimbra/ScriptableSettingsProviders/FindAnywhereScriptableSettingsProvider.cs>

[IScriptableSettingsProvider]:<../Coimbra/IScriptableSettingsProvider.cs>

[LoadOrCreateScriptableSettingsProvider]:<../Coimbra/ScriptableSettingsProviders/LoadOrCreateScriptableSettingsProvider.cs>

[PreferencesAttribute]:<../Coimbra/PreferencesAttribute.cs>

[ProjectSettingsAttribute]:<../Coimbra/ProjectSettingsAttribute.cs>

[ScriptableSettings]:<../Coimbra/ScriptableSettings.cs>

[ScriptableSettingsProviderAttribute]:<../Coimbra/ScriptableSettingsProviderAttribute.cs>

[CreateAssetMenuAttribute]:<https://docs.unity3d.com/ScriptReference/CreateAssetMenuAttribute.html>

[ScriptableObject]:<https://docs.unity3d.com/ScriptReference/ScriptableObject.html>