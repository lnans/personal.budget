import { z } from 'zod'

export const AccountTypeSchema = z.enum(['Checking', 'Savings'])

export type AccountType = z.infer<typeof AccountTypeSchema>
