using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway;

using M = Comment;

partial class Comment
{
    public sealed class TenantPageModel
    {
        public long Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public long PartyId { get; set; }
        public CommentTypes Type { get; set; }
        public long RowId { get; set; }

        public string OperatorName { get; set; } = string.Empty;
    }
}

public static class CommentViewModelExt
{
    public static IQueryable<M.TenantPageModel> ToTenantGeneralPageModel(this IQueryable<M> q) => q.Select(x => new M.TenantPageModel
    {
        Id = x.Id,
        Content = x.Content,
        CreatedOn = x.CreatedOn,
        PartyId = x.PartyId,
        Type = CommentTypes.User,
        RowId = x.PartyId,
        OperatorName = x.Party.NativeName
    });

    public static IQueryable<M.TenantPageModel> ToTenantUserPageModel(this IQueryable<PartyComment> q) => q.Select(x => new M.TenantPageModel
    {
        Id = x.Id,
        Content = x.Content,
        CreatedOn = x.CreatedOn,
        PartyId = x.OperatorPartyId,
        Type = CommentTypes.User,
        RowId = x.PartyId,
        OperatorName = x.OperatorParty.NativeName
    });

    public static IQueryable<M.TenantPageModel> ToTenantAccountPageModel(this IQueryable<AccountComment> q) => q.Select(x => new M.TenantPageModel
    {
        Id = x.Id,
        Content = x.Content,
        CreatedOn = x.CreatedOn,
        PartyId = x.OperatorPartyId,
        Type = CommentTypes.Account,
        RowId = x.AccountId,
        OperatorName = x.OperatorParty.NativeName
    });
}