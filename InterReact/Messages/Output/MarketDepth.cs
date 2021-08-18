﻿namespace InterReact
{
    public sealed class MarketDepth : IHasRequestId
    {
        public int RequestId { get; }

        public int Position { get; } = -1; // stringify always

        public string MarketMaker { get; }

        public MarketDepthOperation Operation { get; } = MarketDepthOperation.Undefined;

        public MarketDepthSide Side { get; } = MarketDepthSide.Undefined;

        public double Price { get; }

        public int Size { get; }

        public bool IsSmartDepth { get; }

        internal MarketDepth(ResponseReader r, bool isLevel2)
        {
            r.IgnoreVersion();
            RequestId = r.ReadInt();
            Position = r.ReadInt();
            MarketMaker = isLevel2 ? r.ReadString() : string.Empty;
            Operation = r.ReadEnum<MarketDepthOperation>();
            Side = r.ReadEnum<MarketDepthSide>();
            Price = r.ReadDouble();
            Size = r.ReadInt();
        }
    }
}
