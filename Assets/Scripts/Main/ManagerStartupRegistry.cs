using System;
using System.Collections.Generic;
using System.Linq;
using MyGame;

public sealed class StartupManagerRegistration
{
    private readonly Func<IStartupManager> createManager;

    public StartupManagerRegistration(Type managerType, Func<IStartupManager> createManager, IReadOnlyList<Type> dependencies)
    {
        ManagerType = managerType;
        this.createManager = createManager;
        Dependencies = dependencies;
    }

    public Type ManagerType { get; }
    public IReadOnlyList<Type> Dependencies { get; }

    public IStartupManager CreateManager()
    {
        return createManager();
    }
}

public static class ManagerStartupRegistry
{
    public static List<StartupManagerRegistration> CreateStartupManagerRegistrations()
    {
        return new List<StartupManagerRegistration>
        {
            Register<GameActionManager>(() => GameActionManager.instance),
            Register<GameActionDataManager>(() => GameActionDataManager.instance, typeof(GameActionManager)),
            Register<GameVolumeManager>(() => GameVolumeManager.instance, typeof(GameActionManager)),
            Register<CameraManager>(() => CameraManager.instance, typeof(GameVolumeManager), typeof(GameActionManager)),
            Register<GameManager>(() => GameManager.instance, typeof(GameActionManager)),
            Register<GameTimeManager>(() => GameTimeManager.instance, typeof(GameActionManager)),
            Register<LanguageManage>(() => LanguageManage.instance, typeof(GameDataManager)),
            Register<GameRandom>(() => GameRandom.instance, typeof(GameDataManager)),
            Register<PayManager>(() => PayManager.instance, typeof(GameSourceManager), typeof(GameActionManager)),
            Register<WorldMapObjManager>(() => WorldMapObjManager.instance, typeof(GameActionManager)),
            Register<ShopManager>(() => ShopManager.instance, typeof(GameDataManager), typeof(GameActionManager)),
            Register<ExploreManager>(() => ExploreManager.instance, typeof(GameDataManager)),
            Register<SceneManager>(() => SceneManager.instance, typeof(GameActionManager)),
            Register<FightManager>(() => FightManager.instance, typeof(GameDataManager), typeof(GameSourceManager), typeof(GameRandom), typeof(GameActionManager)),
            Register<TalkManager>(() => TalkManager.instance, typeof(GameDataManager), typeof(GameActionManager)),
            Register<FestivalManager>(() => FestivalManager.instance, typeof(GameDataManager), typeof(LanguageManage), typeof(GameTimeManager)),
            Register<GameTimeEventManager>(() => GameTimeEventManager.instance, typeof(GameDataManager), typeof(GameActionManager)),
            Register<TeamManager>(() => TeamManager.instance, typeof(GameActionManager)),
            Register<GameGuideManager>(() => GameGuideManager.instance, typeof(GameActionManager)),
            Register<ShowItemManager>(() => ShowItemManager.instance, typeof(GameActionManager)),
            Register<AudioController>(() => AudioController.instance, typeof(GameSourceManager)),
            Register<EnvironmentManger>(() => EnvironmentManger.instance, typeof(GameDataManager), typeof(CameraManager), typeof(GameActionManager)),
            Register<UIManager>(() => UIManager.instance, typeof(GameSourceManager)),
            Register<InputManager>(() => InputManager.instance, typeof(GameSourceManager))
        };
    }

    public static List<StartupManagerRegistration> SortStartupManagers(IReadOnlyList<StartupManagerRegistration> registrations, HashSet<Type> completedManagers)
    {
        var registrationByType = registrations.ToDictionary(registration => registration.ManagerType);
        var sortedRegistrations = new List<StartupManagerRegistration>();
        var visitingManagers = new HashSet<Type>();
        var visitedManagers = new HashSet<Type>(completedManagers);

        for (int i = 0; i < registrations.Count; i++)
        {
            Visit(registrations[i]);
        }

        return sortedRegistrations;

        void Visit(StartupManagerRegistration registration)
        {
            if (visitedManagers.Contains(registration.ManagerType))
            {
                return;
            }

            if (!visitingManagers.Add(registration.ManagerType))
            {
                throw new InvalidOperationException($"Startup manager dependency cycle: {registration.ManagerType.Name}");
            }

            for (int i = 0; i < registration.Dependencies.Count; i++)
            {
                var dependency = registration.Dependencies[i];
                if (visitedManagers.Contains(dependency))
                {
                    continue;
                }

                if (!registrationByType.TryGetValue(dependency, out var dependencyRegistration))
                {
                    throw new InvalidOperationException($"Startup manager dependency is not registered: manager={registration.ManagerType.Name}, dependency={dependency.Name}");
                }

                Visit(dependencyRegistration);
            }

            visitingManagers.Remove(registration.ManagerType);
            visitedManagers.Add(registration.ManagerType);
            sortedRegistrations.Add(registration);
        }
    }

    private static StartupManagerRegistration Register<T>(Func<IStartupManager> createManager, params Type[] dependencies)
    {
        // 启动注册表是自动初始化的唯一顺序来源，GameController 不再手写 Manager 调用顺序。
        return new StartupManagerRegistration(typeof(T), createManager, dependencies);
    }
}
