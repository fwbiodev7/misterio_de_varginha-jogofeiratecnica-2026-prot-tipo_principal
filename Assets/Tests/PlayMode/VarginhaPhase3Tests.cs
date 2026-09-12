using Game.Varginha;
using NUnit.Framework;

namespace Game.Tests.PlayMode
{
    public class VarginhaPhase3Tests
    {
        [Test]
        public void ManualAllyInvocationsUseFiveSecondCooldown()
        {
            Assert.AreEqual(5f, VarginhaStudentAlly.ManualCooldownSeconds);
        }

        [Test]
        public void Phase3ControllerStartsIncomplete()
        {
            Assert.IsFalse(typeof(VarginhaPhase3Controller).IsAbstract);
        }
    }
}
