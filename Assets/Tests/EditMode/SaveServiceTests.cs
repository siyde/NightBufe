using System.IO;
using NightBufe.Core;
using NUnit.Framework;

namespace NightBufe.Tests
{
    public class SaveServiceTests
    {
        private string _dir;

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), "GeceBufesiTests_" + System.Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
        }

        [Test]
        public void RoundTrip_KeepsEverything()
        {
            var saves = new SaveService(_dir);
            var state = new GameState
            {
                night = 3, nightProgress = 0.25f, money = 420,
                agreement = 2, curiosity = 3, reality = 4, childBond = 1,
                playerName = "Şükrü Çağdaş"
            };
            state.AddDiaryPage(1);
            state.AddDiaryPage(2);
            state.usedChoices.Add("n1_sign_name");

            saves.Save(state);
            var loaded = saves.Load();

            Assert.IsNotNull(loaded);
            Assert.AreEqual(3, loaded.night);
            Assert.AreEqual(0.25f, loaded.nightProgress, 1e-5f);
            Assert.AreEqual(420, loaded.money);
            Assert.AreEqual(2, loaded.agreement);
            Assert.AreEqual(3, loaded.curiosity);
            Assert.AreEqual(4, loaded.reality);
            Assert.AreEqual(1, loaded.childBond);
            Assert.AreEqual("Şükrü Çağdaş", loaded.playerName);
            CollectionAssert.AreEqual(new[] { 1, 2 }, loaded.diaryPagesFound);
            CollectionAssert.AreEqual(new[] { "n1_sign_name" }, loaded.usedChoices);
        }

        [Test]
        public void Load_NoFile_ReturnsNull()
        {
            Assert.IsNull(new SaveService(_dir).Load());
        }

        [Test]
        public void Load_CorruptFile_ReturnsNull()
        {
            Directory.CreateDirectory(_dir);
            var saves = new SaveService(_dir);
            File.WriteAllText(saves.FilePath, "{ bu json değil");
            Assert.IsNull(saves.Load());
        }

        [Test]
        public void Delete_RemovesSave()
        {
            var saves = new SaveService(_dir);
            saves.Save(new GameState());
            Assert.IsTrue(saves.HasSave);
            saves.Delete();
            Assert.IsFalse(saves.HasSave);
        }

        [Test]
        public void AdvanceNight_ResetsProgress()
        {
            var state = new GameState { night = 1, nightProgress = 1f };
            state.AdvanceNight();
            Assert.AreEqual(2, state.night);
            Assert.AreEqual(0f, state.nightProgress);
        }
    }
}
