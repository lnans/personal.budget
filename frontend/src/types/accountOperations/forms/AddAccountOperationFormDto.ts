import { z } from 'zod'

const MAX_DESCRIPTION_LENGTH = 100

export const AddAccountOperationFormSchema = z.object({
  description: z.string().min(1).max(MAX_DESCRIPTION_LENGTH),
  amount: z.number(),
})

export type AddAccountOperationFormDto = z.infer<typeof AddAccountOperationFormSchema>
