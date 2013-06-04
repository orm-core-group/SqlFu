using System;
using System.Data;
using System.Linq;
using SqlFu.DDL;
using SqlFu.DDL.Generators.Postgresql;

namespace SqlFu.Providers
{
    public class PostgresProvider : AbstractProvider
    {
        public const string ProviderName = "Npgsql";

        public PostgresProvider(string provider = "Npgsql")
            : base(provider ?? "Npgsql")
        {
        }

        public override string FormatSql(string sql, params string[] paramNames)
        {
            return sql;
        }

        public override DbEngine ProviderType
        {
            get { return DbEngine.PostgreSQL; }
        }

        /// <summary>
        /// Gets expression query builder helpers for the rdbms.
        /// Internal usage
        /// </summary>
        public override IDbProviderExpressionHelper BuilderHelper
        {
            get { return  new PostgresBuilderHelper();}
        }

        protected override IDatabaseTools InitTools(DbAccess db)
        {
            return new PostgresDatabaseTools(db);
        }

        public override void MakePaged(string sql, out string selecSql, out string countSql)
        {
            int formidx;
            var body = GetPagingBody(sql, out formidx);
            countSql = "select count(*) " + body;
            selecSql = string.Format("{0} limit @{2} offset @{1}", sql, PagedSqlStatement.SkipParameterName,
                                     PagedSqlStatement.TakeParameterName);
        }

        public static string EscapeIdentifier(string s)
        {
            s.MustNotBeEmpty();
            if (s.Contains("\""))
            {
                return s;
            }
            if (!s.Contains(".")) return "\"" + s + "\"";
            return string.Join(".", s.Split('.').Select(d => "\"" + d + "\""));
        }

        public override void SetupParameter(IDbDataParameter param, string name, object value)
        {
            if (value != null)
            {
                //var tp = value.GetType();
                //if (tp.IsEnum)
                //{
                //    value = (int) value;
                //}
            }

            base.SetupParameter(param, name, value);
        }

        public override LastInsertId ExecuteInsert(SqlStatement sql, string idKey)
        {
            if (!string.IsNullOrEmpty(idKey))
            {
                sql.Sql += (" returning " + EscapeName(idKey));
            }
            using (sql)
            {
                return new LastInsertId(sql.ExecuteScalar());
            }
        }

        public override string ParamPrefix
        {
            get { return "@"; }
        }
    }
}