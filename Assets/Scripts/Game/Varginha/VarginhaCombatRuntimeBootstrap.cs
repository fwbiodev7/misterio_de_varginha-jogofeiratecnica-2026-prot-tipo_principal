using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Player;

namespace Game.Varginha
{
    /// <summary>Conecta o ataque ao jogador e protege a entidade principal em cenas antigas e na Fase 2.</summary>
    public static class VarginhaCombatRuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            SceneManager.sceneLoaded -= InstallOnSceneLoaded;
            SceneManager.sceneLoaded += InstallOnSceneLoaded;
            InstallOnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private static void InstallOnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var house = root.transform.Find("House_And_Yard");
                if (house != null) VarginhaEnvironmentPolish.EnsureHouse(house);
                else if (root.name == "House_And_Yard") VarginhaEnvironmentPolish.EnsureHouse(root.transform);
            }
            foreach (var camera in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include))
                VarginhaPixelPresentation.Configure(camera);
            var player = Object.FindAnyObjectByType<EdelzioTopDownController>();
            if (player != null)
            {
                if (player.GetComponent<VarginhaPlayerAttack>() == null)
                    player.gameObject.AddComponent<VarginhaPlayerAttack>();
                if (player.GetComponent<HealthSystem>() == null)
                {
                    var health = player.gameObject.AddComponent<HealthSystem>();
                    health.SetMaxHealth(100f, false);
                }
                if (player.GetComponent<VarginhaGameOverFlow>() == null)
                    player.gameObject.AddComponent<VarginhaGameOverFlow>();
            }

            foreach (var ai in Object.FindObjectsByType<EntityManifestationAI>(FindObjectsInactive.Include))
            {
                var target = ai.GetComponent<VarginhaCombatTarget>() ?? ai.gameObject.AddComponent<VarginhaCombatTarget>();
                target.SetKind(VarginhaCombatTarget.EnemyKind.AncestralEntity);
                if (ai.GetComponent<HealthSystem>() == null) ai.gameObject.AddComponent<HealthSystem>();
            }
        }
    }
}
