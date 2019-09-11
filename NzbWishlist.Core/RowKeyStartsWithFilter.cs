using Microsoft.WindowsAzure.Storage.Table;

namespace NzbWishlist.Core
{
    class RowKeyStartsWithFilter
    {
        private readonly string _partitionKey;
        private readonly string _rowKeyStartsWith;

        public RowKeyStartsWithFilter(string partitionKey, string rowKeyStartsWith)
        {
            _partitionKey = partitionKey;
            _rowKeyStartsWith = rowKeyStartsWith;
        }

        public override string ToString()
        {
            var len = _rowKeyStartsWith.Length - 1;
            var lastChar = _rowKeyStartsWith[len];
            var nextLastChar = (char)(lastChar + 1);
            var endsWith = _rowKeyStartsWith.Substring(0, len) + nextLastChar;

            var filter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, _partitionKey),
                TableOperators.And,
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, _rowKeyStartsWith),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, endsWith))
            );

            return filter;
        }
    }
}
