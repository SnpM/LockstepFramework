using UnityEngine;
using System.Collections;

namespace Lockstep
{
    public partial class Command
    {

        static void RegisterBackwards()
        {
            Register<PositionData>();
            Register<TargetData>();
            Register<FlagData>();
            Register<CoordinateData>();
            Register<CountData>();
        }

        public Vector2d Position
        {
            get { return this.GetData<PositionData>().position; }
            set { this.SetFirstData <PositionData>(new PositionData(value)); }
        }

        public bool HasPosition { get { return this.ContainsData<PositionData>(); } }

        class PositionData : ICommandData
        {
            public PositionData(Vector2d pos)
            {
                position = pos;
            }

            public Vector2d position;

            public void Write(Writer writer)
            {
                position.Write(writer);
            }

            public void Read(Reader reader)
            {
                position.Read(reader);
            }
        }

        public ushort Target
        {
            get { return (ushort)this.GetData<TargetData>().Value; }
            set { this.SetFirstData<TargetData>(new TargetData(value)); }
        }

        public bool HasTarget { get { return this.ContainsData<TargetData>(); } }

        class TargetData : DefaultData
        {
            public TargetData(ushort value) : base(DataType.UShort, (object)value)
            {
            }
        }

        public bool Flag
        {
            get { return (bool)this.GetData<FlagData>().Value; }
            set { this.SetFirstData<FlagData>(new FlagData(value)); }
        }

        public bool HasFlag { get { return this.ContainsData<FlagData>(); } }

        class FlagData : DefaultData
        {
            public FlagData(bool value) : base(DataType.Bool, value)
            {
            }
        }

        public Coordinate Coord
        {
            get { return this.GetData<CoordinateData>().coordinate; }
            set { this.SetFirstData<CoordinateData>(new CoordinateData(value)); }
        }

        public bool HasCoord { get { return this.ContainsData<CoordinateData>(); } }

        class CoordinateData : ICommandData
        {
            public CoordinateData(Coordinate coord)
            {
                this.coordinate = coord;
            }

            public Coordinate coordinate;

            public void Write(Writer writer)
            {
                coordinate.Write(writer);
            }

            public void Read(Reader reader)
            {
                coordinate.Read(reader);
            }
        }

        public int Count
        {
            get { return (int)this.GetData<CountData>().Value; }
            set { this.SetFirstData<CountData>(new CountData(value)); }
        }

        public bool HasCount { get { return this.ContainsData<CountData>(); } }

        class CountData : DefaultData
        {
            public CountData(int value) : base(DataType.Int, value)
            {
            }
        }
      
    }
}