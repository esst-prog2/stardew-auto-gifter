using StardewModdingAPI;

namespace StardewAutoGifter.Onboarding
{
    /// <summary>
    /// Tracks whether the one-time welcome message/starter chest has already been given for this
    /// save. See specs/gift-selection - "Starter chest and welcome message on first farm exit".
    /// </summary>
    public class OnboardingStore
    {
        private readonly SaveDataStore<OnboardingData> _store;
        private OnboardingData _data = new();

        public OnboardingStore(IModHelper helper)
        {
            _store = new SaveDataStore<OnboardingData>(helper, "onboarding");
        }

        public bool HasShownWelcome => _data.HasShownWelcome;

        public void Load()
        {
            _data = _store.Read() ?? new OnboardingData();
        }

        public void MarkWelcomeShown()
        {
            _data.HasShownWelcome = true;
            _store.Write(_data);
        }
    }
}
