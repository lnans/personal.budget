import { z } from 'zod'

import { AccountsErrors } from '@/types/accounts/accountsErrors'

const MAX_NAME_LENGTH = 20
const MAX_BANK_LENGTH = 20

export const PatchAccountFormSchema = z
  .object({
    name: z
      .string()
      .min(1, { message: AccountsErrors.NameRequired })
      .max(MAX_NAME_LENGTH, { message: AccountsErrors.NameTooLong }),
    bank: z
      .string()
      .min(1, { message: AccountsErrors.BankRequired })
      .max(MAX_BANK_LENGTH, { message: AccountsErrors.BankTooLong }),
    updateInitialBalance: z.boolean(),
    initialBalance: z.number().optional(),
  })
  .refine((data) => !data.updateInitialBalance || data.initialBalance !== undefined, {
    error: AccountsErrors.InitialBalanceRequired,
    path: ['initialBalance'],
  })

export type PatchAccountSchemaDto = z.infer<typeof PatchAccountFormSchema>

export type PatchAccountFormDto = {
  name: string
  bank: string
  initialBalance?: number
}

export const toPatchAccountRequest = (form: PatchAccountSchemaDto): PatchAccountFormDto => {
  return {
    name: form.name,
    bank: form.bank,
    ...(form.updateInitialBalance && form.initialBalance !== undefined ? { initialBalance: form.initialBalance } : {}),
  }
}
