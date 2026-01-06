import { Modal as MantineModal } from '@mantine/core';
import { type ReactNode } from 'react';

type ModalProps = {
  opened: boolean;
  onClose: () => void;
  children: ReactNode;
  title?: string;
};

export function Modal({ opened, onClose, children, title }: ModalProps) {
  return (
    <MantineModal opened={opened} onClose={onClose} title={title} radius="md">
      {children}
    </MantineModal>
  );
}
