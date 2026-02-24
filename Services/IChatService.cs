namespace AACS.Risk.Web.Services;

using AACS.Risk.Web.Models;

public interface IChatService
{
    Task<ChatSessionResponse> SendSessionMessageAsync(ChatSessionRequest request);
    Task SubmitFeedbackAsync(ChatFeedbackRequest request);
}
