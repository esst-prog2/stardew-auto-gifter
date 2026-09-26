using StardewModdingAPI;

namespace StardewAutoGifter.Onboarding
{
    /// <summary>
    /// Tracks whether the one-time welcome message/starter chest has already been given for this
    /// save. See specs/gift-selection - "Starter chest and welcome message on first farm exit".
    /// </summary>
    public class OnboardingStore
    {
        private const string SaveDataKey = "onboarding";

        private readonly IModHelper _helper;
        private OnboardingData _data = new();

        public OnboardingStore(IModHelper helper)
        {
            _helper = helper;
        }

        public bool HasShownWelcome => _data.HasShownWelcome;

        public void Load()
        {
            _data = _helper.Data.ReadSaveData<OnboardingData>(SaveDataKey) ?? new OnboardingData();
        }

        public void MarkWelcomeShown()
        {
            _data.HasShownWelcome = true;
            _helper.Data.WriteSaveData(SaveDataKey, _data);
        }
    }
}
