import { Grid as MantineGrid } from '@mantine/core';
import { type ReactNode } from 'react';

type GridProps = {
  children: ReactNode;
  gutter?: number;
};

export function Grid({ children, gutter = 16 }: GridProps) {
  return <MantineGrid gutter={gutter}>{children}</MantineGrid>;
}
