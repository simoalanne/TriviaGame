import { useState } from "react";
import { Button } from "../../ui/Button";
import { TextInput } from "../../ui/TextInput";
import { Select } from "../../ui/Select";
import type { QuestionToClient, QuestionType } from "../../hooks/useGameHub";

interface QuestionCardProps {
  question: QuestionToClient;
  type: QuestionType;
  options: string[] | undefined;
  allowedToAnswer: boolean;
  isActive: boolean;
  onActivate: () => void;
  onInputClear: () => void;
  onSubmit: (answer: string) => void;
}

export function QuestionCard({
  question,
  type,
  options,
  allowedToAnswer,
  isActive,
  onActivate,
  onInputClear,
  onSubmit,
}: QuestionCardProps) {
  const [localAnswer, setLocalAnswer] = useState("");

  const answered = !!question.playerAnswer;
  const useTextField = type === "FillInTheBlank";

  const handleSubmit = () => {
    if (localAnswer.trim() === "") return;
    onSubmit(localAnswer.trim());
    setLocalAnswer("");
  };

  const inputDisabled = !allowedToAnswer || !isActive;

  return (
    <div
      style={{
        border: "1px solid #ccc",
        borderRadius: 8,
        padding: 12,
        minWidth: 200,
        maxWidth: 250,
        display: "flex",
        flexDirection: "column",
        gap: 8,
        background: answered ? "#f0f0f0" : "#fff",
      }}
    >
      <div style={{ fontWeight: 600 }}>{question.questionText}</div>

      {answered ? (
        <div>{question.correctAnswer}</div>
      ) : useTextField ? (
        <TextInput
          value={localAnswer}
          onChange={(answer) => {
            setLocalAnswer(answer);
            if (answer) return onActivate();
            onInputClear();
          }}
          placeholder="Your answer"
          disabled={inputDisabled}
        />
      ) : (
        <Select
          value={localAnswer}
          onChange={(answer) => {
            setLocalAnswer(answer || "");
            if (answer) return onActivate();
            onInputClear();
          }}
          options={options!.map((opt) => ({ label: opt, value: opt }))}
          disabled={inputDisabled}
        />
      )}

      {!answered && (
        <Button
          onClick={handleSubmit}
          disabled={localAnswer.trim() === "" || !isActive}
          label="Submit Answer"
        />
      )}
    </div>
  );
}
