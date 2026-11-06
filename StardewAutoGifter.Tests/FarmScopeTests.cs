using StardewAutoGifter.Core;
using Xunit;

namespace StardewAutoGifter.Tests
{
    /// <summary>
    /// Expected values taken from the spec (HW7b, reviews/10-in-game-results.md, D3):
    /// specs/gift-selection - "Farm-wide item pool" (scenarios "Greenhouse chests are counted",
    /// "Farmhouse and cellar chests are ignored", "A farm building without an interior adds nothing").
    /// </summary>
    public class FarmScopeTests
    {
        // "Greenhouse chests are counted": a building interior that is not the farmhouse is in scope.
        [Fact]
        public void GreenhouseInterior_IsInScope()
        {
            Assert.True(FarmScope.IncludesBuildingInterior(hasInterior: true, isFarmHouse: false));
        }

        // "Farmhouse and cellar chests are ignored": the farmhouse's interior is out of scope.
        [Fact]
        public void FarmHouseInterior_IsOutOfScope()
        {
            Assert.False(FarmScope.IncludesBuildingInterior(hasInterior: true, isFarmHouse: true));
        }

        // "A farm building without an interior adds nothing".
        [Fact]
        public void BuildingWithoutInterior_AddsNothing()
        {
            Assert.False(FarmScope.IncludesBuildingInterior(hasInterior: false, isFarmHouse: false));
        }
    }
}
