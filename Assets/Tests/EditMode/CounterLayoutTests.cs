using System.Linq;
using NightBufe.Core;
using NightBufe.EditorTools;
using NUnit.Framework;

namespace NightBufe.Tests
{
    public class CounterLayoutTests
    {
        [Test]
        public void EveryIngredientHasAHotspot()
        {
            var factory = new InMemoryAssetFactory();
            try
            {
                var db = Night1DataBuilder.Build(factory);
                var slots = CounterLayoutDefaults.Slots();
                foreach (var ingredient in db.ingredients)
                    Assert.IsTrue(slots.Any(s => s.ingredientId == ingredient.id), $"'{ingredient.id}' için tıklanabilir alan yok");
            }
            finally
            {
                factory.DestroyAll();
            }
        }

        [Test]
        public void SlotIdsAreUnique()
        {
            var ids = CounterLayoutDefaults.Slots().Select(s => s.id).ToList();
            CollectionAssert.AllItemsAreUnique(ids);
        }

        [Test]
        public void InteractiveSlotsStayVisibleWhenCoverCrops3x2To16x9()
        {
            // Cover-fitting a 1536x1024 image to 16:9 crops ~80 px at top and bottom.
            float visibleHeight = 1536f * 9f / 16f;
            float crop = (1024f - visibleHeight) * 0.5f;
            foreach (var slot in CounterLayoutDefaults.Slots())
            {
                if (string.IsNullOrEmpty(slot.ingredientId) && slot.id != "radio" && slot.id != "rule_paper") continue;
                Assert.GreaterOrEqual(slot.pixelRect.yMin, crop, slot.id);
                Assert.LessOrEqual(slot.pixelRect.yMax, 1024f - crop, slot.id);
            }
        }

        [Test]
        public void IngredientHotspotsDoNotOverlap()
        {
            var hot = CounterLayoutDefaults.Slots().Where(s => !string.IsNullOrEmpty(s.ingredientId)).ToList();
            for (int i = 0; i < hot.Count; i++)
            for (int j = i + 1; j < hot.Count; j++)
                Assert.IsFalse(hot[i].pixelRect.Overlaps(hot[j].pixelRect), $"{hot[i].id} / {hot[j].id}");
        }
    }
}
