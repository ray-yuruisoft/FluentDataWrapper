using FluentData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FluentDataWrapper
{
    public abstract class FluentDataBase<T>
    {

        #region private

        private static IDbContext _dbContext = null;
        private object obj = new object();
        private IDbContext _Context()
        {
            lock (obj)
            {
                if (_dbContext == null)
                {
                    _dbContext = new DbContext()
                    .ConnectionStringName(connectionString, new MySqlProvider())
                    .CommandTimeout(1000)
                    .OnConnectionOpening(OnConnectionOpening)
                    .OnConnectionClosed(OnConnectionClosed)
                    .OnConnectionOpened(OnConnectionOpened)
                    .OnExecuting(OnExecuting)
                    .OnError(OnError)
                    .OnExecuted(OnExecuted);
                }
                return _dbContext;
            }
        }
        private static Stopwatch stopwatch = new Stopwatch();
        private string _tableName { get { return tableName?.ToLower(); } }

        #region Event
        //开始连接
        private static void OnConnectionOpening(ConnectionEventArgs e)
        {
            stopwatch.Start();
            //Console.WriteLine("OnConnectionOpening -- stopwatch start.");
        }
        //连接成功
        private static void OnConnectionOpened(ConnectionEventArgs e)
        {
            stopwatch.Stop();
            TimeSpan tsConnection = stopwatch.Elapsed;
            //Console.WriteLine("OnConnectionOpened -- stopwatch stop. -- time last " + tsConnection.TotalSeconds);
        }
        //sql开始执行
        private static void OnExecuting(CommandEventArgs e)
        {
            stopwatch.Start();
            //Console.WriteLine("OnExecuting -- stopwatch start.");
        }
        //sql执行完成
        private static void OnExecuted(CommandEventArgs e)
        {
            stopwatch.Stop();
            TimeSpan tsCommand = stopwatch.Elapsed;
            //Console.WriteLine("OnExecuted -- stopwatch stop. -- time last " + tsCommand.TotalSeconds);
        }
        //连接结束
        private static void OnConnectionClosed(ConnectionEventArgs e)
        {
            //Console.WriteLine("OnConnectionClosed");
        }
        //异常
        private static void OnError(ErrorEventArgs e)
        {
            //Console.WriteLine("OnError");
        }
        #endregion

        #endregion

        public abstract string connectionString { get; }
        public abstract string tableName { get; }
        public FluentDataBase()
        {

        }

        #region Exists
        public bool Exists(params object[] parameters)
        {
            using (var context = _Context())
            {
                StringBuilder strSql = new StringBuilder();
                int count = 0;
                foreach (var item in typeof(T).GetProperties())
                {
                    if (item.CustomAttributes.FirstOrDefault(c => c.AttributeType.Name == "PrimaryKey") != null)
                    {
                        if (parameters.Length - 1 < count) break;
                        if (parameters[count].GetType().Name == "String")
                        {
                            strSql.Append("and " + item.Name + "='" + parameters[count] + "' ");
                        }
                        else
                        {
                            strSql.Append("and " + item.Name + "=" + parameters[count].ToString() + " ");
                        }
                        count++;
                    }
                }
                if (strSql.Length != 0)
                {
                    strSql.Insert(0, "select count(1) from " + tableName + " where 1=1 ");
                    return _Context().Sql(strSql.ToString()).QuerySingle<int>() == 0 ? false : true;
                }
            }
            return false;
        }

        #endregion

        #region Select

        private string SelectstringBuiler(string sqlStr)
        {
            var sqlstr = sqlStr?.ToLower();
            if (sqlstr == null)
            {
                StringBuilder stringBuilder = new StringBuilder();

                foreach (var item in typeof(T).GetProperties())
                {
                    stringBuilder.Append(item.Name + ",");
                }
                sqlstr = string.Format("select {0} from {1}", stringBuilder.Remove(stringBuilder.Length - 1, 1).ToString(), tableName);
            }
            else
            {
                if (sqlstr.IndexOf(_tableName) == -1)
                {
                    if (sqlstr.IndexOf("select") == -1)
                    {
                        sqlstr = string.Format("select {0} from {1}", sqlstr, tableName);
                    }
                    else
                    {
                        sqlstr = string.Format("{0} from {1}", sqlstr, tableName);
                    }
                }
                else
                {
                    sqlstr = sqlStr;
                }
            }
            return sqlstr;
        }
        public List<T> SelectList(string sqlstr = null, params object[] parameters)
        {
            using (var context = _Context())
            {
                return context.Sql(SelectstringBuiler(sqlstr), parameters)
                      .QueryMany<T>();
            }
        }
        public T SelectSingle(string sqlstr = null, params object[] parameters)
        {
            using (var context = _Context())
            {
                return context.Sql(SelectstringBuiler(sqlstr), parameters)
                      .QuerySingle<T>();
            }
        }

        #endregion

        #region Insert
        public dynamic Insert(T t)
        {
            using (var context = _Context())
            {
                return context.Insert<T>(tableName, t).AutoMap().ExecuteReturnLastId<dynamic>();
            }
        }
        public dynamic InsertSome(T t, Action<IInsertUpdateBuilder<T>> fillMethod)
        {
            using (var context = _Context())
            {
                return context.Insert<T>(tableName, t).Fill(fillMethod).ExecuteReturnLastId<dynamic>();
            }
        }
        #endregion

        #region Update
        public bool Update(T t)
        {
            using (var context = _Context())
            {
                IUpdateBuilder<T> temp = context.Update<T>(tableName, t);
                foreach (var item in typeof(T).GetProperties())
                {
                    if (item.CustomAttributes.FirstOrDefault(c => c.AttributeType.Name == "PrimaryKey") != null)
                    {
                        temp.Where(item.Name, item.GetValue(t));
                    }
                    else
                    {
                        temp = temp.Column(item.Name, item.GetValue(t));
                    }
                }
                return temp.Execute() > 0;
            }
        }
        #endregion

        #region Context

        private static IDbContext dbContext = null;
        public IDbContext Context()
        {
            using (var context = _Context())
            {
                lock (obj)
                {
                    if (dbContext == null)
                    {
                        dbContext = new DbContext()
                                    .ConnectionStringName(connectionString, new MySqlProvider())
                                    .CommandTimeout(1000)
                                    .OnConnectionOpening(OnConnectionOpening)
                                    .OnConnectionClosed(OnConnectionClosed)
                                    .OnConnectionOpened(OnConnectionOpened)
                                    .OnExecuting(OnExecuting)
                                    .OnError(OnError)
                                    .OnExecuted(OnExecuted);
                    }
                    return dbContext;
                }
            }
        }

        #endregion

        #region Dispose
        public void Dispose()
        {
            dbContext.Dispose();
            dbContext = null;
        }
        #endregion

    }

    public class PrimaryKey : Attribute
    {
    }

}
