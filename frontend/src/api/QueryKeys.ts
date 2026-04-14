import type { GetAccountOperationsQueryDto } from '@/types/accountOperations/queries/GetAccountOperationsQueryDto'

export const queryKeys = {
  auth: {
    all: ['auth'] as const,
  },

  accounts: {
    all: ['accounts'] as const,
  },

  accountOperations: {
    all: ['accountOperations'] as const,
    list: (query: GetAccountOperationsQueryDto) => [...queryKeys.accountOperations.all, query] as const,
  },
} as const
