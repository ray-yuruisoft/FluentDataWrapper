using System.Collections;

namespace FluentDataWrapper
{
    internal interface IQueryTypeHandler
    {
        bool IterateDataReader { get; }
        void HandleType(IDictionary dictionary, DataReader reader, dynamic key, dynamic value);
    }

}
