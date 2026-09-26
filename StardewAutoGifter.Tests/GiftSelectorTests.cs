using System.Collections.Generic;
using StardewAutoGifter.Core;
using Xunit;

namespace StardewAutoGifter.Tests
{
    /// <summary>A simple in-memory taste table, standing in for the game's NPC gift-taste lookup.</summary>
    public class FakeTasteProvider : IGiftTasteProvider
    {
        private readonly Dictionary<(string npc, string item), GiftTaste> _tastes = new();

        public FakeTasteProvider Set(string npc, string item, GiftTaste taste)
        {
            _tastes[(npc, item)] = taste;
            return this;
        }

        public GiftTaste GetTaste(string npcName, string qualifiedItemId)
            => _tastes.TryGetValue((npcName, qualifiedItemId), out GiftTaste taste) ? taste : GiftTaste.Neutral;
    }

    public class GiftSelectorTests
    {
        private const string Npc = "Abigail";

        [Fact]
        public void AnchorOnlyPoolWinsOverALargerQuantityAvailableOnlyInTheWiderFarmPool()
        {
            // specs/gift-selection - "Anchor chest prioritized before the wider farm pool": mirrors
            // how PlanComputer calls SelectItem against the anchor-only pool first, falling back to
            // the farm-wide pool only on a miss (`??`).
            var tastes = new FakeTasteProvider()
                .Set(Npc, "(O)Grape", GiftTaste.Love)
                .Set(Npc, "(O)Strawberry", GiftTaste.Love);

            var anchorOnlyPool = new Dictionary<string, int> { ["(O)Grape"] = 2 };
            var farmWidePool = new Dictionary<string, int> { ["(O)Grape"] = 2, ["(O)Strawberry"] = 50 };
            var blocklist = new HashSet<string>();

            string? selected = GiftSelector.SelectItem(Npc, anchorOnlyPool, blocklist, includeLikedItems: true, tastes)
                ?? GiftSelector.SelectItem(Npc, farmWidePool, blocklist, includeLikedItems: true, tastes);

            Assert.Equal("(O)Grape", selected);
        }

        [Fact]
        public void FallsBackToFarmWidePoolWhenAnchorAloneHasNothingEligible()
        {
            var tastes = new FakeTasteProvider().Set(Npc, "(O)Strawberry", GiftTaste.Love);

            var anchorOnlyPool = new Dictionary<string, int>(); // anchor chest is empty
            var farmWidePool = new Dictionary<string, int> { ["(O)Strawberry"] = 5 };
            var blocklist = new HashSet<string>();

            string? selected = GiftSelector.SelectItem(Npc, anchorOnlyPool, blocklist, includeLikedItems: true, tastes)
                ?? GiftSelector.SelectItem(Npc, farmWidePool, blocklist, includeLikedItems: true, tastes);

            Assert.Equal("(O)Strawberry", selected);
        }

        [Fact]
        public void PicksLovedItemWithLargestQuantity()
        {
            var tastes = new FakeTasteProvider()
                .Set(Npc, "(O)Grape", GiftTaste.Love)
                .Set(Npc, "(O)Strawberry", GiftTaste.Love);

            var pool = new Dictionary<string, int> { ["(O)Grape"] = 2, ["(O)Strawberry"] = 8 };

            string? selected = GiftSelector.SelectItem(Npc, pool, new HashSet<string>(), includeLikedItems: true, tastes);

            Assert.Equal("(O)Strawberry", selected);
        }

        [Fact]
        public void SummedQuantityAcrossChestsOutranksALargerSingleChestStackOfAnotherItem()
        {
            // Represents specs/gift-selection "Same loved item split across multiple chests":
            // 12 total (5+7, already summed by FarmItemPool before reaching GiftSelector) beats 10.
            var tastes = new FakeTasteProvider()
                .Set(Npc, "(O)SplitAcrossChests", GiftTaste.Love)
                .Set(Npc, "(O)SingleChestStack", GiftTaste.Love);

            var pool = new Dictionary<string, int> { ["(O)SplitAcrossChests"] = 5 + 7, ["(O)SingleChestStack"] = 10 };

            string? selected = GiftSelector.SelectItem(Npc, pool, new HashSet<string>(), includeLikedItems: true, tastes);

            Assert.Equal("(O)SplitAcrossChests", selected);
        }

