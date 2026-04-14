import { z } from 'zod'

const MAX_DESCRIPTION_LENGTH = 100

export const RenameAccountOperationFormSchema = z.object({
  description: z.string().min(1).max(MAX_DESCRIPTION_LENGTH),
})

export type RenameAccountOperationFormDto = z.infer<typeof RenameAccountOperationFormSchema>
