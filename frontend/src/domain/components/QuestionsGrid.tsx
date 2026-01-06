import { useState } from "react";
import { QuestionCard } from "./QuestionCard";
import type { TriviaItemForClient } from "../../hooks/useGameHub";

type QuestionsGridProps = {
  triviaItem: TriviaItemForClient;
  allowedToAnswer: boolean;
  onAnswerQuestion: (questionIndex: number, answer: string) => void;
};

export function QuestionsGrid({
  triviaItem,
  allowedToAnswer,
  onAnswerQuestion,
}: QuestionsGridProps) {
  let options: string[] | undefined = undefined;

  switch (triviaItem.questionType) {
    case "MultipleChoice":
      options = triviaItem.content.correctAnswers!;
      break;
    case "TrueOrFalse":
      options = ["True", "False"];
      break;
    case "Ordering":
      options = triviaItem.content.questions
        .map((q, index) =>
          q.playerAnswer ? undefined : (index + 1).toString()
        )
        .filter((num): num is string => num !== undefined);
      break;
  }

  const [activeIndex, setActiveIndex] = useState<number | null>(null);

  return (
    <div
      style={{
        display: "grid",
        width: "100%",
        gridTemplateColumns: "repeat(auto-fill, minmax(220px, 1fr))",
        gap: 12,
        justifyItems: "flex-start",
      }}
    >
      {triviaItem.content.questions.map((q, index) => (
        <QuestionCard
          key={index}
          question={q}
          type={triviaItem.questionType}
          options={options}
          allowedToAnswer={allowedToAnswer && !q.playerAnswer}
          isActive={activeIndex === index || activeIndex === null}
          onActivate={() => setActiveIndex(index)}
          onInputClear={() => setActiveIndex(null)}
          onSubmit={(answer) => {
            onAnswerQuestion(index, answer);
            setActiveIndex(null);
          }}
        />
      ))}
    </div>
  );
}