        [Fact]
        public void SingleRemainingUnitIsTreatedAsAbsentAndSkippedInFavorOfNextBestItem()
        {
            // specs/gift-selection - "Minimum one unit reserved per item": exactly 1 unit must not
            // be selected, even if it would otherwise be the best (or only) loved candidate.
            var tastes = new FakeTasteProvider()
                .Set(Npc, "(O)LastGrape", GiftTaste.Love)
                .Set(Npc, "(O)Strawberry", GiftTaste.Love);

            var pool = new Dictionary<string, int> { ["(O)LastGrape"] = 1, ["(O)Strawberry"] = 3 };

            string? selected = GiftSelector.SelectItem(Npc, pool, new HashSet<string>(), includeLikedItems: true, tastes);

            Assert.Equal("(O)Strawberry", selected);
        }

        [Fact]
        public void ReturnsNullWhenTheOnlyEligibleItemHasJustOneUnit()
        {
            var tastes = new FakeTasteProvider().Set(Npc, "(O)LastGrape", GiftTaste.Love);
            var pool = new Dictionary<string, int> { ["(O)LastGrape"] = 1 };

            string? selected = GiftSelector.SelectItem(Npc, pool, new HashSet<string>(), includeLikedItems: true, tastes);

            Assert.Null(selected);
        }

        [Fact]
        public void BlocklistedFavoriteIsSkippedInFavorOfNextBestLovedItem()
        {
            var tastes = new FakeTasteProvider()
                .Set(Npc, "(O)PrismaticShard", GiftTaste.Love)
                .Set(Npc, "(O)Grape", GiftTaste.Love);

            var pool = new Dictionary<string, int> { ["(O)PrismaticShard"] = 99, ["(O)Grape"] = 3 };
            var blocklist = new HashSet<string> { "(O)PrismaticShard" };

            string? selected = GiftSelector.SelectItem(Npc, pool, blocklist, includeLikedItems: true, tastes);

            Assert.Equal("(O)Grape", selected);
        }

        [Fact]
        public void FallsBackToLikedItemWhenNoEligibleLovedItemExistsAndFallbackEnabled()
        {
            var tastes = new FakeTasteProvider().Set(Npc, "(O)Milk", GiftTaste.Like);
            var pool = new Dictionary<string, int> { ["(O)Milk"] = 4 };

            string? selected = GiftSelector.SelectItem(Npc, pool, new HashSet<string>(), includeLikedItems: true, tastes);

            Assert.Equal("(O)Milk", selected);
        }

        [Fact]
        public void DoesNotFallBackToLikedItemWhenSettingDisabled()
        {
            var tastes = new FakeTasteProvider().Set(Npc, "(O)Milk", GiftTaste.Like);
            var pool = new Dictionary<string, int> { ["(O)Milk"] = 4 };

            string? selected = GiftSelector.SelectItem(Npc, pool, new HashSet<string>(), includeLikedItems: false, tastes);

            Assert.Null(selected);
        }

        [Fact]
        public void ReturnsNullWhenNeitherTierHasAnEligibleItem()
        {
            var tastes = new FakeTasteProvider().Set(Npc, "(O)Rock", GiftTaste.Neutral);
            var pool = new Dictionary<string, int> { ["(O)Rock"] = 4 };

            string? selected = GiftSelector.SelectItem(Npc, pool, new HashSet<string>(), includeLikedItems: true, tastes);

            Assert.Null(selected);
        }

        [Fact]
        public void BlocklistedItemIsNeverSelectedEvenAsOnlyCandidate()
        {
            var tastes = new FakeTasteProvider().Set(Npc, "(O)Grape", GiftTaste.Love);
            var pool = new Dictionary<string, int> { ["(O)Grape"] = 10 };
            var blocklist = new HashSet<string> { "(O)Grape" };

            string? selected = GiftSelector.SelectItem(Npc, pool, blocklist, includeLikedItems: true, tastes);

            Assert.Null(selected);
        }
    }
}
