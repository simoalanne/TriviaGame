using FluentValidation;

namespace Shared;

using TriviaItemDto = TriviaItem<QuestionDto>;

public enum QuestionType
{
    MultipleChoice,
    TrueOrFalse,
    FillInTheBlank,
    Ordering
}

public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard
}

public record TriviaItem<TQuestion>
{
    public required string Prompt { get; init; }
    public required List<string> Tags { get; init; }
    public required ItemContent<TQuestion> Content { get; init; }
    public required DifficultyLevel Difficulty { get; init; }
    public required QuestionType QuestionType { get; init; }
    public string? Id { get; init; }
}

public record ItemContent<TQuestion>
{
    public required List<TQuestion> Questions { get; init; }
    public List<string>? CorrectAnswers { get; init; }
}

public record QuestionDto
{
    public required string QuestionText { get; init; }
    public string? Explanation { get; init; }
    public int? OrderingAnswer { get; init; }
    public bool? TrueOrFalseAnswer { get; init; }
    public List<string>? FillInTheBlankAnswer { get; init; }
    public string? MultipleChoiceAnswer { get; init; }
};

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

    // Tags constraints
    private const int MinTagLength = 2;
    private const int MaxTagLength = 30;
    private const int MinTags = 1;
    private const int MaxTags = 4;

    // this could use reflection in the future.
    public static string GetMinAndMaxGuide()
    {
        return
            $"Trivia Question Validation Rules for lengths and counts:\n" +
            $"- Prompt: {MinPromptLength}-{MaxPromptLength} characters\n" +
            $"- Questions per Item: {MinQuestionsPerItem}-{MaxQuestionsPerItem}\n" +
            $"- Question Text: {MinQuestionLength}-{MaxQuestionLength} characters\n" +
            $"- Explanation (optional): {MinExplanationLength}-{MaxExplanationLength} characters\n" +
            $"- Tags per Item: {MinTags}-{MaxTags}, each tag {MinTagLength}-{MaxTagLength} characters\n" +
            $"- Textual Answers: {MinTextualAnswerLength}-{MaxTextualAnswerLength} characters\n" +
            $"- Multiple Choice Answers: {MinMultipleChoiceAnswers}-{MaxMultipleChoiceAnswers}, must be unique\n" +
            $"- Fill in the Blank Acceptable Answers: {MinAcceptableAnswers}-{MaxAcceptableAnswers}, must be unique\n" +
            $"- Ordering Answers: must form a continuous range from 1 to number of questions";
    }

    public TriviaQuestionValidator()
    {
        RuleFor(x => x.Prompt)
            .NotEmpty()
            .Length(MinPromptLength, MaxPromptLength);

        RuleFor(x => x.Tags)
            .NotNull()
            .NotEmpty()
            .Must(tags => tags?.Count is >= MinTags and <= MaxTags)
            .WithMessage($"Number of tags must be between {MinTags} and {MaxTags}.")
            .Must(IsUniqueList)
            .WithMessage("Tags must be unique.");
        RuleForEach(x => x.Tags)
            .NotEmpty()
            .Length(MinTagLength, MaxTagLength);


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
