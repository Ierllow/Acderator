using Intense.Master;
using MasterMemory;
using MessagePack;
using System.Collections.Generic;
using System;

namespace Master.Tables
{
   public sealed partial class SongScoreRateMasterTable : TableBase<SongScoreRateMaster>
   {
        readonly Func<SongScoreRateMaster, int> primaryIndexSelector;


        public SongScoreRateMasterTable(SongScoreRateMaster[] sortedData)
            : base(sortedData)
        {
            this.primaryIndexSelector = x => x.Type;
        }


        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public SongScoreRateMaster FindByType(int key)
        {
            var lo = 0;
            var hi = data.Length - 1;
            while (lo <= hi)
            {
                var mid = (int)(((uint)hi + (uint)lo) >> 1);
                var selected = data[mid].Type;
                var found = (selected < key) ? -1 : (selected > key) ? 1 : 0;
                if (found == 0) { return data[mid]; }
                if (found < 0) { lo = mid + 1; }
                else { hi = mid - 1; }
            }
            return default;
        }

        public SongScoreRateMaster FindClosestByType(int key, bool selectLower = true)
        {
            return FindUniqueClosestCore(data, primaryIndexSelector, System.Collections.Generic.Comparer<int>.Default, key, selectLower);
        }

        public RangeView<SongScoreRateMaster> FindRangeByType(int min, int max, bool ascendant = true)
        {
            return FindUniqueRangeCore(data, primaryIndexSelector, System.Collections.Generic.Comparer<int>.Default, min, max, ascendant);
        }

    }
}