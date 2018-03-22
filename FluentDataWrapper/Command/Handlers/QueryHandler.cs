using FluentData;
using System;
using System.Collections;
using System.Data;

namespace FluentDataWrapper
{
    internal class QueryHandler<TEntity>
    {
        private readonly DbCommandData _data;
        private readonly IQueryTypeHandler _typeHandler;

        public QueryHandler(DbCommandData data)
        {
            _data = data;
            _typeHandler = new QueryDicHandler(data);
        }

        internal IDictionary ExecuteMany(dynamic key, dynamic value)
        {
            var items = (IDictionary)_data.Context.Data.EntityFactory.Create(typeof(TEntity));          
            PrepareDbCommand();

            var reader = new DataReader(_data.InnerCommand.ExecuteReader());
            if (_typeHandler.IterateDataReader)
            {
                while (reader.Read())
                {
                    _typeHandler.HandleType(items, reader, key, value);
                }
            }
            else
            {
                _typeHandler.HandleType(items, reader, key, value);
            }

            return items;
        }

        private bool _queryAlreadyExecuted;

        private void PrepareDbCommand()
        {
            if (_queryAlreadyExecuted)
            {
                if (_data.UseMultipleResultsets)
                    _data.Reader.NextResult();
                else
                    throw new FluentDataException("A query has already been executed on this command object. Please create a new command object.");
            }
            else
            {
                _data.InnerCommand.CommandText = _data.Sql.ToString();

                if (_data.Context.Data.CommandTimeout != Int32.MinValue)
                    _data.InnerCommand.CommandTimeout = _data.Context.Data.CommandTimeout;

                if (_data.InnerCommand.Connection.State != ConnectionState.Open)
                    OpenConnection();

                if (_data.Context.Data.UseTransaction)
                {
                    if (_data.Context.Data.Transaction == null)
                        _data.Context.Data.Transaction = _data.Context.Data.Connection.BeginTransaction((System.Data.IsolationLevel)_data.Context.Data.IsolationLevel);

                    _data.InnerCommand.Transaction = _data.Context.Data.Transaction;
                }

                if (_data.Context.Data.OnExecuting != null)
                    _data.Context.Data.OnExecuting(new CommandEventArgs(_data.InnerCommand));

                _queryAlreadyExecuted = true;
            }
        }

        private void OpenConnection()
        {
            if (_data.Context.Data.OnConnectionOpening != null)
                _data.Context.Data.OnConnectionOpening(new ConnectionEventArgs(_data.InnerCommand.Connection));

            _data.InnerCommand.Connection.Open();

            if (_data.Context.Data.OnConnectionOpened != null)
                _data.Context.Data.OnConnectionOpened(new ConnectionEventArgs(_data.InnerCommand.Connection));
        }


    }

}
