using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

namespace Game.Tests.PlayMode
{
    /// <summary>
    /// Testa carregamento de cenas em PlayMode.
    /// Cobre: cena ativa, carregamento por nome, verificação de existência.
    /// </summary>
    public class LevelLoadTests
    {
        // ─── Scene Info ───────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator ActiveScene_HasValidName()
        {
            yield return null;
            string name = SceneManager.GetActiveScene().name;
            Assert.IsFalse(string.IsNullOrEmpty(name),
                "Cena ativa deve ter nome válido.");
        }

        [UnityTest]
        public IEnumerator ActiveScene_IsLoaded()
        {
            yield return null;
            Assert.IsTrue(SceneManager.GetActiveScene().isLoaded,
                "Cena ativa deve estar carregada.");
        }

        [UnityTest]
        public IEnumerator SceneManager_HasAtLeastOneScene()
        {
            yield return null;
            Assert.GreaterOrEqual(SceneManager.sceneCount, 1,
                "Deve haver pelo menos uma cena carregada.");
        }

        // ─── Build Settings Scenes ────────────────────────────────────────────

        [Test]
        public void BuildSettings_HasScenesConfigured()
        {
            int count = SceneManager.sceneCountInBuildSettings;
            Assert.Greater(count, 0,
                "Deve haver cenas configuradas no Build Settings.");
        }

        // ─── Additive Scene Loading ───────────────────────────────────────────

        [UnityTest]
        public IEnumerator LoadScene_Additive_IncreasesSceneCount()
        {
            int before = SceneManager.sceneCount;

            // Cria uma cena vazia e carrega additivamente
            var newScene = SceneManager.CreateScene("TempTestScene");
            yield return null;

            Assert.AreEqual(before + 1, SceneManager.sceneCount,
                "Cena adicional deve aumentar contagem de cenas.");

            SceneManager.UnloadSceneAsync(newScene);
            yield return null;
        }

        [UnityTest]
        public IEnumerator LoadScene_Additive_CanBeUnloaded()
        {
            var newScene = SceneManager.CreateScene("TempUnloadScene");
            yield return null;

            var op = SceneManager.UnloadSceneAsync(newScene);
            yield return op;

            Assert.IsFalse(newScene.isLoaded,
                "Cena adicional deve poder ser descarregada.");
        }

        // ─── DontDestroyOnLoad ────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator DontDestroyOnLoad_ObjectPersistsBetweenScenes()
        {
            var persistGo = new GameObject("Persistent");
            Object.DontDestroyOnLoad(persistGo);

            // Cria cena temporária
            var tempScene = SceneManager.CreateScene("TempPersistScene");
            SceneManager.SetActiveScene(tempScene);
            yield return null;

            Assert.IsTrue(persistGo != null,
                "Objeto com DontDestroyOnLoad deve persistir.");

            Object.Destroy(persistGo);
            SceneManager.UnloadSceneAsync(tempScene);
            yield return null;
        }

        // ─── LevelManager Integration ─────────────────────────────────────────

        [UnityTest]
        public IEnumerator LevelManager_IsNotLoading_Initially()
        {
            var go = new GameObject("LevelManager");
            var lm = go.AddComponent<Managers.LevelManager>();
            yield return null;
            Assert.IsFalse(lm.IsLoading, "LevelManager não deve estar carregando inicialmente.");
            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator LevelManager_CurrentScene_MatchesActiveScene()
        {
            var go = new GameObject("LevelManager");
            var lm = go.AddComponent<Managers.LevelManager>();
            yield return null;
            Assert.AreEqual(SceneManager.GetActiveScene().name, lm.CurrentScene,
                "CurrentScene deve corresponder à cena ativa.");
            Object.Destroy(go);
        }
    }
}
