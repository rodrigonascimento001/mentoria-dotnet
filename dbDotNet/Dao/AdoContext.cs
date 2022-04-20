using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace dbDotNet.Dao
{
    class AdoContext : IDisposable
    {
        private readonly DbProviderFactory _factory;
        private readonly DbConnection _connect;
        private readonly DbTransaction _transaction;

        private AdoContext(DbProviderFactory factory, DbConnection connect, DbTransaction transaction)
        {
            _factory = factory;
            _connect = connect;
            _transaction = transaction;
        }

        public void Dispose()
        {
            if(_transaction != null)
            {
                _transaction.Dispose();
            }

            if(_connect != null)
            {
                _connect.Close();
                _connect.Dispose();
            }
        }

        public void Commit() => _transaction.Commit();

        public void RollBack() => _transaction.Rollback();

        public DbCommand CreateCommand(string sql, params DbParameter[] parameters)
        {
            var _result = _factory.CreateCommand();
            _result.CommandText = sql;

            foreach (var item in parameters)
            {
                _result.Parameters.Add(item);
            }
            if (_transaction != null)
                _result.Transaction = _transaction;

            return _result;
        }

        public DbParameter CreateParameter(string name, DbType type, object value)
        {
            var _result = _factory.CreateParameter();
            _result.ParameterName = name;
            _result.DbType = type;
            _result.Value = value != null ? value : DBNull.Value;
            return _result;
        }

        public class Builder
        {
            private readonly string _connectionString;
            private readonly DbProviderFactory _factory;
            private bool _useTransaction;
        
            public Builder(string connectionString, Func <DbProviderFactory> factorySelection)
            {
                _factory = factorySelection();
                _connectionString = connectionString;
            }        

            public Builder WithTransaction()
            {
                _useTransaction = true;
                return this;
            }

            public AdoContext Build()
            {
                var _connection = _factory.CreateConnection();
                _connection.ConnectionString = _connectionString;
                _connection.Open();
                var _transaction = _useTransaction ? _connection.BeginTransaction() : null;
                return new AdoContext(_factory, _connection, _transaction);

            }
        }
    }
}
