using FluentData;
using System.Collections;

namespace FluentDataWrapper
{
    internal class QueryDicHandler : IQueryTypeHandler
    {
        private readonly DbCommandData _data;

        public bool IterateDataReader { get { return true; } }

        public QueryDicHandler(DbCommandData data)
        {
            _data = data;
        }

        public void HandleType(IDictionary dictionary, DataReader reader, dynamic key, dynamic value)
        {

            var a = reader.GetValue(key);
            var b = reader.GetValue(value);
            if (!dictionary.Contains(a))
            {
                dictionary.Add(a, b);
            }

        }
    }

}
