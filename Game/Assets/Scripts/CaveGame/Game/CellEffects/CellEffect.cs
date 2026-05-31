namespace CaveTogether.Game.CellEffects
{
    public readonly struct CellEffect
    {
        public readonly CellEffectTrigger Trigger;
        public readonly ICellEffect Effect;

        public CellEffect(CellEffectTrigger trigger, ICellEffect effect)
        {
            Trigger = trigger;
            Effect = effect;
        }
    }
}
