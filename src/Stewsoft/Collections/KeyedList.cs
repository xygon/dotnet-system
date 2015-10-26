using System;

namespace Stewsoft.Collections
{
    public class KeyedList<TKey, TItem> : System.Collections.ObjectModel.KeyedCollection<TKey, TItem>
    {
        public KeyedList(Func<TItem,TKey> getKeyForItem) : base()
        {
            if (getKeyForItem == null) throw new ArgumentNullException("getKeyForItem");

            _getKeyForItem = getKeyForItem;
        }

        private readonly Func<TItem, TKey> _getKeyForItem;

        protected override TKey GetKeyForItem(TItem item)
        {
            return _getKeyForItem(item);
        }
    }
}
