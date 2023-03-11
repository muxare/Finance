using Finance.Api.Domain;

namespace Finance.Domain.Entities
{
    public abstract class BounceState
    {
        public static readonly BounceState None = new N();
        public static readonly BounceState UpTrend = new U();
        public static readonly BounceState Reaction = new R();
        public static readonly BounceState Buy = new B();

        private BounceState()
        { }

        private class N : BounceState
        { }

        private class U : BounceState
        { }

        private class R : BounceState
        { }

        private class B : BounceState
        { }
    }
}
