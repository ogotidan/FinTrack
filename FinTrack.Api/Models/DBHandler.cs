using System.Data;
using MySqlConnector;

namespace FinTrack.Api.Models;

public class DBHandler
{
    private readonly string _connStr;
    public DBHandler(string connStr) => _connStr = connStr;

    private MySqlConnection Open()
    {
        var conn = new MySqlConnection(_connStr);
        conn.Open();
        return conn;
    }

    private static MySqlParameter OutId() =>
        new("p_out_id", MySqlDbType.Int64) { Direction = ParameterDirection.Output };

    public long RegisterAnalyst(string name, string email, string passwordHash)
    {
        using var conn = Open();
        using var cmd = new MySqlCommand("sp_register_analyst", conn) { CommandType = CommandType.StoredProcedure };
        var outId = OutId();
        cmd.Parameters.Add(outId);
        cmd.Parameters.AddWithValue("p_name", name);
        cmd.Parameters.AddWithValue("p_email", email);
        cmd.Parameters.AddWithValue("p_password_hash", passwordHash);
        cmd.ExecuteNonQuery();
        return outId.Value is null or DBNull ? 0 : Convert.ToInt64(outId.Value);
    }

    public Analyst? ValidateLogin(string email)
    {
        using var conn = Open();
        using var cmd = new MySqlCommand("sp_validate_login", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("p_email", email);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new Analyst
        {
            Id = Convert.ToInt64(r["id"]),
            Name = r["name"].ToString() ?? string.Empty,
            Email = r["email"].ToString() ?? string.Empty,
            PasswordHash = r["password_hash"].ToString() ?? string.Empty,
            IsActive = Convert.ToBoolean(r["is_active"])
        };
    }

    public void UpdateLastLogin(long analystId)
    {
        using var conn = Open();
        using var cmd = new MySqlCommand("sp_update_analyst_last_login", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("p_analyst_id", analystId);
        cmd.ExecuteNonQuery();
    }

    public long AddPortfolio(long analystId, string name, string clientName, string baseCurrency)
    {
        using var conn = Open();
        using var cmd = new MySqlCommand("sp_add_portfolio", conn) { CommandType = CommandType.StoredProcedure };
        var outId = OutId();
        cmd.Parameters.Add(outId);
        cmd.Parameters.AddWithValue("p_analyst_id", analystId);
        cmd.Parameters.AddWithValue("p_name", name);
        cmd.Parameters.AddWithValue("p_client_name", clientName);
        cmd.Parameters.AddWithValue("p_base_currency", baseCurrency);
        cmd.ExecuteNonQuery();
        return Convert.ToInt64(outId.Value);
    }

    public List<Portfolio> GetPortfolios(long analystId)
    {
        using var conn = Open();
        using var cmd = new MySqlCommand("sp_get_portfolios", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("p_analyst_id", analystId);
        using var r = cmd.ExecuteReader();
        var list = new List<Portfolio>();
        while (r.Read()) list.Add(MapPortfolio(r, hasCount: true));
        return list;
    }

    public Portfolio? GetPortfolioById(long id, long analystId)
    {
        using var conn = Open();
        using var cmd = new MySqlCommand("sp_get_portfolio_by_id", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("p_id", id);
        cmd.Parameters.AddWithValue("p_analyst_id", analystId);
        using var r = cmd.ExecuteReader();
        return r.Read() ? MapPortfolio(r, hasCount: false) : null;
    }

    public long UpdatePortfolio(long id, long analystId, string name, string clientName, string baseCurrency)
    {
        using var conn = Open();
        using var cmd = new MySqlCommand("sp_update_portfolio", conn) { CommandType = CommandType.StoredProcedure };
        var outRows = OutId();
        cmd.Parameters.Add(outRows);
        cmd.Parameters.AddWithValue("p_id", id);
        cmd.Parameters.AddWithValue("p_analyst_id", analystId);
        cmd.Parameters.AddWithValue("p_name", name);
        cmd.Parameters.AddWithValue("p_client_name", clientName);
        cmd.Parameters.AddWithValue("p_base_currency", baseCurrency);
        cmd.ExecuteNonQuery();
        return Convert.ToInt64(outRows.Value);
    }

    public long DeletePortfolio(long id, long analystId)
    {
        using var conn = Open();
        using var cmd = new MySqlCommand("sp_delete_portfolio", conn) { CommandType = CommandType.StoredProcedure };
        var outRows = OutId();
        cmd.Parameters.Add(outRows);
        cmd.Parameters.AddWithValue("p_id", id);
        cmd.Parameters.AddWithValue("p_analyst_id", analystId);
        cmd.ExecuteNonQuery();
        return Convert.ToInt64(outRows.Value);
    }

    private static Portfolio MapPortfolio(MySqlDataReader r, bool hasCount) => new()
    {
        Id = Convert.ToInt64(r["id"]),
        Name = r["name"].ToString() ?? string.Empty,
        ClientName = r["client_name"].ToString() ?? string.Empty,
        BaseCurrency = r["base_currency"].ToString() ?? "KES",
        CreatedAt = Convert.ToDateTime(r["created_at"]),
        UpdatedAt = Convert.ToDateTime(r["updated_at"]),
        TransactionCount = hasCount ? Convert.ToInt32(r["transaction_count"]) : 0
    };

    public long AddTransaction(long portfolioId, string instrument, string type,
                               decimal quantity, decimal unitPrice, DateTime tradeDate)
    {
        using var conn = Open();
        using var cmd = new MySqlCommand("sp_add_transaction", conn) { CommandType = CommandType.StoredProcedure };
        var outId = OutId();
        cmd.Parameters.Add(outId);
        cmd.Parameters.AddWithValue("p_portfolio_id", portfolioId);
        cmd.Parameters.AddWithValue("p_instrument", instrument);
        cmd.Parameters.AddWithValue("p_type", type);
        cmd.Parameters.AddWithValue("p_quantity", quantity);
        cmd.Parameters.AddWithValue("p_unit_price", unitPrice);
        cmd.Parameters.AddWithValue("p_trade_date", tradeDate.Date);
        cmd.ExecuteNonQuery();
        return Convert.ToInt64(outId.Value);
    }

    public List<Transaction> GetTransactions(long portfolioId)
    {
        using var conn = Open();
        using var cmd = new MySqlCommand("sp_get_transactions", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("p_portfolio_id", portfolioId);
        using var r = cmd.ExecuteReader();
        var list = new List<Transaction>();
        while (r.Read())
        {
            list.Add(new Transaction
            {
                Id = Convert.ToInt64(r["id"]),
                PortfolioId = Convert.ToInt64(r["portfolio_id"]),
                Instrument = r["instrument"].ToString() ?? string.Empty,
                Type = r["type"].ToString() ?? string.Empty,
                Quantity = Convert.ToDecimal(r["quantity"]),
                UnitPrice = Convert.ToDecimal(r["unit_price"]),
                TotalAmount = Convert.ToDecimal(r["total_amount"]),
                TradeDate = Convert.ToDateTime(r["trade_date"]),
                CreatedAt = Convert.ToDateTime(r["created_at"])
            });
        }
        return list;
    }

    public long DeleteTransaction(long id, long analystId)
    {
        using var conn = Open();
        using var cmd = new MySqlCommand("sp_delete_transaction", conn) { CommandType = CommandType.StoredProcedure };
        var outRows = OutId();
        cmd.Parameters.Add(outRows);
        cmd.Parameters.AddWithValue("p_id", id);
        cmd.Parameters.AddWithValue("p_analyst_id", analystId);
        cmd.ExecuteNonQuery();
        return Convert.ToInt64(outRows.Value);
    }

    public List<HoldingSummary> GetPortfolioSummary(long portfolioId)
    {
        using var conn = Open();
        using var cmd = new MySqlCommand("sp_get_portfolio_summary", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("p_portfolio_id", portfolioId);
        using var r = cmd.ExecuteReader();
        var list = new List<HoldingSummary>();
        while (r.Read())
        {
            list.Add(new HoldingSummary
            {
                Instrument = r["instrument"].ToString() ?? string.Empty,
                NetQuantity = Convert.ToDecimal(r["net_quantity"]),
                AverageCost = Convert.ToDecimal(r["average_cost"]),
                TotalInvested = Convert.ToDecimal(r["total_invested"]),
                LatestPrice = Convert.ToDecimal(r["latest_price"]),
                TotalValue = Convert.ToDecimal(r["total_value"])
            });
        }
        return list;
    }
}
