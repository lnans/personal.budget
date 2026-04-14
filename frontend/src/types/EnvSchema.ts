import { z } from 'zod'

export const envSchema = z.object({
  VITE_API_URL: z.url({ protocol: /^https?$/, message: 'VITE_API_URL must be a valid URL' }),
  VITE_LIST_PAGE_SIZE: z.coerce.number().int().positive({ message: 'VITE_LIST_PAGE_SIZE must be a positive integer' }),
})

export type EnvSchemaType = Readonly<z.infer<typeof envSchema>>
