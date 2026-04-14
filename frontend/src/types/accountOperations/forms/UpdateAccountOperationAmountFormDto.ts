import { z } from 'zod'

export const UpdateAccountOperationAmountFormSchema = z.object({
  amount: z.number(),
})

export type UpdateAccountOperationAmountFormDto = z.infer<typeof UpdateAccountOperationAmountFormSchema>
