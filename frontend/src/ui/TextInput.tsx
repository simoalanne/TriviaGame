import { TextInput as MantineTextInput } from '@mantine/core';

type TextInputProps = {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  disabled?: boolean;
};

export function TextInput({ value, onChange, placeholder, disabled }: TextInputProps) {
  return (
    <MantineTextInput
      value={value}
      onChange={(e) => onChange(e.currentTarget.value)}
      placeholder={placeholder}
      disabled={disabled}
      radius="md"
      size="md"
    />
  );
}
