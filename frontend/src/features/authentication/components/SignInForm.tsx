import { zodResolver } from '@hookform/resolvers/zod'
import React from 'react'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'

import { useSignIn } from '@/api/endpoints/AuthenticationEndpoints'
import { InputControlled } from '@/components/forms/InputControlled'
import { Button } from '@/components/ui/Button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/Card'
import { FieldGroup } from '@/components/ui/Field'
import { cn } from '@/lib/utils'
import type { SignInFormDto } from '@/types/authentication/forms/SignInFormDto'
import { SignInFormSchema } from '@/types/authentication/forms/SignInFormDto'

export function SignInForm({ className, ...props }: React.ComponentProps<'div'>) {
  const { t } = useTranslation()

  const signInMutation = useSignIn()

  const form = useForm<SignInFormDto>({
    resolver: zodResolver(SignInFormSchema),
    defaultValues: {
      login: '',
      password: '',
    },
  })

  const isSubmitDisabled = signInMutation.isSuccess || signInMutation.isPending

  const handleSubmit = (event: React.SubmitEvent<HTMLFormElement>) => {
    form.handleSubmit((data: SignInFormDto) => {
      signInMutation.mutate(data)
    })(event)
  }

  return (
    <div className={cn('flex flex-col gap-6', className)} {...props}>
      <Card>
        <CardHeader className="text-center">
          <CardTitle className="text-xl">{t('auth.signIn.title')}</CardTitle>
          <CardDescription>{t('auth.signIn.description')}</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit}>
            <FieldGroup>
              <InputControlled autoFocus control={form.control} label={t('auth.signIn.login.label')} name="login" />
              <InputControlled control={form.control} label={t('auth.signIn.password.label')} name="password" type="password" />
              <Button disabled={isSubmitDisabled} loading={signInMutation.isPending} type="submit">
                {t('auth.signIn.submit')}
              </Button>
            </FieldGroup>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
