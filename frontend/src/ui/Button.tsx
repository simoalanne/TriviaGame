import { Button as MantineButton } from '@mantine/core';

type ButtonProps = {
  label: string;
  onClick: () => void;
  disabled?: boolean;
};

export function Button({ label, onClick, disabled }: ButtonProps) {
  return (
    <MantineButton
      onClick={onClick}
      disabled={disabled}
      color="cyan"
      radius="md"
      size="md"
    >
      {label}
    </MantineButton>
  );
}
