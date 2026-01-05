using FluentValidation;

namespace TriviaGame;
using TriviaItemDto = TriviaItem<QuestionDto>;

public enum QuestionType
{
    MultipleChoice,
    TrueOrFalse,
    FillInTheBlank,
    Ordering
}

public record TriviaItem<TQuestion>(
    string? Id,
    string Prompt,
    ItemContent<TQuestion> Content,
    QuestionType QuestionType
);

public record ItemContent<TQuestion>(
    List<TQuestion> Questions,
    List<string>? CorrectAnswers = null
);

public record QuestionDto(
    string QuestionText,
    string? Explanation = null,
    int? OrderingAnswer = null,
    bool? TrueOrFalseAnswer = null,
    List<string>? FillInTheBlankAnswer = null,
    string? MultipleChoiceAnswer = null
);

public class TriviaQuestionValidator : AbstractValidator<TriviaItemDto>
{
    private static bool IsUniqueList<T>(List<T> list) => list.Distinct().Count() == list.Count;

    private const int MinPromptLength = 10;
    private const int MaxPromptLength = 150;
    private const int MinQuestionLength = 1;
    private const int MaxQuestionLength = 75;
    private const int MinExplanationLength = 10;
    private const int MaxExplanationLength = 30;
    private const int MinTextualAnswerLength = 1;
    private const int MaxTextualAnswerLength = 30;

    private const int MinQuestionsPerItem = 2;
    private const int MaxQuestionsPerItem = 10;
    private const int MinAcceptableAnswers = 1;
    private const int MaxAcceptableAnswers = 4;
    private const int MinMultipleChoiceAnswers = 2;
    private const int MaxMultipleChoiceAnswers = 4;

    public TriviaQuestionValidator()
    {
        RuleFor(x => x.Prompt)
            .NotEmpty()
            .Length(MinPromptLength, MaxPromptLength);

        RuleFor(x => x.Content.Questions)
            .NotEmpty()
            .Must(q => q.Count is >= MinQuestionsPerItem and <= MaxQuestionsPerItem)
            .WithMessage($"Number of questions must be between {MinQuestionsPerItem} and {MaxQuestionsPerItem}.");

        RuleForEach(x => x.Content.Questions).ChildRules(q =>
        {
            q.RuleFor(x => x.QuestionText)
                .NotEmpty()
                .Length(MinQuestionLength, MaxQuestionLength);

            q.RuleFor(x => x.Explanation)
                .Length(MinExplanationLength, MaxExplanationLength)
                .When(x => x.Explanation != null);
        });

        When(x => x.QuestionType == QuestionType.MultipleChoice, () =>
        {
            RuleFor(x => x.Content.CorrectAnswers)
                .NotNull()
                .Must(a => a!.Count is >= MinMultipleChoiceAnswers and <= MaxMultipleChoiceAnswers)
                .Must(a => IsUniqueList(a!));

            RuleForEach(x => x.Content.CorrectAnswers!)
                .Length(MinTextualAnswerLength, MaxTextualAnswerLength);

            RuleFor(x => x).Must(x =>
                    x.Content.Questions.All(q =>
                        x.Content.CorrectAnswers!.Contains(q.MultipleChoiceAnswer ?? string.Empty)))
                .WithMessage("Each multiple choice answer must be included in correct answers.");
        });

        When(x => x.QuestionType == QuestionType.TrueOrFalse,
            () =>
            {
                RuleForEach(x => x.Content.Questions)
                    .ChildRules(q => { q.RuleFor(x => x.TrueOrFalseAnswer).NotNull(); });
            });

        When(x => x.QuestionType == QuestionType.FillInTheBlank, () =>
        {
            RuleForEach(x => x.Content.Questions).ChildRules(q =>
            {
                q.RuleFor(x => x.FillInTheBlankAnswer)
                    .NotNull()
                    .Must(a => a!.Count >= MinAcceptableAnswers && a.Count <= MaxAcceptableAnswers)
                    .Must(a => IsUniqueList(a!));

                q.RuleForEach(x => x.FillInTheBlankAnswer!)
                    .Length(MinTextualAnswerLength, MaxTextualAnswerLength);
            });
        });

        When(x => x.QuestionType == QuestionType.Ordering, () =>
        {
            RuleFor(x => x).Must(x =>
                {
                    var answers = x.Content.Questions
                        .Select(q => q.OrderingAnswer ?? -1)
                        .ToList();

                    return IsUniqueList(answers)
                           && answers.Min() == 1
                           && answers.Max() == x.Content.Questions.Count;
                })
                .WithMessage(
                    "Ordering answers must form a continuous range from 1 to the number of questions");
        });
    }
}
