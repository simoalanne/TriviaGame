import { Select as MantineSelect } from '@mantine/core';

type SelectOption<T extends string | number> = {
  label: string;
  value: T;
};

type SelectProps<T extends string | number> = {
  value: T | null;
  onChange: (value: T | null) => void;
  options: SelectOption<T>[];
  disabled?: boolean;
};

export function Select<T extends string | number>({
  value,
  onChange,
  options,
  disabled,
}: SelectProps<T>) {
  return (
    <MantineSelect
      value={value !== null ? String(value) : null} // convert value to string
      onChange={(val) => {
        if (val === null) return;
        // convert back to original type
        const original = options.find((o) => String(o.value) === val)?.value;
        if (original !== undefined) onChange(original);
      }}
      data={options.map((o) => ({ label: o.label, value: String(o.value) }))} // convert to string
      disabled={disabled}
      radius="md"
      size="md"
      clearable
      onClear={() => onChange(null)}
    />
  );
}
