using System;
using UnityEngine;

namespace IA.FSM
{
    public class AndCondition : Condition
    {
        private Condition _a;
        private Condition _b;

        public override bool Test()
        {
            return _a.Test() && _b.Test();
        }

        public AndCondition(Condition a, Condition b)
        {
            _a = a ?? throw new ArgumentNullException(nameof(a));
            _b = b ?? throw new ArgumentNullException(nameof(b));
        }
    }

    public class OrCondition : Condition
    {
        private Condition _a;
        private Condition _b;

        public override bool Test()
        {
            return _a.Test() || _b.Test();
        }

        public OrCondition(Condition a, Condition b)
        {
            _a = a ?? throw new ArgumentNullException(nameof(a));
            _b = b ?? throw new ArgumentNullException(nameof(b));
        }
    }

    public class NotCondition : Condition
    {
        private Condition _a;

        public override bool Test()
        {
            return !_a.Test();
        }

        public NotCondition(Condition a)
        {
            _a = a ?? throw new ArgumentNullException(nameof(a));
        }
    }
}
