using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Request;

public class VerificationQuizDTO
{
    public long Id { get; set; }
    public string Language { get; set; } = null!;
    public string Question { get; set; } = null!;
    public string CorrectAnswer { get; set; } = null!;
    public AnswerDTO[] Answers { get; set; } = null!;

    public class AnswerDTO
    {
        public string Id { get; set; } = null!;
        public string Answer { get; set; } = null!;
    }
}


public static class VerificationQuizExtensions
{
    public static VerificationQuizDTO ToDTO(this TopicContent model)
    {
        var item = JsonConvert.DeserializeObject<VerificationQuizDTO>(model.Content);
        if (item == null) return new VerificationQuizDTO();
        item.Language = model.Language;
        item.Id = model.TopicId;
        return item;
    }
}