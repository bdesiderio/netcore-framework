using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Data.MongoDB
{
    public abstract class DataSource
    {
        public DataSource(string connectionStringName)
        {
            ConnectionStringName = connectionStringName;
        }

        public string ConnectionStringName { get; }
    }

    public abstract class DataSource<T> : DataSource, IEquatable<T> where T : DataSource<T>
    {
        public DataSource(string connectionStringName) : base(connectionStringName) { }

        public bool Equals(T other)
        {
            if (other == null) return false;
            return string.Equals(ConnectionStringName, other.ConnectionStringName, StringComparison.Ordinal);
        }

        public override bool Equals(object obj) => Equals(obj as T);

        public override int GetHashCode() => ConnectionStringName.GetHashCode();
    }
}
