import { envSchema, type EnvSchemaType } from './types/EnvSchema'

export const ENV: EnvSchemaType = envSchema.parse(import.meta.env)
