using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Data.MongoDB
{
    public class MongoDataSource : DataSource<MongoDataSource>
    {
        public MongoDataSource(string connectionStringName) :
            base(connectionStringName)
        { }
    }
}
