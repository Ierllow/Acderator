using Intense.Master;
using MasterMemory;
using MessagePack;
using System.Collections.Generic;
using System;

namespace Master.Tables
{
   public sealed partial class SoundSheetNameMasterTable : TableBase<SoundSheetNameMaster>
   {
        readonly Func<SoundSheetNameMaster, int> primaryIndexSelector;

        readonly SoundSheetNameMaster[] secondaryIndex0;
        readonly Func<SoundSheetNameMaster, int> secondaryIndex0Selector;

        public SoundSheetNameMasterTable(SoundSheetNameMaster[] sortedData)
            : base(sortedData)
        {
            this.primaryIndexSelector = x => x.Category;
            this.secondaryIndex0Selector = x => x.Id;
            this.secondaryIndex0 = CloneAndSortBy(this.secondaryIndex0Selector, System.Collections.Generic.Comparer<int>.Default);
        }

        public RangeView<SoundSheetNameMaster> SortById => new RangeView<SoundSheetNameMaster>(secondaryIndex0, 0, secondaryIndex0.Length - 1, true);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public SoundSheetNameMaster FindByCategory(int key)
        {
            var lo = 0;
            var hi = data.Length - 1;
            while (lo <= hi)
            {
                var mid = (int)(((uint)hi + (uint)lo) >> 1);
                var selected = data[mid].Category;
                var found = (selected < key) ? -1 : (selected > key) ? 1 : 0;
                if (found == 0) { return data[mid]; }
                if (found < 0) { lo = mid + 1; }
                else { hi = mid - 1; }
            }
            return default;
        }

        public SoundSheetNameMaster FindClosestByCategory(int key, bool selectLower = true)
        {
            return FindUniqueClosestCore(data, primaryIndexSelector, System.Collections.Generic.Comparer<int>.Default, key, selectLower);
        }

        public RangeView<SoundSheetNameMaster> FindRangeByCategory(int min, int max, bool ascendant = true)
        {
            return FindUniqueRangeCore(data, primaryIndexSelector, System.Collections.Generic.Comparer<int>.Default, min, max, ascendant);
        }

        public SoundSheetNameMaster FindById(int key)
        {
            return FindUniqueCoreInt(secondaryIndex0, secondaryIndex0Selector, System.Collections.Generic.Comparer<int>.Default, key);
        }

        public SoundSheetNameMaster FindClosestById(int key, bool selectLower = true)
        {
            return FindUniqueClosestCore(secondaryIndex0, secondaryIndex0Selector, System.Collections.Generic.Comparer<int>.Default, key, selectLower);
        }

        public RangeView<SoundSheetNameMaster> FindRangeById(int min, int max, bool ascendant = true)
        {
            return FindUniqueRangeCore(secondaryIndex0, secondaryIndex0Selector, System.Collections.Generic.Comparer<int>.Default, min, max, ascendant);
        }

    }
}