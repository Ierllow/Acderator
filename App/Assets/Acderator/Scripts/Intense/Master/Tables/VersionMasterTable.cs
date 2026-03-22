using Intense.Master;
using MasterMemory;
using MessagePack;
using System.Collections.Generic;
using System;

namespace Master.Tables
{
   public sealed partial class VersionMasterTable : TableBase<VersionMaster>
   {
        readonly Func<VersionMaster, int> primaryIndexSelector;


        public VersionMasterTable(VersionMaster[] sortedData)
            : base(sortedData)
        {
            this.primaryIndexSelector = x => x.Version;
        }


        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public VersionMaster FindByVersion(int key)
        {
            var lo = 0;
            var hi = data.Length - 1;
            while (lo <= hi)
            {
                var mid = (int)(((uint)hi + (uint)lo) >> 1);
                var selected = data[mid].Version;
                var found = (selected < key) ? -1 : (selected > key) ? 1 : 0;
                if (found == 0) { return data[mid]; }
                if (found < 0) { lo = mid + 1; }
                else { hi = mid - 1; }
            }
            return default;
        }

        public VersionMaster FindClosestByVersion(int key, bool selectLower = true)
        {
            return FindUniqueClosestCore(data, primaryIndexSelector, System.Collections.Generic.Comparer<int>.Default, key, selectLower);
        }

        public RangeView<VersionMaster> FindRangeByVersion(int min, int max, bool ascendant = true)
        {
            return FindUniqueRangeCore(data, primaryIndexSelector, System.Collections.Generic.Comparer<int>.Default, min, max, ascendant);
        }

    }
}