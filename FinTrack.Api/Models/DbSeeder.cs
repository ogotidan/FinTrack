namespace FinTrack.Api.Models;

public static class DbSeeder
{
    public const string DemoEmail = "analyst@fintrack.co.ke";
    public const string DemoPassword = "Demo@12345";

    public static void Seed(DBHandler db, ILogger logger)
    {
        try
        {
            if (db.ValidateLogin(DemoEmail) is not null)
            {
                logger.LogInformation("Seed: demo analyst already present - skipping.");
                return;
            }

            var hash = BCrypt.Net.BCrypt.HashPassword(DemoPassword, workFactor: 12);
            var analystId = db.RegisterAnalyst("Demo Analyst", DemoEmail, hash);
            if (analystId <= 0)
            {
                logger.LogWarning("Seed: could not create demo analyst.");
                return;
            }

            var p1 = db.AddPortfolio(analystId, "NSE Growth Portfolio", "Acme Capital Ltd", "KES");
            db.AddTransaction(p1, "SCOM", "BUY", 1000m, 15.50m, new DateTime(2026, 10, 1));
            db.AddTransaction(p1, "SCOM", "BUY", 500m, 16.20m, new DateTime(2026, 10, 15));
            db.AddTransaction(p1, "KCB", "BUY", 800m, 38.00m, new DateTime(2026, 10, 5));
            db.AddTransaction(p1, "EQTY", "BUY", 600m, 45.50m, new DateTime(2026, 10, 20));
            db.AddTransaction(p1, "SCOM", "SELL", 300m, 17.00m, new DateTime(2026, 11, 1));

            var p2 = db.AddPortfolio(analystId, "Income & Fixed Portfolio", "Mwangi Family Trust", "KES");
            db.AddTransaction(p2, "KCB", "BUY", 1200m, 37.50m, new DateTime(2026, 9, 28));
            db.AddTransaction(p2, "EQTY", "BUY", 1000m, 44.00m, new DateTime(2026, 10, 3));
            db.AddTransaction(p2, "T-BILL-91", "BUY", 50000m, 1.00m, new DateTime(2026, 10, 10));
            db.AddTransaction(p2, "KCB", "SELL", 200m, 39.00m, new DateTime(2026, 11, 5));

            var p3 = db.AddPortfolio(analystId, "Diaspora USD Fund", "Ochieng Holdings", "USD");
            db.AddTransaction(p3, "SCOM", "BUY", 2000m, 0.12m, new DateTime(2026, 10, 8));
            db.AddTransaction(p3, "EQTY", "BUY", 800m, 0.35m, new DateTime(2026, 10, 18));

            logger.LogInformation("Seed: demo data created (analyst_id={Id}).", analystId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Seed: failed to create demo data.");
        }
    }
}
