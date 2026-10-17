using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewAutoGifter.Blocklist;
using StardewAutoGifter.Core;
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
        private PlanStore _planStore = null!;

        /// <summary>The plan being delivered today (its target day is today). See specs/proximity-delivery - "Plan delivered only on its target day".</summary>
        private DailyPlan? _todaysPlan;

        /// <summary>The plan for tomorrow, shown and edited in the menu. See specs/daily-plan-review - "Edited plan carried into the next day".</summary>
        private DailyPlan? _nextPlan;

        /// <summary>Throttles the anchor-chest sparkle to roughly every 4 seconds (see AnchorChestSparkleService).</summary>
        private int _secondsSinceLastSparkle;
        private const int SparkleIntervalSeconds = 4;

        /// <summary>Whether more than one chest was marked at the last check, so the conflict HUD message is shown only when a conflict starts.</summary>
        private bool _wasMarkingConflict;

        public override void Entry(IModHelper helper)
        {
            _config = Helper.ReadConfig<ModConfig>();
            _blocklistStore = new BlocklistStore(Helper);
            _configuredNpcStore = new ConfiguredNpcStore(Helper);
            _onboardingStore = new OnboardingStore(Helper);
            _settingsStore = new ModSettingsStore(Helper);
            _tasteProvider = new NpcGiftTasteProvider();
            _planStore = new PlanStore(Helper);

            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.DayEnding += OnDayEnding;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;
            helper.Events.Player.Warped += OnWarped;
            helper.Events.Display.MenuChanged += OnMenuChanged; // hw4-spike diagnostic, kept on purpose - see OnMenuChanged
        }

        /// <summary>
        /// Diagnostic kept on purpose after the hw4-spike (see spike/sentinel-color-evidence.md,
        /// which cites this handler); remove in a later change if the log noise becomes a problem.
        /// </summary>
        private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is ItemGrabMenu grabMenu && grabMenu.context is Chest chest)
            {
                Monitor.Log($"[hw4-spike] Chest menu opened at {chest.TileLocation}; color before: {chest.playerChoiceColor.Value}.", LogLevel.Debug);
            }
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            _blocklistStore.Load();
            _configuredNpcStore.Load();
            _onboardingStore.Load();
            _settingsStore.Load();
            _todaysPlan = null;
            _wasMarkingConflict = false;
            _nextPlan = _planStore.Load(); // the plan stored as the previous day ended
            PromotePlanForToday();
        }

        /// <summary>Today, as SMAPI's days-since-start count (the day numbering used by <see cref="PlanTiming"/>).</summary>
        private static int Today => SDate.Now().DaysSinceStart;

        private void OnDayStarted(object? sender, DayStartedEventArgs e) => PromotePlanForToday();

        /// <summary>
        /// specs/daily-plan-review - "Edited plan carried into the next day": the next-day plan
        /// becomes today's plan only if it is for today, and a plan for any other day is dropped.
        /// Safe to call more than once a day (it runs both after loading and when a day starts).
        /// </summary>
        private void PromotePlanForToday()
        {
            if (_nextPlan != null && PlanTiming.IsPlanForToday(_nextPlan.TargetDay, Today))
            {
                _todaysPlan = _nextPlan;
                _nextPlan = null;
            }

            if (_todaysPlan != null && !PlanTiming.IsPlanForToday(_todaysPlan.TargetDay, Today))
                _todaysPlan = null;
        }

        private void OnDayEnding(object? sender, DayEndingEventArgs e)
        {
            // specs/daily-plan-review - "Next day's plan computed automatically at the end of the day".
            if (PlanTiming.ShouldComputeAtDayEnding(_nextPlan?.TargetDay, _nextPlan?.IsActive ?? false, Today))
            {
                _nextPlan = ComputeNextPlan(out _, out _);
                Monitor.Log($"No active plan for tomorrow; computed one automatically as the day ended (target day {_nextPlan.TargetDay}).", LogLevel.Debug);
            }

            _planStore.Save(_nextPlan);
        }

        /// <summary>Computes the plan for tomorrow from the current pools, blocklist, and settings.</summary>
        private DailyPlan ComputeNextPlan(out bool isAmbiguous, out int markedChestCount)
        {
            int targetDay = PlanTiming.NextTargetDay(Today);
            Chest? anchor = FarmChestScanner.FindAnchorChest(out isAmbiguous);
            markedChestCount = FarmChestScanner.GetMarkedChests().Count;

            if (anchor == null || isAmbiguous)
            {
                // No single valid anchor chest: mod is inactive (specs/gift-selection -
                // "Marked chest acts as the mod's on/off anchor" and "Ambiguous marking treated as unresolved").
                return new DailyPlan { IsActive = false, TargetDay = targetDay };
            }

            var farmChests = System.Linq.Enumerable.ToList(FarmChestScanner.GetFarmChests());
            var anchorOnlyPool = FarmItemPool.BuildPool(new[] { anchor });
            var farmWidePool = FarmItemPool.BuildPool(farmChests);
            var blocklist = new System.Collections.Generic.HashSet<string>(_blocklistStore.BlockedItemIds);
            var configuredNpcNames = new System.Collections.Generic.HashSet<string>(_configuredNpcStore.SelectedNames);
            return PlanComputer.Compute(configuredNpcNames, anchorOnlyPool, farmWidePool, blocklist, _config.IncludeLikedItems, _settingsStore.AnchorChestOnly, _tasteProvider, targetDay, Today);
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
            // Key names come from config.json, so a remapped key is named correctly (HW6 review 02d).
            Game1.drawObjectDialogue(WelcomeText.Pages(_config.MenuKey.ToString(), _config.ChestMarkKey.ToString(), _settingsStore.AnchorChestOnly));

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

            // specs/daily-plan-review - "Plan computed for the next day when the menu is opened":
            // only tomorrow's plan is recomputed (today's plan is left alone), and rows the player
            // toggled off stay off.
            DailyPlan plan = ComputeNextPlan(out bool isAmbiguous, out int markedChestCount);
            CheckMarkingConflict(isAmbiguous);
            if (_nextPlan != null)
                PlanTiming.CarryExclusions(_nextPlan, plan);

            _nextPlan = plan;

            var villagers = VillagerCatalog.BuildVillagerList();
            var catalog = ItemCatalog.BuildFullCatalog();
            Game1.activeClickableMenu = new ModMainMenu(plan, _blocklistStore, _configuredNpcStore, _settingsStore, villagers, catalog, markedChestCount, _config.ChestMarkKey.ToString());
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

            FarmChestScanner.FindAnchorChest(out bool isAmbiguous);
            CheckMarkingConflict(isAmbiguous);
        }

        /// <summary>
        /// specs/daily-plan-review - "Warnings and info panel": shows the conflict HUD message once,
        /// when a marking conflict starts; called when the chest-marking key is pressed, when the
        /// menu opens, and on the periodic sparkle check.
        /// </summary>
        private void CheckMarkingConflict(bool isConflict)
        {
            if (MarkingConflict.ShouldNotify(_wasMarkingConflict, isConflict))
                Game1.addHUDMessage(new HUDMessage(MarkingConflict.HudMessage(_config.ChestMarkKey.ToString()), HUDMessage.error_type));

            _wasMarkingConflict = isConflict;
        }

        private void OnOneSecondUpdateTicked(object? sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            // specs/proximity-delivery - "Plan delivered only on its target day".
            if (_todaysPlan != null && PlanTiming.IsPlanForToday(_todaysPlan.TargetDay, Today))
                ProximityDeliveryService.CheckAndDeliver(_todaysPlan, Today, Monitor);

            TrySparkleAnchorChest();
        }

        private void TrySparkleAnchorChest()
        {
            if (++_secondsSinceLastSparkle < SparkleIntervalSeconds)
                return;

            _secondsSinceLastSparkle = 0;

            // specs/gift-selection - "Anchor chest sparkles periodically for visibility": only while active (a single, unambiguous anchor).
            Chest? anchor = FarmChestScanner.FindAnchorChest(out bool isAmbiguous);
            CheckMarkingConflict(isAmbiguous);
            if (anchor != null && !isAmbiguous)
                AnchorChestSparkleService.SparkleIfInView(anchor);
        }
    }
}
