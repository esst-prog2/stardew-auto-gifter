using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewAutoGifter.Blocklist;
using StardewAutoGifter.Delivery;
using StardewAutoGifter.GiftSelection;
using StardewAutoGifter.Menus;
using StardewAutoGifter.Npcs;
using StardewAutoGifter.Onboarding;
using StardewAutoGifter.Planning;
using StardewAutoGifter.Settings;
using StardewAutoGifter.Visuals;

namespace StardewAutoGifter
{
    public class ModEntry : Mod
    {
        private ModConfig _config = null!;
        private BlocklistStore _blocklistStore = null!;
        private ConfiguredNpcStore _configuredNpcStore = null!;
        private OnboardingStore _onboardingStore = null!;
        private ModSettingsStore _settingsStore = null!;
        private NpcGiftTasteProvider _tasteProvider = null!;
        private DailyPlan? _currentPlan;

        /// <summary>Throttles the anchor-chest sparkle to roughly every 4 seconds (see AnchorChestSparkleService).</summary>
        private int _secondsSinceLastSparkle;
        private const int SparkleIntervalSeconds = 4;

        public override void Entry(IModHelper helper)
        {
            _config = Helper.ReadConfig<ModConfig>();
            _blocklistStore = new BlocklistStore(Helper);
            _configuredNpcStore = new ConfiguredNpcStore(Helper);
            _onboardingStore = new OnboardingStore(Helper);
            _settingsStore = new ModSettingsStore(Helper);
            _tasteProvider = new NpcGiftTasteProvider();

            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;
            helper.Events.Player.Warped += OnWarped;
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            _blocklistStore.Load();
            _configuredNpcStore.Load();
            _onboardingStore.Load();
            _settingsStore.Load();
            _currentPlan = null;
        }

        private void OnWarped(object? sender, WarpedEventArgs e)
        {
            // specs/gift-selection - "Starter chest and welcome message on first farm exit".
            if (_onboardingStore.HasShownWelcome)
                return;

            if (e.OldLocation is not FarmHouse || e.NewLocation is not Farm)
                return;

            Game1.player.addItemToInventory(ItemRegistry.Create("(BC)130")); // a placeable Chest

            // A single long drawObjectDialogue(string) call overflows the dialogue box (confirmed
            // in-game), so this uses the paged overload instead - one short message per page, with
            // the game's own next-page arrow, like a normal conversation.
            Game1.drawObjectDialogue(new System.Collections.Generic.List<string>
            {
                "Welcome to Auto-Gifter!",
                "A chest has been added to your inventory. Place it anywhere on your farm (or inside the barn, coop, shed, or greenhouse) - not in the farmhouse or cellar.",
                "Open the chest and press G to mark it as your gift chest - it will be renamed and sparkle occasionally so you can spot it.",
                "While marked, the mod gathers gift items from every chest on your farm, not just that one.",
                "Press F8 anytime to open the mod menu: choose which villagers to gift, preview and edit tomorrow's plan, and manage your blocklist.",
                "IMPORTANT: automatic gifting always starts the NEXT day, never the same day.",
                "Selecting a villager or editing the plan today has no effect on today - check the \"Tomorrow's Plan\" label in the Plan tab to see what's actually queued up.",
            });

            _onboardingStore.MarkWelcomeShown();
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (e.Button == _config.MenuKey)
            {
                OpenModMenu();
                return;
            }

            if (e.Button == _config.ChestMarkKey)
                TryToggleChestMark();
        }

        private void OpenModMenu()
        {
            if (Game1.activeClickableMenu != null)
                return;

            Chest? anchor = FarmChestScanner.FindAnchorChest(out bool isAmbiguous);
            bool isActive = anchor != null && !isAmbiguous;

            DailyPlan plan;
            if (isActive)
            {
                var farmChests = System.Linq.Enumerable.ToList(FarmChestScanner.GetFarmChests());
                var anchorOnlyPool = FarmItemPool.BuildPool(new[] { anchor! });
                var farmWidePool = FarmItemPool.BuildPool(farmChests);
                var blocklist = new System.Collections.Generic.HashSet<string>(_blocklistStore.BlockedItemIds);
                var configuredNpcNames = new System.Collections.Generic.HashSet<string>(_configuredNpcStore.SelectedNames);
                plan = PlanComputer.Compute(configuredNpcNames, anchorOnlyPool, farmWidePool, blocklist, _config.IncludeLikedItems, _settingsStore.AnchorChestOnly, _tasteProvider);
            }
            else
            {
                // No single valid anchor chest: mod is inactive (specs/gift-selection -
                // "Marked chest acts as the mod's on/off anchor" and "Ambiguous marking treated as unresolved").
                plan = new DailyPlan { IsActive = false };
            }

            _currentPlan = plan;

            var villagers = VillagerCatalog.BuildVillagerList();
            var catalog = ItemCatalog.BuildFullCatalog();
            Game1.activeClickableMenu = new ModMainMenu(plan, _blocklistStore, _configuredNpcStore, _settingsStore, villagers, catalog, isAmbiguous);
        }

        private void TryToggleChestMark()
        {
            if (Game1.activeClickableMenu is not ItemGrabMenu grabMenu)
            {
                Monitor.Log($"Chest-mark key pressed, but the active menu is {Game1.activeClickableMenu?.GetType().FullName ?? "null"} (expected ItemGrabMenu).", LogLevel.Debug);
                return;
            }

            if (grabMenu.context is not Chest chest)
            {
                Monitor.Log($"Chest-mark key pressed, but ItemGrabMenu.context is {grabMenu.context?.GetType().FullName ?? "null"} (expected a Chest).", LogLevel.Debug);
                return;
            }

            GameLocation? location = Game1.currentLocation;
            bool wasInFarmScope = location != null && FarmChestScanner.IsInFarmScope(location);
            Monitor.Log($"Chest-mark key pressed on a Chest at {chest.TileLocation} in {location?.Name ?? "null"}; in farm scope: {wasInFarmScope}; color before: {chest.playerChoiceColor.Value}.", LogLevel.Debug);

            if (location == null)
                return;

            FarmChestScanner.ToggleMark(chest, location);

            Monitor.Log($"Color after toggle: {chest.playerChoiceColor.Value}.", LogLevel.Debug);
        }

        private void OnOneSecondUpdateTicked(object? sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (_currentPlan != null)
                ProximityDeliveryService.CheckAndDeliver(_currentPlan, Monitor);

            TrySparkleAnchorChest();
        }

        private void TrySparkleAnchorChest()
        {
            if (++_secondsSinceLastSparkle < SparkleIntervalSeconds)
                return;

            _secondsSinceLastSparkle = 0;

            // specs/gift-selection - "Anchor chest sparkles periodically for visibility": only while active (a single, unambiguous anchor).
            Chest? anchor = FarmChestScanner.FindAnchorChest(out bool isAmbiguous);
            if (anchor != null && !isAmbiguous)
                AnchorChestSparkleService.SparkleIfInView(anchor);
        }
    }
}
