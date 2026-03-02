namespace Bacera.Gateway;

using M = EventShopClientPoint;

public partial class EventShopClientPoint
{
    public static M Build(long clientAccountId, long parentAccountId, AccountRoleTypes parentRole,
        short openAccount = 1, int volume = 0,
        long depositAmount = 0)
        => new()
        {
            ChildAccountId = clientAccountId,
            ParentAccountId = parentAccountId,
            ParentAccountRole = (short)parentRole,
            OpenAccount = openAccount,
            Volume = volume,
            DepositAmount = depositAmount,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow
        };

    public int GetVolumeInUnit() => Volume / 100;
    public int GetOpenAccountUnit() => OpenAccount;
    public int AddVolume(double vol) => Volume += (int)(vol * 100);
    public int AddUnitToVolume(int unit) => Volume += unit * 100;
    public long AddUnitToDepositAmount(int unit) => DepositAmount += unit * 100000;
    public int GetDepositAmountInUnit() => (int)(DepositAmount / 100000);
}