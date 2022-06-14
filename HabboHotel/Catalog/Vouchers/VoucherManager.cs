using System.Data;

namespace Plus.HabboHotel.Catalog.Vouchers;

public class VoucherManager : IVoucherManager
{
    private readonly Dictionary<string, Voucher> _vouchers;

    public VoucherManager()
    {
        _vouchers = new Dictionary<string, Voucher>();
    }

    public void Init()
    {
        if (_vouchers.Count > 0)
            _vouchers.Clear();
        DataTable data = null;
        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
        {
            dbClient.SetQuery("SELECT `voucher`,`type`,`value`,`current_uses`,`max_uses` FROM `catalog_vouchers` WHERE `enabled` = '1'");
            data = dbClient.GetTable();
        }
        if (data != null)
        {
            foreach (DataRow row in data.Rows)
            {
                _vouchers.Add(Convert.ToString(row["voucher"]),
                    new Voucher(Convert.ToString(row["voucher"]), Convert.ToString(row["type"]), Convert.ToInt32(row["value"]), Convert.ToInt32(row["current_uses"]),
                        Convert.ToInt32(row["max_uses"])));
            }
        }
    }

    public bool TryGetVoucher(string code, out Voucher voucher) => _vouchers.TryGetValue(code, out voucher);
}