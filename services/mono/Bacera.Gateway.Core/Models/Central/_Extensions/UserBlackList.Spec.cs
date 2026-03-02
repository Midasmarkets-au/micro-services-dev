namespace Bacera.Gateway;

using M = Bacera.Gateway.UserBlackList;

public partial class UserBlackList
{
    public sealed class CreateSpec
    {
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string IdNumber { get; set; } = "";

        public M ToEntity(string operatorName)
            => new()
            {
                Name = Name,
                Phone = Phone,
                Email = Email,
                IdNumber = IdNumber,
                OperatorName = operatorName,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            };
    }

    public sealed class UpdateSpec
    {
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string IdNumber { get; set; } = "";

        public M ApplyTo(M item, string operatorName)
        {
            item.Name = Name;
            item.Phone = Phone;
            item.Email = Email;
            item.IdNumber = IdNumber;
            item.OperatorName = operatorName;
            item.UpdatedOn = DateTime.UtcNow;
            return item;
        }
    }
}