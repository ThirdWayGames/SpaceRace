namespace Assets.Scripts.Components
{
    public class ObservableWinConditionComponent : MutableEventComponent
    {
        public float MutatableComponentValueCondition;

        public bool ConditionMet;

        protected override void TriggerEvent()
        {
            if (this.GetObservedValue() == MutatableComponentValueCondition)
            {
                ConditionMet = true;
            }
        }
    }
}