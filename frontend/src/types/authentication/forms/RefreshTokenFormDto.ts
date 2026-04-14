import { z } from 'zod'

export const RefreshTokenFormSchema = z.object({
  refreshToken: z.string().min(1),
})

export type RefreshTokenFormDto = z.infer<typeof RefreshTokenFormSchema>
