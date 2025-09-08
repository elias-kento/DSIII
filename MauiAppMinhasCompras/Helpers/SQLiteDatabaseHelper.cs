using MauiAppMinhasCompras.Models;
using SQLite;
using System.Linq;

namespace MauiAppMinhasCompras.Helpers
{
    public class SQLiteDatabaseHelper
    {
        readonly SQLiteAsyncConnection _conn;

        public SQLiteDatabaseHelper(string path)
        {
            _conn = new SQLiteAsyncConnection(path);
            _conn.CreateTableAsync<Produto>().Wait();
            //EnsureCategoriaColumnAsync().Wait();
        }

        // MIGRAÇÃO: adiciona coluna 'Categoria' se faltar
        public async Task EnsureCategoriaColumnAsync()
        {
            var cols = await _conn.QueryAsync<TableInfo>("PRAGMA table_info(Produto)");
            bool hasCategoria = cols.Any(c =>
                c.name.Equals("Categoria", StringComparison.OrdinalIgnoreCase));

            if (!hasCategoria)
                await _conn.ExecuteAsync("ALTER TABLE Produto ADD COLUMN Categoria TEXT DEFAULT 'Outros'");
        }


        // Mapeamento do PRAGMA table_info
        private class TableInfo
        {
            public int cid { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public int notnull { get; set; }
            public string dflt_value { get; set; }
            public int pk { get; set; }
        }

        public Task<int> Insert(Produto p)
        {
            return _conn.InsertAsync(p);
        }

        public Task<List<Produto>> Update(Produto p)
        {
            string sql = "UPDATE Produto SET Descricao=?, Quantidade=?, Preco=?, Categoria=? WHERE Id=?";
            return _conn.QueryAsync<Produto>(sql, p.Descricao, p.Quantidade, p.Preco, p.Categoria, p.Id);
        }


        public Task<int> Delete(int id)
        {
            return _conn.Table<Produto>().DeleteAsync(i => i.Id == id);
        }

        public Task<List<Produto>> GetAll()
        {
            return _conn.Table<Produto>().ToListAsync();
        }

        public Task<List<Produto>> Search(string q)
        {
            string sql = "SELECT * FROM Produto WHERE descricao LIKE '%" + q + "%'";

            return _conn.QueryAsync<Produto>(sql);
        }

        public Task<List<MauiAppMinhasCompras.Models.Produto>> Search(string q, string categoria)
        {
            if (string.IsNullOrWhiteSpace(q) && string.IsNullOrWhiteSpace(categoria))
                return GetAll();

            if (!string.IsNullOrWhiteSpace(q) && !string.IsNullOrWhiteSpace(categoria))
                return _conn.QueryAsync<MauiAppMinhasCompras.Models.Produto>(
                    "SELECT * FROM Produto WHERE Descricao LIKE ? AND Categoria = ?",
                    $"%{q}%", categoria);

            if (!string.IsNullOrWhiteSpace(q))
                return _conn.QueryAsync<MauiAppMinhasCompras.Models.Produto>(
                    "SELECT * FROM Produto WHERE Descricao LIKE ?",
                    $"%{q}%");

            // só categoria
            return _conn.QueryAsync<MauiAppMinhasCompras.Models.Produto>(
                "SELECT * FROM Produto WHERE Categoria = ?",
                categoria);
        }

        public class CategoriaTotal
        {
            public string Categoria { get; set; }
            public double Total { get; set; }
        }

        public Task<List<CategoriaTotal>> GetTotalsByCategory()
        {
            return _conn.QueryAsync<CategoriaTotal>(
                "SELECT IFNULL(Categoria,'Outros') AS Categoria, " +
                "SUM(Quantidade * Preco) AS Total " +
                "FROM Produto " +
                "GROUP BY IFNULL(Categoria,'Outros')");
        }
    }
}