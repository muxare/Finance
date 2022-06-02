namespace Finance.Api.Domain
{
    public abstract class BounceState : StrategyState
    {
        public readonly static BounceState None = new N();
        public readonly static BounceState UpTrend = new U();
        public readonly static BounceState Reaction = new R();
        public readonly static BounceState Buy = new B();

        private BounceState() { }

        private class N : BounceState { }

        private class U : BounceState { }

        private class R : BounceState { }

        private class B : BounceState { }

    }
}
