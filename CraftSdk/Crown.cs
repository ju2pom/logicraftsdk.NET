using System;

using CraftSdk.Internal;

namespace CraftSdk
{
    public class Crown
    {
        internal Crown(CrownRootObject crown)
        {
            this.Id = crown.device_id;
            this.IsTouched = crown.touch_state == 1;
            this.Delta = crown.delta;
            this.RatchetDelta = crown.ratchet_delta;
            this.TimeStamp = crown.time_stamp;
            this.State = crown.state;
        }

        public int Id { get; }
        public bool IsTouched { get; }
        public int Delta { get; }
        public int RatchetDelta { get; }
        public Int64 TimeStamp { get; }
        public string State { get; }
    }
}
