namespace TechUI.Mvc.Wizard
{
    public sealed class WizardEventBuilder
    {
        private readonly WizardEventOptions _options;

        internal WizardEventBuilder(WizardEventOptions options)
        {
            _options = options;
        }

        public WizardEventBuilder Change(string handler)
        {
            _options.Change = handler;
            return this;
        }

        public WizardEventBuilder Complete(string handler)
        {
            _options.Complete = handler;
            return this;
        }

        public WizardEventBuilder Finish(string handler)
        {
            _options.Finish = handler;
            return this;
        }
    }
}
