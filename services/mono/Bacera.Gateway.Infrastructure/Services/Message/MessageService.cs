using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Services.Message;

public class MessageService
{
    private readonly TenantDbContext _tenantDbContext;
    private readonly IEmailSender _emailSender;

    public MessageService(TenantDbContext tenantDbContext, IEmailSender emailSender)
    {
        _tenantDbContext = tenantDbContext;
        _emailSender = emailSender;
    }

    public async Task<MessageRecord> AddSendEmailAsync(string email, string subject, string message, long partyId = 0,
        long eventId = 0)
    {
        var item = MessageRecord.Email(email, subject, message, partyId, eventId);
        _tenantDbContext.MessageRecords.Add(item);
        await _tenantDbContext.SaveChangesAsync();
        return item;
    }

    public async Task<(bool, string)> SendEmailByIdAsync(long messageRecordId,
        MessageRecord.EmailSenderOptions senderOptions)
    {
        var item = await _tenantDbContext.MessageRecords
            .SingleOrDefaultAsync(x => x.Id == messageRecordId && x.Method == "email");

        if (item == null) return (false, "Message record not found");
        return await SendEmailByRecordAsync(item, senderOptions);
    }

    public async Task<(bool, string)> SendEmailByRecordAsync(MessageRecord record,
        MessageRecord.EmailSenderOptions senderOptions)
    {
        if (record.Method != "email") return (false, "Message record is not an email");
        if (record.Content == null) return (false, "Message content is empty");

        var email = record.Receiver;
        if (!EmailViewModel.IsValidReceiverEmail(email)) return (false, "Invalid email address");
        try
        {
            var (result, message) = await _emailSender.SendEmailAsync(email, record.Event, record.Content,
                senderOptions.SenderEmailAddress, senderOptions.SenderDisplayName, senderOptions.Bcc);

            if (!result)
            {
                record.Status = (short)MessageRecord.StatusTypes.Failed;
                record.Note = message;
            }

            record.Status = (short)MessageRecord.StatusTypes.Sent;
            await _tenantDbContext.SaveChangesAsync();
            return (true, message);
        }
        catch (Exception e)
        {
            record.Status = (short)MessageRecord.StatusTypes.Failed;
            record.Note = e.Message;
            await _tenantDbContext.SaveChangesAsync();
            return (false, e.Message);
        }
    }
}