import { z } from 'zod'

import { AuthenticationErrors } from '../authenticationErrors'

const MAX_LOGIN_LENGTH = 20

export const SignInFormSchema = z.object({
  login: z
    .string()
    .min(1, { message: AuthenticationErrors.LoginRequired })
    .max(MAX_LOGIN_LENGTH, { message: AuthenticationErrors.LoginTooLong }),
  password: z.string().min(1, { message: AuthenticationErrors.PasswordRequired }),
})

export type SignInFormDto = z.infer<typeof SignInFormSchema>
