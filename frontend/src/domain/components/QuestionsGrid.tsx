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
      const answered = new Set(
        triviaItem.content.questions.map((q) => q.correctAnswer).filter(Boolean)
      );
      // Options consist of range from 1 to number of questions, excluding already answered ones
      options = Array.from(
        { length: triviaItem.content.questions.length },
        (_, i) => `${i + 1}`
      ).filter((o) => !answered.has(o));
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
